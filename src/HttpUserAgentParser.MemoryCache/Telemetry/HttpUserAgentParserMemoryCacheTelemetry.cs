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
    private const int EventCountersFlag = 1;
    private const int MetersFlag = 2;

    private static int s_enabledFlags;

    public static bool IsEnabled => Volatile.Read(ref s_enabledFlags) != 0;

    public static bool AreCountersEnabled
        => (Volatile.Read(ref s_enabledFlags) & EventCountersFlag) != 0
           && HttpUserAgentParserMemoryCacheEventSource.Log.IsEnabled();

    public static bool AreMetersEnabled
        => (Volatile.Read(ref s_enabledFlags) & MetersFlag) != 0
           && HttpUserAgentParserMemoryCacheMeters.IsEnabled;

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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CacheSizeIncrement()
        => HttpUserAgentParserMemoryCacheTelemetryState.CacheSizeIncrement();

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void CacheSizeDecrement()
        => HttpUserAgentParserMemoryCacheTelemetryState.CacheSizeDecrement();
}
