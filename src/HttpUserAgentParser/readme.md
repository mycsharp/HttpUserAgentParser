# MyCSharp.HttpUserAgentParser

Fast HTTP User-Agent parsing for .NET.

Repository:
https://github.com/mycsharp/HttpUserAgentParser

## Install

```bash
dotnet add package MyCSharp.HttpUserAgentParser
```

## Quick start (no DI)

```csharp
string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36";
HttpUserAgentInformation info = HttpUserAgentParser.Parse(userAgent);
// or: HttpUserAgentInformation.Parse(userAgent)
```

## Dependency injection

If you want to inject a parser (e.g., in ASP.NET Core), use `IHttpUserAgentParserProvider`.

### No cache

```csharp
services
	.AddHttpUserAgentParser();
```

### ConcurrentDictionary cache

```csharp
services
	.AddHttpUserAgentCachedParser();
// or: .AddHttpUserAgentParser<HttpUserAgentParserCachedProvider>();
```

## Telemetry (EventCounters)

Telemetry is:

- **Opt-in**: disabled by default (keeps hot path overhead-free)
- **Low overhead**: counters are only written when a listener is attached

### Enable telemetry (Fluent API)

```csharp
services
	.AddHttpUserAgentParser()
	.WithTelemetry();
```

### EventSource + counters

EventSource: `MyCSharp.HttpUserAgentParser` (constant: `HttpUserAgentParserEventSource.EventSourceName`)

- `parse-requests` (incrementing)
- `parse-duration` (ms, event counter)
- `cache-concurrentdictionary-hit` (incrementing)
- `cache-concurrentdictionary-miss` (incrementing)
- `cache-concurrentdictionary-size` (polling)

### Monitor with dotnet-counters

```bash
dotnet-counters monitor --process-id <pid> MyCSharp.HttpUserAgentParser
```

## Export to OpenTelemetry

You can collect these EventCounters via OpenTelemetry metrics and export them (OTLP, Prometheus, Azure Monitor, …).

Packages you typically need:

- `OpenTelemetry`
- `OpenTelemetry.Exporter.OpenTelemetryProtocol` (or another exporter)
- `OpenTelemetry.Instrumentation.EventCounters`

Example (minimal):

```csharp
using OpenTelemetry.Metrics;
using MyCSharp.HttpUserAgentParser.Telemetry;

builder.Services.AddOpenTelemetry()
	.WithMetrics(metrics =>
	{
		metrics
			.AddEventCountersInstrumentation(options =>
			{
				options.AddEventSources(HttpUserAgentParserEventSource.EventSourceName);
			})
			.AddOtlpExporter();
	});
```

> If you also use the MemoryCache/AspNetCore packages, add their EventSource names too.

## Export to Application Insights

There are two common approaches:

### 1) Recommended: OpenTelemetry → Application Insights

Collect with OpenTelemetry (see above) and export to Azure Monitor / Application Insights using an Azure Monitor exporter.
This keeps your pipeline consistent and avoids custom listeners.

Typical packages (names may differ by version):

- `OpenTelemetry`
- `OpenTelemetry.Instrumentation.EventCounters`
- `Azure.Monitor.OpenTelemetry.Exporter`

### 2) Custom EventListener → TelemetryClient

If you prefer a direct listener, you can attach an `EventListener` and forward values as custom metrics.

High-level idea:

- Enable the EventSource
- Parse the `EventCounters` payload
- Track as Application Insights metrics

Notes:

- This is best-effort telemetry (caches can race)
- Keep aggregation intervals reasonable (e.g. 10s)
