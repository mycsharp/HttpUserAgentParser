// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;
using MyCSharp.HttpUserAgentParser.Providers;
using NSubstitute;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.UnitTests;

public class HttpUserAgentParserContextAccessorTests
{
    private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/158.0.0.0 Safari/537.36";

    private readonly IHttpUserAgentParserProvider _parserMock = Substitute.For<IHttpUserAgentParserProvider>();

    public HttpUserAgentParserContextAccessorTests()
        => _parserMock.Parse(UserAgent).Returns(HttpUserAgentInformation.Parse(UserAgent));

    [Fact]
    public void Get_Parses_The_Current_Request()
    {
        HttpUserAgentParserContextAccessor accessor = new(_parserMock, new HttpContextAccessor { HttpContext = HttpContextTestHelpers.GetHttpContext(UserAgent) });

        Assert.Equal(UserAgent, accessor.GetHttpContextUserAgent());
        Assert.Equal("Chrome", accessor.Get()?.Name);
        _parserMock.Received(1).Parse(UserAgent);
    }

    [Fact]
    public void Get_Parses_An_Explicit_Context()
    {
        HttpUserAgentParserContextAccessor accessor = new(_parserMock, new HttpContextAccessor());

        Assert.Equal(UserAgent, accessor.GetHttpContextUserAgent(HttpContextTestHelpers.GetHttpContext(UserAgent)));
        Assert.Equal("Chrome", accessor.Get(HttpContextTestHelpers.GetHttpContext(UserAgent))?.Name);
    }

    [Fact]
    public void Get_Returns_Null_Outside_Of_A_Request()
    {
        HttpUserAgentParserContextAccessor accessor = new(_parserMock, new HttpContextAccessor());

        Assert.Null(accessor.GetHttpContextUserAgent());
        Assert.Null(accessor.Get());
        _parserMock.DidNotReceiveWithAnyArgs().Parse(default!);
    }

    [Fact]
    public void Get_Returns_Null_Without_User_Agent_Header()
    {
        HttpUserAgentParserContextAccessor accessor = new(_parserMock, new HttpContextAccessor { HttpContext = new DefaultHttpContext() });

        Assert.Null(accessor.GetHttpContextUserAgent());
        Assert.Null(accessor.Get());
        _parserMock.DidNotReceiveWithAnyArgs().Parse(default!);
    }
}
