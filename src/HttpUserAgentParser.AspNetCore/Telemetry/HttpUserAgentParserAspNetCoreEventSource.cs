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

    /// <summary>
    /// Singleton instance of the EventSource.
    /// </summary>
    internal static HttpUserAgentParserAspNetCoreEventSource Log { get; } = new();

    private readonly IncrementingEventCounter _userAgentPresent;
    private readonly IncrementingEventCounter _userAgentMissing;

    /// <summary>
    /// Initializes all EventCounters used by this EventSource.
    /// </summary>
    /// <remarks>
    /// Use <see cref="Log"/>; the constructor is only accessible for tests.
    /// </remarks>
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

    /// <summary>
    /// Records a request with a User-Agent header.
    /// </summary>
    [NonEvent]
    internal void UserAgentPresent()
    {
        if (IsEnabled())
        {
            _userAgentPresent.Increment();
        }
    }

    /// <summary>
    /// Records a request without a User-Agent header.
    /// </summary>
    [NonEvent]
    internal void UserAgentMissing()
    {
        if (IsEnabled())
        {
            _userAgentMissing.Increment();
        }
    }

    /// <summary>
    /// Releases all EventCounter resources used by this EventSource.
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
        _userAgentPresent.Dispose();
        _userAgentMissing.Dispose();

        base.Dispose(disposing);
    }
}
