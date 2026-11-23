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
    [
        new(HttpUserAgentPlatformRegexId.WindowsNt100, "Windows 10", HttpUserAgentPlatformType.Windows),
        new(HttpUserAgentPlatformRegexId.WindowsNt63, "Windows 8.1", HttpUserAgentPlatformType.Windows),
        new(HttpUserAgentPlatformRegexId.WindowsNt62, "Windows 8", HttpUserAgentPlatformType.Windows),
        new(HttpUserAgentPlatformRegexId.WindowsNt61, "Windows 7", HttpUserAgentPlatformType.Windows),
        new(HttpUserAgentPlatformRegexId.WindowsNt60, "Windows Vista", HttpUserAgentPlatformType.Windows),
        new(HttpUserAgentPlatformRegexId.WindowsNt52, "Windows 2003", HttpUserAgentPlatformType.Windows),
        new(HttpUserAgentPlatformRegexId.WindowsNt51, "Windows XP", HttpUserAgentPlatformType.Windows),
        new(HttpUserAgentPlatformRegexId.WindowsNt50, "Windows 2000", HttpUserAgentPlatformType.Windows),
        new(HttpUserAgentPlatformRegexId.WindowsNt40, "Windows NT 4.0", HttpUserAgentPlatformType.Windows),
        new(HttpUserAgentPlatformRegexId.WinNt40, "Windows NT 4.0", HttpUserAgentPlatformType.Windows),
        new(HttpUserAgentPlatformRegexId.WinNtSpace40, "Windows NT", HttpUserAgentPlatformType.Windows),
        new(HttpUserAgentPlatformRegexId.WinNt, "Windows NT", HttpUserAgentPlatformType.Windows),
        new(HttpUserAgentPlatformRegexId.Windows98, "Windows 98", HttpUserAgentPlatformType.Windows),
        new(HttpUserAgentPlatformRegexId.Win98, "Windows 98", HttpUserAgentPlatformType.Windows),
        new(HttpUserAgentPlatformRegexId.Windows95, "Windows 95", HttpUserAgentPlatformType.Windows),
        new(HttpUserAgentPlatformRegexId.Win95, "Windows 95", HttpUserAgentPlatformType.Windows),
        new(HttpUserAgentPlatformRegexId.WindowsPhone, "Windows Phone", HttpUserAgentPlatformType.Windows),
        new(HttpUserAgentPlatformRegexId.Windows, "Unknown Windows OS", HttpUserAgentPlatformType.Windows),
        new(HttpUserAgentPlatformRegexId.Android, "Android", HttpUserAgentPlatformType.Android),
        new(HttpUserAgentPlatformRegexId.BlackBerry, "BlackBerry", HttpUserAgentPlatformType.BlackBerry),
        new(HttpUserAgentPlatformRegexId.IPhone, "iOS", HttpUserAgentPlatformType.IOS),
        new(HttpUserAgentPlatformRegexId.IPad, "iOS", HttpUserAgentPlatformType.IOS),
        new(HttpUserAgentPlatformRegexId.IPod, "iOS", HttpUserAgentPlatformType.IOS),
        new(HttpUserAgentPlatformRegexId.CrOS, "ChromeOS", HttpUserAgentPlatformType.ChromeOS),
        new(HttpUserAgentPlatformRegexId.OsX, "Mac OS X", HttpUserAgentPlatformType.MacOS),
        new(HttpUserAgentPlatformRegexId.PpcMac, "Power PC Mac", HttpUserAgentPlatformType.MacOS),
        new(HttpUserAgentPlatformRegexId.FreeBsd, "FreeBSD", HttpUserAgentPlatformType.Unix),
        new(HttpUserAgentPlatformRegexId.Ppc, "Macintosh", HttpUserAgentPlatformType.MacOS),
        new(HttpUserAgentPlatformRegexId.Linux, "Linux", HttpUserAgentPlatformType.Linux),
        new(HttpUserAgentPlatformRegexId.Debian, "Debian", HttpUserAgentPlatformType.Linux),
        new(HttpUserAgentPlatformRegexId.SunOs, "Sun Solaris", HttpUserAgentPlatformType.Unix),
        new(HttpUserAgentPlatformRegexId.BeOs, "BeOS", HttpUserAgentPlatformType.Generic),
        new(HttpUserAgentPlatformRegexId.ApacheBench, "ApacheBench", HttpUserAgentPlatformType.Generic),
        new(HttpUserAgentPlatformRegexId.Aix, "AIX", HttpUserAgentPlatformType.Unix),
        new(HttpUserAgentPlatformRegexId.Irix, "Irix", HttpUserAgentPlatformType.Unix),
        new(HttpUserAgentPlatformRegexId.Osf, "DEC OSF", HttpUserAgentPlatformType.Unix),
        new(HttpUserAgentPlatformRegexId.HpUx, "HP-UX", HttpUserAgentPlatformType.Unix),
        new(HttpUserAgentPlatformRegexId.NetBsd, "NetBSD", HttpUserAgentPlatformType.Unix),
        new(HttpUserAgentPlatformRegexId.Bsdi, "BSDi", HttpUserAgentPlatformType.Unix),
        new(HttpUserAgentPlatformRegexId.OpenBsd, "OpenBSD", HttpUserAgentPlatformType.Unix),
        new(HttpUserAgentPlatformRegexId.Gnu, "GNU/Linux", HttpUserAgentPlatformType.Linux),
        new(HttpUserAgentPlatformRegexId.Unix, "Unknown Unix OS", HttpUserAgentPlatformType.Unix),
        new(HttpUserAgentPlatformRegexId.Symbian, "Symbian OS", HttpUserAgentPlatformType.Symbian),
    ];

    /// <summary>
    /// Browsers
    /// </summary>
    /// <remarks>
    /// Each pattern is <c>{token}.*?([0-9\.]+)</c>; the first group captures the version.
    /// </remarks>
    public static readonly FrozenDictionary<Regex, string> Browsers = new Dictionary<Regex, string>()
    {
        { HttpUserAgentRegexes.BrowserOpr(), "Opera" },
        { HttpUserAgentRegexes.BrowserFlock(), "Flock" },
        { HttpUserAgentRegexes.BrowserEdge(), "Edge" },
        { HttpUserAgentRegexes.BrowserEdgA(), "Edge" },
        { HttpUserAgentRegexes.BrowserEdg(), "Edge" },
        { HttpUserAgentRegexes.BrowserVivaldi(), "Vivaldi" },
        { HttpUserAgentRegexes.BrowserBraveChrome(), "Brave" },
        { HttpUserAgentRegexes.BrowserChrome(), "Chrome" },
        { HttpUserAgentRegexes.BrowserCriOS(), "Chrome" },
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
        { HttpUserAgentRegexes.BrowserMozilla(), "Mozilla" },
        { HttpUserAgentRegexes.BrowserKonqueror(), "Konqueror" },
        { HttpUserAgentRegexes.BrowserICab(), "iCab" },
        { HttpUserAgentRegexes.BrowserLynx(), "Lynx" },
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
