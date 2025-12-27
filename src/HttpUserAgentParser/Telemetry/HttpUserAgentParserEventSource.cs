// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;

namespace MyCSharp.HttpUserAgentParser.Telemetry;

/// <summary>
/// EventSource for EventCounters emitted by MyCSharp.HttpUserAgentParser.
///
/// The implementation is designed to keep overhead negligible unless a listener is enabled.
/// </summary>
[EventSource(Name = "MyCSharp.HttpUserAgentParser")]
[ExcludeFromCodeCoverage]
internal sealed class HttpUserAgentParserEventSource : EventSource
{
    public static readonly HttpUserAgentParserEventSource Log = new();

    private readonly IncrementingEventCounter _parseRequests;
    private readonly EventCounter? _parseDurationMs;

    private readonly IncrementingEventCounter _concurrentCacheHit;
    private readonly IncrementingEventCounter _concurrentCacheMiss;
    private readonly PollingCounter _concurrentCacheSize;

    // Backing values for PollingCounter and for easy verification.
    private static long s_concurrentCacheSize;

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

        _concurrentCacheSize = new PollingCounter("cache-concurrentdictionary-size", this, static () => Volatile.Read(ref s_concurrentCacheSize))
        {
            DisplayName = "ConcurrentDictionary cache size",
            DisplayUnits = "entries",
        };
    }

    [NonEvent]
    public bool IsTelemetryEnabled() => IsEnabled();

    [NonEvent]
    public void ParseRequest()
    {
        if (!IsEnabled()) return;
        _parseRequests?.Increment();
    }

    [NonEvent]
    public void ParseDuration(double milliseconds)
    {
        if (!IsEnabled()) return;
        _parseDurationMs?.WriteMetric(milliseconds);
    }

    [NonEvent]
    public void ConcurrentCacheHit()
    {
        if (!IsEnabled()) return;
        _concurrentCacheHit?.Increment();
    }

    [NonEvent]
    public void ConcurrentCacheMiss()
    {
        if (!IsEnabled()) return;
        _concurrentCacheMiss?.Increment();
    }

    [NonEvent]
    public void ConcurrentCacheSizeSet(int size)
    {
        // Size should be updated even if telemetry is currently disabled, so the polling counter is correct
        // once a listener attaches.
        Volatile.Write(ref s_concurrentCacheSize, size);
    }

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
