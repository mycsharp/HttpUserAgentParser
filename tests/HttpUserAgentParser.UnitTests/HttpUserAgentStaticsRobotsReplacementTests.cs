// Copyright © https://myCSharp.de - all rights reserved

using MyCSharp.HttpUserAgentParser.UnitTests.Telemetry;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests;

/// <summary>
/// <see cref="HttpUserAgentStatics.Robots"/> is a public, mutable array that the parser reads directly.
/// Replacing an entry changes process-wide state, so these tests run in the non-parallel collection.
/// </summary>
[Collection(TelemetryCollection.Name)]
public class HttpUserAgentStaticsRobotsReplacementTests
{
    [Fact]
    public void Robots_Are_Unchanged_By_Default()
    {
        Assert.True(HttpUserAgentRules.AreRobotKeysUnchanged());
        Assert.Equal("YouBot", HttpUserAgentParser.GetRobot(HttpUserAgentStatics.Robots[^1].Key));
    }

    [Fact]
    public void GetRobot_UsesReplacementRuleInOriginalOrder()
    {
        int last = HttpUserAgentStatics.Robots.Length - 1;
        (string Key, string Value) original = HttpUserAgentStatics.Robots[last];

        try
        {
            HttpUserAgentStatics.Robots[last] = ("CustomTestCrawler", "Custom crawler");
            Assert.False(HttpUserAgentRules.AreRobotKeysUnchanged());

            Assert.Equal("Custom crawler", HttpUserAgentParser.GetRobot("customtestcrawler/1.0"));
            Assert.Equal("Googlebot", HttpUserAgentParser.GetRobot("customtestcrawler/1.0 googlebot/2.1"));
            Assert.Equal("BingBot", HttpUserAgentParser.GetRobot("Mozilla/5.0 (compatible; bingbot/2.0)"));
            Assert.Null(HttpUserAgentParser.GetRobot(original.Key));
            Assert.Null(HttpUserAgentParser.GetRobot("Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/158.0.0.0"));
        }
        finally
        {
            HttpUserAgentStatics.Robots[last] = original;
        }

        Assert.True(HttpUserAgentRules.AreRobotKeysUnchanged());
    }

    [Fact]
    public void GetRobot_Returns_Replaced_Display_Name()
    {
        int index = Array.FindIndex(HttpUserAgentStatics.Robots, static robot => string.Equals(robot.Key, "bingbot", StringComparison.Ordinal));
        (string Key, string Value) original = HttpUserAgentStatics.Robots[index];

        try
        {
            // Only the display name changes: the keys are unchanged, so the fast path stays active.
            HttpUserAgentStatics.Robots[index] = (original.Key, "Bing crawler");
            Assert.True(HttpUserAgentRules.AreRobotKeysUnchanged());

            Assert.Equal("Bing crawler", HttpUserAgentParser.GetRobot("Mozilla/5.0 (compatible; bingbot/2.0)"));
        }
        finally
        {
            HttpUserAgentStatics.Robots[index] = original;
        }
    }

    [Fact]
    public void GetRobot_Honors_Replaced_First_Entry()
    {
        (string Key, string Value) original = HttpUserAgentStatics.Robots[0];

        try
        {
            HttpUserAgentStatics.Robots[0] = ("ExampleBot", "Example");

            Assert.Equal("Example", HttpUserAgentParser.GetRobot("Mozilla/5.0 (compatible; ExampleBot/1.0)"));
            Assert.Null(HttpUserAgentParser.GetRobot("Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)"));
            Assert.Equal("BingBot", HttpUserAgentParser.GetRobot("Mozilla/5.0 (compatible; bingbot/2.0)"));
        }
        finally
        {
            HttpUserAgentStatics.Robots[0] = original;
        }

        Assert.Equal("Googlebot", HttpUserAgentParser.GetRobot("Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)"));
    }
}
