// Copyright © https://myCSharp.de - all rights reserved

#if BENCHMARK_BASELINE
extern alias Baseline;
#endif

using BenchmarkDotNet.Attributes;
using MyCSharp.HttpUserAgentParser.TestHelpers;
using Parser = MyCSharp.HttpUserAgentParser.HttpUserAgentParser;

namespace HttpUserAgentParser.Benchmarks;

/// <summary>
/// Parses the browser user agents of <see cref="UserAgentCatalog"/>: the most used browsers in their current and previous version.
/// </summary>
[MemoryDiagnoser]
public class BrowserUserAgentBenchmarks
{
    [ParamsSource(nameof(GetUserAgents))]
    public BrowserUserAgent Data { get; set; }

    public static IEnumerable<BrowserUserAgent> GetUserAgents() => UserAgentCatalog.Browsers;

    [Benchmark]
    public string Parse() => Parser.Parse(Data.UserAgent).Name;

#if BENCHMARK_BASELINE
    [Benchmark(Baseline = true)]
    public string ParseBaseline() => Baseline::MyCSharp.HttpUserAgentParser.HttpUserAgentParser.Parse(Data.UserAgent).Name;
#endif
}

/// <summary>
/// Parses the search engine, SEO and AI crawler user agents of <see cref="UserAgentCatalog"/>.
/// </summary>
[MemoryDiagnoser]
public class RobotUserAgentBenchmarks
{
    [ParamsSource(nameof(GetUserAgents))]
    public RobotUserAgent Data { get; set; }

    public static IEnumerable<RobotUserAgent> GetUserAgents() => UserAgentCatalog.Robots;

    [Benchmark]
    public string Parse() => Parser.Parse(Data.UserAgent).Name;

#if BENCHMARK_BASELINE
    [Benchmark(Baseline = true)]
    public string ParseBaseline() => Baseline::MyCSharp.HttpUserAgentParser.HttpUserAgentParser.Parse(Data.UserAgent).Name;
#endif
}

/// <summary>
/// Parses a mixed workload of real-world user agents in one loop, like a server that sees many different clients.
/// </summary>
/// <remarks>
/// Single user agent benchmarks let Dynamic PGO specialize the code for exactly one input;
/// a mixed workload is closer to production. The reported time is per parsed user agent.
/// </remarks>
[MemoryDiagnoser]
public class MixedWorkloadBenchmarks
{
    // A whole number of passes over the 44, 48, 96 and 60 entry workloads.
    private const int OperationsPerInvoke = 5280;

    private string[] _userAgents;

    [Params("Browsers", "Robots", "All", "Snapshot")]
    public string Workload { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        IReadOnlyList<string> source = Workload switch
        {
            "Browsers" => [.. UserAgentCatalog.Browsers.Select(static ua => ua.UserAgent)],
            "Robots" => [.. UserAgentCatalog.Robots.Select(static ua => ua.UserAgent)],
            "Snapshot" => [.. UserAgentsMeSnapshot.Cases.Select(static ua => ua.UserAgent)],
            _ => UserAgentCatalog.All,
        };

        // Repeat the workload to a fixed number of operations, so the per-operation time is comparable between workloads.
        _userAgents = [.. Enumerable.Range(0, OperationsPerInvoke).Select(i => source[i % source.Count])];
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public int Parse()
    {
        int robots = 0;
        foreach (string userAgent in _userAgents)
        {
            if (Parser.Parse(userAgent).Type == MyCSharp.HttpUserAgentParser.HttpUserAgentType.Robot)
            {
                robots++;
            }
        }

        return robots;
    }

#if BENCHMARK_BASELINE
    [Benchmark(Baseline = true, OperationsPerInvoke = OperationsPerInvoke)]
    public int ParseBaseline()
    {
        int robots = 0;
        foreach (string userAgent in _userAgents)
        {
            if (Baseline::MyCSharp.HttpUserAgentParser.HttpUserAgentParser.Parse(userAgent).Type == Baseline::MyCSharp.HttpUserAgentParser.HttpUserAgentType.Robot)
            {
                robots++;
            }
        }

        return robots;
    }
#endif
}

/// <summary>
/// Benchmarks the standalone detector methods (<c>GetPlatform</c>, <c>GetMobileDevice</c>, <c>GetBrowser</c> and zero-allocation span <c>TryGetBrowser</c>)
/// over the mixed real-world workload.
/// </summary>
[MemoryDiagnoser]
public class DetectorWorkloadBenchmarks
{
    private const int OperationsPerInvoke = 5280;

    private string[] _userAgents;

    [GlobalSetup]
    public void Setup()
    {
        IReadOnlyList<string> source = UserAgentCatalog.All;
        _userAgents = [.. Enumerable.Range(0, OperationsPerInvoke).Select(i => source[i % source.Count])];
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public int GetPlatform()
    {
        int found = 0;
        foreach (string userAgent in _userAgents)
        {
            if (Parser.GetPlatform(userAgent) is not null)
            {
                found++;
            }
        }

        return found;
    }

#if BENCHMARK_BASELINE
    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public int GetPlatformBaseline()
    {
        int found = 0;
        foreach (string userAgent in _userAgents)
        {
            if (Baseline::MyCSharp.HttpUserAgentParser.HttpUserAgentParser.GetPlatform(userAgent) is not null)
            {
                found++;
            }
        }

        return found;
    }
#endif

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public int GetMobileDevice()
    {
        int found = 0;
        foreach (string userAgent in _userAgents)
        {
            if (Parser.GetMobileDevice(userAgent) is not null)
            {
                found++;
            }
        }

        return found;
    }

#if BENCHMARK_BASELINE
    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public int GetMobileDeviceBaseline()
    {
        int found = 0;
        foreach (string userAgent in _userAgents)
        {
            if (Baseline::MyCSharp.HttpUserAgentParser.HttpUserAgentParser.GetMobileDevice(userAgent) is not null)
            {
                found++;
            }
        }

        return found;
    }
#endif

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public int GetBrowser()
    {
        int found = 0;
        foreach (string userAgent in _userAgents)
        {
            if (Parser.GetBrowser(userAgent) is not null)
            {
                found++;
            }
        }

        return found;
    }

    [Benchmark(OperationsPerInvoke = OperationsPerInvoke)]
    public int TryGetBrowserSpan()
    {
        int found = 0;
        foreach (string userAgent in _userAgents)
        {
            if (Parser.TryGetBrowser(userAgent.AsSpan(), out _, out _))
            {
                found++;
            }
        }

        return found;
    }
}
