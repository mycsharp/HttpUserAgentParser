// Copyright © https://myCSharp.de - all rights reserved

using Xunit;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.UnitTests.Telemetry;

public class HttpUserAgentParserMemoryCacheTelemetryTests
{
    [Fact]
    public void EventCounters_DoNotThrow_WhenEnabled()
    {
        using EventCounterTestListener listener = new("MyCSharp.HttpUserAgentParser");

        HttpUserAgentParserMemoryCachedProvider provider = new(new HttpUserAgentParserMemoryCachedProviderOptions());

        const string ua1 = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36";
        const string ua2 = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Firefox/88.0";

        // First call ensures the EventSource gets created (listener enables right after creation).
        _ = provider.Parse(ua1);
        Assert.True(listener.WaitUntilEnabled(TimeSpan.FromSeconds(2)));

        // Now exercise telemetry-enabled paths: miss (ua2), hit (ua1)
        _ = provider.Parse(ua2); // miss under enabled
        _ = provider.Parse(ua1); // hit under enabled

        Assert.True(listener.WaitForCounters(TimeSpan.FromSeconds(2)));
    }
}
