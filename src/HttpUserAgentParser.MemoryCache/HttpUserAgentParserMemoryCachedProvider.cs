// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.Caching.Memory;
using MyCSharp.HttpUserAgentParser.MemoryCache.Telemetry;
using MyCSharp.HttpUserAgentParser.Providers;

namespace MyCSharp.HttpUserAgentParser.MemoryCache;

/// <inheritdoc cref="IHttpUserAgentParserProvider"/>
/// <summary>
/// <see cref="IHttpUserAgentParserProvider"/> that caches the results in an <see cref="IMemoryCache"/>.
/// </summary>
/// <remarks>
/// The provider owns its <see cref="IMemoryCache"/>; disposing the provider disposes the cache.
/// </remarks>
public class HttpUserAgentParserMemoryCachedProvider : IHttpUserAgentParserProvider, IDisposable
{
    /// <summary>
    /// The name of the Meter used for metrics.
    /// </summary>
    public const string MeterName = "mycsharp.http_user_agent_parser.memorycache";

    private readonly Microsoft.Extensions.Caching.Memory.MemoryCache _memoryCache;
    private readonly HttpUserAgentParserMemoryCachedProviderOptions _options;

    /// <summary>
    /// Creates a new instance of <see cref="HttpUserAgentParserMemoryCachedProvider"/>.
    /// </summary>
    /// <param name="options">The options used to set expiration and size limit</param>
    public HttpUserAgentParserMemoryCachedProvider(HttpUserAgentParserMemoryCachedProviderOptions options)
    {
        _memoryCache = new(options.CacheOptions);
        _options = options;

        // Known independent of telemetry, so the cache size also includes entries cached before telemetry was enabled.
        HttpUserAgentParserMemoryCacheTelemetryState.Register(this);
    }

    /// <summary>
    /// Number of cached user agents.
    /// </summary>
    internal int CacheEntryCount => _memoryCache.Count;

    /// <inheritdoc/>
    /// <remarks>
    /// The user agent string itself is the cache key (ordinal comparison), so a cache hit does not allocate.
    /// Telemetry checks use a volatile flag to ensure zero overhead when disabled; the cache size is counted only
    /// when a cache size instrument is observed.
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
    /// Disposes the internal <see cref="IMemoryCache"/>; the provider must not be used afterwards.
    /// </summary>
    public void Dispose()
    {
        HttpUserAgentParserMemoryCacheTelemetryState.Unregister(this);
        _memoryCache.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Parses the user agent and adds the result to the cache (miss path).
    /// </summary>
    /// <remarks>
    /// Every entry gets all <see cref="HttpUserAgentParserMemoryCachedProviderOptions.CacheEntryOptions"/>
    /// (expiration, priority, expiration tokens, eviction callbacks) and the size 1, unless the options specify a size.
    /// Like other cache implementations, concurrent misses of the same user agent may parse it more than once; the last result is kept.
    /// </remarks>
    private HttpUserAgentInformation ParseAndCache(string userAgent)
    {
        HttpUserAgentInformation information = HttpUserAgentParser.Parse(userAgent);

        MemoryCacheEntryOptions entryOptions = _options.CacheEntryOptions;
        using ICacheEntry entry = _memoryCache.CreateEntry(userAgent);
        entry.SetOptions(entryOptions);
        entry.Size = entryOptions.Size ?? 1;
        entry.Value = information;

        if (HttpUserAgentParserMemoryCacheTelemetry.IsEnabled)
        {
            HttpUserAgentParserMemoryCacheTelemetry.CacheMiss();
        }

        return information;
    }
}
