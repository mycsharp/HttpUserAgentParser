// Copyright © https://myCSharp.de - all rights reserved

using System.Runtime.CompilerServices;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.Telemetry;

/// <summary>
/// Knows the live <see cref="HttpUserAgentParserMemoryCachedProvider"/> instances to report their cache size.
/// </summary>
/// <remarks>
/// Every provider registers itself when it is created, independent of whether telemetry is enabled, so the
/// reported size includes entries cached before telemetry was enabled. Providers are referenced weakly.
/// </remarks>
internal static class HttpUserAgentParserMemoryCacheTelemetryState
{
    private static readonly ConditionalWeakTable<HttpUserAgentParserMemoryCachedProvider, object?> s_providers = new();

    /// <summary>
    /// Registers a provider whose entries count towards <see cref="CacheSize"/>.
    /// </summary>
    public static void Register(HttpUserAgentParserMemoryCachedProvider provider) => s_providers.Add(provider, null);

    /// <summary>
    /// Removes a disposed provider, whose entries no longer count.
    /// </summary>
    public static void Unregister(HttpUserAgentParserMemoryCachedProvider provider) => s_providers.Remove(provider);

    /// <summary>
    /// Gets whether the entries of <paramref name="provider"/> count towards <see cref="CacheSize"/>.
    /// </summary>
    public static bool IsRegistered(HttpUserAgentParserMemoryCachedProvider provider) => s_providers.TryGetValue(provider, out _);

    /// <summary>
    /// Gets the number of entries of all live memory caches.
    /// </summary>
    /// <remarks>
    /// Counting only runs when an instrument is observed, never on the parse path.
    /// </remarks>
    public static long CacheSize
    {
        get
        {
            long size = 0;
            foreach (KeyValuePair<HttpUserAgentParserMemoryCachedProvider, object?> provider in (IEnumerable<KeyValuePair<HttpUserAgentParserMemoryCachedProvider, object?>>)s_providers)
            {
                size += provider.Key.CacheEntryCount;
            }

            return size;
        }
    }
}
