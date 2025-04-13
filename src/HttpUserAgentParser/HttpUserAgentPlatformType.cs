// Copyright © https://myCSharp.de - all rights reserved

namespace MyCSharp.HttpUserAgentParser;

/// <summary>
/// Platform types
/// </summary>
public enum HttpUserAgentPlatformType : byte
{
    /// <summary>
    /// Unknown / not mapped
    /// </summary>
    Unknown = 0,
    /// <summary>
    /// Generic
    /// </summary>
    Generic,
    /// <summary>
    /// Windows
    /// </summary>
    Windows,
    /// <summary>
    /// Linux
    /// </summary>
    Linux,
    /// <summary>
    /// Unix
    /// </summary>
    Unix,
    /// <summary>
    /// Apple iOS
    /// </summary>
    IOS,
    /// <summary>
    /// MacOS
    /// </summary>
    MacOS,
    /// <summary>
    /// BlackBerry
    /// </summary>
    BlackBerry,
    /// <summary>
    /// Android
    /// </summary>
    Android,
    /// <summary>
    /// Symbian
    /// </summary>
    Symbian,
    /// <summary>
    /// ChromeOS
    /// </summary>
    ChromeOS
}
