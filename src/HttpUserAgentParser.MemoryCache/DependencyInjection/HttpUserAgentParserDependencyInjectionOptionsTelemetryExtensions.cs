// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Metrics;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using MyCSharp.HttpUserAgentParser.MemoryCache.Telemetry;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.DependencyInjection;

/// <summary>
/// Fluent extensions to enable telemetry for the MemoryCache package.
/// </summary>
public static class HttpUserAgentParserDependencyInjectionOptionsTelemetryExtensions
{
    /// <summary>
    /// Enables EventCounter telemetry for the MemoryCache provider.
    /// </summary>
    public static HttpUserAgentParserDependencyInjectionOptions WithMemoryCacheTelemetry(
        this HttpUserAgentParserDependencyInjectionOptions options)
    {
        HttpUserAgentParserMemoryCacheTelemetry.Enable();
        return options;
    }

    /// <summary>
    /// Enables native System.Diagnostics.Metrics telemetry for the MemoryCache provider.
    /// </summary>
    public static HttpUserAgentParserDependencyInjectionOptions WithMemoryCacheMeterTelemetry(
        this HttpUserAgentParserDependencyInjectionOptions options,
        Meter? meter = null)
    {
        HttpUserAgentParserMemoryCacheTelemetry.EnableMeters(meter);
        return options;
    }

    /// <summary>
    /// Enables native System.Diagnostics.Metrics telemetry for the MemoryCache provider using a custom meter prefix.
    /// </summary>
    /// <param name="options">The options container.</param>
    /// <param name="meterPrefix">The prefix to use for the meter name.</param>
    /// <exception cref="ArgumentException">Thrown when the prefix is not empty and does not match the required format.</exception>
    public static HttpUserAgentParserDependencyInjectionOptions WithMemoryCacheMeterTelemetryPrefix(
        this HttpUserAgentParserDependencyInjectionOptions options,
        string meterPrefix)
    {
        Meter meter = new(HttpUserAgentParserMemoryCacheMeters.GetMeterName(meterPrefix));
        HttpUserAgentParserMemoryCacheTelemetry.EnableMeters(meter);
        return options;
    }
}
