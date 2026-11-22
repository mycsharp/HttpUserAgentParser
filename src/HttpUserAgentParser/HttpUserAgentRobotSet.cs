// Copyright © https://myCSharp.de - all rights reserved

using System.Collections.ObjectModel;

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
    /// <summary>
    /// Creates the set for <paramref name="robots"/> in priority order; the array must not be changed afterwards.
    /// </summary>
    public HttpUserAgentRobotSet((string Key, string Value)[] robots)
    {
        Entries = robots;
        ReadOnlyEntries = Array.AsReadOnly(robots);
        Keys = Array.ConvertAll(robots, static robot => robot.Key);

        // Without robots there is no first robot.
        (FirstKey, FirstValue) = robots.Length > 0 ? robots[0] : (null, null);

        // The first robot is checked on its own (Googlebot in the default list, by far the most common robot): own group.
        Matcher = Keys.Length > 1
            ? new PriorityTokenMatcher<HttpUserAgentRules.RobotDetector>(Keys, 1)
            : new PriorityTokenMatcher<HttpUserAgentRules.RobotDetector>(Keys);
        AnchorMatcher = new PriorityTokenMatcher<HttpUserAgentRules.RobotAnchorDetector>(
            HttpUserAgentRules.CreateRobotAnchors(Keys, HttpUserAgentRules.CuratedRobotAnchors));
    }

    /// <summary>
    /// Gets the key of the first robot, or <see langword="null"/> without robots.
    /// </summary>
    public string? FirstKey { get; }

    /// <summary>
    /// Gets the name of the first robot.
    /// </summary>
    public string? FirstValue { get; }

    /// <summary>
    /// Gets the robots in priority order.
    /// </summary>
    public (string Key, string Value)[] Entries { get; }

    /// <summary>
    /// Gets the robots as read-only list for <see cref="HttpUserAgentStatics.Robots"/>.
    /// </summary>
    public ReadOnlyCollection<(string Key, string Value)> ReadOnlyEntries { get; }

    /// <summary>
    /// Gets the keys of <see cref="Entries"/>.
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
