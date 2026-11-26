// Copyright © https://myCSharp.de - all rights reserved

// Native AOT smoke test: exercises the public API of all packages in a trimmed, natively compiled app.
// Exit code 0 means success; every failed check is printed and makes the exit code non-zero.

using System.Collections.Concurrent;
using System.Diagnostics.Metrics;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser;
using MyCSharp.HttpUserAgentParser.AspNetCore;
using MyCSharp.HttpUserAgentParser.AspNetCore.DependencyInjection;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using MyCSharp.HttpUserAgentParser.MemoryCache.DependencyInjection;
using MyCSharp.HttpUserAgentParser.NativeAot.Smoke;
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
Check(HttpUserAgentStatics.Platforms.Count == 44 && HttpUserAgentStatics.Platforms.All(static p => p.Regex is not null), "platform regexes");
Regex chromeRegex = HttpUserAgentStatics.Browsers.First(static b => b.Key.ToString().StartsWith("Chrome", StringComparison.Ordinal)).Key;
Check(string.Equals(chromeRegex.Match(Chrome).Groups[1].Value, "158.0.0.0", StringComparison.Ordinal), "browser regex version group");

// Telemetry listeners: meters and EventCounters must emit data in a Native AOT app (EventSourceSupport is enabled)
ConcurrentDictionary<string, int> measurements = new(StringComparer.Ordinal);
using MeterListener meterListener = new()
{
    InstrumentPublished = static (instrument, listener) =>
    {
        if (instrument.Meter.Name.StartsWith("smoke.", StringComparison.Ordinal))
        {
            listener.EnableMeasurementEvents(instrument);
        }
    },
};
meterListener.SetMeasurementEventCallback<long>((instrument, _, _, _) => measurements.AddOrUpdate(instrument.Name, 1, static (_, count) => count + 1));
meterListener.SetMeasurementEventCallback<double>((instrument, _, _, _) => measurements.AddOrUpdate(instrument.Name, 1, static (_, count) => count + 1));
meterListener.Start();
using EventCounterListener counterListener = new();

// Dependency injection, caching providers and ASP.NET Core accessors
using Meter coreMeter = new("smoke.core");
using Meter memoryCacheMeter = new("smoke.memorycache");
using Meter aspNetCoreMeter = new("smoke.aspnetcore");
ServiceCollection services = new();
services.AddHttpUserAgentMemoryCachedParser(static options => options.CacheOptions.SizeLimit = 16)
    .AddHttpUserAgentParserContextAccessor()
    .WithMeterTelemetry(coreMeter)
    .WithMemoryCacheMeterTelemetry(memoryCacheMeter)
    .WithAspNetCoreMeterTelemetry(aspNetCoreMeter)
    .WithTelemetry()
    .WithMemoryCacheTelemetry()
    .WithAspNetCoreTelemetry();

using (ServiceProvider provider = services.BuildServiceProvider())
{
    IHttpUserAgentParserProvider parser = provider.GetRequiredService<IHttpUserAgentParserProvider>();
    Check(parser.Parse(Chrome) is { Name: "Chrome" } && parser.Parse(Chrome) is { Name: "Chrome" }, "memory cached provider");

    DefaultHttpContext context = new();
    context.Request.Headers.UserAgent = Safari;
    Check(provider.GetRequiredService<IHttpUserAgentParserAccessor>().Get(context) is { Name: "Safari" }, "ASP.NET Core accessor (explicit HttpContext)");

    provider.GetRequiredService<IHttpContextAccessor>().HttpContext = context;
    Check(provider.GetRequiredService<IHttpUserAgentParserContextAccessor>().Get() is { Name: "Safari" }, "ASP.NET Core accessor (IHttpContextAccessor)");

    HttpUserAgentParserCachedProvider cached = new();
    _ = cached.Parse(Chrome);
    HttpUserAgentStatics.SetRobots([("Chrome/", "Custom robot")]);
    Check(cached.Parse(Chrome) is { Type: HttpUserAgentType.Robot, Name: "Custom robot" }, "updated robot rules in dictionary cache");
    Check(parser.Parse(Chrome) is { Type: HttpUserAgentType.Robot, Name: "Custom robot" }, "updated robot rules in memory cache");
    HttpUserAgentStatics.SetRobots(HttpUserAgentStatics.DefaultRobots);
    Check(parser.Parse(Chrome) is { Type: HttpUserAgentType.Browser }, "restored robot rules");
}

foreach (string instrument in (string[])["parse.requests", "parse.duration", "cache.miss", "cache.hit", "user_agent.present"])
{
    Check(measurements.ContainsKey(instrument), $"meter measurement {instrument}");
}

Check(counterListener.WaitForCounters(TimeSpan.FromSeconds(10)), "EventCounters of all packages");

ServiceCollection genericServices = new();
genericServices.AddHttpUserAgentParser<HttpUserAgentParserCachedProvider>();
using (ServiceProvider provider = genericServices.BuildServiceProvider())
{
    Check(provider.GetRequiredService<IHttpUserAgentParserProvider>() is HttpUserAgentParserCachedProvider, "generic provider registration");
}

Console.WriteLine(failures == 0 ? "Native AOT smoke test passed." : $"Native AOT smoke test failed: {failures} check(s).");
return failures == 0 ? 0 : 1;
