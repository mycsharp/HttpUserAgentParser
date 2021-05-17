// Copyright © myCSharp 2020-2021, all rights reserved

using Microsoft.Extensions.DependencyInjection;

namespace MyCSharp.HttpUserAgentParser.DependencyInjection
{
    /// <summary>
    /// Options for dependency injection
    /// </summary>
    public class HttpUserAgentParserDependencyInjectionOptions
    {
        /// <summary>
        /// Services container
        /// </summary>
        public IServiceCollection Services { get; }

        /// <summary>
        /// Creates a new instance of <see cref="HttpUserAgentParserDependencyInjectionOptions"/>
        /// </summary>
        /// <param name="services"></param>
        public HttpUserAgentParserDependencyInjectionOptions(IServiceCollection services)
        {
            Services = services;
        }
    }
}
