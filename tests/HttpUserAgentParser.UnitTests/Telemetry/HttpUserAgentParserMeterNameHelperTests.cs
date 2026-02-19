// Copyright © https://myCSharp.de - all rights reserved

using MyCSharp.HttpUserAgentParser.Telemetry;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.UnitTests.Telemetry;

public class HttpUserAgentParserMeterNameHelperTests
{
    private const string Suffix = "http_user_agent_parser.test";

    [Fact]
    public void GetMeterName_NullPrefix_ReturnsDefaultPrefixedName()
    {
        string result = HttpUserAgentParserMeterNameHelper.GetMeterName(null, Suffix);

        Assert.Equal("mycsharp." + Suffix, result);
    }

    [Fact]
    public void GetMeterName_EmptyPrefix_ReturnsSuffixOnly()
    {
        string result = HttpUserAgentParserMeterNameHelper.GetMeterName(string.Empty, Suffix);

        Assert.Equal(Suffix, result);
    }

    [Fact]
    public void GetMeterName_WhitespaceOnlyPrefix_ReturnsSuffixOnly()
    {
        string result = HttpUserAgentParserMeterNameHelper.GetMeterName("   ", Suffix);

        Assert.Equal(Suffix, result);
    }

    [Fact]
    public void GetMeterName_ValidAlphanumericPrefix_ReturnsPrefixedName()
    {
        string result = HttpUserAgentParserMeterNameHelper.GetMeterName("acme.", Suffix);

        Assert.Equal("acme." + Suffix, result);
    }

    [Fact]
    public void GetMeterName_ValidNumericPrefix_ReturnsPrefixedName()
    {
        string result = HttpUserAgentParserMeterNameHelper.GetMeterName("org123.", Suffix);

        Assert.Equal("org123." + Suffix, result);
    }

    [Theory]
    [InlineData("acme")]      // missing trailing dot
    [InlineData("acme-")]     // hyphen is not alphanumeric
    [InlineData("acme..")]    // two trailing dots (second is not valid in prefix-1 range check)
    [InlineData("ac me.")]    // space is not alphanumeric
    public void GetMeterName_InvalidPrefix_ThrowsArgumentException(string invalidPrefix)
    {
        Assert.Throws<ArgumentException>(() =>
            HttpUserAgentParserMeterNameHelper.GetMeterName(invalidPrefix, Suffix));
    }
}
