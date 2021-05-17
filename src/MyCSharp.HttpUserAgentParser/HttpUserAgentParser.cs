// Copyright © myCSharp 2020-2021, all rights reserved

using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace MyCSharp.HttpUserAgentParser
{
    /// <summary>
    /// Parser logic for user agents
    /// </summary>
    public static class HttpUserAgentParser
    {
        /// <summary>
        /// Parses given <param name="userAgent">user agent</param>
        /// </summary>
        public static HttpUserAgentInformation Parse(string userAgent)
        {
            // prepare
            userAgent = Cleanup(userAgent);

            // analyze
            if (TryGetRobot(userAgent, out string? robotName))
            {
                return HttpUserAgentInformation.CreateForRobot(userAgent, robotName);
            }

            HttpUserAgentPlatformInformation? platform = GetPlatform(userAgent);
            string? mobileDeviceType = GetMobileDevice(userAgent);

            if (TryGetBrowser(userAgent, out (string Name, string? Version)? browser))
            {
                return HttpUserAgentInformation.CreateForBrowser(userAgent, platform, browser?.Name, browser?.Version, mobileDeviceType);
            }

            return HttpUserAgentInformation.CreateForUnknown(userAgent, platform, mobileDeviceType);
        }

        /// <summary>
        /// pre-cleanup of <param name="userAgent">user agent</param>
        /// </summary>
        public static string Cleanup(string userAgent) => userAgent.Trim();

        /// <summary>
        /// returns the platform or null
        /// </summary>
        public static HttpUserAgentPlatformInformation? GetPlatform(string userAgent)
        {
            foreach (HttpUserAgentPlatformInformation item in HttpUserAgentStatics.Platforms)
            {
                if (item.Regex.IsMatch(userAgent))
                {
                    return item;
                }
            }

            return null;
        }

        /// <summary>
        /// returns true if platform was found
        /// </summary>
        public static bool TryGetPlatform(string userAgent, [NotNullWhen(true)] out HttpUserAgentPlatformInformation? platform)
        {
            platform = GetPlatform(userAgent);
            return platform is not null;
        }

        /// <summary>
        /// returns the browser or null
        /// </summary>
        public static (string Name, string? Version)? GetBrowser(string userAgent)
        {
            foreach ((Regex key, string? value) in HttpUserAgentStatics.Browsers)
            {
                Match match = key.Match(userAgent);
                if (match.Success)
                {
                    return (value, match.Groups[1].Value);
                }
            }

            return null;
        }

        /// <summary>
        /// returns true if browser was found
        /// </summary>
        public static bool TryGetBrowser(string userAgent, [NotNullWhen(true)] out (string Name, string? Version)? browser)
        {
            browser = GetBrowser(userAgent);
            return browser is not null;
        }

        /// <summary>
        /// returns the robot or null
        /// </summary>
        public static string? GetRobot(string userAgent)
        {
            foreach ((string key, string value) in HttpUserAgentStatics.Robots)
            {
#if NETSTANDARD2_0
                if (userAgent.Contains(key))
#else
                if (userAgent.Contains(key, StringComparison.OrdinalIgnoreCase))
#endif
                {
                    return value;
                }
            }

            return null;
        }

        /// <summary>
        /// returns true if robot was found
        /// </summary>
        public static bool TryGetRobot(string userAgent, [NotNullWhen(true)] out string? robotName)
        {
            robotName = GetRobot(userAgent);
            return robotName is not null;
        }

        /// <summary>
        /// returns the device or null
        /// </summary>
        public static string? GetMobileDevice(string userAgent)
        {
            foreach ((string key, string value) in HttpUserAgentStatics.Mobiles)
            {
#if NETSTANDARD2_0
                if (userAgent.Contains(key))
#else
                if (userAgent.Contains(key, StringComparison.OrdinalIgnoreCase))
#endif
                {
                    return value;
                }
            }

            return null;
        }

        /// <summary>
        /// returns true if device was found
        /// </summary>
        public static bool TryGetMobileDevice(string userAgent, [NotNullWhen(true)] out string? device)
        {
            device = GetMobileDevice(userAgent);
            return device is not null;
        }
    }
}
