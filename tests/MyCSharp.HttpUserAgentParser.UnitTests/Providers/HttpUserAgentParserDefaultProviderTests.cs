// Copyright © myCSharp.de - all rights reserved

using FluentAssertions;
using MyCSharp.HttpUserAgentParser.Providers;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests.Providers;

public class HttpUserAgentParserDefaultProviderTests
{
    [Theory]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36 Edg/90.0.818.62")]
    public void Parse(string userAgent)
    {
        HttpUserAgentParserDefaultProvider provider = new();

        HttpUserAgentInformation providerUserAgentInfo = provider.Parse(userAgent);
        HttpUserAgentInformation userAgentInfo = HttpUserAgentInformation.Parse(userAgent);

        providerUserAgentInfo.Should().BeEquivalentTo(userAgentInfo);
    }
}
