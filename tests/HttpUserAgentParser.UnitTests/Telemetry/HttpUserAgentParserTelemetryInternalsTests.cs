// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Metrics;
using MyCSharp.HttpUserAgentParser.Providers;
using MyCSharp.HttpUserAgentParser.Telemetry;
using MyCSharp.HttpUserAgentParser.TestHelpers;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests.Telemetry;

/// <summary>
/// Covers the telemetry building blocks directly: meters before and after enabling, EventSource methods without
/// listener, and the lifetime of an EventSource instance.
/// </summary>
[Collection(TelemetryCollection.Name)]
public sealed class HttpUserAgentParserTelemetryInternalsTests : IDisposable
{
    public HttpUserAgentParserTelemetryInternalsTests() => HttpUserAgentParserTelemetry.ResetForTests();

    public void Dispose() => HttpUserAgentParserTelemetry.ResetForTests();

    [Fact]
    public void Meters_Ignore_Measurements_Before_Enable()
    {
        Assert.False(HttpUserAgentParserMeters.IsEnabled);
        Assert.False(HttpUserAgentParserMeters.IsParseDurationEnabled);

        HttpUserAgentParserMeters.ParseRequest();
        HttpUserAgentParserMeters.ParseDuration(0.1);
        HttpUserAgentParserMeters.CacheHit();
        HttpUserAgentParserMeters.CacheMiss();

        Assert.False(HttpUserAgentParserMeters.IsEnabled);
    }

    [Fact]
    public void Meters_Record_Measurements_After_Enable()
    {
        using Meter meter = new("test.core.meters");
        using MeterTestListener listener = new("test.core.meters");

        HttpUserAgentParserMeters.Enable(meter);
        HttpUserAgentParserMeters.ParseRequest();
        HttpUserAgentParserMeters.ParseDuration(0.1);
        HttpUserAgentParserMeters.CacheHit();
        HttpUserAgentParserMeters.CacheMiss();

        Assert.True(HttpUserAgentParserMeters.IsEnabled);
        Assert.Equal(new[] { "cache.hit", "cache.miss", "parse.duration", "parse.requests" }, listener.InstrumentNames.Order(StringComparer.Ordinal), StringComparer.Ordinal);
    }

    [Fact]
    public void Parse_Duration_Is_Only_Measured_With_A_Listener()
    {
        using Meter meter = new("test.core.duration");
        HttpUserAgentParserMeters.Enable(meter);

        Assert.False(HttpUserAgentParserMeters.IsParseDurationEnabled);

        using MeterTestListener listener = new("test.core.duration");
        Assert.True(HttpUserAgentParserMeters.IsParseDurationEnabled);
    }

    [Fact]
    public void Parse_Duration_Is_Measured_For_Meters_While_Counters_Have_No_Listener()
    {
        using Meter meter = new("test.core.both");
        using MeterTestListener listener = new("test.core.both");

        HttpUserAgentParserTelemetry.Enable();
        HttpUserAgentParserTelemetry.EnableMeters(meter);

        Assert.False(HttpUserAgentParserTelemetry.AreCountersEnabled);
        Assert.True(HttpUserAgentParserTelemetry.ShouldMeasureParseDuration);

        _ = HttpUserAgentParser.Parse("Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/158.0.0.0");

        Assert.Contains("parse.duration", listener.InstrumentNames, StringComparer.Ordinal);
    }

    [Fact]
    public void EventSource_Ignores_Calls_Without_Listener()
    {
        HttpUserAgentParserEventSource source = HttpUserAgentParserEventSource.Log;
        Assert.False(source.IsEnabled());

        source.ParseRequest();
        source.ParseDuration(0.1);
        source.CacheHit();
        source.CacheMiss();

        Assert.False(source.IsEnabled());
    }

    [Fact]
    public void EventSource_Can_Be_Disposed()
    {
        HttpUserAgentParserEventSource source = new();
        Assert.Equal(HttpUserAgentParserEventSource.EventSourceName, source.Name);

        source.Dispose();

        Assert.False(source.IsEnabled());
    }

    [Fact]
    public void Telemetry_State_Counts_The_Entries_Of_Live_Providers()
    {
        HttpUserAgentParserCachedProvider provider = new();
        _ = provider.Parse("Chrome/17");
        _ = provider.Parse("Firefox/12");
        _ = provider.Parse("Chrome/17");

        // Providers of tests running in parallel may add to the size.
        Assert.True(HttpUserAgentParserTelemetryState.ConcurrentCacheSize >= 2);
        GC.KeepAlive(provider);
    }
}
