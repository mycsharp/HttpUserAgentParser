// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.Metrics;
using System.Runtime.CompilerServices;

namespace MyCSharp.HttpUserAgentParser.Telemetry;

/// <summary>
/// Prevents the core and the MemoryCache package from recording to the same <see cref="Meter"/>.
/// </summary>
/// <remarks>
/// Both packages create the instruments <c>cache.hit</c>, <c>cache.miss</c> and <c>cache.size</c>. On one meter, the counters
/// with the same name, unit and description are the same instrument, and exporters keep only one of the two gauges, so the
/// values of both packages would mix. The ASP.NET Core package has no instrument in common with them and may share a meter. A meter that one of the two packages
/// has used stays reserved for it, also after a switch to another meter, because its instruments remain on the meter.
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

    // A meter keeps its instruments until it is disposed, so a reservation lasts as long as the meter (held weakly).
    private static readonly ConditionalWeakTable<Meter, object?> s_coreMeters = new();
    private static readonly ConditionalWeakTable<Meter, object?> s_memoryCacheMeters = new();

    /// <summary>
    /// Reserves <paramref name="meter"/> for the core package before it creates its instruments on it.
    /// </summary>
    /// <exception cref="ArgumentException">The MemoryCache package has used <paramref name="meter"/>.</exception>
    public static void ReserveForCore(Meter meter) => Reserve(meter, s_coreMeters, s_memoryCacheMeters, MemoryCachePackage);

    /// <summary>
    /// Reserves <paramref name="meter"/> for the MemoryCache package before it creates its instruments on it.
    /// </summary>
    /// <exception cref="ArgumentException">The core package has used <paramref name="meter"/>.</exception>
    public static void ReserveForMemoryCache(Meter meter) => Reserve(meter, s_memoryCacheMeters, s_coreMeters, CorePackage);

    /// <summary>
    /// Forgets the reservations of the core package (unit tests).
    /// </summary>
    public static void ResetCoreForTests()
    {
        lock (s_lock)
        {
            s_coreMeters.Clear();
        }
    }

    /// <summary>
    /// Forgets the reservations of the MemoryCache package (unit tests).
    /// </summary>
    public static void ResetMemoryCacheForTests()
    {
        lock (s_lock)
        {
            s_memoryCacheMeters.Clear();
        }
    }

    private static void Reserve(Meter meter, ConditionalWeakTable<Meter, object?> reservations, ConditionalWeakTable<Meter, object?> otherReservations, string otherPackage)
    {
        lock (s_lock)
        {
            if (otherReservations.TryGetValue(meter, out _))
            {
                throw new ArgumentException(
                    $"The meter '{meter.Name}' is used by {otherPackage}. The core and the MemoryCache package both create the instruments "
                    + "cache.hit, cache.miss and cache.size, and a meter keeps them until it is disposed, so each package needs its own meter.",
                    nameof(meter));
            }

            reservations.AddOrUpdate(meter, null);
        }
    }
}
