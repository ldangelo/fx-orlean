
namespace UI.Grains.Partners;

using UI.Grains.Interfaces;

public interface IPartnerGrainFactory
{
    IPartnerGrain GetPartner(String emailAddress);
    Task<List<IPartnerGrain>> GetPartners();
}

[GenerateSerializer, Alias(nameof(PartnerDetails))]
public sealed record class PartnerDetails
{
    [Id(0)] public string emailAddress { get; set; } = "";
    public string firstName { get; set; } = "";
    public string lastName { get; set; } = "";
    public List<String> skills { get; set; } = new List<String>();
    public List<IVideoConference> videoConferences { get; set; } = new List<IVideoConference>();
}

public class PartnerGrain([PersistentState(stateName: "partners", storageName: "partners")] IPersistentState<PartnerDetails> state)
   : Grain, IPartnerGrain
{
    public async Task SetPartnerDetails(string emailAddress, string firstName, string lastName)
    {
        state.State = new()
        {
            emailAddress = emailAddress,
            firstName = firstName,
            lastName = lastName,
            skills = new List<String>()
        };
        await state.WriteStateAsync();
    }

    public Task<IPartnerGrain> GetPartner(String emailAddress)
    {
        return Task.FromResult<IPartnerGrain>(this);
    }

    public Task<string> GetFirstName()
    {
        return Task.FromResult(state.State.firstName);
    }

    public Task<string> GetLastName()
    {
        return Task.FromResult(state.State.lastName);
    }

    public Task<List<String>> GetSkills()
    {
        return Task.FromResult(state.State.skills);
    }

    public Task<string> GetEmailAddress()
    {
        return Task.FromResult(state.State.emailAddress);
    }

    public async Task<List<IVideoConference>> getVideoConferences()
    {
        return await Task.FromResult(state.State.videoConferences);
    }

    public async Task addVideoConference(IVideoConference videoConference)
    {
        state.State.videoConferences.Add(videoConference);
        await state.WriteStateAsync();
    }

    public Task<IPartnerGrain> RemoveSkill(string skill)
    {
        throw new NotImplementedException();
    }

    public async Task<IPartnerGrain> AddSkill(string skill)
    {
        state.State.skills.Add(skill);
        await state.WriteStateAsync();
        return this;
    }

    public Task<IPartnerGrain> DeletePartner()
    {
        throw new NotImplementedException();
    }
}
