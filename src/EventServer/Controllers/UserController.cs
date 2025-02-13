using EventServer.Aggregates.Users.Commands;
using EventServer.Aggregates.Users.Events;
using Fortium.Types;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Wolverine.Http;
using Wolverine.Http.Marten;
using Wolverine.Marten;

namespace EventServer.Controllers;

public static class UserController {
    [WolverinePost("/users")]
    public static (CreationResponse, IStartStream) CreateUsers(CreateUserCommand command) {
        Log.Information("Creating user {Id}.", command.EmailAddress);
        var start = MartenOps.StartStream<User>(command.EmailAddress, new UserCreatedEvent(command.FirstName, command.LastName, command.EmailAddress));
        var response = new CreationResponse("/users/" + start.StreamId);
        return (response, start);
    }

    [WolverinePost("/users/video/{userId}")]
    [EmptyResponse]
    public static VideoConferenceAddedToUserEvent AddConference(
        [FromBody] AddVideoConferenceToUserCommand command,
        [Aggregate] User user) {
        Log.Information("Adding video conference {conferenceId} to user {emailAddress}",command.ConferenceId, command.EmailAddress);
        return new VideoConferenceAddedToUserEvent(command.EmailAddress,command.ConferenceId);
    }

    [WolverinePost("/users/login/{userId}")]
    [EmptyResponse]
    public static UserLoggedInEvent UserLogin(
        [FromBody] UserLoggedInCommand command,
        [Aggregate] User user) {
        Log.Information("Logging User In {date} to user {emailAddress}",command.LoginDate, command.EmailAddress);
        return new UserLoggedInEvent(command.EmailAddress,command.LoginDate);
    }


    [WolverinePost("/users/logout/{userId}")]
    [EmptyResponse]
    public static UserLoggedOutEvent UserLogout(
        [FromBody] UserLoggedOutCommand command,
        [Aggregate] User user) {
        Log.Information("Logging User out {date} to user {emailAddress}",command.LogoutDate, command.EmailAddress);
        return new UserLoggedOutEvent(command.EmailAddress,command.LogoutDate);
    }


[WolverineGet("/users/{EmailAddress}")]
    public static User GetUser([Document("EmailAddress")] User user)
    {
        Log.Information("Getting user {emailAddress}", user.EmailAddress);
        return user;
    }
}
