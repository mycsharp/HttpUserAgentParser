// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using MyCSharp.HttpUserAgentParser.Providers;
using MyCSharp.HttpUserAgentParser.Telemetry;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests.Telemetry;

public class HttpUserAgentParserMetersTelemetryTests
{
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

        Assert.Contains("parse.requests", listener.InstrumentNames);
        Assert.Contains("parse.duration", listener.InstrumentNames);
        Assert.Contains("cache.hit", listener.InstrumentNames);
        Assert.Contains("cache.miss", listener.InstrumentNames);
        Assert.Contains("cache.size", listener.InstrumentNames);

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

        Assert.Contains("parse.requests", listener.InstrumentNames);
        Assert.Contains("parse.duration", listener.InstrumentNames);
        Assert.Contains("cache.hit", listener.InstrumentNames);
        Assert.Contains("cache.miss", listener.InstrumentNames);
        Assert.Contains("cache.size", listener.InstrumentNames);
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

        Assert.Contains("parse.requests", listener.InstrumentNames);
        Assert.Contains("parse.duration", listener.InstrumentNames);
    }

    [Fact]
    public void WithMeterTelemetryPrefix_Throws_WhenInvalid()
    {
        HttpUserAgentParserTelemetry.ResetForTests();

        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());

        Assert.Throws<ArgumentException>(() => options.WithMeterTelemetryPrefix("acme"));
        Assert.Throws<ArgumentException>(() => options.WithMeterTelemetryPrefix("acme-"));
        Assert.Throws<ArgumentException>(() => options.WithMeterTelemetryPrefix("acme.."));
    }
}
