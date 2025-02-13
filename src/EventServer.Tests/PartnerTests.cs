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
        var command = new CreatePartnerCommand("test", "test", "test@fortiumpartners.com");
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
        var command = new CreatePartnerCommand("test", "test", "test@fortiumpartners.com");
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
        var command = new CreatePartnerCommand("test", "test", "test@fortiumpartners.com");
        //        var (response, stream) = PartnerController.CreatePartners(command);

        var initial = await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/partners");
            x.StatusCodeShouldBe(201);
        });

        var login = new PartnerLoggedInCommand("test@fortiumpartners.com", DateTime.UtcNow);
        var loginResponse = await Scenario(x =>
        {
            x.Post.Json(login).ToUrl("/partners/loggedin/test@fortiumpartners.com");
            x.StatusCodeShouldBe(204);
        });

        var partner = Scenario(x =>
        {
            x.Get.Url("/partners/test@fortiumpartners.com");
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
        var command = new CreatePartnerCommand("test", "test", "test@fortiumpartners.com");
        //        var (response, stream) = PartnerController.CreatePartners(command);

        var initial = await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/partners");
            x.StatusCodeShouldBe(201);
        });

        var login = new PartnerLoggedInCommand("test@fortiumpartners.com", DateTime.UtcNow);
        var loginResponse = await Scenario(x =>
        {
            x.Post.Json(login).ToUrl("/partners/loggedin/test@fortiumpartners.com");
            x.StatusCodeShouldBe(204);
        });

        var logout = new PartnerLoggedOutCommand("test@fortiumpartners.com", DateTime.UtcNow);

        var logoutResponse = await Scenario(x =>
        {
            x.Post.Json(logout).ToUrl("/partners/loggedout/test@fortiumpartners.com");
            x.StatusCodeShouldBe(204);
        });

        var partner = Scenario(x =>
        {
            x.Get.Url("/partners/test@fortiumpartners.com");
            x.StatusCodeShouldBe(200);
        })
            .Result.ReadAsJson<Partner>();

        Log.Information("Partner: {p}",partner.ToString());
        partner?.Active.ShouldBeTrue();
        partner?.LoggedIn.ShouldBeFalse();
    }

    [Fact]
    public async Task PartnerUpdateBioTest()
        {
        var command = new CreatePartnerCommand("test", "test", "test@fortiumpartners.com");
        //        var (response, stream) = PartnerController.CreatePartners(command);

        var initial = await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/partners");
            x.StatusCodeShouldBe(201);
        });

        var update = new SetPartnerBioCommand("test@fortiumpartners.com", "This is your new bio!");

        var bio = await Scenario(x => {
            x.Post.Json(update).ToUrl("/partners/bio/test@fortiumpartners.com");
            x.StatusCodeShouldBe(204);
            });

        }

    [Fact]
    public async Task PartnerUpdateSkillsTest()
        {
        var command = new CreatePartnerCommand("test", "test", "test@fortiumpartners.com");
        //        var (response, stream) = PartnerController.CreatePartners(command);

        var initial = await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/partners");
            x.StatusCodeShouldBe(201);
        });

        var update = new AddPartnerSkillCommand("test@fortiumpartners.com", [ new PartnerSkill("C#",10,ExperienceLevel.Proficient), new PartnerSkill("AWS",15,ExperienceLevel.Expert) ]);

        var bio = await Scenario(x => {
            x.Post.Json(update).ToUrl("/partners/skills/test@fortiumpartners.com");
            x.StatusCodeShouldBe(204);
            });

        var partner = Scenario(x =>
        {
            x.Get.Url("/partners/test@fortiumpartners.com");
            x.StatusCodeShouldBe(200);
        })
            .Result.ReadAsJson<Partner>();

        Log.Information("Partner: {p}",partner.ToString());
        Assert.NotNull(partner.Skills.Find(x => x.Skill.Equals("AWS")));
        Assert.NotNull(partner.Skills.Find(x => x.Skill.Equals("C#")));
        Assert.Null(partner.Skills.Find(x => x.Skill.Equals("F#")));
        }

    [Fact]
    public async Task PartnerPrimaryPhoneTest()
        {
        var command = new CreatePartnerCommand("test", "test", "test@fortiumpartners.com");
        //        var (response, stream) = PartnerController.CreatePartners(command);

        var initial = await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/partners");
            x.StatusCodeShouldBe(201);
        });

        var update = new SetPartnerPrimaryPhoneCommand("test@fortiumpartners.com", "9729790116");

        var bio = await Scenario(x => {
            x.Post.Json(update).ToUrl("/partners/primaryphone/test@fortiumpartners.com");
            x.StatusCodeShouldBe(204);
            });

        var partner = Scenario(x =>
        {
            x.Get.Url("/partners/test@fortiumpartners.com");
            x.StatusCodeShouldBe(200);
        })
            .Result.ReadAsJson<Partner>();

        Log.Information("Partner: {p}",partner.ToString());
        partner.PrimaryPhone.ShouldNotBeEmpty();
        }

    [Fact]
    public async Task PartnerPhotoUrlTest()
        {
        var command = new CreatePartnerCommand("test", "test", "test@fortiumpartners.com");
        //        var (response, stream) = PartnerController.CreatePartners(command);

        var initial = await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/partners");
            x.StatusCodeShouldBe(201);
        });

        var update = new SetPartnerPhotoUrlCommand("test@fortiumpartners.com", "http://my.photo/");

        var photo = await Scenario(x => {
            x.Post.Json(update).ToUrl("/partners/photourl/test@fortiumpartners.com");
            x.StatusCodeShouldBe(204);
            });

        var partner = Scenario(x =>
        {
            x.Get.Url("/partners/test@fortiumpartners.com");
            x.StatusCodeShouldBe(200);
        })
            .Result.ReadAsJson<Partner>();

        Log.Information("Partner: {p}",partner.ToString());
        partner.PhotoUrl.ShouldNotBeEmpty();
        }
}
