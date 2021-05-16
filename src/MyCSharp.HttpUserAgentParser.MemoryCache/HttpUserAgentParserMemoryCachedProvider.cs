using Microsoft.Extensions.Caching.Memory;
using MyCSharp.HttpUserAgentParser.Providers;
using System;

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

        public HttpUserAgentInformation Parse(string userAgent)
        {
            CacheKey key = GetKey(userAgent);

            return _memoryCache.GetOrCreate(key, static entry =>
            {
                CacheKey key = (entry.Key as CacheKey)!;
                entry.SlidingExpiration = key.Options.CacheEntryOptions.SlidingExpiration;
                entry.SetSize(1);

                return HttpUserAgentParser.Parse(key.UserAgent);
            });
        }

        [ThreadStatic]
        private static CacheKey? t_key;

        private CacheKey GetKey(string userAgent)
        {
            CacheKey key = t_key ??= new();

            key.UserAgent = userAgent;
            key.Options = _options;

            return key;
        }

        private class CacheKey
        {
            public string UserAgent { get; set; } = null!;

            public HttpUserAgentParserMemoryCachedProviderOptions Options { get; set; } = null!;
        }
    }
}