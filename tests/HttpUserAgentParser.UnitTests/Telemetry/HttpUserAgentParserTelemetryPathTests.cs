// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using MyCSharp.HttpUserAgentParser.Providers;
using MyCSharp.HttpUserAgentParser.Telemetry;
using MyCSharp.HttpUserAgentParser.TestHelpers;
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
    public void Counters_Without_Listener_Do_Not_Measure_Duration()
    {
        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection()).WithTelemetry();

        Assert.True(HttpUserAgentParserTelemetry.IsEnabled);
        Assert.False(HttpUserAgentParserTelemetry.ShouldMeasureParseDuration);

        AssertParsesAndCaches();
    }

    [Fact]
    public void Meters_Without_Duration_Listener_Do_Not_Measure_Duration()
    {
        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection()).WithMeterTelemetry();

        Assert.True(HttpUserAgentParserTelemetry.IsEnabled);
        Assert.False(HttpUserAgentParserTelemetry.ShouldMeasureParseDuration);

        AssertParsesAndCaches();
    }

    [Fact]
    public void Cache_Miss_Listener_Can_Read_The_New_Entry()
    {
        using Meter meter = new("test.cache.miss");
        HttpUserAgentParserCachedProvider provider = new();
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
            }
        });
        listener.Start();
        HttpUserAgentParserTelemetry.EnableMeters(meter);
        Assert.Equal("Chrome", provider.Parse(UserAgent).Name);
        Assert.Equal(1, misses);
    }

    [Fact]
    public void Cache_Size_Includes_Entries_Cached_Before_Telemetry_Was_Enabled()
    {
        HttpUserAgentParserCachedProvider provider = new();
        _ = provider.Parse("Chrome/17");
        _ = provider.Parse("Firefox/12");

        using Meter meter = new("test.size.late");
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

        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection()).WithMeterTelemetry(meter);
        _ = provider.Parse("Chrome/17"); // hit
        listener.RecordObservableInstruments();

        // Providers of tests running in parallel may add to the size.
        Assert.True(size >= 2, $"cache.size: {size}");
        GC.KeepAlive(provider);
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
    public void Only_The_Meter_In_Use_Reports_The_Cache_Size()
    {
        using Meter first = new("test.size.first");
        using Meter second = new("test.size.second");
        using MeterTestListener firstListener = new("test.size.first");
        using MeterTestListener secondListener = new("test.size.second");

        HttpUserAgentParserTelemetry.EnableMeters(first);
        HttpUserAgentParserTelemetry.EnableMeters(second);
        firstListener.RecordObservableInstruments();
        secondListener.RecordObservableInstruments();

        Assert.Empty(firstListener.InstrumentNames);
        Assert.Equal(new[] { "cache.size" }, secondListener.InstrumentNames, StringComparer.Ordinal);

        HttpUserAgentParserTelemetry.EnableMeters(first); // reuses the instruments of the first meter
        firstListener.RecordObservableInstruments();
        secondListener.RecordObservableInstruments();

        Assert.Equal(new[] { "cache.size" }, firstListener.InstrumentNames, StringComparer.Ordinal);
        Assert.Equal(new[] { "cache.size" }, secondListener.InstrumentNames, StringComparer.Ordinal);
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
        using Meter earlier = new("test.earlier");
        ExecutionContext? captured = null;
        using Meter meter = new("test.race");
        using ManualResetEventSlim creating = new();
        using ManualResetEventSlim release = new();
        using MeterListener listener = new()
        {
            InstrumentPublished = (instrument, target) =>
            {
                if (instrument.Meter == earlier)
                {
                    captured = ExecutionContext.Capture();
                }

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

        HttpUserAgentParserTelemetry.EnableMeters(earlier);
        Assert.NotNull(captured);
        Task first = Task.Run(() => HttpUserAgentParserTelemetry.EnableMeters(meter), cancellationToken);
        Assert.True(creating.Wait(TimeSpan.FromSeconds(10), cancellationToken));
        Task second = Task.Run(() => ExecutionContext.Run(captured, _ => HttpUserAgentParserTelemetry.EnableMeters(meter), null), cancellationToken);

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
