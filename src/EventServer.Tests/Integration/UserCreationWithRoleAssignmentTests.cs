using EventServer.Aggregates.Users.Commands;
using EventServer.Aggregates.Users.Events;
using EventServer.Services;
using Shouldly;
using Xunit;
using Xunit.Abstractions;

namespace EventServer.Tests.Integration;

public class UserCreationWithRoleAssignmentTests : IntegrationContext
{
    public UserCreationWithRoleAssignmentTests(AppFixture fixture, ITestOutputHelper output) 
        : base(fixture, output) { }
    [Fact]
    public async Task CreateUser_WithFortiumEmail_ShouldAssignPartnerRole()
    {
        // Arrange
        var command = new CreateUserCommand(
            FirstName: "John",
            LastName: "Partner",
            EmailAddress: "john.partner@fortiumpartners.com");

        // Act - Send command via HTTP endpoint
        await Scenario(_ =>
        {
            _.Post.Json(command).ToUrl("/users");
            _.StatusCodeShouldBe(201);
        });

        // Assert - Verify user was created with PARTNER role
        using var session = Store!.LightweightSession();
        var user = await session.LoadAsync<Fortium.Types.User>("john.partner@fortiumpartners.com");

        user.ShouldNotBeNull();
        user.Role.ShouldBe("PARTNER");
        user.FirstName.ShouldBe("John");
        user.LastName.ShouldBe("Partner");
    }

    [Fact]
    public async Task CreateUser_WithRegularEmail_ShouldAssignClientRole()
    {
        // Arrange
        var command = new CreateUserCommand(
            FirstName: "Jane",
            LastName: "Client",
            EmailAddress: "jane.client@gmail.com");

        // Act - Send command via HTTP endpoint
        await Scenario(_ =>
        {
            _.Post.Json(command).ToUrl("/users");
            _.StatusCodeShouldBe(201);
        });

        // Assert - Verify user was created with CLIENT role
        using var session = Store!.LightweightSession();
        var user = await session.LoadAsync<Fortium.Types.User>("jane.client@gmail.com");

        user.ShouldNotBeNull();
        user.Role.ShouldBe("CLIENT");
        user.FirstName.ShouldBe("Jane");
        user.LastName.ShouldBe("Client");
    }

    [Fact]
    public async Task CreateUser_WithExplicitRole_ShouldUseProvidedRole()
    {
        // Arrange
        var command = new CreateUserCommand(
            FirstName: "Admin",
            LastName: "User",
            EmailAddress: "admin@example.com",
            Role: "PARTNER");

        // Act - Send command via HTTP endpoint
        await Scenario(_ =>
        {
            _.Post.Json(command).ToUrl("/users");
            _.StatusCodeShouldBe(201);
        });

        // Assert - Verify user was created with explicitly provided role
        using var session = Store!.LightweightSession();
        var user = await session.LoadAsync<Fortium.Types.User>("admin@example.com");

        user.ShouldNotBeNull();
        user.Role.ShouldBe("PARTNER");
        user.FirstName.ShouldBe("Admin");
        user.LastName.ShouldBe("User");
    }

    [Fact]
    public async Task CreateUser_WithNullRole_ShouldAutoAssignBasedOnEmail()
    {
        // Arrange
        var command = new CreateUserCommand(
            FirstName: "Auto",
            LastName: "Assignment",
            EmailAddress: "auto@fortiumpartners.com",
            Role: null);

        // Act - Send command via HTTP endpoint
        await Scenario(_ =>
        {
            _.Post.Json(command).ToUrl("/users");
            _.StatusCodeShouldBe(201);
        });

        // Assert - Verify user was auto-assigned PARTNER role based on email domain
        using var session = Store!.LightweightSession();
        var user = await session.LoadAsync<Fortium.Types.User>("auto@fortiumpartners.com");

        user.ShouldNotBeNull();
        user.Role.ShouldBe("PARTNER");
        user.FirstName.ShouldBe("Auto");
        user.LastName.ShouldBe("Assignment");
    }
}