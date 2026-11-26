// Copyright © https://myCSharp.de - all rights reserved

using System.Text.RegularExpressions;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests;

/// <summary>
/// Ensures the public collections keep their contents and regex semantics (patterns, options, timeouts, groups)
/// now that the regular expressions are source-generated instead of being compiled at runtime.
/// </summary>
public class HttpUserAgentStaticsTests
{
    private static readonly TimeSpan s_matchTimeout = TimeSpan.FromSeconds(1);

    /// <summary>
    /// The published platform definitions: token (escaped to the regex pattern), name and type, in declaration order.
    /// </summary>
    private static readonly (string Token, string Name, HttpUserAgentPlatformType Type)[] s_expectedPlatforms =
    [
        ("windows nt 10.0", "Windows 10", HttpUserAgentPlatformType.Windows),
        ("windows nt 6.3", "Windows 8.1", HttpUserAgentPlatformType.Windows),
        ("windows nt 6.2", "Windows 8", HttpUserAgentPlatformType.Windows),
        ("windows nt 6.1", "Windows 7", HttpUserAgentPlatformType.Windows),
        ("windows nt 6.0", "Windows Vista", HttpUserAgentPlatformType.Windows),
        ("windows nt 5.2", "Windows 2003", HttpUserAgentPlatformType.Windows),
        ("windows nt 5.1", "Windows XP", HttpUserAgentPlatformType.Windows),
        ("windows nt 5.0", "Windows 2000", HttpUserAgentPlatformType.Windows),
        ("windows nt 4.0", "Windows NT 4.0", HttpUserAgentPlatformType.Windows),
        ("winnt4.0", "Windows NT 4.0", HttpUserAgentPlatformType.Windows),
        ("winnt 4.0", "Windows NT", HttpUserAgentPlatformType.Windows),
        ("winnt", "Windows NT", HttpUserAgentPlatformType.Windows),
        ("windows 98", "Windows 98", HttpUserAgentPlatformType.Windows),
        ("win98", "Windows 98", HttpUserAgentPlatformType.Windows),
        ("windows 95", "Windows 95", HttpUserAgentPlatformType.Windows),
        ("win95", "Windows 95", HttpUserAgentPlatformType.Windows),
        ("windows phone", "Windows Phone", HttpUserAgentPlatformType.Windows),
        ("windows", "Unknown Windows OS", HttpUserAgentPlatformType.Windows),
        ("android", "Android", HttpUserAgentPlatformType.Android),
        ("blackberry", "BlackBerry", HttpUserAgentPlatformType.BlackBerry),
        ("iphone", "iOS", HttpUserAgentPlatformType.IOS),
        ("ipad", "iOS", HttpUserAgentPlatformType.IOS),
        ("ipod", "iOS", HttpUserAgentPlatformType.IOS),
        ("cros", "ChromeOS", HttpUserAgentPlatformType.ChromeOS),
        ("os x", "Mac OS X", HttpUserAgentPlatformType.MacOS),
        ("ppc mac", "Power PC Mac", HttpUserAgentPlatformType.MacOS),
        ("freebsd", "FreeBSD", HttpUserAgentPlatformType.Unix),
        ("ppc", "Macintosh", HttpUserAgentPlatformType.MacOS),
        ("linux", "Linux", HttpUserAgentPlatformType.Linux),
        ("debian", "Debian", HttpUserAgentPlatformType.Linux),
        ("sunos", "Sun Solaris", HttpUserAgentPlatformType.Unix),
        ("beos", "BeOS", HttpUserAgentPlatformType.Generic),
        ("apachebench", "ApacheBench", HttpUserAgentPlatformType.Generic),
        ("aix", "AIX", HttpUserAgentPlatformType.Unix),
        ("irix", "Irix", HttpUserAgentPlatformType.Unix),
        ("osf", "DEC OSF", HttpUserAgentPlatformType.Unix),
        ("hp-ux", "HP-UX", HttpUserAgentPlatformType.Unix),
        ("netbsd", "NetBSD", HttpUserAgentPlatformType.Unix),
        ("bsdi", "BSDi", HttpUserAgentPlatformType.Unix),
        ("openbsd", "OpenBSD", HttpUserAgentPlatformType.Unix),
        ("gnu", "GNU/Linux", HttpUserAgentPlatformType.Linux),
        ("unix", "Unknown Unix OS", HttpUserAgentPlatformType.Unix),
        ("symbian", "Symbian OS", HttpUserAgentPlatformType.Symbian),
        ("macintosh", "Macintosh", HttpUserAgentPlatformType.MacOS),
    ];

    /// <summary>
    /// The published browser definitions: token (pattern = "{token}.*?([0-9\.]+)") and name.
    /// </summary>
    private static readonly (string Token, string Name)[] s_expectedBrowsers =
    [
        ("OPR", "Opera"), ("Silk", "Amazon Silk"), ("Flock", "Flock"), ("Edge", "Edge"), ("EdgA", "Edge"), ("Edg", "Edge"), ("EdgiOS", "Edge"),
        ("Vivaldi", "Vivaldi"), ("Brave Chrome", "Brave"), ("SamsungBrowser", "Samsung Internet"), ("YaBrowser", "Yandex Browser"),
        ("UCBrowser", "UC Browser"), ("UCMobile", "UC Browser"), ("DuckDuckGo", "DuckDuckGo"), ("Whale", "Whale"), ("QQBrowser", "QQ Browser"),
        ("HuaweiBrowser", "Huawei Browser"), ("XiaoMi/MiuiBrowser", "Miui Browser"), ("MiuiBrowser", "Miui Browser"),
        ("Instagram", "Instagram"), ("FBAV", "Facebook"), ("Chrome", "Chrome"), ("CriOS", "Chrome"), ("HeadlessChrome", "Chrome"),
        ("Opera.*?Version", "Opera"), ("Opera", "Opera"), ("MSIE", "Internet Explorer"), ("Internet Explorer", "Internet Explorer"),
        ("Trident.* rv", "Internet Explorer"), ("Shiira", "Shiira"), ("Firefox", "Firefox"), ("FxiOS", "Firefox"), ("Chimera", "Chimera"),
        ("Phoenix", "Phoenix"), ("Firebird", "Firebird"), ("Camino", "Camino"), ("Netscape", "Netscape"), ("OmniWeb", "OmniWeb"),
        ("Version", "Safari"), ("Konqueror", "Konqueror"), ("icab", "iCab"), ("Lynx", "Lynx"), ("ELinks", "ELinks"), ("Links", "Links"),
        ("hotjava", "HotJava"), ("amaya", "Amaya"), ("IBrowse", "IBrowse"), ("Maxthon", "Maxthon"), ("ipod touch", "Apple iPod"),
        ("Ubuntu", "Ubuntu Web Browser"),
    ];

    [Fact]
    public void Platforms_Keep_Definitions_And_Order()
    {
        (string Pattern, string Name, HttpUserAgentPlatformType Type)[] actual =
            [.. HttpUserAgentStatics.Platforms.Select(static p => (p.Regex.ToString(), p.Name, p.PlatformType))];

        Assert.Equal(s_expectedPlatforms.Select(static p => (Regex.Escape(p.Token), p.Name, p.Type)), actual);
    }

    [Fact]
    public void Platforms_Use_Source_Generated_Regex_With_Previous_Options()
    {
        foreach (HttpUserAgentPlatformInformation platform in HttpUserAgentStatics.Platforms)
        {
            AssertSourceGenerated(platform.Regex);
            Assert.Same(platform.Regex, platform.Regex);
        }
    }

    [Fact]
    public void Platforms_Regex_Matches_Case_Insensitive()
    {
        HttpUserAgentPlatformInformation windows10 = HttpUserAgentStatics.Platforms.First(static p => string.Equals(p.Name, "Windows 10", StringComparison.Ordinal));

        Assert.Matches(windows10.Regex, "Mozilla/5.0 (WINDOWS NT 10.0; Win64; x64)");
        Assert.DoesNotMatch(windows10.Regex, "Mozilla/5.0 (Windows NT 10x0; Win64; x64)");
    }

    [Fact]
    public void Browsers_Keep_Definitions()
    {
        Dictionary<string, string> actual = HttpUserAgentStatics.Browsers.ToDictionary(static b => b.Key.ToString(), static b => b.Value, StringComparer.Ordinal);
        Dictionary<string, string> expected = s_expectedBrowsers.ToDictionary(static b => $@"{b.Token}.*?([0-9\.]+)", static b => b.Name, StringComparer.Ordinal);

        Assert.Equal(expected, actual);
    }

    [Fact]
    public void Browsers_Cover_All_Parser_Browser_Rules()
    {
        HashSet<string> expectedNames = new(HttpUserAgentRules.Browsers.Select(static rule => rule.Name), StringComparer.Ordinal);
        HashSet<string> actualNames = new(HttpUserAgentStatics.Browsers.Values, StringComparer.Ordinal);

        Assert.Equal(expectedNames, actualNames);

        foreach ((string name, string detectToken, string? versionToken) in HttpUserAgentRules.Browsers)
        {
            string expectedToken = detectToken.TrimEnd('/');
            bool matched = HttpUserAgentStatics.Browsers.Any(entry =>
                string.Equals(entry.Value, name, StringComparison.Ordinal)
                && entry.Key.ToString().StartsWith(expectedToken, StringComparison.Ordinal));

            Assert.True(matched, $"HttpUserAgentStatics.Browsers is missing rule '{name}' ({detectToken}, {versionToken}).");
        }
    }

    [Fact]
    public void Browsers_Use_Source_Generated_Regex_With_Previous_Options()
    {
        foreach (Regex regex in HttpUserAgentStatics.Browsers.Keys)
        {
            AssertSourceGenerated(regex);
        }
    }

    [Fact]
    public void Browsers_Regex_Captures_Version_In_Group_1()
    {
        Regex chrome = HttpUserAgentStatics.Browsers.Single(static b => b.Key.ToString().StartsWith("Chrome", StringComparison.Ordinal)).Key;

        Match match = chrome.Match("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) chrome/158.0.0.0 Safari/537.36");

        Assert.True(match.Success);
        Assert.Equal("158.0.0.0", match.Groups[1].Value);
    }

    [Fact]
    public void Mobiles_Contain_All_Rules()
    {
        Assert.Equal(HttpUserAgentRules.Mobiles.Length, HttpUserAgentStatics.Mobiles.Count);
        foreach ((string key, string value) in HttpUserAgentRules.Mobiles)
        {
            Assert.Equal(value, HttpUserAgentStatics.Mobiles[key]);
            Assert.Equal(value, HttpUserAgentStatics.Mobiles[key.ToUpperInvariant()]);
            Assert.Equal(value, HttpUserAgentParser.GetMobileDevice(key.ToUpperInvariant()));
        }
    }

    [Fact]
    public void DefaultRobots_Are_The_Robots_Of_The_Library()
    {
        Assert.Equal(HttpUserAgentRules.DefaultRobots, HttpUserAgentStatics.DefaultRobots);
        Assert.Contains(("googlebot", "Googlebot"), HttpUserAgentStatics.DefaultRobots);
        Assert.Distinct(HttpUserAgentStatics.DefaultRobots.Select(static robot => robot.Key), StringComparer.OrdinalIgnoreCase);
        foreach ((string key, string name) in HttpUserAgentStatics.DefaultRobots)
        {
            Assert.Equal(name, HttpUserAgentParser.GetRobot(key.ToUpperInvariant()));
        }
    }

    [Fact]
    public void Tools_Contains_Curl()
    {
        Assert.Equal("curl", HttpUserAgentStatics.Tools["CURL"]);
    }

    [Fact]
    public void Parser_Platform_Rules_Resolve_The_Regex_Of_Their_Token()
    {
        foreach ((string token, HttpUserAgentPlatformInformation platform) in HttpUserAgentRules.Platforms)
        {
            Assert.Equal(Regex.Escape(token), platform.Regex.ToString());
            Assert.Matches(platform.Regex, token);
            Assert.Contains(platform, HttpUserAgentStatics.Platforms);
            Assert.Equal(platform, HttpUserAgentParser.GetPlatform(token));
        }
    }

    [Fact]
    public void Each_Browser_Rule_Can_Be_Reached_Through_Parse()
    {
        foreach ((string name, string product, string? versionToken) in HttpUserAgentRules.Browsers)
        {
            string ua = product.TrimEnd('/') + "/2.3";
            string expectedVersion = "2.3";
            if (versionToken is not null && !string.Equals(product, versionToken, StringComparison.Ordinal))
            {
                ua += " " + versionToken + "4.5";
                expectedVersion = "4.5";
            }

            HttpUserAgentInformation result = HttpUserAgentParser.Parse(ua);
            Assert.Equal((HttpUserAgentType.Browser, name, expectedVersion), (result.Type, result.Name, result.Version));
        }
    }

    [Fact]
    public void Platform_Regex_Ids_Resolve_To_Distinct_Regexes()
    {
        HttpUserAgentPlatformRegexId[] ids = [.. Enum.GetValues<HttpUserAgentPlatformRegexId>().Where(static id => id != HttpUserAgentPlatformRegexId.None)];

        Regex[] regexes = [.. ids.Select(static id => HttpUserAgentRegexes.GetPlatformRegex(id)!)];

        Assert.All(regexes, static regex => Assert.NotNull(regex));
        Assert.Equal(ids.Length, regexes.Distinct().Count());
        Assert.Equal(HttpUserAgentStatics.Platforms.Count, ids.Length);
    }

    [Theory]
    [InlineData((byte)HttpUserAgentPlatformRegexId.None)]
    [InlineData(byte.MaxValue)]
    public void Platform_Regex_Id_Without_Regex_Returns_Null(byte id)
    {
        Assert.Null(HttpUserAgentRegexes.GetPlatformRegex((HttpUserAgentPlatformRegexId)id));
    }

    private static void AssertSourceGenerated(Regex regex)
    {
        Assert.Equal(RegexOptions.IgnoreCase, regex.Options);
        Assert.Equal(s_matchTimeout, regex.MatchTimeout);

        // Source-generated regexes are Regex subclasses emitted into the declaring assembly.
        Assert.NotEqual(typeof(Regex), regex.GetType());
        Assert.Same(typeof(HttpUserAgentRegexes).Assembly, regex.GetType().Assembly);
    }
}
