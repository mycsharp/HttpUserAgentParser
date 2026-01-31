// Copyright © https://myCSharp.de - all rights reserved

namespace MyCSharp.HttpUserAgentParser;

/// <summary>
/// Extensions for <see cref="HttpUserAgentInformation"/>
/// </summary>
public static class HttpUserAgentInformationExtensions
{
    /// <summary>
    /// Tests if <paramref name="userAgent"/> is of <paramref name="type" />
    /// </summary>
    /// <example>
    /// <code>
    /// bool isBrowser = info.IsType(HttpUserAgentType.Browser);
    /// </code>
    /// </example>
    public static bool IsType(this in HttpUserAgentInformation userAgent, HttpUserAgentType type) => userAgent.Type == type;

    /// <summary>
    /// Tests if <paramref name="userAgent"/> is of type <see cref="HttpUserAgentType.Robot"/>
    /// </summary>
    /// <example>
    /// <code>
    /// bool isRobot = info.IsRobot();
    /// </code>
    /// </example>
    public static bool IsRobot(this in HttpUserAgentInformation userAgent) => IsType(userAgent, HttpUserAgentType.Robot);

    /// <summary>
    /// Tests if <paramref name="userAgent"/> is of type <see cref="HttpUserAgentType.Browser"/>
    /// </summary>
    /// <example>
    /// <code>
    /// bool isBrowser = info.IsBrowser();
    /// </code>
    /// </example>
    public static bool IsBrowser(this in HttpUserAgentInformation userAgent) => IsType(userAgent, HttpUserAgentType.Browser);

    /// <summary>
    /// returns <c>true</c> if agent is a mobile device
    /// </summary>
    /// <remarks>checks if <see cref="HttpUserAgentInformation.MobileDeviceType"/> is null</remarks>
    /// <example>
    /// <code>
    /// bool isMobile = info.IsMobile();
    /// </code>
    /// </example>
    public static bool IsMobile(this in HttpUserAgentInformation userAgent) => userAgent.MobileDeviceType is not null;
}
