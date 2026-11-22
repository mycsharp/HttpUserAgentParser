// Copyright © https://myCSharp.de - all rights reserved

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
    private readonly EventCounter _parseDurationSeconds;

    private readonly IncrementingEventCounter _cacheHit;
    private readonly IncrementingEventCounter _cacheMiss;
    private readonly PollingCounter _cacheSize;

    /// <summary>
    /// Initializes all EventCounters and polling counters used by this EventSource.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Log"/>; the constructor is only accessible for tests.
    /// </remarks>
    internal HttpUserAgentParserEventSource()
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
        if (IsEnabled())
        {
            _parseRequests.Increment();
        }
    }

    /// <summary>
    /// Records the duration of a User-Agent parse operation.
    /// </summary>
    /// <param name="seconds">Elapsed parse time in seconds.</param>
    [NonEvent]
    internal void ParseDuration(double seconds)
    {
        if (IsEnabled())
        {
            _parseDurationSeconds.WriteMetric(seconds);
        }
    }

    /// <summary>
    /// Records a cache hit.
    /// </summary>
    [NonEvent]
    internal void CacheHit()
    {
        if (IsEnabled())
        {
            _cacheHit.Increment();
        }
    }

    /// <summary>
    /// Records a cache miss.
    /// </summary>
    [NonEvent]
    internal void CacheMiss()
    {
        if (IsEnabled())
        {
            _cacheMiss.Increment();
        }
    }

    /// <summary>
    /// Releases all EventCounter and PollingCounter resources used by this EventSource.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> when called from <see cref="EventSource.Dispose()"/>;
    /// <see langword="false"/> when called from a finalizer.
    /// </param>
    /// <remarks>
    /// Follows the pattern of the EventCounters documentation: the counters are released in both cases.
    /// </remarks>
    protected override void Dispose(bool disposing)
    {
        _parseRequests.Dispose();
        _parseDurationSeconds.Dispose();
        _cacheHit.Dispose();
        _cacheMiss.Dispose();
        _cacheSize.Dispose();

        base.Dispose(disposing);
    }
}
