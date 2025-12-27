// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace MyCSharp.HttpUserAgentParser.Telemetry;

/// <summary>
/// Opt-in switch for core telemetry.
/// Telemetry is disabled by default to ensure zero overhead unless explicitly enabled.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class HttpUserAgentParserTelemetry
{
    private const int EventCountersFlag = 1;
    private const int MetersFlag = 2;

    private static int s_enabledFlags;

    public static bool IsEnabled => Volatile.Read(ref s_enabledFlags) != 0;

    public static bool AreCountersEnabled
        => (Volatile.Read(ref s_enabledFlags) & EventCountersFlag) != 0
           && HttpUserAgentParserEventSource.Log.IsEnabled();

    public static bool AreMetersEnabled
        => (Volatile.Read(ref s_enabledFlags) & MetersFlag) != 0
           && HttpUserAgentParserMeters.IsEnabled;

    public static bool ShouldMeasureParseDuration
        => AreCountersEnabled || HttpUserAgentParserMeters.IsParseDurationEnabled;

    /// <summary>
    /// Enables core EventCounter telemetry for the parser.
    /// </summary>
    public static void Enable() => Interlocked.Or(ref s_enabledFlags, EventCountersFlag);

    /// <summary>
    /// Enables native System.Diagnostics.Metrics telemetry for the parser.
    /// </summary>
    public static void EnableMeters(Meter? meter = null)
    {
        HttpUserAgentParserMeters.Enable(meter);
        Interlocked.Or(ref s_enabledFlags, MetersFlag);
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ParseRequest()
    {
        int flags = Volatile.Read(ref s_enabledFlags);
        if ((flags & EventCountersFlag) != 0)
        {
            HttpUserAgentParserEventSource.Log.ParseRequest();
        }

        if ((flags & MetersFlag) != 0)
        {
            HttpUserAgentParserMeters.ParseRequest();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ParseDuration(double milliseconds)
    {
        int flags = Volatile.Read(ref s_enabledFlags);
        if ((flags & EventCountersFlag) != 0)
        {
            HttpUserAgentParserEventSource.Log.ParseDuration(milliseconds);
        }

        if ((flags & MetersFlag) != 0)
        {
            HttpUserAgentParserMeters.ParseDuration(milliseconds);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConcurrentCacheHit()
    {
        int flags = Volatile.Read(ref s_enabledFlags);
        if ((flags & EventCountersFlag) != 0)
        {
            HttpUserAgentParserEventSource.Log.ConcurrentCacheHit();
        }

        if ((flags & MetersFlag) != 0)
        {
            HttpUserAgentParserMeters.ConcurrentCacheHit();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConcurrentCacheMiss()
    {
        int flags = Volatile.Read(ref s_enabledFlags);
        if ((flags & EventCountersFlag) != 0)
        {
            HttpUserAgentParserEventSource.Log.ConcurrentCacheMiss();
        }

        if ((flags & MetersFlag) != 0)
        {
            HttpUserAgentParserMeters.ConcurrentCacheMiss();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConcurrentCacheSizeSet(int size)
        => HttpUserAgentParserTelemetryState.SetConcurrentCacheSize(size);

#if DEBUG
    public static void ResetForTests()
    {
        Volatile.Write(ref s_enabledFlags, 0);
        HttpUserAgentParserTelemetryState.ResetForTests();
        HttpUserAgentParserMeters.ResetForTests();
    }
#endif
}
