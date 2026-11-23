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
    // Shared across packages. Listener callbacks may synchronously register another package
    // or delegate that work to a task and wait for it.
    internal sealed class Publication : IDisposable
    {
        private static readonly AsyncLocal<Publication?> s_current = new();
        private readonly Publication? _parent = s_current.Value;
        private volatile bool _finished;

        public Publication() => s_current.Value = this;

        public static bool InProgress
        {
            get
            {
                for (Publication? scope = s_current.Value; scope is not null; scope = scope._parent)
                {
                    if (!scope._finished)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        public void Dispose()
        {
            // ExecutionContext may have been captured by a listener for work that runs later.
            _finished = true;
            s_current.Value = _parent;
        }
    }

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
    /// threw exists on the meter even when its factory did not return it.
    /// </remarks>
    public abstract void CreateMissingInstruments();

    protected void CreateInstrument<T>(ref T? field, Func<string, string, string, T> create, string name, string unit, string description)
        where T : Instrument
    {
        try
        {
            field ??= create(name, unit, description);
        }
        catch
        {
            // Publication precedes insertion in Meter's counter cache. Recover the published
            // instance so retrying after a listener exception cannot create a second instrument.
            T? published = null;
            using MeterListener inventory = new()
            {
                InstrumentPublished = (instrument, _) =>
                {
                    if (instrument is T candidate && ReferenceEquals(instrument.Meter, Meter)
                        && (instrument.Name, instrument.Unit, instrument.Description).Equals((name, unit, description)))
                    {
                        published = candidate;
                    }
                },
            };
            inventory.Start();
            field = published;
            throw;
        }
    }
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
/// <param name="markEnabled">Enables recording after the complete set of instruments is published.</param>
internal sealed class HttpUserAgentParserMeterRegistration<TInstruments>(
    Func<Meter, TInstruments> createInstruments, Action<Meter> reserve, Action resetReservations, Action markEnabled)
    where TInstruments : HttpUserAgentParserMeterInstruments
{
    // Monitor.Wait releases this lock while an independent registration finishes.
    private readonly object _lock = new();
    private readonly HashSet<Meter> _publishingMeters = new(ReferenceEqualityComparer.Instance);
    private readonly HashSet<string> _publishingNames = new(StringComparer.Ordinal);

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
        Meter target;
        lock (_lock)
        {
            while (meter is null ? _publishingNames.Contains(ownedMeterName) : _publishingMeters.Contains(meter))
            {
                if (HttpUserAgentParserMeterInstruments.Publication.InProgress)
                {
                    return; // the in-flight registration will finish publishing these instruments
                }

                Monitor.Wait(_lock);
            }

            if (own ? string.Equals(_ownedMeter?.Name, ownedMeterName, StringComparison.Ordinal) : ReferenceEquals(meter, _current?.Meter))
            {
                return;
            }

            target = meter ?? new Meter(ownedMeterName);
            if (own)
            {
                _publishingNames.Add(ownedMeterName);
            }

            _publishingMeters.Add(target);
        }

        using HttpUserAgentParserMeterInstruments.Publication publication = new();
        Meter? replacedOwnedMeter;
        try
        {
            reserve(target);
            TInstruments instruments = (TInstruments)_instrumentsByMeter.GetValue(target, _createInstruments);
            instruments.CreateMissingInstruments();

            lock (_lock)
            {
                Volatile.Write(ref _current, instruments);
                replacedOwnedMeter = _ownedMeter;
                _ownedMeter = own ? target : null;
                markEnabled();
            }
        }
        catch when (own)
        {
            target.Dispose();
            throw;
        }
        finally
        {
            lock (_lock)
            {
                _publishingMeters.Remove(target);
                if (own)
                {
                    _publishingNames.Remove(ownedMeterName);
                }

                Monitor.PulseAll(_lock);
            }
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
