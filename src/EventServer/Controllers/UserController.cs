using EventServer.Aggregates.Users;
using EventServer.Aggregates.Users.Commands;
using EventServer.Aggregates.Users.Events;
using Fortium.Types;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Wolverine.Http;
using Wolverine.Http.Marten;
using Wolverine.Marten;

namespace EventServer.Controllers;

public static class UserController
{
    [WolverinePost("/users")]
    public static (CreationResponse, IStartStream) CreateUsers(CreateUserCommand command)
    {
        Log.Information("Creating user {Id}.", command.EmailAddress);
        var start = MartenOps.StartStream<User>(
            command.EmailAddress,
            new UserCreatedEvent(command.FirstName, command.LastName, command.EmailAddress)
        );
        var response = new CreationResponse("/users/" + start.StreamId);
        return (response, start);
    }

    [WolverinePost("/users/video/{EmailAddress}")]
    [EmptyResponse]
    public static VideoConferenceAddedToUserEvent AddConference(
        [FromBody] AddVideoConferenceToUserCommand command,
        [Aggregate("EmailAddress")] User user
    )
    {
        if (user == null)
        {
            throw new KeyNotFoundException($"User not found: {command.EmailAddress}");
        }
        
        Log.Information(
            "Adding video conference {conferenceId} to user {emailAddress}",
            command.ConferenceId,
            command.EmailAddress
        );
        return new VideoConferenceAddedToUserEvent(command.EmailAddress, command.ConferenceId);
    }

    [WolverinePost("/users/login/{EmailAddress}")]
    [EmptyResponse]
    public static UserLoggedInEvent UserLogin(
        [FromBody] UserLoggedInCommand command,
        [Aggregate("EmailAddress")] User user
    )
    {
        if (user == null)
        {
            throw new KeyNotFoundException($"User not found: {command.EmailAddress}");
        }

        Log.Information(
            "Logging User In {date} to user {emailAddress}",
            command.LoginDate,
            command.EmailAddress
        );
        return new UserLoggedInEvent(command.EmailAddress, command.LoginDate);
    }

    [WolverinePost("/users/logout/{EmailAddress}")]
    [EmptyResponse]
    public static UserLoggedOutEvent UserLogout(
        [FromBody] UserLoggedOutCommand command,
        [Aggregate("EmailAddress")] User user
    )
    {
        if (user == null)
        {
            throw new KeyNotFoundException($"User not found: {command.EmailAddress}");
        }

        Log.Information(
            "Logging User out {date} to user {emailAddress}",
            command.LogoutDate,
            command.EmailAddress
        );
        return new UserLoggedOutEvent(command.EmailAddress, command.LogoutDate);
    }

    /*
     * GetUser: Get's a user by email address
     */
    [WolverineGet("/users/{EmailAddress}")]
    public static IResult GetUser([Document("EmailAddress")] User user)
    {
        if (user == null)
        {
            Log.Warning("User not found");
            return Results.NotFound();
        }

        Log.Information("Getting user {emailAddress}", user.EmailAddress);
        return Results.Ok(user);
    }

    [WolverinePost("/users/profile/{EmailAddress}")]
    [EmptyResponse]
    public static UserProfileUpdatedEvent UpdateUserProfile(
        [FromBody] UpdateUserProfileCommand command,
        [Aggregate("EmailAddress")] User user
    )
    {
        if (user == null)
        {
            Log.Warning("User not found: {emailAddress}", command.EmailAddress);
            throw new KeyNotFoundException($"User not found: {command.EmailAddress}");
        }

        var validator = new UpdateUserProfileCommandValidator();
        var result = validator.Validate(command);
        if (!result.IsValid)
        {
            throw new ValidationException("Invalid profile update command", result.Errors);
        }

        Log.Information("Updating profile for user {emailAddress}", command.EmailAddress);
        return new UserProfileUpdatedEvent(
            command.EmailAddress,
            command.FirstName,
            command.LastName,
            command.PhoneNumber,
            command.ProfilePictureUrl
        );
    }

    [WolverinePost("/users/address/{EmailAddress}")]
    [EmptyResponse]
    public static UserAddressUpdatedEvent UpdateUserAddress(
        [FromBody] UpdateUserAddressCommand command,
        [Aggregate("EmailAddress")] User user
    )
    {
        if (user == null)
        {
            Log.Warning("User not found: {emailAddress}", command.EmailAddress);
            throw new KeyNotFoundException($"User not found: {command.EmailAddress}");
        }

        var validator = new UpdateUserAddressCommandValidator();
        var result = validator.Validate(command);
        if (!result.IsValid)
        {
            throw new ValidationException("Invalid address update command", result.Errors);
        }

        Log.Information("Updating address for user {emailAddress}", command.EmailAddress);
        return new UserAddressUpdatedEvent(
            command.EmailAddress,
            command.Street1,
            command.Street2,
            command.City,
            command.State,
            command.ZipCode,
            command.Country
        );
    }

    [WolverinePost("/users/preferences/{EmailAddress}")]
    [EmptyResponse]
    public static UserPreferencesUpdatedEvent UpdateUserPreferences(
        [FromBody] UpdateUserPreferencesCommand command,
        [Aggregate("EmailAddress")] User user
    )
    {
        if (user == null)
        {
            Log.Warning("User not found: {emailAddress}", command.EmailAddress);
            throw new KeyNotFoundException($"User not found: {command.EmailAddress}");
        }

        var validator = new UpdateUserPreferencesCommandValidator();
        var result = validator.Validate(command);
        if (!result.IsValid)
        {
            throw new ValidationException("Invalid preferences update command", result.Errors);
        }

        Log.Information("Updating preferences for user {emailAddress}", command.EmailAddress);
        return new UserPreferencesUpdatedEvent(
            command.EmailAddress,
            command.ReceiveEmailNotifications,
            command.ReceiveSmsNotifications,
            command.PreferredLanguage,
            command.TimeZone,
            command.Theme
        );
    }

    [WolverinePost("/users/theme/{EmailAddress}")]
    [EmptyResponse]
    public static UserThemeUpdatedEvent UpdateUserTheme(
        [FromBody] UpdateUserThemeCommand command,
        [Aggregate("EmailAddress")] User user
    )
    {
        if (user == null)
        {
            Log.Warning("User not found: {emailAddress}", command.EmailAddress);
            throw new KeyNotFoundException($"User not found: {command.EmailAddress}");
        }

        var validator = new UpdateUserThemeCommandValidator();
        var result = validator.Validate(command);
        if (!result.IsValid)
        {
            throw new ValidationException("Invalid theme update command", result.Errors);
        }

        Log.Information("Updating theme to {theme} for user {emailAddress}", command.Theme, command.EmailAddress);
        return new UserThemeUpdatedEvent(command.EmailAddress, command.Theme);
    }

    [WolverineGet("/users/theme/{EmailAddress}")]
    public static IResult GetUserTheme([Document("EmailAddress")] User user)
    {
        if (user == null)
        {
            Log.Warning("User not found");
            return Results.NotFound();
        }

        var theme = user.Preferences?.Theme ?? "Light";
        Log.Information("Getting theme {theme} for user {emailAddress}", theme, user.EmailAddress);
        return Results.Ok(new { Theme = theme });
    }
}
