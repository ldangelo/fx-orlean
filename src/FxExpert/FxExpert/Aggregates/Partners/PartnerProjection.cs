using EventServer.Aggregates.Partners.Events;
using FxExpert.Aggregates.Partners;
using Marten;
using Marten.Events.Aggregation;

namespace EventServer.Aggregates.Partners;

[Serializable]
public class PartnerProjection : SingleStreamProjection<PartnerAggregate>
{
    // Parameterless constructor

    public async Task<PartnerAggregate> Create(PartnerCreatedEvent e, IQuerySession session)
    {
        var aggregate = await session.LoadAsync<PartnerAggregate>(e.partnerId);

        return aggregate;
    }

    public Task<PartnerAggregate> Apply(PartnerCreatedEvent e, PartnerAggregate aggregate)
    {
        if (e.partnerId != aggregate.Id) throw new Exception("Recieved event for wrong partner");
        aggregate.FirstName = e.firstName;
        aggregate.LastName = e.lastName;
        aggregate.EmailAddress = e.emailAddress;

        return Task.FromResult(aggregate);
    }
}