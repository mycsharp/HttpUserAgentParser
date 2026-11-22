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
/// registrations never observe a partially initialized state. The last registered meter is used; a replaced meter
/// no longer reports the cache size.
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
    /// no prefix is applied. Otherwise, the prefix must be ASCII letters or digits followed by '.'.
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

    /// <summary>
    /// The meters this class created (the default meter and meters with a prefixed name), by name.
    /// </summary>
    private static readonly Dictionary<string, Meter> s_ownedMeters = new(StringComparer.Ordinal);

    private static Instruments? s_instruments;

    /// <summary>
    /// The instruments of every registered meter; a meter registered again reuses its instruments.
    /// </summary>
    private static readonly ConditionalWeakTable<Meter, Instruments> s_instrumentsByMeter = new();

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
    /// Another meter replaces the one in use. A meter that is registered again reuses its instruments.
    /// </remarks>
    public static void Enable(Meter? meter = null) => Use(meter, MeterName);

    /// <summary>
    /// Creates the metric instruments on a meter named <paramref name="meterName"/> that this class creates and owns.
    /// </summary>
    /// <remarks>
    /// An owned meter is created once per name and reused when the name is registered again.
    /// </remarks>
    public static void EnableOwnedMeter(string meterName) => Use(null, meterName);

    /// <summary>
    /// Uses <paramref name="meter"/> or, if null, an owned meter named <paramref name="ownedMeterName"/>.
    /// </summary>
    private static void Use(Meter? meter, string ownedMeterName)
    {
        lock (s_lock)
        {
            // Own meters are created once per name and kept: a replaced meter is not disposed, so a measurement that read
            // the previous instruments just before the switch is still recorded. Meters of the caller are never disposed.
            if (meter is null && !s_ownedMeters.TryGetValue(ownedMeterName, out meter))
            {
                meter = new Meter(ownedMeterName);
                s_ownedMeters.Add(ownedMeterName, meter);
            }

            // The core and the MemoryCache package must not share a meter (same instrument names).
            HttpUserAgentParserCacheMeterGuard.UseForCore(meter);

            // Instruments are created once per meter and published only after all of them exist.
            Volatile.Write(ref s_instruments, s_instrumentsByMeter.GetValue(meter, static m => new Instruments(m)));
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
            foreach (Meter ownedMeter in s_ownedMeters.Values)
            {
                ownedMeter.Dispose();
            }

            s_ownedMeters.Clear();
            HttpUserAgentParserCacheMeterGuard.ResetCoreForTests();
            s_instrumentsByMeter.Clear();
        }
    }

    /// <summary>
    /// The instruments of one meter.
    /// </summary>
    private sealed class Instruments
    {
        public Instruments(Meter meter)
        {
            ParseRequests = meter.CreateCounter<long>(
                name: "parse.requests",
                unit: "{call}",
                description: "User-Agent parse requests");

            ParseDuration = meter.CreateHistogram<double>(
                name: "parse.duration",
                unit: "s",
                description: "Parse duration");

            CacheHit = meter.CreateCounter<long>(
                name: "cache.hit",
                unit: "{call}",
                description: "Cache hit");

            CacheMiss = meter.CreateCounter<long>(
                name: "cache.miss",
                unit: "{call}",
                description: "Cache miss");

            CacheSize = meter.CreateObservableGauge(
                name: "cache.size",
                observeValues: ObserveCacheSize,
                unit: "{entry}",
                description: "Cache size");
        }

        public Counter<long> ParseRequests { get; }

        public Histogram<double> ParseDuration { get; }

        public Counter<long> CacheHit { get; }

        public Counter<long> CacheMiss { get; }

        public ObservableGauge<long> CacheSize { get; }

        /// <summary>
        /// Reports the cache size only while these instruments are in use, so a replaced meter does not report it as well.
        /// </summary>
        private IEnumerable<Measurement<long>> ObserveCacheSize()
            => ReferenceEquals(Volatile.Read(ref s_instruments), this)
                ? [new Measurement<long>(HttpUserAgentParserTelemetryState.ConcurrentCacheSize)]
                : [];
    }
}
