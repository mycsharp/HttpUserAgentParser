// Copyright © myCSharp 2020-2021, all rights reserved

namespace MyCSharp.HttpUserAgentParser.Providers
{
    public class HttpUserAgentParserDefaultProvider : IHttpUserAgentParserProvider
    {
        public HttpUserAgentInformation Parse(string userAgent)
            => HttpUserAgentParser.Parse(userAgent);
    }
}