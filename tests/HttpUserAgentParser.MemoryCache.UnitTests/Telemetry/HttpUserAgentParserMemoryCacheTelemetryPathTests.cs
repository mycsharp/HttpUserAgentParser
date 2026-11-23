// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using MyCSharp.HttpUserAgentParser.MemoryCache.DependencyInjection;
using MyCSharp.HttpUserAgentParser.MemoryCache.Telemetry;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.UnitTests.Telemetry;

/// <summary>
/// Covers every telemetry state of the memory cached provider explicitly.
/// </summary>
[Collection(TelemetryCollection.Name)]
public sealed class HttpUserAgentParserMemoryCacheTelemetryPathTests : IDisposable
{
    private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/158.0.0.0 Safari/537.36";

    public HttpUserAgentParserMemoryCacheTelemetryPathTests() => HttpUserAgentParserMemoryCacheTelemetry.ResetForTests();

    public void Dispose() => HttpUserAgentParserMemoryCacheTelemetry.ResetForTests();

    [Fact]
    public void Cached_Result_Expires_When_Robot_Rules_Change()
    {
        using HttpUserAgentParserMemoryCachedProvider provider = new(new HttpUserAgentParserMemoryCachedProviderOptions());
        HttpUserAgentParserMemoryCachedProvider.RobotRulesChangeToken change = new(HttpUserAgentStatics.Robots);
        Assert.False(change.ActiveChangeCallbacks);
        Assert.False(change.HasChanged);
        using IDisposable registration = change.RegisterChangeCallback(static _ => Assert.Fail("A passive token must not invoke callbacks."), null);
        try
        {
            Assert.Equal("Chrome", provider.Parse(UserAgent).Name);
            HttpUserAgentStatics.SetRobots([("Chrome", "Custom")]);
            Assert.True(change.HasChanged);
            Assert.Equal("Custom", provider.Parse(UserAgent).Name);
            HttpUserAgentStatics.SetRobots([]);
            Assert.Equal("Chrome", provider.Parse(UserAgent).Name);
        }
        finally
        {
            HttpUserAgentStatics.SetRobots(HttpUserAgentStatics.DefaultRobots);
        }
    }

    [Fact]
    public void Rule_Change_During_A_Miss_Cannot_Leave_A_Stale_Cache_Hit()
    {
        using HttpUserAgentParserMemoryCachedProvider provider = new(new HttpUserAgentParserMemoryCachedProviderOptions());
        using Meter meter = new("robots.memorycache");
        using MeterListener listener = new()
        {
            InstrumentPublished = (instrument, target) =>
            {
                if (instrument.Meter == meter && string.Equals(instrument.Name, "cache.miss", StringComparison.Ordinal))
                {
                    target.EnableMeasurementEvents(instrument);
                }
            },
        };
        int misses = 0;
        listener.SetMeasurementEventCallback<long>((_, _, _, _) =>
        {
            if (++misses == 1)
            {
                Assert.Equal("Chrome", provider.Parse(UserAgent).Name);
                HttpUserAgentStatics.SetRobots([("Chrome", "New robot")]);
                Assert.Equal("New robot", provider.Parse(UserAgent).Name);
            }
        });
        listener.Start();
        try
        {
            HttpUserAgentParserMemoryCacheTelemetry.EnableMeters(meter);
            Assert.Equal("Chrome", provider.Parse(UserAgent).Name);
            Assert.Equal("New robot", provider.Parse(UserAgent).Name);
            Assert.Equal(2, misses);
        }
        finally
        {
            HttpUserAgentStatics.SetRobots(HttpUserAgentStatics.DefaultRobots);
        }
    }

    [Fact]
    public void Disabled_Telemetry_Parses_And_Caches()
    {
        Assert.False(HttpUserAgentParserMemoryCacheTelemetry.IsEnabled);

        AssertParsesAndCaches();
    }

    [Fact]
    public void Counters_Without_Listener_Parse_And_Cache()
    {
        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection()).WithMemoryCacheTelemetry();

        Assert.True(HttpUserAgentParserMemoryCacheTelemetry.IsEnabled);

        AssertParsesAndCaches();
    }

    [Fact]
    public void Cache_Size_Includes_Entries_Cached_Before_Telemetry_Was_Enabled()
    {
        HttpUserAgentParserMemoryCachedProvider provider = new(new HttpUserAgentParserMemoryCachedProviderOptions());
        _ = provider.Parse("Chrome/17");
        _ = provider.Parse("Firefox/12");

        using Meter meter = new("test.memorycache.size.late");
        long? size = null;
        using MeterListener listener = new()
        {
            InstrumentPublished = (instrument, target) =>
            {
                if (instrument.Meter == meter && string.Equals(instrument.Name, "cache.size", StringComparison.Ordinal))
                {
                    target.EnableMeasurementEvents(instrument);
                }
            },
        };
        listener.SetMeasurementEventCallback<long>((_, value, _, _) => size = value);
        listener.Start();

        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection()).WithMemoryCacheMeterTelemetry(meter);
        _ = provider.Parse("Chrome/17"); // hit
        listener.RecordObservableInstruments();

        // Providers of tests running in parallel may add to the size.
        Assert.True(size >= 2, $"cache.size: {size}");
        GC.KeepAlive(provider);
    }

    private static void AssertParsesAndCaches()
    {
        HttpUserAgentParserMemoryCachedProvider provider = new(new HttpUserAgentParserMemoryCachedProviderOptions());

        HttpUserAgentInformation miss = provider.Parse(UserAgent);
        HttpUserAgentInformation hit = provider.Parse(UserAgent);

        Assert.Equal("Chrome", miss.Name);
        Assert.Equal(miss, hit);
    }
}
