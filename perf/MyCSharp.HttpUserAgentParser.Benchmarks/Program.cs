using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Running;
using MyCSharp.HttpUserAgentParser.Benchmarks.ExternalCode;
using UAParser;

namespace MyCSharp.HttpUserAgentParser.Benchmarks
{
    [MemoryDiagnoser]
    public class UserAgent
    {

        private Parser _uaParser;

        private const string TestUserAgent =
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/90.0.4430.212 Safari/537.36 Edg/90.0.818.62";


        [GlobalSetup]
        public void Setup()
        {
            _uaParser = UAParser.Parser.GetDefault(new ParserOptions());
        }

        [Benchmark(Description = "UA Parser")]
        public void UAParserTest() => _uaParser.Parse(TestUserAgent);


        [Benchmark(Description = "UserAgentService")]
        public void UserAgentServiceTest() => new UserAgentServiceUserAgent(TestUserAgent);


        [Benchmark(Description = "HttpUserAgentParser")]
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
