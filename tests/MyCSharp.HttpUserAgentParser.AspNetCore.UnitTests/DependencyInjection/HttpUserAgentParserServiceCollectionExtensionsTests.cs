// Copyright © myCSharp.de - all rights reserved

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.AspNetCore.DependencyInjection;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.UnitTests.DependencyInjection
{
    public class HttpUserAgentParserDependencyInjectionOptionsExtensionsTests
    {
        [Fact]
        public void AddHttpUserAgentParserAccessor()
        {
            ServiceCollection services = new();
            HttpUserAgentParserDependencyInjectionOptions options = new(services);

            options.AddHttpUserAgentParserAccessor();

            services.Count.Should().Be(1);

            services[0].ServiceType.Should().Be<IHttpUserAgentParserAccessor>();
            services[0].ImplementationType.Should().Be<HttpUserAgentParserAccessor>();
            services[0].Lifetime.Should().Be(ServiceLifetime.Singleton);
        }
    }
}
