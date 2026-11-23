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
    /// internal cache
    /// </summary>
    /// <remarks>
    /// The key is the exact user agent (ordinal): the result contains the user agent, so a differently cased one is another entry.
    /// </remarks>
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
    /// Creates a provider with an empty cache.
    /// </summary>
    public HttpUserAgentParserCachedProvider() => HttpUserAgentParserTelemetryState.Register(this);

    /// <summary>
    /// Parses the user agent or uses the internal cached information
    /// </summary>
    /// <remarks>
    /// Telemetry checks use a volatile flag to ensure zero overhead when disabled. The cache size is counted only
    /// when a cache size instrument is observed.
    /// </remarks>
    public HttpUserAgentInformation Parse(string userAgent)
    {
        Cache cache = GetCurrentCache();
        if (!HttpUserAgentParserTelemetry.IsEnabled)
        {
            return cache.Entries.GetOrAdd(userAgent, static (ua, robots) => HttpUserAgentParser.Parse(ua, robots), cache.Robots);
        }

        if (cache.Entries.TryGetValue(userAgent, out HttpUserAgentInformation cached))
        {
            HttpUserAgentParserTelemetry.ConcurrentCacheHit();
            return cached;
        }

        // Report the observed miss after insertion so a synchronous listener can read the entry.
        HttpUserAgentInformation information = cache.Entries.GetOrAdd(userAgent, static (ua, robots) => HttpUserAgentParser.Parse(ua, robots), cache.Robots);
        HttpUserAgentParserTelemetry.ConcurrentCacheMiss();
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
