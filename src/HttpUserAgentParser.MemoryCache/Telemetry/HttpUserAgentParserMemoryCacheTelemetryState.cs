// Copyright © https://myCSharp.de - all rights reserved

using System.Runtime.CompilerServices;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.Telemetry;

/// <summary>
/// Tracks providers from construction so the reported cache size includes entries added before telemetry was enabled.
/// Weak references let providers be collected between observations.
/// </summary>
internal static class HttpUserAgentParserMemoryCacheTelemetryState
{
    private static readonly ConditionalWeakTable<HttpUserAgentParserMemoryCachedProvider, object?> s_providers = new();

    public static void Register(HttpUserAgentParserMemoryCachedProvider provider) => s_providers.Add(provider, value: null);

    public static void Unregister(HttpUserAgentParserMemoryCachedProvider provider) => s_providers.Remove(provider);

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
