// Copyright © myCSharp.de - all rights reserved

using Xunit;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.UnitTests;

public class HttpUserAgentParserMemoryCachedProviderTests
{
    [Fact]
    public void Parse()
    {
        HttpUserAgentParserMemoryCachedProviderOptions cachedProviderOptions = new();
        HttpUserAgentParserMemoryCachedProvider provider = new(cachedProviderOptions);

        // create first
        const string userAgentOne =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36 Edg/90.0.818.62";

        HttpUserAgentInformation infoOne = provider.Parse(userAgentOne);

        Assert.Equal("Edge", infoOne.Name);
        Assert.Equal("90.0.818.62", infoOne.Version);

        // check duplicate

        HttpUserAgentInformation infoDuplicate = provider.Parse(userAgentOne);

        Assert.Equal("Edge", infoDuplicate.Name);
        Assert.Equal("90.0.818.62", infoDuplicate.Version);

        // create second

        const string userAgentTwo = "Mozilla/5.0 (Android 4.4; Tablet; rv:41.0) Gecko/41.0 Firefox/41.0";

        HttpUserAgentInformation infoTwo = provider.Parse(userAgentTwo);

        Assert.Equal("Firefox", infoTwo.Name);
        Assert.Equal("41.0", infoTwo.Version);
    }
}
