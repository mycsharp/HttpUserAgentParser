// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.DependencyInjection;

namespace MyCSharp.HttpUserAgentParser.DependencyInjection;

/// <summary>
/// Options for dependency injection
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="HttpUserAgentParserDependencyInjectionOptions"/>
/// </remarks>
/// <param name="services"></param>
public class HttpUserAgentParserDependencyInjectionOptions(IServiceCollection services)
{
    /// <summary>
    /// Services container
    /// </summary>
    public IServiceCollection Services { get; } = services;
}
