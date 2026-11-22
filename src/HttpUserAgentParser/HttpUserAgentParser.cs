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

        if (GetBrowser(userAgent) is { } browser)
        {
            return HttpUserAgentInformation.CreateForBrowser(userAgent, platform, browser.Name, browser.Version, mobileDeviceType);
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
        // Rules are sorted by frequency, so a sequential scan usually stops after a few tokens.
        // (Measured with a mixed real-world workload, this beats vectorized token groups here.)
        ReadOnlySpan<char> ua = userAgent.AsSpan();
        foreach ((string token, HttpUserAgentPlatformInformation platform) in HttpUserAgentRules.Platforms)
        {
            if (ContainsIgnoreCase(ua, token))
            {
                return platform;
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

        // Rules are evaluated in priority order (specific browsers before the browsers their user agents contain).
        foreach ((string name, string detectToken, string? versionToken) in HttpUserAgentRules.Browsers)
        {
            if (!TryIndexOf(ua, detectToken, out int detectIndex))
            {
                continue;
            }

            int versionSearchStart = detectIndex + detectToken.Length;

            if (versionToken is null)
            {
                // For rules without a specific version token, ensure pattern Token/<digits>
                if (versionSearchStart >= ua.Length || ua[versionSearchStart] != '/')
                {
                    // Likely a misspelling or partial token (e.g., Edgg, Oprea, Chromee)
                    continue;
                }
            }
            else if (TryIndexOf(ua, versionToken, out int versionTokenIndex))
            {
                // Version token may differ (e.g., Safari uses "Version/")
                versionSearchStart = versionTokenIndex + versionToken.Length;
            }

            // Otherwise (version token not found) the version is searched after the detect token.
            ReadOnlySpan<char> search = ua[versionSearchStart..];
            if (TryExtractVersion(search, out Range range))
            {
                return (name, search[range].ToString());
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
        (string Key, string Value)[] robots = HttpUserAgentRules.Robots;
#if NET9_0_OR_GREATER
        string[] defaultKeys = HttpUserAgentRules.DefaultRobotKeys;

        // Googlebot, by far the most common robot, comes first: if the entry is unchanged and matches, it wins.
        (string firstKey, string firstValue) = robots[0];
        if (ReferenceEquals(firstKey, defaultKeys[0]) && ContainsIgnoreCase(ua, firstKey))
        {
            return firstValue;
        }

        if (HttpUserAgentRules.AreRobotsUnchanged())
        {
            // Few vectorized passes over the anchors rule out most user agents (browsers) before the full key search.
            if (!HttpUserAgentRules.RobotAnchorMatcher.ContainsAny(ua))
            {
                return null;
            }

            int index = HttpUserAgentRules.RobotMatcher.Find(ua, 1, out _);
            return index >= 0 ? robots[index].Value : null;
        }

        // The public array is mutable and callers replaced entries: default keys are filtered by group,
        // replaced keys are checked one by one, in array order.
        foreach (ref readonly PriorityTokenGroup group in HttpUserAgentRules.RobotMatcher.Groups)
        {
            bool mayMatchDefaultKey = group.MayMatch(ua);
            for (int i = group.Start; i < group.End; i++)
            {
                (string key, string value) = robots[i];
                if ((mayMatchDefaultKey || !ReferenceEquals(key, defaultKeys[i])) && ContainsIgnoreCase(ua, key))
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
        int index = HttpUserAgentRules.MobileMatcher.Find(userAgent, 0, out _);
        return index >= 0 ? HttpUserAgentRules.Mobiles[index].Value : null;
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
