// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace MyCSharp.HttpUserAgentParser.Telemetry;

/// <summary>
/// Opt-in switch for core telemetry.
/// </summary>
internal static class HttpUserAgentParserTelemetry
{
    private const int EventCountersFlag = 1;
    private const int MetersFlag = 2;

    private static int s_enabledFlags;

    public static bool IsEnabled => Volatile.Read(ref s_enabledFlags) != 0;

    public static bool AreCountersEnabled
        => (Volatile.Read(ref s_enabledFlags) & EventCountersFlag) != 0
           && HttpUserAgentParserEventSource.Log.IsEnabled();

    public static bool ShouldMeasureParseDuration
        => AreCountersEnabled || HttpUserAgentParserMeters.IsParseDurationEnabled;

    /// <summary>
    /// Enables core EventCounter telemetry for the parser.
    /// </summary>
    public static void Enable()
    {
        // Give listeners a chance to subscribe before recording the first event.
        _ = HttpUserAgentParserEventSource.Log;
        Interlocked.Or(ref s_enabledFlags, EventCountersFlag);
    }

    /// <summary>
    /// Enables native System.Diagnostics.Metrics telemetry for the parser.
    /// </summary>
    public static void EnableMeters(Meter? meter = null)
        => HttpUserAgentParserMeters.Enable(meter);

    /// <summary>
    /// Enables native System.Diagnostics.Metrics telemetry with a meter named <paramref name="meterName"/> that the package owns.
    /// </summary>
    public static void EnableMetersWithName(string meterName)
        => HttpUserAgentParserMeters.EnableOwnedMeter(meterName);

    internal static void MarkMetersEnabled() => Interlocked.Or(ref s_enabledFlags, MetersFlag);

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
    public static void ParseDuration(TimeSpan duration)
    {
        int flags = Volatile.Read(ref s_enabledFlags);
        if ((flags & EventCountersFlag) != 0)
        {
            HttpUserAgentParserEventSource.Log.ParseDuration(duration.TotalSeconds);
        }

        if ((flags & MetersFlag) != 0)
        {
            HttpUserAgentParserMeters.ParseDuration(duration.TotalSeconds);
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConcurrentCacheHit()
    {
        int flags = Volatile.Read(ref s_enabledFlags);
        if ((flags & EventCountersFlag) != 0)
        {
            HttpUserAgentParserEventSource.Log.CacheHit();
        }

        if ((flags & MetersFlag) != 0)
        {
            HttpUserAgentParserMeters.CacheHit();
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConcurrentCacheMiss()
    {
        int flags = Volatile.Read(ref s_enabledFlags);
        if ((flags & EventCountersFlag) != 0)
        {
            HttpUserAgentParserEventSource.Log.CacheMiss();
        }

        if ((flags & MetersFlag) != 0)
        {
            HttpUserAgentParserMeters.CacheMiss();
        }
    }

    public static void ResetForTests()
    {
        Volatile.Write(ref s_enabledFlags, 0);
        HttpUserAgentParserMeters.ResetForTests();
    }
}
