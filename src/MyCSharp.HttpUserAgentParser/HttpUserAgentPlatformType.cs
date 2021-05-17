// Copyright © myCSharp 2020-2021, all rights reserved

namespace MyCSharp.HttpUserAgentParser
{
    public enum HttpUserAgentPlatformType : byte
    {
        Unknown = 0,
        Generic,
        Windows,
        Linux,
        Unix,
        IOS,
        MacOS,
        BlackBerry,
        Android,
        Symbian
    }
}
