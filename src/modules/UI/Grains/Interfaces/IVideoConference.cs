namespace UI.Grains.Interfaces;

public interface IVideoConference : IGrainWithGuidKey
{
    Task<IVideoConference> CreateConference(IPartnerGrain partner, IUsersGrain user, DateTime startTime, DateTime endTime);
    Task<IVideoConference> JoinConference();
    Task<IVideoConference> LeaveConference();
}


