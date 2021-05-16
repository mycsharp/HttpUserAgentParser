﻿using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MyCSharp.HttpUserAgentParser
{
    public static class HttpUserAgentStatics
    {
        public static readonly HashSet<HttpUserAgentPlatformInformation> Platforms = new()
        {
            new("windows nt 10.0", "Windows 10", HttpUserAgentPlatformType.Windows),
            new("windows nt 6.3", "Windows 8.1", HttpUserAgentPlatformType.Windows),
            new("windows nt 6.2", "Windows 8", HttpUserAgentPlatformType.Windows),
            new("windows nt 6.1", "Windows 7", HttpUserAgentPlatformType.Windows),
            new("windows nt 6.0", "Windows Vista", HttpUserAgentPlatformType.Windows),
            new("windows nt 5.2", "Windows 2003", HttpUserAgentPlatformType.Windows),
            new("windows nt 5.1", "Windows XP", HttpUserAgentPlatformType.Windows),
            new("windows nt 5.0", "Windows 2000", HttpUserAgentPlatformType.Windows),
            new("windows nt 4.0", "Windows NT 4.0", HttpUserAgentPlatformType.Windows),
            new("winnt4.0", "Windows NT 4.0", HttpUserAgentPlatformType.Windows),
            new("winnt 4.0", "Windows NT", HttpUserAgentPlatformType.Windows),
            new("winnt", "Windows NT", HttpUserAgentPlatformType.Windows),
            new("windows 98", "Windows 98", HttpUserAgentPlatformType.Windows),
            new("win98", "Windows 98", HttpUserAgentPlatformType.Windows),
            new("windows 95", "Windows 95", HttpUserAgentPlatformType.Windows),
            new("win95", "Windows 95", HttpUserAgentPlatformType.Windows),
            new("windows phone", "Windows Phone", HttpUserAgentPlatformType.Windows),
            new("windows", "Unknown Windows OS", HttpUserAgentPlatformType.Windows),
            new("android", "Android", HttpUserAgentPlatformType.Android),
            new("blackberry", "BlackBerry", HttpUserAgentPlatformType.BlackBerry),
            new("iphone", "iOS", HttpUserAgentPlatformType.IOS),
            new("ipad", "iOS", HttpUserAgentPlatformType.IOS),
            new("ipod", "iOS", HttpUserAgentPlatformType.IOS),
            new("os x", "Mac OS X", HttpUserAgentPlatformType.MacOS),
            new("ppc mac", "Power PC Mac", HttpUserAgentPlatformType.MacOS),
            new("freebsd", "FreeBSD", HttpUserAgentPlatformType.Linux),
            new("ppc", "Macintosh", HttpUserAgentPlatformType.Linux),
            new("linux", "Linux", HttpUserAgentPlatformType.Linux),
            new("debian", "Debian", HttpUserAgentPlatformType.Linux),
            new("sunos", "Sun Solaris", HttpUserAgentPlatformType.Generic),
            new("beos", "BeOS", HttpUserAgentPlatformType.Generic),
            new("apachebench", "ApacheBench", HttpUserAgentPlatformType.Generic),
            new("aix", "AIX", HttpUserAgentPlatformType.Generic),
            new("irix", "Irix", HttpUserAgentPlatformType.Generic),
            new("osf", "DEC OSF", HttpUserAgentPlatformType.Generic),
            new("hp-ux", "HP-UX", HttpUserAgentPlatformType.Windows),
            new("netbsd", "NetBSD", HttpUserAgentPlatformType.Generic),
            new("bsdi", "BSDi", HttpUserAgentPlatformType.Generic),
            new("openbsd", "OpenBSD", HttpUserAgentPlatformType.Unix),
            new("gnu", "GNU/Linux", HttpUserAgentPlatformType.Linux),
            new("unix", "Unknown Unix OS", HttpUserAgentPlatformType.Unix),
            new("symbian", "Symbian OS", HttpUserAgentPlatformType.Symbian),
        };

        private const RegexOptions DefaultBrowserRegexFlags = RegexOptions.IgnoreCase | RegexOptions.Compiled;
        private static Regex CreateDefaultRegex(string key) => new($@"{key}.*?([0-9\.]+)", DefaultBrowserRegexFlags);
        internal static Dictionary<Regex, string> Browsers = new()
        {
            { CreateDefaultRegex("OPR"), "Opera" },
            { CreateDefaultRegex("Flock"), "Flock" },
            { CreateDefaultRegex("Edge"), "Edge" },
            { CreateDefaultRegex("EdgA"), "Edge" },
            { CreateDefaultRegex("Edg"), "Edge" },
            { CreateDefaultRegex("Vivaldi"), "Vivaldi" },
            { CreateDefaultRegex("Brave Chrome"), "Brave" },
            { CreateDefaultRegex("Chrome"), "Chrome" },
            { CreateDefaultRegex("CriOS"), "Chrome" },
            { CreateDefaultRegex("Opera.*?Version"), "Opera" },
            { CreateDefaultRegex("Opera"), "Opera" },
            { CreateDefaultRegex("MSIE"), "Internet Explorer" },
            { CreateDefaultRegex("Internet Explorer"), "Internet Explorer" },
            { CreateDefaultRegex("Trident.* rv"), "Internet Explorer" },
            { CreateDefaultRegex("Shiira"), "Shiira" },
            { CreateDefaultRegex("Firefox"), "Firefox" },
            { CreateDefaultRegex("FxiOS"), "Firefox" },
            { CreateDefaultRegex("Chimera"), "Chimera" },
            { CreateDefaultRegex("Phoenix"), "Phoenix" },
            { CreateDefaultRegex("Firebird"), "Firebird" },
            { CreateDefaultRegex("Camino"), "Camino" },
            { CreateDefaultRegex("Netscape"), "Netscape" },
            { CreateDefaultRegex("OmniWeb"), "OmniWeb" },
            { CreateDefaultRegex("Safari"), "Safari" },
            { CreateDefaultRegex("Mozilla"), "Mozilla" },
            { CreateDefaultRegex("Konqueror"), "Konqueror" },
            { CreateDefaultRegex("icab"), "iCab" },
            { CreateDefaultRegex("Lynx"), "Lynx" },
            { CreateDefaultRegex("Links"), "Links" },
            { CreateDefaultRegex("hotjava"), "HotJava" },
            { CreateDefaultRegex("amaya"), "Amaya" },
            { CreateDefaultRegex("IBrowse"), "IBrowse" },
            { CreateDefaultRegex("Maxthon"), "Maxthon" },
            { CreateDefaultRegex("ipod touch"), "Apple iPod" },
            { CreateDefaultRegex("Ubuntu"), "Ubuntu Web Browser" },
        };

        internal static readonly Dictionary<string, string> Mobiles = new()
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

        internal static readonly Dictionary<string, string> Robots = new()
        {
            { "googlebot", "Googlebot" },
            { "googleweblight", "Google Web Light" },
            { "PetalBot", "PetalBot" },
            { "DuplexWeb-Google", "DuplexWeb-Google" },
            { "Storebot-Google", "Storebot-Google" },
            { "msnbot", "MSNBot" },
            { "baiduspider", "Baiduspider" },
            { "Google Favicon", "Google Favicon" },
            { "Jobboerse", "Jobboerse" },
            { "bingbot", "BingBot" },
            { "BingPreview", "Bing Preview" },
            { "slurp", "Slurp" },
            { "yahoo", "Yahoo" },
            { "ask jeeves", "Ask Jeeves" },
            { "fastcrawler", "FastCrawler" },
            { "infoseek", "InfoSeek Robot 1.0" },
            { "lycos", "Lycos" },
            { "YandexBot", "YandexBot" },
            { "YandexImages", "YandexImages" },
            { "mediapartners-google", "Mediapartners Google" },
            { "apis-google", "APIs Google" },
            { "CRAZYWEBCRAWLER", "Crazy Webcrawler" },
            { "AdsBot-Google-Mobile", "AdsBot Google Mobile" },
            { "adsbot-google", "AdsBot Google" },
            { "feedfetcher-google", "FeedFetcher-Google" },
            { "google-read-aloud", "Google-Read-Aloud" },
            { "curious george", "Curious George" },
            { "ia_archiver", "Alexa Crawler" },
            { "MJ12bot", "Majestic" },
            { "Uptimebot", "Uptimebot" },
            { "CheckMarkNetwork", "CheckMark" },
            { "facebookexternalhit", "Facebook" },
            { "adscanner", "AdScanner" },
            { "AhrefsBot", "Ahrefs" },
            { "BLEXBot", "BLEXBot" },
            { "DotBot", "OpenSite" },
            { "Mail.RU_Bot", "Mail.ru" },
            { "MegaIndex", "MegaIndex" },
            { "SemrushBot", "SEMRush" },
            { "SEOkicks", "SEOkicks" },
            { "seoscanners.net", "SEO Scanners" },
            { "Sistrix", "Sistrix" }
        };

        internal static readonly Dictionary<string, string> Tools = new()
        {
            { "curl", "curl" }
        };
    }
}