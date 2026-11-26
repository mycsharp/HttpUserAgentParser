// Copyright © https://myCSharp.de - all rights reserved

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
        Assert.NotNull(info);
        Assert.Equal(userAgentInformation, info);

        // verify
        _parserMock.Received(1).Parse(userAgent);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Get_Returns_Null_For_An_Empty_User_Agent(string userAgent)
    {
        HttpUserAgentParserAccessor accessor = new(_parserMock);

        Assert.Null(accessor.Get(HttpContextTestHelpers.GetHttpContext(userAgent)));
        _parserMock.DidNotReceiveWithAnyArgs().Parse(default!);
    }

    [Fact]
    public void Get_Caches_Result_Per_HttpContext()
    {
        const string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/120.0.0.0";
        HttpUserAgentInformation expected = HttpUserAgentInformation.Parse(userAgent);
        _parserMock.Parse(userAgent).Returns(expected);

        HttpContext httpContext = HttpContextTestHelpers.GetHttpContext(userAgent);
        HttpUserAgentParserAccessor accessor = new(_parserMock);

        Assert.Equal(expected, accessor.Get(httpContext));
        Assert.Equal(expected, accessor.Get(httpContext));
        _parserMock.Received(1).Parse(userAgent);

        DefaultHttpContext emptyContext = new();
        Assert.Null(accessor.Get(emptyContext));
        Assert.Null(accessor.Get(emptyContext));
        _parserMock.Received(1).Parse(Arg.Any<string>());
    }

    [Fact]
    public void Get_Throws_When_HttpContext_Is_Null()
    {
        HttpUserAgentParserAccessor accessor = new(_parserMock);

        Assert.Throws<ArgumentNullException>(() => accessor.Get(null!));
    }
}
