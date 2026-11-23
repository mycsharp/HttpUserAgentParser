// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
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
        try
        {
            Assert.Equal("Chrome", provider.Parse(UserAgent).Name);
            HttpUserAgentStatics.SetRobots([("Chrome", "Custom")]);
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
    public void Cached_Entry_Releases_The_Old_Rule_List_Before_Its_Next_Lookup()
    {
        using HttpUserAgentParserMemoryCachedProvider provider = new(new HttpUserAgentParserMemoryCachedProviderOptions());
        try
        {
            WeakReference oldRules = CacheWithTemporaryRules(provider);
            HttpUserAgentStatics.SetRobots([]);
            GC.Collect();

            Assert.False(oldRules.IsAlive);
            Assert.Equal("Chrome", provider.Parse(UserAgent).Name);
        }
        finally
        {
            HttpUserAgentStatics.SetRobots(HttpUserAgentStatics.DefaultRobots);
        }
    }

    [MethodImpl(MethodImplOptions.NoInlining)]
    private static WeakReference CacheWithTemporaryRules(HttpUserAgentParserMemoryCachedProvider provider)
    {
        HttpUserAgentStatics.SetRobots([("Chrome", "Temporary")]);
        Assert.Equal("Temporary", provider.Parse(UserAgent).Name);
        return new WeakReference(HttpUserAgentStatics.Robots);
    }

    [Fact]
    public void Expired_Rule_Token_Is_Passive()
    {
        HttpUserAgentParserMemoryCachedProvider.RobotRulesChangeToken expired = new(new object());
        Assert.True(expired.HasChanged);
        Assert.False(expired.ActiveChangeCallbacks);
        using IDisposable registration = expired.RegisterChangeCallback(static _ => Assert.Fail("Passive token invoked a callback."), state: null);
    }

    [Fact]
    public void Miss_Listener_Can_Read_The_Published_Entry_And_Replace_The_Rules()
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
