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
    /// Characters that end a product token ("product/version") or a comment entry of a user agent.
    /// </summary>
    private static readonly SearchValues<char> s_productTokenDelimiters = SearchValues.Create(" \t;(),");

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
            if (TryIndexOf(ua, detectToken, out int detectIndex)
                && TryReadBrowserVersion(ua, detectToken, detectIndex, versionToken, out Range version))
            {
                return (name, ua[version].ToString());
            }

            // Token not found or without version: try the next rule
        }

        return null;
    }

    /// <summary>
    /// Reads the version of a browser rule whose detect token occurs first at <paramref name="detectIndex"/>.
    /// </summary>
    private static bool TryReadBrowserVersion(ReadOnlySpan<char> ua, string detectToken, int detectIndex, string? versionToken, out Range version)
    {
        if (versionToken is null)
        {
            // Pattern Token/<version>. Without '/' the token is likely a misspelling or part of another word (e.g. Edgg, Oprea, Chromee).
            return TryReadVersionAfter(ua, detectToken, detectIndex, slashRequired: true, out version);
        }

        // A separate version token (e.g. Safari "Version/") only counts at the start of a token:
        // in "VendorVersion/120.0" or "Vendor-Version/120.0" the version belongs to another product.
        bool sameToken = string.Equals(detectToken, versionToken, StringComparison.Ordinal);
        int versionIndex = IndexOfTokenStart(ua, versionToken, sameToken ? detectIndex : 0);
        if (versionIndex >= 0)
        {
            return TryReadVersionAfter(ua, versionToken, versionIndex, slashRequired: false, out version);
        }

        // Without version token the version follows the detect token (e.g. "Opera/9.80"), unless both are the same.
        if (sameToken)
        {
            version = default;
            return false;
        }

        return TryReadVersionAfter(ua, detectToken, detectIndex, slashRequired: false, out version);
    }

    /// <summary>
    /// Reads the version after the occurrence of <paramref name="token"/> at <paramref name="index"/> or,
    /// if that one has none, after a later occurrence that starts a token.
    /// </summary>
    /// <remarks>
    /// The given occurrence may continue a word, as before (<c>HeadlessChrome/120</c> is Chrome); later occurrences must start
    /// a token, so <c>MyChrome/2.0</c> does not count. The search continues behind the product token examined before, because
    /// a token cannot start inside it; so every character is examined once, independent of the number of occurrences.
    /// </remarks>
    private static bool TryReadVersionAfter(ReadOnlySpan<char> ua, string token, int index, bool slashRequired, out Range version)
    {
        do
        {
            int end = index + token.Length;
            int next = index + 1;
            if (!slashRequired || (end < ua.Length && ua[end] == '/'))
            {
                if (TryExtractVersion(ua[end..], out Range range, out int productTokenLength))
                {
                    version = new Range(end + range.Start.Value, end + range.End.Value);
                    return true;
                }

                next = end + productTokenLength;
            }

            index = IndexOfTokenStart(ua, token, next);
        }
        while (index >= 0);

        version = default;
        return false;
    }

    /// <summary>
    /// Returns the index of the first occurrence of <paramref name="token"/> at or after <paramref name="start"/>
    /// that starts a token (at the beginning or after a delimiter), or -1.
    /// </summary>
    private static int IndexOfTokenStart(ReadOnlySpan<char> ua, string token, int start)
    {
        while (true)
        {
            int offset = ua[start..].IndexOf(token, StringComparison.OrdinalIgnoreCase);
            if (offset < 0)
            {
                return -1;
            }

            int index = start + offset;
            if (index == 0 || s_productTokenDelimiters.Contains(ua[index - 1]))
            {
                return index;
            }

            start = index + 1;
        }
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
        string[] defaultKeys = HttpUserAgentRules.DefaultRobotKeys;

        // Googlebot, by far the most common robot, comes first: if the entry is unchanged and matches, it wins.
        (string firstKey, string firstValue) = robots[0];
        if (ReferenceEquals(firstKey, defaultKeys[0]) && ContainsIgnoreCase(ua, firstKey))
        {
            return firstValue;
        }

        if (HttpUserAgentRules.AreRobotKeysUnchanged())
        {
            // The few anchors rule out most user agents (browsers) before the full key search.
            if (!HttpUserAgentRules.RobotAnchorMatcher.ContainsAny(ua))
            {
                return null;
            }

            int index = HttpUserAgentRules.RobotMatcher.Find(ua, 1, out _);
            return index >= 0 ? robots[index].Value : null;
        }

        // The public array is mutable and callers replaced keys: they are checked one by one, in array order.
#if NET9_0_OR_GREATER
        // Default keys are filtered by group.
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
    /// Finds the first ASCII digit and reads subsequent digits and dots within the current product token.
    /// </summary>
    /// <remarks>
    /// The search ends at the next delimiter, so an empty version never takes the version of the next product
    /// (<c>SamsungBrowser/ Chrome/131.0</c> has no Samsung Internet version).
    /// </remarks>
    private static bool TryExtractVersion(ReadOnlySpan<char> haystack, out Range range, out int productTokenLength)
    {
        int end = haystack.IndexOfAny(s_productTokenDelimiters);
        productTokenLength = end < 0 ? haystack.Length : end;
        haystack = haystack[..productTokenLength];

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
