// Copyright © https://myCSharp.de - all rights reserved

using System.Collections.Concurrent;
using System.Diagnostics.Metrics;

namespace MyCSharp.HttpUserAgentParser.UnitTests.Telemetry;

internal sealed class MeterTestListener : IDisposable
{
    private readonly string _meterName;
    private readonly MeterListener _listener;

    public ConcurrentBag<string> InstrumentNames { get; } = [];

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

    public void RecordObservableInstruments() => _listener.RecordObservableInstruments();

    public void Dispose() => _listener.Dispose();
}
