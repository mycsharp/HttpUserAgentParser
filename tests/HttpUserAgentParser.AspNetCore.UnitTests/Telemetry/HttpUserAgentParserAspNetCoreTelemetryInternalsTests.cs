// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Metrics;
using MyCSharp.HttpUserAgentParser.AspNetCore.Telemetry;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.UnitTests.Telemetry;

/// <summary>
/// Covers the telemetry building blocks of the AspNetCore package directly.
/// </summary>
[Collection(TelemetryCollection.Name)]
public sealed class HttpUserAgentParserAspNetCoreTelemetryInternalsTests : IDisposable
{
    public HttpUserAgentParserAspNetCoreTelemetryInternalsTests() => HttpUserAgentParserAspNetCoreTelemetry.ResetForTests();

    public void Dispose() => HttpUserAgentParserAspNetCoreTelemetry.ResetForTests();

    [Fact]
    public void Meters_Ignore_Measurements_Before_Enable()
    {
        using MeterTestListener listener = new(HttpUserAgentParserAspNetCoreMeters.MeterName);

        HttpUserAgentParserAspNetCoreMeters.UserAgentPresent();
        HttpUserAgentParserAspNetCoreMeters.UserAgentMissing();

        Assert.Empty(listener.InstrumentNames);
    }

    [Fact]
    public void Meters_Use_The_Last_Registered_Meter()
    {
        using Meter first = new("test.aspnetcore.first");
        using Meter second = new("test.aspnetcore.second");
        using MeterTestListener firstListener = new("test.aspnetcore.first");
        using MeterTestListener secondListener = new("test.aspnetcore.second");

        HttpUserAgentParserAspNetCoreMeters.Enable(first);
        HttpUserAgentParserAspNetCoreMeters.Enable(first); // the meter in use: no effect
        HttpUserAgentParserAspNetCoreMeters.UserAgentPresent();
        HttpUserAgentParserAspNetCoreMeters.Enable(second);
        HttpUserAgentParserAspNetCoreMeters.UserAgentMissing();
        HttpUserAgentParserAspNetCoreMeters.Enable(); // default meter
        HttpUserAgentParserAspNetCoreMeters.Enable(); // default meter in use: no effect

        Assert.Equal(new[] { "user_agent.present" }, firstListener.InstrumentNames, StringComparer.Ordinal);
        Assert.Equal(new[] { "user_agent.missing" }, secondListener.InstrumentNames, StringComparer.Ordinal);
    }

    [Fact]
    public void EventSource_Ignores_Calls_Without_Listener()
    {
        HttpUserAgentParserAspNetCoreEventSource source = HttpUserAgentParserAspNetCoreEventSource.Log;
        Assert.False(source.IsEnabled());

        source.UserAgentPresent();
        source.UserAgentMissing();

        Assert.False(source.IsEnabled());
    }

    [Fact]
    public void EventSource_Can_Be_Disposed()
    {
        HttpUserAgentParserAspNetCoreEventSource source = new();
        Assert.Equal(HttpUserAgentParserAspNetCoreEventSource.EventSourceName, source.Name);

        source.Dispose();

        Assert.False(source.IsEnabled());
    }
}
