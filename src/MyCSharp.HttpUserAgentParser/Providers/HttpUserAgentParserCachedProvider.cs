// Copyright © myCSharp 2020-2021, all rights reserved

using System.Collections.Concurrent;

namespace MyCSharp.HttpUserAgentParser.Providers
{
    public class HttpUserAgentParserCachedProvider : IHttpUserAgentParserProvider
    {
        private readonly ConcurrentDictionary<string, HttpUserAgentInformation> _cache = new();

        public HttpUserAgentInformation Parse(string userAgent)
            => _cache.GetOrAdd(userAgent, static ua => HttpUserAgentParser.Parse(ua));
    }
}