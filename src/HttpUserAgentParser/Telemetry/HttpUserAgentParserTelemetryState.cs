// Copyright © https://myCSharp.de - all rights reserved

using System.Runtime.CompilerServices;
using MyCSharp.HttpUserAgentParser.Providers;

namespace MyCSharp.HttpUserAgentParser.Telemetry;

/// <summary>
/// Knows the live <see cref="HttpUserAgentParserCachedProvider"/> instances to report their cache size.
/// </summary>
/// <remarks>
/// Every provider registers itself when it is created, independent of whether telemetry is enabled, so the
/// reported size includes entries cached before telemetry was enabled. Providers are referenced weakly.
/// </remarks>
internal static class HttpUserAgentParserTelemetryState
{
    private static readonly ConditionalWeakTable<HttpUserAgentParserCachedProvider, object?> s_providers = new();

    /// <summary>
    /// Registers a provider whose entries count towards <see cref="ConcurrentCacheSize"/>.
    /// </summary>
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
