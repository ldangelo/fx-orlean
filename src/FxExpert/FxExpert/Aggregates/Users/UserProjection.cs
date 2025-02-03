using EventServer.Aggregates.Users.Events;
using Marten;
using Marten.Events.Aggregation;

namespace EventServer.Aggregates.Users;

[Serializable]
public class UserProjection : SingleStreamProjection<UserAggregate>
{
    public async Task<UserAggregate> Create(UserCreatedEvent e, IQuerySession session)
    {
        var user = await session.LoadAsync<UserAggregate>(e.userId);
        if (user == null)
        {
            user = new UserAggregate(session.DocumentStore);
            user.FirstName = e.FirstName;
            user.LastName = e.LastName;
            user.EmailAddress = e.EmailAddress;
        }

        return user;
    }
}