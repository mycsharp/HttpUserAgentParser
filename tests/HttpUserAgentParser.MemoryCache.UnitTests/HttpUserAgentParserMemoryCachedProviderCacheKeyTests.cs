// Copyright © https://myCSharp.de - all rights reserved

using Xunit;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.UnitTests;

public class HttpUserAgentParserMemoryCachedProviderCacheKeyTests
{
    private static HttpUserAgentParserMemoryCachedProvider.CacheKey Key(string userAgent)
        => new() { UserAgent = userAgent, Options = new HttpUserAgentParserMemoryCachedProviderOptions() };

    [Fact]
    public void Equals_Ignores_Case()
    {
        Assert.True(Key("Mozilla/5.0 Chrome").Equals(Key("mozilla/5.0 chrome")));
        Assert.True(Key("Mozilla/5.0 Chrome").Equals((object)Key("Mozilla/5.0 Chrome")));
    }

    [Fact]
    public void Not_Equals_Other_User_Agent_Null_Or_Other_Type()
    {
        HttpUserAgentParserMemoryCachedProvider.CacheKey key = Key("Mozilla/5.0 Chrome");

        Assert.False(key.Equals(Key("Mozilla/5.0 Firefox")));
        Assert.False(key.Equals(null));
        Assert.False(key.Equals((object?)null));
        Assert.False(key.Equals("Mozilla/5.0 Chrome"));
    }

    [Fact]
    public void GetHashCode_Is_Ordinal()
    {
        Assert.Equal(Key("Mozilla/5.0 Chrome").GetHashCode(), Key("Mozilla/5.0 Chrome").GetHashCode());
        Assert.Equal("Mozilla/5.0 Chrome".GetHashCode(StringComparison.Ordinal), Key("Mozilla/5.0 Chrome").GetHashCode());
    }
}
