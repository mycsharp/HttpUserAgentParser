// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using MyCSharp.HttpUserAgentParser.Providers;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests.DependencyInjection;

public class HttpUserAgentParserMemoryCacheServiceCollectionExtensions
{
    public class TestHttpUserAgentParserProvider : IHttpUserAgentParserProvider
    {
        public HttpUserAgentInformation Parse(string userAgent) => throw new NotSupportedException();
    }

    [Fact]
    public void AddHttpUserAgentParser()
    {
        ServiceCollection services = new();

        services.AddHttpUserAgentParser();

        Assert.Single(services);
        Assert.Equal(typeof(IHttpUserAgentParserProvider), services[0].ServiceType);
        Assert.Equal(typeof(HttpUserAgentParserDefaultProvider), services[0].ImplementationType);
        Assert.Equal(ServiceLifetime.Singleton, services[0].Lifetime);
    }

    [Fact]
    public void AddHttpUserAgentCachedParser()
    {
        ServiceCollection services = new();

        services.AddHttpUserAgentCachedParser();

        Assert.Single(services);
        Assert.Equal(typeof(IHttpUserAgentParserProvider), services[0].ServiceType);
        Assert.Equal(typeof(HttpUserAgentParserCachedProvider), services[0].ImplementationType);
        Assert.Equal(ServiceLifetime.Singleton, services[0].Lifetime);
    }

    [Fact]
    public void AddHttpUserAgentParser_With_Generic()
    {
        ServiceCollection services = new();

        services.AddHttpUserAgentParser<TestHttpUserAgentParserProvider>();

        Assert.Single(services);
        Assert.Equal(typeof(IHttpUserAgentParserProvider), services[0].ServiceType);
        Assert.Equal(typeof(TestHttpUserAgentParserProvider), services[0].ImplementationType);
        Assert.Equal(ServiceLifetime.Singleton, services[0].Lifetime);
    }
}
