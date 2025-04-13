// Copyright © https://myCSharp.de - all rights reserved

using System.Text.RegularExpressions;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests;

public partial class HttpUserAgentPlatformInformationTests
{
    [Theory]
    [InlineData("Batman", HttpUserAgentPlatformType.Android)]
    [InlineData("Robin", HttpUserAgentPlatformType.Windows)]
    public void Ctor(string name, HttpUserAgentPlatformType platform)
    {
        Regex regex = EmptyRegex();

        HttpUserAgentPlatformInformation info = new(regex, name, platform);

        Assert.Equal(regex, info.Regex);
        Assert.Equal(name, info.Name);
        Assert.Equal(platform, info.PlatformType);
    }

    [GeneratedRegex("", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex EmptyRegex();
}
