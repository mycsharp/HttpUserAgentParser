// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.Telemetry;

/// <summary>
/// System.Diagnostics.Metrics instruments emitted by MyCSharp.HttpUserAgentParser.MemoryCache.
/// </summary>
[ExcludeFromCodeCoverage]
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
    {
        if (meterPrefix is null)
        {
            return MeterName;
        }

        meterPrefix = meterPrefix.Trim();
        if (meterPrefix.Length == 0)
        {
            return MeterNameSuffix;
        }

        if (!meterPrefix.EndsWith('.'))
        {
            throw new ArgumentException("Meter prefix must end with '.'.", nameof(meterPrefix));
        }

        for (int i = 0; i < meterPrefix.Length - 1; i++)
        {
            char c = meterPrefix[i];
            if (!char.IsLetterOrDigit(c))
            {
                throw new ArgumentException("Meter prefix must be alphanumeric.", nameof(meterPrefix));
            }
        }

        return meterPrefix + MeterNameSuffix;
    }

    private static int s_initialized;

    private static Meter? s_meter;
    private static Counter<long>? s_cacheHit;
    private static Counter<long>? s_cacheMiss;
    private static ObservableGauge<long>? s_cacheSize;

    public static bool IsEnabled => Volatile.Read(ref s_initialized) != 0;

    public static void Enable(Meter? meter = null)
    {
        if (Interlocked.Exchange(ref s_initialized, 1) == 1)
        {
            return;
        }

        s_meter = meter ?? new Meter(MeterName);

        s_cacheHit = s_meter.CreateCounter<long>(
            name: "cache.hit",
            unit: "{call}",
            description: "Cache hit");

        s_cacheMiss = s_meter.CreateCounter<long>(
            name: "cache.miss",
            unit: "{call}",
            description: "Cache miss");

        s_cacheSize = s_meter.CreateObservableGauge<long>(
            name: "cache.size",
            observeValue: static () => HttpUserAgentParserMemoryCacheTelemetryState.CacheSize,
            unit: "{entry}",
            description: "Cache size");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CacheHit() => s_cacheHit?.Add(1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CacheMiss() => s_cacheMiss?.Add(1);

    public static void ResetForTests()
    {
        Volatile.Write(ref s_initialized, 0);

        s_meter = null;
        s_cacheHit = null;
        s_cacheMiss = null;
        s_cacheSize = null;
    }
}
