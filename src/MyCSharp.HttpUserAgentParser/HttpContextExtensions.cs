// Copyright © myCSharp 2020-2021, all rights reserved

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
    public static string? GetUserAgentString(this HttpContext httpContext) =>
         httpContext.Request?.Headers["User-Agent"].ToString()!;
}
