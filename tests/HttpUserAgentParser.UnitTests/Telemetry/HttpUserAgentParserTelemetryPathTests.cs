// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using MyCSharp.HttpUserAgentParser.Providers;
using MyCSharp.HttpUserAgentParser.Telemetry;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests.Telemetry;

/// <summary>
/// Covers every telemetry state of the parser and the cached provider explicitly,
/// instead of relying on the order in which other tests switch telemetry on or off.
/// </summary>
[Collection(TelemetryCollection.Name)]
public sealed class HttpUserAgentParserTelemetryPathTests : IDisposable
{
    private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/158.0.0.0 Safari/537.36";

    public HttpUserAgentParserTelemetryPathTests() => HttpUserAgentParserTelemetry.ResetForTests();

    public void Dispose() => HttpUserAgentParserTelemetry.ResetForTests();

    [Fact]
    public void Disabled_Telemetry_Parses_Without_Instrumentation()
    {
        Assert.False(HttpUserAgentParserTelemetry.IsEnabled);

        AssertParsesAndCaches();
    }

    [Fact]
    public void Counters_Without_Listener_Neither_Measure_Duration_Nor_Track_Cache_Size()
    {
        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection()).WithTelemetry();

        Assert.True(HttpUserAgentParserTelemetry.IsEnabled);
        Assert.False(HttpUserAgentParserTelemetry.ShouldMeasureParseDuration);
        Assert.False(HttpUserAgentParserTelemetry.IsCacheSizeEnabled);

        AssertParsesAndCaches();
    }

    [Fact]
    public void Meters_Without_Duration_Listener_Track_Cache_Size_But_Do_Not_Measure_Duration()
    {
        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection()).WithMeterTelemetry();

        Assert.True(HttpUserAgentParserTelemetry.IsEnabled);
        Assert.False(HttpUserAgentParserTelemetry.ShouldMeasureParseDuration);
        Assert.True(HttpUserAgentParserTelemetry.IsCacheSizeEnabled);

        AssertParsesAndCaches();
        Assert.Equal(1, HttpUserAgentParserTelemetryState.ConcurrentCacheSize);
    }

    [Fact]
    public void Meters_Enable_Only_Once()
    {
        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());

        using System.Diagnostics.Metrics.Meter first = new("test.first");
        using System.Diagnostics.Metrics.Meter second = new("test.second");
        using MeterTestListener listener = new("test.first");

        options.WithMeterTelemetry(first).WithMeterTelemetry(second);
        _ = HttpUserAgentParser.Parse(UserAgent);

        Assert.Contains("parse.requests", listener.InstrumentNames, StringComparer.Ordinal);
    }

    private static void AssertParsesAndCaches()
    {
        Assert.Equal("Chrome", HttpUserAgentParser.Parse(UserAgent).Name);

        HttpUserAgentParserCachedProvider provider = new();
        Assert.Equal("Chrome", provider.Parse(UserAgent).Name); // miss
        Assert.Equal("Chrome", provider.Parse(UserAgent).Name); // hit
        Assert.Equal(1, provider.CacheEntryCount);
    }
}
