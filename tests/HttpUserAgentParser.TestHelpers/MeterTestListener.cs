// Copyright © https://myCSharp.de - all rights reserved

using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

namespace MyCSharp.HttpUserAgentParser.TestHelpers;

/// <summary>Records telemetry from the selected meter during a test.</summary>
public sealed class MeterTestListener : IDisposable
{
    private readonly string _meterName;
    private readonly MeterListener _listener;

    /// <summary>Names of the instruments that emitted a measurement.</summary>
    public ConcurrentBag<string> InstrumentNames { get; } = [];
    /// <summary>Units of the published instruments.</summary>
    public ConcurrentDictionary<string, string?> InstrumentUnits { get; } = new(StringComparer.Ordinal);

    /// <summary>Starts listening to the named meter.</summary>
    public MeterTestListener(string meterName)
    {
        _meterName = meterName;
        _listener = new MeterListener();

        _listener.InstrumentPublished = (instrument, listener) =>
        {
            if (!string.Equals(instrument.Meter.Name, _meterName, StringComparison.Ordinal))
            {
                return;
            }

            InstrumentUnits[instrument.Name] = instrument.Unit;
            listener.EnableMeasurementEvents(instrument);
        };

        _listener.SetMeasurementEventCallback<long>((instrument, _, _, _) =>
        {
            InstrumentNames.Add(instrument.Name);
        });

        _listener.SetMeasurementEventCallback<double>((instrument, _, _, _) =>
        {
            InstrumentNames.Add(instrument.Name);
        });

        _listener.Start();
    }

    /// <summary>Counts active meters with the specified name and their instruments.</summary>
    public static (int Meters, int Instruments) CountLiveMeters(string meterName)
    {
        HashSet<Meter> meters = [];
        int instruments = 0;
        using MeterListener listener = new()
        {
            InstrumentPublished = (instrument, _) =>
            {
                if (string.Equals(instrument.Meter.Name, meterName, StringComparison.Ordinal))
                {
                    meters.Add(instrument.Meter);
                    instruments++;
                }
            },
        };
        listener.Start();
        return (meters.Count, instruments);
    }

    /// <summary>Collects the current observable measurements.</summary>
    public void RecordObservableInstruments() => _listener.RecordObservableInstruments();

    /// <inheritdoc/>
    public void Dispose() => _listener.Dispose();
}
