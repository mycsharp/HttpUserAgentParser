// Copyright © https://myCSharp.de - all rights reserved

using System.Collections.Frozen;
using System.Text.RegularExpressions;

namespace MyCSharp.HttpUserAgentParser;

/// <summary>
/// Parser settings
/// </summary>
/// <remarks>
/// The parser does not read <see cref="Platforms"/>, <see cref="Browsers"/>, <see cref="Mobiles"/> or <see cref="Tools"/>;
/// they are provided for consumers. <see cref="Robots"/> is shared with the parser: replaced entries are honored.
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
        new(HttpUserAgentPlatformRegexId.FreeBsd, "FreeBSD", HttpUserAgentPlatformType.Linux),
        new(HttpUserAgentPlatformRegexId.Ppc, "Macintosh", HttpUserAgentPlatformType.Linux),
        new(HttpUserAgentPlatformRegexId.Linux, "Linux", HttpUserAgentPlatformType.Linux),
        new(HttpUserAgentPlatformRegexId.Debian, "Debian", HttpUserAgentPlatformType.Linux),
        new(HttpUserAgentPlatformRegexId.SunOs, "Sun Solaris", HttpUserAgentPlatformType.Generic),
        new(HttpUserAgentPlatformRegexId.BeOs, "BeOS", HttpUserAgentPlatformType.Generic),
        new(HttpUserAgentPlatformRegexId.ApacheBench, "ApacheBench", HttpUserAgentPlatformType.Generic),
        new(HttpUserAgentPlatformRegexId.Aix, "AIX", HttpUserAgentPlatformType.Generic),
        new(HttpUserAgentPlatformRegexId.Irix, "Irix", HttpUserAgentPlatformType.Generic),
        new(HttpUserAgentPlatformRegexId.Osf, "DEC OSF", HttpUserAgentPlatformType.Generic),
        new(HttpUserAgentPlatformRegexId.HpUx, "HP-UX", HttpUserAgentPlatformType.Windows),
        new(HttpUserAgentPlatformRegexId.NetBsd, "NetBSD", HttpUserAgentPlatformType.Generic),
        new(HttpUserAgentPlatformRegexId.Bsdi, "BSDi", HttpUserAgentPlatformType.Generic),
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
    /// Robots
    /// </summary>
    /// <remarks>
    /// The parser uses this array directly: the first matching entry wins and replaced entries are honored.
    /// </remarks>
    public static readonly (string Key, string Value)[] Robots = HttpUserAgentRules.Robots;

    /// <summary>
    /// Tools
    /// </summary>
    public static readonly FrozenDictionary<string, string> Tools = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "curl", "curl" }
    }
    .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
}
