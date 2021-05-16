using System;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;

namespace MyCSharp.HttpUserAgentParser
{
    public static class HttpUserAgentParser
    {
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

        public static string Cleanup(string userAgent) => userAgent.Trim();

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

        public static bool TryGetPlatform(string userAgent, [NotNullWhen(true)] out HttpUserAgentPlatformInformation? platform)
        {
            platform = GetPlatform(userAgent);
            return platform is not null;
        }

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

        public static bool TryGetBrowser(string userAgent, [NotNullWhen(true)] out (string Name, string? Version)? browser)
        {
            browser = GetBrowser(userAgent);
            return browser is not null;
        }

        public static string? GetRobot(string userAgent)
        {
            foreach ((string key, string value) in HttpUserAgentStatics.Robots)
            {
                if (userAgent.Contains(key, StringComparison.OrdinalIgnoreCase))
                {
                    return value;
                }
            }

            return null;
        }

        public static bool TryGetRobot(string userAgent, [NotNullWhen(true)] out string? robotName)
        {
            robotName = GetRobot(userAgent);
            return robotName is not null;
        }

        public static string? GetMobileDevice(string userAgent)
        {
            foreach ((string key, string value) in HttpUserAgentStatics.Mobiles)
            {
                if (userAgent.Contains(key, StringComparison.OrdinalIgnoreCase))
                {
                    return value;
                }
            }

            return null;
        }
        
        public static bool TryGetMobileDevice(string userAgent, [NotNullWhen(true)] out string? device)
        {
            device = GetMobileDevice(userAgent);
            return device is not null;
        }
    }
}
