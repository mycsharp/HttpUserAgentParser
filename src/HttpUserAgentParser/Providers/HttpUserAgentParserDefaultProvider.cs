// Copyright © https://myCSharp.de - all rights reserved

namespace MyCSharp.HttpUserAgentParser.Providers;

/// <summary>
/// Simple parse provider
/// </summary>
public class HttpUserAgentParserDefaultProvider : IHttpUserAgentParserProvider
{
    /// <summary>
    /// returns the result of <see cref="HttpUserAgentParser.Parse"/>
    /// </summary>
    public HttpUserAgentInformation Parse(string userAgent)
        => HttpUserAgentParser.Parse(userAgent);
}
