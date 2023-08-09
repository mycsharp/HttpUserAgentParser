// Copyright © myCSharp.de - all rights reserved

using FluentAssertions;
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

        services.Count.Should().Be(2);

        services[0].ImplementationInstance.Should().BeOfType<HttpUserAgentParserMemoryCachedProviderOptions>();
        services[0].Lifetime.Should().Be(ServiceLifetime.Singleton);

        services[1].ServiceType.Should().Be<IHttpUserAgentParserProvider>();
        services[1].ImplementationType.Should().Be<HttpUserAgentParserMemoryCachedProvider>();
        services[1].Lifetime.Should().Be(ServiceLifetime.Singleton);
    }
}
