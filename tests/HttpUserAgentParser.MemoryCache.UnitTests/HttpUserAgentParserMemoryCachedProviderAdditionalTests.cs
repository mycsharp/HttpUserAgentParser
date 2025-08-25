// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.Caching.Memory;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.UnitTests;

public class HttpUserAgentParserMemoryCachedProviderAdditionalTests
{
    [Fact]
    public void Options_Defaults_Are_Set()
    {
        HttpUserAgentParserMemoryCachedProviderOptions options = new();
        Assert.NotNull(options.CacheOptions);
        Assert.NotNull(options.CacheEntryOptions);
        Assert.True(options.CacheOptions.SizeLimit is null || options.CacheOptions.SizeLimit >= 0);
        Assert.NotEqual(default, options.CacheEntryOptions.SlidingExpiration);
    }

    [Fact]
    public void Provider_Caches_Entries_And_Resolves_Twice()
    {
        HttpUserAgentParserMemoryCachedProvider provider = new(new HttpUserAgentParserMemoryCachedProviderOptions(new MemoryCacheOptions { SizeLimit = 10 }));
        string ua = "Mozilla/5.0 (Windows NT 10.0) AppleWebKit/537.36 Chrome/120.0.0.0 Safari/537.36";
        HttpUserAgentInformation a = provider.Parse(ua);
        HttpUserAgentInformation b = provider.Parse(ua);

        Assert.Equal(a.Name, b.Name);
        Assert.Equal(a.Version, b.Version);
    }
}
