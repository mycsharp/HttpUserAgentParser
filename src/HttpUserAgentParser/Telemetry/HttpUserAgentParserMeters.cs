// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace MyCSharp.HttpUserAgentParser.Telemetry;

/// <summary>
/// System.Diagnostics.Metrics instruments emitted by MyCSharp.HttpUserAgentParser.
/// This is opt-in and designed to keep overhead negligible unless a listener is enabled.
/// </summary>
/// <remarks>
/// Instruments are created once on first enablement and emit no data unless observed
/// by an active listener.
/// </remarks>
[ExcludeFromCodeCoverage]
internal static class HttpUserAgentParserMeters
{
    /// <summary>
    /// The meter name used for all instruments.
    /// </summary>
    private const string MeterNameSuffix = "http_user_agent_parser";

    /// <summary>
    /// The meter name used for all instruments.
    /// </summary>
    public const string MeterName = "mycsharp." + MeterNameSuffix;

    /// <summary>
    /// Builds a meter name from a custom prefix.
    /// </summary>
    /// <param name="meterPrefix">
    /// The prefix to use. When null, the default prefix is used. When empty,
    /// no prefix is applied. Otherwise, the prefix must be alphanumeric and end with '.'.
    /// </param>
    /// <returns>The full meter name.</returns>
    /// <exception cref="ArgumentException">Thrown when the prefix is not empty and does not match the required format.</exception>
    public static string GetMeterName(string? meterPrefix)
        => HttpUserAgentParserMeterNameHelper.GetMeterName(meterPrefix, MeterNameSuffix);

    private static int s_initialized;

    private static Meter? s_meter;

    private static Counter<long>? s_parseRequests;
    private static Histogram<double>? s_parseDuration;

    private static Counter<long>? s_concurrentCacheHit;
    private static Counter<long>? s_concurrentCacheMiss;
    private static ObservableGauge<long>? s_concurrentCacheSize;

    /// <summary>
    /// Gets whether meters have been initialized.
    /// </summary>
    public static bool IsEnabled => Volatile.Read(ref s_initialized) != 0;

    /// <summary>
    /// Gets whether the parse duration histogram is currently enabled by a listener.
    /// </summary>
    public static bool IsParseDurationEnabled => s_parseDuration?.Enabled ?? false;

    /// <summary>
    /// Initializes the meter and creates all metric instruments.
    /// </summary>
    /// <remarks>
    /// Initialization is performed at most once. Subsequent calls are ignored.
    /// </remarks>
    public static void Enable(Meter? meter = null)
    {
        s_meter = meter ?? new Meter(MeterName);

        s_parseRequests = s_meter.CreateCounter<long>(
            name: "parse.requests",
            unit: "{call}",
            description: "User-Agent parse requests");

        s_parseDuration = s_meter.CreateHistogram<double>(
            name: "parse.duration",
            unit: "s",
            description: "Parse duration");

        s_concurrentCacheHit = s_meter.CreateCounter<long>(
            name: "cache.hit",
            unit: "{call}",
            description: "Cache hit");

        s_concurrentCacheMiss = s_meter.CreateCounter<long>(
            name: "cache.miss",
            unit: "{call}",
            description: "Cache miss");

        s_concurrentCacheSize = s_meter.CreateObservableGauge<long>(
            name: "cache.size",
            observeValue: static () => HttpUserAgentParserTelemetryState.ConcurrentCacheSize,
            unit: "{entry}",
            description: "Cache size");
    }

    /// <summary>
    /// Emits a counter increment for a parse request.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ParseRequest() => s_parseRequests?.Add(1);

    /// <summary>
    /// Records the parse duration in seconds.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ParseDuration(double seconds) => s_parseDuration?.Record(seconds);

    /// <summary>
    /// Emits a counter increment for a cache hit.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CacheHit() => s_concurrentCacheHit?.Add(1);

    /// <summary>
    /// Emits a counter increment for a cache miss.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CacheMiss() => s_concurrentCacheMiss?.Add(1);

    /// <summary>
    /// Resets static state to support isolated unit tests.
    /// </summary>
    public static void ResetForTests()
    {
        Volatile.Write(ref s_initialized, 0);

        s_meter = null;
        s_parseRequests = null;
        s_parseDuration = null;
        s_concurrentCacheHit = null;
        s_concurrentCacheMiss = null;
        s_concurrentCacheSize = null;
    }
}
