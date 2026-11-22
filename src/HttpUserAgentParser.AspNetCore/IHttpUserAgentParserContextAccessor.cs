// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;

namespace MyCSharp.HttpUserAgentParser.AspNetCore;

/// <summary>
/// User Agent parser accessor that offers both ways to get the user agent of a request:
/// pass the <see cref="HttpContext"/> explicitly (inherited from <see cref="IHttpUserAgentParserAccessor"/>),
/// or use the current request resolved via <see cref="IHttpContextAccessor"/>.
/// </summary>
/// <remarks>
/// <see cref="IHttpContextAccessor"/> has a small cost on every request. Prefer <see cref="IHttpUserAgentParserAccessor"/>
/// when the <see cref="HttpContext"/> is at hand, e.g. in controllers, minimal APIs and middlewares.
/// </remarks>
public interface IHttpUserAgentParserContextAccessor : IHttpUserAgentParserAccessor
{
    /// <summary>
    /// Returns the User-Agent header of the current request,
    /// or <see langword="null"/> outside of a request or without User-Agent header
    /// </summary>
    string? GetHttpContextUserAgent();

    /// <summary>
    /// Returns the parsed <see cref="HttpUserAgentInformation"/> of the current request,
    /// or <see langword="null"/> outside of a request or without User-Agent header (or with only white space)
    /// </summary>
    HttpUserAgentInformation? Get();
}
