// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using MyCSharp.HttpUserAgentParser.MemoryCache.DependencyInjection;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.UnitTests.Telemetry;

public class HttpUserAgentParserMemoryCacheMetersTelemetryTests
{
    [Fact]
    public void Meters_Emit_WhenEnabled()
    {
        using MeterTestListener listener = new("mycsharp.http_user_agent_parser.memorycache");

        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection())
            .WithMemoryCacheMeterTelemetry();

        HttpUserAgentParserMemoryCachedProvider provider = new(new HttpUserAgentParserMemoryCachedProviderOptions());

        const string ua1 = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36";
        const string ua2 = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Firefox/88.0";

        _ = provider.Parse(ua1); // miss
        _ = provider.Parse(ua1); // hit
        _ = provider.Parse(ua2); // miss

        listener.RecordObservableInstruments();

        Assert.Contains("cache.hit", listener.InstrumentNames);
        Assert.Contains("cache.miss", listener.InstrumentNames);
        Assert.Contains("cache.size", listener.InstrumentNames);
    }
}
