// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.Telemetry;

/// <summary>
/// Opt-in switch for MemoryCache package telemetry.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class HttpUserAgentParserMemoryCacheTelemetry
{
    // Bit flags to track which telemetry systems are enabled.
    // This allows us to support both EventCounters and Meters simultaneously with a single check.
    private const int EventCountersFlag = 1;
    private const int MetersFlag = 2;

    // Volatile integer used as a bitmask.
    // Volatile.Read is used to ensure we get the latest value without the overhead of a full lock,
    // making the "is telemetry enabled?" check extremely cheap on the hot path.
    private static int s_enabledFlags;

    /// <summary>
    /// Fast check if ANY telemetry is enabled.
    /// Used to guard the entire telemetry block to minimize overhead when not in use.
    /// </summary>
    public static bool IsEnabled => Volatile.Read(ref s_enabledFlags) != 0;

    /// <summary>
    /// Checks if EventCounters are specifically enabled.
    /// </summary>
    public static bool AreCountersEnabled
        => (Volatile.Read(ref s_enabledFlags) & EventCountersFlag) != 0
           && HttpUserAgentParserMemoryCacheEventSource.Log.IsEnabled();

    /// <summary>
    /// Checks if Meters are specifically enabled.
    /// </summary>
    public static bool AreMetersEnabled
        => (Volatile.Read(ref s_enabledFlags) & MetersFlag) != 0
           && HttpUserAgentParserMemoryCacheMeters.IsEnabled;

    /// <summary>
    /// Checks if cache size tracking is enabled for either system.
    /// This is used to guard expensive operations like .Count or Interlocked updates.
    /// </summary>
    public static bool IsCacheSizeEnabled
        => AreCountersEnabled || AreMetersEnabled;

    /// <summary>
    /// Enables EventCounter telemetry for the MemoryCache provider.
    /// </summary>
    public static void Enable() => Interlocked.Or(ref s_enabledFlags, EventCountersFlag);

    /// <summary>
    /// Enables native System.Diagnostics.Metrics telemetry for the MemoryCache provider.
    /// </summary>
    public static void EnableMeters(Meter? meter = null)
    {
        HttpUserAgentParserMemoryCacheMeters.Enable(meter);
        Interlocked.Or(ref s_enabledFlags, MetersFlag);
    }

    /// <summary>
    /// Records a cache hit.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CacheHit()
    {
        int flags = Volatile.Read(ref s_enabledFlags);
        if ((flags & EventCountersFlag) != 0)
        {
            HttpUserAgentParserMemoryCacheEventSource.Log.CacheHit();
        }

        if ((flags & MetersFlag) != 0)
        {
            HttpUserAgentParserMemoryCacheMeters.CacheHit();
        }
    }

    /// <summary>
    /// Records a cache miss.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CacheMiss()
    {
        int flags = Volatile.Read(ref s_enabledFlags);
        if ((flags & EventCountersFlag) != 0)
        {
            HttpUserAgentParserMemoryCacheEventSource.Log.CacheMiss();
        }

        if ((flags & MetersFlag) != 0)
        {
            HttpUserAgentParserMemoryCacheMeters.CacheMiss();
        }
    }

    /// <summary>
    /// Increments the cache size counter.
    /// </summary>
    /// <remarks>
    /// The operation is forwarded to the internal telemetry state and is safe
    /// to call concurrently.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CacheSizeIncrement()
        => HttpUserAgentParserMemoryCacheTelemetryState.CacheSizeIncrement();

    /// <summary>
    /// Decrements the cache size counter.
    /// </summary>
    /// <remarks>
    /// The operation is forwarded to the internal telemetry state and is safe
    /// to call concurrently.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CacheSizeDecrement()
        => HttpUserAgentParserMemoryCacheTelemetryState.CacheSizeDecrement();
}
