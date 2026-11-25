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
    /// When telemetry is enabled, the parser records requests and measures duration for active listeners.
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
        userAgent = Cleanup(userAgent);
        return robots.MatchFirst(userAgent) is { } first
            ? HttpUserAgentInformation.CreateForRobot(userAgent, first)
            : Detect(userAgent, robots, AnalyzeUserAgent);
    }

    private static HttpUserAgentInformation AnalyzeUserAgent(string userAgent, ReadOnlySpan<char> ua, HttpUserAgentRobotSet robots)
    {
        UserAgentRuleIndex.RuleMasks candidates = UserAgentRuleIndex.BuiltIn.Find(ua);
        ulong robotMask = ReferenceEquals(robots, HttpUserAgentRules.DefaultRobotSet) ? candidates.Robots : ulong.MaxValue;
        if (robots.MatchAfterFirst(userAgent, ua, robotMask) is { } robotName)
        {
            return HttpUserAgentInformation.CreateForRobot(userAgent, robotName);
        }

        HttpUserAgentPlatformInformation? platform = FindPlatform(ua, candidates.Platforms);
        string? mobileDeviceType = FindMobileDevice(ua, candidates.Devices);

        if (FindBrowser(ua, candidates.Browsers) is { } browser)
        {
            return HttpUserAgentInformation.CreateForBrowser(userAgent, platform, browser.Name, browser.Version, mobileDeviceType);
        }

        return HttpUserAgentInformation.CreateForUnknown(userAgent, platform, mobileDeviceType);
    }

    private delegate T Detector<TState, T>(string original, ReadOnlySpan<char> decoded, TState state);
    private delegate T ProductDetector<T>(ReadOnlySpan<char> products);

    private static T DetectProduct<T>(string userAgent, ProductDetector<T> detector)
        => Detect(userAgent, detector, static (_, ua, detect) => detect(ua));

    // IIS logs use '+' for spaces, including spaces inside product names. Keep the original
    // string in the result and share the decoded span across all detectors.
    private static T Detect<TState, T>(string userAgent, TState state, Detector<TState, T> detect)
    {
        ReadOnlySpan<char> ua = userAgent.AsSpan();
        if (!ua.Contains('+'))
        {
            return detect(userAgent, ua, state);
        }

        // 512 chars are 1 KiB of stack. The parser runs at whatever stack depth the caller has already
        // reached (recursion, threads with small stacks), so the buffer must stay small. Real user agents
        // are far shorter; longer headers rent from the ArrayPool.
        // For robustness the stack buffer has a constant size and is sliced to the input: its stack usage
        // cannot grow with the input, even if the length check is changed later.
        const int MaxStackLength = 512;
        if (ua.Length <= MaxStackLength)
        {
            Span<char> decoded = stackalloc char[MaxStackLength];
            decoded = decoded[..ua.Length];
            ua.Replace(decoded, '+', ' ');
            return detect(userAgent, decoded, state);
        }

        char[] rented = ArrayPool<char>.Shared.Rent(ua.Length);
        Span<char> rentedSpan = rented.AsSpan(0, ua.Length);
        ua.Replace(rentedSpan, '+', ' ');
        T result = detect(userAgent, rentedSpan, state);
        ArrayPool<char>.Shared.Return(rented);
        return result;
    }

    /// <summary>
    /// pre-cleanup of <param name="userAgent">user agent</param>
    /// </summary>
    public static string Cleanup(string userAgent) => userAgent.Trim();

    /// <summary>
    /// returns the platform or null
    /// </summary>
    public static HttpUserAgentPlatformInformation? GetPlatform(string userAgent)
        => DetectProduct(userAgent, static ua => FindPlatform(ua, ulong.MaxValue));

    private static HttpUserAgentPlatformInformation? FindPlatform(ReadOnlySpan<char> ua, ulong mask)
    {
        (string Token, HttpUserAgentPlatformInformation Platform)[] rules = HttpUserAgentRules.Platforms;
        UserAgentRuleIndex.Candidates candidates = new(mask, rules.Length);
        for (int i = candidates.Next(); i >= 0; i = candidates.Next())
        {
            (string token, HttpUserAgentPlatformInformation platform) = rules[i];
            Debug.Assert(token.Length > 0);
            int index = ua.IndexOf(token, StringComparison.OrdinalIgnoreCase);
            while (index >= 0)
            {
                int end = index + token.Length;
                bool matches = platform.RegexId switch
                {
                    // Short names can occur inside other words such as "Microsoft" or "Magnum".
                    HttpUserAgentPlatformRegexId.CrOS or HttpUserAgentPlatformRegexId.Gnu or HttpUserAgentPlatformRegexId.OsX
                        => IsTokenStart(ua, index) && IsProductNameEnd(ua, end),
                    // NT 6.1.7601 is Windows 7, while NT 6.10 is a different version.
                    _ when char.IsAsciiDigit(token[^1]) => end == ua.Length || !char.IsAsciiDigit(ua[end]),
                    _ => true,
                };
                if (matches)
                {
                    return platform;
                }

                index = IndexOfTokenStart(ua, token, end);
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
        => DetectProduct(userAgent, static ua => FindBrowser(ua, UserAgentRuleIndex.BuiltIn.Find(ua).Browsers));

    private static (string Name, string? Version)? FindBrowser(ReadOnlySpan<char> ua, ulong mask)
    {
        (string Name, string DetectToken, string? VersionToken)[] rules = HttpUserAgentRules.Browsers;
        UserAgentRuleIndex.Candidates candidates = new(mask, rules.Length);
        for (int i = candidates.Next(); i >= 0; i = candidates.Next())
        {
            (string name, string detectToken, string? versionToken) = rules[i];
            int detectIndex = IndexOfTokenStart(ua, detectToken, 0);
            if (detectIndex < 0)
            {
                continue;
            }

            if (TryReadBrowserVersion(ua, detectToken, detectIndex, versionToken, out Range version))
            {
                return (name, ua[version].ToString());
            }
        }

        return null;
    }

    private static bool TryReadBrowserVersion(ReadOnlySpan<char> ua, string detectToken, int detectIndex, string? versionToken, out Range version)
    {
        if (versionToken is null)
        {
            // "Token/1.2". Without '/' the token is likely a misspelling or another product (e.g. Edgg, Oprea, ChromeHelper).
            return TryReadVersion(ua, detectToken, detectIndex, allowSpace: false, out version);
        }

        // Safari's "Version/" and IE's "MSIE" identify both the browser and its version.
        if (string.Equals(detectToken, versionToken, StringComparison.Ordinal))
        {
            return TryReadVersion(ua, detectToken, detectIndex, allowSpace: true, out version);
        }

        // A separate version belongs to a complete product name. Skip prefixes such as TridentSpider.
        while (detectIndex >= 0)
        {
            int end = detectIndex + detectToken.Length;
            if (IsProductNameEnd(ua, end))
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
        Debug.Assert(token.Length > 0);
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

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool IsTokenStart(ReadOnlySpan<char> ua, int index)
        => index == 0 || s_productTokenDelimiters.Contains(ua[index - 1]);

    private static bool IsProductNameEnd(ReadOnlySpan<char> ua, int end)
        => end == ua.Length || ua[end] == '/' || s_productTokenDelimiters.Contains(ua[end]);

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
    {
        HttpUserAgentRobotSet robots = HttpUserAgentRules.RobotSet;
        return robots.MatchFirst(userAgent)
            ?? Detect(userAgent, robots, static (original, decoded, rules) => rules.MatchAfterFirst(original, decoded));
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
        => DetectProduct(userAgent, static ua => FindMobileDevice(ua, ulong.MaxValue));

    private static string? FindMobileDevice(ReadOnlySpan<char> ua, ulong mask)
    {
        int index = mask != ulong.MaxValue
            ? UserAgentRuleIndex.FindKey(ua, HttpUserAgentRules.MobileKeys, mask)
            : HttpUserAgentRules.MobileMatcher.Find(ua, 0);
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
