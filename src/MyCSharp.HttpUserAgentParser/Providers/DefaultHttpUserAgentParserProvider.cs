namespace MyCSharp.HttpUserAgentParser.Providers
{
    public class DefaultHttpUserAgentParserProvider : IHttpUserAgentParserProvider
    {
        public HttpUserAgentInformation Parse(string userAgent)
            => HttpUserAgentParser.Parse(userAgent);
    }
}