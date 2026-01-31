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
    public const string MeterName = HttpUserAgentParserMemoryCachedProvider.MeterName;

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
            description: "MemoryCache cache hit");

        s_cacheMiss = s_meter.CreateCounter<long>(
            name: "cache.miss",
            unit: "{call}",
            description: "MemoryCache cache miss");

        s_cacheSize = s_meter.CreateObservableGauge<long>(
            name: "cache.size",
            observeValue: static () => HttpUserAgentParserMemoryCacheTelemetryState.CacheSize,
            unit: "{entry}",
            description: "MemoryCache cache size");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CacheHit() => s_cacheHit?.Add(1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CacheMiss() => s_cacheMiss?.Add(1);
}
