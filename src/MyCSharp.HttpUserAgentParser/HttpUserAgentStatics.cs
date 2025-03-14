// Copyright © myCSharp.de - all rights reserved

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
    private static Regex CreateDefaultPlatformRegex(string key) => new(Regex.Escape($"{key}"), DefaultPlatformsRegexFlags);

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
    /// Regex defauls for browser mappings
    /// </summary>
    private const RegexOptions DefaultBrowserRegexFlags = RegexOptions.IgnoreCase | RegexOptions.Compiled;
    /// <summary>
    /// Creates default browser mapping regex
    /// </summary>
    private static Regex CreateDefaultBrowserRegex(string key)
        => new($@"{key}.*?([0-9\.]+)", DefaultBrowserRegexFlags);

    /// <summary>
    /// Browsers
    /// </summary>
    public static Dictionary<Regex, string> Browsers = new()
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
    };

    /// <summary>
    /// Mobiles
    /// </summary>
    public static readonly Dictionary<string, string> Mobiles = new()
    {
        // Legacy
        { "mobileexplorer", "Mobile Explorer" },
        { "palmsource", "Palm" },
        { "palmscape", "Palmscape" },
        // Phones and Manufacturers
        { "motorola", "Motorola" },
        { "nokia", "Nokia" },
        { "palm", "Palm" },
        { "ipad", "Apple iPad" },
        { "ipod", "Apple iPod" },
        { "iphone", "Apple iPhone" },
        { "sony", "Sony Ericsson" },
        { "ericsson", "Sony Ericsson" },
        { "blackberry", "BlackBerry" },
        { "cocoon", "O2 Cocoon" },
        { "blazer", "Treo" },
        { "lg", "LG" },
        { "amoi", "Amoi" },
        { "xda", "XDA" },
        { "mda", "MDA" },
        { "vario", "Vario" },
        { "htc", "HTC" },
        { "samsung", "Samsung" },
        { "sharp", "Sharp" },
        { "sie-", "Siemens" },
        { "alcatel", "Alcatel" },
        { "benq", "BenQ" },
        { "ipaq", "HP iPaq" },
        { "mot-", "Motorola" },
        { "playstation portable", "PlayStation Portable" },
        { "playstation 3", "PlayStation 3" },
        { "playstation vita", "PlayStation Vita" },
        { "hiptop", "Danger Hiptop" },
        { "nec-", "NEC" },
        { "panasonic", "Panasonic" },
        { "philips", "Philips" },
        { "sagem", "Sagem" },
        { "sanyo", "Sanyo" },
        { "spv", "SPV" },
        { "zte", "ZTE" },
        { "sendo", "Sendo" },
        { "nintendo dsi", "Nintendo DSi" },
        { "nintendo ds", "Nintendo DS" },
        { "nintendo 3ds", "Nintendo 3DS" },
        { "wii", "Nintendo Wii" },
        { "open web", "Open Web" },
        { "openweb", "OpenWeb" },
        // Operating Systems
        { "android", "Android" },
        { "symbian", "Symbian" },
        { "SymbianOS", "SymbianOS" },
        { "elaine", "Palm" },
        { "series60", "Symbian S60" },
        { "windows ce", "Windows CE" },
        // Browsers
        { "obigo", "Obigo" },
        { "netfront", "Netfront Browser" },
        { "openwave", "Openwave Browser" },
        { "mobilexplorer", "Mobile Explorer" },
        { "operamini", "Opera Mini" },
        { "opera mini", "Opera Mini" },
        { "opera mobi", "Opera Mobile" },
        { "fennec", "Firefox Mobile" },
        // Other
        { "digital paths", "Digital Paths" },
        { "avantgo", "AvantGo" },
        { "xiino", "Xiino" },
        { "novarra", "Novarra Transcoder" },
        { "vodafone", "Vodafone" },
        { "docomo", "NTT DoCoMo" },
        { "o2", "O2" },
        // Fallback
        { "mobile", "Generic Mobile" },
        { "wireless", "Generic Mobile" },
        { "j2me", "Generic Mobile" },
        { "midp", "Generic Mobile" },
        { "cldc", "Generic Mobile" },
        { "up.link", "Generic Mobile" },
        { "up.browser", "Generic Mobile" },
        { "smartphone", "Generic Mobile" },
        { "cellphone", "Generic Mobile" },
    };

    /// <summary>
    /// Robots
    /// </summary>
    public static readonly (string Key, string Value)[] Robots =
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
        ( "openai.com/searchbot", "OAI-SearchBot" ),
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
        ( "Cotoyogi", "Cotoyogi" )
    ];

    /// <summary>
    /// Tools
    /// </summary>
    public static readonly Dictionary<string, string> Tools = new()
    {
        { "curl", "curl" }
    };
}
