using EventServer.Aggregates.Partners.Commands;
using Fortium.Types;
using Shouldly;
using Xunit.Abstractions;

namespace EventServer.Tests;

public class PartnerTests : IntegrationContext
{
    public PartnerTests(AppFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public async Task TestPartnerCreation()
    {
        var command = new CreatePartnerCommand("test", "test", "test");
        //        var (response, stream) = PartnerController.CreatePartners(command);

        var initial = await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/partners");
            x.StatusCodeShouldBe(201);
        });
    }

    [Fact]
    public async Task TestDupplicateCreation()
    {
        var command = new CreatePartnerCommand("test", "test", "test");
        //        var (response, stream) = PartnerController.CreatePartners(command);

        var initial = await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/partners");
            x.StatusCodeShouldBe(201);
        });

        var second = await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/partners");
            x.StatusCodeShouldBe(500);
        });
    }

    [Fact]
    public async Task TestLogin()
    {
        var command = new CreatePartnerCommand("test", "test", "test");
        //        var (response, stream) = PartnerController.CreatePartners(command);

        var initial = await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/partners");
            x.StatusCodeShouldBe(201);
        });

        var login = new PartnerLoggedInCommand("test", DateTime.UtcNow);
        var loginResponse = await Scenario(x =>
        {
            x.Post.Json(login).ToUrl("/partners/loggedin/test");
            x.StatusCodeShouldBe(204);
        });

        Thread.Sleep(2000);
        var partner = Scenario(x =>
        {
            x.Get.Url("/partners/test");
            x.StatusCodeShouldBe(200);
        })
            .Result.ReadAsJson<Partner>();

        partner.Active.ShouldBeTrue();
        partner.LoggedIn.ShouldBeTrue();
    }
}

