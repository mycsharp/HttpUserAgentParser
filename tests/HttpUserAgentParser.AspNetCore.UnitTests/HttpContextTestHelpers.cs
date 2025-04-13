// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.UnitTests;

public static class HttpContextTestHelpers
{
    public static HttpContext GetHttpContext(string userAgent)
    {
        DefaultHttpContext context = new();
        context.Request.Headers["User-Agent"] = userAgent;

        return context;
    }
}
