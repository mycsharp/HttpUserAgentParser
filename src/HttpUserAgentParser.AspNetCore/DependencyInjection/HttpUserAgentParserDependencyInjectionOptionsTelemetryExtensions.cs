// Copyright © https://myCSharp.de - all rights reserved

using MyCSharp.HttpUserAgentParser.AspNetCore.Telemetry;
using MyCSharp.HttpUserAgentParser.DependencyInjection;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.DependencyInjection;

/// <summary>
/// Fluent extensions to enable telemetry for the AspNetCore package.
/// </summary>
public static class HttpUserAgentParserDependencyInjectionOptionsTelemetryExtensions
{
    /// <summary>
    /// Enables EventCounter telemetry for the AspNetCore package.
    /// </summary>
    public static HttpUserAgentParserDependencyInjectionOptions WithAspNetCoreTelemetry(
        this HttpUserAgentParserDependencyInjectionOptions options)
    {
        HttpUserAgentParserAspNetCoreTelemetry.Enable();
        return options;
    }
}
