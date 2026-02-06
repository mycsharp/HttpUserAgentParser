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
        HttpUserAgentParserAspNetCoreTelemetry.ResetForTests();

        using MeterTestListener listener = new("mycsharp.http_user_agent_parser.aspnetcore");

        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection())
            .WithAspNetCoreMeterTelemetry();

        DefaultHttpContext ctx = new();

        // present
        ctx.Request.Headers.UserAgent = "UA";
        Assert.NotNull(ctx.GetUserAgentString());

        // missing
        ctx.Request.Headers.Remove("User-Agent");
        Assert.Null(ctx.GetUserAgentString());

        Assert.Contains("user_agent.present", listener.InstrumentNames);
        Assert.Contains("user_agent.missing", listener.InstrumentNames);
    }

    [Fact]
    public void Meters_Emit_WhenEnabled_WithCustomPrefix()
    {
        HttpUserAgentParserAspNetCoreTelemetry.ResetForTests();

        const string prefix = "acme.";
        using MeterTestListener listener = new("acme.http_user_agent_parser.aspnetcore");

        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection())
            .WithAspNetCoreMeterTelemetryPrefix(prefix);

        DefaultHttpContext ctx = new();

        // present
        ctx.Request.Headers.UserAgent = "UA";
        Assert.NotNull(ctx.GetUserAgentString());

        // missing
        ctx.Request.Headers.Remove("User-Agent");
        Assert.Null(ctx.GetUserAgentString());

        Assert.Contains("user_agent.present", listener.InstrumentNames);
        Assert.Contains("user_agent.missing", listener.InstrumentNames);
    }

    [Fact]
    public void Meters_Emit_WhenEnabled_WithEmptyPrefix()
    {
        HttpUserAgentParserAspNetCoreTelemetry.ResetForTests();

        using MeterTestListener listener = new("http_user_agent_parser.aspnetcore");

        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection())
            .WithAspNetCoreMeterTelemetryPrefix(string.Empty);

        DefaultHttpContext ctx = new();

        // present
        ctx.Request.Headers.UserAgent = "UA";
        Assert.NotNull(ctx.GetUserAgentString());

        Assert.Contains("user_agent.present", listener.InstrumentNames);
    }

    [Fact]
    public void WithAspNetCoreMeterTelemetryPrefix_Throws_WhenInvalid()
    {
        HttpUserAgentParserAspNetCoreTelemetry.ResetForTests();

        HttpUserAgentParserDependencyInjectionOptions options = new(new ServiceCollection());

        Assert.Throws<ArgumentException>(() => options.WithAspNetCoreMeterTelemetryPrefix("acme"));
        Assert.Throws<ArgumentException>(() => options.WithAspNetCoreMeterTelemetryPrefix("acme-"));
        Assert.Throws<ArgumentException>(() => options.WithAspNetCoreMeterTelemetryPrefix("acme.."));
    }
}
