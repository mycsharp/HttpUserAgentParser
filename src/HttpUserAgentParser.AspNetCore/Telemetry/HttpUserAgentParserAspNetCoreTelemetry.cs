// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.CodeAnalysis;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.Telemetry;

/// <summary>
/// Opt-in switch for AspNetCore package telemetry.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class HttpUserAgentParserAspNetCoreTelemetry
{
    private static volatile bool s_enabled;

    public static bool IsEnabled => s_enabled;

    public static bool AreCountersEnabled => s_enabled && HttpUserAgentParserAspNetCoreEventSource.Log.IsEnabled();

    public static void Enable() => s_enabled = true;
}
