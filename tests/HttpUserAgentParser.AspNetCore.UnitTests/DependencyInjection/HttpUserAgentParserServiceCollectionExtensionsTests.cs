// Copyright © https://myCSharp.de - all rights reserved

using Microsoft.AspNetCore.Http;
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

    [Fact]
    public void AddHttpUserAgentParserContextAccessor_Offers_Both_Accessors()
    {
        ServiceCollection services = new();

        services.AddHttpUserAgentParser().AddHttpUserAgentParserContextAccessor();

        using ServiceProvider provider = services.BuildServiceProvider();
        IHttpUserAgentParserContextAccessor contextAccessor = provider.GetRequiredService<IHttpUserAgentParserContextAccessor>();

        Assert.IsType<HttpUserAgentParserContextAccessor>(contextAccessor);
        Assert.Same(contextAccessor, provider.GetRequiredService<IHttpUserAgentParserAccessor>());
        Assert.NotNull(provider.GetService<IHttpContextAccessor>());
    }

    [Fact]
    public void AddHttpUserAgentParserContextAccessor_Keeps_A_Registered_Accessor()
    {
        ServiceCollection services = new();

        services.AddHttpUserAgentParser().AddHttpUserAgentParserAccessor().AddHttpUserAgentParserContextAccessor();

        using ServiceProvider provider = services.BuildServiceProvider();

        Assert.IsType<HttpUserAgentParserAccessor>(provider.GetRequiredService<IHttpUserAgentParserAccessor>());
        Assert.IsType<HttpUserAgentParserContextAccessor>(provider.GetRequiredService<IHttpUserAgentParserContextAccessor>());
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Accessor_Registrations_Are_Idempotent_In_Any_Order(bool contextAccessorFirst)
    {
        ServiceCollection services = new();
        HttpUserAgentParserDependencyInjectionOptions options = services.AddHttpUserAgentParser();

        if (contextAccessorFirst)
        {
            options.AddHttpUserAgentParserContextAccessor().AddHttpUserAgentParserAccessor();
        }
        else
        {
            options.AddHttpUserAgentParserAccessor().AddHttpUserAgentParserContextAccessor();
        }

        options.AddHttpUserAgentParserContextAccessor().AddHttpUserAgentParserAccessor();

        using ServiceProvider provider = services.BuildServiceProvider();
        Assert.Single(provider.GetServices<IHttpUserAgentParserAccessor>());
        Assert.Single(provider.GetServices<IHttpUserAgentParserContextAccessor>());
    }
}
