// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;

namespace MyCSharp.HttpUserAgentParser.Telemetry;

/// <summary>
/// EventSource for EventCounters emitted by MyCSharp.HttpUserAgentParser.
/// </summary>
/// <remarks>
/// The implementation is designed to keep overhead negligible unless a listener
/// is enabled. All counters are updated using lightweight, non-blocking operations
/// suitable for hot paths.
/// </remarks>
[EventSource(Name = EventSourceName)]
[ExcludeFromCodeCoverage]
public sealed class HttpUserAgentParserEventSource : EventSource
{
    /// <summary>
    /// The EventSource name used for EventCounters.
    /// </summary>
    public const string EventSourceName = "MyCSharp.HttpUserAgentParser";

    /// <summary>
    /// Singleton instance of the EventSource.
    /// </summary>
    internal static HttpUserAgentParserEventSource Log { get; } = new();

    private readonly IncrementingEventCounter _parseRequests;
    private readonly EventCounter? _parseDurationSeconds;

    private readonly IncrementingEventCounter _cacheHit;
    private readonly IncrementingEventCounter _cacheMiss;
    private readonly PollingCounter _cacheSize;

    /// <summary>
    /// Initializes all EventCounters and polling counters used by this EventSource.
    /// </summary>
    private HttpUserAgentParserEventSource()
    {
        // Parser
        _parseRequests = new IncrementingEventCounter("parse-requests", this)
        {
            DisplayName = "User-Agent parse requests",
            DisplayUnits = "calls",
        };

        _parseDurationSeconds = new EventCounter("parse-duration", this)
        {
            DisplayName = "Parse duration",
            DisplayUnits = "s",
        };

        // Providers (cache)
        _cacheHit = new IncrementingEventCounter("cache-hit", this)
        {
            DisplayName = "Cache hit",
            DisplayUnits = "calls",
        };

        _cacheMiss = new IncrementingEventCounter("cache-miss", this)
        {
            DisplayName = "Cache miss",
            DisplayUnits = "calls",
        };

        _cacheSize = new PollingCounter(
            "cache-size",
            this,
            static () => HttpUserAgentParserTelemetryState.ConcurrentCacheSize)
        {
            DisplayName = "Cache size",
            DisplayUnits = "entries",
        };
    }

    /// <summary>
    /// Records a User-Agent parse request.
    /// </summary>
    [NonEvent]
    internal void ParseRequest()
    {
        if (!IsEnabled())
        {
            return;
        }

        _parseRequests?.Increment();
    }

    /// <summary>
    /// Records the duration of a User-Agent parse operation.
    /// </summary>
    /// <param name="seconds">Elapsed parse time in seconds.</param>
    [NonEvent]
    internal void ParseDuration(double seconds)
    {
        if (!IsEnabled())
        {
            return;
        }

        _parseDurationSeconds?.WriteMetric(seconds);
    }

    /// <summary>
    /// Records a cache hit.
    /// </summary>
    [NonEvent]
    internal void CacheHit()
    {
        if (!IsEnabled())
        {
            return;
        }

        _cacheHit?.Increment();
    }

    /// <summary>
    /// Records a cache miss.
    /// </summary>
    [NonEvent]
    internal void CacheMiss()
    {
        if (!IsEnabled())
        {
            return;
        }

        _cacheMiss?.Increment();
    }

    /// <summary>
    /// Updates the size used by the polling counter.
    /// </summary>
    /// <param name="size">Current number of entries in the cache.</param>
    /// <remarks>
    /// The size is updated even when telemetry is disabled so that the polling
    /// counter reports a correct value once a listener attaches.
    /// </remarks>
    [NonEvent]
    internal static void CacheSizeSet(int size) => HttpUserAgentParserTelemetryState.SetConcurrentCacheSize(size);

    /// <summary>
    /// Releases all EventCounter and PollingCounter resources used by this EventSource.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> when called from <see cref="Dispose(bool)"/>;
    /// <see langword="false"/> when called from a finalizer.
    /// </param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _parseRequests?.Dispose();
            _parseDurationSeconds?.Dispose();

            _cacheHit?.Dispose();
            _cacheMiss?.Dispose();
            _cacheSize?.Dispose();
        }

        base.Dispose(disposing);
    }
}
