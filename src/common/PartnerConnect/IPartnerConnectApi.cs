namespace common.PartnerConnect;

public interface IPartnerConnectApi
{
    public Task<string?> Authorize(CancellationToken token);
    public Task<User?> GetUser(string primaryEmail, CancellationToken token);
}