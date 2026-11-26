// Copyright © https://myCSharp.de - all rights reserved

using System.Text.RegularExpressions;

namespace MyCSharp.HttpUserAgentParser;

/// <summary>
/// Information about the user agent platform
/// </summary>
public readonly struct HttpUserAgentPlatformInformation : IEquatable<HttpUserAgentPlatformInformation>
{
    private readonly Regex? _regex;
    internal HttpUserAgentPlatformRegexId RegexId { get; }

    /// <summary>
    /// Creates a new instance of <see cref="HttpUserAgentPlatformInformation"/>
    /// </summary>
    public HttpUserAgentPlatformInformation(Regex regex, string name, HttpUserAgentPlatformType platformType)
    {
        _regex = regex;
        Name = name;
        PlatformType = platformType;
    }

    /// <summary>
    /// Creates a platform information whose source-generated <see cref="Regex"/> is resolved on first access.
    /// </summary>
    internal HttpUserAgentPlatformInformation(HttpUserAgentPlatformRegexId regexId, string name, HttpUserAgentPlatformType platformType)
    {
        RegexId = regexId;
        Name = name;
        PlatformType = platformType;
    }

    /// <summary>
    /// Regex-pattern that matches this user agent string
    /// </summary>
    /// <remarks>
    /// The built-in platforms use source-generated regular expressions that are created on first access.
    /// The parser does not use this regex; it is provided for consumers that want to match the platform themselves.
    /// </remarks>
    public Regex Regex => _regex ?? HttpUserAgentRegexes.GetPlatformRegex(RegexId)!;

    /// <summary>
    /// Name of the platform
    /// </summary>
    public string Name { get; }

    /// <summary>
    /// Specific platform type aka family
    /// </summary>
    public HttpUserAgentPlatformType PlatformType { get; }

    /// <summary>
    /// Returns <see langword="true"/> if both instances describe the same platform with the same <see cref="Regex"/> instance.
    /// </summary>
    public bool Equals(HttpUserAgentPlatformInformation other)
        => PlatformType == other.PlatformType
        && string.Equals(Name, other.Name, StringComparison.Ordinal)
        && (_regex is null && other._regex is null ? RegexId == other.RegexId : ReferenceEquals(Regex, other.Regex));

    /// <inheritdoc/>
    public override bool Equals(object? obj) => obj is HttpUserAgentPlatformInformation other && Equals(other);

    /// <inheritdoc/>
    public override int GetHashCode() => HashCode.Combine(Name, PlatformType);

    /// <summary>
    /// Returns <see langword="true"/> if both instances are equal.
    /// </summary>
    public static bool operator ==(HttpUserAgentPlatformInformation left, HttpUserAgentPlatformInformation right) => left.Equals(right);

    /// <summary>
    /// Returns <see langword="true"/> if both instances are not equal.
    /// </summary>
    public static bool operator !=(HttpUserAgentPlatformInformation left, HttpUserAgentPlatformInformation right) => !left.Equals(right);

    /// <inheritdoc/>
    public override string ToString()
        => string.IsNullOrEmpty(Name) ? PlatformType.ToString() : $"{Name} ({PlatformType})";
}
