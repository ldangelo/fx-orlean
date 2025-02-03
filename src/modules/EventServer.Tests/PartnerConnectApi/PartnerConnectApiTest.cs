using System.Threading;
using System.Threading.Tasks;
using Alba;
using JetBrains.Annotations;
using Marten;
using Microsoft.Extensions.DependencyInjection;
using Oakton;
using Xunit.DependencyInjection;

namespace EventServer.Tests.PartnerConnectApi;

[TestSubject(typeof(common.PartnerConnect.PartnerConnectApi))]
[Collection("Fx Collection")]
public class PartnerConnectApiTest : IAsyncLifetime
{
    private IAlbaHost _host;
    private ITestOutputHelperAccessor output;

    public PartnerConnectApiTest(ITestOutputHelperAccessor accessor)
    {
        output = accessor;
    }

    public async Task InitializeAsync()
    {
        OaktonEnvironment.AutoStartHost = true;
        _host = await AlbaHost.For<Program>();

        var store = _host.Services.GetRequiredService<IDocumentStore>();
        await store.Advanced.Clean.DeleteAllDocumentsAsync();
    }

    public Task DisposeAsync()
    {
        return _host.DisposeAsync().AsTask();
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