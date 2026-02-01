# MyCSharp.HttpUserAgentParser

Parsing HTTP User Agents with .NET

## NuGet

| NuGet | Install |
|-|-|
| [![MyCSharp.HttpUserAgentParser](https://img.shields.io/nuget/v/MyCSharp.HttpUserAgentParser.svg?logo=nuget&label=MyCSharp.HttpUserAgentParser)](https://www.nuget.org/packages/MyCSharp.HttpUserAgentParser) | `dotnet add package MyCSharp.HttpUserAgentParser` |
| [![MyCSharp.HttpUserAgentParser.MemoryCache](https://img.shields.io/nuget/v/MyCSharp.HttpUserAgentParser.MemoryCache.svg?logo=nuget&label=MyCSharp.HttpUserAgentParser.MemoryCache)](https://www.nuget.org/packages/MyCSharp.HttpUserAgentParser.MemoryCache) | `dotnet add package MyCSharp.HttpUserAgentParser.MemoryCache` |
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
        options.CacheOptions.SizeLimit = 1024; // default is null (= no limit)
    });
}
```

> `AddHttpUserAgentMemoryCachedParser` registers `HttpUserAgentParserMemoryCachedProvider` as singleton which contains an isolated `MemoryCache` object.

### ASP.NET Core

For ASP.NET Core applications, an accessor pattern (`IHttpUserAgentParserAccessor`) implementation can be registered additionally that independently retrieves the user agent based on the `HttpContextAccessor`. This requires the package [MyCSharp.HttpUserAgentParser.AspNetCore](https://www.nuget.org/packages/MyCSharp.HttpUserAgentParser.AspNetCore)

```csharp
public void ConfigureServices(IServiceCollection services)
{
    services
        .AddHttpUserAgentMemoryCachedParser() // registers Parser, returns HttpUserAgentParserDependencyInjectionOptions
        // or use any other Parser registration like services.AddHttpUserAgentParser<TParser>(); above
        .AddHttpUserAgentParserAccessor(); // registers IHttpUserAgentParserAccessor, uses IHttpUserAgentParserProvider
}
```

Now you can use

```csharp
public void MyMethod(IHttpUserAgentParserAccessor parserAccessor, HttpContext httpContext)
{
    HttpUserAgentInformation? info = parserAccessor.Get(httpContext);
}
```

## Benchmark

```shell
BenchmarkDotNet v0.15.8, Windows 10 (10.0.19045.6691/22H2/2022Update)
AMD Ryzen 9 9950X 4.30GHz, 1 CPU, 32 logical and 16 physical cores
.NET SDK 10.0.101
  [Host]   : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v4
  ShortRun : .NET 10.0.1 (10.0.1, 10.0.125.57005), X64 RyuJIT x86-64-v4

Job=ShortRun  IterationCount=3  LaunchCount=1
WarmupCount=3

| Method             | Categories | Data         | Mean            | Error            | StdDev         | Ratio     | RatioSD | Gen0     | Gen1     | Gen2     | Allocated  | Alloc Ratio |
|------------------- |----------- |------------- |----------------:|-----------------:|---------------:|----------:|--------:|---------:|---------:|---------:|-----------:|------------:|
| MyCSharp           | Basic      | Chrome Win10 |       939.54 ns |       113.807 ns |       6.238 ns |      1.00 |    0.01 |   0.0019 |        - |        - |       48 B |        1.00 |
| UAParser           | Basic      | Chrome Win10 | 9,120,055.21 ns | 2,108,412.449 ns | 115,569.201 ns |  9,707.23 |  120.28 | 671.8750 | 609.3750 | 109.3750 | 11659008 B |  242,896.00 |
| DeviceDetector.NET | Basic      | Chrome Win10 | 5,099,680.21 ns | 5,313,448.322 ns | 291,248.033 ns |  5,428.01 |  270.28 | 296.8750 | 140.6250 |  31.2500 |  5034130 B |  104,877.71 |
|                    |            |              |                 |                  |                |           |         |          |          |          |            |             |
| MyCSharp           | Basic      | Google-Bot   |       226.47 ns |        20.818 ns |       1.141 ns |      1.00 |    0.01 |        - |        - |        - |          - |          NA |
| UAParser           | Basic      | Google-Bot   | 9,007,285.42 ns |   491,694.016 ns |  26,951.408 ns | 39,772.36 |  202.28 | 687.5000 | 640.6250 | 125.0000 | 12015474 B |          NA |
| DeviceDetector.NET | Basic      | Google-Bot   | 6,056,996.61 ns |   567,479.924 ns |  31,105.490 ns | 26,745.13 |  166.88 | 546.8750 | 132.8125 |  23.4375 |  8862491 B |          NA |
|                    |            |              |                 |                  |                |           |         |          |          |          |            |             |
| MyCSharp           | Cached     | Chrome Win10 |        24.59 ns |         2.222 ns |       0.122 ns |      1.00 |    0.01 |        - |        - |        - |          - |          NA |
| UAParser           | Cached     | Chrome Win10 |   162,917.93 ns |    36,544.250 ns |   2,003.114 ns |  6,625.90 |   76.03 |   2.1973 |        - |        - |    37488 B |          NA |
|                    |            |              |                 |                  |                |           |         |          |          |          |            |             |
| MyCSharp           | Cached     | Google-Bot   |        17.42 ns |         1.077 ns |       0.059 ns |      1.00 |    0.00 |        - |        - |        - |          - |          NA |
| UAParser           | Cached     | Google-Bot   |   126,321.45 ns |     3,171.908 ns |     173.863 ns |  7,253.51 |   23.01 |   2.6855 |        - |        - |    45856 B |          NA |
```

## Disclaimer

This library is inspired by [UserAgentService by DannyBoyNg](https://github.com/DannyBoyNg/UserAgentService) and contains optimizations for our requirements on [myCSharp.de](https://mycsharp.de).
We decided to fork the project, because we want a general restructuring with corresponding breaking changes.

## Maintained

by [@BenjaminAbt](https://github.com/BenjaminAbt) and [@gfoidl](https://github.com/gfoidl)

## License

MIT License

Copyright (c) 2021-2026 MyCSharp 

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

