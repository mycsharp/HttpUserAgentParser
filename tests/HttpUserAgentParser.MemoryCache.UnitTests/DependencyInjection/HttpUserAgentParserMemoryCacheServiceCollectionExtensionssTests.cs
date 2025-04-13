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
}
