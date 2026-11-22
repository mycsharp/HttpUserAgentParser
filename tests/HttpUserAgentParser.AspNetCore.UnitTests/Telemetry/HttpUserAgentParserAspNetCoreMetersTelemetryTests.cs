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
    public void Own_Meters_Are_Reused_Per_Name_And_Meters_Of_The_Caller_Stay_Alive()
    {
        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());
        using Meter callerMeter = new("caller.http_user_agent_parser.aspnetcore");

        options.WithAspNetCoreMeterTelemetryPrefix("first.");
        options.WithAspNetCoreMeterTelemetryPrefix("second.");
        options.WithAspNetCoreMeterTelemetryPrefix("first."); // reuses the meter and the instruments of the first registration
        options.WithAspNetCoreMeterTelemetry(callerMeter);
        options.WithAspNetCoreMeterTelemetry();

        Assert.Equal((1, 2), CountLiveMeters("first.http_user_agent_parser.aspnetcore"));
        Assert.Equal((1, 2), CountLiveMeters("second.http_user_agent_parser.aspnetcore"));
        Assert.Equal((1, 2), CountLiveMeters("caller.http_user_agent_parser.aspnetcore"));
        Assert.Equal((1, 2), CountLiveMeters("mycsharp.http_user_agent_parser.aspnetcore"));
    }

    [Fact]
    public async Task No_Measurement_Is_Lost_While_Own_Meters_Are_Switched()
    {
        CancellationToken cancellationToken = TestContext.Current.CancellationToken;
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

        HttpUserAgentParserAspNetCoreMeters.EnableOwnedMeter("switch.a");
        using CancellationTokenSource stop = new(TimeSpan.FromMilliseconds(300));
        Task switcher = Task.Run(
            () =>
            {
                for (int i = 0; !stop.IsCancellationRequested; i++)
                {
                    HttpUserAgentParserAspNetCoreMeters.EnableOwnedMeter(i % 2 == 0 ? "switch.b" : "switch.a");
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
