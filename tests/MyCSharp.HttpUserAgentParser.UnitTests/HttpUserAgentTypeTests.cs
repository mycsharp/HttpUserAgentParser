// Copyright © myCSharp.de - all rights reserved

using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests;

public class HttpUserAgentTypeTests
{
    [Theory]
    [InlineData(HttpUserAgentType.Unknown, 0)]
    [InlineData(HttpUserAgentType.Browser, 1)]
    [InlineData(HttpUserAgentType.Robot, 2)]
    public void TestValue(HttpUserAgentType type, byte value)
    {
        Assert.True((byte)type == value);
    }
}
