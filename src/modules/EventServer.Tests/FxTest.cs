using EventServer.Tests.Aggregates.Partners;
using Serilog;
using Serilog.Events;
using Xunit.DependencyInjection;

namespace EventServer.Tests;

public class FxTest
{
    public FxTest(ITestOutputHelperAccessor accessor)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .WriteTo.TestOutput(accessor.Output, LogEventLevel.Debug)
            .CreateLogger()
            .ForContext<PartnerAggregateTest>();
    }
}