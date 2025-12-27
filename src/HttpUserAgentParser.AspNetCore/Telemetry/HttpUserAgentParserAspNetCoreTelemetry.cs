// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.Telemetry;

/// <summary>
/// Opt-in switch for AspNetCore package telemetry.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class HttpUserAgentParserAspNetCoreTelemetry
{
    private const int EventCountersFlag = 1;
    private const int MetersFlag = 2;

    private static int s_enabledFlags;

    public static bool IsEnabled => Volatile.Read(ref s_enabledFlags) != 0;

    public static bool AreCountersEnabled
        => (Volatile.Read(ref s_enabledFlags) & EventCountersFlag) != 0
           && HttpUserAgentParserAspNetCoreEventSource.Log.IsEnabled();

    public static bool AreMetersEnabled
        => (Volatile.Read(ref s_enabledFlags) & MetersFlag) != 0
           && HttpUserAgentParserAspNetCoreMeters.IsEnabled;

    /// <summary>
    /// Enables EventCounter telemetry for the AspNetCore package.
    /// </summary>
    public static void Enable() => Interlocked.Or(ref s_enabledFlags, EventCountersFlag);

    /// <summary>
    /// Enables native System.Diagnostics.Metrics telemetry for the AspNetCore package.
    /// </summary>
    public static void EnableMeters(Meter? meter = null)
    {
        HttpUserAgentParserAspNetCoreMeters.Enable(meter);
        Interlocked.Or(ref s_enabledFlags, MetersFlag);
    }

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
}
