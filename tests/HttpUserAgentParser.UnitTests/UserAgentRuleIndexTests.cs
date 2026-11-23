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
                    ulong[] lanes = [masks.Robots, masks.Platforms, masks.Devices, masks.Browsers];
                    int expected = Array.FindIndex(keys, key => header.Contains(key, StringComparison.OrdinalIgnoreCase));
                    Assert.NotEqual(0UL, lanes[table] & (1UL << (i % 64)));
                    Assert.Equal(expected, UserAgentRuleIndex.FindKey(header, keys, lanes[table]));
                }
            }
        }
    }

    [Fact]
    public void Long_Headers_Use_All_Rules_At_The_Length_Threshold()
    {
        UserAgentRuleIndex index = new(["Safari"], [], [], []);
        Assert.Equal(0UL, index.Find(new string('x', 255)).Robots);
        string header = new string('x', 250) + "Safari";
        ulong mask = index.Find(header).Robots;
        Assert.Equal(ulong.MaxValue, mask);
        Assert.Equal(0, UserAgentRuleIndex.FindKey(header, ["Safari"], mask));
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
