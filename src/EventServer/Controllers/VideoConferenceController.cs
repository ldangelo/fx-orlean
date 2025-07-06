using EventServer.Aggregates.VideoConference.Commands;
using EventServer.Aggregates.VideoConference.Events;
using Fortium.Types;
using Serilog;
using Wolverine.Http;
using Wolverine.Http.Marten;
using Wolverine.Marten;

namespace EventServer.Controllers;

public static class VideoConferenceController
{

    [WolverinePost("/videos")]
    public static (CreationResponse, IStartStream) CreateVideoConference(CreateVideoConferenceCommand command) {
        Log.Information("Creating user {Id}.", command.ConferenceId);
        var start = MartenOps.StartStream<VideoConference>(command.ConferenceId.ToString(), new VideoConferenceCreatedEvent(command.ConferenceId,command.StartTime, command.EndTime,command.UserId,command.PartnerId));
        var response = new CreationResponse("/videos/" + start.StreamId);
        return (response, start);
    }

    [WolverineGet("/videos/{ConferenceId}")]
    public static VideoConference GetConferece([Document("ConferenceId")] VideoConference conference)
    {
        Log.Information("Getting user {conferenceId}", conference.ConferenceId);
        return conference;
    }

}
