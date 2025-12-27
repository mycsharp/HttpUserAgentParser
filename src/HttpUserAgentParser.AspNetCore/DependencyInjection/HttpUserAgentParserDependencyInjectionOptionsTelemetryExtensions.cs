// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Metrics;
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

    /// <summary>
    /// Enables native System.Diagnostics.Metrics telemetry for the AspNetCore package.
    /// </summary>
    public static HttpUserAgentParserDependencyInjectionOptions WithAspNetCoreMeterTelemetry(
        this HttpUserAgentParserDependencyInjectionOptions options,
        Meter? meter = null)
    {
        HttpUserAgentParserAspNetCoreTelemetry.EnableMeters(meter);
        return options;
    }
}
