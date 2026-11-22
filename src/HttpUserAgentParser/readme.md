# MyCSharp.HttpUserAgentParser

Fast HTTP User-Agent parsing for .NET.

- Allocation-free detection with vectorized `SearchValues` (only the version string is allocated), no regular expressions in the parser
- Trimming and Native AOT compatible
- Detects the most used browsers and current search engine, SEO and AI crawlers

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

## Regular expressions

The parser itself does not execute regular expressions. `HttpUserAgentStatics.Platforms`, `HttpUserAgentStatics.Browsers`
and `HttpUserAgentPlatformInformation.Regex` provide source-generated regular expressions (`[GeneratedRegex]`) for your own
matching; they are created on first access. They use `RegexOptions.IgnoreCase` with culture-invariant case mapping and
no `RegexOptions.Compiled`, because the source generator already emits the matching code.

## Native AOT

The package is trimming and Native AOT compatible. EventCounters (`WithTelemetry()`) need
`<EventSourceSupport>true</EventSourceSupport>` in Native AOT apps; native meters work without it.

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

This cache has no size limit and no expiration: every distinct user agent string stays in memory as long as the provider lives.
Use it only for a known, limited set of user agents (e.g. in tests); for web applications use
[MyCSharp.HttpUserAgentParser.MemoryCache](https://www.nuget.org/packages/MyCSharp.HttpUserAgentParser.MemoryCache).

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
- `parse-duration` (s, event counter)
- `cache-hit` (incrementing, `HttpUserAgentParserCachedProvider`)
- `cache-miss` (incrementing, `HttpUserAgentParserCachedProvider`)
- `cache-size` (polling, `HttpUserAgentParserCachedProvider`)

### Monitor with dotnet-counters

```bash
dotnet-counters monitor --process-id <pid> MyCSharp.HttpUserAgentParser
```

## Telemetry (native Meters)

In addition to EventCounters, this package can emit **native** `System.Diagnostics.Metrics` instruments.

Telemetry is:

- **Opt-in**: disabled by default (keeps hot path overhead-free)
- **Low overhead**: measurements are only recorded when enabled

### Enable meters (Fluent API)

```csharp
services
	.AddHttpUserAgentParser()
	.WithMeterTelemetry();
```

Pass your own `Meter` to `WithMeterTelemetry(meter)` to record to it. A later registration with another meter replaces the
previous one, which then neither records nor reports values; a meter that is registered again reuses its instruments.

### Meter + instruments

Meter: `mycsharp.http_user_agent_parser` (the prefix `mycsharp.` can be changed with `WithMeterTelemetryPrefix(...)`)

- `parse.requests` (counter)
- `parse.duration` (histogram, s)
- `cache.hit` (counter)
- `cache.miss` (counter)
- `cache.size` (observable gauge)

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

### Export native meters to OpenTelemetry

If you enabled **native meters** (see above), collect them via `AddMeter(...)`:

```csharp
using OpenTelemetry.Metrics;

builder.Services.AddOpenTelemetry()
	.WithMetrics(metrics =>
	{
		metrics
			.AddMeter("mycsharp.http_user_agent_parser")
			.AddOtlpExporter();
	});
```

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
