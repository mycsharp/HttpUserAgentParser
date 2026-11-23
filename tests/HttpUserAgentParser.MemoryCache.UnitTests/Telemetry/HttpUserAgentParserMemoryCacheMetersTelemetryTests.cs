// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using MyCSharp.HttpUserAgentParser.MemoryCache.DependencyInjection;
using MyCSharp.HttpUserAgentParser.MemoryCache.Telemetry;
using MyCSharp.HttpUserAgentParser.TestHelpers;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.UnitTests.Telemetry;

[Collection(TelemetryCollection.Name)]
public sealed class HttpUserAgentParserMemoryCacheMetersTelemetryTests : IDisposable
{
    public HttpUserAgentParserMemoryCacheMetersTelemetryTests() => HttpUserAgentParserMemoryCacheTelemetry.ResetForTests();

    public void Dispose() => HttpUserAgentParserMemoryCacheTelemetry.ResetForTests();

    [Theory]
    [InlineData(null, "mycsharp.http_user_agent_parser.memorycache")]
    [InlineData("acme.", "acme.http_user_agent_parser.memorycache")]
    [InlineData("", "http_user_agent_parser.memorycache")]
    public void Configured_Meter_Emits_Expected_Measurements(string? prefix, string meterName)
    {
        using MeterTestListener listener = new(meterName);
        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());
        if (prefix is null)
        {
            options.WithMemoryCacheMeterTelemetry();
        }
        else
        {
            options.WithMemoryCacheMeterTelemetryPrefix(prefix);
        }

        using HttpUserAgentParserMemoryCachedProvider provider = new(new HttpUserAgentParserMemoryCachedProviderOptions());
        _ = provider.Parse("Chrome/17");
        _ = provider.Parse("Chrome/17");
        _ = provider.Parse("Firefox/12");
        listener.RecordObservableInstruments();

        Assert.Equal(new[] { "cache.hit", "cache.miss", "cache.miss", "cache.size" }, listener.InstrumentNames.Order(StringComparer.Ordinal), StringComparer.Ordinal);
    }

    [Fact]
    public void WithMemoryCacheMeterTelemetryPrefix_Throws_WhenInvalid()
    {
        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());

        Assert.Throws<ArgumentException>(() => options.WithMemoryCacheMeterTelemetryPrefix("acme"));
        Assert.Throws<ArgumentException>(() => options.WithMemoryCacheMeterTelemetryPrefix("acme-"));
        Assert.Throws<ArgumentException>(() => options.WithMemoryCacheMeterTelemetryPrefix("acme.."));
    }

    [Fact]
    public void Repeated_Prefix_Registrations_Keep_One_Meter()
    {
        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());

        for (int i = 0; i < 32; i++)
        {
            options.WithMemoryCacheMeterTelemetryPrefix("repeated.");
        }

        Assert.Equal((1, 3), MeterTestListener.CountLiveMeters("repeated.http_user_agent_parser.memorycache"));
    }

    [Fact]
    public void A_Replaced_Own_Meter_Is_Disposed_But_A_Meter_Of_The_Caller_Is_Not()
    {
        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());
        using Meter callerMeter = new("caller.http_user_agent_parser.memorycache");

        options.WithMemoryCacheMeterTelemetryPrefix("first.");
        options.WithMemoryCacheMeterTelemetryPrefix("first."); // the meter in use: kept
        Assert.Equal((1, 3), MeterTestListener.CountLiveMeters("first.http_user_agent_parser.memorycache"));

        options.WithMemoryCacheMeterTelemetryPrefix("second.");
        Assert.Equal((0, 0), MeterTestListener.CountLiveMeters("first.http_user_agent_parser.memorycache"));
        Assert.Equal((1, 3), MeterTestListener.CountLiveMeters("second.http_user_agent_parser.memorycache"));

        options.WithMemoryCacheMeterTelemetry(callerMeter);
        Assert.Equal((0, 0), MeterTestListener.CountLiveMeters("second.http_user_agent_parser.memorycache"));

        options.WithMemoryCacheMeterTelemetry(); // the default meter replaces the meter of the caller
        Assert.Equal((1, 3), MeterTestListener.CountLiveMeters("caller.http_user_agent_parser.memorycache"));
        Assert.Equal((1, 3), MeterTestListener.CountLiveMeters("mycsharp.http_user_agent_parser.memorycache"));
    }

    [Fact]
    public void Changing_Prefixes_Keep_Only_The_Meter_In_Use()
    {
        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());

        for (int i = 0; i < 32; i++)
        {
            options.WithMemoryCacheMeterTelemetryPrefix($"changing{i}.");
        }

        for (int i = 0; i < 31; i++)
        {
            Assert.Equal((0, 0), MeterTestListener.CountLiveMeters($"changing{i}.http_user_agent_parser.memorycache"));
        }

        Assert.Equal((1, 3), MeterTestListener.CountLiveMeters("changing31.http_user_agent_parser.memorycache"));
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

        HttpUserAgentParserMemoryCacheMeters.Enable(first);
        Assert.Throws<InvalidOperationException>(() => HttpUserAgentParserMemoryCacheMeters.Enable(second));
        Assert.Throws<InvalidOperationException>(() => HttpUserAgentParserMemoryCacheMeters.EnableOwnedMeter("throwing.own"));
        Assert.Equal((0, 0), MeterTestListener.CountLiveMeters("throwing.own")); // the new own meter is disposed again

        using MeterTestListener recorder = new("throwing.first");
        HttpUserAgentParserMemoryCacheMeters.CacheHit();
        Assert.Equal(new[] { "cache.hit" }, recorder.InstrumentNames, StringComparer.Ordinal);
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
                    && string.Equals(instrument.Name, "cache.hit", StringComparison.Ordinal))
                {
                    target.EnableMeasurementEvents(instrument);
                }
            },
        };
        listener.SetMeasurementEventCallback<long>((_, value, _, _) => Interlocked.Add(ref recorded, value));
        listener.Start();

        HttpUserAgentParserMemoryCacheMeters.Enable(a);
        using CancellationTokenSource stop = new(TimeSpan.FromMilliseconds(300));
        Task switcher = Task.Run(
            () =>
            {
                for (int i = 0; !stop.IsCancellationRequested; i++)
                {
                    HttpUserAgentParserMemoryCacheMeters.Enable(i % 2 == 0 ? b : a);
                }
            },
            cancellationToken);

        long measurements = 0;
        while (!stop.IsCancellationRequested)
        {
            HttpUserAgentParserMemoryCacheMeters.CacheHit();
            measurements++;
        }

        await switcher;
        Assert.Equal(measurements, Interlocked.Read(ref recorded));
    }

    [Fact]
    public void A_Meter_Of_The_Core_Package_Is_Rejected_Also_After_A_Switch()
    {
        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());
        using Meter shared = new("test.memorycache.shared");
        using Meter coreMeter = new("test.memorycache.core");
        using Meter memoryCacheMeter = new("test.memorycache.own");

        options.WithMeterTelemetry(shared);
        Assert.Throws<ArgumentException>(() => options.WithMemoryCacheMeterTelemetry(shared));

        // The core instruments remain on the shared meter, so it stays reserved after the core package moved on.
        options.WithMeterTelemetry(coreMeter);
        Assert.Throws<ArgumentException>(() => options.WithMemoryCacheMeterTelemetry(shared));

        options.WithMemoryCacheMeterTelemetry(memoryCacheMeter);
        Assert.Throws<ArgumentException>(() => options.WithMeterTelemetry(memoryCacheMeter));
    }

    [Fact]
    public void A_Nested_Registration_While_The_Core_Instruments_Are_Published_Is_Rejected()
    {
        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());
        using Meter shared = new("test.memorycache.nested");
        Exception? nested = null;
        using MeterListener listener = new()
        {
            InstrumentPublished = (instrument, _) =>
            {
                if (ReferenceEquals(instrument.Meter, shared) && string.Equals(instrument.Name, "parse.requests", StringComparison.Ordinal))
                {
                    nested = Record.Exception(() => options.WithMemoryCacheMeterTelemetry(shared));
                }
            },
        };
        listener.Start();

        options.WithMeterTelemetry(shared);

        Assert.IsType<ArgumentException>(nested);
    }

    [Fact]
    public void A_Cache_Size_Gauge_Whose_Publication_Failed_Is_Not_Created_Again()
    {
        using Meter meter = new("retry.memorycache");
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

        Assert.Throws<InvalidOperationException>(() => HttpUserAgentParserMemoryCacheMeters.Enable(meter));
        HttpUserAgentParserMemoryCacheMeters.Enable(meter); // the retry completes the instruments

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

    [Fact]
    public async Task Mutual_Registrations_From_Listener_Callbacks_Do_Not_Block()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());
        using Meter coreMeter = new("mutual.core");
        using Meter memoryCacheMeter = new("mutual.memorycache");
        using Meter nestedCoreMeter = new("mutual.core.nested");
        using Meter nestedMemoryCacheMeter = new("mutual.memorycache.nested");
        using Barrier bothInCallbacks = new(2);
        using MeterListener listener = new()
        {
            InstrumentPublished = (instrument, _) =>
            {
                // Each callback waits until the other registration is in its callback as well, then registers the other package.
                if (ReferenceEquals(instrument.Meter, coreMeter) && string.Equals(instrument.Name, "parse.requests", StringComparison.Ordinal))
                {
                    bothInCallbacks.SignalAndWait(TimeSpan.FromSeconds(10), cancellationToken);
                    options.WithMemoryCacheMeterTelemetry(nestedMemoryCacheMeter);
                }
                else if (ReferenceEquals(instrument.Meter, memoryCacheMeter) && string.Equals(instrument.Name, "cache.hit", StringComparison.Ordinal))
                {
                    bothInCallbacks.SignalAndWait(TimeSpan.FromSeconds(10), cancellationToken);
                    options.WithMeterTelemetry(nestedCoreMeter);
                }
            },
        };
        listener.Start();

        Task core = Task.Run(() => options.WithMeterTelemetry(coreMeter), cancellationToken);
        Task memoryCache = Task.Run(() => options.WithMemoryCacheMeterTelemetry(memoryCacheMeter), cancellationToken);

        await Task.WhenAll(core, memoryCache).WaitAsync(TimeSpan.FromSeconds(20), cancellationToken);
    }
}
