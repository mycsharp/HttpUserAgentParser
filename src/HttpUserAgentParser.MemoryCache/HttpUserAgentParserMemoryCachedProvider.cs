// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.Caching.Memory;
using MyCSharp.HttpUserAgentParser.MemoryCache.Telemetry;
using MyCSharp.HttpUserAgentParser.Providers;

namespace MyCSharp.HttpUserAgentParser.MemoryCache;

/// <inheritdoc/>
/// <summary>
/// Creates a new instance of <see cref="HttpUserAgentParserMemoryCachedProvider"/>.
/// </summary>
/// <param name="options">The options used to set expiration and size limit</param>
public class HttpUserAgentParserMemoryCachedProvider(
    HttpUserAgentParserMemoryCachedProviderOptions options) : IHttpUserAgentParserProvider
{
    /// <summary>
    /// The name of the Meter used for metrics.
    /// </summary>
    public const string MeterName = "mycsharp.http_user_agent_parser.memorycache";

    private readonly Microsoft.Extensions.Caching.Memory.MemoryCache _memoryCache = new(options.CacheOptions);
    private readonly HttpUserAgentParserMemoryCachedProviderOptions _options = options;

    /// <summary>
    /// Number of cached user agents.
    /// </summary>
    internal int CacheEntryCount => _memoryCache.Count;

    /// <inheritdoc/>
    /// <remarks>
    /// The user agent string itself is the cache key (ordinal comparison), so a cache hit does not allocate.
    /// Telemetry checks use a volatile flag to ensure zero overhead when disabled; cache size tracking
    /// (via <see cref="Interlocked"/> and <see cref="CacheEntryExtensions.RegisterPostEvictionCallback(ICacheEntry, PostEvictionDelegate)"/>)
    /// is skipped entirely if the size metric is not enabled.
    /// </remarks>
    public HttpUserAgentInformation Parse(string userAgent)
    {
        if (_memoryCache.TryGetValue(userAgent, out HttpUserAgentInformation cached))
        {
            if (HttpUserAgentParserMemoryCacheTelemetry.IsEnabled)
            {
                HttpUserAgentParserMemoryCacheTelemetry.CacheHit();
            }

            return cached;
        }

        return ParseAndCache(userAgent);
    }

    /// <summary>
    /// Parses the user agent and adds the result to the cache (miss path).
    /// </summary>
    /// <remarks>
    /// Like other cache implementations, concurrent misses of the same user agent may parse it more than once; the last result is kept.
    /// </remarks>
    private HttpUserAgentInformation ParseAndCache(string userAgent)
    {
        HttpUserAgentInformation information = HttpUserAgentParser.Parse(userAgent);

        using ICacheEntry entry = _memoryCache.CreateEntry(userAgent);
        entry.SlidingExpiration = _options.CacheEntryOptions.SlidingExpiration;
        entry.Size = 1;
        entry.Value = information;

        if (HttpUserAgentParserMemoryCacheTelemetry.IsEnabled)
        {
            HttpUserAgentParserMemoryCacheTelemetry.CacheMiss();

            if (HttpUserAgentParserMemoryCacheTelemetry.IsCacheSizeEnabled)
            {
                // The eviction callback also runs if the cache rejects the entry because of its size limit.
                HttpUserAgentParserMemoryCacheTelemetry.CacheSizeIncrement();
                entry.RegisterPostEvictionCallback(static (_, _, _, _) => HttpUserAgentParserMemoryCacheTelemetry.CacheSizeDecrement());
            }
        }

        return information;
    }
}
