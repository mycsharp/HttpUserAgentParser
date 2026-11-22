// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace MyCSharp.HttpUserAgentParser.Telemetry;

/// <summary>
/// System.Diagnostics.Metrics instruments emitted by MyCSharp.HttpUserAgentParser.
/// This is opt-in and designed to keep overhead negligible unless a listener is enabled.
/// </summary>
/// <remarks>
/// All instruments of a meter are created before they become visible to the parser, so concurrent
/// registrations never observe a partially initialized state. The last registered meter is used.
/// </remarks>
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

#if NET9_0_OR_GREATER
    private static readonly Lock s_lock = new();
#else
    private static readonly object s_lock = new();
#endif

    private static Meter? s_defaultMeter;
    private static Instruments? s_instruments;

    /// <summary>
    /// Gets whether meters have been initialized.
    /// </summary>
    public static bool IsEnabled => Volatile.Read(ref s_instruments) is not null;

    /// <summary>
    /// Gets whether the parse duration histogram is currently enabled by a listener.
    /// </summary>
    public static bool IsParseDurationEnabled => Volatile.Read(ref s_instruments)?.ParseDuration.Enabled ?? false;

    /// <summary>
    /// Creates the metric instruments on <paramref name="meter"/> (or on a default meter) and uses them from now on.
    /// </summary>
    /// <remarks>
    /// Registering the meter that is already in use has no effect; another meter replaces it.
    /// </remarks>
    public static void Enable(Meter? meter = null)
    {
        lock (s_lock)
        {
            meter ??= s_defaultMeter ??= new Meter(MeterName);
            if (ReferenceEquals(s_instruments?.Meter, meter))
            {
                return;
            }

            // Publish the instruments only after all of them exist.
            Volatile.Write(ref s_instruments, new Instruments(meter));
        }
    }

    /// <summary>
    /// Emits a counter increment for a parse request.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ParseRequest() => Volatile.Read(ref s_instruments)?.ParseRequests.Add(1);

    /// <summary>
    /// Records the parse duration in seconds.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ParseDuration(double seconds) => Volatile.Read(ref s_instruments)?.ParseDuration.Record(seconds);

    /// <summary>
    /// Emits a counter increment for a cache hit.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CacheHit() => Volatile.Read(ref s_instruments)?.CacheHit.Add(1);

    /// <summary>
    /// Emits a counter increment for a cache miss.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CacheMiss() => Volatile.Read(ref s_instruments)?.CacheMiss.Add(1);

    /// <summary>
    /// Resets static state to support isolated unit tests.
    /// </summary>
    public static void ResetForTests()
    {
        lock (s_lock)
        {
            Volatile.Write(ref s_instruments, null);
            s_defaultMeter = null;
        }
    }

    /// <summary>
    /// The instruments of one meter.
    /// </summary>
    private sealed class Instruments(Meter meter)
    {
        public Meter Meter { get; } = meter;

        public Counter<long> ParseRequests { get; } = meter.CreateCounter<long>(
            name: "parse.requests",
            unit: "{call}",
            description: "User-Agent parse requests");

        public Histogram<double> ParseDuration { get; } = meter.CreateHistogram<double>(
            name: "parse.duration",
            unit: "s",
            description: "Parse duration");

        public Counter<long> CacheHit { get; } = meter.CreateCounter<long>(
            name: "cache.hit",
            unit: "{call}",
            description: "Cache hit");

        public Counter<long> CacheMiss { get; } = meter.CreateCounter<long>(
            name: "cache.miss",
            unit: "{call}",
            description: "Cache miss");

        public ObservableGauge<long> CacheSize { get; } = meter.CreateObservableGauge<long>(
            name: "cache.size",
            observeValue: static () => HttpUserAgentParserTelemetryState.ConcurrentCacheSize,
            unit: "{entry}",
            description: "Cache size");
    }
}
