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
    public const string MeterName = HttpUserAgentParser.MeterName;

    private static int s_initialized;

    private static Meter? s_meter;

    private static Counter<long>? s_parseRequests;
    private static Histogram<double>? s_parseDurationMs;

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
    public static bool IsParseDurationEnabled => s_parseDurationMs?.Enabled ?? false;

    /// <summary>
    /// Initializes the meter and creates all metric instruments.
    /// </summary>
    /// <remarks>
    /// Initialization is performed at most once. Subsequent calls are ignored.
    /// </remarks>
    public static void Enable(Meter? meter = null)
    {
        if (Interlocked.Exchange(ref s_initialized, 1) == 1)
        {
            return;
        }

        s_meter = meter ?? new Meter(MeterName);

        s_parseRequests = s_meter.CreateCounter<long>(
            name: "parse-requests",
            unit: "calls",
            description: "User-Agent parse requests");

        s_parseDurationMs = s_meter.CreateHistogram<double>(
            name: "parse-duration",
            unit: "ms",
            description: "Parse duration");

        s_concurrentCacheHit = s_meter.CreateCounter<long>(
            name: "cache-concurrentdictionary-hit",
            unit: "calls",
            description: "ConcurrentDictionary cache hit");

        s_concurrentCacheMiss = s_meter.CreateCounter<long>(
            name: "cache-concurrentdictionary-miss",
            unit: "calls",
            description: "ConcurrentDictionary cache miss");

        s_concurrentCacheSize = s_meter.CreateObservableGauge<long>(
            name: "cache-concurrentdictionary-size",
            observeValue: static () => HttpUserAgentParserTelemetryState.ConcurrentCacheSize,
            unit: "entries",
            description: "ConcurrentDictionary cache size");
    }

    /// <summary>
    /// Emits a counter increment for a parse request.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ParseRequest() => s_parseRequests?.Add(1);

    /// <summary>
    /// Records the parse duration in milliseconds.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ParseDuration(double milliseconds) => s_parseDurationMs?.Record(milliseconds);

    /// <summary>
    /// Emits a counter increment for a concurrent dictionary cache hit.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConcurrentCacheHit() => s_concurrentCacheHit?.Add(1);

    /// <summary>
    /// Emits a counter increment for a concurrent dictionary cache miss.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConcurrentCacheMiss() => s_concurrentCacheMiss?.Add(1);

    /// <summary>
    /// Resets static state to support isolated unit tests.
    /// </summary>
    public static void ResetForTests()
    {
        Volatile.Write(ref s_initialized, 0);

        s_meter = null;
        s_parseRequests = null;
        s_parseDurationMs = null;
        s_concurrentCacheHit = null;
        s_concurrentCacheMiss = null;
        s_concurrentCacheSize = null;
    }
}
