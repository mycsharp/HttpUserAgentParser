// Copyright © myCSharp 2020-2021, all rights reserved

namespace MyCSharp.HttpUserAgentParser
{
    public readonly struct HttpUserAgentInformation
    {
        public string UserAgent { get; }
        public HttpUserAgentType Type { get; }

        public HttpUserAgentPlatformInformation? Platform { get; }
        public string? Name { get; }
        public string? Version { get; }
        public string? MobileDeviceType { get; }

        private HttpUserAgentInformation(string userAgent, HttpUserAgentPlatformInformation? platform, HttpUserAgentType type, string? name, string? version, string? deviceName)
        {
            UserAgent = userAgent;
            Type = type;
            Name = name;
            Platform = platform;
            Version = version;
            MobileDeviceType = deviceName;
        }

        // parse

        public static HttpUserAgentInformation Parse(string userAgent) => HttpUserAgentParser.Parse(userAgent);

        // create factories

        public static HttpUserAgentInformation CreateForRobot(string userAgent, string robotName)
            => new(userAgent, null, HttpUserAgentType.Robot, robotName, null, null);

        public static HttpUserAgentInformation CreateForBrowser(string userAgent, HttpUserAgentPlatformInformation? platform, string? browserName, string? browserVersion, string? deviceName)
            => new(userAgent, platform, HttpUserAgentType.Browser, browserName, browserVersion, deviceName);

        public static HttpUserAgentInformation CreateForUnknown(string userAgent, HttpUserAgentPlatformInformation? platform, string? deviceName)
            => new(userAgent, platform, HttpUserAgentType.Unknown, null, null, deviceName);
    }
}
