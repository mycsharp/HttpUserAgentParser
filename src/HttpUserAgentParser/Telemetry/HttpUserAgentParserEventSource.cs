// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;

namespace MyCSharp.HttpUserAgentParser.Telemetry;

/// <summary>
/// EventSource for EventCounters emitted by MyCSharp.HttpUserAgentParser.
///
/// The implementation is designed to keep overhead negligible unless a listener is enabled.
/// </summary>
[EventSource(Name = EventSourceName)]
[ExcludeFromCodeCoverage]
public sealed class HttpUserAgentParserEventSource : EventSource
{
    /// <summary>
    /// The EventSource name used for EventCounters.
    /// </summary>
    public const string EventSourceName = "MyCSharp.HttpUserAgentParser";

    internal static HttpUserAgentParserEventSource Log { get; } = new();

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
    internal void ParseRequest()
    {
        if (!IsEnabled()) return;
        _parseRequests?.Increment();
    }

    [NonEvent]
    internal void ParseDuration(double milliseconds)
    {
        if (!IsEnabled()) return;
        _parseDurationMs?.WriteMetric(milliseconds);
    }

    [NonEvent]
    internal void ConcurrentCacheHit()
    {
        if (!IsEnabled()) return;
        _concurrentCacheHit?.Increment();
    }

    [NonEvent]
    internal void ConcurrentCacheMiss()
    {
        if (!IsEnabled()) return;
        _concurrentCacheMiss?.Increment();
    }

    [NonEvent]
    internal void ConcurrentCacheSizeSet(int size)
    {
        // Size should be updated even if telemetry is currently disabled, so the polling counter is correct
        // once a listener attaches.
        Volatile.Write(ref s_concurrentCacheSize, size);
    }

    /// <inheritdoc />
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
