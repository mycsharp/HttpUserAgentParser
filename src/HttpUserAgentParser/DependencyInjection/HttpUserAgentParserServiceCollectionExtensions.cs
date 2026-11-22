// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.DependencyInjection;
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
    /// Registers <typeparam name="TProvider"/> as singleton to <see cref="IHttpUserAgentParserProvider"/>
    /// </summary>
    /// <remarks>
    /// <typeparamref name="TProvider"/> is created by the dependency injection container; its public constructors are preserved for trimming and Native AOT.
    /// </remarks>
    public static HttpUserAgentParserDependencyInjectionOptions AddHttpUserAgentParser<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicConstructors)] TProvider>(
        this IServiceCollection services) where TProvider : class, IHttpUserAgentParserProvider
    {
        // create options
        HttpUserAgentParserDependencyInjectionOptions options = new(services);

        // add provider
        services.AddSingleton<IHttpUserAgentParserProvider, TProvider>();

        return options;
    }
}
