// Copyright © https://myCSharp.de - all rights reserved

using MyCSharp.HttpUserAgentParser.Providers;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests.Providers;

public class HttpUserAgentParserCachedProviderTests
{
    [Fact]
    public void Parse()
    {
        HttpUserAgentParserCachedProvider provider = new();

        Assert.Equal(0, provider.CacheEntryCount);

        // create first
        const string userAgentOne =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36 Edg/90.0.818.62";

        HttpUserAgentInformation infoOne = provider.Parse(userAgentOne);

        Assert.Equal("Edge", infoOne.Name);
        Assert.Equal("90.0.818.62", infoOne.Version);

        Assert.Equal(1, provider.CacheEntryCount);
        Assert.True(provider.HasCacheEntry(userAgentOne));

        // check duplicate

        HttpUserAgentInformation infoDuplicate = provider.Parse(userAgentOne);

        Assert.Equal("Edge", infoDuplicate.Name);
        Assert.Equal("90.0.818.62", infoDuplicate.Version);

        Assert.Equal(1, provider.CacheEntryCount);
        Assert.True(provider.HasCacheEntry(userAgentOne));

        // create second

        const string userAgentTwo = "Mozilla/5.0 (Android 4.4; Tablet; rv:41.0) Gecko/41.0 Firefox/41.0";

        HttpUserAgentInformation infoTwo = provider.Parse(userAgentTwo);

        Assert.Equal("Firefox", infoTwo.Name);
        Assert.Equal("41.0", infoTwo.Version);

        Assert.Equal(2, provider.CacheEntryCount);
        Assert.True(provider.HasCacheEntry(userAgentTwo));
    }

    [Fact]
    public void Keys_Are_Case_Sensitive()
    {
        HttpUserAgentParserCachedProvider provider = new();

        // The result contains the original user agent, so a differently cased user agent is a different entry.
        Assert.Equal("Chrome/17", provider.Parse("Chrome/17").UserAgent);
        Assert.Equal("CHROME/17", provider.Parse("CHROME/17").UserAgent);
        Assert.Equal(2, provider.CacheEntryCount);
        Assert.False(provider.HasCacheEntry("chrome/17"));
    }

    [Fact]
    public void Default_Constructor_Uses_DefaultMaxCacheEntries()
    {
        HttpUserAgentParserCachedProvider provider = new();

        Assert.Equal(HttpUserAgentParserCachedProvider.DefaultMaxCacheEntries, provider.MaxCacheEntries);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void Constructor_Throws_When_MaxCacheEntries_Is_Not_Positive(int maxCacheEntries)
    {
        Assert.Throws<ArgumentOutOfRangeException>(() => new HttpUserAgentParserCachedProvider(maxCacheEntries));
    }

    [Fact]
    public void Cache_Rotates_When_MaxCacheEntries_Is_Reached()
    {
        HttpUserAgentParserCachedProvider provider = new(maxCacheEntries: 2);

        Assert.Equal(2, provider.MaxCacheEntries);

        _ = provider.Parse("Chrome/1");
        _ = provider.Parse("Chrome/2");
        Assert.Equal(2, provider.CacheEntryCount);
        Assert.True(provider.HasCacheEntry("Chrome/1"));
        Assert.True(provider.HasCacheEntry("Chrome/2"));

        // Cache hit does not rotate the cache.
        Assert.Equal("1", provider.Parse("Chrome/1").Version);
        Assert.Equal(2, provider.CacheEntryCount);

        // Next miss exceeds the limit and rotates the cache.
        Assert.Equal("3", provider.Parse("Chrome/3").Version);
        Assert.Equal(1, provider.CacheEntryCount);
        Assert.True(provider.HasCacheEntry("Chrome/3"));
        Assert.False(provider.HasCacheEntry("Chrome/1"));
        Assert.False(provider.HasCacheEntry("Chrome/2"));
    }

    [Fact]
    public void Parse_Throws_ArgumentNullException_When_UserAgent_Is_Null()
    {
        HttpUserAgentParserCachedProvider provider = new();

        Assert.Throws<ArgumentNullException>(() => provider.Parse(null!));
        Assert.Throws<ArgumentNullException>(() => provider.HasCacheEntry(null!));
    }
}
