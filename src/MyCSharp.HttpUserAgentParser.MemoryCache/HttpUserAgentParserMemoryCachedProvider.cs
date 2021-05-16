using Microsoft.Extensions.Caching.Memory;
using MyCSharp.HttpUserAgentParser.Providers;

namespace MyCSharp.HttpUserAgentParser.MemoryCache
{
    public class HttpUserAgentParserMemoryCachedProvider : IHttpUserAgentParserProvider
    {
        private readonly IMemoryCache _memoryCache;
        private readonly HttpUserAgentParserMemoryCachedProviderOptions _options;

        public HttpUserAgentParserMemoryCachedProvider(IMemoryCache memoryCache, HttpUserAgentParserMemoryCachedProviderOptions options)
        {
            _memoryCache = memoryCache;
            _options = options;
        }

        private static string Cleanup(string userAgent)
            => userAgent.Trim();


        public HttpUserAgentInformation Parse(string userAgent)
        {
            return _memoryCache.GetOrCreate(Cleanup(userAgent), entry =>
            {
                entry.SlidingExpiration = _options.CacheEntryOptions.SlidingExpiration;
                return HttpUserAgentInformation.Parse(userAgent);
            });
        }
    }
}