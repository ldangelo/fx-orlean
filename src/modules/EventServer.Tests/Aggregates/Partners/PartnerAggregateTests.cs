using System.Threading.Tasks;
using FxExpert.Aggregates.Partners;
using JetBrains.Annotations;
using Xunit.DependencyInjection;

namespace EventServer.Tests.Aggregates.Partners;

[TestSubject(typeof(PartnerAggregate))]
[Collection("Fx Collection")]
public class PartnerAggregateTest : FxTest
{
    public PartnerAggregateTest(ITestOutputHelperAccessor accessor) : base(accessor)
    {
    }

    [Fact]
    public async Task PartnerDetailsTest()
    {
    }
}