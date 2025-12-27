// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.AspNetCore.DependencyInjection;
using MyCSharp.HttpUserAgentParser.AspNetCore.Telemetry;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.UnitTests.Telemetry;

public class HttpUserAgentParserAspNetCoreMetersTelemetryTests
{
    [Fact]
    public void Meters_Emit_WhenEnabled()
    {
        using MeterTestListener listener = new("MyCSharp.HttpUserAgentParser.AspNetCore");

        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection())
            .WithAspNetCoreMeterTelemetry();

        DefaultHttpContext ctx = new();

        // present
        ctx.Request.Headers.UserAgent = "UA";
        Assert.NotNull(ctx.GetUserAgentString());

        // missing
        ctx.Request.Headers.Remove("User-Agent");
        Assert.Null(ctx.GetUserAgentString());

        Assert.Contains("useragent-present", listener.InstrumentNames);
        Assert.Contains("useragent-missing", listener.InstrumentNames);
    }
}
