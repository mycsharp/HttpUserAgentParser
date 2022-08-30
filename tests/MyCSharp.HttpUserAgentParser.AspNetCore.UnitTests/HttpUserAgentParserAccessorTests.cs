// Copyright © myCSharp 2020-2022, all rights reserved

using FluentAssertions;
using Microsoft.AspNetCore.Http;
using Moq;
using MyCSharp.HttpUserAgentParser.Providers;
using MyCSharp.HttpUserAgentParser.TestHelpers;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.UnitTests
{
    public class HttpUserAgentParserAccessorTests
    {
        [Theory]
        [InlineData("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36 Edg/90.0.818.62")]
        public void Get(string userAgent)
        {
            HttpUserAgentInformation userAgentInformation = HttpUserAgentInformation.Parse(userAgent);

            Mock<IHttpUserAgentParserProvider> parserMock = new();
            {
                parserMock.Setup(x => x.Parse(userAgent)).Returns(userAgentInformation);
            }

            HttpContext httpContext = HttpContextTestHelpers.GetHttpContext(userAgent);

            HttpUserAgentParserAccessor accessor = new HttpUserAgentParserAccessor(parserMock.Object);
            HttpUserAgentInformation? info = accessor.Get(httpContext);

            info.Should().NotBeNull();
            info.Should().Be(userAgentInformation);
            parserMock.Verify(x => x.Parse(userAgent), Times.Once);
        }
    }
}
