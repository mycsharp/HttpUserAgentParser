// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using NSubstitute;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests.DependencyInjection;

public class UserAgentParserDependencyInjectionOptionsTests
{
    private readonly IServiceCollection _scMock = Substitute.For<IServiceCollection>();

    [Fact]
    public void Ctor_Should_Set_Property()
    {
        HttpUserAgentParserDependencyInjectionOptions options = new(_scMock);

        Assert.Equal(_scMock, options.Services);
    }
}
