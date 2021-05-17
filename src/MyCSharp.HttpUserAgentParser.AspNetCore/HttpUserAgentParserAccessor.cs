// Copyright © myCSharp 2020-2021, all rights reserved

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
        string HttpContextUserAgent { get; }

        /// <summary>
        /// Returns current <see cref="HttpUserAgentInformation"/>
        /// </summary>
        HttpUserAgentInformation Get();
    }

    /// <summary>
    /// User Agent parser accessor. Implements <see cref="IHttpContextAccessor.HttpContext"/>
    /// </summary>
    public class HttpUserAgentParserAccessor : IHttpUserAgentParserAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpUserAgentParserProvider _httpUserAgentParser;

        /// <summary>
        /// Creates a new instance of <see cref="HttpUserAgentParserAccessor"/>
        /// </summary>
        public HttpUserAgentParserAccessor(IHttpContextAccessor httpContextAccessor, IHttpUserAgentParserProvider httpUserAgentParser)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpUserAgentParser = httpUserAgentParser;
        }

        /// <summary>
        /// User agent of current <see cref="IHttpContextAccessor"/>
        /// </summary>
        public string HttpContextUserAgent =>
            _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString()!;

        /// <summary>
        /// Returns current <see cref="HttpUserAgentInformation"/> of current <see cref="IHttpContextAccessor"/>
        /// </summary>
        public HttpUserAgentInformation Get()
            => _httpUserAgentParser.Parse(this.HttpContextUserAgent);
    }
}
