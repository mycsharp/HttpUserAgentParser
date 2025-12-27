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
    private readonly ConcurrentDictionary<string, HttpUserAgentInformation> _cache = new(StringComparer.OrdinalIgnoreCase);

    /// <summary>
    /// Parses the user agent or uses the internal cached information
    /// </summary>
    public HttpUserAgentInformation Parse(string userAgent)
    {
        if (!HttpUserAgentParserTelemetry.AreCountersEnabled)
            return _cache.GetOrAdd(userAgent, static ua => HttpUserAgentParser.Parse(ua));

        if (_cache.TryGetValue(userAgent, out HttpUserAgentInformation cached))
        {
            HttpUserAgentParserEventSource.Log.ConcurrentCacheHit();
            HttpUserAgentParserEventSource.Log.ConcurrentCacheSizeSet(_cache.Count);
            return cached;
        }

        // Note: ConcurrentDictionary can invoke the factory multiple times in races; counters are best-effort.
        HttpUserAgentInformation result = _cache.GetOrAdd(userAgent, static ua =>
        {
            HttpUserAgentParserEventSource.Log.ConcurrentCacheMiss();
            return HttpUserAgentParser.Parse(ua);
        });

        HttpUserAgentParserEventSource.Log.ConcurrentCacheSizeSet(_cache.Count);
        return result;
    }

    /// <summary>
    /// Total count of entries in cache
    /// </summary>
    public int CacheEntryCount => _cache.Count;

    /// <summary>
    /// returns true if given user agent is in cache
    /// </summary>
    public bool HasCacheEntry(string userAgent) => _cache.ContainsKey(userAgent);
}
