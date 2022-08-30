// Copyright © myCSharp 2020-2022, all rights reserved

using System.Text.RegularExpressions;

namespace MyCSharp.HttpUserAgentParser
{
    /// <summary>
    /// Information about the user agent platform
    /// </summary>
    public readonly struct HttpUserAgentPlatformInformation
    {
        /// <summary>
        /// Regex-pattern that matches this user agent string
        /// </summary>
        public Regex Regex { get; }

        /// <summary>
        /// Name of the platform
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Specific platform type aka family
        /// </summary>
        public HttpUserAgentPlatformType PlatformType { get; }

        /// <summary>
        /// Creates a new instance of <see cref="HttpUserAgentPlatformInformation"/>
        /// </summary>
        public HttpUserAgentPlatformInformation(Regex regex, string name, HttpUserAgentPlatformType platformType)
        {
            Regex = regex;
            Name = name;
            PlatformType = platformType;
        }
    }
}
