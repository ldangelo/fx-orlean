using System.Threading.Tasks;
using EventServer.Aggregates.Partners;
using JetBrains.Annotations;
using Xunit.DependencyInjection;

namespace EventServer.Tests.Aggregates.Partners;

[TestSubject(typeof(Partner))]
[Collection("Fx Collection")]
public class PartnerAggregateTest : FxTest
{
    public PartnerAggregateTest(ITestOutputHelperAccessor accessor)
        : base(accessor) { }

    [Fact]
    public async Task PartnerDetailsTest() { }
}

