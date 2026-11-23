// Copyright © https://myCSharp.de - all rights reserved

namespace MyCSharp.HttpUserAgentParser;

/// <summary>
/// An immutable list of robots with the matchers the parser needs for it.
/// </summary>
/// <remarks>
/// <see cref="HttpUserAgentStatics.SetRobots"/> creates a new set and replaces the current one atomically; a parse reads the
/// current set once, so it never sees a partially replaced list.
/// </remarks>
internal sealed class HttpUserAgentRobotSet
{
    private readonly int[] _literalPlusRules;

    /// <summary>
    /// Creates the set for <paramref name="robots"/> in priority order; the array must not be changed afterwards.
    /// </summary>
    public HttpUserAgentRobotSet((string Key, string Value)[] robots)
    {
        ReadOnlyEntries = [.. robots];
        Keys = Array.ConvertAll(robots, static robot => robot.Key);

        _literalPlusRules = [.. Enumerable.Range(0, robots.Length).Where(i => robots[i].Key.Contains('+'))];

        // The first robot is checked on its own (Googlebot in the default list, by far the most common robot): own group.
        Matcher = new PriorityTokenMatcher<HttpUserAgentRules.RobotDetector>(Keys, 1);
        AnchorMatcher = new PriorityTokenMatcher<HttpUserAgentRules.RobotAnchorDetector>(
            HttpUserAgentRules.CreateRobotAnchors(Keys, HttpUserAgentRules.CuratedRobotAnchors));
    }

    /// <summary>
    /// Matches both IIS-encoded names and literal plus signs in custom keys, in configured priority order.
    /// </summary>
    public string? Match(string original, ReadOnlySpan<char> decoded)
    {
        int index = Find(decoded);
        foreach (int literal in _literalPlusRules)
        {
            if (index >= 0 && literal >= index)
            {
                break;
            }

            if (original.AsSpan().Contains(Keys[literal], StringComparison.OrdinalIgnoreCase))
            {
                return ReadOnlyEntries[literal].Value;
            }
        }

        return index >= 0 ? ReadOnlyEntries[index].Value : null;
    }

    private int Find(ReadOnlySpan<char> input)
    {
        if (Keys.Length > 0 && input.Contains(Keys[0], StringComparison.OrdinalIgnoreCase))
        {
            return 0;
        }

        return Keys.Length > 1 && AnchorMatcher.ContainsAny(input) ? Matcher.Find(input, 1) : -1;
    }

    /// <summary>
    /// Opaque identity for cache entries. Expired entries can retain this object without retaining the rules.
    /// </summary>
    public object CacheGeneration { get; } = new();

    /// <summary>
    /// Gets the robots as read-only list for <see cref="HttpUserAgentStatics.Robots"/>.
    /// </summary>
    public IReadOnlyList<(string Key, string Value)> ReadOnlyEntries { get; }

    /// <summary>
    /// Gets the keys in priority order.
    /// </summary>
    public string[] Keys { get; }

    /// <summary>
    /// Gets the matcher over <see cref="Keys"/>.
    /// </summary>
    public PriorityTokenMatcher<HttpUserAgentRules.RobotDetector> Matcher { get; }

    /// <summary>
    /// Gets the matcher over the anchors: every key contains one, so a user agent without any anchor matches no key.
    /// </summary>
    public PriorityTokenMatcher<HttpUserAgentRules.RobotAnchorDetector> AnchorMatcher { get; }
}
