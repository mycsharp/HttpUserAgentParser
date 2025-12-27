// Copyright © https://myCSharp.de - all rights reserved

using MyCSharp.HttpUserAgentParser.Telemetry;

namespace MyCSharp.HttpUserAgentParser.DependencyInjection;

/// <summary>
/// Fluent extensions to enable telemetry.
/// </summary>
public static class HttpUserAgentParserDependencyInjectionOptionsTelemetryExtensions
{
    /// <summary>
    /// Enables core EventCounter telemetry for the parser.
    /// This is opt-in to keep the default path free of telemetry overhead.
    /// </summary>
    public static HttpUserAgentParserDependencyInjectionOptions WithTelemetry(
        this HttpUserAgentParserDependencyInjectionOptions options)
    {
        HttpUserAgentParserTelemetry.Enable();
        return options;
    }
}
