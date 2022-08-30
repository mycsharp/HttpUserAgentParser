// Copyright © myCSharp 2020-2022, all rights reserved

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests.DependencyInjection
{
    public class UserAgentParserDependencyInjectionOptionsTests
    {
        [Fact]
        public void Ctor_Should_Set_Property()
        {
            Mock<IServiceCollection> scMock = new();

            HttpUserAgentParserDependencyInjectionOptions options = new(scMock.Object);

            options.Services.Should().BeEquivalentTo(scMock.Object);
        }
    }
}
