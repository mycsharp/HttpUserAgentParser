// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;

namespace MyCSharp.HttpUserAgentParser.AspNetCore;

/// <summary>
/// Provides access to User-Agent parsing functionality within an ASP.NET Core context.
/// </summary>
public interface IHttpUserAgentParserAccessor
{
    /// <summary>
    /// Gets the User-Agent header value from the specified HTTP context.
    /// </summary>
    /// <param name="httpContext">The HTTP context to extract the User-Agent from.</param>
    /// <returns>The User-Agent string, or <see langword="null"/> if not present.</returns>
    string? GetHttpContextUserAgent(HttpContext httpContext);

    /// <summary>
    /// Parses the User-Agent from the specified HTTP context.
    /// </summary>
    /// <param name="httpContext">The HTTP context to extract and parse the User-Agent from.</param>
    /// <returns>
    /// An <see cref="HttpUserAgentInformation"/> instance if the User-Agent header is present;
    /// otherwise, <see langword="null"/>.
    /// </returns>
    HttpUserAgentInformation? Get(HttpContext httpContext);
}
