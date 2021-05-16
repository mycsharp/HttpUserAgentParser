﻿// Copyright © myCSharp 2020-2021, all rights reserved

using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using MyCSharp.HttpUserAgentParser.Providers;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.DependencyInjection
{
    public static class HttpUserAgentParserDependencyInjectionOptionsExtensions
    {
        /// <summary>
        /// Registers <see cref="HttpUserAgentParserAccessor"/> as <see cref="IHttpUserAgentParserAccessor"/>.
        ///  Requires a registered <see cref="IHttpUserAgentParserProvider"/>
        /// </summary>
        public static HttpUserAgentParserDependencyInjectionOptions AddHttpUserAgentParserAccessor(
            this HttpUserAgentParserDependencyInjectionOptions options)
        {
            options.Services.AddSingleton<IHttpUserAgentParserAccessor, HttpUserAgentParserAccessor>();
            return options;
        }
    }
}