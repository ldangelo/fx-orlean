using EventServer.Aggregates.Users.Commands;
using Fortium.Types;
using Xunit.Abstractions;

namespace EventServer.Tests;

public class UserTests : IntegrationContext
{
    public UserTests(AppFixture fixture, ITestOutputHelper output)
        : base(fixture, output) { }

    [Fact]
    public async Task CreateUserTest()
    {
        var command = new CreateUserCommand("test","test","test@gmail.com");

        var initial = await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/users");
            x.StatusCodeShouldBe(201);
        });

        var getuser = await Scenario(x =>
        {
            x.Get.Url("/users/test@gmail.com");
            x.StatusCodeShouldBe(200);
        });
    }

    [Fact]
    public async Task UserLoginTest()
    {
        var command = new CreateUserCommand("test","test","test@gmail.com");

        var initial = await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/users");
            x.StatusCodeShouldBe(201);
        });

        var login = await Scenario(x =>
        {
            x.Post.Json(new UserLoggedInCommand("test@gmail.com", DateTime.Now)).ToUrl("/users/login/test@gmail.com");
            x.StatusCodeShouldBe(204);
        });
    }

    [Fact]
    public async Task UserLogoutnTest()
    {
        var command = new CreateUserCommand("test","test","test@gmail.com");

        var initial = await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/users");
            x.StatusCodeShouldBe(201);
        });

        var login = await Scenario(x =>
        {
            x.Post.Json(new UserLoggedInCommand("test@gmail.com", DateTime.Now)).ToUrl("/users/login/test@gmail.com");
            x.StatusCodeShouldBe(204);
        });

        var logoff = await Scenario(x =>
        {
            x.Post.Json(new UserLoggedOutCommand("test@gmail.com", DateTime.Now)).ToUrl("/users/logout/test@gmail.com");
            x.StatusCodeShouldBe(204);
        });
     }

    [Fact]
    public async Task AddVideoToUserTest()
    {
        var command = new CreateUserCommand("test","test","test@gmail.com");

        var initial = await Scenario(x =>
        {
            x.Post.Json(command).ToUrl("/users");
            x.StatusCodeShouldBe(201);
        });

        var addconference = await Scenario(x => {
           x.Post.Json(new AddVideoConferenceToUserCommand("test@gmail.com", Guid.NewGuid())).ToUrl("/users/video/test@gmail.com");
           x.StatusCodeShouldBe(204);
            });

        var getuser = Scenario(x =>
        {
            x.Get.Url("/users/test@gmail.com");
            x.StatusCodeShouldBe(200);
        }).Result.ReadAsJson<User>();

        Assert.True(getuser.VideoConferences.Count > 0);
     }
}
