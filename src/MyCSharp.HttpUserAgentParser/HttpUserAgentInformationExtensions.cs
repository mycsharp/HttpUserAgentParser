namespace MyCSharp.HttpUserAgentParser
{
    public static class HttpUserAgentInformationExtensions
    {
        public static bool IsType(this in HttpUserAgentInformation userAgent, HttpUserAgentType type) => userAgent.Type == type;
        public static bool IsRobot(this in HttpUserAgentInformation userAgent) => IsType(userAgent, HttpUserAgentType.Robot);
        public static bool IsBrowser(this in HttpUserAgentInformation userAgent) => IsType(userAgent, HttpUserAgentType.Browser);
        public static bool IsMobile(this in HttpUserAgentInformation userAgent) => userAgent.MobileDeviceType is not null;
    }
}
