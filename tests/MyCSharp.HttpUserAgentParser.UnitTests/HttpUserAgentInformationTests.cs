// Copyright © myCSharp 2020-2021, all rights reserved

using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;
using Xunit.Sdk;

namespace MyCSharp.HttpUserAgentParser.UnitTests
{
    public class HttpUserAgentInformationTests
    {
        [Theory]
        [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36 Edg/90.0.818.62")]
        public void Parse(string userAgent)
        {
            HttpUserAgentInformation ua1 = HttpUserAgentParser.Parse(userAgent);
            HttpUserAgentInformation ua2 = HttpUserAgentInformation.Parse(userAgent);

            ua1.Should().BeEquivalentTo(ua2);
        }

        [Theory]
        [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36 Edg/90.0.818.62")]
        public void CreateForRobot(string userAgent)
        {

            HttpUserAgentInformation ua = HttpUserAgentInformation.CreateForRobot(userAgent, "Chrome");

            ua.UserAgent.Should().Be(userAgent);
            ua.Type.Should().Be(HttpUserAgentType.Robot);
            ua.Platform.Should().Be(null);
            ua.Name.Should().Be("Chrome");
            ua.Version.Should().Be(null);
            ua.MobileDeviceType.Should().Be(null);
        }

        [Theory]
        [InlineData("Mozilla/5.0 (Linux; Android 10; HD1913) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.210 Mobile Safari/537.36 EdgA/46.3.4.5155")]
        public void CreateForBrowser(string userAgent)
        {
            HttpUserAgentPlatformInformation platformInformation =
                new HttpUserAgentPlatformInformation(new Regex(""), "Android", HttpUserAgentPlatformType.Android);

            HttpUserAgentInformation ua = HttpUserAgentInformation.CreateForBrowser(userAgent,
                platformInformation, "Edge", "46.3.4.5155", "Android");

            ua.UserAgent.Should().Be(userAgent);
            ua.Type.Should().Be(HttpUserAgentType.Browser);
            ua.Platform.Should().Be(platformInformation);
            ua.Name.Should().Be("Edge");
            ua.Version.Should().Be("46.3.4.5155");
            ua.MobileDeviceType.Should().Be("Android");
        }

        [Theory]
        [InlineData("Invalid user agent")]
        public void CreateForUnknown(string userAgent)
        {
            HttpUserAgentPlatformInformation platformInformation =
                new(new Regex(""), "Batman", HttpUserAgentPlatformType.Linux);

            HttpUserAgentInformation ua =
              HttpUserAgentInformation.CreateForUnknown(userAgent, platformInformation, null);

            ua.UserAgent.Should().Be(userAgent);
            ua.Type.Should().Be(HttpUserAgentType.Unknown);
            ua.Platform.Should().Be(platformInformation);
            ua.Name.Should().Be(null);
            ua.Version.Should().Be(null);
            ua.MobileDeviceType.Should().Be(null);
        }
    }
}
