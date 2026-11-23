// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.AspNetCore.DependencyInjection;
using MyCSharp.HttpUserAgentParser.AspNetCore.Telemetry;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using MyCSharp.HttpUserAgentParser.TestHelpers;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.UnitTests.Telemetry;

[Collection(TelemetryCollection.Name)]
public sealed class HttpUserAgentParserAspNetCoreMetersTelemetryTests : IDisposable
{
    public HttpUserAgentParserAspNetCoreMetersTelemetryTests() => HttpUserAgentParserAspNetCoreTelemetry.ResetForTests();

    public void Dispose() => HttpUserAgentParserAspNetCoreTelemetry.ResetForTests();

    [Theory]
    [InlineData(null, "mycsharp.http_user_agent_parser.aspnetcore")]
    [InlineData("acme.", "acme.http_user_agent_parser.aspnetcore")]
    [InlineData("", "http_user_agent_parser.aspnetcore")]
    public void Configured_Meter_Emits_Expected_Measurements(string? prefix, string meterName)
    {
        using MeterTestListener listener = new(meterName);
        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());
        if (prefix is null)
        {
            options.WithAspNetCoreMeterTelemetry();
        }
        else
        {
            options.WithAspNetCoreMeterTelemetryPrefix(prefix);
        }

        DefaultHttpContext context = new();
        Assert.Null(context.GetUserAgentString());
        context.Request.Headers.UserAgent = "Chrome/17";
        Assert.Equal("Chrome/17", context.GetUserAgentString());

        Assert.Equal(new[] { "user_agent.missing", "user_agent.present" }, listener.InstrumentNames.Order(StringComparer.Ordinal), StringComparer.Ordinal);
    }

    [Fact]
    public void WithAspNetCoreMeterTelemetryPrefix_Throws_WhenInvalid()
    {
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

        Assert.Equal((1, 2), MeterTestListener.CountLiveMeters("repeated.http_user_agent_parser.aspnetcore"));
    }

    [Fact]
    public void A_Replaced_Own_Meter_Is_Disposed_But_A_Meter_Of_The_Caller_Is_Not()
    {
        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());
        using Meter callerMeter = new("caller.http_user_agent_parser.aspnetcore");

        options.WithAspNetCoreMeterTelemetryPrefix("first.");
        options.WithAspNetCoreMeterTelemetryPrefix("first."); // the meter in use: kept
        Assert.Equal((1, 2), MeterTestListener.CountLiveMeters("first.http_user_agent_parser.aspnetcore"));

        options.WithAspNetCoreMeterTelemetryPrefix("second.");
        Assert.Equal((0, 0), MeterTestListener.CountLiveMeters("first.http_user_agent_parser.aspnetcore"));
        Assert.Equal((1, 2), MeterTestListener.CountLiveMeters("second.http_user_agent_parser.aspnetcore"));

        options.WithAspNetCoreMeterTelemetry(callerMeter);
        Assert.Equal((0, 0), MeterTestListener.CountLiveMeters("second.http_user_agent_parser.aspnetcore"));

        options.WithAspNetCoreMeterTelemetry(); // the default meter replaces the meter of the caller
        Assert.Equal((1, 2), MeterTestListener.CountLiveMeters("caller.http_user_agent_parser.aspnetcore"));
        Assert.Equal((1, 2), MeterTestListener.CountLiveMeters("mycsharp.http_user_agent_parser.aspnetcore"));
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
            Assert.Equal((0, 0), MeterTestListener.CountLiveMeters($"changing{i}.http_user_agent_parser.aspnetcore"));
        }

        Assert.Equal((1, 2), MeterTestListener.CountLiveMeters("changing31.http_user_agent_parser.aspnetcore"));
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
        Assert.Equal((0, 0), MeterTestListener.CountLiveMeters("throwing.own")); // the new own meter is disposed again

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
}
