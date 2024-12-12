
namespace UI.Grains.Users;

using UI.Grains.Interfaces;

[GenerateSerializer, Alias(nameof(UserDetails))]
public sealed record class UserDetails
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<IVideoConference> VideoConferences { get; set; } = new List<IVideoConference>();
}

public class UserGrain([PersistentState(stateName: "users", storageName: "users")] IPersistentState<UserDetails> state) : Grain, IUsersGrain
{
    public async Task SetUserDetails(string firstName, string lastName, string email)
    {
        state.State = new UserDetails()
        {
            FirstName = firstName,
            LastName = lastName,
            Email = email,
            VideoConferences = state.State?.VideoConferences ?? new List<IVideoConference>()
        };

        await state.WriteStateAsync();
    }

    public Task<string> getFirstName()
    {
        return Task.FromResult(state.State.FirstName);
    }

    public Task<string> getLastName()
    {
        return Task.FromResult(state.State.LastName);
    }

    public Task<string> getEmail()
    {
        return Task.FromResult(state.State.Email);
    }

    public Task addVideoConference(IVideoConference videoConference)
    {
        state.State.VideoConferences.Add(videoConference);
        return state.WriteStateAsync();
    }

    public Task<List<IVideoConference>> getVideoConferences()
    {
        return Task.FromResult(state.State.VideoConferences);
    }
}
