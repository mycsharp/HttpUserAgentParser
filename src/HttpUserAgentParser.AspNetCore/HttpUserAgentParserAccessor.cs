// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;
using MyCSharp.HttpUserAgentParser.Providers;

namespace MyCSharp.HttpUserAgentParser.AspNetCore;

/// <summary>
/// Default implementation of <see cref="IHttpUserAgentParserAccessor"/> for ASP.NET Core applications.
/// </summary>
/// <remarks>
/// Extracts and parses the User-Agent header from HTTP requests.
/// Register via <c>services.AddHttpUserAgentParser().AddHttpUserAgentParserAccessor()</c>.
/// </remarks>
/// <param name="httpUserAgentParser">The parser provider to use for parsing.</param>
public class HttpUserAgentParserAccessor(IHttpUserAgentParserProvider httpUserAgentParser)
    : IHttpUserAgentParserAccessor
{
    private readonly IHttpUserAgentParserProvider _httpUserAgentParser = httpUserAgentParser;

    /// <inheritdoc/>
    /// <example>
    /// <code>
    /// string? userAgent = accessor.GetHttpContextUserAgent(httpContext);
    /// </code>
    /// </example>
    public string? GetHttpContextUserAgent(HttpContext httpContext)
        => httpContext.GetUserAgentString();

    /// <inheritdoc/>
    /// <example>
    /// <code>
    /// HttpUserAgentInformation? info = accessor.Get(httpContext);
    /// if (info != null)
    /// {
    ///     Console.WriteLine(info.Value.Name);
    /// }
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
