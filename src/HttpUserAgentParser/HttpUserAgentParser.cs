// Copyright © https://myCSharp.de - all rights reserved

using System.Buffers;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using MyCSharp.HttpUserAgentParser.Telemetry;

namespace MyCSharp.HttpUserAgentParser;

#pragma warning disable MA0049 // Type name should not match containing namespace

/// <summary>
/// Parser logic for user agents
/// </summary>
public static class HttpUserAgentParser
{
    private static readonly SearchValues<char> s_versionCharacters = SearchValues.Create("0123456789.");

    /// <summary>
    /// Parses given <param name="userAgent">user agent</param>
    /// </summary>
    /// <remarks>
    /// If telemetry is enabled, this method will emit metrics for parse requests and duration.
    /// The telemetry check is designed to be zero-overhead when disabled (using a volatile boolean check).
    /// </remarks>
    public static HttpUserAgentInformation Parse(string userAgent)
    {
        if (!HttpUserAgentParserTelemetry.IsEnabled)
        {
            return ParseInternal(userAgent);
        }

        bool measureDuration = HttpUserAgentParserTelemetry.ShouldMeasureParseDuration;
        long startTimestamp = measureDuration ? Stopwatch.GetTimestamp() : 0;

        HttpUserAgentParserTelemetry.ParseRequest();

        HttpUserAgentInformation result = ParseInternal(userAgent);

        if (measureDuration)
        {
            HttpUserAgentParserTelemetry.ParseDuration(Stopwatch.GetElapsedTime(startTimestamp));
        }

        return result;
    }

    private static HttpUserAgentInformation ParseInternal(string userAgent)
    {
        // prepare
        userAgent = Cleanup(userAgent);

        // analyze
        if (TryGetRobot(userAgent, out string? robotName))
        {
            return HttpUserAgentInformation.CreateForRobot(userAgent, robotName);
        }

        HttpUserAgentPlatformInformation? platform = GetPlatform(userAgent);
        string? mobileDeviceType = GetMobileDevice(userAgent);

        if (TryGetBrowser(userAgent, out (string Name, string? Version)? browser))
        {
            return HttpUserAgentInformation.CreateForBrowser(userAgent, platform, browser?.Name, browser?.Version, mobileDeviceType);
        }

        return HttpUserAgentInformation.CreateForUnknown(userAgent, platform, mobileDeviceType);
    }

    /// <summary>
    /// pre-cleanup of <param name="userAgent">user agent</param>
    /// </summary>
    public static string Cleanup(string userAgent) => userAgent.Trim();

    /// <summary>
    /// returns the platform or null
    /// </summary>
    public static HttpUserAgentPlatformInformation? GetPlatform(string userAgent)
    {
        ReadOnlySpan<char> ua = userAgent.AsSpan();
        foreach ((string Token, string Name, HttpUserAgentPlatformType PlatformType) in HttpUserAgentStatics.s_platformRules)
        {
            if (ContainsIgnoreCase(ua, Token))
            {
                return new HttpUserAgentPlatformInformation(
                    HttpUserAgentStatics.GetPlatformRegexForToken(Token),
                    Name, PlatformType);
            }
        }

        return null;
    }

    /// <summary>
    /// returns true if platform was found
    /// </summary>
    public static bool TryGetPlatform(string userAgent, [NotNullWhen(true)] out HttpUserAgentPlatformInformation? platform)
    {
        platform = GetPlatform(userAgent);
        return platform is not null;
    }

    /// <summary>
    /// returns the browser or null
    /// </summary>
    public static (string Name, string? Version)? GetBrowser(string userAgent)
    {
        ReadOnlySpan<char> ua = userAgent.AsSpan();

        foreach ((string Name, string DetectToken, string? VersionToken) in HttpUserAgentStatics.s_browserRules)
        {
            if (!TryIndexOf(ua, DetectToken, out int detectIndex))
            {
                continue;
            }

            // Version token may differ (e.g., Safari uses "Version/")

            int versionSearchStart;
            // For rules without a specific version token, ensure pattern Token/<digits>
            if (string.IsNullOrEmpty(VersionToken))
            {
                int afterDetect = detectIndex + DetectToken.Length;
                if (afterDetect >= ua.Length || ua[afterDetect] != '/')
                {
                    // Likely a misspelling or partial token (e.g., Edgg, Oprea, Chromee)
                    continue;
                }
            }
            if (!string.IsNullOrEmpty(VersionToken))
            {
                if (TryIndexOf(ua, VersionToken!, out int vtIndex))
                {
                    versionSearchStart = vtIndex + VersionToken!.Length;
                }
                else
                {
                    // If specific version token wasn't found, fall back to detect token area
                    versionSearchStart = detectIndex + DetectToken.Length;
                }
            }
            else
            {
                versionSearchStart = detectIndex + DetectToken.Length;
            }

            ReadOnlySpan<char> search = ua.Slice(versionSearchStart);
            if (TryExtractVersion(search, out Range range))
            {
                string? version = search[range].ToString();
                return (Name, version);
            }

            // If we didn't find a version for this rule, try next rule
        }

        return null;
    }

    /// <summary>
    /// returns true if browser was found
    /// </summary>
    public static bool TryGetBrowser(string userAgent, [NotNullWhen(true)] out (string Name, string? Version)? browser)
    {
        browser = GetBrowser(userAgent);
        return browser is not null;
    }

    /// <summary>
    /// returns the robot or null
    /// </summary>
    public static string? GetRobot(string userAgent)
    {
        ReadOnlySpan<char> ua = userAgent.AsSpan();
        (string Key, string Value)[] robots = HttpUserAgentStatics.Robots;
#if NET9_0_OR_GREATER
        (string firstKey, string firstValue) = robots[0];
        if (ContainsIgnoreCase(ua, firstKey))
        {
            return firstValue;
        }

        int index = 1;
        foreach (SearchValues<string> group in HttpUserAgentStatics.s_robotTokenGroups)
        {
            bool containsDefaultRobot = ua.ContainsAny(group);
            int end = Math.Min(index + HttpUserAgentStatics.TokenGroupSize, robots.Length);
            for (; index < end; index++)
            {
                (string key, string value) = robots[index];

                // The public array is mutable. Check any keys callers have replaced.
                if ((containsDefaultRobot || !ReferenceEquals(key, HttpUserAgentStatics.s_defaultRobotKeys[index]))
                    && ContainsIgnoreCase(ua, key))
                {
                    return value;
                }
            }
        }
#else
        foreach ((string key, string value) in robots)
        {
            if (ContainsIgnoreCase(ua, key))
            {
                return value;
            }
        }
#endif

        return null;
    }

    /// <summary>
    /// returns true if robot was found
    /// </summary>
    public static bool TryGetRobot(string userAgent, [NotNullWhen(true)] out string? robotName)
    {
        robotName = GetRobot(userAgent);
        return robotName is not null;
    }

    /// <summary>
    /// returns the device or null
    /// </summary>
    public static string? GetMobileDevice(string userAgent)
    {
        ReadOnlySpan<char> ua = userAgent.AsSpan();
#if NET9_0_OR_GREATER
        ReadOnlySpan<string> keys = HttpUserAgentStatics.Mobiles.Keys.AsSpan();
        ReadOnlySpan<string> values = HttpUserAgentStatics.Mobiles.Values.AsSpan();
        for (int i = 0; i < HttpUserAgentStatics.s_mobileTokenGroups.Length; i++)
        {
            if (!ua.ContainsAny(HttpUserAgentStatics.s_mobileTokenGroups[i]))
            {
                continue;
            }

            int start = i * HttpUserAgentStatics.TokenGroupSize;
            int end = Math.Min(start + HttpUserAgentStatics.TokenGroupSize, keys.Length);
            for (int j = start; j < end; j++)
            {
                if (ContainsIgnoreCase(ua, keys[j]))
                {
                    return values[j];
                }
            }
        }
#else
        foreach ((string key, string value) in HttpUserAgentStatics.Mobiles)
        {
            if (ContainsIgnoreCase(ua, key))
            {
                return value;
            }
        }
#endif

        return null;
    }

    /// <summary>
    /// returns true if device was found
    /// </summary>
    public static bool TryGetMobileDevice(string userAgent, [NotNullWhen(true)] out string? device)
    {
        device = GetMobileDevice(userAgent);
        return device is not null;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ContainsIgnoreCase(ReadOnlySpan<char> haystack, ReadOnlySpan<char> needle)
        => TryIndexOf(haystack, needle, out _);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryIndexOf(ReadOnlySpan<char> haystack, ReadOnlySpan<char> needle, out int index)
    {
        index = haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase);
        return index >= 0;
    }

    /// <summary>
    /// Finds the first ASCII digit and reads subsequent digits and dots within the search window.
    /// </summary>
    private static bool TryExtractVersion(ReadOnlySpan<char> haystack, out Range range)
    {
        const int SearchWindow = 128;
        haystack = haystack[..Math.Min(haystack.Length, SearchWindow)];

        int start = haystack.IndexOfAnyInRange('0', '9');
        if (start < 0)
        {
            range = default;
            return false;
        }

        ReadOnlySpan<char> version = haystack[start..];
        int length = version.IndexOfAnyExcept(s_versionCharacters);
        range = new Range(start, length < 0 ? haystack.Length : start + length);
        return true;
    }
}
