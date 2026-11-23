// Copyright © https://myCSharp.de - all rights reserved

using MyCSharp.HttpUserAgentParser.TestHelpers;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests;

/// <summary>
/// Real-world user agents: the most used browsers (current and previous version) and search engine, SEO and AI crawlers.
/// </summary>
public class UserAgentCatalogTests
{
    public static TheoryData<string> BrowserLabels => [.. UserAgentCatalog.Browsers.Select(static ua => ua.Label)];

    public static TheoryData<string> RobotLabels => [.. UserAgentCatalog.Robots.Select(static ua => ua.Label)];

    public static TheoryData<string> UnknownUserAgents => [.. UserAgentCatalog.Unknown];

    [Theory]
    [MemberData(nameof(BrowserLabels))]
    public void Browser(string label)
    {
        BrowserUserAgent expected = UserAgentCatalog.Browsers.Single(ua => string.Equals(ua.Label, label, StringComparison.Ordinal));

        HttpUserAgentInformation info = HttpUserAgentParser.Parse(expected.UserAgent);

        Assert.Equal(HttpUserAgentType.Browser, info.Type);
        Assert.Equal(expected.Name, info.Name);
        Assert.Equal(expected.Version, info.Version);
        HttpUserAgentPlatformInformation platform = Assert.NotNull(info.Platform);
        Assert.Equal(expected.PlatformName, platform.Name);
        Assert.Equal(expected.PlatformType, platform.PlatformType);
        Assert.Equal(expected.MobileDeviceType, info.MobileDeviceType);
        Assert.Equal(expected.MobileDeviceType is not null, info.IsMobile());
    }

    [Theory]
    [MemberData(nameof(RobotLabels))]
    public void Robot(string label)
    {
        RobotUserAgent expected = UserAgentCatalog.Robots.Single(ua => string.Equals(ua.Label, label, StringComparison.Ordinal));

        HttpUserAgentInformation info = HttpUserAgentParser.Parse(expected.UserAgent);

        Assert.Equal(HttpUserAgentType.Robot, info.Type);
        Assert.Equal(expected.Name, info.Name);
        Assert.Null(info.Version);
        Assert.Null(info.Platform);
        Assert.Null(info.MobileDeviceType);
        Assert.True(info.IsRobot());
    }

    [Theory]
    [MemberData(nameof(UnknownUserAgents))]
    public void Unknown(string userAgent)
    {
        HttpUserAgentInformation info = HttpUserAgentParser.Parse(userAgent);

        Assert.Equal(HttpUserAgentType.Unknown, info.Type);
        Assert.Null(info.Name);
        Assert.Null(info.Version);
    }
}
