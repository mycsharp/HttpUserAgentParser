// Copyright © https://myCSharp.de - all rights reserved

using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests;

public class UserAgentRuleIndexTests
{
    [Fact]
    public void Built_In_Keys_Are_Candidates_At_Either_Alignment()
    {
        string[][] tables =
        [
            HttpUserAgentRules.DefaultRobotSet.Keys, Array.ConvertAll(HttpUserAgentRules.Platforms, static r => r.Token),
            HttpUserAgentRules.MobileKeys, Array.ConvertAll(HttpUserAgentRules.Browsers, static r => r.DetectToken),
        ];

        for (int table = 0; table < tables.Length; table++)
        {
            string[] keys = tables[table];
            for (int i = 0; i < keys.Length; i++)
            {
                foreach (string prefix in new[] { "", " " })
                {
                    string header = prefix + keys[i].ToUpperInvariant();
                    UserAgentRuleIndex.RuleMasks masks = UserAgentRuleIndex.BuiltIn.Find(header);
                    UInt128[] lanes = [masks.Robots, masks.Platforms, masks.Devices, masks.Browsers];
                    int expected = Array.FindIndex(keys, key => header.Contains(key, StringComparison.OrdinalIgnoreCase));
                    Assert.NotEqual(UInt128.Zero, lanes[table] & ((UInt128)1 << (i % 128)));
                    Assert.Equal(expected, UserAgentRuleIndex.FindKey(header, keys, lanes[table]));
                }
            }
        }
    }

    [Fact]
    public void Built_In_Rule_Tables_Fit_In_128_Bits_Without_Aliasing()
    {
        Assert.True(HttpUserAgentRules.DefaultRobotSet.Keys.Length <= 128);
        Assert.True(HttpUserAgentRules.Platforms.Length <= 128);
        Assert.True(HttpUserAgentRules.MobileKeys.Length <= 128);
        Assert.True(HttpUserAgentRules.Browsers.Length <= 128);
    }

    [Fact]
    public void Rule_Tables_Beyond_128_Entries_Match_Exact_Key()
    {
        string[] keys = [.. Enumerable.Range(0, 130).Select(static i => $"RuleKey{i:D3}")];
        UserAgentRuleIndex index = new(keys, [], [], []);

        Assert.Equal(129, UserAgentRuleIndex.FindKey("RuleKey129", keys, index.Find("RuleKey129").Robots));
        Assert.Equal(65, UserAgentRuleIndex.FindKey("RuleKey065", keys, index.Find("RuleKey065").Robots));
        Assert.Equal(-1, UserAgentRuleIndex.FindKey("NoMatchHere", keys, index.Find("NoMatchHere").Robots));
    }

    [Fact]
    public void Long_Headers_Use_All_Rules_At_The_Length_Threshold()
    {
        UserAgentRuleIndex index = new(["Safari"], [], [], []);
        Assert.Equal(UInt128.Zero, index.Find(new string('x', 255)).Robots);
        string header = new string('x', 250) + "Safari";
        UInt128 mask = index.Find(header).Robots;
        Assert.Equal(UInt128.MaxValue, mask);
        Assert.Equal(0, UserAgentRuleIndex.FindKey(header, ["Safari"], mask));
        Assert.Equal(-1, UserAgentRuleIndex.FindKey(new string('x', 260), ["Safari"], UInt128.MaxValue));
    }

    [Fact]
    public void Short_And_Unicode_Keys_Are_Checked_Exactly()
    {
        string[] keys = ["LG", "Café", "Safari"];
        UserAgentRuleIndex index = new(keys, [], [], []);
        foreach (string header in new[] { "L", "lg", "Café", "K Safari/17", "SafariHelper", "Cafe" })
        {
            int expected = Array.FindIndex(keys, key => header.Contains(key, StringComparison.OrdinalIgnoreCase));
            Assert.Equal(expected, UserAgentRuleIndex.FindKey(header, keys, index.Find(header).Robots));
        }
    }
}
