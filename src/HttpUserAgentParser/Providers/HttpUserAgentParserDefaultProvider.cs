// Copyright © https://myCSharp.de - all rights reserved

namespace MyCSharp.HttpUserAgentParser.Providers;

/// <summary>
/// Default implementation of <see cref="IHttpUserAgentParserProvider"/> without caching.
/// </summary>
/// <remarks>
/// Each call to <see cref="Parse"/> performs a full parse operation.
/// For repeated parsing of the same user agents, consider using <see cref="HttpUserAgentParserCachedProvider"/>.
/// </remarks>
public class HttpUserAgentParserDefaultProvider : IHttpUserAgentParserProvider
{
    /// <inheritdoc/>
    /// <example>
    /// <code>
    /// IHttpUserAgentParserProvider provider = new HttpUserAgentParserDefaultProvider();
    /// HttpUserAgentInformation info = provider.Parse("Mozilla/5.0 Chrome/90.0.4430.212");
    /// </code>
    /// </example>
    public HttpUserAgentInformation Parse(string userAgent)
        => HttpUserAgentParser.Parse(userAgent);
}
