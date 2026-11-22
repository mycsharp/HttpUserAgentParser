// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Primitives;
using MyCSharp.HttpUserAgentParser.MemoryCache.DependencyInjection;
using MyCSharp.HttpUserAgentParser.MemoryCache.Telemetry;
using MyCSharp.HttpUserAgentParser.Providers;
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

    [Fact]
    public void Entries_Of_One_Thread_Are_Hits_On_Another_Thread()
    {
        HttpUserAgentParserMemoryCachedProvider provider = new(new HttpUserAgentParserMemoryCachedProviderOptions());

        RunOnNewThread(() =>
        {
            provider.Parse("Chrome/17");
            provider.Parse("Firefox/12"); // must not change the key of the cached Chrome entry
        });

        HttpUserAgentInformation chrome = default;
        RunOnNewThread(() => chrome = provider.Parse("Chrome/17"));

        Assert.Equal(2, provider.CacheEntryCount);
        Assert.Equal(("Chrome", "17"), (chrome.Name, chrome.Version));
    }

    [Fact]
    public void Keys_Are_Case_Sensitive()
    {
        HttpUserAgentParserMemoryCachedProvider provider = new(new HttpUserAgentParserMemoryCachedProviderOptions());

        // The result contains the original user agent, so a differently cased user agent is a different entry.
        Assert.Equal("Chrome/17", provider.Parse("Chrome/17").UserAgent);
        Assert.Equal("CHROME/17", provider.Parse("CHROME/17").UserAgent);
        Assert.Equal(2, provider.CacheEntryCount);
    }

    [Fact]
    public void Entry_Size_Of_The_Options_Counts_Against_The_Size_Limit()
    {
        using HttpUserAgentParserMemoryCachedProvider sizeOne = new(new HttpUserAgentParserMemoryCachedProviderOptions(new MemoryCacheOptions { SizeLimit = 3 }));
        using HttpUserAgentParserMemoryCachedProvider sizeTwo = new(new HttpUserAgentParserMemoryCachedProviderOptions(
            new MemoryCacheOptions { SizeLimit = 3 }, new MemoryCacheEntryOptions { Size = 2 }));

        foreach (HttpUserAgentParserMemoryCachedProvider provider in new[] { sizeOne, sizeTwo })
        {
            _ = provider.Parse("Chrome/17");
            _ = provider.Parse("Firefox/12");
        }

        Assert.Equal(2, sizeOne.CacheEntryCount);
        Assert.InRange(sizeTwo.CacheEntryCount, 0, 1); // two entries of size 2 exceed the limit of 3
    }

    [Fact]
    public void Expiration_Tokens_And_Eviction_Callbacks_Of_The_Options_Apply()
    {
        using CancellationTokenSource expire = new();
        int evicted = 0;
        MemoryCacheEntryOptions entryOptions = new();
        entryOptions.ExpirationTokens.Add(new CancellationChangeToken(expire.Token));
        entryOptions.RegisterPostEvictionCallback((_, _, _, _) => Interlocked.Increment(ref evicted));
        using HttpUserAgentParserMemoryCachedProvider provider = new(new HttpUserAgentParserMemoryCachedProviderOptions(entryOptions));

        HttpUserAgentInformation first = provider.Parse("Chrome/17");
        Assert.Same(first.Version, provider.Parse("Chrome/17").Version); // hit

        expire.Cancel();

        Assert.NotSame(first.Version, provider.Parse("Chrome/17").Version); // expired: parsed again
        Assert.True(SpinWait.SpinUntil(() => Volatile.Read(ref evicted) >= 1, TimeSpan.FromSeconds(10)));
    }

    [Fact]
    public void Dispose_Disposes_The_Cache_And_Removes_The_Provider_From_The_Cache_Size()
    {
        HttpUserAgentParserMemoryCachedProvider provider = new(new HttpUserAgentParserMemoryCachedProviderOptions());
        _ = provider.Parse("Chrome/17");
        Assert.True(HttpUserAgentParserMemoryCacheTelemetryState.IsRegistered(provider));

        provider.Dispose();
        provider.Dispose(); // a second call has no effect

        Assert.False(HttpUserAgentParserMemoryCacheTelemetryState.IsRegistered(provider));
        Assert.Throws<ObjectDisposedException>(() => provider.Parse("Chrome/17"));
    }

    [Fact]
    public void The_Container_Disposes_The_Provider()
    {
        ServiceCollection services = new();
        services.AddHttpUserAgentMemoryCachedParser();
        ServiceProvider container = services.BuildServiceProvider();
        IHttpUserAgentParserProvider provider = container.GetRequiredService<IHttpUserAgentParserProvider>();
        _ = provider.Parse("Chrome/17");

        container.Dispose();

        Assert.Throws<ObjectDisposedException>(() => provider.Parse("Chrome/17"));
    }

    private static void RunOnNewThread(Action action)
    {
        Thread thread = new(() => action());
        thread.Start();
        thread.Join();
    }
}
