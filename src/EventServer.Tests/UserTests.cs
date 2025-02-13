using EventServer.Aggregates.Users.Commands;
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
        }
}
