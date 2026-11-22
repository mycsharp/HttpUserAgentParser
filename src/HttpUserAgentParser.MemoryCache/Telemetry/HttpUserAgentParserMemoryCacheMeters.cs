// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using MyCSharp.HttpUserAgentParser.Telemetry;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.Telemetry;

/// <summary>
/// System.Diagnostics.Metrics instruments emitted by MyCSharp.HttpUserAgentParser.MemoryCache.
/// </summary>
/// <remarks>
/// All instruments of a meter are created before they become visible to the provider, so concurrent
/// registrations never observe a partially initialized state. The last registered meter is used; a replaced meter
/// no longer reports the cache size.
/// </remarks>
internal static class HttpUserAgentParserMemoryCacheMeters
{
    private const string MeterNameSuffix = "http_user_agent_parser.memorycache";

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

    /// <summary>
    /// The meter this class created and uses (the default meter or a meter with a prefixed name), if any.
    /// </summary>
    private static Meter? s_ownedMeter;

    private static Instruments? s_instruments;

    /// <summary>
    /// The instruments of every registered meter; a meter registered again reuses its instruments.
    /// </summary>
    private static readonly ConditionalWeakTable<Meter, Instruments> s_instrumentsByMeter = new();

    public static bool IsEnabled => Volatile.Read(ref s_instruments) is not null;

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
    /// An owned meter is created once per name; it is disposed when another meter replaces it.
    /// </remarks>
    public static void EnableOwnedMeter(string meterName) => Use(null, meterName);

    /// <summary>
    /// Uses <paramref name="meter"/> or, if null, an owned meter named <paramref name="ownedMeterName"/>.
    /// </summary>
    private static void Use(Meter? meter, string ownedMeterName)
    {
        lock (s_lock)
        {
            Meter? previousOwnedMeter = s_ownedMeter;
            if (meter is null)
            {
                meter = previousOwnedMeter is not null && string.Equals(previousOwnedMeter.Name, ownedMeterName, StringComparison.Ordinal)
                    ? previousOwnedMeter
                    : new Meter(ownedMeterName);
                s_ownedMeter = meter;
            }
            else
            {
                s_ownedMeter = null;
            }

            // Instruments are created once per meter and published only after all of them exist.
            Volatile.Write(ref s_instruments, s_instrumentsByMeter.GetValue(meter, static m => new Instruments(m)));

            // A replaced owned meter is no longer used: dispose it, so repeated registrations do not keep meters alive.
            // Meters passed in by the caller are never disposed.
            if (previousOwnedMeter is not null && !ReferenceEquals(previousOwnedMeter, meter))
            {
                previousOwnedMeter.Dispose();
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CacheHit() => Volatile.Read(ref s_instruments)?.CacheHit.Add(1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CacheMiss() => Volatile.Read(ref s_instruments)?.CacheMiss.Add(1);

    public static void ResetForTests()
    {
        lock (s_lock)
        {
            Volatile.Write(ref s_instruments, null);
            s_ownedMeter?.Dispose();
            s_ownedMeter = null;
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

        public Counter<long> CacheHit { get; }

        public Counter<long> CacheMiss { get; }

        public ObservableGauge<long> CacheSize { get; }

        /// <summary>
        /// Reports the cache size only while these instruments are in use, so a replaced meter does not report it as well.
        /// </summary>
        private IEnumerable<Measurement<long>> ObserveCacheSize()
            => ReferenceEquals(Volatile.Read(ref s_instruments), this)
                ? [new Measurement<long>(HttpUserAgentParserMemoryCacheTelemetryState.CacheSize)]
                : [];
    }
}
