// Copyright © https://myCSharp.de - all rights reserved

// Native AOT smoke test: exercises the public API of all packages in a trimmed, natively compiled app.
// Exit code 0 means success; every failed check is printed and makes the exit code non-zero.

using System.Diagnostics.Metrics;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser;
using MyCSharp.HttpUserAgentParser.AspNetCore;
using MyCSharp.HttpUserAgentParser.AspNetCore.DependencyInjection;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using MyCSharp.HttpUserAgentParser.MemoryCache.DependencyInjection;
using MyCSharp.HttpUserAgentParser.Providers;

const string Chrome = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/158.0.0.0 Safari/537.36";
const string Safari = "Mozilla/5.0 (iPhone; CPU iPhone OS 18_7 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/27.2 Mobile/15E148 Safari/604.1";
const string ClaudeBot = "Mozilla/5.0 AppleWebKit/537.36 (KHTML, like Gecko; compatible; ClaudeBot/1.0; +claudebot@anthropic.com)";

int failures = 0;

void Check(bool condition, string description)
{
    Console.WriteLine($"{(condition ? "PASS" : "FAIL")} {description}");
    if (!condition)
    {
        failures++;
    }
}

// Parser
HttpUserAgentInformation chrome = HttpUserAgentParser.Parse(Chrome);
Check(chrome is { Type: HttpUserAgentType.Browser, Name: "Chrome", Version: "158.0.0.0" }, "parse Chrome");
Check(chrome.Platform is { Name: "Windows 10", PlatformType: HttpUserAgentPlatformType.Windows }, "parse Windows platform");

HttpUserAgentInformation safari = HttpUserAgentParser.Parse(Safari);
Check(safari is { Name: "Safari", Version: "27.2", MobileDeviceType: "Apple iPhone" }, "parse Safari on iPhone");

HttpUserAgentInformation robot = HttpUserAgentParser.Parse(ClaudeBot);
Check(robot is { Type: HttpUserAgentType.Robot, Name: "ClaudeBot" }, "parse ClaudeBot");
Check(chrome.Equals(HttpUserAgentParser.Parse(Chrome)), "struct equality");

// Source-generated regular expressions
Regex? windows10 = chrome.Platform?.Regex;
Check(windows10 is not null && windows10.IsMatch(Chrome), "platform regex");
Check(HttpUserAgentStatics.Platforms.Count == 43 && HttpUserAgentStatics.Platforms.All(static p => p.Regex is not null), "platform regexes");
Regex chromeRegex = HttpUserAgentStatics.Browsers.First(static b => b.Key.ToString().StartsWith("Chrome", StringComparison.Ordinal)).Key;
Check(string.Equals(chromeRegex.Match(Chrome).Groups[1].Value, "158.0.0.0", StringComparison.Ordinal), "browser regex version group");

// Dependency injection, caching providers and ASP.NET Core accessor
using Meter meter = new("smoke.http_user_agent_parser");
ServiceCollection services = new();
services.AddHttpUserAgentMemoryCachedParser(static options => options.CacheOptions.SizeLimit = 16)
    .AddHttpUserAgentParserAccessor()
    .WithMeterTelemetry(meter)
    .WithTelemetry();

using (ServiceProvider provider = services.BuildServiceProvider())
{
    IHttpUserAgentParserProvider parser = provider.GetRequiredService<IHttpUserAgentParserProvider>();
    Check(parser.Parse(Chrome) is { Name: "Chrome" } && parser.Parse(Chrome) is { Name: "Chrome" }, "memory cached provider");

    DefaultHttpContext context = new();
    context.Request.Headers.UserAgent = Safari;
    Check(provider.GetRequiredService<IHttpUserAgentParserAccessor>().Get(context) is { Name: "Safari" }, "ASP.NET Core accessor");
}

ServiceCollection genericServices = new();
genericServices.AddHttpUserAgentParser<HttpUserAgentParserCachedProvider>();
using (ServiceProvider provider = genericServices.BuildServiceProvider())
{
    Check(provider.GetRequiredService<IHttpUserAgentParserProvider>() is HttpUserAgentParserCachedProvider, "generic provider registration");
}

Console.WriteLine(failures == 0 ? "Native AOT smoke test passed." : $"Native AOT smoke test failed: {failures} check(s).");
return failures == 0 ? 0 : 1;
