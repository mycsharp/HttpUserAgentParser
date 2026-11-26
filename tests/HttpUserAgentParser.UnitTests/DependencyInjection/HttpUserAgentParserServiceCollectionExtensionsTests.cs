// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using MyCSharp.HttpUserAgentParser.Providers;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests.DependencyInjection;

public class HttpUserAgentParserServiceCollectionExtensionsTests
{
    public class TestHttpUserAgentParserProvider : IHttpUserAgentParserProvider
    {
        public HttpUserAgentInformation Parse(string userAgent) => throw new NotSupportedException();
    }

    [Fact]
    public void AddHttpUserAgentParser()
    {
        ServiceCollection services = new();

        HttpUserAgentParserDependencyInjectionOptions options = services.AddHttpUserAgentParser();

        Assert.Same(services, options.Services);

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

    [Fact]
    public void AddHttpUserAgentCachedParser_With_MaxCacheEntries()
    {
        ServiceCollection services = new();

        HttpUserAgentParserDependencyInjectionOptions options = services.AddHttpUserAgentCachedParser(maxCacheEntries: 128);

        Assert.Same(services, options.Services);
        Assert.Single(services);

        using ServiceProvider serviceProvider = services.BuildServiceProvider();
        HttpUserAgentParserCachedProvider provider = Assert.IsType<HttpUserAgentParserCachedProvider>(
            serviceProvider.GetRequiredService<IHttpUserAgentParserProvider>());
        Assert.Equal(128, provider.MaxCacheEntries);
    }

    [Fact]
    public void AddHttpUserAgentParser_Is_Idempotent()
    {
        ServiceCollection services = new();

        services.AddHttpUserAgentParser();
        services.AddHttpUserAgentCachedParser();
        services.AddHttpUserAgentCachedParser(64);
        services.AddHttpUserAgentParser<TestHttpUserAgentParserProvider>();

        Assert.Single(services);
        Assert.Equal(typeof(HttpUserAgentParserDefaultProvider), services[0].ImplementationType);
    }

    [Fact]
    public void AddHttpUserAgentParser_Validates_Arguments()
    {
        IServiceCollection nullServices = null!;
        ServiceCollection services = new();

        Assert.Throws<ArgumentNullException>(() => nullServices.AddHttpUserAgentParser());
        Assert.Throws<ArgumentNullException>(() => nullServices.AddHttpUserAgentCachedParser());
        Assert.Throws<ArgumentNullException>(() => nullServices.AddHttpUserAgentCachedParser(10));
        Assert.Throws<ArgumentNullException>(() => nullServices.AddHttpUserAgentParser<TestHttpUserAgentParserProvider>());
        Assert.Throws<ArgumentOutOfRangeException>(() => services.AddHttpUserAgentCachedParser(0));
        Assert.Throws<ArgumentOutOfRangeException>(() => services.AddHttpUserAgentCachedParser(-5));
    }
}
