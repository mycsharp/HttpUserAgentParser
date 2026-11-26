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

    [StructLayout(LayoutKind.Auto)]
    private struct Bucket
    {
        public Vector256<ulong> Low;
        public Vector256<ulong> High;
    }

    private readonly Bucket[] _buckets = new Bucket[BucketCount];
    private readonly Bucket _always;

    // Built only when a shared filter is needed. The first robot rule can return before this point.
    public static UserAgentRuleIndex BuiltIn { get; } = new(
        HttpUserAgentRules.DefaultRobotSet.Keys, Array.ConvertAll(HttpUserAgentRules.Platforms, static r => r.Token),
        HttpUserAgentRules.MobileKeys, Array.ConvertAll(HttpUserAgentRules.Browsers, static r => r.DetectToken));

    // Each lane belongs to one rule table, split across Low (rules 0..63) and High (rules 64..127)
    // so every built-in rule table fits without bit aliasing. Register both overlapping triples of the
    // first four characters: one triple must start at an even offset, regardless of the key's position.
    public UserAgentRuleIndex(string[] robots, string[] platforms, string[] devices, string[] browsers)
    {
        Bucket always = default;
        string[][] tables = [robots, platforms, devices, browsers];
        for (int table = 0; table < tables.Length; table++)
        {
            string[] keys = tables[table];
            for (int rule = 0; rule < keys.Length; rule++)
            {
                string key = keys[rule];
                if (key.Length < 4 || !Ascii.IsValid(key))
                {
                    AddBit(ref always, table, rule);
                    continue;
                }

                AddBit(ref _buckets[Hash(key[0], key[1], key[2])], table, rule);
                AddBit(ref _buckets[Hash(key[1], key[2], key[3])], table, rule);
            }
        }

        _always = always;
    }

    private static void AddBit(ref Bucket target, int table, int rule)
    {
        int slot = rule & 127;
        if (slot < 64)
        {
            ulong bit = 1UL << slot;
            target.Low = target.Low.WithElement(table, target.Low.GetElement(table) | bit);
        }
        else
        {
            ulong bit = 1UL << (slot - 64);
            target.High = target.High.WithElement(table, target.High.GetElement(table) | bit);
        }
    }

    // Fold ASCII letters to the same bucket. Punctuation and hash collisions can only add candidates.
    private static int Hash(char a, char b, char c) => (((a | 32) * 31) ^ ((b | 32) << 5) ^ (c | 32)) & (BucketCount - 1);

    public RuleMasks Find(ReadOnlySpan<char> input)
    {
        if (input.Length > MaxIndexedLength || !Ascii.IsValid(input))
        {
            // Non-ASCII headers also require the full ordinal case-folding semantics.
            Vector256<ulong> all = Vector256.Create(ulong.MaxValue);
            return new RuleMasks(all, all);
        }

        Vector256<ulong> low = _always.Low;
        Vector256<ulong> high = _always.High;
        for (int i = 2; i < input.Length; i += 2)
        {
            ref readonly Bucket bucket = ref _buckets[Hash(input[i - 2], input[i - 1], input[i])];
            low |= bucket.Low;
            high |= bucket.High;
        }

        return new RuleMasks(low, high);
    }

    [StructLayout(LayoutKind.Auto)]
    internal readonly struct RuleMasks(Vector256<ulong> low, Vector256<ulong> high)
    {
        public UInt128 Robots => new(high.GetElement(0), low.GetElement(0));
        public UInt128 Platforms => new(high.GetElement(1), low.GetElement(1));
        public UInt128 Devices => new(high.GetElement(2), low.GetElement(2));
        public UInt128 Browsers => new(high.GetElement(3), low.GetElement(3));
    }

    public static int FindKey(ReadOnlySpan<char> input, string[] keys, UInt128 mask)
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

    // The 128 bits cover each successive block of 128 rules. The full key confirms the match.
    [StructLayout(LayoutKind.Auto)]
    internal struct Candidates(UInt128 mask, int count)
    {
        private readonly ulong _lowMask = (ulong)mask;
        private readonly ulong _highMask = (ulong)(mask >> 64);
        private ulong _remaining = (ulong)mask;
        private int _offset;

        public int Next()
        {
            while (_remaining == 0)
            {
                _offset += 64;
                if (_offset >= count)
                {
                    return -1;
                }

                _remaining = (_offset & 64) != 0 ? _highMask : _lowMask;
            }

            int index = _offset + BitOperations.TrailingZeroCount(_remaining);
            _remaining &= _remaining - 1;
            return index < count ? index : -1;
        }
    }
}
