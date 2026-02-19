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
    /// <remarks>
    /// This method includes performance optimizations for telemetry:
    /// <list type="bullet">
    /// <item><description>Telemetry checks use a volatile flag to ensure zero overhead when disabled.</description></item>
    /// <item><description>Cache size reporting (which requires an expensive <see cref="ConcurrentDictionary{TKey,TValue}.Count"/> lock) is only executed if the specific metric is enabled.</description></item>
    /// </list>
    /// </remarks>
    public HttpUserAgentInformation Parse(string userAgent)
    {
        if (!HttpUserAgentParserTelemetry.IsEnabled)
        {
            return _cache.GetOrAdd(userAgent, static ua => HttpUserAgentParser.Parse(ua));
        }

        if (_cache.TryGetValue(userAgent, out HttpUserAgentInformation cached))
        {
            HttpUserAgentParserTelemetry.ConcurrentCacheHit();
            return cached;
        }

        // Note: ConcurrentDictionary can invoke the factory multiple times in races; counters are best-effort.
        HttpUserAgentInformation result = _cache.GetOrAdd(userAgent, static ua =>
        {
            HttpUserAgentParserTelemetry.ConcurrentCacheMiss();
            return HttpUserAgentParser.Parse(ua);
        });

        if (HttpUserAgentParserTelemetry.IsCacheSizeEnabled)
        {
            // Optimization: Avoid expensive .Count property access (locks all buckets) if telemetry is disabled.
            HttpUserAgentParserTelemetry.ConcurrentCacheSizeSet(_cache.Count);
        }

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
