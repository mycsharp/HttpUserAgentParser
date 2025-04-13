// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace MyCSharp.HttpUserAgentParser.AspNetCore;

/// <summary>
/// Static extensions for <see cref="HttpContext"/>
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Returns the User-Agent header value
    /// </summary>
    public static string? GetUserAgentString(this HttpContext httpContext)
    {
        if (httpContext.Request.Headers.TryGetValue("User-Agent", out StringValues value))
            return value;

        return null;
    }
}
