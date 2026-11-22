// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.AspNetCore.DependencyInjection;
using MyCSharp.HttpUserAgentParser.AspNetCore.Telemetry;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.UnitTests.Telemetry;

[Collection(TelemetryCollection.Name)]
public sealed class HttpUserAgentParserAspNetCoreMetersTelemetryTests : IDisposable
{
    public HttpUserAgentParserAspNetCoreMetersTelemetryTests() => HttpUserAgentParserAspNetCoreTelemetry.ResetForTests();

    public void Dispose() => HttpUserAgentParserAspNetCoreTelemetry.ResetForTests();

    [Fact]
    public void Meters_Emit_WhenEnabled()
    {
        HttpUserAgentParserAspNetCoreTelemetry.ResetForTests();

        using MeterTestListener listener = new("mycsharp.http_user_agent_parser.aspnetcore");

        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection())
            .WithAspNetCoreMeterTelemetry();

        DefaultHttpContext ctx = new();

        // present
        ctx.Request.Headers.UserAgent = "UA";
        Assert.NotNull(ctx.GetUserAgentString());

        // missing
        ctx.Request.Headers.Remove("User-Agent");
        Assert.Null(ctx.GetUserAgentString());

        Assert.Contains("user_agent.present", listener.InstrumentNames, StringComparer.Ordinal);
        Assert.Contains("user_agent.missing", listener.InstrumentNames, StringComparer.Ordinal);
    }

    [Fact]
    public void Meters_Emit_WhenEnabled_WithCustomPrefix()
    {
        HttpUserAgentParserAspNetCoreTelemetry.ResetForTests();

        const string prefix = "acme.";
        using MeterTestListener listener = new("acme.http_user_agent_parser.aspnetcore");

        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection())
            .WithAspNetCoreMeterTelemetryPrefix(prefix);

        DefaultHttpContext ctx = new();

        // present
        ctx.Request.Headers.UserAgent = "UA";
        Assert.NotNull(ctx.GetUserAgentString());

        // missing
        ctx.Request.Headers.Remove("User-Agent");
        Assert.Null(ctx.GetUserAgentString());

        Assert.Contains("user_agent.present", listener.InstrumentNames, StringComparer.Ordinal);
        Assert.Contains("user_agent.missing", listener.InstrumentNames, StringComparer.Ordinal);
    }

    [Fact]
    public void Meters_Emit_WhenEnabled_WithEmptyPrefix()
    {
        HttpUserAgentParserAspNetCoreTelemetry.ResetForTests();

        using MeterTestListener listener = new("http_user_agent_parser.aspnetcore");

        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection())
            .WithAspNetCoreMeterTelemetryPrefix(string.Empty);

        DefaultHttpContext ctx = new();

        // present
        ctx.Request.Headers.UserAgent = "UA";
        Assert.NotNull(ctx.GetUserAgentString());

        Assert.Contains("user_agent.present", listener.InstrumentNames, StringComparer.Ordinal);
    }

    [Fact]
    public void WithAspNetCoreMeterTelemetryPrefix_Throws_WhenInvalid()
    {
        HttpUserAgentParserAspNetCoreTelemetry.ResetForTests();

        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());

        Assert.Throws<ArgumentException>(() => options.WithAspNetCoreMeterTelemetryPrefix("acme"));
        Assert.Throws<ArgumentException>(() => options.WithAspNetCoreMeterTelemetryPrefix("acme-"));
        Assert.Throws<ArgumentException>(() => options.WithAspNetCoreMeterTelemetryPrefix("acme.."));
    }

    [Fact]
    public void Repeated_Prefix_Registrations_Keep_One_Meter()
    {
        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());

        for (int i = 0; i < 32; i++)
        {
            options.WithAspNetCoreMeterTelemetryPrefix("repeated.");
        }

        Assert.Equal((1, 2), CountLiveMeters("repeated.http_user_agent_parser.aspnetcore"));
    }

    [Fact]
    public void A_Replaced_Own_Meter_Is_Disposed_But_A_Meter_Of_The_Caller_Is_Not()
    {
        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());
        using Meter callerMeter = new("caller.http_user_agent_parser.aspnetcore");

        options.WithAspNetCoreMeterTelemetryPrefix("first.");
        options.WithAspNetCoreMeterTelemetryPrefix("first."); // the meter in use: kept
        Assert.Equal((1, 2), CountLiveMeters("first.http_user_agent_parser.aspnetcore"));

        options.WithAspNetCoreMeterTelemetryPrefix("second.");
        Assert.Equal((0, 0), CountLiveMeters("first.http_user_agent_parser.aspnetcore"));
        Assert.Equal((1, 2), CountLiveMeters("second.http_user_agent_parser.aspnetcore"));

        options.WithAspNetCoreMeterTelemetry(callerMeter);
        Assert.Equal((0, 0), CountLiveMeters("second.http_user_agent_parser.aspnetcore"));

        options.WithAspNetCoreMeterTelemetry(); // the default meter replaces the meter of the caller
        Assert.Equal((1, 2), CountLiveMeters("caller.http_user_agent_parser.aspnetcore"));
        Assert.Equal((1, 2), CountLiveMeters("mycsharp.http_user_agent_parser.aspnetcore"));
    }

    [Fact]
    public void Changing_Prefixes_Keep_Only_The_Meter_In_Use()
    {
        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());

        for (int i = 0; i < 32; i++)
        {
            options.WithAspNetCoreMeterTelemetryPrefix($"changing{i}.");
        }

        for (int i = 0; i < 31; i++)
        {
            Assert.Equal((0, 0), CountLiveMeters($"changing{i}.http_user_agent_parser.aspnetcore"));
        }

        Assert.Equal((1, 2), CountLiveMeters("changing31.http_user_agent_parser.aspnetcore"));
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

        HttpUserAgentParserAspNetCoreMeters.Enable(first);
        Assert.Throws<InvalidOperationException>(() => HttpUserAgentParserAspNetCoreMeters.Enable(second));
        Assert.Throws<InvalidOperationException>(() => HttpUserAgentParserAspNetCoreMeters.EnableOwnedMeter("throwing.own"));
        Assert.Equal((0, 0), CountLiveMeters("throwing.own")); // the new own meter is disposed again

        using MeterTestListener recorder = new("throwing.first");
        HttpUserAgentParserAspNetCoreMeters.UserAgentPresent();
        Assert.Equal(new[] { "user_agent.present" }, recorder.InstrumentNames, StringComparer.Ordinal);
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
                    && string.Equals(instrument.Name, "user_agent.present", StringComparison.Ordinal))
                {
                    target.EnableMeasurementEvents(instrument);
                }
            },
        };
        listener.SetMeasurementEventCallback<long>((_, value, _, _) => Interlocked.Add(ref recorded, value));
        listener.Start();

        HttpUserAgentParserAspNetCoreMeters.Enable(a);
        using CancellationTokenSource stop = new(TimeSpan.FromMilliseconds(300));
        Task switcher = Task.Run(
            () =>
            {
                for (int i = 0; !stop.IsCancellationRequested; i++)
                {
                    HttpUserAgentParserAspNetCoreMeters.Enable(i % 2 == 0 ? b : a);
                }
            },
            cancellationToken);

        long measurements = 0;
        while (!stop.IsCancellationRequested)
        {
            HttpUserAgentParserAspNetCoreMeters.UserAgentPresent();
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
}
