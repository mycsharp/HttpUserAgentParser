// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using MyCSharp.HttpUserAgentParser.Telemetry;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.Telemetry;

/// <summary>
/// System.Diagnostics.Metrics instruments emitted by MyCSharp.HttpUserAgentParser.MemoryCache.
/// </summary>
/// <remarks>
/// See <see cref="HttpUserAgentParserMeterRegistration{TInstruments}"/> for the registration rules; a replaced meter no longer
/// reports the cache size.
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
    /// no prefix is applied. Otherwise, the prefix must be ASCII letters or digits followed by '.'.
    /// </param>
    /// <returns>The full meter name.</returns>
    /// <exception cref="ArgumentException">Thrown when the prefix is not empty and does not match the required format.</exception>
    public static string GetMeterName(string? meterPrefix)
        => HttpUserAgentParserMeterNameHelper.GetMeterName(meterPrefix, MeterNameSuffix);

    private static readonly HttpUserAgentParserMeterRegistration<Instruments> s_registration = new(
        static meter => new Instruments(meter),
        HttpUserAgentParserCacheMeterGuard.ReserveForMemoryCache, // the core and the MemoryCache package must not share a meter
        HttpUserAgentParserCacheMeterGuard.ResetMemoryCacheForTests,
        HttpUserAgentParserMemoryCacheTelemetry.MarkMetersEnabled);

    public static bool IsEnabled => s_registration.Current is not null;

    /// <summary>
    /// Creates the metric instruments on <paramref name="meter"/> (or on a default meter) and uses them from now on.
    /// </summary>
    public static void Enable(Meter? meter = null) => s_registration.Use(meter, MeterName);

    /// <summary>
    /// Creates the metric instruments on a meter named <paramref name="meterName"/> that this class creates and owns.
    /// </summary>
    public static void EnableOwnedMeter(string meterName) => s_registration.Use(meter: null, meterName);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CacheHit() => s_registration.Current?.CacheHit.Add(1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CacheMiss() => s_registration.Current?.CacheMiss.Add(1);

    public static void ResetForTests() => s_registration.ResetForTests();

    /// <summary>
    /// The instruments of one meter.
    /// </summary>
    private sealed class Instruments(Meter meter) : HttpUserAgentParserMeterInstruments(meter)
    {
        private Counter<long>? _cacheHit;
        private Counter<long>? _cacheMiss;
        private int _cacheSizeCreated;

        public Counter<long> CacheHit => _cacheHit!;

        public Counter<long> CacheMiss => _cacheMiss!;

        public override void CreateMissingInstruments()
        {
            CreateInstrument(ref _cacheHit, Meter.CreateCounter<long>, "cache.hit", "{call}", "Cache hit");
            CreateInstrument(ref _cacheMiss, Meter.CreateCounter<long>, "cache.miss", "{call}", "Cache miss");

            // Observable instruments are not shared by the meter: created at most once, also if a listener threw.
            if (Interlocked.Exchange(ref _cacheSizeCreated, 1) == 0)
            {
                _ = Meter.CreateObservableGauge(name: "cache.size", observeValues: ObserveCacheSize, unit: "{entry}", description: "Cache size");
            }
        }

        /// <summary>
        /// Reports the cache size only while these instruments are in use, so a replaced meter does not report it as well.
        /// </summary>
        private IEnumerable<Measurement<long>> ObserveCacheSize()
            => ReferenceEquals(s_registration.Current, this)
                ? [new Measurement<long>(HttpUserAgentParserMemoryCacheTelemetryState.CacheSize)]
                : [];
    }
}
