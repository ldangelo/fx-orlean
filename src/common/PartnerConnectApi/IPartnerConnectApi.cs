namespace common.PartnerConnectApi;

public interface IPartnerConnectApi
{
    public string? Authorize();
    public User GetUser(string primaryEmail);
}