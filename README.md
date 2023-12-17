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

```sh
BenchmarkDotNet=v0.12.1, OS=Windows 10.0.19042
AMD Ryzen 9 5950X, 1 CPU, 32 logical and 16 physical cores
.NET Core SDK=5.0.300-preview.21228.15
  [Host]     : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT
  DefaultJob : .NET Core 5.0.5 (CoreCLR 5.0.521.16609, CoreFX 5.0.521.16609), X64 RyuJIT
```

|              Method |        Mean |     Error |    StdDev |   Gen 0 |  Gen 1 | Gen 2 | Allocated |
|-------------------- |------------:|----------:|----------:|--------:|-------:|------:|----------:|
|         'UA Parser' | 3,238.59 us | 27.435 us | 25.663 us |  7.8125 |      - |     - |  168225 B |
|    UserAgentService |   391.11 us |  5.126 us |  4.795 us | 35.1563 | 3.4180 |     - |  589664 B |
| HttpUserAgentParser |    67.07 us |  0.740 us |  0.693 us |       - |      - |     - |     848 B |

More benchmark results can be found [in this comment](https://github.com/mycsharp/HttpUserAgentParser/issues/2#issuecomment-842188532).

## Disclaimer

This library is inspired by [UserAgentService by DannyBoyNg](https://github.com/DannyBoyNg/UserAgentService) and contains optimizations for our requirements on [myCSharp.de](https://mycsharp.de).
We decided to fork the project, because we want a general restructuring with corresponding breaking changes.

## Maintained

by [@BenjaminAbt](https://github.com/BenjaminAbt) and [@gfoidl](https://github.com/gfoidl)

## License

MIT License

Copyright (c) 2021-2023 MyCSharp 

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

