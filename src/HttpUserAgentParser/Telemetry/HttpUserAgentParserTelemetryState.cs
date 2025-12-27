// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.CodeAnalysis;

namespace MyCSharp.HttpUserAgentParser.Telemetry;

/// <summary>
/// Holds shared telemetry state for the concurrent dictionary cache.
/// </summary>
/// <remarks>
/// The state is updated independently of whether telemetry is currently enabled
/// so that polling-based instruments can report correct values once a listener
/// attaches.
/// </remarks>
[ExcludeFromCodeCoverage]
internal static class HttpUserAgentParserTelemetryState
{
    private static long s_concurrentCacheSize;

    /// <summary>
    /// Gets the current size of the concurrent dictionary cache.
    /// </summary>
    public static long ConcurrentCacheSize
        => Volatile.Read(ref s_concurrentCacheSize);

    /// <summary>
    /// Updates the current size of the concurrent dictionary cache.
    /// </summary>
    /// <param name="size">Current number of entries in the cache.</param>
    public static void SetConcurrentCacheSize(int size)
        => Volatile.Write(ref s_concurrentCacheSize, size);

    /// <summary>
    /// Resets the telemetry state for unit tests.
    /// </summary>
    public static void ResetForTests()
        => Volatile.Write(ref s_concurrentCacheSize, 0);
}
