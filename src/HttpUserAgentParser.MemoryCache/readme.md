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
	options.CacheOptions.SizeLimit = 1024; // default is 256
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

- `cache-hit` (incrementing)
- `cache-miss` (incrementing)
- `cache-size` (polling): the entries of all live providers of the package, including entries cached before telemetry was enabled

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

Pass your own `Meter` to `WithMemoryCacheMeterTelemetry(meter)` to record to it. A later registration with another meter
replaces the previous one, which then neither records nor reports values; a meter that is registered again reuses its instruments.
Meters the package creates itself (without argument or with a prefix) are created once per name and disposed when they are replaced.

### Meter + instruments

Meter: `mycsharp.http_user_agent_parser.memorycache` (constant: `HttpUserAgentParserMemoryCachedProvider.MeterName`)

- `cache.hit` (counter)
- `cache.miss` (counter)
- `cache.size` (observable gauge): the entries of all live providers of the package, including entries cached before telemetry was enabled

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
using MyCSharp.HttpUserAgentParser.MemoryCache;

metrics.AddMeter(HttpUserAgentParserMemoryCachedProvider.MeterName);
```

### Application Insights listener registration

If you prefer a direct listener instead of OpenTelemetry, you can attach an `EventListener` and forward values into Application Insights.
