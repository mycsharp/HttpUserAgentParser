# MyCSharp.HttpUserAgentParser

Parsing HTTP User Agents with .NET

## NuGet

| NuGet |
|-|
| [![MyCSharp.HttpUserAgentParser](https://img.shields.io/nuget/v/MyCSharp.HttpUserAgentParser.svg?logo=nuget&label=MyCSharp.HttpUserAgentParser)](https://www.nuget.org/packages/MyCSharp.HttpUserAgentParser) |
| [![MyCSharp.HttpUserAgentParser](https://img.shields.io/nuget/v/MyCSharp.HttpUserAgentParser.MemoryCache.svg?logo=nuget&label=MyCSharp.HttpUserAgentParser.MemoryCache)](https://www.nuget.org/packages/MyCSharp.HttpUserAgentParser.MemoryCache)| `dotnet add package MyCSharp.HttpUserAgentParser.MemoryCach.MemoryCache` |
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
public void MyMethod(IHttpUserAgentParserAccessor parserAccessor)
{
    HttpUserAgentInformation info = parserAccessor.Get();
}
```

## Benchmark

```shell
BenchmarkDotNet v0.14.0, Windows 10 (10.0.19045.6216/22H2/2022Update)
AMD Ryzen 9 9950X, 1 CPU, 32 logical and 16 physical cores
.NET SDK 10.0.100-preview.7.25380.108
  [Host]   : .NET 10.0.0 (10.0.25.38108), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI
  ShortRun : .NET 10.0.0 (10.0.25.38108), X64 RyuJIT AVX-512F+CD+BW+DQ+VL+VBMI

Job=ShortRun  IterationCount=3  LaunchCount=1
WarmupCount=3

| Method             | Categories | Data         | Mean            | Error            | StdDev         | Ratio     | RatioSD | Gen0     | Gen1     | Gen2     | Allocated  | Alloc Ratio |
|------------------- |----------- |------------- |----------------:|-----------------:|---------------:|----------:|--------:|---------:|---------:|---------:|-----------:|------------:|
| MyCSharp           | Basic      | Chrome Win10 |       936.44 ns |       131.253 ns |       7.194 ns |      1.00 |    0.01 |   0.0029 |        - |        - |       48 B |        1.00 |
| UAParser           | Basic      | Chrome Win10 | 9,512,347.40 ns | 3,961,045.109 ns | 217,118.249 ns | 10,158.42 |  211.89 | 656.2500 | 546.8750 | 109.3750 | 11523315 B |  240,069.06 |
| DeviceDetector.NET | Basic      | Chrome Win10 | 5,428,530.73 ns | 5,276,988.556 ns | 289,249.550 ns |  5,797.23 |  270.29 | 296.8750 | 125.0000 |  31.2500 |  5002239 B |  104,213.31 |
|                    |            |              |                 |                  |                |           |         |          |          |          |            |             |
| MyCSharp           | Basic      | Google-Bot   |       165.66 ns |        21.926 ns |       1.202 ns |      1.00 |    0.01 |        - |        - |        - |          - |          NA |
| UAParser           | Basic      | Google-Bot   | 9,737,403.12 ns | 2,336,698.462 ns | 128,082.328 ns | 58,781.92 |  764.74 | 671.8750 | 656.2500 | 109.3750 | 11877003 B |          NA |
| DeviceDetector.NET | Basic      | Google-Bot   | 6,331,960.42 ns | 1,602,716.199 ns |  87,850.283 ns | 38,224.23 |  518.30 | 500.0000 |  62.5000 |        - |  8817013 B |          NA |
|                    |            |              |                 |                  |                |           |         |          |          |          |            |             |
| MyCSharp           | Cached     | Chrome Win10 |        26.75 ns |         3.749 ns |       0.205 ns |      1.00 |    0.01 |        - |        - |        - |          - |          NA |
| UAParser           | Cached     | Chrome Win10 |   250,039.55 ns |     6,502.182 ns |     356.407 ns |  9,346.54 |   63.39 |   2.1973 |        - |        - |    37488 B |          NA |
|                    |            |              |                 |                  |                |           |         |          |          |          |            |             |
| MyCSharp           | Cached     | Google-Bot   |        19.66 ns |         4.312 ns |       0.236 ns |      1.00 |    0.01 |        - |        - |        - |          - |          NA |
| UAParser           | Cached     | Google-Bot   |   184,991.85 ns |    46,235.986 ns |   2,534.350 ns |  9,408.77 |  148.82 |   2.6855 |        - |        - |    45857 B |          NA |
```

## Disclaimer

This library is inspired by [UserAgentService by DannyBoyNg](https://github.com/DannyBoyNg/UserAgentService) and contains optimizations for our requirements on [myCSharp.de](https://mycsharp.de).
We decided to fork the project, because we want a general restructuring with corresponding breaking changes.

## Maintained

by [@BenjaminAbt](https://github.com/BenjaminAbt) and [@gfoidl](https://github.com/gfoidl)

## License

MIT License

Copyright (c) 2021-2025 MyCSharp 

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

