// Copyright © myCSharp 2020-2021, all rights reserved

using System.Collections.Generic;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Columns;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using DeviceDetectorNET;
using MyCSharp.HttpUserAgentParser.Providers;

namespace MyCSharp.HttpUserAgentParser.Benchmarks.LibraryComparison
{
    [ShortRunJob]
    [MemoryDiagnoser]
    [CategoriesColumn]
    [GroupBenchmarksBy(BenchmarkLogicalGroupRule.ByCategory)]
    public class LibraryComparisonBenchmarks
    {
        public record TestData(string Label, string UserAgent)
        {
            public override string ToString() => Label;
        }

        [ParamsSource(nameof(GetTestUserAgents))]
        public TestData Data { get; set; }

        public IEnumerable<TestData> GetTestUserAgents()
        {
            yield return new("Chrome Win10", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36");
            yield return new("Google-Bot", "APIs-Google (+https://developers.google.com/webmasters/APIs-Google.html)");
        }

        [Benchmark(Baseline = true, Description = "MyCSharp")]
        [BenchmarkCategory("Basic")]
        public HttpUserAgentInformation MyCSharpBasic()
        {
            HttpUserAgentInformation info = HttpUserAgentParser.Parse(Data.UserAgent);
            return info;
        }

        private static readonly HttpUserAgentParserCachedProvider s_myCSharpCachedProvider = new();

        [Benchmark(Baseline = true, Description = "MyCSharp")]
        [BenchmarkCategory("Cached")]
        public HttpUserAgentInformation MyCSharpCached()
        {
            return s_myCSharpCachedProvider.Parse(Data.UserAgent);
        }

        [Benchmark(Description = "UAParser")]
        [BenchmarkCategory("Basic")]
        public UAParser.ClientInfo UAParserBasic()
        {
            UAParser.ClientInfo info = UAParser.Parser.GetDefault().Parse(Data.UserAgent);
            return info;
        }

        private static readonly UAParser.Parser s_uaParser = UAParser.Parser.GetDefault(new UAParser.ParserOptions { UseCompiledRegex = true });

        [Benchmark(Description = "UAParser")]
        [BenchmarkCategory("Cached")]
        public UAParser.ClientInfo UAParserCached()
        {
            UAParser.ClientInfo info = s_uaParser.Parse(Data.UserAgent);
            return info;
        }

        [Benchmark(Description = "DeviceDetector.NET")]
        [BenchmarkCategory("Basic")]
        public object DeviceDetectorNETBasic()
        {
            DeviceDetector dd = new(Data.UserAgent);
            dd.Parse();

            var info = new
            {
                Client = dd.GetClient(),
                OS = dd.GetOs(),
                Device = dd.GetDeviceName(),
                Brand = dd.GetBrandName(),
                Model = dd.GetModel()
            };

            return info;
        }
    }
}
