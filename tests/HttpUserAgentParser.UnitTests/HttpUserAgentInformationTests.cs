// Copyright © https://myCSharp.de - all rights reserved

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

    [Fact]
    public void Equals_Same_Values()
    {
        const string UserAgent = "Mozilla/5.0 (Linux; Android 10; K) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/158.0.0.0 Mobile Safari/537.36";

        HttpUserAgentInformation first = HttpUserAgentParser.Parse(UserAgent);
        HttpUserAgentInformation second = HttpUserAgentParser.Parse(new string(UserAgent.AsSpan()));

        Assert.True(first.Equals(second));
        Assert.True(first.Equals((object)second));
        Assert.True(first == second);
        Assert.False(first != second);
        Assert.Equal(first.GetHashCode(), second.GetHashCode());
    }

    public static TheoryData<int> DifferingProperties => [0, 1, 2, 3, 4, 5, 6];

    [Theory]
    [MemberData(nameof(DifferingProperties))]
    public void Not_Equals_When_Any_Property_Differs(int property)
    {
        HttpUserAgentPlatformInformation platform = new(TextRegex(), "Android", HttpUserAgentPlatformType.Android);
        HttpUserAgentInformation info = HttpUserAgentInformation.CreateForBrowser("ua", platform, "Chrome", "1.0", "Android");

        HttpUserAgentInformation other = property switch
        {
            0 => HttpUserAgentInformation.CreateForBrowser("other", platform, "Chrome", "1.0", "Android"),
            1 => HttpUserAgentInformation.CreateForUnknown("ua", platform, "Android"),
            2 => HttpUserAgentInformation.CreateForBrowser("ua", platform, "Edge", "1.0", "Android"),
            3 => HttpUserAgentInformation.CreateForBrowser("ua", platform, "Chrome", "2.0", "Android"),
            4 => HttpUserAgentInformation.CreateForBrowser("ua", platform, "Chrome", "1.0", deviceName: null),
            5 => HttpUserAgentInformation.CreateForBrowser("ua", platform: null, "Chrome", "1.0", "Android"),
            _ => HttpUserAgentInformation.CreateForBrowser("ua", new HttpUserAgentPlatformInformation(TextRegex(), "Linux", HttpUserAgentPlatformType.Linux), "Chrome", "1.0", "Android"),
        };

        Assert.False(info.Equals(other));
        Assert.False(info.Equals((object)other));
        Assert.False(info == other);
        Assert.True(info != other);
    }

    [Fact]
    public void Not_Equals_Other_Types()
    {
        HttpUserAgentInformation info = HttpUserAgentInformation.CreateForRobot("ua", "Googlebot");

        Assert.False(info.Equals((object?)null));
        Assert.False(info.Equals("ua"));
    }

    [GeneratedRegex("", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex TextRegex();
}
