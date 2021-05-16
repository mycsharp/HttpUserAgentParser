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


        public HttpUserAgentInformation(string userAgent, HttpUserAgentType type, in HttpUserAgentPlatformInformation? platform, string? name, string? version, string? mobileDeviceType)
        {
            UserAgent = userAgent;
            Type = type;
            MobileDeviceType = mobileDeviceType;
            Platform = platform;
            Name = name;
            Version = version;
        }

        public static HttpUserAgentInformation Parse(string userAgent) => HttpUserAgentParser.Parse(userAgent);
    }
}
