// Copyright © https://myCSharp.de - all rights reserved

using System.Collections.Concurrent;

namespace MyCSharp.HttpUserAgentParser.Providers;

/// <summary>
/// Implementation of <see cref="IHttpUserAgentParserProvider"/> with in-memory caching using <see cref="ConcurrentDictionary{TKey, TValue}"/>.
/// </summary>
/// <remarks>
/// Parsed results are cached indefinitely. The cache grows unbounded.
/// For production use with expiration support, consider using the
/// <c>MyCSharp.HttpUserAgentParser.MemoryCache</c> package instead.
/// </remarks>
public class HttpUserAgentParserCachedProvider : IHttpUserAgentParserProvider
{
    private readonly ConcurrentDictionary<string, HttpUserAgentInformation> _cache = new(StringComparer.OrdinalIgnoreCase);

    /// <inheritdoc/>
    /// <example>
    /// <code>
    /// IHttpUserAgentParserProvider provider = new HttpUserAgentParserCachedProvider();
    /// HttpUserAgentInformation info = provider.Parse("Mozilla/5.0 Chrome/90.0.4430.212");
    /// // Subsequent calls with the same user agent return cached results.
    /// </code>
    /// </example>
    public HttpUserAgentInformation Parse(string userAgent)
        => _cache.GetOrAdd(userAgent, static ua => HttpUserAgentParser.Parse(ua));

    /// <summary>
    /// Gets the number of entries currently stored in the cache.
    /// </summary>
    public int CacheEntryCount => _cache.Count;

    /// <summary>
    /// Determines whether the specified user agent is already cached.
    /// </summary>
    /// <param name="userAgent">The user agent string to check.</param>
    /// <returns><see langword="true"/> if the user agent is in the cache; otherwise, <see langword="false"/>.</returns>
    /// <example>
    /// <code>
    /// HttpUserAgentParserCachedProvider provider = new HttpUserAgentParserCachedProvider();
    /// provider.Parse("Mozilla/5.0 Chrome/90.0");
    /// bool cached = provider.HasCacheEntry("Mozilla/5.0 Chrome/90.0"); // true
    /// </code>
    /// </example>
    public bool HasCacheEntry(string userAgent) => _cache.ContainsKey(userAgent);
}
