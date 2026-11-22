# MyCSharp.HttpUserAgentParser

Parsing HTTP User Agents with .NET

The NuGet packages target .NET 8, 9, 10, and 11. Building the solution requires the .NET 11 SDK.

## NuGet

| NuGet |
|-|
| [![MyCSharp.HttpUserAgentParser](https://img.shields.io/nuget/v/MyCSharp.HttpUserAgentParser.svg?logo=nuget&label=MyCSharp.HttpUserAgentParser)](https://www.nuget.org/packages/MyCSharp.HttpUserAgentParser) |
| [![MyCSharp.HttpUserAgentParser](https://img.shields.io/nuget/v/MyCSharp.HttpUserAgentParser.MemoryCache.svg?logo=nuget&label=MyCSharp.HttpUserAgentParser.MemoryCache)](https://www.nuget.org/packages/MyCSharp.HttpUserAgentParser.MemoryCache)| `dotnet add package MyCSharp.HttpUserAgentParser.MemoryCache` |
| [![MyCSharp.HttpUserAgentParser.AspNetCore](https://img.shields.io/nuget/v/MyCSharp.HttpUserAgentParser.AspNetCore.svg?logo=nuget&label=MyCSharp.HttpUserAgentParser.AspNetCore)](https://www.nuget.org/packages/MyCSharp.HttpUserAgentParser.AspNetCore) | `dotnet add package MyCSharp.HttpUserAgentParser.AspNetCore` |


## Usage

```csharp
string userAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36";
HttpUserAgentInformation info = HttpUserAgentParser.Parse(userAgent); // alias HttpUserAgentInformation.Parse()
```
returns
```csharp
UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36"
Type = HttpUserAgentType.Browser
Platform = {
    Name = "Windows 10",
    PlatformType = HttpUserAgentPlatformType.Windows
}
Name = "Chrome"
Version = "90.0.4430.212"
MobileDeviceType = null
```

### Dependency Injection and Caching

For dependency injection mechanisms, the `IHttpUserAgentParserProvider` interface exists, for which built-in or custom caching mechanisms can be used. The use is always:

```csharp
private IHttpUserAgentParserProvider _parser;
public void MyMethod(string userAgent)
{
    HttpUserAgentInformation info = _parser.Parse(userAgent);
}
```

If no cache is required but dependency injection is still desired, the default cache provider can simply be used. This registers the `HttpUserAgentParserDefaultProvider`, which does not cache at all.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHttpUserAgentParser(); // uses HttpUserAgentParserDefaultProvider and does not cache
}
```

Likewise, an In Process Cache mechanism is provided, based on a `ConcurrentDictionary`.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHttpUserAgentCachedParser(); // uses `HttpUserAgentParserCachedProvider`
    // or
    // services.AddHttpUserAgentParser<HttpUserAgentParserCachedProvider>();
}
```

 This is especially recommended for tests. For web applications, the `IMemoryCache` implementation should be used, which offers a timed expiration of the entries.

The package [MyCSharp.HttpUserAgentParser.MemoryCache](https://www.nuget.org/packages/MyCSharp.HttpUserAgentParser.MemoryCache) is required to use the IMemoryCache. This enables the registration of the `IMemoryCache` implementation:


```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHttpUserAgentMemoryCachedParser();

    // or use options

    services.AddHttpUserAgentMemoryCachedParser(options =>
    {
        options.CacheEntryOptions.SlidingExpiration = TimeSpan.FromMinutes(60); // default is 1 day

        // limit the total entries in the MemoryCache
        //   each unique user agent string counts as one entry
        options.CacheOptions.SizeLimit = 1024; // default is 256
    });
}
```

> `AddHttpUserAgentMemoryCachedParser` registers `HttpUserAgentParserMemoryCachedProvider` as singleton which contains an isolated `MemoryCache` object.

### ASP.NET Core

For ASP.NET Core applications, an accessor can be registered additionally. It reads the `User-Agent` header of a request and parses it with the registered `IHttpUserAgentParserProvider`. This requires the package [MyCSharp.HttpUserAgentParser.AspNetCore](https://www.nuget.org/packages/MyCSharp.HttpUserAgentParser.AspNetCore)

There are two options:

| Registration | Service | Usage |
| --- | --- | --- |
| `AddHttpUserAgentParserAccessor()` | `IHttpUserAgentParserAccessor` | `Get(httpContext)`: you pass the `HttpContext` |
| `AddHttpUserAgentParserContextAccessor()` | `IHttpUserAgentParserContextAccessor` | `Get()`: the current request via `IHttpContextAccessor`, plus `Get(httpContext)` |

Passing the `HttpContext` is the recommended option: `IHttpContextAccessor` has a small cost on every request. The context accessor is useful in services that do not have the `HttpContext` at hand.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHttpUserAgentMemoryCachedParser() // registers Parser, returns HttpUserAgentParserDependencyInjectionOptions
        // or use any other Parser registration like services.AddHttpUserAgentParser<TParser>(); above
        .AddHttpUserAgentParserAccessor(); // registers IHttpUserAgentParserAccessor, uses IHttpUserAgentParserProvider
        // or: .AddHttpUserAgentParserContextAccessor(); // additionally registers IHttpUserAgentParserContextAccessor and IHttpContextAccessor
}
```

Now you can use

```csharp
// e.g. in a controller, a minimal API handler or a middleware
public void MyMethod(IHttpUserAgentParserAccessor parserAccessor, HttpContext httpContext)
{
    HttpUserAgentInformation? info = parserAccessor.Get(httpContext); // null if the request has no User-Agent header
}

// in any service, with AddHttpUserAgentParserContextAccessor()
public void MyMethod(IHttpUserAgentParserContextAccessor parserAccessor)
{
    HttpUserAgentInformation? info = parserAccessor.Get(); // null outside of a request or without User-Agent header
}
```

If you only need the raw header value, use `httpContext.GetUserAgentString()`.

## Detection rules

`HttpUserAgentParser.Parse` checks, in this order:

1. **Robots**: search engine crawlers (e.g. Googlebot, Bingbot, Applebot, DuckDuckBot, YandexBot), SEO crawlers (e.g. AhrefsBot, SemrushBot, Screaming Frog) and AI crawlers and AI user agents (e.g. GPTBot, ChatGPT-User, OAI-SearchBot, ClaudeBot, PerplexityBot, Amazonbot, Bytespider, CCBot)
2. **Platform**: Windows, macOS, iOS, Android, ChromeOS, Linux and others
3. **Mobile device**: devices and manufacturers before operating systems before mobile browsers before generic tokens (e.g. a Samsung phone reports `Samsung`, an Android phone without manufacturer token `Android`)
4. **Browser and version**: Chrome, Edge, Firefox, Safari, Opera, Samsung Internet, Yandex Browser, UC Browser and others

In every list the first matching entry wins. Brave and Vivaldi send the same user agent as Chrome and are reported as Chrome; iPadOS sends the macOS user agent by default.

### `HttpUserAgentStatics` and regular expressions

The parser does not execute regular expressions. `HttpUserAgentStatics.Platforms`, `HttpUserAgentStatics.Browsers` and `HttpUserAgentPlatformInformation.Regex` still provide regular expressions for your own matching. They are generated at compile time with `[GeneratedRegex]` (no runtime compilation, trimming and Native AOT compatible) and are only created on first access; the browser patterns capture the version in group 1.

`HttpUserAgentStatics.Robots` is the array the parser uses: if you replace an entry, the parser honors it.

## Native AOT and trimming

All packages are annotated with `IsAotCompatible` and are verified by publishing a Native AOT smoke test for every target framework (`tests/HttpUserAgentParser.NativeAot.Smoke`):

```shell
dotnet publish tests/HttpUserAgentParser.NativeAot.Smoke -c Release -f net11.0 -r linux-x64
```

Meters work out of the box. EventCounters rely on `EventSource`, which is disabled by default in Native AOT apps; enable it with `<EventSourceSupport>true</EventSourceSupport>` in your app's project file if you use `WithTelemetry()`.

## Telemetry (EventCounters)

Telemetry is **opt-in** and **modular per package**.

- Opt-in: no telemetry overhead unless you explicitly enable it.
- Modular: each package has its own `EventSource` name, so you can monitor only what you use.

### Enable telemetry (Fluent API)

Core parser telemetry:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHttpUserAgentParser()
        .WithTelemetry();
}
```

MemoryCache telemetry (in addition to core, optional):

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHttpUserAgentMemoryCachedParser()
        .WithTelemetry() // core counters (optional)
        .WithMemoryCacheTelemetry();
}
```

ASP.NET Core telemetry (header present/missing):

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHttpUserAgentMemoryCachedParser()
        .AddHttpUserAgentParserAccessor()
        .WithAspNetCoreTelemetry();
}
```

### EventSource names and counters

Core (`MyCSharp.HttpUserAgentParser`)

- `parse-requests`
- `parse-duration` (s)
- `cache-hit` (`HttpUserAgentParserCachedProvider`)
- `cache-miss` (`HttpUserAgentParserCachedProvider`)
- `cache-size` (`HttpUserAgentParserCachedProvider`)

MemoryCache (`MyCSharp.HttpUserAgentParser.MemoryCache`)

- `cache-hit`
- `cache-miss`
- `cache-size`

ASP.NET Core (`MyCSharp.HttpUserAgentParser.AspNetCore`)

- `useragent-present`
- `useragent-missing`

### Monitor counters

Using `dotnet-counters`:

```bash
dotnet-counters monitor --process-id <pid> MyCSharp.HttpUserAgentParser
dotnet-counters monitor --process-id <pid> MyCSharp.HttpUserAgentParser.MemoryCache
dotnet-counters monitor --process-id <pid> MyCSharp.HttpUserAgentParser.AspNetCore
```

## Telemetry (Meters)

Native `System.Diagnostics.Metrics` instruments are **opt-in** per package.

### Enable meters (Fluent API)

Core parser meters:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHttpUserAgentParser()
        .WithMeterTelemetry();
}
```

MemoryCache meters:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHttpUserAgentMemoryCachedParser()
        .WithMeterTelemetry() // core meters (optional)
        .WithMemoryCacheMeterTelemetry();
}
```

ASP.NET Core meters:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHttpUserAgentMemoryCachedParser()
        .AddHttpUserAgentParserAccessor()
        .WithAspNetCoreMeterTelemetry();
}
```

### Meter names and instruments

Core meter (default: `mycsharp.http_user_agent_parser`)

- `parse.requests` (counter, `{call}`)
- `parse.duration` (histogram, `s`)
- `cache.hit` (counter, `{call}`)
- `cache.miss` (counter, `{call}`)
- `cache.size` (observable gauge, `{entry}`)

MemoryCache meter (default: `mycsharp.http_user_agent_parser.memorycache`)

- `cache.hit` (counter, `{call}`)
- `cache.miss` (counter, `{call}`)
- `cache.size` (observable gauge, `{entry}`)

ASP.NET Core meter (default: `mycsharp.http_user_agent_parser.aspnetcore`)

- `user_agent.present` (counter, `{call}`)
- `user_agent.missing` (counter, `{call}`)

### Meter prefix configuration

The default prefix is `mycsharp.`. The prefix can be configured via DI:

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHttpUserAgentParser()
        .WithMeterTelemetryPrefix("acme.");
}
```

Rules:

- `""` (empty) is allowed and removes the prefix.
- Otherwise the prefix must be **alphanumeric** and **end with `.`**.

Example results:

- Prefix `"mycsharp."` -> `mycsharp.http_user_agent_parser`
- Prefix `""` -> `http_user_agent_parser`
- Prefix `"acme."` -> `acme.http_user_agent_parser`

### Export to OpenTelemetry

You can collect these EventCounters via OpenTelemetry metrics.

Packages you typically need:

- `OpenTelemetry`
- `OpenTelemetry.Instrumentation.EventCounters`
- an exporter (e.g. `OpenTelemetry.Exporter.OpenTelemetryProtocol`)

Example (minimal):

```csharp
using OpenTelemetry.Metrics;
using MyCSharp.HttpUserAgentParser.Telemetry;
using MyCSharp.HttpUserAgentParser.MemoryCache.Telemetry;
using MyCSharp.HttpUserAgentParser.AspNetCore.Telemetry;

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .AddEventCountersInstrumentation(options =>
            {
                options.AddEventSources(
                    HttpUserAgentParserEventSource.EventSourceName,
                    HttpUserAgentParserMemoryCacheEventSource.EventSourceName,
                    HttpUserAgentParserAspNetCoreEventSource.EventSourceName);
            })
            .AddOtlpExporter();
    });
```

### Export to Application Insights

Two common approaches:

1) OpenTelemetry → Application Insights (recommended)
   - Collect counters with OpenTelemetry (see above)
   - Export using an Azure Monitor / Application Insights exporter (API varies by package/version)

2) Custom `EventListener` → `TelemetryClient`
   - Attach an `EventListener`
   - Parse the `EventCounters` payload
   - Forward values as custom metrics

### OpenTelemetry listener (recommended)

You can collect EventCounters as OpenTelemetry metrics.

Typical packages:

- `OpenTelemetry`
- `OpenTelemetry.Instrumentation.EventCounters`
- An exporter, e.g. `OpenTelemetry.Exporter.OpenTelemetryProtocol`

Example:

```csharp
using OpenTelemetry.Metrics;

builder.Services.AddOpenTelemetry()
    .WithMetrics(metrics =>
    {
        metrics
            .AddEventCountersInstrumentation(options =>
            {
                options.AddEventSources(
                    HttpUserAgentParserEventSource.EventSourceName,
                    HttpUserAgentParserMemoryCacheEventSource.EventSourceName,
                    HttpUserAgentParserAspNetCoreEventSource.EventSourceName);
            })
            .AddOtlpExporter();
    });
```

From there you can route metrics to:

- OpenTelemetry Collector
- Prometheus
- Azure Monitor / Application Insights (via an Azure Monitor exporter)

### Application Insights listener (custom)

If you want a direct listener, you can attach an `EventListener` and forward counter values into Application Insights custom metrics.

High-level steps:

1) Enable telemetry via `.WithTelemetry()` / `.WithMemoryCacheTelemetry()` / `.WithAspNetCoreTelemetry()`
2) Register an `EventListener` that enables the corresponding EventSources
3) On `EventCounters` payload, forward values to `TelemetryClient.GetMetric(...).TrackValue(...)`

Notes:

- This is best-effort telemetry.
- Prefer longer polling intervals (e.g. 10s) to reduce noise.

> Notes
>
> - Counters are only emitted when telemetry is enabled and a listener is attached.
> - Values are best-effort and may include cache races.

## Benchmark

Run the parser benchmarks with the .NET 11 runtime.

```shell
# 7 typical user agents
dotnet run --project perf/HttpUserAgentParser.Benchmarks -c Release -f net11.0 -- --filter "*HttpUserAgentParserBenchmarks*" --inProcess
# mixed real-world workload (closest to production)
dotnet run --project perf/HttpUserAgentParser.Benchmarks -c Release -f net11.0 -- --filter "*MixedWorkloadBenchmarks*" --inProcess
# every browser (current and previous browser version) and every crawler of the test catalog
dotnet run --project perf/HttpUserAgentParser.Benchmarks -c Release -f net11.0 -- --filter "*BrowserUserAgentBenchmarks*" "*RobotUserAgentBenchmarks*" --inProcess
```

The user agents come from `tests/HttpUserAgentParser.TestHelpers/UserAgentCatalog.cs`, which the unit tests use as well.

### Parser performance on .NET 11

BenchmarkDotNet 0.15.8 on .NET 11.0.0 x64, Windows 10 and an AMD Ryzen 9 9950X, without caching. The only allocation is the version string.

Mixed workload: the catalog user agents parsed in a loop, mean time per user agent.

| Workload | Mean | Allocated |
| --- | ---: | ---: |
| 44 browser user agents | 291.6 ns | 38 B |
| 49 crawler user agents | 143.1 ns | 0 B |
| All 97 user agents | 223.5 ns | 18 B |

Single user agents:

| User agent | Mean | Allocated |
| --- | ---: | ---: |
| Chrome on Windows | 261.2 ns | 40 B |
| Firefox on Linux | 372.0 ns | 32 B |
| Chrome on Android | 276.6 ns | 40 B |
| Safari on iPhone | 320.3 ns | 32 B |
| Applebot | 170.7 ns | 0 B |
| Googlebot | 9.96 ns | 0 B |
| Unknown client | 422.3 ns | 0 B |

Startup in a new process (median of 7 runs):

| | Time | Allocated |
| --- | ---: | ---: |
| First `Parse` call | 9.6 ms | 129 KB |
| First use of all `HttpUserAgentStatics` regular expressions (43 platforms, 35 browsers) | 16.6 ms | 114 KB |

### Library comparison on .NET 11

```shell
dotnet run --project perf/HttpUserAgentParser.Benchmarks -c Release -f net11.0 -- --filter "*LibraryComparison*"
```

The ShortRun job (3 iterations) executes in-process.

```shell
BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.7846/22H2/2022Update)
AMD Ryzen 9 9950X 4.30GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 11.0.100
  [Host] : .NET 11.0.0 (11.0.0, 11.0.26.53018), X64 RyuJIT x86-64-v4

Job=ShortRun  Toolchain=InProcessEmitToolchain  IterationCount=3
LaunchCount=1  WarmupCount=3
```

| Method             | Categories | Data         | Mean             | Error            | StdDev         | Ratio     | RatioSD  | Gen0     | Gen1     | Gen2    | Allocated  | Alloc Ratio |
|------------------- |----------- |------------- |-----------------:|-----------------:|---------------:|----------:|---------:|---------:|---------:|--------:|-----------:|------------:|
| MyCSharp           | Basic      | Chrome Win10 |        290.42 ns |        21.325 ns |       1.169 ns |      1.00 |     0.00 |        - |        - |       - |       48 B |        1.00 |
| UAParser           | Basic      | Chrome Win10 | 11,049,381.25 ns | 2,603,063.009 ns | 142,682.668 ns | 38,046.15 |   445.58 | 656.2500 | 468.7500 | 93.7500 | 11654118 B |  242,794.12 |
| DeviceDetector.NET | Basic      | Chrome Win10 |  6,239,145.83 ns | 1,814,748.937 ns |  99,472.513 ns | 21,483.15 |   305.89 | 335.9375 | 179.6875 | 46.8750 |  5529599 B |  115,199.98 |
|                    |            |              |                  |                  |                |           |          |          |          |         |            |             |
| MyCSharp           | Basic      | Google-Bot   |        246.81 ns |        96.469 ns |       5.288 ns |      1.00 |     0.03 |        - |        - |       - |          - |          NA |
| UAParser           | Basic      | Google-Bot   | 13,090,585.94 ns | 3,799,306.664 ns | 208,252.819 ns | 53,054.68 | 1,231.46 | 718.7500 | 484.3750 | 31.2500 | 12010207 B |          NA |
| DeviceDetector.NET | Basic      | Google-Bot   |  7,389,143.49 ns |   348,698.377 ns |  19,113.335 ns | 29,947.37 |   563.38 | 609.3750 | 132.8125 | 23.4375 | 10124580 B |          NA |
|                    |            |              |                  |                  |                |           |          |          |          |         |            |             |
| MyCSharp           | Cached     | Chrome Win10 |         24.20 ns |        16.275 ns |       0.892 ns |      1.00 |     0.05 |        - |        - |       - |          - |          NA |
| UAParser           | Cached     | Chrome Win10 |    146,715.00 ns |    23,237.727 ns |   1,273.738 ns |  6,069.10 |   203.13 |   2.1973 |        - |       - |    37402 B |          NA |
|                    |            |              |                  |                  |                |           |          |          |          |         |            |             |
| MyCSharp           | Cached     | Google-Bot   |         18.58 ns |         0.046 ns |       0.002 ns |      1.00 |     0.00 |        - |        - |       - |          - |          NA |
| UAParser           | Cached     | Google-Bot   |    125,494.21 ns |     6,022.587 ns |     330.118 ns |  6,754.11 |    15.41 |   2.6855 |        - |       - |    45834 B |          NA |

## Disclaimer

This library is inspired by [UserAgentService by DannyBoyNg](https://github.com/DannyBoyNg/UserAgentService) and contains optimizations for our requirements on [myCSharp.de](https://mycsharp.de).
We decided to fork the project, because we want a general restructuring with corresponding breaking changes.

## Maintained

by [@BenjaminAbt](https://github.com/BenjaminAbt) and [@gfoidl](https://github.com/gfoidl)

## License

MIT License

Copyright (c) 2021-2026 myCSharp.de

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.

