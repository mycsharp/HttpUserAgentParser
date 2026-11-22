// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;
using MyCSharp.HttpUserAgentParser.Providers;

namespace MyCSharp.HttpUserAgentParser.AspNetCore;

/// <summary>
/// User Agent parser accessor. Reads the User-Agent header of the given <see cref="HttpContext"/> and parses it
/// with the registered <see cref="IHttpUserAgentParserProvider"/>.
/// </summary>
/// <remarks>
/// The accessor does not depend on <see cref="IHttpContextAccessor"/>: the <see cref="HttpContext"/> is passed explicitly,
/// e.g. from a controller, a minimal API handler or a middleware.
/// <see cref="HttpUserAgentParserContextAccessor"/> additionally parses the current request via <see cref="IHttpContextAccessor"/>.
/// </remarks>
public class HttpUserAgentParserAccessor(IHttpUserAgentParserProvider httpUserAgentParser)
    : IHttpUserAgentParserAccessor
{
    private readonly IHttpUserAgentParserProvider _httpUserAgentParser = httpUserAgentParser;

    /// <summary>
    /// Returns the User-Agent header of <paramref name="httpContext"/>
    /// </summary>
    public string? GetHttpContextUserAgent(HttpContext httpContext)
        => httpContext.GetUserAgentString();

    /// <summary>
    /// Returns the parsed <see cref="HttpUserAgentInformation"/> of <paramref name="httpContext"/>,
    /// or <see langword="null"/> if the request has no User-Agent header or only white space
    /// </summary>
    public HttpUserAgentInformation? Get(HttpContext httpContext)
    {
        string? httpUserAgent = GetHttpContextUserAgent(httpContext);
        if (string.IsNullOrWhiteSpace(httpUserAgent))
        {
            return null;
        }

        return _httpUserAgentParser.Parse(httpUserAgent);
    }
}
