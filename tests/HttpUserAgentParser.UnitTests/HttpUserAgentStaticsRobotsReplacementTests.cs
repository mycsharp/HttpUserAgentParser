// Copyright © https://myCSharp.de - all rights reserved

using System.Collections;
using System.Diagnostics.Metrics;
using MyCSharp.HttpUserAgentParser.Providers;
using MyCSharp.HttpUserAgentParser.Telemetry;
using MyCSharp.HttpUserAgentParser.TestHelpers;
using MyCSharp.HttpUserAgentParser.UnitTests.Telemetry;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests;

/// <summary>
/// <see cref="HttpUserAgentStatics.SetRobots"/> changes process-wide state, so these tests run in the non-parallel collection
/// and restore the default robots afterwards.
/// </summary>
[Collection(TelemetryCollection.Name)]
public sealed class HttpUserAgentStaticsRobotsReplacementTests : IDisposable
{
    private const string Chrome = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/158.0.0.0 Safari/537.36";

    public void Dispose() => HttpUserAgentRules.SetRobotSet(HttpUserAgentRules.DefaultRobotSet);

    [Fact]
    public void Robots_Are_The_Defaults_And_Read_Only()
    {
        Assert.Same(HttpUserAgentRules.DefaultRobotSet, HttpUserAgentRules.RobotSet);
        Assert.Equal(HttpUserAgentStatics.DefaultRobots, HttpUserAgentStatics.Robots);
        Assert.Equal(HttpUserAgentRules.DefaultRobots, HttpUserAgentStatics.Robots);
        Assert.IsNotType<(string Key, string Value)[]>(HttpUserAgentStatics.Robots, exactMatch: false);
        Assert.Throws<NotSupportedException>(() => ((IList<(string Key, string Value)>)HttpUserAgentStatics.Robots)[0] = ("x", "y"));
        Assert.Same(HttpUserAgentStatics.Robots, ((ICollection)HttpUserAgentStatics.Robots).SyncRoot);
    }

    [Fact]
    public void Cached_Provider_Uses_The_Current_Robot_Rules()
    {
        HttpUserAgentParserCachedProvider provider = new();
        Assert.Equal(HttpUserAgentType.Browser, provider.Parse(Chrome).Type);

        HttpUserAgentStatics.SetRobots([("Chrome", "Custom")]);
        Assert.False(provider.HasCacheEntry(Chrome));
        Assert.Equal(0, provider.CacheEntryCount);
        Assert.Equal("Custom", provider.Parse(Chrome).Name);

        HttpUserAgentStatics.SetRobots([]);
        Assert.Equal(HttpUserAgentType.Browser, provider.Parse(Chrome).Type);
        Assert.Equal(1, provider.CacheEntryCount);
    }

    [Fact]
    public void Rule_Change_During_A_Cache_Miss_Keeps_The_Two_Generations_Separate()
    {
        HttpUserAgentParserCachedProvider provider = new();
        using Meter meter = new("robots.concurrent");
        using MeterListener listener = new()
        {
            InstrumentPublished = (instrument, target) =>
            {
                if (instrument.Meter == meter && string.Equals(instrument.Name, "parse.requests", StringComparison.Ordinal))
                {
                    target.EnableMeasurementEvents(instrument);
                }
            },
        };
        int requests = 0;
        listener.SetMeasurementEventCallback<long>((_, _, _, _) =>
        {
            if (++requests == 1)
            {
                HttpUserAgentStatics.SetRobots([("Chrome", "New robot")]);
                Assert.Equal("New robot", provider.Parse(Chrome).Name);
            }
        });
        listener.Start();

        try
        {
            HttpUserAgentParserTelemetry.EnableMeters(meter);
            Assert.Equal("Chrome", provider.Parse(Chrome).Name);
            Assert.Equal("New robot", provider.Parse(Chrome).Name);
            Assert.Equal(2, requests);
        }
        finally
        {
            HttpUserAgentParserTelemetry.ResetForTests();
        }
    }

    [Fact]
    public void SetRobots_Extends_The_Defaults()
    {
        HttpUserAgentStatics.SetRobots([.. HttpUserAgentStatics.DefaultRobots, ("CustomTestCrawler", "Custom crawler")]);

        Assert.Equal("Custom crawler", HttpUserAgentParser.GetRobot("customtestcrawler/1.0"));
        Assert.Equal("Googlebot", HttpUserAgentParser.GetRobot("customtestcrawler/1.0 googlebot/2.1")); // priority order
        Assert.Equal("BingBot", HttpUserAgentParser.GetRobot("Mozilla/5.0 (compatible; bingbot/2.0)"));
        Assert.Null(HttpUserAgentParser.GetRobot(Chrome));
        Assert.Equal(HttpUserAgentType.Robot, HttpUserAgentParser.Parse("customtestcrawler/1.0").Type);
    }

    [Fact]
    public void SetRobots_Replaces_Keys_And_Names()
    {
        HttpUserAgentStatics.SetRobots([("ExampleBot", "Example"), ("bingbot", "Bing crawler")]);

        Assert.Equal("Example", HttpUserAgentParser.GetRobot("Mozilla/5.0 (compatible; ExampleBot/1.0)")); // first entry
        Assert.Equal("Bing crawler", HttpUserAgentParser.GetRobot("Mozilla/5.0 (compatible; bingbot/2.0)"));
        Assert.Null(HttpUserAgentParser.GetRobot("Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)"));
        Assert.Null(HttpUserAgentParser.GetRobot(Chrome));
    }

    [Fact]
    public void Literal_And_Iis_Encoded_Robot_Keys_Respect_Their_Configured_Order()
    {
        HttpUserAgentStatics.SetRobots([("C++Bot", "Literal"), ("SEO Spider", "Encoded"), ("SEO+Spider", "Literal fallback")]);

        Assert.Equal("Literal", HttpUserAgentParser.GetRobot("seo+spider C++Bot"));
        Assert.Equal("Encoded", HttpUserAgentParser.GetRobot("seo+spider"));
        Assert.Equal("Literal", HttpUserAgentParser.GetRobot("c++bot"));
        Assert.Null(HttpUserAgentParser.GetRobot("C  Bot"));
    }

    [Fact]
    public void SetRobots_Accepts_A_Single_Robot_And_No_Robots()
    {
        HttpUserAgentStatics.SetRobots([("ExampleBot", "Example")]);
        Assert.Equal("Example", HttpUserAgentParser.GetRobot("ExampleBot/1.0"));
        Assert.Null(HttpUserAgentParser.GetRobot("Mozilla/5.0 (compatible; bingbot/2.0)"));

        HttpUserAgentStatics.SetRobots([]);
        Assert.Empty(HttpUserAgentStatics.Robots);
        Assert.Null(HttpUserAgentParser.GetRobot("Mozilla/5.0 (compatible; Googlebot/2.1)"));
        Assert.Equal(HttpUserAgentType.Browser, HttpUserAgentParser.Parse(Chrome).Type);
    }

    [Fact]
    public void SetRobots_Keeps_No_Reference_To_The_Input()
    {
        (string Key, string Value)[] robots = [("ExampleBot", "Example")];
        HttpUserAgentStatics.SetRobots(robots);

        robots[0] = ("OtherBot", "Other");

        Assert.Equal("Example", HttpUserAgentParser.GetRobot("ExampleBot/1.0"));
        Assert.Null(HttpUserAgentParser.GetRobot("OtherBot/1.0"));
    }

    [Fact]
    public void SetRobots_With_The_Defaults_Detects_Like_The_Defaults()
    {
        string?[] expected = [.. UserAgentCatalog.All.Select(HttpUserAgentParser.GetRobot)];
        HttpUserAgentStatics.SetRobots([]);

        HttpUserAgentStatics.SetRobots(HttpUserAgentStatics.DefaultRobots);

        Assert.Equal(expected, UserAgentCatalog.All.Select(HttpUserAgentParser.GetRobot), StringComparer.Ordinal);
    }

    [Fact]
    public void SetRobots_Rejects_Invalid_Robots()
    {
        Assert.Throws<ArgumentNullException>(() => HttpUserAgentStatics.SetRobots(null!));
        Assert.Throws<ArgumentException>(() => HttpUserAgentStatics.SetRobots([("", "Empty")]));
        Assert.Throws<ArgumentException>(() => HttpUserAgentStatics.SetRobots([(null!, "Null")]));
        Assert.Throws<ArgumentException>(() => HttpUserAgentStatics.SetRobots([("bot", null!)]));

        Assert.Same(HttpUserAgentRules.DefaultRobotSet, HttpUserAgentRules.RobotSet); // unchanged
    }
}
