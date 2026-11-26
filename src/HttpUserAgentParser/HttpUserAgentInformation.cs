// Copyright © https://myCSharp.de - all rights reserved

namespace MyCSharp.HttpUserAgentParser;

/// <summary>
/// Analyzed user agent
/// </summary>
public readonly struct HttpUserAgentInformation : IEquatable<HttpUserAgentInformation>
{
    /// <summary>
    /// Full User Agent string
    /// </summary>
    public string UserAgent { get; }

    /// <summary>
    /// Type of user agent, see <see cref="HttpUserAgentType"/>
    /// </summary>
    public HttpUserAgentType Type { get; }

    /// <summary>
    /// Platform of user agent, see <see cref="HttpUserAgentPlatformInformation"/>
    /// </summary>
    public HttpUserAgentPlatformInformation? Platform { get; }

    /// <summary>
    /// Browser or Bot Name of user agent e.g. "Chrome", "Edge"..
    /// </summary>
    public string? Name { get; }

    /// <summary>
    /// Version of Browser or Bot Name of user agent e.g. "79.0", "83.0.125.4"
    /// </summary>
    public string? Version { get; }

    /// <summary>
    /// Device Type of user agent, e.g. "Android", "Apple iPhone"
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
    /// Parses given <param name="userAgent">User Agent</param>
    /// </summary>
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

    /// <summary>
    /// Returns <see langword="true"/> if all properties of both instances are equal (ordinal string comparison).
    /// </summary>
    public bool Equals(HttpUserAgentInformation other)
        => Type == other.Type
        && string.Equals(UserAgent, other.UserAgent, StringComparison.Ordinal)
        && string.Equals(Name, other.Name, StringComparison.Ordinal)
        && string.Equals(Version, other.Version, StringComparison.Ordinal)
        && string.Equals(MobileDeviceType, other.MobileDeviceType, StringComparison.Ordinal)
        && Nullable.Equals(Platform, other.Platform);

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is HttpUserAgentInformation other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(UserAgent, Type, Name, Version);

    /// <summary>
    /// Returns <see langword="true"/> if both instances are equal.
    /// </summary>
    public static bool operator ==(HttpUserAgentInformation left, HttpUserAgentInformation right) => left.Equals(right);

    /// <summary>
    /// Returns <see langword="true"/> if both instances are not equal.
    /// </summary>
    public static bool operator !=(HttpUserAgentInformation left, HttpUserAgentInformation right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString() => (Name, Version) switch
    {
        ({ } name, { } version) => $"{Type}: {name} {version}",
        ({ } name, null) => $"{Type}: {name}",
        _ => Type.ToString(),
    };
}
