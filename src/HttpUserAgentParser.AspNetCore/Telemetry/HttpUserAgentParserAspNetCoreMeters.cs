// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;
using MyCSharp.HttpUserAgentParser.Telemetry;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.Telemetry;

/// <summary>
/// System.Diagnostics.Metrics instruments emitted by MyCSharp.HttpUserAgentParser.AspNetCore.
/// </summary>
/// <remarks>
/// All instruments of a meter are created before they become visible, so concurrent registrations never observe
/// a partially initialized state. The last registered meter is used.
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

#if NET9_0_OR_GREATER
    private static readonly Lock s_lock = new();
#else
    private static readonly object s_lock = new();
#endif

    /// <summary>
    /// The meter this class created and uses (the default meter or a meter with a prefixed name), if any.
    /// </summary>
    private static Meter? s_ownedMeter;

    private static Instruments? s_instruments;

    /// <summary>
    /// The instruments of every registered meter; a meter registered again reuses its instruments.
    /// </summary>
    private static readonly ConditionalWeakTable<Meter, Instruments> s_instrumentsByMeter = new();

    /// <summary>
    /// Creates the metric instruments on <paramref name="meter"/> (or on a default meter) and uses them from now on.
    /// </summary>
    /// <param name="meter">
    /// Optional externally managed <see cref="Meter"/> instance. If not provided,
    /// a meter named <see cref="MeterName"/> is used.
    /// </param>
    /// <remarks>
    /// Another meter replaces the one in use. A meter that is registered again reuses its instruments.
    /// </remarks>
    public static void Enable(Meter? meter = null) => Use(meter, MeterName);

    /// <summary>
    /// Creates the metric instruments on a meter named <paramref name="meterName"/> that this class creates and owns.
    /// </summary>
    /// <remarks>
    /// The owned meter is reused for the same name; it is disposed when another meter replaces it.
    /// </remarks>
    public static void EnableOwnedMeter(string meterName) => Use(null, meterName);

    /// <summary>
    /// Uses <paramref name="meter"/> or, if null, an owned meter named <paramref name="ownedMeterName"/>.
    /// </summary>
    private static void Use(Meter? meter, string ownedMeterName)
    {
        lock (s_lock)
        {
            Meter? previousOwnedMeter = s_ownedMeter;
            bool own = meter is null;
            if (meter is null)
            {
                // Own meters: the current one is reused for the same name, another name gets a new meter.
                meter = previousOwnedMeter is not null && string.Equals(previousOwnedMeter.Name, ownedMeterName, StringComparison.Ordinal)
                    ? previousOwnedMeter
                    : new Meter(ownedMeterName);
            }

            Instruments instruments;
            try
            {
                // Instruments are created once per meter; a listener may throw while they are published.
                instruments = s_instrumentsByMeter.GetValue(meter, static m => new Instruments(m));
            }
            catch when (own && !ReferenceEquals(meter, previousOwnedMeter))
            {
                meter.Dispose();
                throw;
            }

            // Published only after all instruments exist.
            Volatile.Write(ref s_instruments, instruments);
            s_ownedMeter = own ? meter : null;

            // A replaced own meter is disposed, so changing names does not keep meters alive. A measurement that read its
            // instruments just before the switch may be lost (best effort); meters of the caller are never disposed.
            if (previousOwnedMeter is not null && !ReferenceEquals(previousOwnedMeter, meter))
            {
                previousOwnedMeter.Dispose();
            }
        }
    }

    /// <summary>
    /// Records a metric indicating that a User-Agent header was present.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UserAgentPresent()
        => Volatile.Read(ref s_instruments)?.UserAgentPresent.Add(1);

    /// <summary>
    /// Records a metric indicating that a User-Agent header was missing.
    /// </summary>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void UserAgentMissing()
        => Volatile.Read(ref s_instruments)?.UserAgentMissing.Add(1);

    /// <summary>
    /// Resets static state to support isolated unit tests.
    /// </summary>
    public static void ResetForTests()
    {
        lock (s_lock)
        {
            Volatile.Write(ref s_instruments, null);
            s_ownedMeter?.Dispose();
            s_ownedMeter = null;
            s_instrumentsByMeter.Clear();
        }
    }

    /// <summary>
    /// The instruments of one meter.
    /// </summary>
    private sealed class Instruments(Meter meter)
    {
        public Counter<long> UserAgentPresent { get; } = meter.CreateCounter<long>(
            name: "user_agent.present",
            unit: "{call}",
            description: "User-Agent header present");

        public Counter<long> UserAgentMissing { get; } = meter.CreateCounter<long>(
            name: "user_agent.missing",
            unit: "{call}",
            description: "User-Agent header missing");
    }
}
