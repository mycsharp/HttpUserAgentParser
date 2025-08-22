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
    public static HttpUserAgentInformation Parse(ReadOnlySpan<char> userAgent)
    {
        // prepare
        userAgent = Cleanup(userAgent);

        // analyze
        if (TryGetRobot(userAgent, out string? robotName))
        {
            return HttpUserAgentInformation.CreateForRobot(userAgent.ToString(), robotName);
        }

        HttpUserAgentPlatformInformation? platform = GetPlatform(userAgent);
        string? mobileDeviceType = GetMobileDevice(userAgent);

        if (TryGetBrowser(userAgent, out (string Name, string? Version)? browser))
        {
            return HttpUserAgentInformation.CreateForBrowser(userAgent.ToString(), platform, browser?.Name, browser?.Version, mobileDeviceType);
        }

        return HttpUserAgentInformation.CreateForUnknown(userAgent.ToString(), platform, mobileDeviceType);
    }

    /// <summary>
    /// pre-cleanup of <param name="userAgent">user agent</param>
    /// </summary>
    public static ReadOnlySpan<char> Cleanup(ReadOnlySpan<char> userAgent) => userAgent.Trim();

    /// <summary>
    /// returns the platform or null
    /// </summary>
    public static HttpUserAgentPlatformInformation? GetPlatform(ReadOnlySpan<char> userAgent)
    {
        foreach ((string Token, string Name, HttpUserAgentPlatformType PlatformType) p in HttpUserAgentStatics.s_platformRules)
        {
            if (ContainsIgnoreCase(userAgent, p.Token))
            {
                return new HttpUserAgentPlatformInformation(
                regex: HttpUserAgentStatics.GetPlatformRegexForToken(p.Token),
                name: p.Name,
                platformType: p.PlatformType);
            }
        }

        return null;
    }

    /// <summary>
    /// returns true if platform was found
    /// </summary>
    public static bool TryGetPlatform(ReadOnlySpan<char> userAgent, [NotNullWhen(true)] out HttpUserAgentPlatformInformation? platform)
    {
        platform = GetPlatform(userAgent);
        return platform is not null;
    }

    /// <summary>
    /// returns the browser or null
    /// </summary>
    public static (string Name, string? Version)? GetBrowser(ReadOnlySpan<char> userAgent)
    {
        foreach ((string Name, string DetectToken, string? VersionToken) rule in HttpUserAgentStatics.s_browserRules)
        {
            if (!TryIndexOf(userAgent, rule.DetectToken, out int detectIndex))
            {
                continue;
            }

            // Version token may differ (e.g., Safari uses "Version/")
            int versionSearchStart = detectIndex;
            if (!string.IsNullOrEmpty(rule.VersionToken))
            {
                if (TryIndexOf(userAgent, rule.VersionToken!, out int vtIndex))
                {
                    versionSearchStart = vtIndex + rule.VersionToken!.Length;
                }
                else
                {
                    // If specific version token wasn't found, fall back to detect token area
                    versionSearchStart = detectIndex + rule.DetectToken.Length;
                }
            }
            else
            {
                versionSearchStart = detectIndex + rule.DetectToken.Length;
            }

            string? version = null;
            if (TryExtractVersion(userAgent, versionSearchStart, out Range range))
            {
                version = userAgent.ToString();
            }

            return (rule.Name, version);
        }

        return null;
    }

    /// <summary>
    /// returns true if browser was found
    /// </summary>
    public static bool TryGetBrowser(ReadOnlySpan<char> userAgent, [NotNullWhen(true)] out (string Name, string? Version)? browser)
    {
        browser = GetBrowser(userAgent);
        return browser is not null;
    }

    /// <summary>
    /// returns the robot or null
    /// </summary>
    public static string? GetRobot(ReadOnlySpan<char> userAgent)
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
    public static bool TryGetRobot(ReadOnlySpan<char> userAgent, [NotNullWhen(true)] out string? robotName)
    {
        robotName = GetRobot(userAgent);
        return robotName is not null;
    }

    /// <summary>
    /// returns the device or null
    /// </summary>
    public static string? GetMobileDevice(ReadOnlySpan<char> userAgent)
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
    public static bool TryGetMobileDevice(ReadOnlySpan<char> userAgent, [NotNullWhen(true)] out string? device)
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
