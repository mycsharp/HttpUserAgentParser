// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.Telemetry;

/// <summary>
/// EventSource for EventCounters emitted by MyCSharp.HttpUserAgentParser.MemoryCache.
/// </summary>
[EventSource(Name = EventSourceName)]
[ExcludeFromCodeCoverage]
public sealed class HttpUserAgentParserMemoryCacheEventSource : EventSource
{
    /// <summary>
    /// The EventSource name used for EventCounters.
    /// </summary>
    public const string EventSourceName = "MyCSharp.HttpUserAgentParser.MemoryCache";

    internal static HttpUserAgentParserMemoryCacheEventSource Log { get; } = new();

    private readonly IncrementingEventCounter? _cacheHit;
    private readonly IncrementingEventCounter _cacheMiss;
    private readonly PollingCounter _cacheSize;

    private HttpUserAgentParserMemoryCacheEventSource()
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

    [NonEvent]
    internal void CacheHit()
    {
        if (!IsEnabled())
        {
            return;
        }

        _cacheHit?.Increment();
    }

    [NonEvent]
    internal void CacheMiss()
    {
        if (!IsEnabled())
        {
            return;
        }

        _cacheMiss?.Increment();
    }

    [NonEvent]
    internal static void CacheSizeIncrement() => HttpUserAgentParserMemoryCacheTelemetryState.CacheSizeIncrement();

    [NonEvent]
    internal static void CacheSizeDecrement() => HttpUserAgentParserMemoryCacheTelemetryState.CacheSizeDecrement();

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _cacheHit?.Dispose();
            _cacheMiss?.Dispose();
            _cacheSize?.Dispose();
        }

        base.Dispose(disposing);
    }
}
