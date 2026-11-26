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
    private const string MeterNameSuffix = "http_user_agent_parser.aspnetcore";

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
        static () => { },
        HttpUserAgentParserAspNetCoreTelemetry.MarkMetersEnabled);

    /// <summary>
    /// Creates the metric instruments on <paramref name="meter"/> (or on a default meter) and uses them from now on.
    /// </summary>
    public static void Enable(Meter? meter = null) => s_registration.Use(meter, MeterName);

    /// <summary>
    /// Creates the metric instruments on a meter named <paramref name="meterName"/> that this class creates and owns.
    /// </summary>
    public static void EnableOwnedMeter(string meterName) => s_registration.Use(meter: null, meterName);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UserAgentPresent() => s_registration.Current?.UserAgentPresent.Add(1);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UserAgentMissing() => s_registration.Current?.UserAgentMissing.Add(1);

    public static void ResetForTests() => s_registration.ResetForTests();

    private sealed class Instruments(Meter meter) : HttpUserAgentParserMeterInstruments(meter)
    {
        private Counter<long>? _userAgentPresent;
        private Counter<long>? _userAgentMissing;

        public Counter<long> UserAgentPresent => _userAgentPresent!;

        public Counter<long> UserAgentMissing => _userAgentMissing!;

        public override void CreateMissingInstruments()
        {
            CreateInstrument(ref _userAgentPresent, Meter.CreateCounter<long>, "user_agent.present", "{call}", "User-Agent header present");
            CreateInstrument(ref _userAgentMissing, Meter.CreateCounter<long>, "user_agent.missing", "{call}", "User-Agent header missing");
        }
    }
}
