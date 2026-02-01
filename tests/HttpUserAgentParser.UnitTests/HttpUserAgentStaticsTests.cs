// Copyright © https://myCSharp.de - all rights reserved

using System.Linq;
using System.Text.RegularExpressions;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests;

public class HttpUserAgentStaticsTests
{
    [Fact]
    public void Platforms_Contain_Windows_Regex()
    {
        Assert.NotEmpty(HttpUserAgentStatics.Platforms);

        Regex regex = HttpUserAgentStatics.GetPlatformRegexForToken("windows nt 10.0");
        Assert.Matches(regex, "Mozilla/5.0 (Windows NT 10.0; Win64; x64)");
    }

    [Fact]
    public void Browsers_Contain_Chrome_Regex()
    {
        const string ua = "Mozilla/5.0 Chrome/90.0.4430.212 Safari/537.36";

        KeyValuePair<Regex, string> entry = HttpUserAgentStatics.Browsers
            .First(candidate => string.Equals(candidate.Value, "Chrome", System.StringComparison.Ordinal) && candidate.Key.IsMatch(ua));

        Match match = entry.Key.Match(ua);
        Assert.True(match.Success);
        Assert.Equal("90.0.4430.212", match.Groups[1].Value);
    }

    [Fact]
    public void Mobiles_Contains_Apple_IPhone()
    {
        bool found = HttpUserAgentStatics.Mobiles.TryGetValue("iphone", out string? name);
        Assert.True(found);
        Assert.Equal("Apple iPhone", name);
    }

    [Fact]
    public void Robots_Contains_Googlebot()
    {
        Assert.Contains(HttpUserAgentStatics.Robots, robot => robot.Key.Equals("googlebot", System.StringComparison.OrdinalIgnoreCase));
    }
}
