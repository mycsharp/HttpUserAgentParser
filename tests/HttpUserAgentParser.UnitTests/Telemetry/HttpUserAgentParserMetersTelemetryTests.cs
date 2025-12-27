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
#if DEBUG
        HttpUserAgentParserTelemetry.ResetForTests();
#endif

        using MeterTestListener listener = new(MyCSharp.HttpUserAgentParser.Telemetry.HttpUserAgentParserMeters.MeterName);

        const string ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36";
        _ = HttpUserAgentInformation.Parse(ua);

        Assert.Empty(listener.InstrumentNames);
    }

    [Fact]
    public void Meters_Emit_WhenEnabled()
    {
#if DEBUG
        HttpUserAgentParserTelemetry.ResetForTests();
#endif

        using MeterTestListener listener = new(MyCSharp.HttpUserAgentParser.Telemetry.HttpUserAgentParserMeters.MeterName);

        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection())
            .WithMeterTelemetry();

        const string ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36";

        _ = HttpUserAgentInformation.Parse(ua);

        HttpUserAgentParserCachedProvider provider = new();
        _ = provider.Parse(ua); // miss
        _ = provider.Parse(ua); // hit

        listener.RecordObservableInstruments();

        Assert.Contains("parse-requests", listener.InstrumentNames);
        Assert.Contains("parse-duration", listener.InstrumentNames);
        Assert.Contains("cache-concurrentdictionary-hit", listener.InstrumentNames);
        Assert.Contains("cache-concurrentdictionary-miss", listener.InstrumentNames);
        Assert.Contains("cache-concurrentdictionary-size", listener.InstrumentNames);
    }
}
