// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.Providers;

namespace MyCSharp.HttpUserAgentParser.DependencyInjection;

/// <summary>
/// Dependency injection extensions
/// </summary>
public static class HttpUserAgentParserServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="HttpUserAgentParserDefaultProvider"/> as a singleton implementation of <see cref="IHttpUserAgentParserProvider"/>.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>Options for further configuration.</returns>
    /// <example>
    /// <code>
    /// IServiceCollection services = new ServiceCollection();
    /// HttpUserAgentParserDependencyInjectionOptions options = services.AddHttpUserAgentParser();
    /// </code>
    /// </example>
    public static HttpUserAgentParserDependencyInjectionOptions AddHttpUserAgentParser(
        this IServiceCollection services)
    {
        return AddHttpUserAgentParser<HttpUserAgentParserDefaultProvider>(services);
    }

    /// <summary>
    /// Registers <see cref="HttpUserAgentParserCachedProvider"/> as a singleton implementation of <see cref="IHttpUserAgentParserProvider"/>.
    /// </summary>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>Options for further configuration.</returns>
    /// <remarks>
    /// This provider caches parsed results indefinitely using a <see cref="System.Collections.Concurrent.ConcurrentDictionary{TKey, TValue}"/>.
    /// For expiration-based caching, use the MemoryCache package instead.
    /// </remarks>
    /// <example>
    /// <code>
    /// IServiceCollection services = new ServiceCollection();
    /// HttpUserAgentParserDependencyInjectionOptions options = services.AddHttpUserAgentCachedParser();
    /// </code>
    /// </example>
    public static HttpUserAgentParserDependencyInjectionOptions AddHttpUserAgentCachedParser(
        this IServiceCollection services)
    {
        return AddHttpUserAgentParser<HttpUserAgentParserCachedProvider>(services);
    }

    /// <summary>
    /// Registers a custom <see cref="IHttpUserAgentParserProvider"/> implementation as a singleton.
    /// </summary>
    /// <typeparam name="TProvider">The provider type implementing <see cref="IHttpUserAgentParserProvider"/>.</typeparam>
    /// <param name="services">The service collection to add the services to.</param>
    /// <returns>Options for further configuration.</returns>
    /// <example>
    /// <code>
    /// IServiceCollection services = new ServiceCollection();
    /// HttpUserAgentParserDependencyInjectionOptions options = services
    ///     .AddHttpUserAgentParser&lt;HttpUserAgentParserDefaultProvider&gt;();
    /// </code>
    /// </example>
    public static HttpUserAgentParserDependencyInjectionOptions AddHttpUserAgentParser<TProvider>(
        this IServiceCollection services) where TProvider : class, IHttpUserAgentParserProvider
    {
        // create options
        HttpUserAgentParserDependencyInjectionOptions options = new(services);

        // add provider
        services.AddSingleton<IHttpUserAgentParserProvider, TProvider>();

        return options;
    }
}
