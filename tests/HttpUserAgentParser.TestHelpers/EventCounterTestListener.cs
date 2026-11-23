// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Tracing;

namespace MyCSharp.HttpUserAgentParser.TestHelpers;

/// <summary>Records telemetry from the selected event source during a test.</summary>
public sealed class EventCounterTestListener(string eventSourceName) : EventListener
{
    private readonly string _eventSourceName = eventSourceName;
    private volatile bool _sawEventCounters;
    private volatile bool _enabled;

    /// <inheritdoc/>
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

        _enabled = true;
    }

    /// <inheritdoc/>
    protected override void OnEventWritten(EventWrittenEventArgs eventData)
    {
        if (string.Equals(eventData.EventName, "EventCounters", StringComparison.Ordinal))
        {
            _sawEventCounters = true;
        }
    }

    /// <summary>Waits for the first counter event.</summary>
    public bool WaitForCounters(TimeSpan timeout) => SpinWait.SpinUntil(() => _sawEventCounters, timeout);

    /// <summary>Waits for the listener to enable the event source.</summary>
    public bool WaitUntilEnabled(TimeSpan timeout) => SpinWait.SpinUntil(() => _enabled, timeout);
}
