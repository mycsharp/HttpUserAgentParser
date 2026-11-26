// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Primitives;
using MyCSharp.HttpUserAgentParser.AspNetCore.Telemetry;

namespace MyCSharp.HttpUserAgentParser.AspNetCore;

/// <summary>
/// Static extensions for <see cref="HttpContext"/>
/// </summary>
public static class HttpContextExtensions
{
    /// <summary>
    /// Returns the User-Agent header value
    /// </summary>
    /// <remarks>
    /// Uses the strongly typed <see cref="IHeaderDictionary.UserAgent"/> property, which Kestrel serves from a
    /// pre-parsed known-header field instead of a dictionary lookup.
    /// </remarks>
    public static string? GetUserAgentString(this HttpContext httpContext)
    {
        ArgumentNullException.ThrowIfNull(httpContext);

        StringValues value = httpContext.Request.Headers.UserAgent;
        if (value.Count > 0)
        {
            if (HttpUserAgentParserAspNetCoreTelemetry.IsEnabled)
            {
                HttpUserAgentParserAspNetCoreTelemetry.UserAgentPresent();
            }

            return value;
        }

        if (HttpUserAgentParserAspNetCoreTelemetry.IsEnabled)
        {
            HttpUserAgentParserAspNetCoreTelemetry.UserAgentMissing();
        }

        return null;
    }
}
