
using UI.Grains.Interfaces;

namespace UI.Grains.VideoConference;

public sealed record class ConferenceDetails
{
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
    public TimeSpan Duration => EndTime - StartTime;
    public IPartnerGrain? Partner { get; set; }
    public IUsersGrain? User { get; set; }
}

public class VideoConferenceGrain([PersistentState(stateName: "conferences", storageName: "conferences")] IPersistentState<ConferenceDetails> state) : Grain, IVideoConferenceGrain
{
    public async Task<IVideoConferenceGrain> CreateConference(IPartnerGrain partner, IUsersGrain user, DateTime startTime, DateTime endTime)
    {
        if (state.State == null)
            state.State = new ConferenceDetails();

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
