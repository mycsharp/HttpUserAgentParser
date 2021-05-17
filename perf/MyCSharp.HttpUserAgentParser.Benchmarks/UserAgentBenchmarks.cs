// Copyright © myCSharp 2020-2021, all rights reserved

using System.Collections.Generic;
using System.Linq;
using BenchmarkDotNet.Attributes;
using MyCSharp.HttpUserAgentParser.Benchmarks.ExternalCode;
using UAParser;

namespace MyCSharp.HttpUserAgentParser.Benchmarks
{
    [MemoryDiagnoser]
#if OS_WIN
    [EtwProfiler]   // needs admin-rights
#endif
    public class UserAgentBenchmarks
    {
        private Parser _uaParser;

        private string[] _testUserAgentMix;

        private static IEnumerable<string> GetTestUserAgents()
        {
            yield return
                "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36";
            yield return "APIs-Google (+https://developers.google.com/webmasters/APIs-Google.html)";
            yield return "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:88.0) Gecko/20100101 Firefox/88.0";
            yield return "yeah I'm unknown user agent, just to bring some fun to the mix";
        }

        [GlobalSetup]
        public void Setup()
        {
            _uaParser = UAParser.Parser.GetDefault(new ParserOptions());
            _testUserAgentMix = GetTestUserAgents().ToArray();
        }

        [Benchmark(Description = "UA Parser")]
        public void UAParserTest()
        {
            string[] testUserAgentMix = _testUserAgentMix;

            for (int i = 0; i < testUserAgentMix.Length; ++i)
            {
                _uaParser.Parse(testUserAgentMix[i]);
            }
        }

        [Benchmark(Description = "UserAgentService")]
        public void UserAgentServiceTest()
        {
            string[] testUserAgentMix = _testUserAgentMix;

            for (int i = 0; i < testUserAgentMix.Length; ++i)
            {
                new UserAgentServiceUserAgent(testUserAgentMix[i]);
            }
        }

        [Benchmark(Description = "HttpUserAgentParser")]
        public void HttpUserAgentParserTest()
        {
            string[] testUserAgentMix = _testUserAgentMix;

            for (int i = 0; i < testUserAgentMix.Length; ++i)
            {
                HttpUserAgentParser.Parse(testUserAgentMix[i]);
            }
        }
    }
}
