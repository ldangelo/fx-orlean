using Whaally.Domain.Abstractions;

namespace UI.Aggregates.VideoConference;

public record VideoConferenceAggregate: IAggregate
{
    public DateTime StartTime;
    public DateTime EndTime;
    public string UserId;
    public string PartnerId;

    public VideoConferenceAggregate(DateTime eventStartTime, DateTime eventEndTime, string eventUserId, string eventPartnerId)
    {
        StartTime = eventStartTime;
        EndTime = eventEndTime;
        UserId = eventUserId;
        PartnerId = eventPartnerId;
    }
}