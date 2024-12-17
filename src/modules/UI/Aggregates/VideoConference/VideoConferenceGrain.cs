
using UI.Grains.Interfaces;

namespace UI.Grains.VideoConference;

public class VideoConferenceGrain([PersistentState(stateName: "conferences", storageName: "conferences")] IPersistentState<VideoConferenceSnapshot> state) : Grain, IVideoConferenceGrain
{
    public async Task<IVideoConferenceGrain> CreateConference(IPartnerGrain partner, IUsersGrain user, DateTime startTime, DateTime endTime)
    {
        if (state.State == null)
            state.State = new VideoConferenceSnapshot();

        state.State.Partner = partner;
        state.State.User = user;
        state.State.StartTime = startTime;
        state.State.EndTime = endTime;

        await user.addVideoConference(this);
        await partner.addVideoConference(this);

        await state.WriteStateAsync();
        return this;
    }

    public Task<IVideoConferenceGrain> JoinConference()
    {
        throw new NotImplementedException();
    }

    public Task<IVideoConferenceGrain> LeaveConference()
    {
        throw new NotImplementedException();
    }

    public Task<List<IVideoConferenceGrain>> GetConferences()
    {
        throw new NotImplementedException();
    }
}
