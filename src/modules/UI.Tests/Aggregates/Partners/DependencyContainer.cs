using System;
using Microsoft.Extensions.DependencyInjection;
using Whaally.Domain;

namespace UI.Tests.Grains.Partners;

internal class DependencyContainer
{
    public static IServiceCollection Services { get; } = new ServiceCollection().AddDomain();
    
    public static IServiceProvider Create()
    {
        return Services.BuildServiceProvider();
    }
}