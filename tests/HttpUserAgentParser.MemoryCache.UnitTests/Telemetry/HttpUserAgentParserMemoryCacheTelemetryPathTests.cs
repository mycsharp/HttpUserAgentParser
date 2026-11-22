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
