// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.Telemetry;

/// <summary>
/// Opt-in switch for AspNetCore package telemetry.
/// </summary>
/// <remarks>
/// Controls whether telemetry is emitted via event counters and/or meters.
/// The state is evaluated using lock-free, thread-safe reads and is intended
/// to be checked on hot paths.
/// </remarks>
internal static class HttpUserAgentParserAspNetCoreTelemetry
{
    /// <summary>
    /// Flag indicating that event counter–based telemetry is enabled.
    /// </summary>
    private const int EventCountersFlag = 1;

    /// <summary>
    /// Flag indicating that meter-based telemetry is enabled.
    /// </summary>
    private const int MetersFlag = 2;

    /// <summary>
    /// Bit field storing the currently enabled telemetry backends.
    /// </summary>
    /// <remarks>
    /// Accessed using volatile reads to ensure cross-thread visibility
    /// without requiring synchronization.
    /// </remarks>
    private static int s_enabledFlags;

    /// <summary>
    /// Gets a value indicating whether any telemetry backend is enabled.
    /// </summary>
    /// <remarks>
    /// Returns <see langword="true"/> if at least one telemetry backend
    /// has been enabled.
    /// </remarks>
    public static bool IsEnabled
        => Volatile.Read(ref s_enabledFlags) != 0;

    /// <summary>
    /// Enables EventCounter telemetry for the AspNetCore package.
    /// </summary>
    public static void Enable()
    {
        // Force EventSource construction at enable-time so listeners can subscribe deterministically.
        // This avoids CI-only timing races where first telemetry events happen before listener attachment.
        _ = HttpUserAgentParserAspNetCoreEventSource.Log;
        Interlocked.Or(ref s_enabledFlags, EventCountersFlag);
    }

    /// <summary>
    /// Enables native System.Diagnostics.Metrics telemetry for the AspNetCore package.
    /// </summary>
    public static void EnableMeters(Meter? meter = null)
    {
        HttpUserAgentParserAspNetCoreMeters.Enable(meter);
        Interlocked.Or(ref s_enabledFlags, MetersFlag);
    }

    /// <summary>
    /// Enables native System.Diagnostics.Metrics telemetry with a meter named <paramref name="meterName"/> that the package owns.
    /// </summary>
    public static void EnableMetersWithName(string meterName)
    {
        HttpUserAgentParserAspNetCoreMeters.EnableOwnedMeter(meterName);
        Interlocked.Or(ref s_enabledFlags, MetersFlag);
    }

    /// <summary>
    /// Records telemetry indicating that a User-Agent header was present.
    /// </summary>
    /// <remarks>
    /// Emits telemetry only for the enabled backends (event counters and/or meters).
    /// The method is optimized for hot paths and performs a single volatile flag read.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UserAgentPresent()
    {
        int flags = Volatile.Read(ref s_enabledFlags);
        if ((flags & EventCountersFlag) != 0)
        {
            HttpUserAgentParserAspNetCoreEventSource.Log.UserAgentPresent();
        }

        if ((flags & MetersFlag) != 0)
        {
            HttpUserAgentParserAspNetCoreMeters.UserAgentPresent();
        }
    }

    /// <summary>
    /// Records telemetry indicating that a User-Agent header was missing.
    /// </summary>
    /// <remarks>
    /// Emits telemetry only for the enabled backends (event counters and/or meters).
    /// The method is optimized for hot paths and performs a single volatile flag read.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UserAgentMissing()
    {
        int flags = Volatile.Read(ref s_enabledFlags);
        if ((flags & EventCountersFlag) != 0)
        {
            HttpUserAgentParserAspNetCoreEventSource.Log.UserAgentMissing();
        }

        if ((flags & MetersFlag) != 0)
        {
            HttpUserAgentParserAspNetCoreMeters.UserAgentMissing();
        }
    }

    /// <summary>
    /// Resets telemetry state for unit tests.
    /// </summary>
    public static void ResetForTests()
    {
        Volatile.Write(ref s_enabledFlags, 0);
        HttpUserAgentParserAspNetCoreMeters.ResetForTests();
    }
}
