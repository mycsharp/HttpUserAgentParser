// Copyright © https://myCSharp.de - all rights reserved

using System.Text.RegularExpressions;

namespace MyCSharp.HttpUserAgentParser;

/// <summary>
/// Information about the user agent platform
/// </summary>
/// <remarks>
/// Creates a new instance of <see cref="HttpUserAgentPlatformInformation"/>
/// </remarks>
public readonly struct HttpUserAgentPlatformInformation(Regex regex, string name, HttpUserAgentPlatformType platformType)
{
    /// <summary>
    /// Regex-pattern that matches this user agent string
    /// </summary>
    public Regex Regex { get; } = regex;

    /// <summary>
    /// Name of the platform
    /// </summary>
    public string Name { get; } = name;

    /// <summary>
    /// Specific platform type aka family
    /// </summary>
    public HttpUserAgentPlatformType PlatformType { get; } = platformType;
}
