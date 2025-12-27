# MyCSharp.HttpUserAgentParser.AspNetCore

ASP.NET Core integration for MyCSharp.HttpUserAgentParser.

Repository:
https://github.com/mycsharp/HttpUserAgentParser

## Install

```bash
dotnet add package MyCSharp.HttpUserAgentParser.AspNetCore
```

## Quick start

Register a provider (any of the available ones) and then add the accessor:

The accessor pattern reads the `User-Agent` header from the current `HttpContext` and parses it using the registered provider.

```csharp
services
	.AddHttpUserAgentMemoryCachedParser() // or: AddHttpUserAgentParser / AddHttpUserAgentCachedParser
	.AddHttpUserAgentParserAccessor();
```

Usage:

```csharp
public sealed class MyController(IHttpUserAgentParserAccessor accessor)
{
	public HttpUserAgentInformation Get() => accessor.Get();
}
```

### Just read the header

If you only want the raw User-Agent string:

```csharp
string? ua = HttpContext.GetUserAgentString();
```

## Telemetry (EventCounters)

Telemetry is **modular** and **opt-in**.

### Enable (Fluent API)

```csharp
services
	.AddHttpUserAgentParserAccessor()
	.WithAspNetCoreTelemetry();
```

> The accessor registration returns the same options object, so you can chain this after any parser registration.

### EventSource + counters

EventSource: `MyCSharp.HttpUserAgentParser.AspNetCore` (constant: `HttpUserAgentParserAspNetCoreEventSource.EventSourceName`)

- `useragent-present` (incrementing)
- `useragent-missing` (incrementing)

### Monitor with dotnet-counters

```bash
dotnet-counters monitor --process-id <pid> MyCSharp.HttpUserAgentParser.AspNetCore
```

## Telemetry (native Meters)

This package can also emit native `System.Diagnostics.Metrics` instruments.

### Enable meters (Fluent API)

```csharp
services
	.AddHttpUserAgentParserAccessor()
	.WithAspNetCoreMeterTelemetry();
```

### Meter + instruments

Meter: `MyCSharp.HttpUserAgentParser.AspNetCore` (constant: `HttpUserAgentParserAspNetCoreMeters.MeterName`)

- `useragent-present` (counter)
- `useragent-missing` (counter)

## Export to OpenTelemetry / Application Insights

Collect via OpenTelemetry EventCounters instrumentation:

```csharp
using OpenTelemetry.Metrics;

metrics.AddEventCountersInstrumentation(options =>
{
	options.AddEventSources(HttpUserAgentParserAspNetCoreEventSource.EventSourceName);
});
```

Then export using your preferred exporter (OTLP, Prometheus, Azure Monitor / Application Insights, …).

### Export native meters to OpenTelemetry

If you enabled **native meters** (see above), collect them via `AddMeter(...)`:

```csharp
using OpenTelemetry.Metrics;
using MyCSharp.HttpUserAgentParser.AspNetCore.Telemetry;

metrics.AddMeter(HttpUserAgentParserAspNetCoreMeters.MeterName);
```
