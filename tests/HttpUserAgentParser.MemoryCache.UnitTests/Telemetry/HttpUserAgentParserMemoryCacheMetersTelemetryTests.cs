// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Metrics;
using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using MyCSharp.HttpUserAgentParser.MemoryCache.DependencyInjection;
using MyCSharp.HttpUserAgentParser.MemoryCache.Telemetry;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.UnitTests.Telemetry;

[Collection(TelemetryCollection.Name)]
public sealed class HttpUserAgentParserMemoryCacheMetersTelemetryTests : IDisposable
{
    public HttpUserAgentParserMemoryCacheMetersTelemetryTests() => HttpUserAgentParserMemoryCacheTelemetry.ResetForTests();

    public void Dispose() => HttpUserAgentParserMemoryCacheTelemetry.ResetForTests();

    [Fact]
    public void Meters_Emit_WhenEnabled()
    {
        HttpUserAgentParserMemoryCacheTelemetry.ResetForTests();

        using MeterTestListener listener = new("mycsharp.http_user_agent_parser.memorycache");

        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection())
            .WithMemoryCacheMeterTelemetry();

        HttpUserAgentParserMemoryCachedProvider provider = new(new HttpUserAgentParserMemoryCachedProviderOptions());

        const string ua1 = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36";
        const string ua2 = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Firefox/88.0";

        _ = provider.Parse(ua1); // miss
        _ = provider.Parse(ua1); // hit
        _ = provider.Parse(ua2); // miss

        listener.RecordObservableInstruments();

        Assert.Contains("cache.hit", listener.InstrumentNames, StringComparer.Ordinal);
        Assert.Contains("cache.miss", listener.InstrumentNames, StringComparer.Ordinal);
        Assert.Contains("cache.size", listener.InstrumentNames, StringComparer.Ordinal);
    }

    [Fact]
    public void Meters_Emit_WhenEnabled_WithCustomPrefix()
    {
        HttpUserAgentParserMemoryCacheTelemetry.ResetForTests();

        const string prefix = "acme.";
        using MeterTestListener listener = new("acme.http_user_agent_parser.memorycache");

        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection())
            .WithMemoryCacheMeterTelemetryPrefix(prefix);

        HttpUserAgentParserMemoryCachedProvider provider = new(new HttpUserAgentParserMemoryCachedProviderOptions());

        const string ua1 = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36";
        const string ua2 = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Firefox/88.0";

        _ = provider.Parse(ua1); // miss
        _ = provider.Parse(ua1); // hit
        _ = provider.Parse(ua2); // miss

        listener.RecordObservableInstruments();

        Assert.Contains("cache.hit", listener.InstrumentNames, StringComparer.Ordinal);
        Assert.Contains("cache.miss", listener.InstrumentNames, StringComparer.Ordinal);
        Assert.Contains("cache.size", listener.InstrumentNames, StringComparer.Ordinal);
    }

    [Fact]
    public void Meters_Emit_WhenEnabled_WithEmptyPrefix()
    {
        HttpUserAgentParserMemoryCacheTelemetry.ResetForTests();

        using MeterTestListener listener = new("http_user_agent_parser.memorycache");

        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection())
            .WithMemoryCacheMeterTelemetryPrefix(string.Empty);

        HttpUserAgentParserMemoryCachedProvider provider = new(new HttpUserAgentParserMemoryCachedProviderOptions());

        const string ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36";

        _ = provider.Parse(ua); // miss

        listener.RecordObservableInstruments();

        Assert.Contains("cache.miss", listener.InstrumentNames, StringComparer.Ordinal);
        Assert.Contains("cache.size", listener.InstrumentNames, StringComparer.Ordinal);
    }

    [Fact]
    public void WithMemoryCacheMeterTelemetryPrefix_Throws_WhenInvalid()
    {
        HttpUserAgentParserMemoryCacheTelemetry.ResetForTests();

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

        Assert.Equal((1, 3), CountLiveMeters("repeated.http_user_agent_parser.memorycache"));
    }

    [Fact]
    public void A_Replaced_Own_Meter_Is_Disposed_But_A_Meter_Of_The_Caller_Is_Not()
    {
        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());
        using Meter callerMeter = new("caller.http_user_agent_parser.memorycache");

        options.WithMemoryCacheMeterTelemetryPrefix("first.");
        options.WithMemoryCacheMeterTelemetryPrefix("first."); // the meter in use: kept
        Assert.Equal((1, 3), CountLiveMeters("first.http_user_agent_parser.memorycache"));

        options.WithMemoryCacheMeterTelemetryPrefix("second.");
        Assert.Equal((0, 0), CountLiveMeters("first.http_user_agent_parser.memorycache"));
        Assert.Equal((1, 3), CountLiveMeters("second.http_user_agent_parser.memorycache"));

        options.WithMemoryCacheMeterTelemetry(callerMeter);
        Assert.Equal((0, 0), CountLiveMeters("second.http_user_agent_parser.memorycache"));

        options.WithMemoryCacheMeterTelemetry(); // the default meter replaces the meter of the caller
        Assert.Equal((1, 3), CountLiveMeters("caller.http_user_agent_parser.memorycache"));
        Assert.Equal((1, 3), CountLiveMeters("mycsharp.http_user_agent_parser.memorycache"));
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
            Assert.Equal((0, 0), CountLiveMeters($"changing{i}.http_user_agent_parser.memorycache"));
        }

        Assert.Equal((1, 3), CountLiveMeters("changing31.http_user_agent_parser.memorycache"));
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
        Assert.Equal((0, 0), CountLiveMeters("throwing.own")); // the new own meter is disposed again

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
}
