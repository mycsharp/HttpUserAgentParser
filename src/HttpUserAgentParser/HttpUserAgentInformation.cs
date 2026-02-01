// Copyright © https://myCSharp.de - all rights reserved

namespace MyCSharp.HttpUserAgentParser;

/// <summary>
/// Represents parsed information from an HTTP User-Agent string, including browser, platform, and device details.
/// </summary>
/// <remarks>
/// This is an immutable value type. Use <see cref="Parse"/> or <see cref="HttpUserAgentParser.Parse"/> to create instances.
/// </remarks>
public readonly struct HttpUserAgentInformation
{
    /// <summary>
    /// Gets the original User-Agent string that was parsed.
    /// </summary>
    public string UserAgent { get; }

    /// <summary>
    /// Gets the type of the user agent (Browser, Robot, or Unknown).
    /// </summary>
    /// <seealso cref="HttpUserAgentType"/>
    public HttpUserAgentType Type { get; }

    /// <summary>
    /// Gets the platform information, or <see langword="null"/> if no platform was detected.
    /// </summary>
    /// <seealso cref="HttpUserAgentPlatformInformation"/>
    public HttpUserAgentPlatformInformation? Platform { get; }

    /// <summary>
    /// Gets the browser or robot name (e.g., "Chrome", "Firefox", "Googlebot"),
    /// or <see langword="null"/> if not detected.
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Gets the browser or robot version (e.g., "90.0.4430.212"),
    /// or <see langword="null"/> if not detected.
    /// </summary>
    public string? Version { get; }

    /// <summary>
    /// Gets the mobile device type (e.g., "Apple iPhone", "Android"),
    /// or <see langword="null"/> if not a mobile device.
    /// </summary>
    public string? MobileDeviceType { get; }

    /// <summary>
    /// Creates a new instance of <see cref="HttpUserAgentInformation"/>
    /// </summary>
    private HttpUserAgentInformation(string userAgent, HttpUserAgentPlatformInformation? platform, HttpUserAgentType type, string? name, string? version, string? deviceName)
    {
        UserAgent = userAgent;
        Type = type;
        Name = name;
        Platform = platform;
        Version = version;
        MobileDeviceType = deviceName;
    }

    /// <summary>
    /// Parses the specified User-Agent string and returns parsed information.
    /// </summary>
    /// <param name="userAgent">The HTTP User-Agent header value to parse.</param>
    /// <returns>An <see cref="HttpUserAgentInformation"/> instance containing the parsed data.</returns>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="userAgent"/> is <see langword="null"/>.</exception>
    /// <example>
    /// <code>
    /// HttpUserAgentInformation info = HttpUserAgentInformation.Parse(
    ///     "Mozilla/5.0 (Windows NT 10.0; Win64; x64) Chrome/90.0.4430.212 Safari/537.36");
    ///
    /// Console.WriteLine(info.Name);    // "Chrome"
    /// Console.WriteLine(info.Version); // "90.0.4430.212"
    /// </code>
    /// </example>
    public static HttpUserAgentInformation Parse(string userAgent) => HttpUserAgentParser.Parse(userAgent);

    /// <summary>
    /// Creates <see cref="HttpUserAgentInformation"/> for a robot
    /// </summary>
    internal static HttpUserAgentInformation CreateForRobot(string userAgent, string robotName)
        => new(userAgent, platform: null, HttpUserAgentType.Robot, robotName, version: null, deviceName: null);

    /// <summary>
    /// Creates <see cref="HttpUserAgentInformation"/> for a browser
    /// </summary>
    internal static HttpUserAgentInformation CreateForBrowser(string userAgent, HttpUserAgentPlatformInformation? platform, string? browserName, string? browserVersion, string? deviceName)
        => new(userAgent, platform, HttpUserAgentType.Browser, browserName, browserVersion, deviceName);

    /// <summary>
    /// Creates <see cref="HttpUserAgentInformation"/> for an unknown agent type
    /// </summary>
    internal static HttpUserAgentInformation CreateForUnknown(string userAgent, HttpUserAgentPlatformInformation? platform, string? deviceName)
        => new(userAgent, platform, HttpUserAgentType.Unknown, name: null, version: null, deviceName);
}
