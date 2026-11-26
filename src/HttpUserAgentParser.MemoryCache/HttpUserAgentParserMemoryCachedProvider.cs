// Copyright © https://myCSharp.de - all rights reserved

using System.Runtime.CompilerServices;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Primitives;
using MyCSharp.HttpUserAgentParser.MemoryCache.Telemetry;
using MyCSharp.HttpUserAgentParser.Providers;

namespace MyCSharp.HttpUserAgentParser.MemoryCache;

/// <summary>
/// <see cref="IHttpUserAgentParserProvider"/> that caches the results in an <see cref="IMemoryCache"/>.
/// </summary>
/// <remarks>
/// The provider owns its <see cref="IMemoryCache"/>; disposing the provider disposes the cache.
/// </remarks>
public class HttpUserAgentParserMemoryCachedProvider : IHttpUserAgentParserProvider, IDisposable
{
    // MemoryCache checks passive expiration tokens on every lookup, also when a miss from an
    // older generation finishes after SetRobots. The cache key and callback value stay public types.
    internal sealed class RobotRulesChangeToken(object generation) : IChangeToken
    {
        public bool HasChanged => !ReferenceEquals(generation, HttpUserAgentRules.RobotSet.CacheGeneration);
        public bool ActiveChangeCallbacks => false;
        public IDisposable RegisterChangeCallback(Action<object?> callback, object? state)
            => CancellationToken.None.Register(callback, state);
    }

    private static readonly ConditionalWeakTable<HttpUserAgentRobotSet, RobotRulesChangeToken> s_changeTokens = new();

    /// <summary>
    /// The name of the Meter used for metrics.
    /// </summary>
    public const string MeterName = "mycsharp.http_user_agent_parser.memorycache";

    private readonly Microsoft.Extensions.Caching.Memory.MemoryCache _memoryCache;
    private readonly HttpUserAgentParserMemoryCachedProviderOptions _options;

    /// <summary>
    /// Creates a provider with its own <see cref="IMemoryCache"/>.
    /// </summary>
    /// <param name="options">The options used to set expiration and size limit</param>
    public HttpUserAgentParserMemoryCachedProvider(HttpUserAgentParserMemoryCachedProviderOptions options)
    {
        ArgumentNullException.ThrowIfNull(options);

        _memoryCache = new(options.CacheOptions);
        _options = options;

        // Known independent of telemetry, so the cache size also includes entries cached before telemetry was enabled.
        HttpUserAgentParserMemoryCacheTelemetryState.Register(this);
    }

    internal int CacheEntryCount => _memoryCache.Count;

    /// <inheritdoc/>
    /// <remarks>
    /// The cache uses the exact header as its key. Concurrent misses for the same header can cause multiple parses.
    /// </remarks>
    public HttpUserAgentInformation Parse(string userAgent)
    {
        if (_memoryCache.TryGetValue(userAgent, out HttpUserAgentInformation cached))
        {
            if (HttpUserAgentParserMemoryCacheTelemetry.IsEnabled)
            {
                HttpUserAgentParserMemoryCacheTelemetry.CacheHit();
            }

            return cached;
        }

        return ParseAndCache(userAgent);
    }

    /// <summary>
    /// Disposes the internal <see cref="IMemoryCache"/>; the provider must not be used afterwards.
    /// </summary>
    public void Dispose()
    {
        HttpUserAgentParserMemoryCacheTelemetryState.Unregister(this);
        _memoryCache.Dispose();
        GC.SuppressFinalize(this);
    }

    private HttpUserAgentInformation ParseAndCache(string userAgent)
    {
        HttpUserAgentRobotSet robots = HttpUserAgentRules.RobotSet;
        HttpUserAgentInformation information = HttpUserAgentParser.Parse(userAgent, robots);

        MemoryCacheEntryOptions entryOptions = _options.CacheEntryOptions;
        using (ICacheEntry entry = _memoryCache.CreateEntry(userAgent))
        {
            entry.SetOptions(entryOptions);
            entry.ExpirationTokens.Add(s_changeTokens.GetValue(robots, static rules => new RobotRulesChangeToken(rules.CacheGeneration)));
            entry.Size = entryOptions.Size ?? 1;
            entry.Value = information;
        }

        // Listeners can parse the same user agent. Publish the entry before notifying them.
        if (HttpUserAgentParserMemoryCacheTelemetry.IsEnabled)
        {
            HttpUserAgentParserMemoryCacheTelemetry.CacheMiss();
        }

        return information;
    }
}
