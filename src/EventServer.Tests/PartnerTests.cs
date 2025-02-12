using EventServer.Aggregates.Partners.Commands;
using EventServer.Controllers;
using Fortium.Types;
using Shouldly;

namespace EventServer.Tests;

public class PartnerTests : IntegrationContext
{
    public PartnerTests(AppFixture fixture)
        : base(fixture) { }

    [Fact]
    public void TestPartnerCreation()
    {
        var command = new CreatePartnerCommand("test", "test", "test");
        //        var (response, stream) = PartnerController.CreatePartners(command);

        var initial = Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/partners");
            x.StatusCodeShouldBe(201);
        });
    }

    [Fact]
    public void TestDupplicateCreation()
    {
        var command = new CreatePartnerCommand("test", "test", "test");
        //        var (response, stream) = PartnerController.CreatePartners(command);

        var initial = Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/partners");
            x.StatusCodeShouldBe(201);
        });

        var second = Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/partners");
            x.StatusCodeShouldBe(500);
        });
    }

    [Fact]
    public void TestLogin()
    {
        var command = new CreatePartnerCommand("test", "test", "test");
        //        var (response, stream) = PartnerController.CreatePartners(command);

        var initial = Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/partners");
            x.StatusCodeShouldBe(201);
        });

        var login = new PartnerLoggedInCommand("test", DateTime.UtcNow);
        var loginResponse = Scenario(x =>
        {
            x.Post.Json(login).ToUrl("/partners/login");
            x.StatusCodeShouldBe(201);
        });

        var partner = Scenario(x =>
        {
            x.Get.Url("/partners/test");
            x.StatusCodeShouldBe(200);
        })
            .Result.ReadAsJson<Partner>();

        partner.LoggedIn.ShouldBeTrue();
    }
}

