namespace MyCSharp.HttpUserAgentParser.Providers
{
    public class HttpUserAgentParserDefaultProvider : IHttpUserAgentParserProvider
    {
        public HttpUserAgentInformation Parse(string userAgent)
            => HttpUserAgentParser.Parse(userAgent);
    }
}