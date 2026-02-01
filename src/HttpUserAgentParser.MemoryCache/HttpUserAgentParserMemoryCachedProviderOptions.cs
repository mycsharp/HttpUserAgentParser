// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.Caching.Memory;

namespace MyCSharp.HttpUserAgentParser.MemoryCache;

/// <summary>
/// Configuration options for <see cref="HttpUserAgentParserMemoryCachedProvider"/>.
/// </summary>
/// <remarks>
/// <para>Default <see cref="MemoryCacheOptions.SizeLimit"/>: 256 entries.</para>
/// <para>Default <see cref="MemoryCacheEntryOptions.SlidingExpiration"/>: 1 day.</para>
/// </remarks>
public class HttpUserAgentParserMemoryCachedProviderOptions
{
    /// <summary>
    /// Gets the memory cache configuration options.
    /// </summary>
    /// <remarks>Controls size limits and compaction behavior.</remarks>
    public MemoryCacheOptions CacheOptions { get; }

    /// <summary>
    /// Gets the options applied to each cache entry.
    /// </summary>
    /// <remarks>Controls sliding expiration for cached user agent information.</remarks>
    public MemoryCacheEntryOptions CacheEntryOptions { get; }

    /// <summary>
    /// Creates a new instance of <see cref="HttpUserAgentParserMemoryCachedProviderOptions"/>
    /// </summary>
    /// <example>
    /// <code>
    /// var options = new HttpUserAgentParserMemoryCachedProviderOptions(new MemoryCacheOptions { SizeLimit = 512 });
    /// </code>
    /// </example>
    public HttpUserAgentParserMemoryCachedProviderOptions(MemoryCacheOptions cacheOptions)
        : this(cacheOptions, null) { }

    /// <summary>
    /// Creates a new instance of <see cref="HttpUserAgentParserMemoryCachedProviderOptions"/>
    /// </summary>
    /// <example>
    /// <code>
    /// var options = new HttpUserAgentParserMemoryCachedProviderOptions(
    ///     new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromHours(6) });
    /// </code>
    /// </example>
    public HttpUserAgentParserMemoryCachedProviderOptions(MemoryCacheEntryOptions cacheEntryOptions)
        : this(null, cacheEntryOptions) { }

    /// <summary>
    /// Creates a new instance of <see cref="HttpUserAgentParserMemoryCachedProviderOptions"/>
    /// </summary>
    /// <example>
    /// <code>
    /// var options = new HttpUserAgentParserMemoryCachedProviderOptions(
    ///     new MemoryCacheOptions { SizeLimit = 512 },
    ///     new MemoryCacheEntryOptions { SlidingExpiration = TimeSpan.FromHours(6) });
    /// </code>
    /// </example>
    public HttpUserAgentParserMemoryCachedProviderOptions(
        MemoryCacheOptions? cacheOptions = null, MemoryCacheEntryOptions? cacheEntryOptions = null)
    {
        CacheEntryOptions = cacheEntryOptions ?? new MemoryCacheEntryOptions
        {
            // defaults
            SlidingExpiration = TimeSpan.FromDays(1)
        };

        CacheOptions = cacheOptions ?? new MemoryCacheOptions
        {
            // defaults
            SizeLimit = 256
        };
    }
}
