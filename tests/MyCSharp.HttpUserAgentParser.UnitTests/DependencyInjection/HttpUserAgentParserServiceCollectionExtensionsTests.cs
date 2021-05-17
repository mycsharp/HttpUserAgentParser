// Copyright © myCSharp 2020-2021, all rights reserved

using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using MyCSharp.HttpUserAgentParser.Providers;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests.DependencyInjection
{
    public class HttpUserAgentParserMemoryCacheServiceCollectionExtensions
    {
        public class TestHttpUserAgentParserProvider : IHttpUserAgentParserProvider
        {
            public HttpUserAgentInformation Parse(string userAgent) => throw new System.NotImplementedException();
        }

        [Fact]
        public void AddHttpUserAgentParser()
        {
            ServiceCollection services = new();

            services.AddHttpUserAgentParser();

            services.Count.Should().Be(1);
            services[0].ServiceType.Should().Be<IHttpUserAgentParserProvider>();
            services[0].ImplementationType.Should().Be<HttpUserAgentParserDefaultProvider>();
            services[0].Lifetime.Should().Be(ServiceLifetime.Singleton);
        }

        [Fact]
        public void AddHttpUserAgentCachedParser()
        {
            ServiceCollection services = new();

            services.AddHttpUserAgentCachedParser();

            services.Count.Should().Be(1);
            services[0].ServiceType.Should().Be<IHttpUserAgentParserProvider>();
            services[0].ImplementationType.Should().Be<HttpUserAgentParserCachedProvider>();
            services[0].Lifetime.Should().Be(ServiceLifetime.Singleton);
        }

        [Fact]
        public void AddHttpUserAgentParser_With_Generic()
        {
            ServiceCollection services = new();

            services.AddHttpUserAgentParser<TestHttpUserAgentParserProvider>();

            services.Count.Should().Be(1);
            services[0].ServiceType.Should().Be<IHttpUserAgentParserProvider>();
            services[0].ImplementationType.Should().Be<TestHttpUserAgentParserProvider>();
            services[0].Lifetime.Should().Be(ServiceLifetime.Singleton);
        }
    }
}
