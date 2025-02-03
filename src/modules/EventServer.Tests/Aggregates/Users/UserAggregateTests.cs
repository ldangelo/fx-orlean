using Xunit.DependencyInjection;

namespace EventServer.Tests.Aggregates.Users;

public class UserAggregateTests : FxTest
{
    public UserAggregateTests(ITestOutputHelperAccessor accessor) : base(accessor)
    {
    }
}