// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.CodeAnalysis;
using Microsoft.Extensions.Caching.Memory;
using MyCSharp.HttpUserAgentParser.Providers;
using MyCSharp.HttpUserAgentParser.MemoryCache.Telemetry;

namespace MyCSharp.HttpUserAgentParser.MemoryCache;

/// <inheritdoc/>
/// <summary>
/// Creates a new instance of <see cref="HttpUserAgentParserMemoryCachedProvider"/>.
/// </summary>
/// <param name="options">The options used to set expiration and size limit</param>
public class HttpUserAgentParserMemoryCachedProvider(
    HttpUserAgentParserMemoryCachedProviderOptions options) : IHttpUserAgentParserProvider
{
    private readonly Microsoft.Extensions.Caching.Memory.MemoryCache _memoryCache = new(options.CacheOptions);
    private readonly HttpUserAgentParserMemoryCachedProviderOptions _options = options;

    /// <inheritdoc/>
    public HttpUserAgentInformation Parse(string userAgent)
    {
        CacheKey key = GetKey(userAgent);

        if (!HttpUserAgentParserMemoryCacheTelemetry.IsEnabled)
        {
            return ParseWithoutTelemetry(key);
        }

        if (_memoryCache.TryGetValue(key, out HttpUserAgentInformation cached))
        {
            HttpUserAgentParserMemoryCacheTelemetry.CacheHit();
            return cached;
        }

        return _memoryCache.GetOrCreate(key, static entry =>
        {
            CacheKey key = (entry.Key as CacheKey)!;
            entry.SlidingExpiration = key.Options.CacheEntryOptions.SlidingExpiration;
            entry.SetSize(1);

            // Miss path. Note: Like other cache implementations, races can happen; counters are best-effort.
            HttpUserAgentParserMemoryCacheTelemetry.CacheMiss();

            HttpUserAgentParserMemoryCacheTelemetry.CacheSizeIncrement();
            entry.RegisterPostEvictionCallback(static (_, _, _, _) => HttpUserAgentParserMemoryCacheTelemetry.CacheSizeDecrement());

            return HttpUserAgentParser.Parse(key.UserAgent);
        });
    }

    [ExcludeFromCodeCoverage]
    private HttpUserAgentInformation ParseWithoutTelemetry(CacheKey key)
    {
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
