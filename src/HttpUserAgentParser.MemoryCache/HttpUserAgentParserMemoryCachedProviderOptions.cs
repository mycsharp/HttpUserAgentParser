// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.Caching.Memory;

namespace MyCSharp.HttpUserAgentParser.MemoryCache;

/// <summary>
/// Provider options for <see cref="HttpUserAgentParserMemoryCachedProvider"/>
/// <remarks>
/// Default of <seealso cref="MemoryCacheOptions.SizeLimit"/> is 256.
/// Default of <seealso cref="MemoryCacheEntryOptions.SlidingExpiration"/> is 1 day
/// </remarks>
/// </summary>
public class HttpUserAgentParserMemoryCachedProviderOptions
{
    /// <summary>
    /// Cache options
    /// </summary>
    public MemoryCacheOptions CacheOptions { get; }

    /// <summary>
    /// Cache entry options
    /// </summary>
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
