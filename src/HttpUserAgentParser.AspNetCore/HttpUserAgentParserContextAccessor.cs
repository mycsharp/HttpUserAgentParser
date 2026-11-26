// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;
using MyCSharp.HttpUserAgentParser.Providers;

namespace MyCSharp.HttpUserAgentParser.AspNetCore;

/// <summary>
/// User Agent parser accessor for an explicitly passed <see cref="HttpContext"/> and for the current request,
/// which is resolved via <see cref="IHttpContextAccessor"/>.
/// </summary>
public class HttpUserAgentParserContextAccessor(IHttpUserAgentParserProvider httpUserAgentParser, IHttpContextAccessor httpContextAccessor)
    : HttpUserAgentParserAccessor(httpUserAgentParser), IHttpUserAgentParserContextAccessor
{
    private readonly IHttpContextAccessor _httpContextAccessor = httpContextAccessor;

    /// <inheritdoc/>
    public string? GetHttpContextUserAgent()
        => _httpContextAccessor.HttpContext is { } httpContext ? GetHttpContextUserAgent(httpContext) : null;

    /// <inheritdoc/>
    public HttpUserAgentInformation? Get()
        => _httpContextAccessor.HttpContext is { } httpContext ? Get(httpContext) : null;
}
