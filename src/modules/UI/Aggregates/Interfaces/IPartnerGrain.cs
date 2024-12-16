namespace UI.Grains.Interfaces;

public interface IPartnerGrain : IGrainWithStringKey
{
    Task<IPartnerGrain> GetPartner(String emailAddress);
    Task<IPartnerGrain> AddSkill(string skill);
    Task SetPartnerDetails(string emailAddress, string firstName, string lastName);
    Task<IPartnerGrain> RemoveSkill(string skill);
    Task<IPartnerGrain> DeletePartner();
    Task<string> GetFirstName();
    Task<string> GetLastName();
    Task<List<string>> GetSkills();
    Task<string> GetEmailAddress();
    Task<List<IVideoConferenceGrain>> getVideoConferences();
    Task addVideoConference(IVideoConferenceGrain videoConference);
}


