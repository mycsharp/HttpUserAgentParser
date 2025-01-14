// Copyright © myCSharp.de - all rights reserved

using System.Text.RegularExpressions;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests;

public class HttpUserAgentPlatformInformationTests
{
    [Theory]
    [InlineData("Batman", HttpUserAgentPlatformType.Android)]
    [InlineData("Robin", HttpUserAgentPlatformType.Windows)]
    public void Ctor(string name, HttpUserAgentPlatformType platform)
    {
        Regex regex = new("");

        HttpUserAgentPlatformInformation info = new(regex, name, platform);

        Assert.Equal(regex, info.Regex);
        Assert.Equal(name, info.Name);
        Assert.Equal(platform, info.PlatformType);
    }
}
