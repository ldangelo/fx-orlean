using UI.Grains.Interfaces;

namespace UI.Grains.VideoConference;

public sealed record class VideoConferenceSnapshot
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration => EndTime - StartTime;
    public IPartnerGrain? Partner { get; set; }
    public IUsersGrain? User { get; set; }
}