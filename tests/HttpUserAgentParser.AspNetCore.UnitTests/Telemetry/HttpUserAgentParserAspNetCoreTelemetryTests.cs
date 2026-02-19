// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.AspNetCore.DependencyInjection;
using MyCSharp.HttpUserAgentParser.AspNetCore.Telemetry;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.UnitTests.Telemetry;

public class HttpUserAgentParserAspNetCoreTelemetryTests
{
    [Fact]
    public void EventCounters_DoNotThrow_WhenEnabled()
    {
        using EventCounterTestListener listener = new(HttpUserAgentParserAspNetCoreEventSource.EventSourceName);

        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection())
            .WithAspNetCoreTelemetry();

        DefaultHttpContext ctx = new();

        // First call ensures the EventSource gets created (listener enables right after creation).
        ctx.Request.Headers.UserAgent = "UA";
        Assert.NotNull(ctx.GetUserAgentString());
        Assert.True(listener.WaitUntilEnabled(TimeSpan.FromSeconds(2)));

        // Now exercise telemetry-enabled paths.
        ctx.Request.Headers.UserAgent = "UA";
        Assert.NotNull(ctx.GetUserAgentString());

        ctx.Request.Headers.Remove("User-Agent");
        Assert.Null(ctx.GetUserAgentString());

        Assert.True(listener.WaitForCounters(TimeSpan.FromSeconds(2)));
    }
}
