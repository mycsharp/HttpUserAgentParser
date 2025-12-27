// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.Telemetry;

/// <summary>
/// System.Diagnostics.Metrics instruments emitted by MyCSharp.HttpUserAgentParser.AspNetCore.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class HttpUserAgentParserAspNetCoreMeters
{
    public const string MeterName = "MyCSharp.HttpUserAgentParser.AspNetCore";

    private static int s_initialized;

    private static Meter? s_meter;
    private static Counter<long>? s_userAgentPresent;
    private static Counter<long>? s_userAgentMissing;

    public static bool IsEnabled => Volatile.Read(ref s_initialized) != 0;

    public static void Enable(Meter? meter = null)
    {
        if (Interlocked.Exchange(ref s_initialized, 1) == 1)
        {
            return;
        }

        s_meter = meter ?? new Meter(MeterName);

        s_userAgentPresent = s_meter.CreateCounter<long>(
            name: "useragent-present",
            unit: "calls",
            description: "User-Agent header present");

        s_userAgentMissing = s_meter.CreateCounter<long>(
            name: "useragent-missing",
            unit: "calls",
            description: "User-Agent header missing");
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UserAgentPresent() => s_userAgentPresent?.Add(1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UserAgentMissing() => s_userAgentMissing?.Add(1);
}
