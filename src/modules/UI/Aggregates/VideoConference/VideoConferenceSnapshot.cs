namespace UI.Aggregates.VideoConference;

public sealed record class VideoConferenceSnapshot
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration => EndTime - StartTime;
    public string? PartnerId { get; set; }
    public string? UserId { get; set; }
}
