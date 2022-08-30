// Copyright © myCSharp 2020-2022, all rights reserved

using Microsoft.AspNetCore.Http;

namespace MyCSharp.HttpUserAgentParser;

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
        {
            return value;
        }

        return null;
    }
}
