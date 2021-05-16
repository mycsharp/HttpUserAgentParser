// Copyright © myCSharp 2020-2021, all rights reserved

using FluentAssertions;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests
{
    public class HttpUserAgentInformationExtensionsTests
    {
        [Theory]
        [InlineData("Mozilla/5.0 (Linux; Android 10; HD1913) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.210 Mobile Safari/537.36 EdgA/46.3.4.5155", HttpUserAgentType.Browser, true)]
        [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36 Edg/90.0.818.62", HttpUserAgentType.Browser, false)]
        [InlineData("Mozilla/5.0 (iPhone; CPU iPhone OS 9_1 like Mac OS X) AppleWebKit/601.1.46 (KHTML,like Gecko) Version/9.0 Mobile/13B143 Safari/601.1 (compatible; AdsBot-Google-Mobile; +http://www.google.com/mobile/adsbot.html)", HttpUserAgentType.Robot, false)]
        [InlineData("APIs-Google (+https://developers.google.com/webmasters/APIs-Google.html)", HttpUserAgentType.Robot, false)]
        [InlineData("Invalid user agent", HttpUserAgentType.Unknown, false)]
        public void IsType(string userAgent, HttpUserAgentType expectedType, bool isMobile)
        {
            HttpUserAgentInformation info = HttpUserAgentInformation.Parse(userAgent);
            info.IsType(expectedType).Should().Be(true);

            if (expectedType == HttpUserAgentType.Browser)
            {
                info.IsBrowser().Should().Be(true);
                info.IsRobot().Should().Be(false);
            }
            else if (expectedType == HttpUserAgentType.Robot)
            {
                info.IsBrowser().Should().Be(false);
                info.IsRobot().Should().Be(true);
            }
            else if (expectedType == HttpUserAgentType.Unknown)
            {
                info.IsBrowser().Should().Be(false);
                info.IsRobot().Should().Be(false);
            }

            info.IsMobile().Should().Be(isMobile);
        }
    }
}
