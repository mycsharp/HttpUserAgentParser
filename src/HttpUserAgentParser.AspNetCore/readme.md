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

The accessor reads the `User-Agent` header of a request and parses it using the registered provider. There are two options:

- `AddHttpUserAgentParserAccessor()` registers `IHttpUserAgentParserAccessor`: you pass the `HttpContext` (`Get(httpContext)`). Recommended.
- `AddHttpUserAgentParserContextAccessor()` registers `IHttpUserAgentParserContextAccessor` and `IHttpContextAccessor`: `Get()` uses the current request, `Get(httpContext)` works as well. Useful in services without access to the `HttpContext`; `IHttpContextAccessor` has a small cost on every request.

```csharp
services
	.AddHttpUserAgentMemoryCachedParser() // or: AddHttpUserAgentParser / AddHttpUserAgentCachedParser
	.AddHttpUserAgentParserAccessor(); // or: .AddHttpUserAgentParserContextAccessor()
```

Usage with the `HttpContext`:

```csharp
public sealed class MyController(IHttpUserAgentParserAccessor accessor) : ControllerBase
{
	[HttpGet]
	public HttpUserAgentInformation? Get() => accessor.Get(HttpContext); // null without User-Agent header
}

// Minimal API
app.MapGet("/ua", (HttpContext context, IHttpUserAgentParserAccessor accessor) => accessor.Get(context));
```

Usage with the current request (`AddHttpUserAgentParserContextAccessor()`):

```csharp
public sealed class MyService(IHttpUserAgentParserContextAccessor accessor)
{
	public bool IsRobot() => accessor.Get()?.IsRobot() ?? false; // null outside of a request or without User-Agent header
}
```

The package is trimming and Native AOT compatible.

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

Pass your own `Meter` to `WithAspNetCoreMeterTelemetry(meter)` to record to it. A later registration with another meter
replaces the previous one; registering the meter in use again has no effect.

### Meter + instruments

Meter: `mycsharp.http_user_agent_parser.aspnetcore` (the prefix `mycsharp.` can be changed with `WithAspNetCoreMeterTelemetryPrefix(...)`)

- `user_agent.present` (counter)
- `user_agent.missing` (counter)

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

metrics.AddMeter("mycsharp.http_user_agent_parser.aspnetcore");
```
