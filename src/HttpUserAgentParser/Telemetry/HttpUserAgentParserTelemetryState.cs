// Copyright © https://myCSharp.de - all rights reserved

using System.Diagnostics.CodeAnalysis;

namespace MyCSharp.HttpUserAgentParser.Telemetry;

[ExcludeFromCodeCoverage]
internal static class HttpUserAgentParserTelemetryState
{
    private static long s_concurrentCacheSize;

    public static long ConcurrentCacheSize => Volatile.Read(ref s_concurrentCacheSize);

    public static void SetConcurrentCacheSize(int size) => Volatile.Write(ref s_concurrentCacheSize, size);

#if DEBUG
    public static void ResetForTests() => Volatile.Write(ref s_concurrentCacheSize, 0);
#endif
}
