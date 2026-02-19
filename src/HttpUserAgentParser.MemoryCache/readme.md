# MyCSharp.HttpUserAgentParser.MemoryCache

IMemoryCache-based caching provider for MyCSharp.HttpUserAgentParser.

Repository:
https://github.com/mycsharp/HttpUserAgentParser

## Install

```bash
dotnet add package MyCSharp.HttpUserAgentParser.MemoryCache
```

## Quick start

Register the provider:

```csharp
services.AddHttpUserAgentMemoryCachedParser();
```

Then inject `IHttpUserAgentParserProvider`:

```csharp
public sealed class MyService(IHttpUserAgentParserProvider parser)
{
	public HttpUserAgentInformation Parse(string userAgent) => parser.Parse(userAgent);
```

### Configure cache

```csharp
services.AddHttpUserAgentMemoryCachedParser(options =>
{
	options.CacheEntryOptions.SlidingExpiration = TimeSpan.FromMinutes(60); // default is 1 day
	options.CacheOptions.SizeLimit = 1024; // default is null (= no limit)
});
```

Notes:

- Each unique user-agent string counts as one cache entry.
- The provider is registered as singleton and owns its internal `MemoryCache` instance.
- Like any cache, concurrent requests for a new key can race; counters are best-effort.

## Telemetry (EventCounters)

Telemetry is **modular** and **opt-in**.

### Enable (Fluent API)

```csharp
services
	.AddHttpUserAgentMemoryCachedParser()
	.WithMemoryCacheTelemetry();
```

Optionally enable core counters too:

```csharp
services
	.AddHttpUserAgentMemoryCachedParser()
	.WithTelemetry()
	.WithMemoryCacheTelemetry();
```

### EventSource + counters

EventSource: `MyCSharp.HttpUserAgentParser.MemoryCache` (constant: `HttpUserAgentParserMemoryCacheEventSource.EventSourceName`)

- `user_agent_parser.cache.hit` (incrementing)
- `user_agent_parser.cache.miss` (incrementing)
- `user_agent_parser.cache.size` (polling)

### Monitor with dotnet-counters

```bash
dotnet-counters monitor --process-id <pid> MyCSharp.HttpUserAgentParser.MemoryCache
```

## Telemetry (native Meters)

This package can also emit native `System.Diagnostics.Metrics` instruments.

### Enable meters (Fluent API)

```csharp
services
	.AddHttpUserAgentMemoryCachedParser()
	.WithMemoryCacheMeterTelemetry();
```

Optionally enable core meters too:

```csharp
services
	.AddHttpUserAgentMemoryCachedParser()
	.WithMeterTelemetry()
	.WithMemoryCacheMeterTelemetry();
```

### Meter + instruments

Meter: `MyCSharp.HttpUserAgentParser.MemoryCache` (constant: `HttpUserAgentParserMemoryCacheMeters.MeterName`)

- `user_agent_parser.cache.hit` (counter)
- `user_agent_parser.cache.miss` (counter)
- `user_agent_parser.cache.size` (observable gauge)

## Export to OpenTelemetry / Application Insights

You can collect these counters with OpenTelemetry’s EventCounters instrumentation.

Add the EventSource name:

```csharp
using OpenTelemetry.Metrics;

metrics.AddEventCountersInstrumentation(options =>
{
	options.AddEventSources(HttpUserAgentParserMemoryCacheEventSource.EventSourceName);
});
```

From there you can export to:

- OTLP (Collector)
- Prometheus
- Azure Monitor / Application Insights (via an Azure Monitor exporter)

### Export native meters to OpenTelemetry

If you enabled **native meters** (see above), collect them via `AddMeter(...)`:

```csharp
using OpenTelemetry.Metrics;
using MyCSharp.HttpUserAgentParser.MemoryCache.Telemetry;

metrics.AddMeter(HttpUserAgentParserMemoryCacheMeters.MeterName);
```

### Application Insights listener registration

If you prefer a direct listener instead of OpenTelemetry, you can attach an `EventListener` and forward values into Application Insights.
