// Copyright © myCSharp 2020-2021, all rights reserved

namespace MyCSharp.HttpUserAgentParser.Providers
{
    public interface IHttpUserAgentParserProvider
    {
        HttpUserAgentInformation Parse(string userAgent);
    }
}