// Copyright © https://myCSharp.de - all rights reserved

using Xunit;

namespace MyCSharp.HttpUserAgentParser.MemoryCache.UnitTests.Telemetry;

/// <summary>
/// Telemetry switches are process-wide static state. Tests that change or depend on it run in this collection,
/// which is not parallelized and runs after all parallel tests, so no other test observes a foreign telemetry state.
/// </summary>
[CollectionDefinition(Name, DisableParallelization = true)]
public sealed class TelemetryCollection
{
    public const string Name = "Telemetry";
}
