// Copyright © myCSharp 2020-2021, all rights reserved

using System;
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

        public HttpUserAgentInformation Parse(string userAgent)
        {
            CacheKey key = this.GetKey(userAgent);

            return _memoryCache.GetOrCreate(key, static entry =>
            {
                CacheKey key = (entry.Key as CacheKey)!;
                entry.SlidingExpiration = key.Options.CacheEntryOptions.SlidingExpiration;
                entry.SetSize(1);

                return HttpUserAgentParser.Parse(key.UserAgent);
            });
        }

        [ThreadStatic]
        private static CacheKey? s_tKey;

        private CacheKey GetKey(string userAgent)
        {
            CacheKey key = s_tKey ??= new CacheKey();

            key.UserAgent = userAgent;
            key.Options = _options;

            return key;
        }

        private class CacheKey : IEquatable<CacheKey> // required for IMemoryCache
        {
            public string UserAgent { get; set; } = null!;

            public HttpUserAgentParserMemoryCachedProviderOptions Options { get; set; } = null!;

            public bool Equals(CacheKey? other) => this.UserAgent == other?.UserAgent;
            public override bool Equals(object? obj) => this.Equals(obj as CacheKey);

            public override int GetHashCode() => this.UserAgent.GetHashCode();
        }
    }
}
