// Copyright © https://myCSharp.de - all rights reserved

namespace MyCSharp.HttpUserAgentParser.Telemetry;

/// <summary>
/// Provides shared logic for building meter names from a customizable prefix and a per-component suffix.
/// </summary>
/// <remarks>
/// All MyCSharp.HttpUserAgentParser meter classes follow the naming convention
/// <c>mycsharp.&lt;suffix&gt;</c>. This helper centralises the prefix-validation and
/// name-composition rules so they do not need to be duplicated across every package.
/// </remarks>
internal static class HttpUserAgentParserMeterNameHelper
{
    /// <summary>
    /// The default organisation prefix applied when the <c>meterPrefix</c> argument is <see langword="null"/>.
    /// </summary>
    private const string DefaultPrefix = "mycsharp.";

    /// <summary>
    /// Builds a meter name from an optional custom prefix and a fixed component suffix.
    /// </summary>
    /// <param name="meterPrefix">
    /// Controls the prefix of the returned meter name:
    /// <list type="bullet">
    ///   <item><description><see langword="null"/> — uses the default prefix <c>mycsharp.</c>.</description></item>
    ///   <item><description>Empty (or whitespace-only) string — no prefix is applied; the suffix is returned as-is.</description></item>
    ///   <item><description>Any other value — must consist solely of ASCII letters or digits and must end with <c>.</c>.
    ///   The prefix is then prepended to <paramref name="meterNameSuffix"/>.</description></item>
    /// </list>
    /// </param>
    /// <param name="meterNameSuffix">
    /// The component-specific part of the meter name (e.g. <c>http_user_agent_parser.aspnetcore</c>).
    /// Must not be <see langword="null"/>.
    /// </param>
    /// <returns>The fully composed meter name.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="meterPrefix"/> is non-empty but either does not end with <c>.</c>
    /// or contains non-alphanumeric characters (excluding the trailing dot).
    /// </exception>
    public static string GetMeterName(string? meterPrefix, string meterNameSuffix)
    {
        if (meterPrefix is null)
        {
            return DefaultPrefix + meterNameSuffix;
        }

        meterPrefix = meterPrefix.Trim();
        if (meterPrefix.Length == 0)
        {
            return meterNameSuffix;
        }

        if (!meterPrefix.EndsWith('.'))
        {
            throw new ArgumentException("Meter prefix must end with '.'.", nameof(meterPrefix));
        }

        for (int i = 0; i < meterPrefix.Length - 1; i++)
        {
            char c = meterPrefix[i];
            if (!char.IsLetterOrDigit(c))
            {
                throw new ArgumentException("Meter prefix must be alphanumeric.", nameof(meterPrefix));
            }
        }

        return meterPrefix + meterNameSuffix;
    }
}
