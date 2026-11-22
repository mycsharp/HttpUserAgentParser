// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.AspNetCore.DependencyInjection;
using MyCSharp.HttpUserAgentParser.AspNetCore.Telemetry;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.UnitTests.Telemetry;

/// <summary>
/// Covers header present / missing with telemetry disabled and enabled explicitly.
/// </summary>
[Collection(TelemetryCollection.Name)]
public sealed class HttpUserAgentParserAspNetCoreTelemetryPathTests : IDisposable
{
    public HttpUserAgentParserAspNetCoreTelemetryPathTests() => HttpUserAgentParserAspNetCoreTelemetry.ResetForTests();

    public void Dispose() => HttpUserAgentParserAspNetCoreTelemetry.ResetForTests();

    [Fact]
    public void Disabled_Telemetry_Reads_Header()
    {
        Assert.False(HttpUserAgentParserAspNetCoreTelemetry.IsEnabled);

        AssertReadsHeader();
    }

    [Fact]
    public void Enabled_Telemetry_Reads_Header()
    {
        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection()).WithAspNetCoreMeterTelemetry();

        Assert.True(HttpUserAgentParserAspNetCoreTelemetry.IsEnabled);

        AssertReadsHeader();
    }

    private static void AssertReadsHeader()
    {
        DefaultHttpContext context = new();
        Assert.Null(context.GetUserAgentString());

        context.Request.Headers.UserAgent = "UA";
        Assert.Equal("UA", context.GetUserAgentString());

        context.Request.Headers.UserAgent = string.Empty;
        Assert.Equal(string.Empty, context.GetUserAgentString());
    }
}
