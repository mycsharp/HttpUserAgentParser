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
    /// A token starts at the beginning of the user agent or after one of them; <c>+</c> stands for a space in URL-encoded logs.
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
        => Parse(userAgent, HttpUserAgentRules.RobotSet);

    internal static HttpUserAgentInformation Parse(string userAgent, HttpUserAgentRobotSet robots)
    {
        if (!HttpUserAgentParserTelemetry.IsEnabled)
        {
            return ParseInternal(userAgent, robots);
        }

        bool measureDuration = HttpUserAgentParserTelemetry.ShouldMeasureParseDuration;
        long startTimestamp = measureDuration ? Stopwatch.GetTimestamp() : 0;

        HttpUserAgentParserTelemetry.ParseRequest();

        HttpUserAgentInformation result = ParseInternal(userAgent, robots);

        if (measureDuration)
        {
            HttpUserAgentParserTelemetry.ParseDuration(Stopwatch.GetElapsedTime(startTimestamp));
        }

        return result;
    }

    private static HttpUserAgentInformation ParseInternal(string userAgent, HttpUserAgentRobotSet robots)
    {
        // prepare
        userAgent = Cleanup(userAgent);

        // analyze
        if (GetRobot(userAgent, robots) is { } robotName)
        {
            return HttpUserAgentInformation.CreateForRobot(userAgent, robotName);
        }

        return DetectProducts(userAgent, AnalyzeProducts);
    }

    private static HttpUserAgentInformation AnalyzeProducts(string userAgent, ReadOnlySpan<char> ua)
    {
        HttpUserAgentPlatformInformation? platform = FindPlatform(ua);
        string? mobileDeviceType = FindMobileDevice(ua);

        if (FindBrowser(ua) is { } browser)
        {
            return HttpUserAgentInformation.CreateForBrowser(userAgent, platform, browser.Name, browser.Version, mobileDeviceType);
        }

        return HttpUserAgentInformation.CreateForUnknown(userAgent, platform, mobileDeviceType);
    }

    private delegate T ProductDetector<T>(string original, ReadOnlySpan<char> products);

    // IIS logs use '+' for spaces, including spaces inside product names. Keep the original
    // string in the result and use the decoded span for all three product detectors.
    private static T DetectProducts<T>(string userAgent, ProductDetector<T> detect)
    {
        ReadOnlySpan<char> ua = userAgent.AsSpan();
        if (!ua.Contains('+'))
        {
            return detect(userAgent, ua);
        }

        if (ua.Length <= 512)
        {
            Span<char> decoded = stackalloc char[ua.Length];
            ua.Replace(decoded, '+', ' ');
            return detect(userAgent, decoded);
        }

        char[] buffer = ArrayPool<char>.Shared.Rent(ua.Length);
        try
        {
            Span<char> decoded = buffer.AsSpan(0, ua.Length);
            ua.Replace(decoded, '+', ' ');
            return detect(userAgent, decoded);
        }
        finally
        {
            ArrayPool<char>.Shared.Return(buffer);
        }
    }

    /// <summary>
    /// pre-cleanup of <param name="userAgent">user agent</param>
    /// </summary>
    public static string Cleanup(string userAgent) => userAgent.Trim();

    /// <summary>
    /// returns the platform or null
    /// </summary>
    public static HttpUserAgentPlatformInformation? GetPlatform(string userAgent)
        => DetectProducts(userAgent, static (_, ua) => FindPlatform(ua));

    private static HttpUserAgentPlatformInformation? FindPlatform(ReadOnlySpan<char> ua)
    {
        // Rules are sorted by frequency, so a sequential scan usually stops after a few tokens.
        // (Measured with a mixed real-world workload, this beats vectorized token groups here.)
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
    /// <remarks>
    /// Rules are evaluated in priority order (specific browsers before the browsers their user agents contain); the first rule
    /// with a version wins. Browser and version tokens only count at the start of a token, i.e. at the beginning of the user agent
    /// or after a space, tab, <c>+</c>, <c>;</c>, <c>(</c>, <c>)</c> or <c>,</c>.
    /// </remarks>
    public static (string Name, string? Version)? GetBrowser(string userAgent)
        => DetectProducts(userAgent, static (_, ua) => FindBrowser(ua));

    private static (string Name, string? Version)? FindBrowser(ReadOnlySpan<char> ua)
    {
        foreach ((string name, string detectToken, string? versionToken) in HttpUserAgentRules.Browsers)
        {
            // Fast path: most rules do not match at all, and a match usually starts a token.
            int detectIndex = ua.IndexOf(detectToken, StringComparison.OrdinalIgnoreCase);
            if (detectIndex < 0)
            {
                continue;
            }

            if (!IsTokenStart(ua, detectIndex))
            {
                detectIndex = IndexOfTokenStart(ua, detectToken, detectIndex + 1);
                if (detectIndex < 0)
                {
                    continue;
                }
            }

            if (TryReadBrowserVersion(ua, detectToken, detectIndex, versionToken, out Range version))
            {
                return (name, ua[version].ToString());
            }
        }

        return null;
    }

    /// <summary>
    /// Reads the version of a browser rule whose detect token starts a token at <paramref name="detectIndex"/> first.
    /// </summary>
    private static bool TryReadBrowserVersion(ReadOnlySpan<char> ua, string detectToken, int detectIndex, string? versionToken, out Range version)
    {
        if (versionToken is null)
        {
            // "Token/1.2". Without '/' the token is likely a misspelling or another product (e.g. Edgg, Oprea, ChromeHelper).
            return TryReadVersion(ua, detectToken, detectIndex, allowSpace: false, out version);
        }

        // A separate version token after the detect token ("Opera/9.80 ... Version/12.16", "Trident/7.0; rv:11.0"); with the same
        // token as the detect token (Safari "Version/", "MSIE 6.0") the detect token has been found already.
        if (string.Equals(detectToken, versionToken, StringComparison.Ordinal))
        {
            return TryReadVersion(ua, detectToken, detectIndex, allowSpace: true, out version);
        }

        // A separate version belongs to a complete product name. Skip prefixes such as TridentSpider.
        while (detectIndex >= 0)
        {
            int end = detectIndex + detectToken.Length;
            if (end == ua.Length || ua[end] == '/' || s_productTokenDelimiters.Contains(ua[end]))
            {
                int versionIndex = IndexOfTokenStart(ua, versionToken, end);
                if (versionIndex >= 0 && TryReadVersion(ua, versionToken, versionIndex, allowSpace: true, out version))
                {
                    return true;
                }

                return TryReadVersion(ua, detectToken, detectIndex, allowSpace: true, out version);
            }

            detectIndex = IndexOfTokenStart(ua, detectToken, end);
        }

        version = default;
        return false;
    }

    /// <summary>
    /// Reads the version after the occurrence of <paramref name="token"/> at <paramref name="index"/> or,
    /// if that one has none, after a later occurrence that starts a token.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The version follows the token as <c>Token/1.2</c> (the first digits of the product token, which ends at the next
    /// delimiter), directly after a token that ends with its separator (<c>Version/1.2</c>, <c>rv:11.0</c>) or, if
    /// <paramref name="allowSpace"/> is set, as <c>Token 1.2</c> (<c>MSIE 6.0</c>, <c>Opera 9.50</c>, also with <c>+</c>).
    /// </para>
    /// <para>
    /// The search continues behind the characters examined before, where no token can start; so every character is
    /// examined a constant number of times, independent of the number of occurrences.
    /// </para>
    /// </remarks>
    private static bool TryReadVersion(ReadOnlySpan<char> ua, string token, int index, bool allowSpace, out Range version)
    {
        bool separatorIncluded = token[^1] is '/' or ':';
        do
        {
            int end = index + token.Length;
            int next = index + 1;
            if (separatorIncluded || (end < ua.Length && ua[end] == '/'))
            {
                if (TryExtractVersion(ua[end..], out Range range, out int productTokenLength))
                {
                    version = new Range(end + range.Start.Value, end + range.End.Value);
                    return true;
                }

                next = end + productTokenLength;
            }
            else if (allowSpace && end < ua.Length && IsSpace(ua[end]))
            {
                int versionStart = end + 1;
                while (versionStart < ua.Length && IsSpace(ua[versionStart]))
                {
                    versionStart++;
                }

                if (versionStart < ua.Length && char.IsAsciiDigit(ua[versionStart]))
                {
                    int length = ua[versionStart..].IndexOfAnyExcept(s_versionCharacters);
                    version = new Range(versionStart, length < 0 ? ua.Length : versionStart + length);
                    return true;
                }

                next = versionStart;
            }

            index = IndexOfTokenStart(ua, token, next);
        }
        while (index >= 0);

        version = default;
        return false;
    }

    /// <summary>
    /// Returns whether a token at <paramref name="index"/> starts a token of its own (at the beginning or after a delimiter).
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsTokenStart(ReadOnlySpan<char> ua, int index)
        => index == 0 || s_productTokenDelimiters.Contains(ua[index - 1]);

    /// <summary>
    /// Returns whether <paramref name="c"/> separates a product name from its version in <c>Token 1.2</c>
    /// (<c>+</c> stands for a space in URL-encoded logs).
    /// </summary>
    private static bool IsSpace(char c) => c is ' ' or '\t';

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
            if (IsTokenStart(ua, index))
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
        => GetRobot(userAgent, HttpUserAgentRules.RobotSet);

    private static string? GetRobot(string userAgent, HttpUserAgentRobotSet robots)
    {
        // The first robot (Googlebot in the default list, by far the most common robot) is checked on its own.
        if (robots.FirstKey is { } firstKey && ContainsIgnoreCase(userAgent, firstKey))
        {
            return robots.FirstValue;
        }

        return FindRobot(userAgent, robots);
    }

    /// <summary>
    /// Finds the first robot after the first one; separate from <see cref="GetRobot(string)"/>, so its check of the first robot stays small.
    /// </summary>
    private static string? FindRobot(ReadOnlySpan<char> ua, HttpUserAgentRobotSet robots)
    {
        // The few anchors rule out most user agents (browsers) before the full key search.
        if (!robots.AnchorMatcher.ContainsAny(ua))
        {
            return null;
        }

        int index = robots.Matcher.Find(ua, 1, out _);
        return index >= 0 ? robots.Entries[index].Value : null;
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
        => DetectProducts(userAgent, static (_, ua) => FindMobileDevice(ua));

    private static string? FindMobileDevice(ReadOnlySpan<char> ua)
    {
        int index = HttpUserAgentRules.MobileMatcher.Find(ua, 0, out _);
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
