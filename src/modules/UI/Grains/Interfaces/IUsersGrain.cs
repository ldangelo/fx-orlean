namespace UI.Grains.Interfaces;

public interface IUsersGrain : IGrainWithStringKey
{
    public Task SetUserDetails(string firstName, string lastName, string email);
    public Task<string> getFirstName();
    public Task<string> getLastName();
    public Task<string> getEmail();
    public Task addVideoConference(IVideoConferenceGrain videoConference);
    public Task<List<IVideoConferenceGrain>> getVideoConferences();
}


