// Copyright © myCSharp 2020-2022, all rights reserved

using System.Text.RegularExpressions;
using FluentAssertions;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests
{
    public class HttpUserAgentPlatformInformationTests
    {
        [Theory]
        [InlineData("Batman", HttpUserAgentPlatformType.Android)]
        [InlineData("Robin", HttpUserAgentPlatformType.Windows)]
        public void Ctor(string name, HttpUserAgentPlatformType platform)
        {
            Regex regex = new("");

            HttpUserAgentPlatformInformation info = new(regex, name, platform);

            info.Regex.Should().Be(regex);
            info.Name.Should().Be(name);
            info.PlatformType.Should().Be(platform);
        }
    }
}
