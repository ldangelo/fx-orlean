using System.Threading;
using System.Threading.Tasks;

namespace EventServer.Tests.PartnerConnectApi;

public class PartnerConnectApiTest : IntegrationContext
{
    public PartnerConnectApiTest(AppFixture fixture)
        : base(fixture) { }

    [Fact]
    public async Task TestAuthorization()
    {
        var api = new common.PartnerConnect.PartnerConnectApi();

        var token = await api.Authorize(CancellationToken.None);

        Assert.NotNull(token);
    }

    [Fact]
    public async Task TestGetUser()
    {
        var api = new common.PartnerConnect.PartnerConnectApi();
        var user = await api.GetUser("leo.dangelo@fortiumpartners.com", CancellationToken.None);

        Assert.NotNull(user);
        Assert.NotNull(user.PrimaryEmail);
    }
}
