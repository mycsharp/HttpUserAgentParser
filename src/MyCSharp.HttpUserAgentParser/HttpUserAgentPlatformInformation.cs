using System.Text.RegularExpressions;

namespace MyCSharp.HttpUserAgentParser
{
    public readonly struct HttpUserAgentPlatformInformation
    {
        public Regex Regex { get; }
        public string Name { get; }
        public HttpUserAgentPlatformType PlatformType { get; }

        public HttpUserAgentPlatformInformation(Regex regex, string name, HttpUserAgentPlatformType platformType)
        {
            Regex = regex;
            Name = name;
            PlatformType = platformType;
        }
    }
}