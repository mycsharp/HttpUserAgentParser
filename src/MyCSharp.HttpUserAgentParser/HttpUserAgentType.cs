// Copyright © myCSharp 2020-2022, all rights reserved

namespace MyCSharp.HttpUserAgentParser
{
    /// <summary>
    /// HTTP User Agent Types
    /// </summary>
    public enum HttpUserAgentType : byte
    {
        /// <summary>
        /// Unkown / not mapped
        /// </summary>
        Unknown,
        /// <summary>
        /// Browser
        /// </summary>
        Browser,
        /// <summary>
        /// Robot
        /// </summary>
        Robot,
    }
}
