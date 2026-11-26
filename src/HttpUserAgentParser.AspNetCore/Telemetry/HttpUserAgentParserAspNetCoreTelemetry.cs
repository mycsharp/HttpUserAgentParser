// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.Telemetry;

/// <summary>
/// Opt-in switch for AspNetCore package telemetry.
/// </summary>
internal static class HttpUserAgentParserAspNetCoreTelemetry
{
    private const int EventCountersFlag = 1;

    private const int MetersFlag = 2;

    private static int s_enabledFlags;

    public static bool IsEnabled
        => Volatile.Read(ref s_enabledFlags) != 0;

    /// <summary>
    /// Enables EventCounter telemetry for the AspNetCore package.
    /// </summary>
    public static void Enable()
    {
        // Give listeners a chance to subscribe before recording the first event.
        _ = HttpUserAgentParserAspNetCoreEventSource.Log;
        Interlocked.Or(ref s_enabledFlags, EventCountersFlag);
    }

    /// <summary>
    /// Enables native System.Diagnostics.Metrics telemetry for the AspNetCore package.
    /// </summary>
    public static void EnableMeters(Meter? meter = null)
        => HttpUserAgentParserAspNetCoreMeters.Enable(meter);

    /// <summary>
    /// Enables native System.Diagnostics.Metrics telemetry with a meter named <paramref name="meterName"/> that the package owns.
    /// </summary>
    public static void EnableMetersWithName(string meterName)
        => HttpUserAgentParserAspNetCoreMeters.EnableOwnedMeter(meterName);

    internal static void MarkMetersEnabled() => Interlocked.Or(ref s_enabledFlags, MetersFlag);

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

    public static void ResetForTests()
    {
        Volatile.Write(ref s_enabledFlags, 0);
        HttpUserAgentParserAspNetCoreMeters.ResetForTests();
    }
}
