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
    public void Meters_Are_Enabled_Only_Once()
    {
        using Meter first = new("test.aspnetcore.first");
        using Meter second = new("test.aspnetcore.second");
        using MeterTestListener listener = new("test.aspnetcore.first");

        HttpUserAgentParserAspNetCoreMeters.Enable(first);
        HttpUserAgentParserAspNetCoreMeters.Enable(second);
        HttpUserAgentParserAspNetCoreMeters.UserAgentPresent();
        HttpUserAgentParserAspNetCoreMeters.UserAgentMissing();

        Assert.Equal(new[] { "user_agent.missing", "user_agent.present" }, listener.InstrumentNames.Order(StringComparer.Ordinal), StringComparer.Ordinal);
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
