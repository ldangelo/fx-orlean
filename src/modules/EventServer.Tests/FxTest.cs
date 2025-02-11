using EventServer.Tests.Aggregates.Partners;
using Serilog;
using Serilog.Events;

namespace EventServer.Tests;

public class FxTest
{
    public FxTest()
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
//            .WriteTo.TestOutput(accessor.Output, LogEventLevel.Debug)
            .CreateLogger()
            .ForContext<PartnerAggregateTest>();
    }
}
