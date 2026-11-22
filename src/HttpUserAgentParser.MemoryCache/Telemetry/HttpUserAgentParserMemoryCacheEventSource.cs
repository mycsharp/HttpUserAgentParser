// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Tracing;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.Telemetry;

/// <summary>
/// EventSource for EventCounters emitted by MyCSharp.HttpUserAgentParser.MemoryCache.
/// </summary>
[EventSource(Name = EventSourceName)]
public sealed class HttpUserAgentParserMemoryCacheEventSource : EventSource
{
    /// <summary>
    /// The EventSource name used for EventCounters.
    /// </summary>
    public const string EventSourceName = "MyCSharp.HttpUserAgentParser.MemoryCache";

    /// <summary>
    /// Singleton instance of the EventSource.
    /// </summary>
    internal static HttpUserAgentParserMemoryCacheEventSource Log { get; } = new();

    private readonly IncrementingEventCounter _cacheHit;
    private readonly IncrementingEventCounter _cacheMiss;
    private readonly PollingCounter _cacheSize;

    /// <summary>
    /// Initializes all EventCounters and polling counters used by this EventSource.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Log"/>; the constructor is only accessible for tests.
    /// </remarks>
    internal HttpUserAgentParserMemoryCacheEventSource()
    {
        _cacheHit = new IncrementingEventCounter("cache-hit", this)
        {
            DisplayName = "MemoryCache cache hit",
            DisplayUnits = "calls",
        };

        _cacheMiss = new IncrementingEventCounter("cache-miss", this)
        {
            DisplayName = "MemoryCache cache miss",
            DisplayUnits = "calls",
        };

        _cacheSize = new PollingCounter("cache-size", this, static () => HttpUserAgentParserMemoryCacheTelemetryState.CacheSize)
        {
            DisplayName = "MemoryCache cache size",
            DisplayUnits = "entries",
        };
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
        _cacheHit.Dispose();
        _cacheMiss.Dispose();
        _cacheSize.Dispose();

        base.Dispose(disposing);
    }
}
