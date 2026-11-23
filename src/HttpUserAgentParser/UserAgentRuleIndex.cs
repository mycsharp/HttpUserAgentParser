// Copyright © https://myCSharp.de - all rights reserved

using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Text;

namespace MyCSharp.HttpUserAgentParser;

/// <summary>Shares one candidate search across the four built-in rule tables.</summary>
internal sealed class UserAgentRuleIndex
{
    private const int BucketCount = 2048;
    // For longer headers the individual vectorized substring searches cost less than indexing the full input.
    private const int MaxIndexedLength = 255;
    private readonly Vector256<ulong>[] _buckets = new Vector256<ulong>[BucketCount];
    private readonly Vector256<ulong> _always;

    // Built only when a shared filter is needed. The first robot rule can return before this point.
    public static UserAgentRuleIndex BuiltIn { get; } = new(
        HttpUserAgentRules.DefaultRobotSet.Keys, Array.ConvertAll(HttpUserAgentRules.Platforms, static r => r.Token),
        HttpUserAgentRules.MobileKeys, Array.ConvertAll(HttpUserAgentRules.Browsers, static r => r.DetectToken));

    // Each lane belongs to one rule table. Register both overlapping triples of the first four
    // characters: one triple must start at an even offset, regardless of the key's position.
    public UserAgentRuleIndex(string[] robots, string[] platforms, string[] devices, string[] browsers)
    {
        string[][] tables = [robots, platforms, devices, browsers];
        for (int table = 0; table < tables.Length; table++)
        {
            string[] keys = tables[table];
            for (int rule = 0; rule < keys.Length; rule++)
            {
                string key = keys[rule];
                ulong bit = 1UL << (rule % 64);
                if (key.Length < 4 || !Ascii.IsValid(key))
                {
                    _always = _always.WithElement(table, _always.GetElement(table) | bit);
                    continue;
                }

                Add(Hash(key[0], key[1], key[2]), table, bit);
                Add(Hash(key[1], key[2], key[3]), table, bit);
            }
        }
    }

    private void Add(int hash, int table, ulong bit)
    {
        ref Vector256<ulong> bucket = ref _buckets[hash];
        bucket = bucket.WithElement(table, bucket.GetElement(table) | bit);
    }

    // Fold ASCII letters to the same bucket. Punctuation and hash collisions can only add candidates.
    private static int Hash(char a, char b, char c) => (((a | 32) * 31) ^ ((b | 32) << 5) ^ (c | 32)) & (BucketCount - 1);

    public RuleMasks Find(ReadOnlySpan<char> input)
    {
        if (input.Length > MaxIndexedLength || !Ascii.IsValid(input))
        {
            // Non-ASCII headers also require the full ordinal case-folding semantics.
            return new RuleMasks(Vector256.Create(ulong.MaxValue));
        }

        Vector256<ulong> candidates = _always;
        for (int i = 2; i < input.Length; i += 2)
        {
            candidates |= _buckets[Hash(input[i - 2], input[i - 1], input[i])];
        }

        return new RuleMasks(candidates);
    }

    [StructLayout(LayoutKind.Auto)]
    internal readonly struct RuleMasks(Vector256<ulong> bits)
    {
        public ulong Robots => bits.GetElement(0);
        public ulong Platforms => bits.GetElement(1);
        public ulong Devices => bits.GetElement(2);
        public ulong Browsers => bits.GetElement(3);
    }

    public static int FindKey(ReadOnlySpan<char> input, string[] keys, ulong mask)
    {
        Candidates candidates = new(mask, keys.Length);
        for (int i = candidates.Next(); i >= 0; i = candidates.Next())
        {
            if (input.Contains(keys[i], StringComparison.OrdinalIgnoreCase))
            {
                return i;
            }
        }

        return -1;
    }

    // The same 64 bits cover each successive block of rules. The full key confirms the match.
    [StructLayout(LayoutKind.Auto)]
    internal struct Candidates(ulong mask, int count)
    {
        private readonly ulong _mask = mask;
        private ulong _remaining = mask;
        private int _offset;

        public int Next()
        {
            if (_remaining == 0)
            {
                _offset += 64;
                if (_mask == 0 || _offset >= count)
                {
                    return -1;
                }

                _remaining = _mask;
            }

            int index = _offset + BitOperations.TrailingZeroCount(_remaining);
            _remaining &= _remaining - 1;
            return index < count ? index : -1;
        }
    }
}
