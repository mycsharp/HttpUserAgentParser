using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.Providers;

namespace MyCSharp.HttpUserAgentParser.DependencyInjection
{
    public static class HttpUserAgentParserServiceCollectionExtensions
    {
        /// <summary>
        /// Registers <see cref="DefaultHttpUserAgentParserProvider"/> as singleton to <see cref="IHttpUserAgentParserProvider"/>
        /// </summary>
        public static HttpUserAgentParserDependencyInjectionOptions AddHttpUserAgentParser(
            this IServiceCollection services)
        {
            return AddHttpUserAgentParser<DefaultHttpUserAgentParserProvider>(services);
        }

        /// <summary>
        /// Registers <see cref="CachedHttpUserAgentParserProvider"/> as singleton to <see cref="IHttpUserAgentParserProvider"/>
        /// </summary>
        public static HttpUserAgentParserDependencyInjectionOptions AddCachedHttpUserAgentParser(
            this IServiceCollection services)
        {
            return AddHttpUserAgentParser<CachedHttpUserAgentParserProvider>(services);
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
