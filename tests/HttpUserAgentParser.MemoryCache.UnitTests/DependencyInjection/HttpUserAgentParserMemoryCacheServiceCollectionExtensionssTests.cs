// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.MemoryCache.DependencyInjection;
using MyCSharp.HttpUserAgentParser.Providers;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.UnitTests.DependencyInjection;

public class HttpUserAgentParserMemoryCacheServiceCollectionExtensionssTests
{
    [Fact]
    public void AddHttpUserAgentMemoryCachedParser()
    {
        ServiceCollection services = new();

        services.AddHttpUserAgentMemoryCachedParser();

        Assert.Equal(2, services.Count);

        Assert.IsType<HttpUserAgentParserMemoryCachedProviderOptions>(services[0].ImplementationInstance);
        Assert.Equal(ServiceLifetime.Singleton, services[0].Lifetime);

        Assert.Equal(typeof(IHttpUserAgentParserProvider), services[1].ServiceType);
        Assert.Equal(typeof(HttpUserAgentParserMemoryCachedProvider), services[1].ImplementationType);
        Assert.Equal(ServiceLifetime.Singleton, services[1].Lifetime);
    }

    [Fact]
    public void AddHttpUserAgentMemoryCachedParser_Applies_Options()
    {
        ServiceCollection services = new();

        services.AddHttpUserAgentMemoryCachedParser(options =>
        {
            options.CacheOptions.SizeLimit = 1024;
            options.CacheEntryOptions.SlidingExpiration = TimeSpan.FromMinutes(60);
        });

        HttpUserAgentParserMemoryCachedProviderOptions options = Assert.IsType<HttpUserAgentParserMemoryCachedProviderOptions>(services[0].ImplementationInstance);
        Assert.Equal(1024, options.CacheOptions.SizeLimit);
        Assert.Equal(TimeSpan.FromMinutes(60), options.CacheEntryOptions.SlidingExpiration);
    }

    [Fact]
    public void AddHttpUserAgentMemoryCachedParser_Is_Idempotent()
    {
        ServiceCollection services = new();

        services.AddHttpUserAgentMemoryCachedParser(options => options.CacheOptions.SizeLimit = 512);
        services.AddHttpUserAgentMemoryCachedParser(options => options.CacheOptions.SizeLimit = 2048);

        Assert.Equal(2, services.Count);
        HttpUserAgentParserMemoryCachedProviderOptions options = Assert.IsType<HttpUserAgentParserMemoryCachedProviderOptions>(services[0].ImplementationInstance);
        Assert.Equal(512, options.CacheOptions.SizeLimit);
    }

    [Fact]
    public void AddHttpUserAgentMemoryCachedParser_Throws_When_Services_Is_Null()
    {
        IServiceCollection services = null!;

        Assert.Throws<ArgumentNullException>(() => services.AddHttpUserAgentMemoryCachedParser());
    }

    [Fact]
    public void MemoryCachedProvider_Throws_When_Options_Is_Null()
    {
        Assert.Throws<ArgumentNullException>(() => new HttpUserAgentParserMemoryCachedProvider(null!));
    }
}
