// Copyright © https://myCSharp.de - all rights reserved

using System.Collections.Frozen;
using System.Text.RegularExpressions;

namespace MyCSharp.HttpUserAgentParser;

/// <summary>
/// Parser settings
/// </summary>
/// <remarks>
/// The parser does not read <see cref="Platforms"/>, <see cref="Browsers"/>, <see cref="Mobiles"/> or <see cref="Tools"/>;
/// they are provided for consumers. The parser detects the <see cref="Robots"/>; change them with <see cref="SetRobots"/>.
/// All regular expressions are source-generated (no runtime compilation, trimming and Native AOT compatible).
/// </remarks>
public static class HttpUserAgentStatics
{
    /// <summary>
    /// Platforms
    /// </summary>
    /// <remarks>
    /// The <see cref="HttpUserAgentPlatformInformation.Regex"/> of each entry is created on first access.
    /// </remarks>
    public static readonly HashSet<HttpUserAgentPlatformInformation> Platforms =
        [.. HttpUserAgentRules.Platforms.Select(static rule => rule.Platform).OrderBy(static platform => platform.RegexId)];

    /// <summary>
    /// Browsers
    /// </summary>
    /// <remarks>
    /// Each pattern is <c>{token}.*?([0-9\.]+)</c>; the first group captures the version.
    /// </remarks>
    public static readonly FrozenDictionary<Regex, string> Browsers = new Dictionary<Regex, string>()
    {
        { HttpUserAgentRegexes.BrowserOpr(), "Opera" },
        { HttpUserAgentRegexes.BrowserSilk(), "Amazon Silk" },
        { HttpUserAgentRegexes.BrowserFlock(), "Flock" },
        { HttpUserAgentRegexes.BrowserEdge(), "Edge" },
        { HttpUserAgentRegexes.BrowserEdgA(), "Edge" },
        { HttpUserAgentRegexes.BrowserEdg(), "Edge" },
        { HttpUserAgentRegexes.BrowserEdgiOS(), "Edge" },
        { HttpUserAgentRegexes.BrowserVivaldi(), "Vivaldi" },
        { HttpUserAgentRegexes.BrowserBraveChrome(), "Brave" },
        { HttpUserAgentRegexes.BrowserSamsungBrowser(), "Samsung Internet" },
        { HttpUserAgentRegexes.BrowserYaBrowser(), "Yandex Browser" },
        { HttpUserAgentRegexes.BrowserUCBrowser(), "UC Browser" },
        { HttpUserAgentRegexes.BrowserUCMobile(), "UC Browser" },
        { HttpUserAgentRegexes.BrowserChrome(), "Chrome" },
        { HttpUserAgentRegexes.BrowserCriOS(), "Chrome" },
        { HttpUserAgentRegexes.BrowserHeadlessChrome(), "Chrome" },
        { HttpUserAgentRegexes.BrowserOperaVersion(), "Opera" },
        { HttpUserAgentRegexes.BrowserOpera(), "Opera" },
        { HttpUserAgentRegexes.BrowserMsie(), "Internet Explorer" },
        { HttpUserAgentRegexes.BrowserInternetExplorer(), "Internet Explorer" },
        { HttpUserAgentRegexes.BrowserTridentRv(), "Internet Explorer" },
        { HttpUserAgentRegexes.BrowserShiira(), "Shiira" },
        { HttpUserAgentRegexes.BrowserFirefox(), "Firefox" },
        { HttpUserAgentRegexes.BrowserFxiOS(), "Firefox" },
        { HttpUserAgentRegexes.BrowserChimera(), "Chimera" },
        { HttpUserAgentRegexes.BrowserPhoenix(), "Phoenix" },
        { HttpUserAgentRegexes.BrowserFirebird(), "Firebird" },
        { HttpUserAgentRegexes.BrowserCamino(), "Camino" },
        { HttpUserAgentRegexes.BrowserNetscape(), "Netscape" },
        { HttpUserAgentRegexes.BrowserOmniWeb(), "OmniWeb" },
        { HttpUserAgentRegexes.BrowserVersion(), "Safari" }, // https://github.com/mycsharp/HttpUserAgentParser/issues/34
        { HttpUserAgentRegexes.BrowserKonqueror(), "Konqueror" },
        { HttpUserAgentRegexes.BrowserICab(), "iCab" },
        { HttpUserAgentRegexes.BrowserLynx(), "Lynx" },
        { HttpUserAgentRegexes.BrowserELinks(), "ELinks" },
        { HttpUserAgentRegexes.BrowserLinks(), "Links" },
        { HttpUserAgentRegexes.BrowserHotJava(), "HotJava" },
        { HttpUserAgentRegexes.BrowserAmaya(), "Amaya" },
        { HttpUserAgentRegexes.BrowserIBrowse(), "IBrowse" },
        { HttpUserAgentRegexes.BrowserMaxthon(), "Maxthon" },
        { HttpUserAgentRegexes.BrowserIPodTouch(), "Apple iPod" },
        { HttpUserAgentRegexes.BrowserUbuntu(), "Ubuntu Web Browser" },
    }.ToFrozenDictionary();

    /// <summary>
    /// Mobiles
    /// </summary>
    public static readonly FrozenDictionary<string, string> Mobiles = HttpUserAgentRules.CreateMobileDictionary();

    /// <summary>
    /// The robots the parser detects, in priority order: the first entry whose key occurs in the user agent wins.
    /// </summary>
    /// <remarks>
    /// Read-only; <see cref="SetRobots"/> replaces the list. Initially <see cref="DefaultRobots"/>.
    /// </remarks>
    public static IReadOnlyList<(string Key, string Value)> Robots => HttpUserAgentRules.RobotSet.ReadOnlyEntries;

    /// <summary>
    /// The robots of this library, in priority order.
    /// </summary>
    public static IReadOnlyList<(string Key, string Value)> DefaultRobots => HttpUserAgentRules.DefaultRobotSet.ReadOnlyEntries;

    /// <summary>
    /// Replaces the robots the parser detects, in priority order: the first entry whose key occurs in the user agent
    /// (ignoring case) wins, and its value is the robot name.
    /// </summary>
    /// <remarks>
    /// The list applies to all following parses in the process; it is replaced atomically, so a parse running at the same
    /// time uses either the previous or the new list. Both built-in cache providers honor the change on the next call.
    /// Spaces in a key also match IIS-encoded plus signs. A plus sign in the key matches the original header literally.
    /// To extend the defaults, pass e.g.
    /// <c>[.. HttpUserAgentStatics.DefaultRobots, ("MyCrawler", "My Crawler")]</c>; <see cref="DefaultRobots"/> restores them.
    /// Building the matchers takes a moment, so set the list once at startup rather than per request.
    /// </remarks>
    /// <param name="robots">The robots in priority order.</param>
    /// <exception cref="ArgumentNullException"><paramref name="robots"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentException">A key is <see langword="null"/> or empty, or a value is <see langword="null"/>.</exception>
    public static void SetRobots(IEnumerable<(string Key, string Value)> robots)
    {
        ArgumentNullException.ThrowIfNull(robots);

        (string Key, string Value)[] entries = [.. robots];
        foreach ((string key, string value) in entries)
        {
            if (string.IsNullOrEmpty(key) || value is null)
            {
                throw new ArgumentException("A robot needs a key that is not empty and a value.", nameof(robots));
            }
        }

        HttpUserAgentRules.SetRobotSet(new HttpUserAgentRobotSet(entries));
    }

    /// <summary>
    /// Tools
    /// </summary>
    public static readonly FrozenDictionary<string, string> Tools = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "curl", "curl" }
    }
    .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
}
