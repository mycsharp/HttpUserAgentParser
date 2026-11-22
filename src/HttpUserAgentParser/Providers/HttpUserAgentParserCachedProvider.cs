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
    private readonly ConcurrentDictionary<string, HttpUserAgentInformation> _cache = new(StringComparer.Ordinal);

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
        return _cache.GetOrAdd(userAgent, static ua =>
        {
            HttpUserAgentParserTelemetry.ConcurrentCacheMiss();
            return HttpUserAgentParser.Parse(ua);
        });
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
