// Copyright © https://myCSharp.de - all rights reserved

using MyCSharp.HttpUserAgentParser.TestHelpers;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests;

public class PriorityTokenMatcherTests
{
    private readonly struct TestDetector;

    private static PriorityTokenMatcher<TestDetector> Create(string[] tokens, params ReadOnlySpan<int> groupStarts) => new(tokens, groupStarts);

    [Fact]
    public void Find_Returns_First_Token_In_Priority_Order_Not_Leftmost_Occurrence()
    {
        PriorityTokenMatcher<TestDetector> matcher = Create(["beta", "alpha"]);

        Assert.Equal(0, matcher.Find("alpha beta", 0, out int position));
        Assert.Equal(6, position);
        Assert.Equal(2, matcher.Count);
    }

    [Fact]
    public void Find_Starts_At_StartIndex()
    {
        PriorityTokenMatcher<TestDetector> matcher = Create(["beta", "alpha"]);

        Assert.Equal(1, matcher.Find("alpha beta", 1, out int position));
        Assert.Equal(0, position);
    }

    [Fact]
    public void Find_Is_Case_Insensitive()
    {
        PriorityTokenMatcher<TestDetector> matcher = Create(["iphone"]);

        Assert.Equal(0, matcher.Find("(IPHONE; CPU)", 0, out int position));
        Assert.Equal(1, position);
    }

    [Fact]
    public void Find_Returns_Minus_One_Without_Match()
    {
        PriorityTokenMatcher<TestDetector> matcher = Create(["alpha", "beta", "lg"]);

        Assert.Equal(-1, matcher.Find("gamma", 0, out int position));
        Assert.Equal(-1, position);
    }

    [Fact]
    public void Find_Without_Tokens_Returns_Minus_One()
    {
        PriorityTokenMatcher<TestDetector> matcher = Create([]);

        Assert.Equal(-1, matcher.Find("anything", 0, out _));
    }

    [Fact]
    public void Find_Skips_Token_After_Covered_Token_When_StartIndex_Is_Inside_Group()
    {
        // "edg" is the group filter for "edge"; starting at "edge" with only "edg" present must not match.
        PriorityTokenMatcher<TestDetector> matcher = Create(["edg", "edge", "other"]);

        Assert.Equal(-1, matcher.Find("edgx", 1, out _));
        Assert.Equal(0, matcher.Find("edgx", 0, out _));
    }

    [Fact]
    public void Find_Matches_Short_Tokens()
    {
        PriorityTokenMatcher<TestDetector> matcher = Create(["lg", "o2", "longer"]);

        Assert.Equal(1, matcher.Find("Model O2 phone", 0, out int position));
        Assert.Equal(6, position);
        Assert.Equal(0, matcher.Find("LG-L160L longer", 0, out _));
        Assert.Equal(2, matcher.Find("a longer text", 0, out _));
    }

    [Fact]
    public void Find_Honors_Forced_Single_Token_Group()
    {
        PriorityTokenMatcher<TestDetector> matcher = Create(["windows nt 10.0", "windows nt 6.1", "android"], 1);

        Assert.Equal(0, matcher.Find("(Windows NT 10.0; Win64)", 0, out int position));
        Assert.Equal(1, position);
        Assert.Equal(2, matcher.Find("(Linux; Android 15)", 0, out _));
        Assert.Equal(-1, matcher.Find("(Linux; Android 15)", 3, out _));
    }

#if NET9_0_OR_GREATER
    [Fact]
    public void Groups_Extract_Short_Tokens()
    {
        PriorityTokenMatcher<TestDetector> matcher = Create(["lg", "longer", "o2"]);

        PriorityTokenGroup group = Assert.Single(matcher.Groups.ToArray());
        Assert.Equal(new[] { "longer" }, group.FilterValues);
        Assert.Equal(new[] { "lg", "o2" }, group.ShortTokens);
    }

    [Fact]
    public void Groups_With_Short_Tokens_Only_Have_No_Vectorized_Filter()
    {
        PriorityTokenMatcher<TestDetector> matcher = Create(["lg"]);

        PriorityTokenGroup group = Assert.Single(matcher.Groups.ToArray());
        Assert.Null(group.Values);
        Assert.Equal(0, matcher.Find("LG-L160L", 0, out _));
        Assert.Equal(-1, matcher.Find("Pixel", 0, out _));
    }

    [Fact]
    public void Groups_Drop_Tokens_Covered_By_Shorter_Tokens()
    {
        PriorityTokenMatcher<TestDetector> covered = Create(["Edg", "Edge", "EdgA", "EdgiOS"]);
        PriorityTokenMatcher<TestDetector> replaced = Create(["Edge", "EdgA", "Edg"]);

        Assert.Equal(new[] { "Edg" }, Assert.Single(covered.Groups.ToArray()).FilterValues);
        Assert.Equal(new[] { "Edg" }, Assert.Single(replaced.Groups.ToArray()).FilterValues);
    }

    [Fact]
    public void Groups_Hold_At_Most_Eight_Values()
    {
        PriorityTokenMatcher<TestDetector> matcher = Create(["aaa", "bbb", "ccc", "ddd", "eee", "fff", "ggg", "hhh", "iii"]);

        PriorityTokenGroup[] groups = matcher.Groups.ToArray();
        Assert.Equal(2, groups.Length);
        Assert.Equal(PriorityTokenGroup.MaxValuesPerGroup, groups[0].FilterValues.Length);
        Assert.Equal((8, 9), (groups[1].Start, groups[1].End));
        Assert.Equal(8, matcher.Find("iii", 0, out _));
    }

    [Fact]
    public void Groups_Accept_A_Token_That_Replaces_Existing_Values()
    {
        // The ninth token replaces "aaa1" and "aaa2", so the group does not overflow.
        PriorityTokenMatcher<TestDetector> matcher = Create(["aaa1", "aaa2", "bbb", "ccc", "ddd", "eee", "fff", "ggg", "aaa"]);

        PriorityTokenGroup group = Assert.Single(matcher.Groups.ToArray());
        Assert.Equal(new[] { "bbb", "ccc", "ddd", "eee", "fff", "ggg", "aaa" }, group.FilterValues);
    }

    [Fact]
    public void Groups_Start_At_Requested_Indices()
    {
        PriorityTokenMatcher<TestDetector> matcher = Create(["aaa", "bbb", "ccc", "ddd"], 1, 3);

        PriorityTokenGroup[] groups = matcher.Groups.ToArray();
        Assert.Equal(new[] { (0, 1), (1, 3), (3, 4) }, groups.Select(static g => (g.Start, g.End)));
    }

    [Fact]
    public void Built_In_Matchers_Use_Small_Vectorized_Groups()
    {
        Assert.All(HttpUserAgentRules.DefaultRobotSet.Matcher.Groups.ToArray(), static g => Assert.InRange(g.FilterValues.Length, 1, PriorityTokenGroup.MaxValuesPerGroup));
        Assert.All(HttpUserAgentRules.MobileMatcher.Groups.ToArray(), static g => Assert.InRange(g.FilterValues.Length, 1, PriorityTokenGroup.MaxValuesPerGroup));

        // Googlebot, by far the most common robot, is checked on its own.
        Assert.Equal((0, 1), (HttpUserAgentRules.DefaultRobotSet.Matcher.Groups[0].Start, HttpUserAgentRules.DefaultRobotSet.Matcher.Groups[0].End));
    }
#endif

    [Fact]
    public void ContainsAny_Checks_All_Groups()
    {
        PriorityTokenMatcher<TestDetector> matcher = Create(["aaa", "bbb", "ccc", "ddd", "eee", "fff", "ggg", "hhh", "iii", "lg"]);

        Assert.True(matcher.ContainsAny("xx iii xx"));
        Assert.True(matcher.ContainsAny("LG-L160L"));
        Assert.False(matcher.ContainsAny("nothing"));
    }

    [Fact]
    public void Robot_Anchors_Add_Uncovered_Keys()
    {
        string[] anchors = HttpUserAgentRules.CreateRobotAnchors(["Examplebot", "Turnitin", "Bytespider"], ["bot", "spider"]);

        Assert.Equal(new[] { "bot", "spider", "Turnitin" }, anchors);
    }

    [Fact]
    public void Curated_Robot_Anchors_Cover_All_Default_Robot_Keys()
    {
        // When a new robot key is not covered, it becomes an anchor on its own (correct, but an extra search).
        // Extend the curated anchors instead.
        Assert.Equal(HttpUserAgentRules.CuratedRobotAnchors.Length, HttpUserAgentRules.DefaultRobotSet.AnchorMatcher.Count);
    }

    public static TheoryData<string> CatalogBrowserUserAgents => [.. UserAgentCatalog.Browsers.Select(static ua => ua.UserAgent)];

    [Theory]
    [MemberData(nameof(CatalogBrowserUserAgents))]
    public void Robot_Anchors_Do_Not_Occur_In_Browser_User_Agents(string userAgent)
    {
        Assert.False(HttpUserAgentRules.DefaultRobotSet.AnchorMatcher.ContainsAny(userAgent));
    }

    [Theory]
    [MemberData(nameof(CatalogUserAgents))]
    public void GetRobot_Equals_Sequential_Scan(string userAgent)
    {
        string? expected = null;
        foreach ((string key, string value) in HttpUserAgentStatics.Robots)
        {
            if (userAgent.Contains(key, StringComparison.OrdinalIgnoreCase))
            {
                expected = value;
                break;
            }
        }

        Assert.Equal(expected, HttpUserAgentParser.GetRobot(userAgent));
    }

    public static TheoryData<string> CatalogUserAgents => [.. UserAgentCatalog.All];

    [Theory]
    [MemberData(nameof(CatalogUserAgents))]
    public void Built_In_Matchers_Equal_Sequential_Scan(string userAgent)
    {
        AssertEqualsSequentialScan(HttpUserAgentRules.MobileMatcher, [.. HttpUserAgentRules.Mobiles.Select(static rule => rule.Key)], userAgent);
        AssertEqualsSequentialScan(HttpUserAgentRules.DefaultRobotSet.Matcher, HttpUserAgentRules.DefaultRobotSet.Keys, userAgent);
    }

    private static void AssertEqualsSequentialScan<T>(PriorityTokenMatcher<T> matcher, string[] tokens, string input)
        where T : struct
    {
        for (int start = 0; start <= tokens.Length; start++)
        {
            int expected = -1;
            int expectedPosition = -1;
            for (int i = start; i < tokens.Length; i++)
            {
                expectedPosition = input.IndexOf(tokens[i], StringComparison.OrdinalIgnoreCase);
                if (expectedPosition >= 0)
                {
                    expected = i;
                    break;
                }
            }

            Assert.Equal(expected, matcher.Find(input, start, out int position));
            Assert.Equal(expected >= 0 ? expectedPosition : -1, position);
        }
    }

    /// <summary>
    /// User agents that contain several mobile tokens: the most specific token wins
    /// (device or manufacturer before operating system before browser before generic fallback).
    /// </summary>
    [Theory]
    [InlineData("Mozilla/5.0 (iPhone; CPU iPhone OS 17_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.0 Mobile/15E148 Safari/604.1", "Apple iPhone")]
    [InlineData("Mozilla/5.0 (iPod touch; CPU iPhone 14_5_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/14.0 Mobile/15E148 Safari/604.1", "Apple iPod")]
    [InlineData("Mozilla/5.0 (Linux; Android 14; SAMSUNG SM-S918B) AppleWebKit/537.36 (KHTML, like Gecko) SamsungBrowser/23.0 Chrome/115.0.0.0 Mobile Safari/537.36", "Samsung")]
    [InlineData("Mozilla/5.0 (Windows Phone 10.0; Android 6.0.1; Microsoft; Lumia 950) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/52.0.2743.116 Mobile Safari/537.36 Edge/15.15063", "Android")]
    [InlineData("Mozilla/5.0 (Series40; Nokia501/14.0.4/java_runtime_version=Nokia_Asha_1_2; Profile/MIDP-2.1 Configuration/CLDC-1.1) Gecko/20100401 S40OviBrowser/3.8.1.0.5", "Nokia")]
    [InlineData("Opera/9.80 (Android; Opera Mini/36.2.2254/119.132; U; id) Presto/2.12.423 Version/12.16", "Android")]
    [InlineData("Opera/9.80 (J2ME/MIDP; Opera Mini/9.80 (S60; SymbOS; Opera Mobi/23.348; U; en) Presto/2.5.25 Version/10.54", "Opera Mini")]
    [InlineData("Mozilla/5.0 (Nintendo 3DS; U; ; en) Version/1.7412.EU", "Nintendo 3DS")]
    [InlineData("Opera/9.50 (Nintendo DSi; Opera/507; U; en-US)", "Nintendo DSi")]
    [InlineData("Mozilla/5.0 (Linux; U; Android 4.0.3; ko-kr; LG-L160L Build/IML74K) AppleWebkit/534.30 (KHTML, like Gecko) Version/4.0 Mobile Safari/534.30", "LG")]
    [InlineData("Mozilla/5.0 (SymbianOS/9.4; Series60/5.0 NokiaN97-1/12.0.024; Profile/MIDP-2.1 Configuration/CLDC-1.1; en-us) AppleWebKit/525 (KHTML, like Gecko) BrowserNG/7.1.18124", "Nokia")]
    [InlineData("Mozilla/5.0 (SymbianOS/9.2; U; Series60/3.1) AppleWebKit/413 (KHTML, like Gecko) Safari/413", "SymbianOS")]
    [InlineData("BlackBerry9700/5.0.0.351 Profile/MIDP-2.1 Configuration/CLDC-1.1 VendorID/123", "BlackBerry")]
    [InlineData("Mozilla/5.0 (PlayStation Vita 3.74) AppleWebKit/537.73 (KHTML, like Gecko) Silk/3.2", "PlayStation Vita")]
    [InlineData("Mozilla/5.0 (Linux; Android 9; SHARP AQUOS sense3) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/120.0 Mobile Safari/537.36", "Sharp")]
    [InlineData("Mozilla/5.0 (Mobile; rv:48.0; A405DL) Gecko/48.0 Firefox/48.0 KAIOS/2.5", "Generic Mobile")]
    public void Mobile_Priority_Prefers_Specific_Matches(string userAgent, string expected)
    {
        Assert.Equal(expected, HttpUserAgentParser.GetMobileDevice(userAgent));
    }
}
