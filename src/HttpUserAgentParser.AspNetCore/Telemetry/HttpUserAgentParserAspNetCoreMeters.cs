// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using MyCSharp.HttpUserAgentParser.Telemetry;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.Telemetry;

/// <summary>
/// System.Diagnostics.Metrics instruments emitted by MyCSharp.HttpUserAgentParser.AspNetCore.
/// </summary>
/// <remarks>
/// See <see cref="HttpUserAgentParserMeterRegistration{TInstruments}"/> for the registration rules.
/// </remarks>
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
    /// no prefix is applied. Otherwise, the prefix must be ASCII letters or digits followed by '.'.
    /// </param>
    /// <returns>The full meter name.</returns>
    /// <exception cref="ArgumentException">Thrown when the prefix is not empty and does not match the required format.</exception>
    public static string GetMeterName(string? meterPrefix)
        => HttpUserAgentParserMeterNameHelper.GetMeterName(meterPrefix, MeterNameSuffix);

    private static readonly HttpUserAgentParserMeterRegistration<Instruments> s_registration = new(
        static meter => new Instruments(meter),
        static _ => { }, // no instrument in common with the other packages: a meter may be shared
        static () => { });

    /// <summary>
    /// Creates the metric instruments on <paramref name="meter"/> (or on a default meter) and uses them from now on.
    /// </summary>
    public static void Enable(Meter? meter = null) => s_registration.Use(meter, MeterName);

    /// <summary>
    /// Creates the metric instruments on a meter named <paramref name="meterName"/> that this class creates and owns.
    /// </summary>
    public static void EnableOwnedMeter(string meterName) => s_registration.Use(null, meterName);

    /// <summary>
    /// Records a metric indicating that a User-Agent header was present.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UserAgentPresent() => s_registration.Current?.UserAgentPresent.Add(1);

    /// <summary>
    /// Records a metric indicating that a User-Agent header was missing.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UserAgentMissing() => s_registration.Current?.UserAgentMissing.Add(1);

    /// <summary>
    /// Resets static state to support isolated unit tests.
    /// </summary>
    public static void ResetForTests() => s_registration.ResetForTests();

    /// <summary>
    /// The instruments of one meter.
    /// </summary>
    private sealed class Instruments(Meter meter) : HttpUserAgentParserMeterInstruments(meter)
    {
        private Counter<long>? _userAgentPresent;
        private Counter<long>? _userAgentMissing;

        public Counter<long> UserAgentPresent => _userAgentPresent!;

        public Counter<long> UserAgentMissing => _userAgentMissing!;

        public override void CreateMissingInstruments()
        {
            _userAgentPresent ??= Meter.CreateCounter<long>(name: "user_agent.present", unit: "{call}", description: "User-Agent header present");
            _userAgentMissing ??= Meter.CreateCounter<long>(name: "user_agent.missing", unit: "{call}", description: "User-Agent header missing");
        }
    }
}
