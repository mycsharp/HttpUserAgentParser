// Copyright © https://myCSharp.de - all rights reserved

namespace MyCSharp.HttpUserAgentParser.Providers;

/// <summary>
/// Simple parse provider
/// </summary>
public class HttpUserAgentParserDefaultProvider : IHttpUserAgentParserProvider
{
    /// <summary>
    /// returns the result of <see cref="HttpUserAgentParser.Parse(string)"/>
    /// </summary>
    /// <example>
    /// <code>
    /// HttpUserAgentInformation info = provider.Parse("Mozilla/5.0 Chrome/90.0.4430.212");
    /// </code>
    /// </example>
    public HttpUserAgentInformation Parse(string userAgent)
        => HttpUserAgentParser.Parse(userAgent);
}
