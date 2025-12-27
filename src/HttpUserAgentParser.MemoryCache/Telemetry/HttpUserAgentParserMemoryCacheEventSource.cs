// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.Telemetry;

/// <summary>
/// EventSource for EventCounters emitted by MyCSharp.HttpUserAgentParser.MemoryCache.
/// </summary>
[EventSource(Name = "MyCSharp.HttpUserAgentParser.MemoryCache")]
[ExcludeFromCodeCoverage]
internal sealed class HttpUserAgentParserMemoryCacheEventSource : EventSource
{
    public static readonly HttpUserAgentParserMemoryCacheEventSource Log = new();

    private readonly IncrementingEventCounter? _cacheHit;
    private readonly IncrementingEventCounter _cacheMiss;
    private readonly PollingCounter _cacheSize;

    private static long s_cacheSize;

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

        _cacheSize = new PollingCounter("cache-size", this, static () => Volatile.Read(ref s_cacheSize))
        {
            DisplayName = "MemoryCache cache size",
            DisplayUnits = "entries",
        };
    }

    [NonEvent]
    public void CacheHit()
    {
        if (!IsEnabled()) return;
        _cacheHit?.Increment();
    }

    [NonEvent]
    public void CacheMiss()
    {
        if (!IsEnabled()) return;
        _cacheMiss?.Increment();
    }

    [NonEvent]
    public void CacheSizeIncrement() => Interlocked.Increment(ref s_cacheSize);

    [NonEvent]
    public void CacheSizeDecrement() => Interlocked.Decrement(ref s_cacheSize);

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
