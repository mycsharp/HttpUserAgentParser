// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.Extensions.DependencyInjection;
using MyCSharp.HttpUserAgentParser.AspNetCore.DependencyInjection;
using MyCSharp.HttpUserAgentParser.DependencyInjection;
using Xunit;

namespace MyCSharp.HttpUserAgentParser.AspNetCore.UnitTests.DependencyInjection;

public class HttpUserAgentParserDependencyInjectionOptionsExtensionsTests
{
    [Fact]
    public void AddHttpUserAgentParserAccessor()
    {
        ServiceCollection services = new();
        HttpUserAgentParserDependencyInjectionOptions options = new(services);

        options.AddHttpUserAgentParserAccessor();

        Assert.Single(services);

        Assert.Equal(typeof(IHttpUserAgentParserAccessor), services[0].ServiceType);
        Assert.Equal(typeof(HttpUserAgentParserAccessor), services[0].ImplementationType);
        Assert.Equal(ServiceLifetime.Singleton, services[0].Lifetime);
    }
}
