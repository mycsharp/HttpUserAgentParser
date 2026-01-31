// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;
using MyCSharp.HttpUserAgentParser.Providers;

namespace MyCSharp.HttpUserAgentParser.AspNetCore;

/// <summary>
/// User Agent parser accessor. Implements <see cref="IHttpContextAccessor.HttpContext"/>
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="HttpUserAgentParserAccessor"/>
/// </remarks>
public class HttpUserAgentParserAccessor(IHttpUserAgentParserProvider httpUserAgentParser)
    : IHttpUserAgentParserAccessor
{
    private readonly IHttpUserAgentParserProvider _httpUserAgentParser = httpUserAgentParser;

    /// <summary>
    /// User agent of current <see cref="IHttpContextAccessor"/>
    /// </summary>
    /// <example>
    /// <code>
    /// string? userAgent = accessor.GetHttpContextUserAgent(httpContext);
    /// </code>
    /// </example>
    public string? GetHttpContextUserAgent(HttpContext httpContext)
        => httpContext.GetUserAgentString();

    /// <summary>
    /// Returns current <see cref="HttpUserAgentInformation"/> of current <see cref="IHttpContextAccessor"/>
    /// </summary>
    /// <example>
    /// <code>
    /// HttpUserAgentInformation? info = accessor.Get(httpContext);
    /// </code>
    /// </example>
    public HttpUserAgentInformation? Get(HttpContext httpContext)
    {
        string? httpUserAgent = GetHttpContextUserAgent(httpContext);
        if (string.IsNullOrEmpty(httpUserAgent))
        {
            return null;
        }

        return _httpUserAgentParser.Parse(httpUserAgent);
    }
}
