using System.Reflection;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Running;
using Ng.Services;
using UAParser;

namespace MyCSharp.HttpUserAgentParser.Benchmarks
{
    [MemoryDiagnoser]
    public class UserAgent
    {

        private Parser _uaParser;
        private UserAgentService _userAgentService;

        private const string TestUserAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36 Edg/90.0.818.62";

        [GlobalSetup]
        public void Setup()
        {
            _uaParser = UAParser.Parser.GetDefault(new ParserOptions());
            _userAgentService = new UserAgentService();
        }

        [Benchmark]
        public void UAParserTest() => _uaParser.Parse(TestUserAgent);

        [Benchmark]
        public void UserAgentService() => _userAgentService.Parse(TestUserAgent);

        [Benchmark]
        public void HttpUserAgentParserTest() => HttpUserAgentParser.Parse(TestUserAgent);
    }

    class Program
    {
        static void Main()
        {
            BenchmarkRunner.Run<UserAgent>();
        }
    }
}
