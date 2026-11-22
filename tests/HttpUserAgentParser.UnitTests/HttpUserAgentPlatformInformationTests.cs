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

    [Fact]
    public void Default_Has_No_Regex()
    {
        HttpUserAgentPlatformInformation info = default;

        Assert.Null(info.Regex);
        Assert.Null(info.Name);
        Assert.Equal(HttpUserAgentPlatformType.Unknown, info.PlatformType);
    }

    [Fact]
    public void Lazy_Regex_Is_Resolved_On_Access()
    {
        HttpUserAgentPlatformInformation info = new(HttpUserAgentPlatformRegexId.Android, "Android", HttpUserAgentPlatformType.Android);

        Assert.Same(HttpUserAgentRegexes.GetPlatformRegex(HttpUserAgentPlatformRegexId.Android), info.Regex);
    }

    [Fact]
    public void Equals_Same_Regex_Name_And_Type()
    {
        HttpUserAgentPlatformInformation lazy = new(HttpUserAgentPlatformRegexId.Android, "Android", HttpUserAgentPlatformType.Android);
        HttpUserAgentPlatformInformation eager = new(lazy.Regex, "Android", HttpUserAgentPlatformType.Android);

        Assert.True(lazy.Equals(eager));
        Assert.True(lazy.Equals((object)eager));
        Assert.Equal(lazy.GetHashCode(), eager.GetHashCode());
    }

    [Fact]
    public void Not_Equals_When_Any_Property_Differs()
    {
        Regex regex = EmptyRegex();
        HttpUserAgentPlatformInformation info = new(regex, "Android", HttpUserAgentPlatformType.Android);

        Assert.False(info.Equals(new HttpUserAgentPlatformInformation(regex, "Android", HttpUserAgentPlatformType.Linux)));
        Assert.False(info.Equals(new HttpUserAgentPlatformInformation(regex, "android", HttpUserAgentPlatformType.Android)));
        Assert.False(info.Equals(new HttpUserAgentPlatformInformation(OtherRegex(), "Android", HttpUserAgentPlatformType.Android)));
        Assert.False(info.Equals((object?)null));
        Assert.False(info.Equals("Android"));
    }

    [Fact]
    public void Parse_Results_Are_Equal_To_Published_Platforms()
    {
        HttpUserAgentPlatformInformation platform = Assert.NotNull(HttpUserAgentParser.GetPlatform("Mozilla/5.0 (Windows NT 10.0; Win64; x64)"));

        Assert.Contains(platform, HttpUserAgentStatics.Platforms);
    }

    [GeneratedRegex("", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex EmptyRegex();

    [GeneratedRegex("other", RegexOptions.None, matchTimeoutMilliseconds: 1000)]
    private static partial Regex OtherRegex();
}
