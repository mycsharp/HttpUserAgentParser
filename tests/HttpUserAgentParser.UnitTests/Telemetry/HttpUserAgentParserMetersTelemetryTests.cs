// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using MyCSharp.HttpUserAgentParser.Providers;
using MyCSharp.HttpUserAgentParser.Telemetry;
using MyCSharp.HttpUserAgentParser.TestHelpers;
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
        using MeterTestListener listener = new(HttpUserAgentParserMeters.MeterName);

        const string ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36";
        _ = HttpUserAgentInformation.Parse(ua);

        Assert.Empty(listener.InstrumentNames);
    }

    [Theory]
    [InlineData(null, "mycsharp.http_user_agent_parser")]
    [InlineData("acme.", "acme.http_user_agent_parser")]
    [InlineData("", "http_user_agent_parser")]
    public void Configured_Meter_Emits_Expected_Measurements(string? prefix, string meterName)
    {
        using MeterTestListener listener = new(meterName);
        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());
        if (prefix is null)
        {
            options.WithMeterTelemetry();
        }
        else
        {
            options.WithMeterTelemetryPrefix(prefix);
        }

        HttpUserAgentParserCachedProvider provider = new();
        _ = HttpUserAgentInformation.Parse("Firefox/12");
        _ = provider.Parse("Chrome/17");
        _ = provider.Parse("Chrome/17");
        listener.RecordObservableInstruments();

        string[] expected = ["cache.hit", "cache.miss", "cache.size", "parse.duration", "parse.duration", "parse.requests", "parse.requests"];
        Assert.Equal(expected, listener.InstrumentNames.Order(StringComparer.Ordinal), StringComparer.Ordinal);
        Assert.Equal("s", listener.InstrumentUnits["parse.duration"]);
    }

    [Fact]
    public void WithMeterTelemetryPrefix_Throws_WhenInvalid()
    {
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

        Assert.Equal((1, 5), MeterTestListener.CountLiveMeters("repeated.http_user_agent_parser"));
    }

    [Fact]
    public void A_Replaced_Own_Meter_Is_Disposed_But_A_Meter_Of_The_Caller_Is_Not()
    {
        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());
        using Meter callerMeter = new("caller.http_user_agent_parser");

        options.WithMeterTelemetryPrefix("first.");
        options.WithMeterTelemetryPrefix("first."); // the meter in use: kept
        Assert.Equal((1, 5), MeterTestListener.CountLiveMeters("first.http_user_agent_parser"));

        options.WithMeterTelemetryPrefix("second.");
        Assert.Equal((0, 0), MeterTestListener.CountLiveMeters("first.http_user_agent_parser"));
        Assert.Equal((1, 5), MeterTestListener.CountLiveMeters("second.http_user_agent_parser"));

        options.WithMeterTelemetry(callerMeter);
        Assert.Equal((0, 0), MeterTestListener.CountLiveMeters("second.http_user_agent_parser"));

        options.WithMeterTelemetry(); // the default meter replaces the meter of the caller
        Assert.Equal((1, 5), MeterTestListener.CountLiveMeters("caller.http_user_agent_parser"));
        Assert.Equal((1, 5), MeterTestListener.CountLiveMeters("mycsharp.http_user_agent_parser"));
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
            Assert.Equal((0, 0), MeterTestListener.CountLiveMeters($"changing{i}.http_user_agent_parser"));
        }

        Assert.Equal((1, 5), MeterTestListener.CountLiveMeters("changing31.http_user_agent_parser"));
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
        Assert.Equal((0, 0), MeterTestListener.CountLiveMeters("throwing.own")); // the new own meter is disposed again

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

    [Fact]
    public void Retrying_After_A_Counter_Listener_Throws_Reuses_The_Published_Instance()
    {
        using Meter meter = new("retry.counter");
        _ = meter.CreateCounter<long>("parse.requests", "other", "Other definition");
        using (MeterListener failing = new()
        {
            InstrumentPublished = (instrument, _) =>
            {
                if (ReferenceEquals(instrument.Meter, meter) && string.Equals(instrument.Unit, "{call}", StringComparison.Ordinal))
                {
                    HttpUserAgentParserTelemetry.EnableMeters(meter);
                    throw new InvalidOperationException("listener failure");
                }
            },
        })
        {
            failing.Start();
            Assert.Throws<InvalidOperationException>(() => HttpUserAgentParserMeters.Enable(meter));
            Assert.False(HttpUserAgentParserTelemetry.IsEnabled);
        }

        HttpUserAgentParserMeters.Enable(meter);
        Assert.Equal((1, 6), MeterTestListener.CountLiveMeters(meter.Name)); // five parser instruments and the unrelated counter
    }

    [Fact]
    public void Reentrant_Registration_Publishes_Each_Instrument_Once()
    {
        const string name = "reentrant.core";
        using MeterListener listener = new()
        {
            InstrumentPublished = (instrument, _) =>
            {
                if (string.Equals(instrument.Meter.Name, name, StringComparison.Ordinal))
                {
                    HttpUserAgentParserMeters.Enable(instrument.Meter);
                    HttpUserAgentParserMeters.EnableOwnedMeter(name);
                }
            },
        };
        listener.Start();
        HttpUserAgentParserMeters.EnableOwnedMeter(name);
        Assert.Equal((1, 5), MeterTestListener.CountLiveMeters(name));
    }

    [Fact]
    public void A_Nested_Registration_Of_Another_Own_Meter_Leaves_No_Meter_Behind()
    {
        using Meter callerMeter = new("nested.caller");
        using MeterListener listener = new()
        {
            InstrumentPublished = (instrument, _) =>
            {
                if (string.Equals(instrument.Meter.Name, "nested.a", StringComparison.Ordinal)
                    && string.Equals(instrument.Name, "parse.requests", StringComparison.Ordinal))
                {
                    HttpUserAgentParserMeters.EnableOwnedMeter("nested.b");
                }
            },
        };
        listener.Start();

        HttpUserAgentParserMeters.EnableOwnedMeter("nested.a"); // registers nested.b while its instruments are created
        Assert.Equal((1, 5), MeterTestListener.CountLiveMeters("nested.a"));
        Assert.Equal((0, 0), MeterTestListener.CountLiveMeters("nested.b"));

        HttpUserAgentParserMeters.Enable(callerMeter);
        Assert.Equal((0, 0), MeterTestListener.CountLiveMeters("nested.a"));
    }

    [Fact]
    public void Package_Owned_Meter_Can_Be_Reused_Until_Replaced()
    {
        HttpUserAgentParserMeters.EnableOwnedMeter("own.inuse");
        Meter? ownMeter = null;
        using (MeterListener finder = new()
        {
            InstrumentPublished = (instrument, _) =>
            {
                if (string.Equals(instrument.Meter.Name, "own.inuse", StringComparison.Ordinal))
                {
                    ownMeter = instrument.Meter;
                }
            },
        })
        {
            finder.Start();
        }

        HttpUserAgentParserMeters.Enable(ownMeter!);

        Assert.Equal((1, 5), MeterTestListener.CountLiveMeters("own.inuse"));
        using MeterTestListener recorder = new("own.inuse");
        HttpUserAgentParserMeters.ParseRequest();
        Assert.Equal(new[] { "parse.requests" }, recorder.InstrumentNames, StringComparer.Ordinal);

        using Meter replacement = new("replacement.inuse");
        using MeterTestListener active = new(replacement.Name);
        HttpUserAgentParserMeters.Enable(replacement);
        Assert.Throws<ObjectDisposedException>(() => HttpUserAgentParserMeters.Enable(ownMeter!));
        HttpUserAgentParserMeters.ParseRequest();
        Assert.Equal(new[] { "parse.requests" }, active.InstrumentNames, StringComparer.Ordinal);
    }

    [Fact]
    public void A_Cache_Size_Gauge_Whose_Publication_Failed_Is_Not_Created_Again()
    {
        using Meter meter = new("retry.core");
        bool fail = true;
        using MeterListener failing = new()
        {
            InstrumentPublished = (instrument, _) =>
            {
                if (ReferenceEquals(instrument.Meter, meter) && string.Equals(instrument.Name, "cache.size", StringComparison.Ordinal) && fail)
                {
                    fail = false;
                    throw new InvalidOperationException("listener failure");
                }
            },
        };
        failing.Start();

        Assert.Throws<InvalidOperationException>(() => HttpUserAgentParserMeters.Enable(meter));
        HttpUserAgentParserMeters.Enable(meter); // the retry completes the instruments

        int gauges = 0;
        long? size = null;
        using MeterListener observer = new()
        {
            InstrumentPublished = (instrument, target) =>
            {
                if (ReferenceEquals(instrument.Meter, meter) && string.Equals(instrument.Name, "cache.size", StringComparison.Ordinal))
                {
                    gauges++;
                    target.EnableMeasurementEvents(instrument);
                }
            },
        };
        observer.SetMeasurementEventCallback<long>((_, value, _, _) => size = value);
        observer.Start();
        observer.RecordObservableInstruments();

        Assert.Equal(1, gauges);
        Assert.NotNull(size); // the gauge published first reports, because its meter is in use
    }
}
