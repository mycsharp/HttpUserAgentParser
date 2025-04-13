// Copyright © https://myCSharp.de - all rights reserved

using System.Collections.Concurrent;

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
        => _cache.GetOrAdd(userAgent, static ua => HttpUserAgentParser.Parse(ua));

    /// <summary>
    /// Total count of entries in cache
    /// </summary>
    public int CacheEntryCount => _cache.Count;

    /// <summary>
    /// returns true if given user agent is in cache
    /// </summary>
    public bool HasCacheEntry(string userAgent) => _cache.ContainsKey(userAgent);
}
