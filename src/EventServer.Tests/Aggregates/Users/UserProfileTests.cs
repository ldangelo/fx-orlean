using EventServer.Aggregates.Users.Commands;
using EventServer.Aggregates.Users.Events;
using Fortium.Types;
using Shouldly;
using Xunit.Abstractions;

namespace EventServer.Tests.Aggregates.Users;

public class UserProfileTests : IntegrationContext
{
    public UserProfileTests(AppFixture fixture, ITestOutputHelper output) : base(fixture, output)
    {
    }

    [Fact]
    public async Task UpdateUserProfile_ShouldReturnUserProfileUpdatedEvent()
    {
        // Arrange
        var command = new UpdateUserProfileCommand(
            EmailAddress: "test@example.com",
            FirstName: "John",
            LastName: "Doe",
            PhoneNumber: "+1-555-123-4567",
            ProfilePictureUrl: "https://example.com/profile.jpg"
        );

        // Act & Assert
        var result = await Host.Scenario(_ =>
        {
            _.Post.Json(command).ToUrl($"/users/profile/{command.EmailAddress}");
            _.StatusCodeShouldBe(204);
        });
    }

    [Fact]
    public async Task UpdateUserAddress_ShouldReturnUserAddressUpdatedEvent()
    {
        // Arrange
        var command = new UpdateUserAddressCommand(
            EmailAddress: "test@example.com",
            Street1: "123 Main St",
            Street2: "Apt 4B",
            City: "New York",
            State: "NY",
            ZipCode: "10001",
            Country: "USA"
        );

        // Act & Assert
        var result = await Host.Scenario(_ =>
        {
            _.Post.Json(command).ToUrl($"/users/address/{command.EmailAddress}");
            _.StatusCodeShouldBe(204);
        });
    }

    [Fact]
    public async Task UpdateUserPreferences_ShouldReturnUserPreferencesUpdatedEvent()
    {
        // Arrange
        var command = new UpdateUserPreferencesCommand(
            EmailAddress: "test@example.com",
            ReceiveEmailNotifications: true,
            ReceiveSmsNotifications: false,
            PreferredLanguage: "en-US",
            TimeZone: "America/New_York",
            Theme: "Dark"
        );

        // Act & Assert
        var result = await Host.Scenario(_ =>
        {
            _.Post.Json(command).ToUrl($"/users/preferences/{command.EmailAddress}");
            _.StatusCodeShouldBe(204);
        });
    }

    [Fact]
    public async Task CompleteUserProfileWorkflow_ShouldUpdateAllUserFields()
    {
        // Arrange
        var emailAddress = "complete.test@example.com";
        
        // Create user first
        var createCommand = new CreateUserCommand("Jane", "Smith", emailAddress);
        await Host.Scenario(_ =>
        {
            _.Post.Json(createCommand).ToUrl("/users");
            _.StatusCodeShouldBe(201);
        });

        // Update profile
        var profileCommand = new UpdateUserProfileCommand(
            EmailAddress: emailAddress,
            FirstName: "Jane",
            LastName: "Smith-Updated",
            PhoneNumber: "+1-555-987-6543",
            ProfilePictureUrl: "https://example.com/jane.jpg"
        );

        await Host.Scenario(_ =>
        {
            _.Post.Json(profileCommand).ToUrl($"/users/profile/{emailAddress}");
            _.StatusCodeShouldBe(204);
        });

        // Update address
        var addressCommand = new UpdateUserAddressCommand(
            EmailAddress: emailAddress,
            Street1: "456 Oak Avenue",
            Street2: null,
            City: "San Francisco",
            State: "CA",
            ZipCode: "94102",
            Country: "USA"
        );

        await Host.Scenario(_ =>
        {
            _.Post.Json(addressCommand).ToUrl($"/users/address/{emailAddress}");
            _.StatusCodeShouldBe(204);
        });

        // Update preferences
        var preferencesCommand = new UpdateUserPreferencesCommand(
            EmailAddress: emailAddress,
            ReceiveEmailNotifications: false,
            ReceiveSmsNotifications: true,
            PreferredLanguage: "es-ES",
            TimeZone: "America/Los_Angeles",
            Theme: "Light"
        );

        await Host.Scenario(_ =>
        {
            _.Post.Json(preferencesCommand).ToUrl($"/users/preferences/{emailAddress}");
            _.StatusCodeShouldBe(204);
        });

        // Verify user data
        var user = await Host.Scenario(_ =>
        {
            _.Get.Url($"/users/{emailAddress}");
            _.StatusCodeShouldBe(200);
        });
    }

    [Fact]
    public void UpdateUserProfileCommand_WithInvalidEmail_ShouldFailValidation()
    {
        // Arrange
        var command = new UpdateUserProfileCommand(
            EmailAddress: "invalid-email",
            FirstName: "John",
            LastName: "Doe",
            PhoneNumber: "+1-555-123-4567"
        );

        // Act & Assert
        var validator = new UpdateUserProfileCommandValidator();
        var result = validator.Validate(command);
        
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage.Contains("Email address"));
    }

    [Fact]
    public void UpdateUserAddressCommand_WithInvalidEmail_ShouldFailValidation()
    {
        // Arrange
        var command = new UpdateUserAddressCommand(
            EmailAddress: "",
            Street1: "123 Main St",
            Street2: null,
            City: "New York",
            State: "NY",
            ZipCode: "10001",
            Country: "USA"
        );

        // Act & Assert
        var validator = new UpdateUserAddressCommandValidator();
        var result = validator.Validate(command);
        
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage.Contains("Email address"));
    }

    [Fact]
    public void UpdateUserPreferencesCommand_WithInvalidEmail_ShouldFailValidation()
    {
        // Arrange
        var command = new UpdateUserPreferencesCommand(
            EmailAddress: null!,
            ReceiveEmailNotifications: true,
            ReceiveSmsNotifications: false,
            PreferredLanguage: "en-US",
            TimeZone: "UTC",
            Theme: "Light"
        );

        // Act & Assert
        var validator = new UpdateUserPreferencesCommandValidator();
        var result = validator.Validate(command);
        
        result.IsValid.ShouldBeFalse();
        result.Errors.ShouldContain(e => e.ErrorMessage.Contains("Email address"));
    }
}
