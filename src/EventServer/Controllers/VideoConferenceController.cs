using EventServer.Aggregates.VideoConference;
using EventServer.Aggregates.VideoConference.Commands;
using EventServer.Aggregates.VideoConference.Events;
using FluentValidation.Results;
using Fortium.Types;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using Wolverine.Http;
using Wolverine.Http.Marten;
using Wolverine.Marten;

namespace EventServer.Controllers;

public static class VideoConferenceController
{
    [WolverinePost("/conferences")]
    public static (CreationResponse, IStartStream) CreateVideoConference(CreateVideoConferenceCommand command)
    {
        Log.Information("Creating video conference {Id}.", command.ConferenceId);
        
        if (!ValidateRate(command.RateInformation, command.StartTime, command.EndTime))
        {
            throw new ValidationException("Invalid rate configuration", new List<ValidationFailure>());
        }

        var start = MartenOps.StartStream<VideoConferenceState>(
            command.ConferenceId.ToString(),
            new VideoConferenceCreatedEvent(
                command.ConferenceId,
                command.StartTime, 
                command.EndTime,
                command.UserId,
                command.PartnerId,
                command.RateInformation
            )
        );
        
        return (new CreationResponse($"/conferences/{command.ConferenceId}"), start);
    }

    [WolverinePost("/videos")]
    public static (CreationResponse, IStartStream) CreateVideoConferenceVideo(CreateVideoConferenceCommand command)
    {
        return CreateVideoConference(command);
    }

    [WolverineGet("/videos/{conferenceId}")]
    public static IResult GetVideoConference(
        [FromRoute] Guid conferenceId,
        [Document] VideoConferenceState conference)
    {
        if (conference == null)
        {
            Log.Warning("Video conference {conferenceId} not found", conferenceId);
            return Results.NotFound();
        }

        Log.Information("Getting video conference {conferenceId}", conferenceId);
        
        return Results.Ok(conference);
    }

    [WolverineGet("/conferences/{conferenceId}")]
    public static IResult GetConference(
        [FromRoute] Guid conferenceId,
        [Document] VideoConferenceState conference)
    {
        if (conference == null)
        {
            Log.Warning("Video conference {conferenceId} not found", conferenceId);
            return Results.NotFound();
        }

        Log.Information("Getting video conference {conferenceId}", conferenceId);
        
        // Estimated cost is calculated from the rate information
        return Results.Ok(conference);
    }

    private static bool ValidateRate(RateInformation? rate, DateTime startTime, DateTime endTime)
    {
        if (rate == null || rate.RatePerMinute <= 0)
            return false;

        if (rate.ExpirationDate.HasValue && rate.ExpirationDate.Value < endTime)
            return false;

        if (rate.MinimumCharge < 0 || rate.MinimumMinutes < 0 || rate.BillingIncrementMinutes <= 0)
            return false;

        return true;
    }
}
