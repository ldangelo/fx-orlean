using EventServer.Aggregates.VideoConference;
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
    public static (CreationResponse, IStartStream) CreateVideoConference(CreateVideoConferenceCommand command) 
    {
        Log.Information("Creating video conference {Id}.", command.ConferenceId);
        
        var start = MartenOps.StartStream<VideoConferenceState>(
            command.ConferenceId.ToString(), 
            new VideoConferenceCreatedEvent(
                command.ConferenceId,
                command.StartTime, 
                command 
                    .EndTime,
                command.UserId,
                command.PartnerId,
                command.RateInformation
            )
        );
        
        var response = new CreationResponse("/videos/" + start.StreamId);
        return (response, start);
    }

    [WolverineGet("/videos/{ConferenceId}")]
    public static VideoConferenceState GetConference([Document("ConferenceId")] VideoConferenceState conference)
    {
        Log.Information("Getting video conference {conferenceId}", conference.Id);
        return conference;
    }
}
