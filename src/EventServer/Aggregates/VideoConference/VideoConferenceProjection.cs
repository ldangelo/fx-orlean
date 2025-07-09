using Marten.Events.Aggregation;
using EventServer.Aggregates.VideoConference.Events;
using Fortium.Types;

namespace EventServer.Aggregates.VideoConference;

public class VideoConferenceState
{
    public required string Id { get; set; }
    public required DateTime StartTime { get; set; }
    public required DateTime EndTime { get; set; }
    public required string UserId { get; set; }
    public required string PartnerId { get; set; }
    public required RateInformation RateInformation { get; set; }
    public decimal EstimatedCost => RateInformation?.CalculateCost(StartTime, EndTime) ?? 0;
}

public class VideoConferenceProjection : SingleStreamProjection<VideoConferenceState, string>
{
    public VideoConferenceProjection()
    {
        ProjectEvent<VideoConferenceCreatedEvent>((state, @event) =>
        {
            state = state ?? new VideoConferenceState
            {
                Id = @event.ConferenceId.ToString(),
                StartTime = @event.StartTime,
                EndTime = @event.EndTime,
                UserId = @event.UserId,
                PartnerId = @event.PartnerId,
                RateInformation = @event.RateInformation
            };
            
            return state;
        });
    }
}
