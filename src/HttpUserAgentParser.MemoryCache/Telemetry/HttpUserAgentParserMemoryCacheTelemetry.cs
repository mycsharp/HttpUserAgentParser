// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.CodeAnalysis;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.Telemetry;

/// <summary>
/// Opt-in switch for MemoryCache package telemetry.
/// </summary>
[ExcludeFromCodeCoverage]
internal static class HttpUserAgentParserMemoryCacheTelemetry
{
    private static volatile bool s_enabled;

    public static bool IsEnabled => s_enabled;

    public static bool AreCountersEnabled => s_enabled && HttpUserAgentParserMemoryCacheEventSource.Log.IsEnabled();

    public static void Enable() => s_enabled = true;
}
