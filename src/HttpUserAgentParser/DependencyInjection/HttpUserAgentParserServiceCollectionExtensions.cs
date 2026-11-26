// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyCSharp.HttpUserAgentParser.Providers;

namespace MyCSharp.HttpUserAgentParser.DependencyInjection;

/// <summary>
/// Dependency injection extensions
/// </summary>
public static class HttpUserAgentParserServiceCollectionExtensions
{
    /// <summary>
    /// Registers <see cref="HttpUserAgentParserDefaultProvider"/> as singleton to <see cref="IHttpUserAgentParserProvider"/>
    /// </summary>
    public static HttpUserAgentParserDependencyInjectionOptions AddHttpUserAgentParser(
        this IServiceCollection services)
    {
        return AddHttpUserAgentParser<HttpUserAgentParserDefaultProvider>(services);
    }

    /// <summary>
    /// Registers <see cref="HttpUserAgentParserCachedProvider"/> as singleton to <see cref="IHttpUserAgentParserProvider"/>
    /// </summary>
    public static HttpUserAgentParserDependencyInjectionOptions AddHttpUserAgentCachedParser(
        this IServiceCollection services)
    {
        return AddHttpUserAgentParser<HttpUserAgentParserCachedProvider>(services);
    }

    /// <summary>
    /// Registers <see cref="HttpUserAgentParserCachedProvider"/> with the specified <paramref name="maxCacheEntries"/> limit
    /// as singleton to <see cref="IHttpUserAgentParserProvider"/>
    /// </summary>
    public static HttpUserAgentParserDependencyInjectionOptions AddHttpUserAgentCachedParser(
        this IServiceCollection services, int maxCacheEntries)
    {
        ArgumentNullException.ThrowIfNull(services);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxCacheEntries);

        HttpUserAgentParserDependencyInjectionOptions options = new(services);
        services.TryAddSingleton<IHttpUserAgentParserProvider>(_ => new HttpUserAgentParserCachedProvider(maxCacheEntries));

        return options;
    }

    /// <summary>
    /// Registers <typeparam name="TProvider"/> as singleton to <see cref="IHttpUserAgentParserProvider"/>
    /// </summary>
    /// <remarks>
    /// <typeparamref name="TProvider"/> is created by the dependency injection container; its public constructors are preserved for trimming and Native AOT.
    /// </remarks>
    public static HttpUserAgentParserDependencyInjectionOptions AddHttpUserAgentParser<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TProvider>(
        this IServiceCollection services) where TProvider : class, IHttpUserAgentParserProvider
    {
        ArgumentNullException.ThrowIfNull(services);

        // create options
        HttpUserAgentParserDependencyInjectionOptions options = new(services);

        // add provider
        services.TryAddSingleton<IHttpUserAgentParserProvider, TProvider>();

        return options;
    }
}
