// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using MyCSharp.HttpUserAgentParser.Providers;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.DependencyInjection;

/// <summary>
/// Dependency injection extensions for ASP.NET Core environments
/// </summary>
public static class HttpUserAgentParserDependencyInjectionOptionsExtensions
{
    /// <summary>
    /// Registers <see cref="HttpUserAgentParserAccessor"/> as <see cref="IHttpUserAgentParserAccessor"/>:
    /// the <see cref="HttpContext"/> is passed explicitly, e.g. <c>accessor.Get(httpContext)</c>.
    ///  Requires a registered <see cref="IHttpUserAgentParserProvider"/>
    /// </summary>
    /// <remarks>
    /// Does nothing if an <see cref="IHttpUserAgentParserAccessor"/> is already registered, e.g. by
    /// <see cref="AddHttpUserAgentParserContextAccessor"/>, whose accessor supports <c>Get(httpContext)</c> as well.
    /// </remarks>
    public static HttpUserAgentParserDependencyInjectionOptions AddHttpUserAgentParserAccessor(
        this HttpUserAgentParserDependencyInjectionOptions options)
    {
        options.Services.TryAddSingleton<IHttpUserAgentParserAccessor, HttpUserAgentParserAccessor>();
        return options;
    }

    /// <summary>
    /// Registers <see cref="HttpUserAgentParserContextAccessor"/> as <see cref="IHttpUserAgentParserContextAccessor"/> and,
    /// unless already registered, as <see cref="IHttpUserAgentParserAccessor"/>, plus <see cref="IHttpContextAccessor"/>:
    /// the current request is used with <c>accessor.Get()</c>, an explicit one with <c>accessor.Get(httpContext)</c>.
    ///  Requires a registered <see cref="IHttpUserAgentParserProvider"/>
    /// </summary>
    /// <remarks>
    /// <see cref="IHttpContextAccessor"/> has a small cost on every request; prefer <see cref="AddHttpUserAgentParserAccessor"/>
    /// when the <see cref="HttpContext"/> is at hand. Calling this method again has no effect.
    /// </remarks>
    public static HttpUserAgentParserDependencyInjectionOptions AddHttpUserAgentParserContextAccessor(
        this HttpUserAgentParserDependencyInjectionOptions options)
    {
        options.Services.AddHttpContextAccessor();
        options.Services.TryAddSingleton<IHttpUserAgentParserContextAccessor, HttpUserAgentParserContextAccessor>();
        options.Services.TryAddSingleton<IHttpUserAgentParserAccessor>(
            static serviceProvider => serviceProvider.GetRequiredService<IHttpUserAgentParserContextAccessor>());
        return options;
    }
}
