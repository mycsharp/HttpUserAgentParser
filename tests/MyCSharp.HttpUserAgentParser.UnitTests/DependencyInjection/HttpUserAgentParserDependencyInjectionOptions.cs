// Copyright © myCSharp.de - all rights reserved

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using NSubstitute;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests.DependencyInjection;

public class UserAgentParserDependencyInjectionOptionsTests
{
    private readonly IServiceCollection scMock = Substitute.For<IServiceCollection>();

    [Fact]
    public void Ctor_Should_Set_Property()
    {
        HttpUserAgentParserDependencyInjectionOptions options = new(scMock);

        options.Services.Should().BeEquivalentTo(scMock);
    }
}
