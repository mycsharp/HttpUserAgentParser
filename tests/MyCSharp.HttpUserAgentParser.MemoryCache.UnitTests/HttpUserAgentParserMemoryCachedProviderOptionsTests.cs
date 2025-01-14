// Copyright © myCSharp.de - all rights reserved

using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.UnitTests;

public class HttpUserAgentParserMemoryCachedProviderOptionsTests
{
    [Fact]
    public void Ctor()
    {
        MemoryCacheOptions cacheOptions = new();
        MemoryCacheEntryOptions cacheEntryOptions = new();

        HttpUserAgentParserMemoryCachedProviderOptions options = new(cacheOptions, cacheEntryOptions);

        Assert.Equal(cacheOptions, options.CacheOptions);
        Assert.Equal(cacheEntryOptions, options.CacheEntryOptions);
    }

    [Fact]
    public void Ctor_MemoryCacheOptions()
    {
        MemoryCacheOptions cacheOptions = new();

        HttpUserAgentParserMemoryCachedProviderOptions options = new(cacheOptions);

        Assert.Equal(cacheOptions, options.CacheOptions);
        Assert.NotNull(options.CacheEntryOptions);
    }

    [Fact]
    public void Ctor_MemoryCacheEntryOptions()
    {
        MemoryCacheEntryOptions cacheEntryOptions = new();

        HttpUserAgentParserMemoryCachedProviderOptions options = new(cacheEntryOptions);

        Assert.NotNull(options.CacheOptions);
        Assert.Equal(cacheEntryOptions, options.CacheEntryOptions);
    }

    [Fact]
    public void Ctor_Empty()
    {
        HttpUserAgentParserMemoryCachedProviderOptions options = new();

        Assert.NotNull(options.CacheOptions);
        Assert.NotNull(options.CacheEntryOptions);
    }
}
