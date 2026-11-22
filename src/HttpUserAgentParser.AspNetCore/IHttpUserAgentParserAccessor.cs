// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;

namespace MyCSharp.HttpUserAgentParser.AspNetCore;

/// <summary>
/// User Agent parser accessor for an explicitly passed <see cref="HttpContext"/>
/// </summary>
/// <remarks>
/// <see cref="IHttpUserAgentParserContextAccessor"/> additionally parses the current request via <see cref="IHttpContextAccessor"/>.
/// </remarks>
public interface IHttpUserAgentParserAccessor
{
    /// <summary>
    /// Returns the User-Agent header of <paramref name="httpContext"/>, or <see langword="null"/> without User-Agent header
    /// </summary>
    string? GetHttpContextUserAgent(HttpContext httpContext);

    /// <summary>
    /// Returns the parsed <see cref="HttpUserAgentInformation"/> of <paramref name="httpContext"/>,
    /// or <see langword="null"/> without User-Agent header
    /// </summary>
    HttpUserAgentInformation? Get(HttpContext httpContext);
}
