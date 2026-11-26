// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Tracing;

namespace MyCSharp.HttpUserAgentParser.Telemetry;

/// <summary>
/// EventSource for EventCounters emitted by MyCSharp.HttpUserAgentParser.
/// </summary>
[EventSource(Name = EventSourceName)]
public sealed class HttpUserAgentParserEventSource : EventSource
{
    /// <summary>
    /// The EventSource name used for EventCounters.
    /// </summary>
    public const string EventSourceName = "MyCSharp.HttpUserAgentParser";

    internal static HttpUserAgentParserEventSource Log { get; } = new();

    private readonly IncrementingEventCounter _parseRequests;
    private readonly EventCounter _parseDurationSeconds;

    private readonly IncrementingEventCounter _cacheHit;
    private readonly IncrementingEventCounter _cacheMiss;
    private readonly PollingCounter _cacheSize;

    // Tests construct and dispose their own event source.
    internal HttpUserAgentParserEventSource()
    {
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

    [NonEvent]
    internal void ParseRequest()
    {
        if (IsEnabled())
        {
            _parseRequests.Increment();
        }
    }

    [NonEvent]
    internal void ParseDuration(double seconds)
    {
        if (IsEnabled())
        {
            _parseDurationSeconds.WriteMetric(seconds);
        }
    }

    [NonEvent]
    internal void CacheHit()
    {
        if (IsEnabled())
        {
            _cacheHit.Increment();
        }
    }

    [NonEvent]
    internal void CacheMiss()
    {
        if (IsEnabled())
        {
            _cacheMiss.Increment();
        }
    }

    /// <inheritdoc />
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
