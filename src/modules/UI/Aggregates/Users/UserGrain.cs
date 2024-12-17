
namespace UI.Grains.Users;

using UI.Grains.Interfaces;
using Whaally.Domain.Abstractions;

[GenerateSerializer, Alias(nameof(UserDetails))]
public record class UserDetails: IAggregate
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<IVideoConferenceGrain> VideoConferences { get; set; } = new List<IVideoConferenceGrain>();
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
            VideoConferences = state.State?.VideoConferences ?? new List<IVideoConferenceGrain>()
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

    public Task addVideoConference(IVideoConferenceGrain videoConference)
    {
        state.State.VideoConferences.Add(videoConference);
        return state.WriteStateAsync();
    }

    public Task<List<IVideoConferenceGrain>> getVideoConferences()
    {
        return Task.FromResult(state.State.VideoConferences);
    }
}
