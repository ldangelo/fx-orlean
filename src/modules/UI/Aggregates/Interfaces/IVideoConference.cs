namespace UI.Grains.Interfaces;

public interface IVideoConferenceGrain : IGrainWithGuidKey
{
    Task<IVideoConferenceGrain> CreateConference(IPartnerGrain partner, IUsersGrain user, DateTime startTime, DateTime endTime);
    Task<IVideoConferenceGrain> JoinConference();
    Task<IVideoConferenceGrain> LeaveConference();
}


