// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.DependencyInjection;

namespace MyCSharp.HttpUserAgentParser.DependencyInjection;

/// <summary>
/// Configuration options returned by the dependency injection registration methods.
/// Used for fluent configuration of additional features.
/// </summary>
/// <remarks>
/// This class provides access to the service collection for registering additional services
/// such as telemetry or ASP.NET Core integrations.
/// </remarks>
/// <param name="services">The service collection to configure.</param>
public class HttpUserAgentParserDependencyInjectionOptions(IServiceCollection services)
{
    /// <summary>
    /// Gets the service collection being configured.
    /// </summary>
    public IServiceCollection Services { get; } = services;
}
