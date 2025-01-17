using System;
using Orleankka;
using Serilog;
using Xunit.DependencyInjection;

namespace UI.Tests;

public class FxTestFixture : IDisposable
{
    private static IActorSystem _system;

    public LoggerConfiguration Logger;

    public FxTestFixture(IActorSystem actorSystem, ITestOutputHelperAccessor outputHelperAccessor)
    {
        _system = actorSystem;
    }

    public void Dispose() { }

    public IActorSystem getActorSystem()
    {
        return _system;
    }
}

[CollectionDefinition("Fx Collection")]
public class FxTestCollection : ICollectionFixture<FxTestFixture> { }

