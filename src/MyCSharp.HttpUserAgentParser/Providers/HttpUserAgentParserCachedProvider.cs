using System.Collections.Concurrent;

namespace MyCSharp.HttpUserAgentParser.Providers
{
    public class HttpUserAgentParserCachedProvider : IHttpUserAgentParserProvider
    {
        private readonly ConcurrentDictionary<string, HttpUserAgentInformation> _cache = new();

        public HttpUserAgentInformation Parse(string userAgent)
            => _cache.GetOrAdd(userAgent, HttpUserAgentParser.Parse);
    }
}