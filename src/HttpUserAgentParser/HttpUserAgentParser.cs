// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;

namespace MyCSharp.HttpUserAgentParser;

#pragma warning disable MA0049 // Type name should not match containing namespace

/// <summary>
/// Provides methods to parse HTTP User-Agent strings and extract browser, platform, device, and robot information.
/// </summary>
/// <remarks>
/// This parser is optimized for performance using span-based operations and vectorized string matching.
/// For repeated parsing of the same user agent strings, consider using <see cref="Providers.HttpUserAgentParserCachedProvider"/>.
/// </remarks>
public static class HttpUserAgentParser
{
    /// <summary>
    /// Parses the specified User-Agent string and returns detailed information about the browser, platform, and device.
    /// </summary>
    /// <param name="userAgent">The HTTP User-Agent header value to parse.</param>
    /// <returns>An <see cref="HttpUserAgentInformation"/> instance containing the parsed information.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="userAgent"/> is <see langword="null"/>.</exception>
    /// <example>
    /// <code>
    /// string userAgentString = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/90.0.4430.212 Safari/537.36";
    /// HttpUserAgentInformation info = HttpUserAgentParser.Parse(userAgentString);
    ///
    /// Console.WriteLine(info.Name);     // "Chrome"
    /// Console.WriteLine(info.Version);  // "90.0.4430.212"
    /// </code>
    /// </example>
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
    /// Removes leading and trailing whitespace from the User-Agent string.
    /// </summary>
    /// <param name="userAgent">The User-Agent string to clean up.</param>
    /// <returns>A trimmed copy of the User-Agent string.</returns>
    /// <example>
    /// <code>
    /// string cleaned = HttpUserAgentParser.Cleanup("  Mozilla/5.0  ");
    /// // Result: "Mozilla/5.0"
    /// </code>
    /// </example>
    public static string Cleanup(string userAgent) => userAgent.Trim();

    /// <summary>
    /// Extracts the platform information from the User-Agent string.
    /// </summary>
    /// <param name="userAgent">The User-Agent string to analyze.</param>
    /// <returns>
    /// An <see cref="HttpUserAgentPlatformInformation"/> instance if a platform is detected; otherwise, <see langword="null"/>.
    /// </returns>
    /// <example>
    /// <code>
    /// HttpUserAgentPlatformInformation? platform = HttpUserAgentParser.GetPlatform(
    ///     "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
    ///
    /// if (platform != null)
    /// {
    ///     Console.WriteLine(platform.Value.Name);         // "Windows 10"
    ///     Console.WriteLine(platform.Value.PlatformType); // Windows
    /// }
    /// </code>
    /// </example>
    public static HttpUserAgentPlatformInformation? GetPlatform(string userAgent)
    {
        ReadOnlySpan<char> ua = userAgent.AsSpan();
        foreach ((string Token, string Name, HttpUserAgentPlatformType PlatformType) platform in HttpUserAgentFastRules.s_platformRules)
        {
            if (ContainsIgnoreCase(ua, platform.Token))
            {
                return new HttpUserAgentPlatformInformation(
                    HttpUserAgentFastRules.GetPlatformRegexForToken(platform.Token),
                    platform.Name, platform.PlatformType);
            }
        }

        return null;
    }

    /// <summary>
    /// Attempts to extract the platform information from the User-Agent string.
    /// </summary>
    /// <param name="userAgent">The User-Agent string to analyze.</param>
    /// <param name="platform">When this method returns <see langword="true"/>, contains the platform information.</param>
    /// <returns><see langword="true"/> if a platform was detected; otherwise, <see langword="false"/>.</returns>
    /// <example>
    /// <code>
    /// bool found = HttpUserAgentParser.TryGetPlatform(
    ///     "Mozilla/5.0 (Windows NT 10.0)",
    ///     out HttpUserAgentPlatformInformation? platform);
    /// </code>
    /// </example>
    public static bool TryGetPlatform(string userAgent, [NotNullWhen(true)] out HttpUserAgentPlatformInformation? platform)
    {
        platform = GetPlatform(userAgent);
        return platform is not null;
    }

    /// <summary>
    /// Extracts the browser name and version from the User-Agent string.
    /// </summary>
    /// <param name="userAgent">The User-Agent string to analyze.</param>
    /// <returns>
    /// A tuple containing the browser name and version if detected; otherwise, <see langword="null"/>.
    /// </returns>
    /// <remarks>
    /// Uses a fast path with token-based matching to avoid regex where possible.
    /// </remarks>
    /// <example>
    /// <code>
    /// (string Name, string? Version)? browser = HttpUserAgentParser.GetBrowser(
    ///     "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/90.0.4430.212 Safari/537.36");
    ///
    /// if (browser != null)
    /// {
    ///     Console.WriteLine(browser.Value.Name);    // "Chrome"
    ///     Console.WriteLine(browser.Value.Version); // "90.0.4430.212"
    /// }
    /// </code>
    /// </example>
    public static (string Name, string? Version)? GetBrowser(string userAgent)
    {
        ReadOnlySpan<char> ua = userAgent.AsSpan();

        foreach ((string Name, string DetectToken, string? VersionToken) browserRule in HttpUserAgentFastRules.s_browserRules)
        {
            if (!TryIndexOf(ua, browserRule.DetectToken, out int detectIndex))
            {
                continue;
            }

            int afterDetectIndex = detectIndex + browserRule.DetectToken.Length;
            int versionSearchStart;

            // For rules without a specific version token, ensure pattern Token/<digits>
            if (string.IsNullOrEmpty(browserRule.VersionToken))
            {
                if (afterDetectIndex >= ua.Length || ua[afterDetectIndex] != '/')
                {
                    // Likely a misspelling or partial token (e.g., Edgg, Oprea, Chromee)
                    continue;
                }
                versionSearchStart = afterDetectIndex;
            }
            else
            {
                // Version token may differ (e.g., Safari uses "Version/")
                ReadOnlySpan<char> afterDetect = afterDetectIndex < ua.Length ? ua.Slice(afterDetectIndex) : [];

                if (!afterDetect.IsEmpty && TryIndexOf(afterDetect, browserRule.VersionToken, out int vtIndex))
                {
                    versionSearchStart = afterDetectIndex + vtIndex + browserRule.VersionToken.Length;
                }
                else
                {
                    // If specific version token wasn't found, fall back to detect token area
                    versionSearchStart = afterDetectIndex;
                }
            }

            ReadOnlySpan<char> search = ua.Slice(versionSearchStart);
            if (TryExtractVersion(search, out Range range))
            {
                return (browserRule.Name, search[range].ToString());
            }

            // If we didn't find a version for this rule, try next rule
        }

        return null;
    }

    /// <summary>
    /// Attempts to extract the browser name and version from the User-Agent string.
    /// </summary>
    /// <param name="userAgent">The User-Agent string to analyze.</param>
    /// <param name="browser">When this method returns <see langword="true"/>, contains the browser name and version.</param>
    /// <returns><see langword="true"/> if a browser was detected; otherwise, <see langword="false"/>.</returns>
    /// <example>
    /// <code>
    /// bool found = HttpUserAgentParser.TryGetBrowser(
    ///     "Mozilla/5.0 Chrome/90.0.4430.212",
    ///     out (string Name, string? Version)? browser);
    /// </code>
    /// </example>
    public static bool TryGetBrowser(string userAgent, [NotNullWhen(true)] out (string Name, string? Version)? browser)
    {
        browser = GetBrowser(userAgent);
        return browser is not null;
    }

    /// <summary>
    /// Extracts the robot/bot name from the User-Agent string if it matches a known bot signature.
    /// </summary>
    /// <param name="userAgent">The User-Agent string to analyze.</param>
    /// <returns>The robot name if detected; otherwise, <see langword="null"/>.</returns>
    /// <example>
    /// <code>
    /// string? robot = HttpUserAgentParser.GetRobot("Googlebot/2.1 (+http://www.google.com/bot.html)");
    /// // Result: "Googlebot"
    /// </code>
    /// </example>
    public static string? GetRobot(string userAgent)
    {
        ReadOnlySpan<char> ua = userAgent.AsSpan();
        foreach ((string key, string value) in HttpUserAgentFastRules.s_robotRules)
        {
            if (ContainsIgnoreCase(ua, key))
            {
                return value;
            }
        }

        return null;
    }

    /// <summary>
    /// Attempts to extract the robot/bot name from the User-Agent string.
    /// </summary>
    /// <param name="userAgent">The User-Agent string to analyze.</param>
    /// <param name="robotName">When this method returns <see langword="true"/>, contains the robot name.</param>
    /// <returns><see langword="true"/> if a robot was detected; otherwise, <see langword="false"/>.</returns>
    /// <example>
    /// <code>
    /// bool isBot = HttpUserAgentParser.TryGetRobot("Googlebot/2.1", out string? robotName);
    /// </code>
    /// </example>
    public static bool TryGetRobot(string userAgent, [NotNullWhen(true)] out string? robotName)
    {
        robotName = GetRobot(userAgent);
        return robotName is not null;
    }

    /// <summary>
    /// Extracts the mobile device type from the User-Agent string.
    /// </summary>
    /// <param name="userAgent">The User-Agent string to analyze.</param>
    /// <returns>The device type (e.g., "Apple iPhone", "Android") if detected; otherwise, <see langword="null"/>.</returns>
    /// <example>
    /// <code>
    /// string? device = HttpUserAgentParser.GetMobileDevice(
    ///     "Mozilla/5.0 (iPhone; CPU iPhone OS 14_5) Mobile");
    /// // Result: "Apple iPhone"
    /// </code>
    /// </example>
    public static string? GetMobileDevice(string userAgent)
    {
        ReadOnlySpan<char> ua = userAgent.AsSpan();
        foreach ((string key, string value) in HttpUserAgentFastRules.s_mobileRules)
        {
            if (ContainsIgnoreCase(ua, key))
            {
                return value;
            }
        }

        return null;
    }

    /// <summary>
    /// Attempts to extract the mobile device type from the User-Agent string.
    /// </summary>
    /// <param name="userAgent">The User-Agent string to analyze.</param>
    /// <param name="device">When this method returns <see langword="true"/>, contains the device type.</param>
    /// <returns><see langword="true"/> if a mobile device was detected; otherwise, <see langword="false"/>.</returns>
    /// <example>
    /// <code>
    /// bool isMobile = HttpUserAgentParser.TryGetMobileDevice(
    ///     "Mozilla/5.0 (iPhone; CPU iPhone OS 14_5)",
    ///     out string? device);
    /// </code>
    /// </example>
    public static bool TryGetMobileDevice(string userAgent, [NotNullWhen(true)] out string? device)
    {
        device = GetMobileDevice(userAgent);
        return device is not null;
    }

    /// <summary>
    /// Fast case-insensitive substring check.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool ContainsIgnoreCase(ReadOnlySpan<char> haystack, ReadOnlySpan<char> needle)
        => TryIndexOf(haystack, needle, out _);

    /// <summary>
    /// Finds the first index of a token in a span using ordinal-ignore-case.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryIndexOf(ReadOnlySpan<char> haystack, ReadOnlySpan<char> needle, out int index)
    {
        index = haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase);
        return index >= 0;
    }

    /// <summary>
    /// Extracts a dotted numeric version.
    /// Accepts digits and dots; skips common separators until first digit.
    /// Returns false if no version-like token is found.
    /// </summary>
    private static bool TryExtractVersion(ReadOnlySpan<char> haystack, out Range range)
    {
        range = default;

        // Vectorization is used in a optimistic way and specialized to common (trimmed down) user agents.
        // When the first two char-vectors don't yield any success, we fall back to the scalar path.
        // This penalized not found versions, but has an advantage for found versions.
        // Vector512 is left out, because there are no common inputs with length 128 or more.
        //
        // Two short (same size as char) vectors are read, then packed to byte vectors on which the
        // operation is done. For short / chart the higher byte is not of interest and zero or outside
        // the target characters, thus with bytes we can process twice as much elements at once.

        if (Vector256.IsHardwareAccelerated && haystack.Length >= 2 * Vector256<short>.Count)
        {
            ref char ptr = ref MemoryMarshal.GetReference(haystack);

            Vector256<byte> vec = ptr.ReadVector256AsBytes(0);
            Vector256<byte> between0and9 = Vector256.LessThan(vec - Vector256.Create((byte)'0'), Vector256.Create((byte)('9' - '0' + 1)));

            if (between0and9 == Vector256<byte>.Zero)
            {
                goto Scalar;
            }

            uint bitMask = between0and9.ExtractMostSignificantBits();
            int idx = (int)uint.TrailingZeroCount(bitMask);
            Debug.Assert(idx is >= 0 and <= 32);
            int start = idx;

            Vector256<byte> byteMask = between0and9 | Vector256.Equals(vec, Vector256.Create((byte)'.'));
            byteMask = ~byteMask;

            if (byteMask == Vector256<byte>.Zero)
            {
                goto Scalar;
            }

            bitMask = byteMask.ExtractMostSignificantBits();
            bitMask >>= start;

            idx = start + (int)uint.TrailingZeroCount(bitMask);
            Debug.Assert(idx is >= 0 and <= 32);
            int end = idx;

            range = new Range(start, end);
            return true;
        }
        else if (Vector128.IsHardwareAccelerated && haystack.Length >= 2 * Vector128<short>.Count)
        {
            ref char ptr = ref MemoryMarshal.GetReference(haystack);

            Vector128<byte> vec = ptr.ReadVector128AsBytes(0);
            Vector128<byte> between0and9 = Vector128.LessThan(vec - Vector128.Create((byte)'0'), Vector128.Create((byte)('9' - '0' + 1)));

            if (between0and9 == Vector128<byte>.Zero)
            {
                goto Scalar;
            }

            uint bitMask = between0and9.ExtractMostSignificantBits();
            int idx = (int)uint.TrailingZeroCount(bitMask);
            Debug.Assert(idx is >= 0 and <= 16);
            int start = idx;

            Vector128<byte> byteMask = between0and9 | Vector128.Equals(vec, Vector128.Create((byte)'.'));
            byteMask = ~byteMask;

            if (byteMask == Vector128<byte>.Zero)
            {
                goto Scalar;
            }

            bitMask = byteMask.ExtractMostSignificantBits();
            bitMask >>= start;

            idx = start + (int)uint.TrailingZeroCount(bitMask);
            Debug.Assert(idx is >= 0 and <= 16);
            int end = idx;

            range = new Range(start, end);
            return true;
        }

    Scalar:
        {
            // Limit search window to avoid scanning entire UA string unnecessarily
            const int Windows = 128;
            if (haystack.Length > Windows)
            {
                haystack = haystack.Slice(0, Windows);
            }

            int start = -1;
            int i = 0;

            for (; i < haystack.Length; ++i)
            {
                char c = haystack[i];
                if ((uint)(c - '0') <= 9)
                {
                    start = i;
                    break;
                }
            }

            if (start < 0)
            {
                // No digit found => no version
                return false;
            }

            haystack = haystack.Slice(i + 1);
            for (i = 0; i < haystack.Length; ++i)
            {
                char c = haystack[i];
                if (!((uint)(c - '0') <= 9 || c == '.'))
                {
                    break;
                }
            }

            i += start + 1;     // shift back the previous domain

            if (i == start)
            {
                return false;
            }

            range = new Range(start, i);
            return true;
        }
    }
}
