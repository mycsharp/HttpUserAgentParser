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
           && HttpUserAgentParserEventSource.Log.IsEnabled();

    /// <summary>
    /// Checks if Meters are specifically enabled.
    /// </summary>
    public static bool AreMetersEnabled
        => (Volatile.Read(ref s_enabledFlags) & MetersFlag) != 0
           && HttpUserAgentParserMeters.IsEnabled;

    /// <summary>
    /// Checks if parse duration should be measured.
    /// This is true if either EventCounters are enabled OR if the specific Meter instrument for duration is enabled.
    /// </summary>
    public static bool ShouldMeasureParseDuration
        => AreCountersEnabled || HttpUserAgentParserMeters.IsParseDurationEnabled;

    /// <summary>
    /// Checks if cache size tracking is enabled for either system.
    /// This is used to guard expensive operations like .Count or Interlocked updates.
    /// </summary>
    public static bool IsCacheSizeEnabled
        => AreCountersEnabled || AreMetersEnabled;

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

    /// <summary>
    /// Records a parse request event.
    /// </summary>
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

    /// <summary>
    /// Records the duration of a parse request.
    /// </summary>
    /// <param name="duration">The elapsed duration.</param>
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

    /// <summary>
    /// Records a cache hit in the concurrent dictionary.
    /// </summary>
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

    /// <summary>
    /// Records a cache miss in the concurrent dictionary.
    /// </summary>
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

    /// <summary>
    /// Updates the concurrent cache size.
    /// </summary>
    /// <param name="size">The current size of the cache.</param>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void ConcurrentCacheSizeSet(int size)
        => HttpUserAgentParserTelemetryState.SetConcurrentCacheSize(size);

    /// <summary>
    /// Resets telemetry state for unit testing.
    /// </summary>
    public static void ResetForTests()
    {
        Volatile.Write(ref s_enabledFlags, 0);
        HttpUserAgentParserTelemetryState.ResetForTests();
        HttpUserAgentParserMeters.ResetForTests();
    }
}
