// Copyright © myCSharp.de - all rights reserved

using FluentAssertions;
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

        options.CacheOptions.Should().Be(cacheOptions);
        options.CacheEntryOptions.Should().Be(cacheEntryOptions);
    }

    [Fact]
    public void Ctor_MemoryCacheOptions()
    {
        MemoryCacheOptions cacheOptions = new();

        HttpUserAgentParserMemoryCachedProviderOptions options = new(cacheOptions);

        options.CacheOptions.Should().Be(cacheOptions);
        options.CacheEntryOptions.Should().NotBeNull();
    }

    [Fact]
    public void Ctor_MemoryCacheEntryOptions()
    {
        MemoryCacheEntryOptions cacheEntryOptions = new();

        HttpUserAgentParserMemoryCachedProviderOptions options = new(cacheEntryOptions);

        options.CacheOptions.Should().NotBeNull();
        options.CacheEntryOptions.Should().Be(cacheEntryOptions);
    }

    [Fact]
    public void Ctor_Empty()
    {
        HttpUserAgentParserMemoryCachedProviderOptions options = new();

        options.CacheOptions.Should().NotBeNull();
        options.CacheEntryOptions.Should().NotBeNull();
    }
}
