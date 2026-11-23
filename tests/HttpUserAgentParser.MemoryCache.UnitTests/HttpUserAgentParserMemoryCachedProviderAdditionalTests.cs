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
    public async Task Entries_Of_One_Thread_Are_Hits_On_Another_Thread()
    {
        using HttpUserAgentParserMemoryCachedProvider provider = new(new HttpUserAgentParserMemoryCachedProviderOptions());

        HttpUserAgentInformation first = await OnDedicatedThread(() =>
        {
            HttpUserAgentInformation chrome = provider.Parse("Chrome/17");
            _ = provider.Parse("Firefox/12");
            return chrome;
        });

        HttpUserAgentInformation hit = await OnDedicatedThread(() => provider.Parse("Chrome/17"));

        Assert.Equal(2, provider.CacheEntryCount);
        Assert.Equal(("Chrome", "17"), (hit.Name, hit.Version));
        Assert.Same(first.Version, hit.Version);
    }

    [Fact]
    public void Keys_Are_Case_Sensitive()
    {
        using HttpUserAgentParserMemoryCachedProvider provider = new(new HttpUserAgentParserMemoryCachedProviderOptions());

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

    private static Task<T> OnDedicatedThread<T>(Func<T> action)
        => Task.Factory.StartNew(action, TestContext.Current.CancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
}
