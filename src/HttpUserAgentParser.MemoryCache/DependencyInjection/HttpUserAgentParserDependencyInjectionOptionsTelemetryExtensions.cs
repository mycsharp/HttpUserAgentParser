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
    /// <exception cref="System.ArgumentException">
    /// <paramref name="meter"/> is used by the other cache package: the core and the MemoryCache package need separate meters,
    /// because both create the instruments <c>cache.hit</c>, <c>cache.miss</c> and <c>cache.size</c>.
    /// </exception>
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
        HttpUserAgentParserMemoryCacheTelemetry.EnableMetersWithName(HttpUserAgentParserMemoryCacheMeters.GetMeterName(meterPrefix));
        return options;
    }
}
