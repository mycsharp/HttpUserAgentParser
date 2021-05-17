// Copyright © myCSharp 2020-2021, all rights reserved

namespace MyCSharp.HttpUserAgentParser
{
    /// <summary>
    /// Extensions for <see cref="HttpUserAgentInformation"/>
    /// </summary>
    public static class HttpUserAgentInformationExtensions
    {
        /// <summary>
        /// returns true <typeparam name="userAgent.Type"></typeparam> is of <param name="type">type</param>
        /// </summary>
        public static bool IsType(this in HttpUserAgentInformation userAgent, HttpUserAgentType type) => userAgent.Type == type;

        /// <summary>
        /// returns true <see cref="HttpUserAgentInformation.Type"/> is a robot
        /// </summary>
        public static bool IsRobot(this in HttpUserAgentInformation userAgent) => IsType(userAgent, HttpUserAgentType.Robot);

        /// <summary>
        /// returns true <see cref="HttpUserAgentInformation.Type"/> is a browser
        /// </summary>
        public static bool IsBrowser(this in HttpUserAgentInformation userAgent) => IsType(userAgent, HttpUserAgentType.Browser);

        /// <summary>
        /// returns true if agent is a mobile device
        /// </summary>
        /// <remarks>checks if <seealso cref="HttpUserAgentInformation.MobileDeviceType"/> is null</remarks>
        public static bool IsMobile(this in HttpUserAgentInformation userAgent) => userAgent.MobileDeviceType is not null;
    }
}
