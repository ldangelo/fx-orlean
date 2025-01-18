using JetBrains.Annotations;
using UI.Tests;
using Xunit.DependencyInjection;

namespace EventServer.Tests.PartnerConnectApi;

[TestSubject(typeof(common.PartnerConnectApi.PartnerConnectApi))]
public class PartnerConnectApiTest : FxTest
{
    private ITestOutputHelperAccessor output;

    public PartnerConnectApiTest(ITestOutputHelperAccessor accessor) : base(accessor)
    {
        output = accessor;
    }

    [Fact]
    public void TestAuthorization()
    {
        var api = new common.PartnerConnectApi.PartnerConnectApi();

        var token = api.Authorize();
        
        Assert.NotNull(token);
    }
}