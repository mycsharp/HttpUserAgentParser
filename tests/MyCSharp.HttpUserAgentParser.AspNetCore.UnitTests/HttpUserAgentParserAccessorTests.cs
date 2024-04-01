// Copyright © myCSharp.de - all rights reserved

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using MyCSharp.HttpUserAgentParser.Providers;
using NSubstitute;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.UnitTests;

public class HttpUserAgentParserAccessorTests
{
    private readonly IHttpUserAgentParserProvider _parserMock = Substitute.For<IHttpUserAgentParserProvider>();

    [Theory]
    [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36 Edg/90.0.818.62")]
    public void Get(string userAgent)
    {
        // arrange
        HttpUserAgentInformation userAgentInformation = HttpUserAgentInformation.Parse(userAgent);
        _parserMock.Parse(userAgent).Returns(userAgentInformation);

        // act
        HttpContext httpContext = HttpContextTestHelpers.GetHttpContext(userAgent);

        HttpUserAgentParserAccessor accessor = new(_parserMock);
        HttpUserAgentInformation? info = accessor.Get(httpContext);

        // assert
        info.Should().NotBeNull();
        info.Should().Be(userAgentInformation);

        // verify
        _parserMock.Received(1).Parse(userAgent);
    }
}
