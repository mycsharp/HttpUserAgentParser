// Copyright © myCSharp 2020-2021, all rights reserved

using System;
using Microsoft.Extensions.Caching.Memory;

namespace MyCSharp.HttpUserAgentParser.MemoryCache
{
    /// <summary>
    /// Provider options for <see cref="HttpUserAgentParserMemoryCachedProvider"/>
    /// <remarks>
    /// Default of <seealso cref="MemoryCacheEntryOptions.SlidingExpiration"/> is 1 day
    /// </remarks>
    /// </summary>
    public class HttpUserAgentParserMemoryCachedProviderOptions
    {
        public MemoryCacheOptions CacheOptions { get; }
        public MemoryCacheEntryOptions CacheEntryOptions { get; }

        public HttpUserAgentParserMemoryCachedProviderOptions(MemoryCacheOptions cacheOptions)
            : this(cacheOptions, null) { }

        public HttpUserAgentParserMemoryCachedProviderOptions(MemoryCacheEntryOptions cacheEntryOptions)
            : this(null, cacheEntryOptions) { }

        public HttpUserAgentParserMemoryCachedProviderOptions(MemoryCacheOptions? cacheOptions = null, MemoryCacheEntryOptions? cacheEntryOptions = null)
        {
            this.CacheEntryOptions = cacheEntryOptions ?? new MemoryCacheEntryOptions
            {
                // defaults
                SlidingExpiration = TimeSpan.FromDays(1)
            };
            this.CacheOptions = cacheOptions ?? new MemoryCacheOptions();
        }
    }
}
