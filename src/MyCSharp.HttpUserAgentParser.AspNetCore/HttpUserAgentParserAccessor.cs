// Copyright © myCSharp 2020-2022, all rights reserved

using Microsoft.AspNetCore.Http;
using MyCSharp.HttpUserAgentParser.Providers;

namespace MyCSharp.HttpUserAgentParser.AspNetCore
{
    /// <summary>
    /// User Agent parser accessor
    /// </summary>
    public interface IHttpUserAgentParserAccessor
    {
        /// <summary>
        /// User agent value
        /// </summary>
        string? GetHttpContextUserAgent(HttpContext httpContext);

        /// <summary>
        /// Returns current <see cref="HttpUserAgentInformation"/>
        /// </summary>
        HttpUserAgentInformation? Get(HttpContext httpContext);
    }

    /// <summary>
    /// User Agent parser accessor. Implements <see cref="IHttpContextAccessor.HttpContext"/>
    /// </summary>
    public class HttpUserAgentParserAccessor : IHttpUserAgentParserAccessor
    {
        private readonly IHttpUserAgentParserProvider _httpUserAgentParser;

        /// <summary>
        /// Creates a new instance of <see cref="HttpUserAgentParserAccessor"/>
        /// </summary>
        public HttpUserAgentParserAccessor(IHttpUserAgentParserProvider httpUserAgentParser)
        {
            _httpUserAgentParser = httpUserAgentParser;
        }

        /// <summary>
        /// User agent of current <see cref="IHttpContextAccessor"/>
        /// </summary>
        public string? GetHttpContextUserAgent(HttpContext httpContext)
            => httpContext.GetUserAgentString();

        /// <summary>
        /// Returns current <see cref="HttpUserAgentInformation"/> of current <see cref="IHttpContextAccessor"/>
        /// </summary>
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
}
