// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace MyCSharp.HttpUserAgentParser.Telemetry;

/// <summary>
/// The instruments of one package on one meter.
/// </summary>
/// <param name="meter">The meter the instruments belong to.</param>
internal abstract class HttpUserAgentParserMeterInstruments(Meter meter)
{
    /// <summary>
    /// Gets the meter the instruments belong to.
    /// </summary>
    public Meter Meter { get; } = meter;

    /// <summary>
    /// Creates the instruments that do not exist yet.
    /// </summary>
    /// <remarks>
    /// .NET runs the <see cref="MeterListener.InstrumentPublished"/> callbacks while an instrument is created, so listener code
    /// runs here and may throw or register meters itself; no lock is held. An instrument that was published before a listener
    /// threw exists on the meter, so a later call must not create it again: counters and histograms with the same name, unit and
    /// description are returned by the meter, observable instruments have to be created at most once by the implementation.
    /// </remarks>
    public abstract void CreateMissingInstruments();
}

/// <summary>
/// Registration of the meter a package records to (shared by all packages).
/// </summary>
/// <remarks>
/// <para>
/// The last registered meter is used. A meter that is registered again reuses its instruments. The meter a package creates itself
/// (the default meter or a meter with a prefixed name) is reused while it is in use and disposed when another meter replaces it;
/// a measurement that runs exactly during that switch may be lost. Meters of the caller are never disposed.
/// </para>
/// <para>
/// Listener code never runs while the lock is held: instruments are created and replaced meters are disposed outside of it, so
/// listeners may register meters of any package without deadlocks. The lock only protects the switch of the state.
/// </para>
/// </remarks>
/// <typeparam name="TInstruments">The instruments of the package.</typeparam>
/// <param name="createInstruments">Creates the (still empty) instruments of a meter; must not create instruments itself.</param>
/// <param name="reserve">Reserves a meter for the package before its instruments are created; throws if the meter must not be used.</param>
/// <param name="resetReservations">Forgets the reservations of the package (unit tests).</param>
internal sealed class HttpUserAgentParserMeterRegistration<TInstruments>(
    Func<Meter, TInstruments> createInstruments, Action<Meter> reserve, Action resetReservations)
    where TInstruments : HttpUserAgentParserMeterInstruments
{
#if NET9_0_OR_GREATER
    private readonly Lock _lock = new();
#else
    private readonly object _lock = new();
#endif

    // Keyed by the non-generic base type: the table's value type needs a trimming annotation a generic argument cannot provide.
    private readonly ConditionalWeakTable<Meter, HttpUserAgentParserMeterInstruments> _instrumentsByMeter = new();
    private readonly ConditionalWeakTable<Meter, HttpUserAgentParserMeterInstruments>.CreateValueCallback _createInstruments = createInstruments.Invoke;

    private Meter? _ownedMeter;
    private TInstruments? _current;

    /// <summary>
    /// Gets the instruments in use, or <see langword="null"/> before the first registration.
    /// </summary>
    public TInstruments? Current => Volatile.Read(ref _current);

    /// <summary>
    /// Records to <paramref name="meter"/> from now on or, if <see langword="null"/>, to a meter named
    /// <paramref name="ownedMeterName"/> that the package creates and owns.
    /// </summary>
    public void Use(Meter? meter, string ownedMeterName)
    {
        bool own = meter is null;
        lock (_lock)
        {
            // Already in use: nothing to do. This also keeps the own meter from being disposed if a caller passes it in.
            bool inUse = own
                ? _ownedMeter is not null && string.Equals(_ownedMeter.Name, ownedMeterName, StringComparison.Ordinal)
                : ReferenceEquals(meter, _ownedMeter);
            if (inUse)
            {
                return;
            }
        }

        // A new own meter is visible to nobody until it is published below.
        meter ??= new Meter(ownedMeterName);

        TInstruments instruments;
        try
        {
            reserve(meter);
            instruments = (TInstruments)_instrumentsByMeter.GetValue(meter, _createInstruments);
            instruments.CreateMissingInstruments();
        }
        catch when (own)
        {
            meter.Dispose();
            throw;
        }

        Meter? replacedOwnedMeter;
        lock (_lock)
        {
            // Published only after all instruments exist.
            Volatile.Write(ref _current, instruments);
            replacedOwnedMeter = _ownedMeter;
            _ownedMeter = own ? meter : null;
        }

        // Disposing notifies listeners, so it runs outside of the lock.
        replacedOwnedMeter?.Dispose();
    }

    /// <summary>
    /// Resets the registration (unit tests).
    /// </summary>
    public void ResetForTests()
    {
        Meter? ownedMeter;
        lock (_lock)
        {
            Volatile.Write(ref _current, null);
            ownedMeter = _ownedMeter;
            _ownedMeter = null;
            _instrumentsByMeter.Clear();
        }

        ownedMeter?.Dispose();
        resetReservations();
    }
}
