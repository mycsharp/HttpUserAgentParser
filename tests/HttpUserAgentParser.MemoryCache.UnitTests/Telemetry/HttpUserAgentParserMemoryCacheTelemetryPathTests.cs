// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using MyCSharp.HttpUserAgentParser.MemoryCache.DependencyInjection;
using MyCSharp.HttpUserAgentParser.MemoryCache.Telemetry;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.UnitTests.Telemetry;

/// <summary>
/// Covers every telemetry state of the memory cached provider explicitly.
/// </summary>
[Collection(TelemetryCollection.Name)]
public sealed class HttpUserAgentParserMemoryCacheTelemetryPathTests : IDisposable
{
    private const string UserAgent = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/158.0.0.0 Safari/537.36";

    public HttpUserAgentParserMemoryCacheTelemetryPathTests() => HttpUserAgentParserMemoryCacheTelemetry.ResetForTests();

    public void Dispose() => HttpUserAgentParserMemoryCacheTelemetry.ResetForTests();

    [Fact]
    public void Disabled_Telemetry_Parses_And_Caches()
    {
        Assert.False(HttpUserAgentParserMemoryCacheTelemetry.IsEnabled);

        AssertParsesAndCaches();
        Assert.Equal(0, HttpUserAgentParserMemoryCacheTelemetryState.CacheSize);
    }

    [Fact]
    public void Counters_Without_Listener_Do_Not_Track_Cache_Size()
    {
        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection()).WithMemoryCacheTelemetry();

        Assert.True(HttpUserAgentParserMemoryCacheTelemetry.IsEnabled);
        Assert.False(HttpUserAgentParserMemoryCacheTelemetry.IsCacheSizeEnabled);

        AssertParsesAndCaches();
        Assert.Equal(0, HttpUserAgentParserMemoryCacheTelemetryState.CacheSize);
    }

    [Fact]
    public void Meters_Track_Cache_Size()
    {
        new HttpUserAgentParserDependencyInjectionOptions(new ServiceCollection()).WithMemoryCacheMeterTelemetry();

        Assert.True(HttpUserAgentParserMemoryCacheTelemetry.IsCacheSizeEnabled);

        AssertParsesAndCaches();
        Assert.Equal(1, HttpUserAgentParserMemoryCacheTelemetryState.CacheSize);
    }

    private static void AssertParsesAndCaches()
    {
        HttpUserAgentParserMemoryCachedProvider provider = new(new HttpUserAgentParserMemoryCachedProviderOptions());

        HttpUserAgentInformation miss = provider.Parse(UserAgent);
        HttpUserAgentInformation hit = provider.Parse(UserAgent);

        Assert.Equal("Chrome", miss.Name);
        Assert.Equal(miss, hit);
    }
}
