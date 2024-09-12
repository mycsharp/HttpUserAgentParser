// Copyright © myCSharp.de - all rights reserved

using BenchmarkDotNet.Attributes;

#if OS_WIN
using BenchmarkDotNet.Diagnostics.Windows.Configs;
using BenchmarkDotNet.Jobs;
#endif

namespace MyCSharp.HttpUserAgentParser.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net90)]
#if OS_WIN
[EtwProfiler]   // needs admin-rights
#endif
public class HttpUserAgentParserBenchmarks
{
    private string[] _testUserAgentMix;
    private HttpUserAgentInformation[] _results;

    [GlobalSetup]
    public void GlobalSetup()
    {
        _testUserAgentMix = GetTestUserAgents().ToArray();
        _results = new HttpUserAgentInformation[_testUserAgentMix.Length];
    }

    private static IEnumerable<string> GetTestUserAgents()
    {
        yield return "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36";
        yield return "APIs-Google (+https://developers.google.com/webmasters/APIs-Google.html)";
        yield return "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0";
        yield return "yeah I'm unknown user agent, just to bring some fun to the mix";
    }

    [Benchmark]
    public void Parse()
    {
        string[] testUserAgentMix = _testUserAgentMix;
        HttpUserAgentInformation[] results = _results;

        for (int i = 0; i < testUserAgentMix.Length; ++i)
        {
            results[i] = HttpUserAgentParser.Parse(testUserAgentMix[i]);
        }
    }
}
