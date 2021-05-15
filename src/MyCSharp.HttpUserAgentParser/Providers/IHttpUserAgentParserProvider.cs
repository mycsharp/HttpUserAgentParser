namespace MyCSharp.HttpUserAgentParser.Providers
{
    public interface IHttpUserAgentParserProvider
    {
        HttpUserAgentInformation Parse(string userAgent);
    }
}