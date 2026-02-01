// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using MyCSharp.HttpUserAgentParser.Providers;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.DependencyInjection;

/// <summary>
/// Extension methods for registering <see cref="HttpUserAgentParserMemoryCachedProvider"/> with dependency injection.
/// </summary>
public static class HttpUserAgentParserMemoryCacheServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="HttpUserAgentParserMemoryCachedProvider"/> as a singleton implementation of <see cref="IHttpUserAgentParserProvider"/>.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <param name="options">Optional action to configure the cache options.</param>
    /// <returns>Options for further configuration.</returns>
    /// <remarks>
    /// <para>Default configuration: 256 entries maximum, 1 day sliding expiration.</para>
    /// <para>Use the <paramref name="options"/> parameter to customize cache behavior.</para>
    /// </remarks>
    /// <example>
    /// <code>
    /// IServiceCollection services = new ServiceCollection();
    /// services.AddHttpUserAgentMemoryCachedParser(opts =>
    /// {
    ///     opts.CacheOptions.SizeLimit = 512;
    ///     opts.CacheEntryOptions.SlidingExpiration = TimeSpan.FromHours(6);
    /// });
    /// </code>
    /// </example>
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
