// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using MyCSharp.HttpUserAgentParser.Providers;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.DependencyInjection;

/// <summary>
/// Dependency injection extensions for IMemoryCache
/// </summary>
public static class HttpUserAgentParserMemoryCacheServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="HttpUserAgentParserCachedProvider"/> as singleton to <see cref="IHttpUserAgentParserProvider"/>
    /// </summary>
    public static HttpUserAgentParserDependencyInjectionOptions AddHttpUserAgentMemoryCachedParser(
        this IServiceCollection services, Action<HttpUserAgentParserMemoryCachedProviderOptions>? options = null)
    {
        HttpUserAgentParserMemoryCachedProviderOptions providerOptions = new();
        options?.Invoke(providerOptions);

        // register options
        services.AddSingleton(providerOptions);

        // register cache provider
        return services.AddHttpUserAgentParser<HttpUserAgentParserMemoryCachedProvider>();
    }
}
