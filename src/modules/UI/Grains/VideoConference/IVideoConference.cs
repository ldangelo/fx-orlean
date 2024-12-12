
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
public interface IVideoConferenceFactory
{
    IVideoConference GetConference(Guid id);
}

public class VideoConferenceGrain([PersistentState(stateName: "conferences", storageName: "conferences")] IPersistentState<ConferenceDetails> state) : Grain, IVideoConference
{
    public async Task<IVideoConference> CreateConference(IPartnerGrain partner, IUsersGrain user, DateTime startTime, DateTime endTime)
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

    public Task<IVideoConference> JoinConference()
    {
        throw new NotImplementedException();
    }

    public Task<IVideoConference> LeaveConference()
    {
        throw new NotImplementedException();
    }

    public Task<List<IVideoConference>> GetConferences()
    {
        throw new NotImplementedException();
    }
}
