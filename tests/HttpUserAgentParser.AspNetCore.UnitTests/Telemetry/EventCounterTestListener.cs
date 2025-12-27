// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Tracing;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.UnitTests.Telemetry;

internal sealed class EventCounterTestListener(string eventSourceName) : EventListener
{
    private readonly string _eventSourceName = eventSourceName;
    private volatile bool _sawEventCounters;

    protected override void OnEventSourceCreated(EventSource eventSource)
    {
        if (!string.Equals(eventSource.Name, _eventSourceName, StringComparison.Ordinal))
        {
            return;
        }

        EnableEvents(
            eventSource,
            EventLevel.LogAlways,
            EventKeywords.All,
            new Dictionary<string, string?>(StringComparer.Ordinal)
            {
                ["EventCounterIntervalSec"] = "0.1"
            });
    }

    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        if (string.Equals(eventData.EventName, "EventCounters", StringComparison.Ordinal))
        {
            _sawEventCounters = true;
        }
    }

    public bool WaitForCounters(TimeSpan timeout)
    {
        DateTimeOffset start = DateTimeOffset.UtcNow;
        while (!_sawEventCounters && DateTimeOffset.UtcNow - start < timeout)
        {
            Thread.Sleep(10);
        }

        return _sawEventCounters;
    }
}
