using System.Threading;
using EventServer.Tests;
using JetBrains.Annotations;
using Xunit.DependencyInjection;

namespace EventServer.Tests.PartnerConnectApi;

[TestSubject(typeof(common.PartnerConnect.PartnerConnectApi))]
[Collection("Fx Collection")]
public class PartnerConnectApiTest : FxTestFixture
{
    private ITestOutputHelperAccessor output;

    public PartnerConnectApiTest(ITestOutputHelperAccessor accessor)
    {
        output = accessor;
    }

    [Fact]
    public async void TestAuthorization()
    {
        var api = new common.PartnerConnect.PartnerConnectApi();

        var token = await api.Authorize(CancellationToken.None);

        Assert.NotNull(token);
    }

    [Fact]
    public async void TestGetUser()
    {
        var api = new common.PartnerConnect.PartnerConnectApi();
        var user = await api.GetUser("leo.dangelo@fortiumpartners.com", CancellationToken.None);

        Assert.NotNull(user);
        Assert.NotNull(user.PrimaryEmail);
    }
}

