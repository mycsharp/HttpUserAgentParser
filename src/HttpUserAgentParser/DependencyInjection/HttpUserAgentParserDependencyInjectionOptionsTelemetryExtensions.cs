// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Metrics;
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

    /// <summary>
    /// Enables native System.Diagnostics.Metrics telemetry for the parser.
    /// This is opt-in to keep the default path free of telemetry overhead.
    /// </summary>
    /// <exception cref="System.ArgumentException">
    /// <paramref name="meter"/> is used by the other cache package: the core and the MemoryCache package need separate meters,
    /// because both create the instruments <c>cache.hit</c>, <c>cache.miss</c> and <c>cache.size</c>.
    /// </exception>
    public static HttpUserAgentParserDependencyInjectionOptions WithMeterTelemetry(
        this HttpUserAgentParserDependencyInjectionOptions options,
        Meter? meter = null)
    {
        HttpUserAgentParserTelemetry.EnableMeters(meter);
        return options;
    }

    /// <summary>
    /// Enables native System.Diagnostics.Metrics telemetry for the parser using a custom meter prefix.
    /// </summary>
    /// <param name="options">The options container.</param>
    /// <param name="meterPrefix">The prefix to use for the meter name.</param>
    /// <exception cref="ArgumentException">Thrown when the prefix is not empty and does not match the required format.</exception>
    public static HttpUserAgentParserDependencyInjectionOptions WithMeterTelemetryPrefix(
        this HttpUserAgentParserDependencyInjectionOptions options,
        string meterPrefix)
    {
        HttpUserAgentParserTelemetry.EnableMetersWithName(HttpUserAgentParserMeters.GetMeterName(meterPrefix));
        return options;
    }
}
