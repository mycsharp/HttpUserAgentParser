// Copyright © https://myCSharp.de - all rights reserved

namespace MyCSharp.HttpUserAgentParser;

/// <summary>
/// Extensions for <see cref="HttpUserAgentInformation"/>
/// </summary>
public static class HttpUserAgentInformationExtensions
{
    /// <summary>
    /// Determines whether the user agent is of the specified type.
    /// </summary>
    /// <param name="userAgent">The user agent information to check.</param>
    /// <param name="type">The type to compare against.</param>
    /// <returns><see langword="true"/> if the user agent matches the specified type; otherwise, <see langword="false"/>.</returns>
    /// <example>
    /// <code>
    /// HttpUserAgentInformation info = HttpUserAgentInformation.Parse("Mozilla/5.0 Chrome/90.0");
    /// bool isBrowser = info.IsType(HttpUserAgentType.Browser);
    /// </code>
    /// </example>
    public static bool IsType(this in HttpUserAgentInformation userAgent, HttpUserAgentType type) => userAgent.Type == type;

    /// <summary>
    /// Determines whether the user agent is a robot/crawler/bot.
    /// </summary>
    /// <param name="userAgent">The user agent information to check.</param>
    /// <returns><see langword="true"/> if the user agent is a robot; otherwise, <see langword="false"/>.</returns>
    /// <example>
    /// <code>
    /// HttpUserAgentInformation info = HttpUserAgentInformation.Parse("Googlebot/2.1");
    /// bool isRobot = info.IsRobot(); // true
    /// </code>
    /// </example>
    public static bool IsRobot(this in HttpUserAgentInformation userAgent) => IsType(userAgent, HttpUserAgentType.Robot);

    /// <summary>
    /// Determines whether the user agent is a browser.
    /// </summary>
    /// <param name="userAgent">The user agent information to check.</param>
    /// <returns><see langword="true"/> if the user agent is a browser; otherwise, <see langword="false"/>.</returns>
    /// <example>
    /// <code>
    /// HttpUserAgentInformation info = HttpUserAgentInformation.Parse("Mozilla/5.0 Chrome/90.0");
    /// bool isBrowser = info.IsBrowser(); // true
    /// </code>
    /// </example>
    public static bool IsBrowser(this in HttpUserAgentInformation userAgent) => IsType(userAgent, HttpUserAgentType.Browser);

    /// <summary>
    /// Determines whether the user agent represents a mobile device.
    /// </summary>
    /// <param name="userAgent">The user agent information to check.</param>
    /// <returns><see langword="true"/> if the user agent is from a mobile device; otherwise, <see langword="false"/>.</returns>
    /// <remarks>This method checks if <see cref="HttpUserAgentInformation.MobileDeviceType"/> is not <see langword="null"/>.</remarks>
    /// <example>
    /// <code>
    /// HttpUserAgentInformation info = HttpUserAgentInformation.Parse("Mozilla/5.0 (iPhone; CPU iPhone OS 14_5)");
    /// bool isMobile = info.IsMobile(); // true
    /// </code>
    /// </example>
    public static bool IsMobile(this in HttpUserAgentInformation userAgent) => userAgent.MobileDeviceType is not null;
}
