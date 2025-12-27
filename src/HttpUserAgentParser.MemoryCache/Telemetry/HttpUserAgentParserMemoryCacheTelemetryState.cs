// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.CodeAnalysis;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.Telemetry;

[ExcludeFromCodeCoverage]
internal static class HttpUserAgentParserMemoryCacheTelemetryState
{
    private static long s_cacheSize;

    public static long CacheSize => Volatile.Read(ref s_cacheSize);

    public static void CacheSizeIncrement() => Interlocked.Increment(ref s_cacheSize);

    public static void CacheSizeDecrement() => Interlocked.Decrement(ref s_cacheSize);
}
