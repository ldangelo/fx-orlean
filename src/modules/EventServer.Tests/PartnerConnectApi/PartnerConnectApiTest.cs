using JetBrains.Annotations;
using UI.Tests;
using Xunit.DependencyInjection;
using System.Threading;

namespace EventServer.Tests.PartnerConnectApi;

[TestSubject(typeof(common.PartnerConnect.PartnerConnectApi))]
public class PartnerConnectApiTest : FxTest
{
    private ITestOutputHelperAccessor output;

    public PartnerConnectApiTest(ITestOutputHelperAccessor accessor) : base(accessor)
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
