using Microsoft.Extensions.DependencyInjection;

namespace MyCSharp.HttpUserAgentParser.DependencyInjection
{
    public class HttpUserAgentParserDependencyInjectionOptions
    {
        public IServiceCollection Services { get; }

        public HttpUserAgentParserDependencyInjectionOptions(IServiceCollection services)
        {
            Services = services;
        }
    }
}