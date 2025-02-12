using EventServer.Aggregates.Partners.Commands;
using Fortium.Types;
using Serilog;
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

        Log.Information("Partner: {p}",partner.ToString());
        partner.Active.ShouldBeTrue();
        partner.LoggedIn.ShouldBeTrue();
    }

    [Fact]
    public async Task TestLogout()
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

        var logout = new PartnerLoggedOutCommand("test", DateTime.UtcNow);
        var logoutResponse = await Scenario(x =>
        {
            x.Post.Json(logout).ToUrl("/partners/loggedout/test");
            x.StatusCodeShouldBe(204);
        });

        Thread.Sleep(2000);
        var partner = Scenario(x =>
        {
            x.Get.Url("/partners/test");
            x.StatusCodeShouldBe(200);
        })
            .Result.ReadAsJson<Partner>();

        Log.Information("Partner: {p}",partner.ToString());
        partner?.Active.ShouldBeTrue();
        partner?.LoggedIn.ShouldBeFalse();
    }
}

