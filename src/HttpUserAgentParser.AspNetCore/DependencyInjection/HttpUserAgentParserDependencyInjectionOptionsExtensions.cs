// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using MyCSharp.HttpUserAgentParser.Providers;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.DependencyInjection;

/// <summary>
/// Dependency injection extensions for ASP.NET Core environments
/// </summary>
public static class HttpUserAgentParserDependencyInjectionOptionsExtensions
{
    /// <summary>
    /// Registers <see cref="HttpUserAgentParserAccessor"/> as <see cref="IHttpUserAgentParserAccessor"/>.
    ///  Requires a registered <see cref="IHttpUserAgentParserProvider"/>
    /// </summary>
    public static HttpUserAgentParserDependencyInjectionOptions AddHttpUserAgentParserAccessor(
        this HttpUserAgentParserDependencyInjectionOptions options)
    {
        options.Services.AddSingleton<IHttpUserAgentParserAccessor, HttpUserAgentParserAccessor>();
        return options;
    }
}
