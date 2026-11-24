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

    /// <inheritdoc/>
    public string? GetHttpContextUserAgent(HttpContext httpContext)
        => httpContext.GetUserAgentString();

    /// <inheritdoc/>
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
