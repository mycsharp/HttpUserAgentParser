// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.Caching.Memory;
using MyCSharp.HttpUserAgentParser.Providers;

namespace MyCSharp.HttpUserAgentParser.MemoryCache;

/// <summary>
/// Implementation of <see cref="IHttpUserAgentParserProvider"/> with caching using <see cref="Microsoft.Extensions.Caching.Memory.MemoryCache"/>.
/// </summary>
/// <remarks>
/// <para>Provides sliding expiration and size limits for cached entries.</para>
/// <para>Default configuration: 256 entries maximum, 1 day sliding expiration.</para>
/// </remarks>
/// <param name="options">The options controlling cache size and expiration.</param>
public class HttpUserAgentParserMemoryCachedProvider(
    HttpUserAgentParserMemoryCachedProviderOptions options) : IHttpUserAgentParserProvider
{
    private readonly Microsoft.Extensions.Caching.Memory.MemoryCache _memoryCache = new(options.CacheOptions);
    private readonly HttpUserAgentParserMemoryCachedProviderOptions _options = options;

    /// <inheritdoc/>
    /// <example>
    /// <code>
    /// HttpUserAgentParserMemoryCachedProviderOptions options = new();
    /// HttpUserAgentParserMemoryCachedProvider provider = new(options);
    /// HttpUserAgentInformation info = provider.Parse("Mozilla/5.0 Chrome/90.0.4430.212");
    /// </code>
    /// </example>
    public HttpUserAgentInformation Parse(string userAgent)
    {
        CacheKey key = GetKey(userAgent);

        return _memoryCache.GetOrCreate(key, static entry =>
        {
            CacheKey key = (entry.Key as CacheKey)!;
            entry.SlidingExpiration = key.Options.CacheEntryOptions.SlidingExpiration;
            entry.SetSize(1);

            return HttpUserAgentParser.Parse(key.UserAgent);
        });
    }

    [ThreadStatic]
    private static CacheKey? s_tKey;

    private CacheKey GetKey(string userAgent)
    {
        CacheKey key = s_tKey ??= new CacheKey();

        key.UserAgent = userAgent;
        key.Options = _options;

        return key;
    }

    private class CacheKey : IEquatable<CacheKey> // required for IMemoryCache
    {
        public string UserAgent { get; set; } = null!;

        public HttpUserAgentParserMemoryCachedProviderOptions Options { get; set; } = null!;

        public bool Equals(CacheKey? other) => string.Equals(UserAgent, other?.UserAgent, StringComparison.OrdinalIgnoreCase);
        public override bool Equals(object? obj) => Equals(obj as CacheKey);

        public override int GetHashCode() => UserAgent.GetHashCode(StringComparison.Ordinal);
    }
}
