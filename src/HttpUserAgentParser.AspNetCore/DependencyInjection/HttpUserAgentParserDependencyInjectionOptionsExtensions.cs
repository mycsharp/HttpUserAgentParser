// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using MyCSharp.HttpUserAgentParser.Providers;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.DependencyInjection;

/// <summary>
/// Extension methods for <see cref="HttpUserAgentParserDependencyInjectionOptions"/> to add ASP.NET Core integration.
/// </summary>
public static class HttpUserAgentParserDependencyInjectionOptionsExtensions
{
    /// <summary>
    /// Registers <see cref="HttpUserAgentParserAccessor"/> as a singleton implementation of <see cref="IHttpUserAgentParserAccessor"/>.
    /// </summary>
    /// <param name="options">The options instance from the parser registration.</param>
    /// <returns>The same options instance for method chaining.</returns>
    /// <remarks>
    /// Requires a registered <see cref="IHttpUserAgentParserProvider"/>.
    /// Call this after <c>AddHttpUserAgentParser()</c> or <c>AddHttpUserAgentCachedParser()</c>.
    /// </remarks>
    /// <example>
    /// <code>
    /// IServiceCollection services = new ServiceCollection();
    /// services.AddHttpUserAgentParser()
    ///     .AddHttpUserAgentParserAccessor();
    /// </code>
    /// </example>
    public static HttpUserAgentParserDependencyInjectionOptions AddHttpUserAgentParserAccessor(
        this HttpUserAgentParserDependencyInjectionOptions options)
    {
        options.Services.AddSingleton<IHttpUserAgentParserAccessor, HttpUserAgentParserAccessor>();
        return options;
    }
}
