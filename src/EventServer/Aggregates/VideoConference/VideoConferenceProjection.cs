using Marten.Events.Aggregation;
using EventServer.Aggregates.VideoConference.Events;
using Fortium.Types;

namespace EventServer.Aggregates.VideoConference;

public class VideoConferenceState
{
    public string Id { get; set; } = string.Empty;
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string PartnerId { get; set; } = string.Empty;
    public RateInformation? RateInformation { get; set; }
    
    public decimal EstimatedCost 
    {
        get 
        {
            if (RateInformation == null)
            {
                return 0;
            }
            
            return RateInformation.CalculateCost(StartTime, EndTime);
        }
    }
}

public class VideoConferenceProjection : SingleStreamProjection<VideoConferenceState, string>
{
    public VideoConferenceProjection()
    {
        ProjectEvent<VideoConferenceCreatedEvent>((state, @event) =>
        {
            if (state == null)
            {
                state = new VideoConferenceState
                {
                    Id = @event.ConferenceId.ToString(),
                    StartTime = @event.StartTime,
                    EndTime = @event.EndTime,
                    UserId = @event.UserId,
                    PartnerId = @event.PartnerId,
                    RateInformation = @event.RateInformation
                };
            }
            
            return state;
        });
    }
}
