// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.Telemetry;

/// <summary>
/// EventSource for EventCounters emitted by MyCSharp.HttpUserAgentParser.AspNetCore.
/// </summary>
/// <remarks>
/// Provides EventCounter-based telemetry for User-Agent presence detection.
/// Counters are incremented only when the EventSource is enabled to minimize
/// overhead on hot paths.
/// </remarks>
[EventSource(Name = EventSourceName)]
[ExcludeFromCodeCoverage]
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
    /// Initializes the EventCounters used by this EventSource.
    /// </summary>
    private HttpUserAgentParserAspNetCoreEventSource()
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
    /// Increments the EventCounter for requests with a present User-Agent header.
    /// </summary>
    [NonEvent]
    internal void UserAgentPresent()
    {
        if (!IsEnabled())
        {
            return;
        }

        _userAgentPresent?.Increment();
    }

    /// <summary>
    /// Increments the EventCounter for requests with a missing User-Agent header.
    /// </summary>
    [NonEvent]
    internal void UserAgentMissing()
    {
        if (!IsEnabled())
        {
            return;
        }

        _userAgentMissing?.Increment();
    }

    /// <summary>
    /// Releases all EventCounter resources used by this EventSource.
    /// </summary>
    /// <param name="disposing">
    /// <see langword="true"/> when called from Dispose;
    /// <see langword="false"/> when called from a finalizer.
    /// </param>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            _userAgentPresent?.Dispose();
            _userAgentMissing?.Dispose();
        }

        base.Dispose(disposing);
    }
}
