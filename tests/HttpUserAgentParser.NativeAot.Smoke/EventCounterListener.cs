// Copyright © https://myCSharp.de - all rights reserved

using System.Collections.Concurrent;
using System.Diagnostics.Tracing;
using MyCSharp.HttpUserAgentParser.AspNetCore.Telemetry;
using MyCSharp.HttpUserAgentParser.MemoryCache.Telemetry;
using MyCSharp.HttpUserAgentParser.Telemetry;

namespace MyCSharp.HttpUserAgentParser.NativeAot.Smoke;

/// <summary>
/// Enables the EventSources of all packages and records which of them published EventCounters.
/// </summary>
internal sealed class EventCounterListener : EventListener
{
    private static readonly string[] s_eventSourceNames =
    [
        HttpUserAgentParserEventSource.EventSourceName,
        HttpUserAgentParserMemoryCacheEventSource.EventSourceName,
        HttpUserAgentParserAspNetCoreEventSource.EventSourceName,
    ];

    private readonly ConcurrentDictionary<string, bool> _sourcesWithCounters = new(StringComparer.Ordinal);

    public bool WaitForCounters(TimeSpan timeout)
        => SpinWait.SpinUntil(() => _sourcesWithCounters.Count == s_eventSourceNames.Length, timeout);

    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (Array.Exists(s_eventSourceNames, name => string.Equals(name, eventSource.Name, StringComparison.Ordinal)))
        {
            EnableEvents(eventSource, EventLevel.LogAlways, EventKeywords.All, new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["EventCounterIntervalSec"] = "0.1",
            });
        }
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        if (string.Equals(eventData.EventName, "EventCounters", StringComparison.Ordinal) && eventData.EventSource.Name is { } name)
        {
            _sourcesWithCounters[name] = true;
        }
    }
}
