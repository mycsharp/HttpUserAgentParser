// Copyright © myCSharp 2020-2022, all rights reserved

namespace MyCSharp.HttpUserAgentParser.Providers
{
    /// <summary>
    /// Provides the basic parsing of user agent strings.
    /// </summary>
    public interface IHttpUserAgentParserProvider
    {
        /// <summary>
        /// Parsed the <paramref name="userAgent"/>-string.
        /// </summary>
        /// <param name="userAgent">The user agent to parse.</param>
        /// <returns>The parsed user agent information</returns>
        HttpUserAgentInformation Parse(string userAgent);
    }
}
