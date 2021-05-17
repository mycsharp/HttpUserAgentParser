// Copyright © myCSharp 2020-2021, all rights reserved

using BenchmarkDotNet.Running;

namespace MyCSharp.HttpUserAgentParser.Benchmarks
{
    class Program
    {
        static void Main()
        {
            BenchmarkRunner.Run<UserAgentBenchmarks>();
        }
    }
}
