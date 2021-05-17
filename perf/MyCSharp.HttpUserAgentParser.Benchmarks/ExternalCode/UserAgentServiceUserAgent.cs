// Copyright © myCSharp 2020-2021, all rights reserved

using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace MyCSharp.HttpUserAgentParser.Benchmarks.ExternalCode
{
    /// <summary>
    /// from https://raw.githubusercontent.com/DannyBoyNg/UserAgentService/master/UserAgentService/UserAgent.cs
    /// as copy because ctor is internal
    /// </summary>
    internal class UserAgentServiceUserAgent
    {
        internal static Dictionary<string, string> platforms = new()
        {
            { "windows nt 10.0", "Windows 10" },
            { "windows nt 6.3", "Windows 8.1" },
            { "windows nt 6.2", "Windows 8" },
            { "windows nt 6.1", "Windows 7" },
            { "windows nt 6.0", "Windows Vista" },
            { "windows nt 5.2", "Windows 2003" },
            { "windows nt 5.1", "Windows XP" },
            { "windows nt 5.0", "Windows 2000" },
            { "windows nt 4.0", "Windows NT 4.0" },
            { "winnt4.0", "Windows NT 4.0" },
            { "winnt 4.0", "Windows NT" },
            { "winnt", "Windows NT" },
            { "windows 98", "Windows 98" },
            { "win98", "Windows 98" },
            { "windows 95", "Windows 95" },
            { "win95", "Windows 95" },
            { "windows phone", "Windows Phone" },
            { "windows", "Unknown Windows OS" },
            { "android", "Android" },
            { "blackberry", "BlackBerry" },
            { "iphone", "iOS" },
            { "ipad", "iOS" },
            { "ipod", "iOS" },
            { "os x", "Mac OS X" },
            { "ppc mac", "Power PC Mac" },
            { "freebsd", "FreeBSD" },
            { "ppc", "Macintosh" },
            { "linux", "Linux" },
            { "debian", "Debian" },
            { "sunos", "Sun Solaris" },
            { "beos", "BeOS" },
            { "apachebench", "ApacheBench" },
            { "aix", "AIX" },
            { "irix", "Irix" },
            { "osf", "DEC OSF" },
            { "hp-ux", "HP-UX" },
            { "netbsd", "NetBSD" },
            { "bsdi", "BSDi" },
            { "openbsd", "OpenBSD" },
            { "gnu", "GNU/Linux" },
            { "unix", "Unknown Unix OS" },
            { "symbian", "Symbian OS" },
        };

        internal static Dictionary<string, string> browsers = new()
        {
            { "OPR", "Opera" },
            { "Flock", "Flock" },
            { "Edge", "Edge" },
            { "Edg", "Edge" },
            { "Chrome", "Chrome" },
            { "Opera.*?Version", "Opera" },
            { "Opera", "Opera" },
            { "MSIE", "Internet Explorer" },
            { "Internet Explorer", "Internet Explorer" },
            { "Trident.* rv", "Internet Explorer" },
            { "Shiira", "Shiira" },
            { "Firefox", "Firefox" },
            { "Chimera", "Chimera" },
            { "Phoenix", "Phoenix" },
            { "Firebird", "Firebird" },
            { "Camino", "Camino" },
            { "Netscape", "Netscape" },
            { "OmniWeb", "OmniWeb" },
            { "Safari", "Safari" },
            { "Mozilla", "Mozilla" },
            { "Konqueror", "Konqueror" },
            { "icab", "iCab" },
            { "Lynx", "Lynx" },
            { "Links", "Links" },
            { "hotjava", "HotJava" },
            { "amaya", "Amaya" },
            { "IBrowse", "IBrowse" },
            { "Maxthon", "Maxthon" },
            { "Ubuntu", "Ubuntu Web Browser" },
            { "Vivaldi", "Vivaldi" },
        };

        internal static Dictionary<string, string> mobiles = new()
        {
            // Legacy
            { "mobileexplorer", "Mobile Explorer" },
            { "palmsource", "Palm" },
            { "palmscape", "Palmscape" },
            // Phones and Manufacturers
            { "motorola", "Motorola" },
            { "nokia", "Nokia" },
            { "palm", "Palm" },
            { "iphone", "Apple iPhone" },
            { "ipad", "iPad" },
            { "ipod", "Apple iPod Touch" },
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

        internal static Dictionary<string, string> robots = new()
        {
            { "googlebot", "Googlebot" },
            { "msnbot", "MSNBot" },
            { "baiduspider", "Baiduspider" },
            { "bingbot", "Bing" },
            { "slurp", "Inktomi Slurp" },
            { "yahoo", "Yahoo" },
            { "ask jeeves", "Ask Jeeves" },
            { "fastcrawler", "FastCrawler" },
            { "infoseek", "InfoSeek Robot 1.0" },
            { "lycos", "Lycos" },
            { "yandex", "YandexBot" },
            { "mediapartners-google", "MediaPartners Google" },
            { "CRAZYWEBCRAWLER", "Crazy Webcrawler" },
            { "adsbot-google", "AdsBot Google" },
            { "feedfetcher-google", "Feedfetcher Google" },
            { "curious george", "Curious George" },
            { "ia_archiver", "Alexa Crawler" },
            { "MJ12bot", "Majestic-12" },
            { "Uptimebot", "Uptimebot" },
        };

        internal string Agent = "";

        /// <summary>
        /// Gets or sets a value indicating whether this UserAgent is a browser.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this UserAgent is a browser; otherwise, <c>false</c>.
        /// </value>
        public bool IsBrowser { get; set; } = false;
        /// <summary>
        /// Gets or sets a value indicating whether this UserAgent is a robot.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this UserAgent is a robot; otherwise, <c>false</c>.
        /// </value>
        public bool IsRobot { get; set; } = false;
        /// <summary>
        /// Gets or sets a value indicating whether this UserAgent is a mobile device.
        /// </summary>
        /// <value>
        ///   <c>true</c> if this UserAgent is a mobile device; otherwise, <c>false</c>.
        /// </value>
        public bool IsMobile { get; set; } = false;
        /// <summary>
        /// Gets or sets the platform.
        /// </summary>
        /// <value>
        /// The platform or operating system.
        /// </value>
        public string Platform { get; set; } = "";
        /// <summary>
        /// Gets or sets the browser.
        /// </summary>
        /// <value>
        /// The browser.
        /// </value>
        public string Browser { get; set; } = "";
        /// <summary>
        /// Gets or sets the browser version.
        /// </summary>
        /// <value>
        /// The browser version.
        /// </value>
        public string BrowserVersion { get; set; } = "";
        /// <summary>
        /// Gets or sets the mobile device.
        /// </summary>
        /// <value>
        /// The mobile device.
        /// </value>
        public string Mobile { get; set; } = "";
        /// <summary>
        /// Gets or sets the robot.
        /// </summary>
        /// <value>
        /// The robot.
        /// </value>
        public string Robot { get; set; } = "";

        internal UserAgentServiceUserAgent(string? userAgentString = null)
        {
            if (userAgentString != null)
            {
                Agent = userAgentString.Trim();
                this.SetPlatform();
                if (this.SetRobot()) return;
                if (this.SetBrowser()) return;
                if (this.SetMobile()) return;
            }
        }

        internal bool SetPlatform()
        {
            foreach (var item in platforms)
            {
                if (Regex.IsMatch(Agent, $"{Regex.Escape(item.Key)}", RegexOptions.IgnoreCase))
                {
                    this.Platform = item.Value;
                    return true;
                }
            }
            this.Platform = "Unknown Platform";
            return false;
        }

        internal bool SetBrowser()
        {
            foreach (var item in browsers)
            {
                var match = Regex.Match(Agent, $@"{item.Key}.*?([0-9\.]+)", RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    this.IsBrowser = true;
                    this.BrowserVersion = match.Groups[1].Value;
                    this.Browser = item.Value;
                    this.SetMobile();
                    return true;
                }
            }
            return false;
        }

        internal bool SetRobot()
        {
            foreach (var item in robots)
            {
                if (Regex.IsMatch(Agent, $"{Regex.Escape(item.Key)}", RegexOptions.IgnoreCase))
                {
                    this.IsRobot = true;
                    this.Robot = item.Value;
                    this.SetMobile();
                    return true;
                }
            }
            return false;
        }

        internal bool SetMobile()
        {
            foreach (var item in mobiles)
            {
                if (Agent?.IndexOf(item.Key, StringComparison.OrdinalIgnoreCase) != -1)
                {
                    this.IsMobile = true;
                    this.Mobile = item.Value;
                    return true;
                }
            }
            return false;
        }
    }
}
