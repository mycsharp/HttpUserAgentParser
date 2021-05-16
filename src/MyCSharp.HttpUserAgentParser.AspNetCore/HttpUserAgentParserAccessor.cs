// Copyright © myCSharp 2020-2021, all rights reserved

using Microsoft.AspNetCore.Http;
using MyCSharp.HttpUserAgentParser.Providers;

namespace MyCSharp.HttpUserAgentParser.AspNetCore
{
    public interface IHttpUserAgentParserAccessor
    {
        string HttpContextUserAgent { get; }
        HttpUserAgentInformation Get();
    }

    public class HttpUserAgentParserAccessor : IHttpUserAgentParserAccessor
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IHttpUserAgentParserProvider _httpUserAgentParser;

        public HttpUserAgentParserAccessor(IHttpContextAccessor httpContextAccessor, IHttpUserAgentParserProvider httpUserAgentParser)
        {
            _httpContextAccessor = httpContextAccessor;
            _httpUserAgentParser = httpUserAgentParser;
        }

        public string HttpContextUserAgent =>
            _httpContextAccessor.HttpContext?.Request?.Headers["User-Agent"].ToString()!;

        public HttpUserAgentInformation Get()
            => _httpUserAgentParser.Parse(this.HttpContextUserAgent);
    }
}
