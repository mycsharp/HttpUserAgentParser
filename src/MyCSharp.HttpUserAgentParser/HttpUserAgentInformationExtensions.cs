namespace MyCSharp.HttpUserAgentParser
{
    public static class HttpUserAgentInformationExtensions
    {
        public static bool IsType(this HttpUserAgentInformation userAgent, HttpUserAgentType type) => userAgent.Type == type;
        public static bool IsRobot(this HttpUserAgentInformation userAgent) => IsType(userAgent, HttpUserAgentType.Robot);
        public static bool IsBrowser(this HttpUserAgentInformation userAgent) => IsType(userAgent, HttpUserAgentType.Browser);
        public static bool IsMobile(this HttpUserAgentInformation userAgent) => userAgent.MobileDeviceType is not null;
    }
}