// Copyright © myCSharp 2020-2021, all rights reserved

using System;
using Microsoft.Extensions.Caching.Memory;
using MyCSharp.HttpUserAgentParser.Providers;

namespace MyCSharp.HttpUserAgentParser.MemoryCache
{
    /// <inheritdoc/>
    public class HttpUserAgentParserMemoryCachedProvider : IHttpUserAgentParserProvider
    {
        private readonly IMemoryCache _memoryCache;
        private readonly HttpUserAgentParserMemoryCachedProviderOptions _options;

        public HttpUserAgentParserMemoryCachedProvider(IMemoryCache memoryCache, HttpUserAgentParserMemoryCachedProviderOptions options)
        {
            _memoryCache = memoryCache;
            _options = options;
        }

        /// <inheritdoc/>
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

            public bool Equals(CacheKey? other)
            {
                if (ReferenceEquals(this, other)) return true;
                if (other is null) return false;

                return this.UserAgent == other.UserAgent && this.Options == other.Options;
            }

            public override bool Equals(object? obj)
            {
                return this.Equals(obj as CacheKey);
            }

            public override int GetHashCode() => HashCode.Combine(this.UserAgent, this.Options);
        }
    }
}
