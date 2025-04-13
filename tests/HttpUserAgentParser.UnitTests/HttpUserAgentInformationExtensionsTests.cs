// Copyright © https://myCSharp.de - all rights reserved

using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests;

public class HttpUserAgentInformationExtensionsTests
{
    [Theory]
    [InlineData("Mozilla/5.0 (Linux; Android 10; HD1913) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.210 Mobile Safari/537.36 EdgA/46.3.4.5155", HttpUserAgentType.Browser, true)]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36 Edg/90.0.818.62", HttpUserAgentType.Browser, false)]
    [InlineData("Mozilla/5.0 (iPhone; CPU iPhone OS 9_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML,like Gecko) Version/9.0 Mobile/13B143 Safari/601.1 (compatible; AdsBot-Google-Mobile; +http://www.google.com/mobile/adsbot.html)", HttpUserAgentType.Robot, false)]
    [InlineData("APIs-Google (+https://developers.google.com/webmasters/APIs-Google.html)", HttpUserAgentType.Robot, false)]
    [InlineData("WhatsApp/2.22.20.72 A", HttpUserAgentType.Robot, false)]
    [InlineData("WhatsApp/2.22.19.78 I", HttpUserAgentType.Robot, false)]
    [InlineData("WhatsApp/2.2236.3 N", HttpUserAgentType.Robot, false)]
    [InlineData("Invalid user agent", HttpUserAgentType.Unknown, false)]
    public void IsType(string userAgent, HttpUserAgentType expectedType, bool isMobile)
    {
        HttpUserAgentInformation info = HttpUserAgentInformation.Parse(userAgent);

        if (expectedType == HttpUserAgentType.Browser)
        {
            Assert.True(info.IsType(HttpUserAgentType.Browser));
            Assert.False(info.IsType(HttpUserAgentType.Robot));
            Assert.False(info.IsType(HttpUserAgentType.Unknown));

            Assert.True(info.IsBrowser());
            Assert.False(info.IsRobot());
        }
        else if (expectedType == HttpUserAgentType.Robot)
        {
            Assert.False(info.IsType(HttpUserAgentType.Browser));
            Assert.True(info.IsType(HttpUserAgentType.Robot));
            Assert.False(info.IsType(HttpUserAgentType.Unknown));

            Assert.False(info.IsBrowser());
            Assert.True(info.IsRobot());
        }
        else if (expectedType == HttpUserAgentType.Unknown)
        {
            Assert.False(info.IsType(HttpUserAgentType.Browser));
            Assert.False(info.IsType(HttpUserAgentType.Robot));
            Assert.True(info.IsType(HttpUserAgentType.Unknown));

            Assert.False(info.IsBrowser());
            Assert.False(info.IsRobot());
        }

        Assert.Equal(isMobile, info.IsMobile());
    }
}
