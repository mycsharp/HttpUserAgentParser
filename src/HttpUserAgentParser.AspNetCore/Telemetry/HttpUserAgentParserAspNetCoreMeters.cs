// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using MyCSharp.HttpUserAgentParser.Telemetry;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.Telemetry;

/// <summary>
/// System.Diagnostics.Metrics instruments emitted by MyCSharp.HttpUserAgentParser.AspNetCore.
/// </summary>
/// <remarks>
/// Provides meter-based telemetry for User-Agent parsing and presence detection.
/// Instrument creation is performed once and guarded by a lock-free initialization
/// check to minimize overhead on hot paths.
/// </remarks>
[ExcludeFromCodeCoverage]
internal static class HttpUserAgentParserAspNetCoreMeters
{
    /// <summary>
    /// Name of the meter used to publish AspNetCore User-Agent metrics.
    /// </summary>
    private const string MeterNameSuffix = "http_user_agent_parser.aspnetcore";

    /// <summary>
    /// Name of the meter used to publish AspNetCore User-Agent metrics.
    /// </summary>
    public const string MeterName = "mycsharp." + MeterNameSuffix;

    /// <summary>
    /// Builds a meter name from a custom prefix.
    /// </summary>
    /// <param name="meterPrefix">
    /// The prefix to use. When null, the default prefix is used. When empty,
    /// no prefix is applied. Otherwise, the prefix must be alphanumeric and end with '.'.
    /// </param>
    /// <returns>The full meter name.</returns>
    /// <exception cref="ArgumentException">Thrown when the prefix is not empty and does not match the required format.</exception>
    public static string GetMeterName(string? meterPrefix)
        => HttpUserAgentParserMeterNameHelper.GetMeterName(meterPrefix, MeterNameSuffix);

    /// <summary>
    /// Indicates whether the meter and its instruments have been initialized.
    /// </summary>
    private static int s_initialized;

    private static Meter? s_meter;
    private static Counter<long>? s_userAgentPresent;
    private static Counter<long>? s_userAgentMissing;

    /// <summary>
    /// Gets a value indicating whether meter-based telemetry is enabled.
    /// </summary>
    /// <remarks>
    /// Returns <see langword="true"/> once the meter and counters have been initialized.
    /// </remarks>
    public static bool IsEnabled
        => Volatile.Read(ref s_initialized) != 0;

    /// <summary>
    /// Enables meter-based telemetry and initializes all metric instruments.
    /// </summary>
    /// <param name="meter">
    /// Optional externally managed <see cref="Meter"/> instance. If not provided,
    /// a new meter is created using <see cref="MeterName"/>.
    /// </param>
    /// <remarks>
    /// Initialization is performed at most once. Subsequent calls are ignored.
    /// </remarks>
    public static void Enable(Meter? meter = null)
    {
        if (Interlocked.Exchange(ref s_initialized, 1) == 1)
        {
            return;
        }

        s_meter = meter ?? new Meter(MeterName);

        s_userAgentPresent = s_meter.CreateCounter<long>(
            name: "user_agent.present",
            unit: "{call}",
            description: "User-Agent header present");

        s_userAgentMissing = s_meter.CreateCounter<long>(
            name: "user_agent.missing",
            unit: "{call}",
            description: "User-Agent header missing");
    }

    /// <summary>
    /// Records a metric indicating that a User-Agent header was present.
    /// </summary>
    /// <remarks>
    /// This method is optimized for hot paths and performs no work
    /// if the counter has not been initialized.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UserAgentPresent()
        => s_userAgentPresent?.Add(1);

    /// <summary>
    /// Records a metric indicating that a User-Agent header was missing.
    /// </summary>
    /// <remarks>
    /// This method is optimized for hot paths and performs no work
    /// if the counter has not been initialized.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UserAgentMissing()
        => s_userAgentMissing?.Add(1);

    /// <summary>
    /// Resets static state to support isolated unit tests.
    /// </summary>
    public static void ResetForTests()
    {
        Volatile.Write(ref s_initialized, 0);

        s_meter = null;
        s_userAgentPresent = null;
        s_userAgentMissing = null;
    }
}
