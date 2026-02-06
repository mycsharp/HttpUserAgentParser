// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using MyCSharp.HttpUserAgentParser.MemoryCache.DependencyInjection;
using MyCSharp.HttpUserAgentParser.MemoryCache.Telemetry;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.UnitTests.Telemetry;

public class HttpUserAgentParserMemoryCacheMetersTelemetryTests
{
    [Fact]
    public void Meters_Emit_WhenEnabled()
    {
        HttpUserAgentParserMemoryCacheTelemetry.ResetForTests();

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

    [Fact]
    public void Meters_Emit_WhenEnabled_WithCustomPrefix()
    {
        HttpUserAgentParserMemoryCacheTelemetry.ResetForTests();

        const string prefix = "acme.";
        using MeterTestListener listener = new("acme.http_user_agent_parser.memorycache");

        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection())
            .WithMemoryCacheMeterTelemetryPrefix(prefix);

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

    [Fact]
    public void Meters_Emit_WhenEnabled_WithEmptyPrefix()
    {
        HttpUserAgentParserMemoryCacheTelemetry.ResetForTests();

        using MeterTestListener listener = new("http_user_agent_parser.memorycache");

        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection())
            .WithMemoryCacheMeterTelemetryPrefix(string.Empty);

        HttpUserAgentParserMemoryCachedProvider provider = new(new HttpUserAgentParserMemoryCachedProviderOptions());

        const string ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36";

        _ = provider.Parse(ua); // miss

        listener.RecordObservableInstruments();

        Assert.Contains("cache.miss", listener.InstrumentNames);
        Assert.Contains("cache.size", listener.InstrumentNames);
    }

    [Fact]
    public void WithMemoryCacheMeterTelemetryPrefix_Throws_WhenInvalid()
    {
        HttpUserAgentParserMemoryCacheTelemetry.ResetForTests();

        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());

        Assert.Throws<ArgumentException>(() => options.WithMemoryCacheMeterTelemetryPrefix("acme"));
        Assert.Throws<ArgumentException>(() => options.WithMemoryCacheMeterTelemetryPrefix("acme-"));
        Assert.Throws<ArgumentException>(() => options.WithMemoryCacheMeterTelemetryPrefix("acme.."));
    }
}
