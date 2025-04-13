// Copyright © https://myCSharp.de - all rights reserved

using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests;

public class HttpUserAgentPlatformTypeTests
{
    [Theory]
    [InlineData(HttpUserAgentPlatformType.Unknown, 0)]
    [InlineData(HttpUserAgentPlatformType.Generic, 1)]
    [InlineData(HttpUserAgentPlatformType.Windows, 2)]
    [InlineData(HttpUserAgentPlatformType.Linux, 3)]
    [InlineData(HttpUserAgentPlatformType.Unix, 4)]
    [InlineData(HttpUserAgentPlatformType.IOS, 5)]
    [InlineData(HttpUserAgentPlatformType.MacOS, 6)]
    [InlineData(HttpUserAgentPlatformType.BlackBerry, 7)]
    [InlineData(HttpUserAgentPlatformType.Android, 8)]
    [InlineData(HttpUserAgentPlatformType.Symbian, 9)]
    public void TestValue(HttpUserAgentPlatformType type, byte value)
    {
        Assert.True((byte)type == value);
    }
}
