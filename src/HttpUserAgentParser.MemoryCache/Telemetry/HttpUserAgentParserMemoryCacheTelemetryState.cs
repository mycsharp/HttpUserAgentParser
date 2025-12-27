// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.CodeAnalysis;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.Telemetry;

/// <summary>
/// Holds telemetry state for tracking the size of the HTTP User-Agent parser memory cache.
/// </summary>
/// <remarks>
/// This class is excluded from code coverage as it contains only simple
/// thread-safe state management logic.
/// </remarks>
[ExcludeFromCodeCoverage]
internal static class HttpUserAgentParserMemoryCacheTelemetryState
{
    private static long s_cacheSize;

    /// <summary>
    /// Gets the current number of entries in the memory cache.
    /// </summary>
    /// <remarks>
    /// The value is read atomically to ensure thread safety.
    /// </remarks>
    public static long CacheSize => Volatile.Read(ref s_cacheSize);

    /// <summary>
    /// Increments the cached entry counter by one.
    /// </summary>
    /// <remarks>
    /// Uses an atomic operation to remain safe in concurrent scenarios.
    /// </remarks>
    public static void CacheSizeIncrement() => Interlocked.Increment(ref s_cacheSize);

    /// <summary>
    /// Decrements the cached entry counter by one.
    /// </summary>
    /// <remarks>
    /// Uses an atomic operation to remain safe in concurrent scenarios.
    /// </remarks>
    public static void CacheSizeDecrement() => Interlocked.Decrement(ref s_cacheSize);
}
