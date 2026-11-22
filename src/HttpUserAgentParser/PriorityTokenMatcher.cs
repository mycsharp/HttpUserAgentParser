// Copyright © https://myCSharp.de - all rights reserved

#if NET9_0_OR_GREATER
using System.Buffers;
#endif

namespace MyCSharp.HttpUserAgentParser;

/// <summary>
/// Finds the first token of a priority-ordered token list that occurs in an input
/// (ordinal, case-insensitive substring match).
/// </summary>
/// <typeparam name="TDetector">
/// A marker struct per detector (robot, mobile). Value type arguments give every detector its own
/// JIT-compiled code, so Dynamic PGO optimizes each call site for the <see cref="System.Buffers.SearchValues{T}"/>
/// implementations of that detector instead of mixing the profiles of all detectors.
/// </typeparam>
/// <remarks>
/// <para>
/// On .NET 9 and later, consecutive tokens are grouped into small <see cref="System.Buffers.SearchValues{T}"/> sets.
/// A single vectorized pass rejects a whole group, so only groups that may contain a match are checked
/// token by token. Groups never reorder tokens, which preserves the priority semantics of a sequential scan.
/// </para>
/// <para>
/// See <c>PriorityTokenGroup</c> for the group layout.
/// </para>
/// </remarks>
internal sealed class PriorityTokenMatcher<TDetector>
    where TDetector : struct
{
    private readonly string[] _tokens;

#if NET9_0_OR_GREATER
    private readonly PriorityTokenGroup[] _groups;
#endif

    /// <summary>
    /// Creates a matcher for <paramref name="tokens"/> in priority order.
    /// </summary>
    /// <param name="tokens">Tokens, highest priority first.</param>
    /// <param name="groupStarts">Token indices that must start a new group, e.g. to give a very frequent token its own group.</param>
    public PriorityTokenMatcher(string[] tokens, params ReadOnlySpan<int> groupStarts)
    {
        _tokens = tokens;
#if NET9_0_OR_GREATER
        _groups = PriorityTokenGroup.Create(tokens, groupStarts);
#else
        _ = groupStarts;
#endif
    }

    /// <summary>
    /// Number of tokens.
    /// </summary>
    public int Count => _tokens.Length;

#if NET9_0_OR_GREATER
    /// <summary>
    /// The token groups.
    /// </summary>
    internal ReadOnlySpan<PriorityTokenGroup> Groups => _groups;
#endif

    /// <summary>
    /// Returns <see langword="true"/> if any token occurs in <paramref name="input"/>.
    /// </summary>
    public bool ContainsAny(ReadOnlySpan<char> input)
    {
#if NET9_0_OR_GREATER
        // The group filters alone answer the question.
        foreach (ref readonly PriorityTokenGroup group in _groups.AsSpan())
        {
            if (group.MayMatch(input))
            {
                return true;
            }
        }
#else
        foreach (string token in _tokens)
        {
            if (input.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }
#endif

        return false;
    }

    /// <summary>
    /// Returns the index of the first token at or after <paramref name="startIndex"/> that occurs in <paramref name="input"/>,
    /// or -1 if none occurs.
    /// </summary>
    /// <param name="input">The text to search.</param>
    /// <param name="startIndex">The first token index to consider.</param>
    /// <param name="position">The position of the first occurrence of the returned token in <paramref name="input"/>, otherwise -1.</param>
    public int Find(ReadOnlySpan<char> input, int startIndex, out int position)
    {
        string[] tokens = _tokens;
#if NET9_0_OR_GREATER
        foreach (ref readonly PriorityTokenGroup group in _groups.AsSpan())
        {
            if (group.End <= startIndex || !group.MayMatch(input))
            {
                continue;
            }

            for (int i = Math.Max(group.Start, startIndex); i < group.End; i++)
            {
                position = input.IndexOf(tokens[i], StringComparison.OrdinalIgnoreCase);
                if (position >= 0)
                {
                    return i;
                }
            }
        }
#else
        for (int i = startIndex; i < tokens.Length; i++)
        {
            position = input.IndexOf(tokens[i], StringComparison.OrdinalIgnoreCase);
            if (position >= 0)
            {
                return i;
            }
        }
#endif

        position = -1;
        return -1;
    }
}

#if NET9_0_OR_GREATER
/// <summary>
/// A run of consecutive tokens of a <see cref="PriorityTokenMatcher{TDetector}"/> that is rejected with one vectorized search.
/// </summary>
/// <remarks>
/// The layout is tuned for the Teddy algorithm used by <see cref="SearchValues{T}"/>: at most eight values per group
/// (larger, bucketized groups verify many more false-positive candidates and are measurably slower),
/// tokens shorter than three characters are checked separately (they would force the slower two-character fingerprint
/// for the whole group), and tokens containing another token of the same group are dropped from the group filter
/// because whenever they occur, the shorter token occurs, too.
/// </remarks>
internal readonly struct PriorityTokenGroup
{
    internal const int MaxValuesPerGroup = 8;
    internal const int MinVectorizedTokenLength = 3;

    private PriorityTokenGroup(int start, int end, List<string> values, List<string> shortTokens)
    {
        Start = start;
        End = end;
        FilterValues = [.. values];
        Values = values.Count > 0 ? SearchValues.Create(FilterValues, StringComparison.OrdinalIgnoreCase) : null;
        ShortTokens = [.. shortTokens];
    }

    /// <summary>
    /// Index of the first token of the group.
    /// </summary>
    public int Start { get; }

    /// <summary>
    /// Index after the last token of the group.
    /// </summary>
    public int End { get; }

    /// <summary>
    /// The tokens used for the vectorized group filter (after removing redundant tokens).
    /// </summary>
    public string[] FilterValues { get; }

    /// <summary>
    /// The vectorized group filter, <see langword="null"/> if the group only has short tokens.
    /// </summary>
    public SearchValues<string>? Values { get; }

    /// <summary>
    /// Tokens shorter than <see cref="MinVectorizedTokenLength"/>, checked one by one.
    /// </summary>
    public string[] ShortTokens { get; }

    /// <summary>
    /// Returns <see langword="false"/> if no token of the group occurs in <paramref name="input"/>.
    /// </summary>
    public bool MayMatch(ReadOnlySpan<char> input)
    {
        if (Values is not null && input.ContainsAny(Values))
        {
            return true;
        }

        foreach (string token in ShortTokens)
        {
            if (input.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Splits <paramref name="tokens"/> into groups.
    /// </summary>
    public static PriorityTokenGroup[] Create(string[] tokens, ReadOnlySpan<int> groupStarts)
    {
        List<PriorityTokenGroup> groups = [];
        List<string> values = [];
        List<string> shortTokens = [];
        int start = 0;

        for (int i = 0; i < tokens.Length; i++)
        {
            string token = tokens[i];
            if (i > start && (groupStarts.Contains(i) || !CanAdd(values, token)))
            {
                groups.Add(new PriorityTokenGroup(start, i, values, shortTokens));
                values.Clear();
                shortTokens.Clear();
                start = i;
            }

            Add(values, shortTokens, token);
        }

        if (tokens.Length > 0)
        {
            groups.Add(new PriorityTokenGroup(start, tokens.Length, values, shortTokens));
        }

        return [.. groups];
    }

    private static bool CanAdd(List<string> values, string token)
    {
        if (token.Length < MinVectorizedTokenLength || IsCoveredBy(values, token))
        {
            return true;
        }

        int remaining = values.Count;
        foreach (string value in values)
        {
            if (value.Contains(token, StringComparison.OrdinalIgnoreCase))
            {
                remaining--;
            }
        }

        return remaining < MaxValuesPerGroup;
    }

    private static void Add(List<string> values, List<string> shortTokens, string token)
    {
        if (token.Length < MinVectorizedTokenLength)
        {
            shortTokens.Add(token);
            return;
        }

        if (IsCoveredBy(values, token))
        {
            return;
        }

        // A value containing the new token is redundant: whenever it matches, the new token matches, too.
        values.RemoveAll(value => value.Contains(token, StringComparison.OrdinalIgnoreCase));
        values.Add(token);
    }

    /// <summary>
    /// Returns <see langword="true"/> if <paramref name="token"/> contains one of <paramref name="values"/>:
    /// an occurrence of the token then always implies a match of the group filter.
    /// </summary>
    private static bool IsCoveredBy(List<string> values, string token)
    {
        foreach (string value in values)
        {
            if (token.Contains(value, StringComparison.OrdinalIgnoreCase))
            {
                return true;
            }
        }

        return false;
    }
}
#endif
