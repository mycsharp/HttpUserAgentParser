// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using MyCSharp.HttpUserAgentParser.Providers;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.DependencyInjection;

/// <summary>
/// Dependency injection extensions for IMemoryCache
/// </summary>
public static class HttpUserAgentParserMemoryCacheServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="HttpUserAgentParserMemoryCachedProvider"/> as singleton to <see cref="IHttpUserAgentParserProvider"/>;
    /// the container disposes it, and with it its <see cref="Microsoft.Extensions.Caching.Memory.IMemoryCache"/>
    /// </summary>
    public static HttpUserAgentParserDependencyInjectionOptions AddHttpUserAgentMemoryCachedParser(
        this IServiceCollection services, Action<HttpUserAgentParserMemoryCachedProviderOptions>? options = null)
    {
        ArgumentNullException.ThrowIfNull(services);

        HttpUserAgentParserMemoryCachedProviderOptions providerOptions = new();
        options?.Invoke(providerOptions);

        // register options
        services.TryAddSingleton(providerOptions);

        // register cache provider
        return services.AddHttpUserAgentParser<HttpUserAgentParserMemoryCachedProvider>();
    }
}
