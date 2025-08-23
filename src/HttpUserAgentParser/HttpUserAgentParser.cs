// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace MyCSharp.HttpUserAgentParser;

#pragma warning disable MA0049 // Type name should not match containing namespace

/// <summary>
/// Parser logic for user agents
/// </summary>
public static class HttpUserAgentParser

{
    /// <summary>
    /// Parses given <param name="userAgent">user agent</param>
    /// </summary>
    public static HttpUserAgentInformation Parse(string userAgent)
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
        // Fast, allocation-free token scan (keeps public statics untouched)
        ReadOnlySpan<char> ua = userAgent.AsSpan();
        foreach ((string Token, string Name, HttpUserAgentPlatformType PlatformType) platform in HttpUserAgentStatics.s_platformRules)
        {
            if (ContainsIgnoreCase(ua, platform.Token))
            {
                return new HttpUserAgentPlatformInformation(
                    HttpUserAgentStatics.GetPlatformRegexForToken(platform.Token),
                    platform.Name, platform.PlatformType);
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
        foreach ((string Name, string DetectToken, string? VersionToken) browserRule in HttpUserAgentStatics.s_browserRules)
        {
            if (!TryIndexOf(ua, browserRule.DetectToken, out int detectIndex))
            {
                continue;
            }

            // Version token may differ (e.g., Safari uses "Version/")
            int versionSearchStart = detectIndex;
            if (!string.IsNullOrEmpty(browserRule.VersionToken))
            {
                if (TryIndexOf(ua, browserRule.VersionToken!, out int vtIndex))
                {
                    versionSearchStart = vtIndex + browserRule.VersionToken!.Length;
                }
                else
                {
                    // If specific version token wasn't found, fall back to detect token area
                    versionSearchStart = detectIndex + browserRule.DetectToken.Length;
                }
            }
            else
            {
                versionSearchStart = detectIndex + browserRule.DetectToken.Length;
            }

            string? version = null;
            if (TryExtractVersion(ua, versionSearchStart, out Range range))
            {
                version = userAgent.AsSpan(range.Start.Value, range.End.Value - range.Start.Value).ToString();
            }

            return (browserRule.Name, version);
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
        foreach ((string key, string value) in HttpUserAgentStatics.Robots)
        {
            if (userAgent.Contains(key, StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }
        }

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
        foreach ((string key, string value) in HttpUserAgentStatics.Mobiles)
        {
            if (userAgent.Contains(key, StringComparison.OrdinalIgnoreCase))
            {
                return value;
            }
        }

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
    private static bool ContainsIgnoreCase(ReadOnlySpan<char> haystack, string needle)
        => TryIndexOf(haystack, needle, out _);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryIndexOf(ReadOnlySpan<char> haystack, string needle, out int index)
    {
        index = haystack.IndexOf(needle.AsSpan(), StringComparison.OrdinalIgnoreCase);
        return index >= 0;
    }

    /// <summary>
    /// Extracts a dotted numeric version starting at or after <paramref name="startIndex"/>.
    /// Accepts digits and dots; skips common separators ('/', ' ', ':', '=') until first digit.
    /// Returns false if no version-like token is found.
    /// </summary>
    private static bool TryExtractVersion(ReadOnlySpan<char> haystack, int startIndex, out Range range)
    {
        range = default;
        if ((uint)startIndex >= (uint)haystack.Length)
        {
            return false;
        }

        // Limit search window to avoid scanning entire UA string unnecessarily
        const int window = 128;
        int end = Math.Min(haystack.Length, startIndex + window);
        int i = startIndex;

        // Skip separators until we hit a digit
        while (i < end)
        {
            char c = haystack[i];
            if ((uint)(c - '0') <= 9)
            {
                break;
            }
            i++;
        }

        if (i >= end)
        {
            return false;
        }

        int s = i;
        while (i < end)
        {
            char c = haystack[i];
            if (!((uint)(c - '0') <= 9 || c == '.'))
            {
                break;
            }
            i++;
        }

        if (i == s)
        {
            return false;
        }

        range = new Range(s, i);
        return true;
    }
}
