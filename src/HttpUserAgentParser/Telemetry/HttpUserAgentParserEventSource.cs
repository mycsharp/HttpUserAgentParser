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
    private readonly EventCounter? _parseDurationMs;

    private readonly IncrementingEventCounter _concurrentCacheHit;
    private readonly IncrementingEventCounter _concurrentCacheMiss;
    private readonly PollingCounter _concurrentCacheSize;

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

        _parseDurationMs = new EventCounter("parse-duration", this)
        {
            DisplayName = "Parse duration",
            DisplayUnits = "ms",
        };

        // Providers (cache)
        _concurrentCacheHit = new IncrementingEventCounter("cache-concurrentdictionary-hit", this)
        {
            DisplayName = "ConcurrentDictionary cache hit",
            DisplayUnits = "calls",
        };

        _concurrentCacheMiss = new IncrementingEventCounter("cache-concurrentdictionary-miss", this)
        {
            DisplayName = "ConcurrentDictionary cache miss",
            DisplayUnits = "calls",
        };

        _concurrentCacheSize = new PollingCounter(
            "cache-concurrentdictionary-size",
            this,
            static () => HttpUserAgentParserTelemetryState.ConcurrentCacheSize)
        {
            DisplayName = "ConcurrentDictionary cache size",
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
    /// <param name="milliseconds">Elapsed parse time in milliseconds.</param>
    [NonEvent]
    internal void ParseDuration(double milliseconds)
    {
        if (!IsEnabled())
        {
            return;
        }

        _parseDurationMs?.WriteMetric(milliseconds);
    }

    /// <summary>
    /// Records a cache hit in the concurrent dictionary provider.
    /// </summary>
    [NonEvent]
    internal void ConcurrentCacheHit()
    {
        if (!IsEnabled())
        {
            return;
        }

        _concurrentCacheHit?.Increment();
    }

    /// <summary>
    /// Records a cache miss in the concurrent dictionary provider.
    /// </summary>
    [NonEvent]
    internal void ConcurrentCacheMiss()
    {
        if (!IsEnabled())
        {
            return;
        }

        _concurrentCacheMiss?.Increment();
    }

    /// <summary>
    /// Updates the concurrent cache size used by the polling counter.
    /// </summary>
    /// <param name="size">Current number of entries in the cache.</param>
    /// <remarks>
    /// The size is updated even when telemetry is disabled so that the polling
    /// counter reports a correct value once a listener attaches.
    /// </remarks>
    [NonEvent]
    internal void ConcurrentCacheSizeSet(int size)
    {
        HttpUserAgentParserTelemetryState.SetConcurrentCacheSize(size);
    }

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
            _parseDurationMs?.Dispose();

            _concurrentCacheHit?.Dispose();
            _concurrentCacheMiss?.Dispose();
            _concurrentCacheSize?.Dispose();
        }

        base.Dispose(disposing);
    }
}
