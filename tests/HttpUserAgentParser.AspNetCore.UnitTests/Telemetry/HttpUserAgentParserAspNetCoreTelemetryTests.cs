// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.UnitTests.Telemetry;

public class HttpUserAgentParserAspNetCoreTelemetryTests
{
    [Fact]
    public void EventCounters_DoNotThrow_WhenEnabled()
    {
        using EventCounterTestListener listener = new("MyCSharp.HttpUserAgentParser");

        DefaultHttpContext ctx = new();

        // present
        ctx.Request.Headers.UserAgent = "UA";
        Assert.NotNull(ctx.GetUserAgentString());

        // missing
        ctx.Request.Headers.Remove("User-Agent");
        Assert.Null(ctx.GetUserAgentString());

        Assert.True(listener.WaitForCounters(TimeSpan.FromSeconds(2)));
    }
}
