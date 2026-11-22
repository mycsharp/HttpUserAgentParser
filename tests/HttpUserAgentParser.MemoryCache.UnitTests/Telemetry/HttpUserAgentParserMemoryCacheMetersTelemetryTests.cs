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

        Assert.Equal(1, CountLiveMeters("repeated.http_user_agent_parser.memorycache"));
    }

    [Fact]
    public void A_Replaced_Own_Meter_Is_Disposed_But_A_Meter_Of_The_Caller_Is_Not()
    {
        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());
        using Meter callerMeter = new("caller.http_user_agent_parser.memorycache");

        options.WithMemoryCacheMeterTelemetryPrefix("first.");
        options.WithMemoryCacheMeterTelemetryPrefix("second.");
        Assert.Equal(0, CountLiveMeters("first.http_user_agent_parser.memorycache"));
        Assert.Equal(1, CountLiveMeters("second.http_user_agent_parser.memorycache"));

        options.WithMemoryCacheMeterTelemetry(callerMeter);
        Assert.Equal(0, CountLiveMeters("second.http_user_agent_parser.memorycache"));

        options.WithMemoryCacheMeterTelemetry(); // the default meter replaces the meter of the caller
        Assert.Equal(1, CountLiveMeters("caller.http_user_agent_parser.memorycache"));
        Assert.Equal(1, CountLiveMeters("mycsharp.http_user_agent_parser.memorycache"));
    }

    /// <summary>
    /// Counts the meters with instruments that a new listener sees, i.e. the meters that are not disposed.
    /// </summary>
    private static int CountLiveMeters(string meterName)
    {
        HashSet<Meter> meters = [];
        using MeterListener listener = new()
        {
            InstrumentPublished = (instrument, _) =>
            {
                if (string.Equals(instrument.Meter.Name, meterName, StringComparison.Ordinal))
                {
                    meters.Add(instrument.Meter);
                }
            },
        };
        listener.Start();
        return meters.Count;
    }
}
