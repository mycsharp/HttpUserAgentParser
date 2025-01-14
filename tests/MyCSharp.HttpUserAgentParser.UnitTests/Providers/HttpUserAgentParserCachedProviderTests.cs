// Copyright © myCSharp.de - all rights reserved

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
}
