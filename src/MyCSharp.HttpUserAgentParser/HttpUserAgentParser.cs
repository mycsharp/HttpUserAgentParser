using System;
using System.Collections.Generic;
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
                return new HttpUserAgentInformation(userAgent, HttpUserAgentType.Robot, null, robotName, null, null);
            }

            HttpUserAgentPlatformInformation? platform = GetPlatform(userAgent);
            string? mobileDeviceType = GetMobileDevice(userAgent);

            if (TryGetBrowser(userAgent, out (string Name, string? Version)? browser))
            {
                return new HttpUserAgentInformation(userAgent, HttpUserAgentType.Browser, platform, browser?.Name, browser?.Version, mobileDeviceType);
            }

            return new HttpUserAgentInformation(userAgent, HttpUserAgentType.Unknown, platform, null, null, mobileDeviceType);
        }

        public static string Cleanup(string userAgent) => userAgent.Trim();

        public static HttpUserAgentPlatformInformation? GetPlatform(string userAgent)
        {
            foreach (HttpUserAgentPlatformInformation item in HttpUserAgentStatics.Platforms)
            {
                if (Regex.IsMatch(userAgent, $"{Regex.Escape(item.Id)}", RegexOptions.IgnoreCase))
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
            foreach (KeyValuePair<Regex, string> item in HttpUserAgentStatics.Browsers)
            {
                Match match = item.Key.Match(userAgent);
                if (match.Success)
                {
                    return (item.Value, match.Groups[1].Value);
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
            foreach (KeyValuePair<string, string> item in HttpUserAgentStatics.Robots)
            {
                if (userAgent.Contains(item.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return item.Value;
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
            foreach (KeyValuePair<string, string> item in HttpUserAgentStatics.Mobiles)
            {
                if (userAgent.Contains(item.Key, StringComparison.OrdinalIgnoreCase))
                {
                    return item.Value;
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
