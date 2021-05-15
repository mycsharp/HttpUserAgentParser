using Microsoft.AspNetCore.Http;
using MyCSharp.HttpUserAgentParser.Providers;

namespace MyCSharp.HttpUserAgentParser.AspNetCore
{
    public interface IHttpUserAgentParserAccessor
    {
        string HttpContextUserAgent { get; }
        HttpUserAgentInformation ParseFromHttpContext();
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
            _httpContextAccessor?.HttpContext?.Request?.Headers["User-Agent"].ToString()!;

        public HttpUserAgentInformation ParseFromHttpContext()
            => _httpUserAgentParser.Parse(HttpContextUserAgent);
    }
}
