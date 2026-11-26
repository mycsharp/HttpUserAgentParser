// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Tracing;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.Telemetry;

/// <summary>
/// EventSource for EventCounters emitted by MyCSharp.HttpUserAgentParser.AspNetCore.
/// </summary>
[EventSource(Name = EventSourceName)]
public sealed class HttpUserAgentParserAspNetCoreEventSource : EventSource
{
    /// <summary>
    /// The EventSource name used for EventCounters.
    /// </summary>
    public const string EventSourceName = "MyCSharp.HttpUserAgentParser.AspNetCore";

    internal static HttpUserAgentParserAspNetCoreEventSource Log { get; } = new();

    private readonly IncrementingEventCounter _userAgentPresent;
    private readonly IncrementingEventCounter _userAgentMissing;

    // Tests construct and dispose their own event source.
    internal HttpUserAgentParserAspNetCoreEventSource()
    {
        _userAgentPresent = new IncrementingEventCounter("useragent-present", this)
        {
            DisplayName = "User-Agent header present",
            DisplayUnits = "calls",
        };

        _userAgentMissing = new IncrementingEventCounter("useragent-missing", this)
        {
            DisplayName = "User-Agent header missing",
            DisplayUnits = "calls",
        };
    }

    [NonEvent]
    internal void UserAgentPresent()
    {
        if (IsEnabled())
        {
            _userAgentPresent.Increment();
        }
    }

    [NonEvent]
    internal void UserAgentMissing()
    {
        if (IsEnabled())
        {
            _userAgentMissing.Increment();
        }
    }

    /// <inheritdoc />
    protected override void Dispose(bool disposing)
    {
        _userAgentPresent.Dispose();
        _userAgentMissing.Dispose();

        base.Dispose(disposing);
    }
}
