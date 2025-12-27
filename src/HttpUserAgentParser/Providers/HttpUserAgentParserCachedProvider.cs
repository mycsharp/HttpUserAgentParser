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
        if (!HttpUserAgentParserTelemetry.IsEnabled)
        {
            return _cache.GetOrAdd(userAgent, static ua => HttpUserAgentParser.Parse(ua));
        }

        if (_cache.TryGetValue(userAgent, out HttpUserAgentInformation cached))
        {
            HttpUserAgentParserTelemetry.ConcurrentCacheHit();
            HttpUserAgentParserTelemetry.ConcurrentCacheSizeSet(_cache.Count);
            return cached;
        }

        // Note: ConcurrentDictionary can invoke the factory multiple times in races; counters are best-effort.
        HttpUserAgentInformation result = _cache.GetOrAdd(userAgent, static ua =>
        {
            HttpUserAgentParserTelemetry.ConcurrentCacheMiss();
            return HttpUserAgentParser.Parse(ua);
        });

        HttpUserAgentParserTelemetry.ConcurrentCacheSizeSet(_cache.Count);
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
