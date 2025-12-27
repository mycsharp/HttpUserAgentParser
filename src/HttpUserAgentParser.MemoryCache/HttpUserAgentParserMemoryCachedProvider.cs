// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.CodeAnalysis;
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
    public const string MeterName = "MyCSharp.HttpUserAgentParser.MemoryCache";

    private readonly Microsoft.Extensions.Caching.Memory.MemoryCache _memoryCache = new(options.CacheOptions);
    private readonly HttpUserAgentParserMemoryCachedProviderOptions _options = options;

    /// <inheritdoc/>
    /// <remarks>
    /// This method includes performance optimizations for telemetry:
    /// <list type="bullet">
    /// <item><description>Telemetry checks use a volatile flag to ensure zero overhead when disabled.</description></item>
    /// <item><description>Cache size tracking (via <see cref="Interlocked"/> and <see cref="CacheEntryExtensions.RegisterPostEvictionCallback(ICacheEntry, PostEvictionDelegate)"/>) is skipped entirely if the size metric is not enabled to avoid allocations.</description></item>
    /// </list>
    /// </remarks>
    public HttpUserAgentInformation Parse(string userAgent)
    {
        CacheKey key = GetKey(userAgent);

        if (!HttpUserAgentParserMemoryCacheTelemetry.IsEnabled)
        {
            return ParseWithoutTelemetry(key);
        }

        if (_memoryCache.TryGetValue(key, out HttpUserAgentInformation cached))
        {
            HttpUserAgentParserMemoryCacheTelemetry.CacheHit();
            return cached;
        }

        return _memoryCache.GetOrCreate(key, static entry =>
        {
            CacheKey key = (entry.Key as CacheKey)!;
            entry.SlidingExpiration = key.Options.CacheEntryOptions.SlidingExpiration;
            entry.SetSize(1);

            // Miss path. Note: Like other cache implementations, races can happen; counters are best-effort.
            HttpUserAgentParserMemoryCacheTelemetry.CacheMiss();

            if (HttpUserAgentParserMemoryCacheTelemetry.IsCacheSizeEnabled)
            {
                // Optimization: Avoid Interlocked overhead and delegate allocation if telemetry is disabled.
                HttpUserAgentParserMemoryCacheTelemetry.CacheSizeIncrement();
                entry.RegisterPostEvictionCallback(static (_, _, _, _) => HttpUserAgentParserMemoryCacheTelemetry.CacheSizeDecrement());
            }

            return HttpUserAgentParser.Parse(key.UserAgent);
        });
    }

    /// <summary>
    /// Parses the user agent string using the memory cache without emitting telemetry.
    /// </summary>
    /// <remarks>
    /// This method is excluded from code coverage as it mainly wires together
    /// cache access and parsing logic without additional behavior.
    /// </remarks>
    [ExcludeFromCodeCoverage]
    private HttpUserAgentInformation ParseWithoutTelemetry(CacheKey key)
    {
        return _memoryCache.GetOrCreate(key, static entry =>
        {
            CacheKey key = (entry.Key as CacheKey)!;

            entry.SlidingExpiration = key.Options.CacheEntryOptions.SlidingExpiration;
            entry.SetSize(1);

            return HttpUserAgentParser.Parse(key.UserAgent);
        });
    }

    /// <summary>
    /// Thread-local reusable cache key instance to avoid per-call allocations.
    /// </summary>
    /// <remarks>
    /// Marked as <see langword="ThreadStatic"/> to ensure thread safety without locking.
    /// </remarks>
    [ThreadStatic]
    private static CacheKey? s_tKey;

    /// <summary>
    /// Gets a cache key instance initialized for the specified user agent.
    /// </summary>
    /// <remarks>
    /// Reuses a thread-local instance to minimize allocations. The returned instance
    /// must not be stored or shared across threads.
    /// </remarks>
    private CacheKey GetKey(string userAgent)
    {
        CacheKey key = s_tKey ??= new CacheKey();

        key.UserAgent = userAgent;
        key.Options = _options;

        return key;
    }

    /// <summary>
    /// Cache key used for memory-cached HTTP User-Agent parsing.
    /// </summary>
    /// <remarks>
    /// Implements <see cref="IEquatable{T}"/> as required by <c>IMemoryCache</c>
    /// to ensure correct key comparison semantics.
    /// </remarks>
    private class CacheKey : IEquatable<CacheKey> // required for IMemoryCache
    {
        /// <summary>
        /// Gets or sets the raw User-Agent string.
        /// </summary>
        public string UserAgent { get; set; } = null!;

        /// <summary>
        /// Gets or sets the cache configuration options associated with this key.
        /// </summary>
        public HttpUserAgentParserMemoryCachedProviderOptions Options { get; set; } = null!;

        /// <summary>
        /// Determines equality based on the User-Agent string, ignoring case.
        /// </summary>
        public bool Equals(CacheKey? other)
            => string.Equals(UserAgent, other?.UserAgent, StringComparison.OrdinalIgnoreCase);

        /// <inheritdoc />
        public override bool Equals(object? obj)
            => Equals(obj as CacheKey);

        /// <summary>
        /// Returns a hash code based on the User-Agent string.
        /// </summary>
        /// <remarks>
        /// Uses ordinal comparison for performance and consistency with the cache.
        /// </remarks>
        public override int GetHashCode()
            => UserAgent.GetHashCode(StringComparison.Ordinal);
    }
}
