// Copyright © https://myCSharp.de - all rights reserved

using System.Collections.Concurrent;
using MyCSharp.HttpUserAgentParser.Telemetry;

namespace MyCSharp.HttpUserAgentParser.Providers;

/// <summary>
/// In process cache provider for <see cref="IHttpUserAgentParserProvider"/>
/// </summary>
public class HttpUserAgentParserCachedProvider : IHttpUserAgentParserProvider
{
    /// <summary>
    /// Default maximum number of cached user agent entries before the cache is rotated.
    /// </summary>
    public const int DefaultMaxCacheEntries = 10_000;

    private sealed class Cache(HttpUserAgentRobotSet robots)
    {
        public HttpUserAgentRobotSet Robots { get; } = robots;
        public ConcurrentDictionary<string, HttpUserAgentInformation> Entries { get; } = new(StringComparer.Ordinal);
    }

    private Cache _cache = new(HttpUserAgentRules.RobotSet);

    private Cache GetCurrentCache()
    {
        while (true)
        {
            Cache cache = Volatile.Read(ref _cache);
            HttpUserAgentRobotSet robots = HttpUserAgentRules.RobotSet;
            if (ReferenceEquals(cache.Robots, robots))
            {
                return cache;
            }

            // A parse already in progress can finish writing to its own generation.
            Interlocked.CompareExchange(ref _cache, new Cache(robots), cache);
        }
    }

    /// <summary>
    /// Creates a provider with an empty cache limited to <see cref="DefaultMaxCacheEntries"/> entries.
    /// </summary>
    public HttpUserAgentParserCachedProvider()
        : this(DefaultMaxCacheEntries)
    {
    }

    /// <summary>
    /// Creates a provider with an empty cache limited to <paramref name="maxCacheEntries"/> entries.
    /// </summary>
    /// <param name="maxCacheEntries">Maximum number of entries retained before the cache is rotated.</param>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="maxCacheEntries"/> is less than or equal to zero.</exception>
    public HttpUserAgentParserCachedProvider(int maxCacheEntries)
    {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(maxCacheEntries);

        MaxCacheEntries = maxCacheEntries;
        HttpUserAgentParserTelemetryState.Register(this);
    }

    /// <summary>
    /// Maximum number of entries retained in the cache before it is rotated.
    /// </summary>
    public int MaxCacheEntries { get; }

    /// <inheritdoc/>
    /// <remarks>
    /// Cache keys use ordinal comparison because each result retains the original header.
    /// </remarks>
    public HttpUserAgentInformation Parse(string userAgent)
    {
        Cache cache = GetCurrentCache();
        if (cache.Entries.TryGetValue(userAgent, out HttpUserAgentInformation cached))
        {
            if (HttpUserAgentParserTelemetry.IsEnabled)
            {
                HttpUserAgentParserTelemetry.ConcurrentCacheHit();
            }

            return cached;
        }

        if (cache.Entries.Count >= MaxCacheEntries)
        {
            Interlocked.CompareExchange(ref _cache, new Cache(cache.Robots), cache);
            cache = GetCurrentCache();
        }

        // Report the observed miss after insertion so a synchronous listener can read the entry.
        HttpUserAgentInformation information = cache.Entries.GetOrAdd(userAgent, static (ua, robots) => HttpUserAgentParser.Parse(ua, robots), cache.Robots);
        if (HttpUserAgentParserTelemetry.IsEnabled)
        {
            HttpUserAgentParserTelemetry.ConcurrentCacheMiss();
        }

        return information;
    }

    /// <summary>
    /// Total count of entries in cache
    /// </summary>
    public int CacheEntryCount => GetCurrentCache().Entries.Count;

    /// <summary>
    /// returns true if given user agent is in cache
    /// </summary>
    public bool HasCacheEntry(string userAgent) => GetCurrentCache().Entries.ContainsKey(userAgent);
}
