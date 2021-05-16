using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.Providers;

namespace MyCSharp.HttpUserAgentParser.DependencyInjection
{
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
        public static HttpUserAgentParserDependencyInjectionOptions AddCachedHttpUserAgentParser(
            this IServiceCollection services)
        {
            return AddHttpUserAgentParser<HttpUserAgentParserCachedProvider>(services);
        }

        /// <summary>
        /// Registers <typeparam name="TProvider"/> as singleton to <see cref="IHttpUserAgentParserProvider"/>
        /// </summary>
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
}
