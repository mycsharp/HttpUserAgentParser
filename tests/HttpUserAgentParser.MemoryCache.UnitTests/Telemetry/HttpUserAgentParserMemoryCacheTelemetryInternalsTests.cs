// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Metrics;
using Microsoft.Extensions.Caching.Memory;
using MyCSharp.HttpUserAgentParser.MemoryCache.Telemetry;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.UnitTests.Telemetry;

/// <summary>
/// Covers the telemetry building blocks of the MemoryCache package directly.
/// </summary>
[Collection(TelemetryCollection.Name)]
public sealed class HttpUserAgentParserMemoryCacheTelemetryInternalsTests : IDisposable
{
    public HttpUserAgentParserMemoryCacheTelemetryInternalsTests() => HttpUserAgentParserMemoryCacheTelemetry.ResetForTests();

    public void Dispose() => HttpUserAgentParserMemoryCacheTelemetry.ResetForTests();

    [Fact]
    public void Meters_Ignore_Measurements_Before_Enable()
    {
        HttpUserAgentParserMemoryCacheMeters.CacheHit();
        HttpUserAgentParserMemoryCacheMeters.CacheMiss();

        Assert.False(HttpUserAgentParserMemoryCacheMeters.IsEnabled);
    }

    [Fact]
    public void Meters_Use_The_Last_Registered_Meter()
    {
        using Meter first = new("test.memorycache.first");
        using Meter second = new("test.memorycache.second");
        using MeterTestListener firstListener = new("test.memorycache.first");
        using MeterTestListener secondListener = new("test.memorycache.second");

        HttpUserAgentParserMemoryCacheMeters.Enable(first);
        HttpUserAgentParserMemoryCacheMeters.Enable(first); // the meter in use: no effect
        HttpUserAgentParserMemoryCacheMeters.CacheHit();
        HttpUserAgentParserMemoryCacheMeters.Enable(second);
        HttpUserAgentParserMemoryCacheMeters.CacheMiss();
        HttpUserAgentParserMemoryCacheMeters.Enable(); // default meter
        HttpUserAgentParserMemoryCacheMeters.Enable(); // default meter in use: no effect

        Assert.True(HttpUserAgentParserMemoryCacheMeters.IsEnabled);
        Assert.Equal(new[] { "cache.hit" }, firstListener.InstrumentNames, StringComparer.Ordinal);
        Assert.Equal(new[] { "cache.miss" }, secondListener.InstrumentNames, StringComparer.Ordinal);
    }

    [Fact]
    public void EventSource_Ignores_Calls_Without_Listener()
    {
        HttpUserAgentParserMemoryCacheEventSource source = HttpUserAgentParserMemoryCacheEventSource.Log;
        Assert.False(source.IsEnabled());

        source.CacheHit();
        source.CacheMiss();

        Assert.False(source.IsEnabled());
    }

    [Fact]
    public void EventSource_Can_Be_Disposed()
    {
        HttpUserAgentParserMemoryCacheEventSource source = new();
        Assert.Equal(HttpUserAgentParserMemoryCacheEventSource.EventSourceName, source.Name);

        source.Dispose();

        Assert.False(source.IsEnabled());
    }

    [Fact]
    public void Evicted_Entries_Decrement_The_Cache_Size()
    {
        using Meter meter = new("test.memorycache.eviction");
        HttpUserAgentParserMemoryCacheTelemetry.EnableMeters(meter);

        // A size limit of one entry: the second user agent exceeds the capacity and gets evicted.
        HttpUserAgentParserMemoryCachedProvider provider = new(new HttpUserAgentParserMemoryCachedProviderOptions(new MemoryCacheOptions { SizeLimit = 1 }));
        _ = provider.Parse("Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/158.0.0.0");
        _ = provider.Parse("Mozilla/5.0 (X11; Linux x86_64; rv:160.0) Gecko/20100101 Firefox/160.0");

        // Eviction callbacks run asynchronously.
        Assert.True(SpinWait.SpinUntil(static () => HttpUserAgentParserMemoryCacheTelemetryState.CacheSize < 2, TimeSpan.FromSeconds(10)));
    }
}
