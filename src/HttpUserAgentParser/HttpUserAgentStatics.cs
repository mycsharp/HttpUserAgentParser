// Copyright © https://myCSharp.de - all rights reserved

using System.Collections.Frozen;
using System.Text.RegularExpressions;

namespace MyCSharp.HttpUserAgentParser;

/// <summary>
/// Parser settings
/// </summary>
public static class HttpUserAgentStatics
{
    /// <summary>
    /// Regex defauls for platform mappings
    /// </summary>
    private const RegexOptions DefaultPlatformsRegexFlags = RegexOptions.IgnoreCase | RegexOptions.Compiled;

    /// <summary>
    /// Creates default platform mapping regex
    /// </summary>
    private static Regex CreateDefaultPlatformRegex(string key) => new(Regex.Escape($"{key}"),
        DefaultPlatformsRegexFlags, matchTimeout: TimeSpan.FromMilliseconds(1000));

    /// <summary>
    /// Platforms
    /// </summary>
    public static readonly HashSet<HttpUserAgentPlatformInformation> Platforms =
    [
        new(CreateDefaultPlatformRegex("windows nt 10.0"), "Windows 10", HttpUserAgentPlatformType.Windows),
        new(CreateDefaultPlatformRegex("windows nt 6.3"), "Windows 8.1", HttpUserAgentPlatformType.Windows),
        new(CreateDefaultPlatformRegex("windows nt 6.2"), "Windows 8", HttpUserAgentPlatformType.Windows),
        new(CreateDefaultPlatformRegex("windows nt 6.1"), "Windows 7", HttpUserAgentPlatformType.Windows),
        new(CreateDefaultPlatformRegex("windows nt 6.0"), "Windows Vista", HttpUserAgentPlatformType.Windows),
        new(CreateDefaultPlatformRegex("windows nt 5.2"), "Windows 2003", HttpUserAgentPlatformType.Windows),
        new(CreateDefaultPlatformRegex("windows nt 5.1"), "Windows XP", HttpUserAgentPlatformType.Windows),
        new(CreateDefaultPlatformRegex("windows nt 5.0"), "Windows 2000", HttpUserAgentPlatformType.Windows),
        new(CreateDefaultPlatformRegex("windows nt 4.0"), "Windows NT 4.0", HttpUserAgentPlatformType.Windows),
        new(CreateDefaultPlatformRegex("winnt4.0"), "Windows NT 4.0", HttpUserAgentPlatformType.Windows),
        new(CreateDefaultPlatformRegex("winnt 4.0"), "Windows NT", HttpUserAgentPlatformType.Windows),
        new(CreateDefaultPlatformRegex("winnt"), "Windows NT", HttpUserAgentPlatformType.Windows),
        new(CreateDefaultPlatformRegex("windows 98"), "Windows 98", HttpUserAgentPlatformType.Windows),
        new(CreateDefaultPlatformRegex("win98"), "Windows 98", HttpUserAgentPlatformType.Windows),
        new(CreateDefaultPlatformRegex("windows 95"), "Windows 95", HttpUserAgentPlatformType.Windows),
        new(CreateDefaultPlatformRegex("win95"), "Windows 95", HttpUserAgentPlatformType.Windows),
        new(CreateDefaultPlatformRegex("windows phone"), "Windows Phone", HttpUserAgentPlatformType.Windows),
        new(CreateDefaultPlatformRegex("windows"), "Unknown Windows OS", HttpUserAgentPlatformType.Windows),
        new(CreateDefaultPlatformRegex("android"), "Android", HttpUserAgentPlatformType.Android),
        new(CreateDefaultPlatformRegex("blackberry"), "BlackBerry", HttpUserAgentPlatformType.BlackBerry),
        new(CreateDefaultPlatformRegex("iphone"), "iOS", HttpUserAgentPlatformType.IOS),
        new(CreateDefaultPlatformRegex("ipad"), "iOS", HttpUserAgentPlatformType.IOS),
        new(CreateDefaultPlatformRegex("ipod"), "iOS", HttpUserAgentPlatformType.IOS),
        new(CreateDefaultPlatformRegex("cros"), "ChromeOS", HttpUserAgentPlatformType.ChromeOS),
        new(CreateDefaultPlatformRegex("os x"), "Mac OS X", HttpUserAgentPlatformType.MacOS),
        new(CreateDefaultPlatformRegex("ppc mac"), "Power PC Mac", HttpUserAgentPlatformType.MacOS),
        new(CreateDefaultPlatformRegex("freebsd"), "FreeBSD", HttpUserAgentPlatformType.Linux),
        new(CreateDefaultPlatformRegex("ppc"), "Macintosh", HttpUserAgentPlatformType.Linux),
        new(CreateDefaultPlatformRegex("linux"), "Linux", HttpUserAgentPlatformType.Linux),
        new(CreateDefaultPlatformRegex("debian"), "Debian", HttpUserAgentPlatformType.Linux),
        new(CreateDefaultPlatformRegex("sunos"), "Sun Solaris", HttpUserAgentPlatformType.Generic),
        new(CreateDefaultPlatformRegex("beos"), "BeOS", HttpUserAgentPlatformType.Generic),
        new(CreateDefaultPlatformRegex("apachebench"), "ApacheBench", HttpUserAgentPlatformType.Generic),
        new(CreateDefaultPlatformRegex("aix"), "AIX", HttpUserAgentPlatformType.Generic),
        new(CreateDefaultPlatformRegex("irix"), "Irix", HttpUserAgentPlatformType.Generic),
        new(CreateDefaultPlatformRegex("osf"), "DEC OSF", HttpUserAgentPlatformType.Generic),
        new(CreateDefaultPlatformRegex("hp-ux"), "HP-UX", HttpUserAgentPlatformType.Windows),
        new(CreateDefaultPlatformRegex("netbsd"), "NetBSD", HttpUserAgentPlatformType.Generic),
        new(CreateDefaultPlatformRegex("bsdi"), "BSDi", HttpUserAgentPlatformType.Generic),
        new(CreateDefaultPlatformRegex("openbsd"), "OpenBSD", HttpUserAgentPlatformType.Unix),
        new(CreateDefaultPlatformRegex("gnu"), "GNU/Linux", HttpUserAgentPlatformType.Linux),
        new(CreateDefaultPlatformRegex("unix"), "Unknown Unix OS", HttpUserAgentPlatformType.Unix),
        new(CreateDefaultPlatformRegex("symbian"), "Symbian OS", HttpUserAgentPlatformType.Symbian),
    ];

    /// <summary>
    /// Precompiled platform regex map to attach to PlatformInformation without per-call allocations.
    /// </summary>
    private static readonly FrozenDictionary<string, Regex> s_platformRegexMap = HttpUserAgentFastRules.s_platformRules
        .ToFrozenDictionary(p => p.Token, p => CreateDefaultPlatformRegex(p.Token), StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Gets a precompiled platform regex for a given token.
    /// </summary>
    internal static Regex GetPlatformRegexForToken(string token) => s_platformRegexMap[token];

    /// <summary>
    /// Regex defauls for browser mappings
    /// </summary>
    private const RegexOptions DefaultBrowserRegexFlags = RegexOptions.IgnoreCase | RegexOptions.Compiled;
    /// <summary>
    /// Creates default browser mapping regex
    /// </summary>
    private static Regex CreateDefaultBrowserRegex(string key)
        => new($@"{key}.*?([0-9\.]+)", DefaultBrowserRegexFlags, matchTimeout: TimeSpan.FromMilliseconds(1000));

    /// <summary>
    /// Browsers
    /// </summary>
    public static readonly FrozenDictionary<Regex, string> Browsers = new Dictionary<Regex, string>()
    {
        { CreateDefaultBrowserRegex("OPR"), "Opera" },
        { CreateDefaultBrowserRegex("Flock"), "Flock" },
        { CreateDefaultBrowserRegex("Edge"), "Edge" },
        { CreateDefaultBrowserRegex("EdgA"), "Edge" },
        { CreateDefaultBrowserRegex("Edg"), "Edge" },
        { CreateDefaultBrowserRegex("Vivaldi"), "Vivaldi" },
        { CreateDefaultBrowserRegex("Brave Chrome"), "Brave" },
        { CreateDefaultBrowserRegex("Chrome"), "Chrome" },
        { CreateDefaultBrowserRegex("CriOS"), "Chrome" },
        { CreateDefaultBrowserRegex("Opera.*?Version"), "Opera" },
        { CreateDefaultBrowserRegex("Opera"), "Opera" },
        { CreateDefaultBrowserRegex("MSIE"), "Internet Explorer" },
        { CreateDefaultBrowserRegex("Internet Explorer"), "Internet Explorer" },
        { CreateDefaultBrowserRegex("Trident.* rv"), "Internet Explorer" },
        { CreateDefaultBrowserRegex("Shiira"), "Shiira" },
        { CreateDefaultBrowserRegex("Firefox"), "Firefox" },
        { CreateDefaultBrowserRegex("FxiOS"), "Firefox" },
        { CreateDefaultBrowserRegex("Chimera"), "Chimera" },
        { CreateDefaultBrowserRegex("Phoenix"), "Phoenix" },
        { CreateDefaultBrowserRegex("Firebird"), "Firebird" },
        { CreateDefaultBrowserRegex("Camino"), "Camino" },
        { CreateDefaultBrowserRegex("Netscape"), "Netscape" },
        { CreateDefaultBrowserRegex("OmniWeb"), "OmniWeb" },
        { CreateDefaultBrowserRegex("Version"), "Safari" }, // https://github.com/mycsharp/HttpUserAgentParser/issues/34
        { CreateDefaultBrowserRegex("Mozilla"), "Mozilla" },
        { CreateDefaultBrowserRegex("Konqueror"), "Konqueror" },
        { CreateDefaultBrowserRegex("icab"), "iCab" },
        { CreateDefaultBrowserRegex("Lynx"), "Lynx" },
        { CreateDefaultBrowserRegex("Links"), "Links" },
        { CreateDefaultBrowserRegex("hotjava"), "HotJava" },
        { CreateDefaultBrowserRegex("amaya"), "Amaya" },
        { CreateDefaultBrowserRegex("IBrowse"), "IBrowse" },
        { CreateDefaultBrowserRegex("Maxthon"), "Maxthon" },
        { CreateDefaultBrowserRegex("ipod touch"), "Apple iPod" },
        { CreateDefaultBrowserRegex("Ubuntu"), "Ubuntu Web Browser" },
    }.ToFrozenDictionary();

    /// <summary>
    /// Mobiles
    /// </summary>
    public static readonly FrozenDictionary<string, string> Mobiles = HttpUserAgentFastRules.s_mobileRules
        .ToFrozenDictionary(mobile => mobile.Key, mobile => mobile.Value, StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Robots
    /// </summary>
    public static readonly (string Key, string Value)[] Robots = HttpUserAgentFastRules.s_robotRules;

    /// <summary>
    /// Tools
    /// </summary>
    public static readonly FrozenDictionary<string, string> Tools = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        { "curl", "curl" }
    }
    .ToFrozenDictionary(StringComparer.OrdinalIgnoreCase);
}
