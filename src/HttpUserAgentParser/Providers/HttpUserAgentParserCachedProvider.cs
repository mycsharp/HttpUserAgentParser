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
    /// <example>
    /// <code>
    /// HttpUserAgentInformation info = provider.Parse("Mozilla/5.0 Chrome/90.0.4430.212");
    /// </code>
    /// </example>
    public HttpUserAgentInformation Parse(string userAgent)
        => _cache.GetOrAdd(userAgent, static ua => HttpUserAgentParser.Parse(ua));

    /// <summary>
    /// Total count of entries in cache
    /// </summary>
    public int CacheEntryCount => _cache.Count;

    /// <summary>
    /// returns true if given user agent is in cache
    /// </summary>
    /// <example>
    /// <code>
    /// bool cached = provider.HasCacheEntry("Mozilla/5.0 Chrome/90.0.4430.212");
    /// </code>
    /// </example>
    public bool HasCacheEntry(string userAgent) => _cache.ContainsKey(userAgent);
}
