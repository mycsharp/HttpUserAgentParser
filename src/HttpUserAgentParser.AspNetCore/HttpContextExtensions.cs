// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;

namespace MyCSharp.HttpUserAgentParser.AspNetCore;

/// <summary>
/// Extension methods for <see cref="HttpContext"/> to access User-Agent information.
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Gets the User-Agent header value from the HTTP request.
    /// </summary>
    /// <param name="httpContext">The HTTP context.</param>
    /// <returns>The User-Agent string, or <see langword="null"/> if not present.</returns>
    /// <example>
    /// <code>
    /// string? userAgent = httpContext.GetUserAgentString();
    /// if (userAgent != null)
    /// {
    ///     HttpUserAgentInformation info = HttpUserAgentParser.Parse(userAgent);
    /// }
    /// </code>
    /// </example>
    public static string? GetUserAgentString(this HttpContext httpContext)
    {
        if (httpContext.Request.Headers.TryGetValue("User-Agent", out StringValues value))
            return value;

        return null;
    }
}
