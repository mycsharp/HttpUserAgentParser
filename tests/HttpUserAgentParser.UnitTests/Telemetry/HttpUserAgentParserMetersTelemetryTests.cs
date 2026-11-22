// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using MyCSharp.HttpUserAgentParser.Providers;
using MyCSharp.HttpUserAgentParser.Telemetry;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests.Telemetry;

[Collection(TelemetryCollection.Name)]
public sealed class HttpUserAgentParserMetersTelemetryTests : IDisposable
{
    public HttpUserAgentParserMetersTelemetryTests() => HttpUserAgentParserTelemetry.ResetForTests();

    public void Dispose() => HttpUserAgentParserTelemetry.ResetForTests();

    [Fact]
    public void Meters_DoNotEmit_WhenDisabled()
    {
        HttpUserAgentParserTelemetry.ResetForTests();

        using MeterTestListener listener = new(MyCSharp.HttpUserAgentParser.Telemetry.HttpUserAgentParserMeters.MeterName);

        const string ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36";
        _ = HttpUserAgentInformation.Parse(ua);

        Assert.Empty(listener.InstrumentNames);
    }

    [Fact]
    public void Meters_Emit_WhenEnabled()
    {
        HttpUserAgentParserTelemetry.ResetForTests();

        using MeterTestListener listener = new("mycsharp.http_user_agent_parser");

        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection())
            .WithMeterTelemetry();

        const string ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36";

        _ = HttpUserAgentInformation.Parse(ua);

        HttpUserAgentParserCachedProvider provider = new();
        _ = provider.Parse(ua); // miss
        _ = provider.Parse(ua); // hit

        listener.RecordObservableInstruments();

        Assert.Contains("parse.requests", listener.InstrumentNames, StringComparer.Ordinal);
        Assert.Contains("parse.duration", listener.InstrumentNames, StringComparer.Ordinal);
        Assert.Contains("cache.hit", listener.InstrumentNames, StringComparer.Ordinal);
        Assert.Contains("cache.miss", listener.InstrumentNames, StringComparer.Ordinal);
        Assert.Contains("cache.size", listener.InstrumentNames, StringComparer.Ordinal);

        Assert.Equal("s", listener.InstrumentUnits["parse.duration"]);
    }

    [Fact]
    public void Meters_Emit_WhenEnabled_WithCustomPrefix()
    {
        HttpUserAgentParserTelemetry.ResetForTests();

        const string prefix = "acme.";
        using MeterTestListener listener = new("acme.http_user_agent_parser");

        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection())
            .WithMeterTelemetryPrefix(prefix);

        const string ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36";

        _ = HttpUserAgentInformation.Parse(ua);

        HttpUserAgentParserCachedProvider provider = new();
        _ = provider.Parse(ua); // miss
        _ = provider.Parse(ua); // hit

        listener.RecordObservableInstruments();

        Assert.Contains("parse.requests", listener.InstrumentNames, StringComparer.Ordinal);
        Assert.Contains("parse.duration", listener.InstrumentNames, StringComparer.Ordinal);
        Assert.Contains("cache.hit", listener.InstrumentNames, StringComparer.Ordinal);
        Assert.Contains("cache.miss", listener.InstrumentNames, StringComparer.Ordinal);
        Assert.Contains("cache.size", listener.InstrumentNames, StringComparer.Ordinal);
    }

    [Fact]
    public void Meters_Emit_WhenEnabled_WithEmptyPrefix()
    {
        HttpUserAgentParserTelemetry.ResetForTests();

        using MeterTestListener listener = new("http_user_agent_parser");

        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection())
            .WithMeterTelemetryPrefix(string.Empty);

        const string ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36";

        _ = HttpUserAgentInformation.Parse(ua);

        listener.RecordObservableInstruments();

        Assert.Contains("parse.requests", listener.InstrumentNames, StringComparer.Ordinal);
        Assert.Contains("parse.duration", listener.InstrumentNames, StringComparer.Ordinal);
    }

    [Fact]
    public void WithMeterTelemetryPrefix_Throws_WhenInvalid()
    {
        HttpUserAgentParserTelemetry.ResetForTests();

        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());

        Assert.Throws<ArgumentException>(() => options.WithMeterTelemetryPrefix("acme"));
        Assert.Throws<ArgumentException>(() => options.WithMeterTelemetryPrefix("acme-"));
        Assert.Throws<ArgumentException>(() => options.WithMeterTelemetryPrefix("acme.."));
        Assert.Throws<ArgumentException>(() => options.WithMeterTelemetryPrefix("."));
        Assert.Throws<ArgumentException>(() => options.WithMeterTelemetryPrefix("\u00E4."));
        Assert.Throws<ArgumentException>(() => options.WithMeterTelemetryPrefix("\uFF11."));
    }

    [Fact]
    public void Repeated_Prefix_Registrations_Keep_One_Meter()
    {
        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());

        for (int i = 0; i < 32; i++)
        {
            options.WithMeterTelemetryPrefix("repeated.");
        }

        Assert.Equal((1, 5), CountLiveMeters("repeated.http_user_agent_parser"));
    }

    [Fact]
    public void A_Replaced_Own_Meter_Is_Disposed_But_A_Meter_Of_The_Caller_Is_Not()
    {
        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());
        using Meter callerMeter = new("caller.http_user_agent_parser");

        options.WithMeterTelemetryPrefix("first.");
        options.WithMeterTelemetryPrefix("first."); // the meter in use: kept
        Assert.Equal((1, 5), CountLiveMeters("first.http_user_agent_parser"));

        options.WithMeterTelemetryPrefix("second.");
        Assert.Equal((0, 0), CountLiveMeters("first.http_user_agent_parser"));
        Assert.Equal((1, 5), CountLiveMeters("second.http_user_agent_parser"));

        options.WithMeterTelemetry(callerMeter);
        Assert.Equal((0, 0), CountLiveMeters("second.http_user_agent_parser"));

        options.WithMeterTelemetry(); // the default meter replaces the meter of the caller
        Assert.Equal((1, 5), CountLiveMeters("caller.http_user_agent_parser"));
        Assert.Equal((1, 5), CountLiveMeters("mycsharp.http_user_agent_parser"));
    }

    [Fact]
    public void Changing_Prefixes_Keep_Only_The_Meter_In_Use()
    {
        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());

        for (int i = 0; i < 32; i++)
        {
            options.WithMeterTelemetryPrefix($"changing{i}.");
        }

        for (int i = 0; i < 31; i++)
        {
            Assert.Equal((0, 0), CountLiveMeters($"changing{i}.http_user_agent_parser"));
        }

        Assert.Equal((1, 5), CountLiveMeters("changing31.http_user_agent_parser"));
    }

    [Fact]
    public void A_Throwing_Listener_Keeps_The_Meter_In_Use()
    {
        using Meter first = new("throwing.first");
        using Meter second = new("throwing.second");
        using MeterListener listener = new()
        {
            InstrumentPublished = (instrument, _) =>
            {
                if (instrument.Meter.Name.StartsWith("throwing.", StringComparison.Ordinal) && !ReferenceEquals(instrument.Meter, first))
                {
                    throw new InvalidOperationException("listener failure");
                }
            },
        };
        listener.Start();

        HttpUserAgentParserMeters.Enable(first);
        Assert.Throws<InvalidOperationException>(() => HttpUserAgentParserMeters.Enable(second));
        Assert.Throws<InvalidOperationException>(() => HttpUserAgentParserMeters.EnableOwnedMeter("throwing.own"));
        Assert.Equal((0, 0), CountLiveMeters("throwing.own")); // the new own meter is disposed again

        using MeterTestListener recorder = new("throwing.first");
        HttpUserAgentParserMeters.ParseRequest();
        Assert.Equal(new[] { "parse.requests" }, recorder.InstrumentNames, StringComparer.Ordinal);
    }

    [Fact]
    public async Task No_Measurement_Is_Lost_While_Meters_Of_The_Caller_Are_Switched()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        using Meter a = new("switch.a");
        using Meter b = new("switch.b");
        long recorded = 0;
        using MeterListener listener = new()
        {
            InstrumentPublished = (instrument, target) =>
            {
                if (instrument.Meter.Name.StartsWith("switch.", StringComparison.Ordinal)
                    && string.Equals(instrument.Name, "parse.requests", StringComparison.Ordinal))
                {
                    target.EnableMeasurementEvents(instrument);
                }
            },
        };
        listener.SetMeasurementEventCallback<long>((_, value, _, _) => Interlocked.Add(ref recorded, value));
        listener.Start();

        HttpUserAgentParserMeters.Enable(a);
        using CancellationTokenSource stop = new(TimeSpan.FromMilliseconds(300));
        Task switcher = Task.Run(
            () =>
            {
                for (int i = 0; !stop.IsCancellationRequested; i++)
                {
                    HttpUserAgentParserMeters.Enable(i % 2 == 0 ? b : a);
                }
            },
            cancellationToken);

        long measurements = 0;
        while (!stop.IsCancellationRequested)
        {
            HttpUserAgentParserMeters.ParseRequest();
            measurements++;
        }

        await switcher;
        Assert.Equal(measurements, Interlocked.Read(ref recorded));
    }

    /// <summary>
    /// Counts the meters with instruments that a new listener sees, i.e. the meters that are not disposed.
    /// </summary>
    private static (int Meters, int Instruments) CountLiveMeters(string meterName)
    {
        HashSet<Meter> meters = [];
        int instruments = 0;
        using MeterListener listener = new()
        {
            InstrumentPublished = (instrument, _) =>
            {
                if (string.Equals(instrument.Meter.Name, meterName, StringComparison.Ordinal))
                {
                    meters.Add(instrument.Meter);
                    instruments++;
                }
            },
        };
        listener.Start();
        return (meters.Count, instruments);
    }

    [Fact]
    public void Core_And_MemoryCache_Cannot_Share_A_Meter()
    {
        using Meter shared = new("test.guard.shared");
        using Meter other = new("test.guard.other");
        using Meter third = new("test.guard.third");

        HttpUserAgentParserCacheMeterGuard.ReserveForCore(shared);
        HttpUserAgentParserCacheMeterGuard.ReserveForCore(shared); // again: no effect
        Assert.Throws<ArgumentException>(() => HttpUserAgentParserCacheMeterGuard.ReserveForMemoryCache(shared));

        HttpUserAgentParserCacheMeterGuard.ReserveForMemoryCache(other);
        Assert.Throws<ArgumentException>(() => HttpUserAgentParserCacheMeterGuard.ReserveForCore(other));

        // The instruments of the core package remain on the first meter after it moved to another one.
        HttpUserAgentParserCacheMeterGuard.ReserveForCore(third);
        Assert.Throws<ArgumentException>(() => HttpUserAgentParserCacheMeterGuard.ReserveForMemoryCache(shared));

        HttpUserAgentParserCacheMeterGuard.ResetMemoryCacheForTests();
    }

    [Fact]
    public void WithMeterTelemetry_Rejects_The_Meter_Of_The_MemoryCache_Package()
    {
        using Meter shared = new("test.guard.memorycache");
        HttpUserAgentParserCacheMeterGuard.ReserveForMemoryCache(shared);

        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());
        Assert.Throws<ArgumentException>(() => options.WithMeterTelemetry(shared));
        Assert.False(HttpUserAgentParserTelemetry.IsEnabled);

        HttpUserAgentParserCacheMeterGuard.ResetMemoryCacheForTests();
    }
}
