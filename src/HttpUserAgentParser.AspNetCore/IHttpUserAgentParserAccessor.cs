// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;

namespace MyCSharp.HttpUserAgentParser.AspNetCore;

/// <summary>
/// User Agent parser accessor
/// </summary>
public interface IHttpUserAgentParserAccessor
{
    /// <summary>
    /// User agent value
    /// </summary>
    string? GetHttpContextUserAgent(HttpContext httpContext);

    /// <summary>
    /// Returns current <see cref="HttpUserAgentInformation"/>
    /// </summary>
    HttpUserAgentInformation? Get(HttpContext httpContext);
}
