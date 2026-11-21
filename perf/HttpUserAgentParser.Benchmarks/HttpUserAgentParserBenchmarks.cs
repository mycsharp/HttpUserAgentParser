// Copyright © https://myCSharp.de - all rights reserved

#if BENCHMARK_BASELINE
extern alias Baseline;
#endif

using BenchmarkDotNet.Attributes;
using Parser = MyCSharp.HttpUserAgentParser.HttpUserAgentParser;

namespace HttpUserAgentParser.Benchmarks;

[MemoryDiagnoser]
public class HttpUserAgentParserBenchmarks
{
    public sealed record TestData(string Label, string UserAgent)
    {
        public override string ToString() => Label;
    }

    [ParamsSource(nameof(GetTestUserAgents))]
    public TestData Data { get; set; }

    public static IEnumerable<TestData> GetTestUserAgents()
    {
        yield return new("Chrome Windows", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Safari/537.36");
        yield return new("Firefox Linux", "Mozilla/5.0 (X11; Linux x86_64; rv:132.0) Gecko/20100101 Firefox/132.0");
        yield return new("Chrome Android", "Mozilla/5.0 (Linux; Android 15; Pixel 8) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/131.0.0.0 Mobile Safari/537.36");
        yield return new("Safari iPhone", "Mozilla/5.0 (iPhone; CPU iPhone OS 18_1 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/18.1 Mobile/15E148 Safari/604.1");
        yield return new("Googlebot", "Mozilla/5.0 (compatible; Googlebot/2.1; +http://www.google.com/bot.html)");
        yield return new("Applebot", "Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.4 Safari/605.1.15 (Applebot/0.1; +http://www.apple.com/go/applebot)");
        yield return new("Unknown", "custom-client/1.0 (+https://example.org/client)");
    }

    [Benchmark]
    public string Parse() => Parser.Parse(Data.UserAgent).Name;

#if BENCHMARK_BASELINE
    [Benchmark(Baseline = true)]
    public string ParseBaseline() => Baseline::MyCSharp.HttpUserAgentParser.HttpUserAgentParser.Parse(Data.UserAgent).Name;
#endif
}
