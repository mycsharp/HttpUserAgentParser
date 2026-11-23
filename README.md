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

Likewise, an in-process cache based on a `ConcurrentDictionary` is provided.

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services.AddHttpUserAgentCachedParser(); // uses `HttpUserAgentParserCachedProvider`
    // or
    // services.AddHttpUserAgentParser<HttpUserAgentParserCachedProvider>();
}
```

This cache has no size limit and no expiration: every distinct user agent string stays in memory as long as the provider lives. Both caches use the exact user agent string as key (ordinal), because the result contains the user agent. Clients choose the `User-Agent` header freely, so use it only for a known, limited set of user agents, e.g. in tests. For web applications, use the `IMemoryCache` implementation, which limits the number of entries and expires them.

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

> `AddHttpUserAgentMemoryCachedParser` registers `HttpUserAgentParserMemoryCachedProvider` as singleton which contains an isolated `MemoryCache` object; the container disposes the provider and its cache. Every entry gets all `CacheEntryOptions` (e.g. `AbsoluteExpirationRelativeToNow`, `Priority`, expiration tokens, eviction callbacks) and the size 1, unless `CacheEntryOptions.Size` is set.

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
    HttpUserAgentInformation? info = parserAccessor.Get(httpContext); // null if the request has no (or an empty) User-Agent header
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
4. **Browser and version**: Chrome, Edge, Firefox, Safari, Opera, Samsung Internet, Amazon Silk, Yandex Browser, UC Browser and others

In every list the first matching entry wins. Browser and version tokens only count at the start of a token: at the beginning of the user agent or after a space, tab, `+`, `;`, `(`, `)` or `,`, so `Knowledge/1.0` is no Edge and `VendorVersion/1.0` no version token. The version is read from the browser's own product token (`Chrome/131.0.0.0`), which ends at the next of these characters; Opera and Internet Explorer also accept `Opera 9.50` and `MSIE 6.0`; their separate version tokens (`Version/`, `rv:`) only count after the browser token. If a token has no version, later occurrences of the token are checked, then the next rule. `+` stands for a space as in URL-encoded logs (IIS); decode other encodings such as `%20` before parsing, e.g. with `WebUtility.UrlDecode`.

Brave and Vivaldi send the same user agent as Chrome and are reported as Chrome, as is Headless Chrome; iPadOS sends the macOS user agent by default.

### `HttpUserAgentStatics` and regular expressions

The parser does not execute regular expressions. `HttpUserAgentStatics.Platforms`, `HttpUserAgentStatics.Browsers` and `HttpUserAgentPlatformInformation.Regex` still provide regular expressions for your own matching. They are generated at compile time with `[GeneratedRegex]` (no runtime compilation, trimming and Native AOT compatible) and are created on first use: the browser regexes when `HttpUserAgentStatics` is first used (including `HttpUserAgentStatics.Robots`), a platform regex on first access of `HttpUserAgentPlatformInformation.Regex`. The browser patterns capture the version in group 1.

The patterns use `RegexOptions.IgnoreCase` with culture-invariant case mapping, so a match does not depend on the current culture (e.g. `LINUX` matches `linux` under `tr-TR` as well). `RegexOptions.Compiled` is not set, because the source generator already emits the matching code.

### Custom robots

`HttpUserAgentStatics.Robots` lists the robots in priority order. The first key found in the user agent wins, ignoring case. Spaces in a key also match IIS-encoded plus signs, while a key containing a literal plus sign is matched against the original header. `SetRobots` replaces the list for the entire process, including existing instances of both built-in cache providers. A parse already in progress can finish with its previous rules. To extend the defaults at startup, pass the default entries followed by your own.

```csharp
HttpUserAgentStatics.SetRobots([.. HttpUserAgentStatics.DefaultRobots, ("MyCrawler", "My Crawler")]);
```

## Native AOT and trimming

All packages are annotated with `IsAotCompatible`. The Native AOT smoke test (`tests/HttpUserAgentParser.NativeAot.Smoke`) is published and run with .NET 8, 9, 10 and 11 in every CI build of `main` and of pull requests:

```shell
dotnet publish tests/HttpUserAgentParser.NativeAot.Smoke -c Release -f net11.0 -r linux-x64
```

The app roots all three library assemblies, so the trimmer and the AOT compiler analyze all of their code, and the publish fails on any trimming or AOT warning. At runtime it checks:

- parsing of browsers, platforms, mobile devices and crawlers
- the source-generated regular expressions of `HttpUserAgentStatics`
- dependency injection with the cached providers and both ASP.NET Core accessors
- measurements of the meters of all three packages, received by a `MeterListener`
- the EventCounters of all three packages, received by an `EventListener`

Meters work out of the box. EventCounters rely on `EventSource`, which is disabled by default in Native AOT apps; enable it with `<EventSourceSupport>true</EventSourceSupport>` in your app's project file if you use `WithTelemetry()`. The smoke test sets this property.

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
- `cache-size` (`HttpUserAgentParserCachedProvider`): the entries of all live providers of the package, including entries cached before telemetry was enabled

MemoryCache (`MyCSharp.HttpUserAgentParser.MemoryCache`)

- `cache-hit`
- `cache-miss`
- `cache-size`: the entries of all live providers of the package, including entries cached before telemetry was enabled

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

Each package records to one meter at a time. Without an argument, the package creates its meter once. Pass your own `Meter` to `WithMeterTelemetry(meter)`, `WithMemoryCacheMeterTelemetry(meter)` or `WithAspNetCoreMeterTelemetry(meter)` to record to it instead. A later registration with another meter (including `WithMeterTelemetryPrefix`) replaces the previous one, which then neither records nor reports values; a meter that is registered again reuses its instruments. The meter the package creates itself (without argument or with a prefix) is reused for the same name and disposed when another meter replaces it; a measurement that runs exactly during that switch may be lost. Registering that disposed meter again with the same package throws an `ObjectDisposedException`. Meters supplied by callers remain the caller’s responsibility to dispose. The core and the MemoryCache package need separate meters, because both create the instruments `cache.hit`, `cache.miss` and `cache.size`, and a meter keeps its instruments until it is disposed: a meter that one of them has used is rejected by the other with an `ArgumentException`, also after a switch. The ASP.NET Core package may share a meter with either.

### Meter names and instruments

Core meter (default: `mycsharp.http_user_agent_parser`)

- `parse.requests` (counter, `{call}`)
- `parse.duration` (histogram, `s`)
- `cache.hit` (counter, `{call}`)
- `cache.miss` (counter, `{call}`)
- `cache.size` (observable gauge, `{entry}`): the entries of all live providers of the package, including entries cached before telemetry was enabled

MemoryCache meter (default: `mycsharp.http_user_agent_parser.memorycache`)

- `cache.hit` (counter, `{call}`)
- `cache.miss` (counter, `{call}`)
- `cache.size` (observable gauge, `{entry}`): the entries of all live providers of the package, including entries cached before telemetry was enabled

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
- Otherwise the prefix must be **ASCII letters or digits** followed by **`.`** (e.g. `acme.`).

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

Run the parser benchmarks with the .NET 11 runtime (use `-f net8.0`, `net9.0` or `net10.0` for the other runtimes).

```shell
# individual cases, including the useragents.me snapshot
dotnet run --project perf/HttpUserAgentParser.Benchmarks -c Release -f net11.0 -- --filter "*HttpUserAgentParserBenchmarks*"
# mixed real-world workload (closest to production)
dotnet run --project perf/HttpUserAgentParser.Benchmarks -c Release -f net11.0 -- --filter "*MixedWorkloadBenchmarks*"
# every browser (current and previous browser version) and every crawler of the test catalog
dotnet run --project perf/HttpUserAgentParser.Benchmarks -c Release -f net11.0 -- --filter "*BrowserUserAgentBenchmarks*" "*RobotUserAgentBenchmarks*"
```

The individual benchmarks include a fixed sample of desktop, phone, tablet, crawler and HTTP client headers from November 15–22, 2026. The parser tests validate each captured header in `UserAgentsMeSnapshot.cs`. The browser, crawler and mixed workload benchmarks use the separate `UserAgentCatalog.cs` corpus. Both datasets run offline. [Snapshot source](https://useragents.me/api)

### Parser performance

BenchmarkDotNet 0.16.0-preview.2 (default job: every benchmark case runs in its own process), x64, Windows 10 and an AMD Ryzen 9 9950X, without caching. For a user agent without leading or trailing white space, the only allocation is the version string; otherwise `Parse` also allocates the trimmed string.

Mixed workload: the catalog user agents parsed in a loop, mean time per user agent.

| Workload | .NET 8.0.34 | .NET 9.0.23 | .NET 10.0.15 | .NET 11.0.0 | Allocated |
| --- | ---: | ---: | ---: | ---: | ---: |
| 44 browser user agents | 754.0 ns | 331.8 ns | 291.7 ns | 262.5 ns | 38 B |
| 48 crawler user agents | 355.5 ns | 159.0 ns | 227.4 ns | 151.5 ns | 0 B |
| All 96 user agents | 609.4 ns | 261.0 ns | 262.3 ns | 216.2 ns | 18 B |

.NET 8 has no `SearchValues<string>`; the parser checks the tokens one by one there.

Single user agents on .NET 11:

| User agent | Mean | Allocated |
| --- | ---: | ---: |
| Chrome on Windows | 248.6 ns | 40 B |
| Firefox on Linux | 373.1 ns | 32 B |
| Chrome on Android | 267.5 ns | 40 B |
| Safari on iPhone | 289.2 ns | 32 B |
| Applebot | 195.5 ns | 0 B |
| Googlebot | 6.98 ns | 0 B |
| Unknown client | 378.2 ns | 0 B |

Startup in a new .NET 11 process (median of 7 runs):

| | Time | Allocated |
| --- | ---: | ---: |
| First `Parse` call | 9.1 ms | 122 KB |
| First use of all `HttpUserAgentStatics` regular expressions (43 platforms, 35 browsers) | 15.7 ms | 120 KB |

### Library comparison on .NET 11

```shell
dotnet run --project perf/HttpUserAgentParser.Benchmarks -c Release -f net11.0 -- --filter "*LibraryComparison*"
```

The ShortRun job (3 iterations) runs every benchmark case in its own process.

```shell
BenchmarkDotNet v0.16.0-preview.2, Windows 10 (10.0.19045.7846/22H2/2022Update)
AMD Ryzen 9 9950X 4.30GHz, 1 CPU, 32 logical and 16 physical cores
Memory: 61,64 GB Total, 31,34 GB Available
.NET SDK 11.0.100
  [Host]   : .NET 11.0.0 (11.0.0, 11.0.26.53018), X64 RyuJIT x86-64-v4
  ShortRun : .NET 11.0.0 (11.0.0, 11.0.26.53018), X64 RyuJIT x86-64-v4

Job=ShortRun  IterationCount=3  LaunchCount=1  
WarmupCount=3  
```

| Method             | Categories | Data         | Mean            | Error            | StdDev         | Ratio     | RatioSD | Gen0     | Gen1     | Gen2     | Allocated  | Alloc Ratio |
|------------------- |----------- |------------- |----------------:|-----------------:|---------------:|----------:|--------:|---------:|---------:|---------:|-----------:|------------:|
| MyCSharp           | Basic      | Chrome Win10 |       304.64 ns |         9.540 ns |       0.523 ns |      1.00 |    0.00 |   0.0029 |        - |        - |       48 B |        1.00 |
| UAParser           | Basic      | Chrome Win10 | 9,491,617.71 ns | 1,334,799.918 ns |  73,164.888 ns | 31,156.50 |  213.09 | 671.8750 | 609.3750 | 109.3750 | 11653583 B |  242,782.98 |
| DeviceDetector.NET | Basic      | Chrome Win10 | 5,911,814.32 ns | 2,450,967.779 ns | 134,345.815 ns | 19,405.70 |  383.00 | 335.9375 | 179.6875 |  46.8750 |  5529304 B |  115,193.83 |
|                    |            |              |                 |                  |                |           |         |          |          |          |            |             |
| MyCSharp           | Basic      | Google-Bot   |       239.51 ns |        12.113 ns |       0.664 ns |      1.00 |    0.00 |        - |        - |        - |          - |          NA |
| UAParser           | Basic      | Google-Bot   | 9,398,484.90 ns | 2,371,656.128 ns | 129,998.476 ns | 39,239.89 |  479.38 | 687.5000 | 625.0000 | 125.0000 | 12010118 B |          NA |
| DeviceDetector.NET | Basic      | Google-Bot   | 7,600,150.00 ns | 1,546,038.456 ns |  84,743.585 ns | 31,731.61 |  315.73 | 656.2500 | 179.6875 |  70.3125 | 10123039 B |          NA |
|                    |            |              |                 |                  |                |           |         |          |          |          |            |             |
| MyCSharp           | Cached     | Chrome Win10 |        18.95 ns |         0.720 ns |       0.039 ns |      1.00 |    0.00 |        - |        - |        - |          - |          NA |
| UAParser           | Cached     | Chrome Win10 |   126,033.06 ns |    11,193.962 ns |     613.579 ns |  6,652.00 |   30.50 |   2.1973 |        - |        - |    37400 B |          NA |
|                    |            |              |                 |                  |                |           |         |          |          |          |            |             |
| MyCSharp           | Cached     | Google-Bot   |        13.42 ns |         1.006 ns |       0.055 ns |      1.00 |    0.00 |        - |        - |        - |          - |          NA |
| UAParser           | Cached     | Google-Bot   |   108,784.53 ns |    19,649.227 ns |   1,077.040 ns |  8,105.83 |   75.26 |   2.6855 |        - |        - |    45832 B |          NA |

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

