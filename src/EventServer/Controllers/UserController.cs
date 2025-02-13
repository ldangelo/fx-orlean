using common.PartnerConnect;
using EventServer.Aggregates.Users.Commands;
using EventServer.Aggregates.Users.Events;
using Serilog;
using Wolverine.Http;
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
}
