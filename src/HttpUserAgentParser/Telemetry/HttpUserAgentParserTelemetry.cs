// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.CodeAnalysis;

namespace MyCSharp.HttpUserAgentParser.Telemetry;

/// <summary>
/// Opt-in switch for core telemetry.
/// Telemetry is disabled by default to ensure zero overhead unless explicitly enabled.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class HttpUserAgentParserTelemetry
{
    private static volatile bool s_enabled;

    public static bool IsEnabled => s_enabled;

    public static bool AreCountersEnabled => s_enabled && HttpUserAgentParserEventSource.Log.IsEnabled();

    public static void Enable() => s_enabled = true;
}
