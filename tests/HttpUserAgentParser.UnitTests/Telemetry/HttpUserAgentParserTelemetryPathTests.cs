// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Metrics;
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
    public void Meters_Use_The_Last_Registered_Meter()
    {
        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());

        using Meter first = new("test.first");
        using Meter second = new("test.second");
        using MeterTestListener firstListener = new("test.first");
        using MeterTestListener secondListener = new("test.second");

        options.WithMeterTelemetry(first);
        _ = HttpUserAgentParser.Parse(UserAgent);
        options.WithMeterTelemetry(second);
        _ = HttpUserAgentParser.Parse(UserAgent);

        Assert.Single(firstListener.InstrumentNames, static name => string.Equals(name, "parse.requests", StringComparison.Ordinal));
        Assert.Single(secondListener.InstrumentNames, static name => string.Equals(name, "parse.requests", StringComparison.Ordinal));
    }

    [Fact]
    public void Registering_The_Same_Meter_Again_Keeps_The_Instruments()
    {
        using Meter meter = new("test.same");
        int published = 0;
        int publishedOnDefaultMeter = 0;
        using MeterListener listener = new()
        {
            InstrumentPublished = (instrument, _) =>
            {
                published += instrument.Meter == meter ? 1 : 0;
                publishedOnDefaultMeter += string.Equals(instrument.Meter.Name, HttpUserAgentParserMeters.MeterName, StringComparison.Ordinal) ? 1 : 0;
            },
        };
        listener.Start();
        publishedOnDefaultMeter = 0; // ignore instruments of earlier tests

        HttpUserAgentParserTelemetry.EnableMeters(meter);
        HttpUserAgentParserTelemetry.EnableMeters(meter);
        Assert.Equal(5, published);

        HttpUserAgentParserTelemetry.EnableMeters();
        HttpUserAgentParserTelemetry.EnableMeters();
        Assert.Equal(5, publishedOnDefaultMeter);
    }

    [Fact]
    public async Task Concurrent_Registration_Returns_After_All_Instruments_Exist()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        using Meter meter = new("test.race");
        using ManualResetEventSlim creating = new();
        using ManualResetEventSlim release = new();
        using MeterListener listener = new()
        {
            InstrumentPublished = (instrument, target) =>
            {
                if (instrument.Meter != meter)
                {
                    return;
                }

                if (string.Equals(instrument.Name, "parse.requests", StringComparison.Ordinal))
                {
                    // The first registration stops while it creates its instruments.
                    creating.Set();
                    release.Wait(TimeSpan.FromSeconds(10), cancellationToken);
                }

                target.EnableMeasurementEvents(instrument);
            },
        };
        listener.Start();

        Task first = Task.Run(() => HttpUserAgentParserTelemetry.EnableMeters(meter), cancellationToken);
        Assert.True(creating.Wait(TimeSpan.FromSeconds(10), cancellationToken));
        Task second = Task.Run(() => HttpUserAgentParserTelemetry.EnableMeters(meter), cancellationToken);

        // The second registration must not return while the instruments of the first one are incomplete.
        Assert.NotSame(second, await Task.WhenAny(second, Task.Delay(200, cancellationToken)));

        release.Set();
        await Task.WhenAll(first, second).WaitAsync(TimeSpan.FromSeconds(10), cancellationToken);

        Assert.True(HttpUserAgentParserTelemetry.ShouldMeasureParseDuration);
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
