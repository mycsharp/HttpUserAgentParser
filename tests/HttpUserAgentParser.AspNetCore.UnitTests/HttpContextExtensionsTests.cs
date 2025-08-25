// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;
using MyCSharp.HttpUserAgentParser.AspNetCore;
using MyCSharp.HttpUserAgentParser.Providers;
using NSubstitute;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.UnitTests;

public class HttpContextExtensionsTests
{
    [Fact]
    public void GetUserAgentString_Returns_Value_When_Present()
    {
        HttpContext ctx = HttpContextTestHelpers.GetHttpContext("UA");
        Assert.Equal("UA", ctx.GetUserAgentString());
    }

    [Fact]
    public void GetUserAgentString_Returns_Null_When_Absent()
    {
        DefaultHttpContext ctx = new();
        Assert.Null(ctx.GetUserAgentString());
    }

    [Fact]
    public void Accessor_Get_Returns_Null_When_Header_Missing()
    {
        var provider = Substitute.For<IHttpUserAgentParserProvider>();
        HttpUserAgentParserAccessor accessor = new(provider);
        DefaultHttpContext ctx = new();

        Assert.Null(accessor.Get(ctx));
        provider.DidNotReceiveWithAnyArgs().Parse(default!);
    }
}
