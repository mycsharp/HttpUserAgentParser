// Copyright © myCSharp.de - all rights reserved

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
