// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using MyCSharp.HttpUserAgentParser.Telemetry;

namespace MyCSharp.HttpUserAgentParser;

#pragma warning disable MA0049 // Type name should not match containing namespace

/// <summary>
/// Parser logic for user agents
/// </summary>
public static class HttpUserAgentParser
{
    /// <summary>
    /// The name of the Meter used for metrics.
    /// </summary>
    public const string MeterName = "MyCSharp.HttpUserAgentParser";

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
            HttpUserAgentParserTelemetry.ParseDuration(Stopwatch.GetElapsedTime(startTimestamp).TotalMilliseconds);
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
        foreach ((string key, string value) in HttpUserAgentStatics.Robots)
        {
            if (ContainsIgnoreCase(ua, key))
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
        ReadOnlySpan<char> ua = userAgent.AsSpan();
        foreach ((string key, string value) in HttpUserAgentStatics.Mobiles)
        {
            if (ContainsIgnoreCase(ua, key))
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
    private static bool ContainsIgnoreCase(ReadOnlySpan<char> haystack, ReadOnlySpan<char> needle)
        => TryIndexOf(haystack, needle, out _);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static bool TryIndexOf(ReadOnlySpan<char> haystack, ReadOnlySpan<char> needle, out int index)
    {
        index = haystack.IndexOf(needle, StringComparison.OrdinalIgnoreCase);
        return index >= 0;
    }

    /// <summary>
    /// Extracts a dotted numeric version.
    /// Accepts digits and dots; skips common separators ('/', ' ', ':', '=') until first digit.
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
                if (char.IsBetween(c, '0', '9'))
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
                if (!(char.IsBetween(c, '0', '9') || c == '.'))
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
