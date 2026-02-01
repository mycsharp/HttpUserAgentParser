// Copyright © https://myCSharp.de - all rights reserved

namespace MyCSharp.HttpUserAgentParser.Providers;

/// <summary>
/// Defines a contract for parsing HTTP User-Agent strings.
/// </summary>
/// <remarks>
/// Implementations may provide caching or other optimizations.
/// Use <see cref="HttpUserAgentParserDefaultProvider"/> for simple parsing
/// or <see cref="HttpUserAgentParserCachedProvider"/> for in-memory caching.
/// </remarks>
public interface IHttpUserAgentParserProvider
{
    /// <summary>
    /// Parses the specified User-Agent string and returns the parsed information.
    /// </summary>
    /// <param name="userAgent">The HTTP User-Agent header value to parse.</param>
    /// <returns>An <see cref="HttpUserAgentInformation"/> instance containing the parsed data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="userAgent"/> is <see langword="null"/>.</exception>
    HttpUserAgentInformation Parse(string userAgent);
}
