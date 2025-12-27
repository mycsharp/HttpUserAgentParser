// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Tracing;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.Telemetry;

/// <summary>
/// EventSource for EventCounters emitted by MyCSharp.HttpUserAgentParser.AspNetCore.
/// </summary>
[EventSource(Name = "MyCSharp.HttpUserAgentParser.AspNetCore")]
[ExcludeFromCodeCoverage]
internal sealed class HttpUserAgentParserAspNetCoreEventSource : EventSource
{
    public static readonly HttpUserAgentParserAspNetCoreEventSource Log = new();

    private readonly IncrementingEventCounter _userAgentPresent;
    private readonly IncrementingEventCounter _userAgentMissing;

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

    [NonEvent]
    public void UserAgentPresent()
    {
        if (!IsEnabled()) return;
        _userAgentPresent?.Increment();
    }

    [NonEvent]
    public void UserAgentMissing()
    {
        if (!IsEnabled()) return;
        _userAgentMissing?.Increment();
    }

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
