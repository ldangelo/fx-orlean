using Marten;

namespace EventServer.Aggregates.Users;

public interface IUserAggregate
{
}

public class UserAggregate
{
    public UserAggregate(IDocumentStore eventStore)
    {
    }

    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string EmailAddress { get; set; } = string.Empty;
    public List<Guid?> VideoConferences { get; } = new();
}