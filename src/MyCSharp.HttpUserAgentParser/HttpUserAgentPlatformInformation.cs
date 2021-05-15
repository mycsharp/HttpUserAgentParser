namespace MyCSharp.HttpUserAgentParser
{
    public readonly struct HttpUserAgentPlatformInformation
    {
        public string Id { get; }
        public string Name { get; }
        public HttpUserAgentPlatformType PlatformType { get; }

        public HttpUserAgentPlatformInformation(string id, string name, HttpUserAgentPlatformType platformType)
        {
            Id = id;
            Name = name;
            PlatformType = platformType;
        }
    }
}