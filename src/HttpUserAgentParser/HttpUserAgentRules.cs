// Copyright © https://myCSharp.de - all rights reserved

using System.Collections.Frozen;

namespace MyCSharp.HttpUserAgentParser;

/// <summary>
/// Detection rules used by <see cref="HttpUserAgentParser"/>.
/// </summary>
/// <remarks>
/// The rules live outside of <see cref="HttpUserAgentStatics"/> so that parsing never triggers the initialization
/// of the public regex-based collections. In every table the order defines the priority: the first matching rule wins.
/// </remarks>
internal static class HttpUserAgentRules
{
    /// <summary>Marker type of the robot <see cref="PriorityTokenMatcher{TDetector}"/>.</summary>
    internal readonly struct RobotDetector;

    /// <summary>Marker type of the robot anchor <see cref="PriorityTokenMatcher{TDetector}"/>.</summary>
    internal readonly struct RobotAnchorDetector;

    /// <summary>Marker type of the mobile device <see cref="PriorityTokenMatcher{TDetector}"/>.</summary>
    internal readonly struct MobileDetector;

    /// <summary>
    /// Platform rules, sorted by frequency (most common platforms first, specific versions before generic tokens).
    /// </summary>
    internal static (string Token, HttpUserAgentPlatformInformation Platform)[] Platforms { get; } =
    [
        // Compatibility tokens on Windows Phone can identify both desktop Windows and Android.
        ("windows phone", new(HttpUserAgentPlatformRegexId.WindowsPhone, "Windows Phone", HttpUserAgentPlatformType.Windows)),
        // Most common: Windows (specific versions before generic)
        ("windows nt 10.0", new(HttpUserAgentPlatformRegexId.WindowsNt100, "Windows 10", HttpUserAgentPlatformType.Windows)),
        ("windows nt 6.1", new(HttpUserAgentPlatformRegexId.WindowsNt61, "Windows 7", HttpUserAgentPlatformType.Windows)),
        ("windows nt 6.3", new(HttpUserAgentPlatformRegexId.WindowsNt63, "Windows 8.1", HttpUserAgentPlatformType.Windows)),
        ("windows nt 6.2", new(HttpUserAgentPlatformRegexId.WindowsNt62, "Windows 8", HttpUserAgentPlatformType.Windows)),
        ("windows nt 6.0", new(HttpUserAgentPlatformRegexId.WindowsNt60, "Windows Vista", HttpUserAgentPlatformType.Windows)),
        // Android (very common on mobile)
        ("android", new(HttpUserAgentPlatformRegexId.Android, "Android", HttpUserAgentPlatformType.Android)),
        // iOS devices (very common)
        ("iphone", new(HttpUserAgentPlatformRegexId.IPhone, "iOS", HttpUserAgentPlatformType.IOS)),
        ("ipad", new(HttpUserAgentPlatformRegexId.IPad, "iOS", HttpUserAgentPlatformType.IOS)),
        ("ipod", new(HttpUserAgentPlatformRegexId.IPod, "iOS", HttpUserAgentPlatformType.IOS)),
        ("cros", new(HttpUserAgentPlatformRegexId.CrOS, "ChromeOS", HttpUserAgentPlatformType.ChromeOS)),
        // Mac OS (common)
        ("os x", new(HttpUserAgentPlatformRegexId.OsX, "Mac OS X", HttpUserAgentPlatformType.MacOS)),
        // A distribution can also identify itself as GNU/Linux.
        ("debian", new(HttpUserAgentPlatformRegexId.Debian, "Debian", HttpUserAgentPlatformType.Linux)),
        ("gnu", new(HttpUserAgentPlatformRegexId.Gnu, "GNU/Linux", HttpUserAgentPlatformType.Linux)),
        ("linux", new(HttpUserAgentPlatformRegexId.Linux, "Linux", HttpUserAgentPlatformType.Linux)),
        // Other Windows versions
        ("windows nt 5.2", new(HttpUserAgentPlatformRegexId.WindowsNt52, "Windows 2003", HttpUserAgentPlatformType.Windows)),
        ("windows nt 5.1", new(HttpUserAgentPlatformRegexId.WindowsNt51, "Windows XP", HttpUserAgentPlatformType.Windows)),
        ("windows nt 5.0", new(HttpUserAgentPlatformRegexId.WindowsNt50, "Windows 2000", HttpUserAgentPlatformType.Windows)),
        ("windows nt 4.0", new(HttpUserAgentPlatformRegexId.WindowsNt40, "Windows NT 4.0", HttpUserAgentPlatformType.Windows)),
        ("winnt4.0", new(HttpUserAgentPlatformRegexId.WinNt40, "Windows NT 4.0", HttpUserAgentPlatformType.Windows)),
        ("winnt 4.0", new(HttpUserAgentPlatformRegexId.WinNtSpace40, "Windows NT", HttpUserAgentPlatformType.Windows)),
        ("winnt", new(HttpUserAgentPlatformRegexId.WinNt, "Windows NT", HttpUserAgentPlatformType.Windows)),
        ("windows 98", new(HttpUserAgentPlatformRegexId.Windows98, "Windows 98", HttpUserAgentPlatformType.Windows)),
        ("win98", new(HttpUserAgentPlatformRegexId.Win98, "Windows 98", HttpUserAgentPlatformType.Windows)),
        ("windows 95", new(HttpUserAgentPlatformRegexId.Windows95, "Windows 95", HttpUserAgentPlatformType.Windows)),
        ("win95", new(HttpUserAgentPlatformRegexId.Win95, "Windows 95", HttpUserAgentPlatformType.Windows)),
        ("windows", new(HttpUserAgentPlatformRegexId.Windows, "Unknown Windows OS", HttpUserAgentPlatformType.Windows)),
        // Less common platforms
        ("blackberry", new(HttpUserAgentPlatformRegexId.BlackBerry, "BlackBerry", HttpUserAgentPlatformType.BlackBerry)),
        ("ppc mac", new(HttpUserAgentPlatformRegexId.PpcMac, "Power PC Mac", HttpUserAgentPlatformType.MacOS)),
        ("freebsd", new(HttpUserAgentPlatformRegexId.FreeBsd, "FreeBSD", HttpUserAgentPlatformType.Unix)),
        ("openbsd", new(HttpUserAgentPlatformRegexId.OpenBsd, "OpenBSD", HttpUserAgentPlatformType.Unix)),
        ("symbian", new(HttpUserAgentPlatformRegexId.Symbian, "Symbian OS", HttpUserAgentPlatformType.Symbian)),
        ("sunos", new(HttpUserAgentPlatformRegexId.SunOs, "Sun Solaris", HttpUserAgentPlatformType.Unix)),
        ("beos", new(HttpUserAgentPlatformRegexId.BeOs, "BeOS", HttpUserAgentPlatformType.Generic)),
        ("apachebench", new(HttpUserAgentPlatformRegexId.ApacheBench, "ApacheBench", HttpUserAgentPlatformType.Generic)),
        ("aix", new(HttpUserAgentPlatformRegexId.Aix, "AIX", HttpUserAgentPlatformType.Unix)),
        ("irix", new(HttpUserAgentPlatformRegexId.Irix, "Irix", HttpUserAgentPlatformType.Unix)),
        ("osf", new(HttpUserAgentPlatformRegexId.Osf, "DEC OSF", HttpUserAgentPlatformType.Unix)),
        ("hp-ux", new(HttpUserAgentPlatformRegexId.HpUx, "HP-UX", HttpUserAgentPlatformType.Unix)),
        ("netbsd", new(HttpUserAgentPlatformRegexId.NetBsd, "NetBSD", HttpUserAgentPlatformType.Unix)),
        ("bsdi", new(HttpUserAgentPlatformRegexId.Bsdi, "BSDi", HttpUserAgentPlatformType.Unix)),
        ("unix", new(HttpUserAgentPlatformRegexId.Unix, "Unknown Unix OS", HttpUserAgentPlatformType.Unix)),
        ("ppc", new(HttpUserAgentPlatformRegexId.Ppc, "Macintosh", HttpUserAgentPlatformType.MacOS)),
    ];

    /// <summary>
    /// Browser token rules for detection and version extraction.
    /// Sorted by specificity first, then frequency - more specific tokens must come before generic ones
    /// (e.g., Edge/Opera before Chrome, since Edge/Opera UAs contain "Chrome").
    /// </summary>
    internal static (string Name, string DetectToken, string? VersionToken)[] Browsers { get; } =
    [
        // Most specific browsers first (contain Chrome/Mozilla in their UA)
        ("Opera", "OPR", null),
        ("Opera", "Opera", "Version/"),
        ("Edge", "Edg", null),
        ("Edge", "Edge", null),
        ("Edge", "EdgA", null),
        ("Edge", "EdgiOS", null),
        ("Brave", "Brave Chrome", null),
        ("Vivaldi", "Vivaldi", null),
        ("Flock", "Flock", null),
        // Chromium based browsers with their own token (their UAs contain "Chrome/" as well)
        ("Samsung Internet", "SamsungBrowser", null),
        ("Yandex Browser", "YaBrowser", null),
        ("UC Browser", "UCBrowser", null),
        ("UC Browser", "UCMobile", null),
        ("Maxthon", "Maxthon", null),
        // Common browsers
        ("Chrome", "Chrome", null),
        ("Chrome", "CriOS", null),
        ("Netscape", "Netscape", null),
        ("Firefox", "Firefox", null),
        ("Firefox", "FxiOS", null),
        // Internet Explorer (legacy but still in use - MSIE before Trident to avoid false matches)
        ("Internet Explorer", "MSIE", "MSIE"), // "MSIE 6.0"
        ("Internet Explorer", "Trident", "rv:"),
        ("Internet Explorer", "Internet Explorer", null),
        // Less common browsers
        ("Konqueror", "Konqueror", null),
        ("OmniWeb", "OmniWeb", null),
        ("Shiira", "Shiira", null),
        ("Chimera", "Chimera", null),
        ("Camino", "Camino", null),
        ("Firebird", "Firebird", null),
        ("Phoenix", "Phoenix", null),
        ("iCab", "icab", null),
        ("Lynx", "Lynx", null),
        ("ELinks", "ELinks", null),
        ("Links", "Links", null),
        ("HotJava", "hotjava", null),
        ("Amaya", "amaya", null),
        ("IBrowse", "IBrowse", null),
        ("Apple iPod", "ipod touch", null),
        ("Ubuntu Web Browser", "Ubuntu", null),
        // The generic Version/ token only identifies Safari when no specific browser matched.
        ("Chrome", "HeadlessChrome", null),
        ("Safari", "Version/", "Version/"),
    ];

    /// <summary>
    /// The default robots in priority order (<see cref="HttpUserAgentStatics.DefaultRobots"/>); never changed.
    /// </summary>
    internal static (string Key, string Value)[] DefaultRobots { get; } =
    [
        ( "googlebot", "Googlebot" ),
        ( "meta-externalagent", "meta-externalagent" ),
        ( "openai.com/searchbot", "OAI-SearchBot" ),
        ( "CCBot", "CCBot" ),
        ( "archive.org/details/archive.org_bot", "archive.org" ),
        ( "opensiteexplorer.org/dotbot", "DotBot" ),
        ( "awario.com/bots.html", "AwarioBot" ),
        ( "Turnitin", "Turnitin" ),
        ( "openai.com/gptbot", "GPTBot" ),
        ( "perplexity.ai/perplexitybot", "PerplexityBot" ),
        ( "developer.amazon.com/support/amazonbot", "Amazonbot" ),
        ( "trendictionbot", "trendictionbot" ),
        ( "Bytespider", "Bytespider" ),
        ( "MojeekBot", "MojeekBot" ),
        ( "SeekportBot", "SeekportBot" ),
        ( "googleweblight", "Google Web Light" ),
        ( "PetalBot", "PetalBot"),
        ( "DuplexWeb-Google", "DuplexWeb-Google"),
        ( "Storebot-Google", "Storebot-Google"),
        ( "msnbot", "MSNBot"),
        ( "baiduspider", "Baiduspider"),
        ( "Google Favicon", "Google Favicon"),
        ( "Jobboerse", "Jobboerse"),
        ( "bingbot", "BingBot"),
        ( "BingPreview", "Bing Preview"),
        ( "slurp", "Slurp"),
        ( "yahoo", "Yahoo"),
        ( "ask jeeves", "Ask Jeeves"),
        ( "fastcrawler", "FastCrawler"),
        ( "infoseek", "InfoSeek Robot 1.0"),
        ( "lycos", "Lycos"),
        ( "YandexBot", "YandexBot"),
        ( "YandexImages", "YandexImages"),
        ( "mediapartners-google", "Mediapartners Google"),
        ( "apis-google", "APIs Google"),
        ( "CRAZYWEBCRAWLER", "Crazy Webcrawler"),
        ( "AdsBot-Google-Mobile", "AdsBot Google Mobile"),
        ( "adsbot-google", "AdsBot Google"),
        ( "feedfetcher-google", "FeedFetcher-Google"),
        ( "google-read-aloud", "Google-Read-Aloud"),
        ( "curious george", "Curious George"),
        ( "ia_archiver", "Alexa Crawler"),
        ( "MJ12bot", "Majestic"),
        ( "Uptimebot", "Uptimebot"),
        ( "CheckMarkNetwork", "CheckMark"),
        ( "facebookexternalhit", "Facebook"),
        ( "adscanner", "AdScanner"),
        ( "AhrefsBot", "Ahrefs"),
        ( "BLEXBot", "BLEXBot"),
        ( "DotBot", "OpenSite"),
        ( "Mail.RU_Bot", "Mail.ru"),
        ( "MegaIndex", "MegaIndex"),
        ( "SemrushBot", "SEMRush"),
        ( "SEOkicks", "SEOkicks"),
        ( "seoscanners.net", "SEO Scanners"),
        ( "Sistrix", "Sistrix" ),
        ( "WhatsApp", "WhatsApp" ),
        ( "CensysInspect", "CensysInspect" ),
        ( "InternetMeasurement", "InternetMeasurement" ),
        ( "Barkrowler", "Barkrowler" ),
        ( "BrightEdge", "BrightEdge" ),
        ( "ImagesiftBot", "ImagesiftBot" ),
        ( "Cotoyogi", "Cotoyogi" ),
        ( "Applebot", "Applebot" ),
        ( "360Spider", "360Spider" ),
        ( "GeedoProductSearch", "GeedoProductSearch" ),
        // Search engine crawlers
        ( "Google-InspectionTool", "Google-InspectionTool" ),
        ( "GoogleOther", "GoogleOther" ),
        ( "Google-CloudVertexBot", "Google-CloudVertexBot" ),
        ( "DuckDuckBot", "DuckDuckBot" ),
        ( "SeznamBot", "SeznamBot" ),
        ( "Sogou web spider", "Sogou" ), // "Sogou" alone would match the Sogou browser
        ( "Yeti/", "Naver Yeti" ),
        ( "Qwantbot", "Qwantbot" ),
        // SEO crawlers
        ( "AhrefsSiteAudit", "Ahrefs Site Audit" ),
        ( "SiteAuditBot", "SiteAuditBot" ),
        ( "rogerbot", "rogerbot" ),
        ( "Screaming Frog SEO Spider", "Screaming Frog SEO Spider" ),
        ( "DataForSeoBot", "DataForSeoBot" ),
        ( "serpstatbot", "serpstatbot" ),
        // AI crawlers and AI user agents (Claude-SearchBot and Claude-User contain "claudebot@anthropic.com")
        ( "ChatGPT-User", "ChatGPT-User" ),
        ( "Claude-SearchBot", "Claude-SearchBot" ),
        ( "Claude-User", "Claude-User" ),
        ( "ClaudeBot", "ClaudeBot" ),
        ( "Perplexity-User", "Perplexity-User" ),
        ( "Amazonbot", "Amazonbot" ),
        ( "meta-externalfetcher", "meta-externalfetcher" ),
        ( "cohere-ai", "cohere-ai" ),
        ( "Diffbot", "Diffbot" ),
        ( "MistralAI-User", "MistralAI-User" ),
        ( "DuckAssistBot", "DuckAssistBot" ),
        ( "AI2Bot", "AI2Bot" ),
        ( "ShowyouBot", "ShowyouBot" ), // before YouBot, whose key it contains
        ( "YouBot/", "YouBot" ),
    ];

    /// <summary>
    /// Short substrings ("anchors") of the default robot keys. Every key contains at least one anchor,
    /// so a user agent without any anchor cannot match a default key. The few anchors need far fewer searches
    /// than all keys, which makes the common case (a browser) cheap.
    /// </summary>
    internal static string[] CuratedRobotAnchors { get; } =
    [
        "bot", "spider", "crawl", "google", "-user", "external", "slurp", "yahoo",
        "jeeves", "infoseek", "lycos", "yandex", "curious", "archiver", "turnitin", "jobboerse",
        "bingpreview", "checkmark", "adscanner", "megaindex", "seo", "sistrix", "whatsapp", "censys",
        "measurement", "barkrowler", "brightedge", "cotoyogi", "geedo", "ahrefs", "cohere", "yeti/",
    ];

    /// <summary>
    /// The default robots with their matchers.
    /// </summary>
    internal static HttpUserAgentRobotSet DefaultRobotSet { get; } = new(DefaultRobots);

    private static HttpUserAgentRobotSet s_robotSet = DefaultRobotSet;

    /// <summary>
    /// Gets the robots the parser detects.
    /// </summary>
    internal static HttpUserAgentRobotSet RobotSet => Volatile.Read(ref s_robotSet);

    /// <summary>
    /// Replaces the robots the parser detects.
    /// </summary>
    internal static void SetRobotSet(HttpUserAgentRobotSet robotSet) => Volatile.Write(ref s_robotSet, robotSet);

    /// <summary>
    /// Returns <paramref name="curatedAnchors"/> plus every key that contains none of them,
    /// so the anchors always cover all <paramref name="keys"/>.
    /// </summary>
    internal static string[] CreateRobotAnchors(string[] keys, string[] curatedAnchors)
    {
        List<string> anchors = [.. curatedAnchors];
        foreach (string key in keys)
        {
            if (!anchors.Exists(anchor => key.Contains(anchor, StringComparison.OrdinalIgnoreCase)))
            {
                anchors.Add(key);
            }
        }

        return [.. anchors];
    }

    /// <summary>
    /// Mobile device rules in priority order: the most specific match wins.
    /// </summary>
    /// <remarks>
    /// <list type="number">
    /// <item><description>Legacy devices and browsers</description></item>
    /// <item><description>Devices and manufacturers (e.g. "Samsung" before the "Android" operating system)</description></item>
    /// <item><description>Operating systems</description></item>
    /// <item><description>Mobile browsers</description></item>
    /// <item><description>Carriers and other</description></item>
    /// <item><description>Generic fallbacks ("mobile", ...)</description></item>
    /// </list>
    /// A token that contains another token comes first ("palmsource" before "palm", "nintendo dsi" before "nintendo ds",
    /// "SymbianOS" before "symbian").
    /// The order is explicit on purpose: the enumeration order of a (frozen) dictionary is unspecified and must not decide the priority.
    /// </remarks>
    internal static (string Key, string Value)[] Mobiles { get; } =
    [
        // Legacy
        ("mobileexplorer", "Mobile Explorer"),
        ("palmsource", "Palm"),
        ("palmscape", "Palmscape"),
        // Phones and manufacturers
        ("motorola", "Motorola"),
        ("nokia", "Nokia"),
        ("palm", "Palm"),
        ("ipad", "Apple iPad"),
        ("ipod", "Apple iPod"),
        ("iphone", "Apple iPhone"),
        ("sony", "Sony Ericsson"),
        ("ericsson", "Sony Ericsson"),
        ("blackberry", "BlackBerry"),
        ("cocoon", "O2 Cocoon"),
        ("blazer", "Treo"),
        ("lg", "LG"),
        ("amoi", "Amoi"),
        ("xda", "XDA"),
        ("mda", "MDA"),
        ("vario", "Vario"),
        ("htc", "HTC"),
        ("samsung", "Samsung"),
        ("sharp", "Sharp"),
        ("sie-", "Siemens"),
        ("alcatel", "Alcatel"),
        ("benq", "BenQ"),
        ("ipaq", "HP iPaq"),
        ("mot-", "Motorola"),
        ("playstation portable", "PlayStation Portable"),
        ("playstation 3", "PlayStation 3"),
        ("playstation vita", "PlayStation Vita"),
        ("hiptop", "Danger Hiptop"),
        ("nec-", "NEC"),
        ("panasonic", "Panasonic"),
        ("philips", "Philips"),
        ("sagem", "Sagem"),
        ("sanyo", "Sanyo"),
        ("spv", "SPV"),
        ("zte", "ZTE"),
        ("sendo", "Sendo"),
        ("nintendo dsi", "Nintendo DSi"),
        ("nintendo ds", "Nintendo DS"),
        ("nintendo 3ds", "Nintendo 3DS"),
        ("wii", "Nintendo Wii"),
        ("open web", "Open Web"),
        ("openweb", "OpenWeb"),
        // Operating systems
        ("android", "Android"),
        ("SymbianOS", "SymbianOS"),
        ("symbian", "Symbian"),
        ("elaine", "Palm"),
        ("series60", "Symbian S60"),
        ("windows ce", "Windows CE"),
        // Browsers
        ("obigo", "Obigo"),
        ("netfront", "Netfront Browser"),
        ("openwave", "Openwave Browser"),
        ("mobilexplorer", "Mobile Explorer"),
        ("operamini", "Opera Mini"),
        ("opera mini", "Opera Mini"),
        ("opera mobi", "Opera Mobile"),
        ("fennec", "Firefox Mobile"),
        // Other
        ("digital paths", "Digital Paths"),
        ("avantgo", "AvantGo"),
        ("xiino", "Xiino"),
        ("novarra", "Novarra Transcoder"),
        ("vodafone", "Vodafone"),
        ("docomo", "NTT DoCoMo"),
        ("o2", "O2"),
        // Fallback
        ("mobile", "Generic Mobile"),
        ("wireless", "Generic Mobile"),
        ("j2me", "Generic Mobile"),
        ("midp", "Generic Mobile"),
        ("cldc", "Generic Mobile"),
        ("up.link", "Generic Mobile"),
        ("up.browser", "Generic Mobile"),
        ("smartphone", "Generic Mobile"),
        ("cellphone", "Generic Mobile"),
    ];

    /// <summary>
    /// Mobile token matcher.
    /// </summary>
    internal static PriorityTokenMatcher<MobileDetector> MobileMatcher { get; } = new(Array.ConvertAll(Mobiles, static rule => rule.Key));

    /// <summary>
    /// Creates the public, case-insensitive lookup of <see cref="Mobiles"/>.
    /// </summary>
    internal static FrozenDictionary<string, string> CreateMobileDictionary()
        => Mobiles.ToFrozenDictionary(static rule => rule.Key, static rule => rule.Value, StringComparer.OrdinalIgnoreCase);
}
