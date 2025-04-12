// Copyright © myCSharp.de - all rights reserved

using System.Text.RegularExpressions;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests;

public partial class HttpUserAgentInformationTests
{
    [Theory]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36 Edg/90.0.818.62")]
    public void Parse(string userAgent)
    {
        HttpUserAgentInformation ua1 = HttpUserAgentParser.Parse(userAgent);
        HttpUserAgentInformation ua2 = HttpUserAgentInformation.Parse(userAgent);

        Assert.Equal(ua2, ua1);
    }

    [Theory]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36 Edg/90.0.818.62")]
    public void CreateForRobot(string userAgent)
    {
        HttpUserAgentInformation ua = HttpUserAgentInformation.CreateForRobot(userAgent, "Chrome");

        Assert.Equal(userAgent, ua.UserAgent);
        Assert.Equal(HttpUserAgentType.Robot, ua.Type);
        Assert.Null(ua.Platform);
        Assert.Equal("Chrome", ua.Name);
        Assert.Null(ua.Version);
        Assert.Null(ua.MobileDeviceType);
    }

    [Theory]
    [InlineData("Mozilla/5.0 (Linux; Android 10; HD1913) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.210 Mobile Safari/537.36 EdgA/46.3.4.5155")]
    public void CreateForBrowser(string userAgent)
    {
        HttpUserAgentPlatformInformation platformInformation = new(TextRegex(), "Android", HttpUserAgentPlatformType.Android);

        HttpUserAgentInformation ua = HttpUserAgentInformation.CreateForBrowser(userAgent,
            platformInformation, "Edge", "46.3.4.5155", "Android");

        Assert.Equal(userAgent, ua.UserAgent);
        Assert.Equal(HttpUserAgentType.Browser, ua.Type);
        Assert.Equal(platformInformation, ua.Platform);
        Assert.Equal("Edge", ua.Name);
        Assert.Equal("46.3.4.5155", ua.Version);
        Assert.Equal("Android", ua.MobileDeviceType);
    }

    [Theory]
    [InlineData("Invalid user agent")]
    public void CreateForUnknown(string userAgent)
    {
        HttpUserAgentPlatformInformation platformInformation = new(TextRegex(), "Batman", HttpUserAgentPlatformType.Linux);

        HttpUserAgentInformation ua =
          HttpUserAgentInformation.CreateForUnknown(userAgent, platformInformation, deviceName: null);

        Assert.Equal(userAgent, ua.UserAgent);
        Assert.Equal(HttpUserAgentType.Unknown, ua.Type);
        Assert.Equal(platformInformation, ua.Platform);
        Assert.Null(ua.Name);
        Assert.Null(ua.Version);
        Assert.Null(ua.MobileDeviceType);
    }

    [GeneratedRegex("", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex TextRegex();
}
