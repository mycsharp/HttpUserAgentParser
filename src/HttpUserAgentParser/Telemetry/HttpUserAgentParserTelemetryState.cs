// Copyright © https://myCSharp.de - all rights reserved

using System.Runtime.CompilerServices;
using MyCSharp.HttpUserAgentParser.Providers;

namespace MyCSharp.HttpUserAgentParser.Telemetry;

/// <summary>
/// Tracks providers from construction so the reported cache size includes entries added before telemetry was enabled.
/// Weak references let providers be collected between observations.
/// </summary>
internal static class HttpUserAgentParserTelemetryState
{
    private static readonly ConditionalWeakTable<HttpUserAgentParserCachedProvider, object?> s_providers = new();

    public static void Register(HttpUserAgentParserCachedProvider provider) => s_providers.Add(provider, value: null);

    /// <summary>
    /// Gets the number of entries of all live concurrent dictionary caches.
    /// </summary>
    /// <remarks>
    /// Counting locks each dictionary briefly; it only runs when an instrument is observed, never on the parse path.
    /// </remarks>
    public static long ConcurrentCacheSize
    {
        get
        {
            long size = 0;
            foreach (KeyValuePair<HttpUserAgentParserCachedProvider, object?> provider in (IEnumerable<KeyValuePair<HttpUserAgentParserCachedProvider, object?>>)s_providers)
            {
                size += provider.Key.CacheEntryCount;
            }

            return size;
        }
    }
}
