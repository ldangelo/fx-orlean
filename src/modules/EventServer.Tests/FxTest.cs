using Serilog;
using UI.Tests.Grains.Partners;
using Xunit.DependencyInjection;

namespace UI.Tests;

public class FxTest
{
    public FxTest(ITestOutputHelperAccessor accessor)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.TestOutput(accessor.Output, Serilog.Events.LogEventLevel.Debug)
            .CreateLogger()
            .ForContext<PartnerAggregateTest>();
    }
}
