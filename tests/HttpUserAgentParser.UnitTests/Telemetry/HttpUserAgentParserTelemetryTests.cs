// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using MyCSharp.HttpUserAgentParser.Providers;
using MyCSharp.HttpUserAgentParser.Telemetry;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests.Telemetry;

public class HttpUserAgentParserTelemetryTests
{
    [Fact]
    public void EventCounters_DoNotThrow_WhenEnabled()
    {
        using EventCounterTestListener listener = new(HttpUserAgentParserEventSource.EventSourceName);

        // Opt-in telemetry so production default stays overhead-free.
        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection())
            .WithTelemetry();

        const string ua = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36";

        // Core parser
        _ = HttpUserAgentInformation.Parse(ua);

        // ConcurrentDictionary cached provider
        HttpUserAgentParserCachedProvider provider = new();
        _ = provider.Parse(ua); // miss
        _ = provider.Parse(ua); // hit

        Assert.True(listener.WaitForCounters(TimeSpan.FromSeconds(2)));
    }
}
