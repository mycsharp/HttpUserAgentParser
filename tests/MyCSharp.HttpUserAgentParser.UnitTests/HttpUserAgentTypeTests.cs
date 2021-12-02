// Copyright © myCSharp 2020-2021, all rights reserved

using FluentAssertions;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests
{
    public class HttpUserAgentTypeTests
    {
        [Theory]
        [InlineData(HttpUserAgentType.Unknown, 0)]
        [InlineData(HttpUserAgentType.Browser, 1)]
        [InlineData(HttpUserAgentType.Robot, 2)]
        public void TestValue(HttpUserAgentType type, byte value)
        {
            ((byte)type == value).Should().Be(true);
        }
    }
}
