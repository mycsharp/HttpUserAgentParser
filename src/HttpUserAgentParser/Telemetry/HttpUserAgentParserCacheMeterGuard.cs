// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Metrics;

namespace MyCSharp.HttpUserAgentParser.Telemetry;

/// <summary>
/// Prevents the core and the MemoryCache package from recording to the same <see cref="Meter"/>.
/// </summary>
/// <remarks>
/// Both packages create the instruments <c>cache.hit</c>, <c>cache.miss</c> and <c>cache.size</c>. On one meter, the counters
/// with the same name, unit and description are the same instrument, and exporters keep only one of the two gauges, so the
/// values of both packages would mix. The ASP.NET Core package has no instrument in common with them and may share a meter.
/// </remarks>
internal static class HttpUserAgentParserCacheMeterGuard
{
    private const string CorePackage = "MyCSharp.HttpUserAgentParser";
    private const string MemoryCachePackage = "MyCSharp.HttpUserAgentParser.MemoryCache";

#if NET9_0_OR_GREATER
    private static readonly Lock s_lock = new();
#else
    private static readonly object s_lock = new();
#endif

    private static Meter? s_coreMeter;
    private static Meter? s_memoryCacheMeter;

    /// <summary>
    /// Records that the core package uses <paramref name="meter"/> from now on.
    /// </summary>
    /// <exception cref="ArgumentException">The MemoryCache package uses <paramref name="meter"/>.</exception>
    public static void UseForCore(Meter meter)
    {
        lock (s_lock)
        {
            ThrowIfUsed(meter, s_memoryCacheMeter, MemoryCachePackage);
            s_coreMeter = meter;
        }
    }

    /// <summary>
    /// Records that the MemoryCache package uses <paramref name="meter"/> from now on.
    /// </summary>
    /// <exception cref="ArgumentException">The core package uses <paramref name="meter"/>.</exception>
    public static void UseForMemoryCache(Meter meter)
    {
        lock (s_lock)
        {
            ThrowIfUsed(meter, s_coreMeter, CorePackage);
            s_memoryCacheMeter = meter;
        }
    }

    /// <summary>
    /// Forgets the meter of the core package (unit tests).
    /// </summary>
    public static void ResetCoreForTests()
    {
        lock (s_lock)
        {
            s_coreMeter = null;
        }
    }

    /// <summary>
    /// Forgets the meter of the MemoryCache package (unit tests).
    /// </summary>
    public static void ResetMemoryCacheForTests()
    {
        lock (s_lock)
        {
            s_memoryCacheMeter = null;
        }
    }

    private static void ThrowIfUsed(Meter meter, Meter? meterOfOtherPackage, string otherPackage)
    {
        if (ReferenceEquals(meter, meterOfOtherPackage))
        {
            throw new ArgumentException(
                $"The meter '{meter.Name}' is already used by {otherPackage}. The core and the MemoryCache package both create the instruments "
                + "cache.hit, cache.miss and cache.size, so each of them needs its own meter.",
                nameof(meter));
        }
    }
}
