using Orleankka;
using Orleankka.Meta;

using Orleans;
using Orleans.Concurrency;
using Orleans.Serialization.Invocation;
using UI.Aggregates;
using UI.Aggregates.Partners.Commands;
using UI.Aggregates.Partners.Events;
using Serilog;
using Serilog.Context;

namespace UI.Grains.Partners;

public interface IPartnerAggregate : IActorGrain, IGrainWithStringKey {}

[Serializable, GenerateSerializer]
public class GetPartnerDetails: Query<PartnerSnapshot>
    {

    }

[MayInterleave(nameof(Interleave))]
public class PartnerAggregate: DispatchActorGrain, IPartnerAggregate
{
    public static bool Interleave(IInvokable req) => req.Message() is GetPartnerDetails;

    private bool active = false;
    private string EmailAddress { get; set; } = "";
    private string FirstName { get; set; } = "";
    private string LastName { get; set; } = "";
    private List<String> skills { get; init; } = new List<String>();
    private List<String> videoConferences { get; set; } = new List<String>();
    private List<Event> events { get; set; } = new List<Event>();


    void on(PartnerCreatedEvent e) {
        active = true;
        Log.Information("Created partner: " + e.EmailAddress);
    }

    IEnumerable<Event> Handle(CreatePartnerCommand cmd) {
        // TODO: Change to fluid validator
        if (string.IsNullOrEmpty(cmd.FirstName))
            throw new ArgumentException("Partner first name is required.");
        if (string.IsNullOrEmpty(cmd.LastName))
            throw new ArgumentException("Partner last name is required.");
        if (string.IsNullOrEmpty(cmd.EmailAddress))
            throw new ArgumentException("Partner email address is required.");

        Log.Information("Creating partner: " + cmd.EmailAddress);
       yield return new PartnerCreatedEvent(cmd.FirstName, cmd.LastName, cmd.EmailAddress);
    }

    IEnumerable<Event> Handle(AddVideoConferenceToPartnerCommand cmd) {
        if(string.IsNullOrEmpty(cmd.conferenceId))
            throw new ArgumentException("Conference Id must be set.");
        videoConferences.Add(cmd.conferenceId);

        yield return new VideoConferenceAddedToPartnerEvent(cmd.conferenceId);
    }

    private void CheckIsActive()
        {
            if (!active)
                throw new InvalidOperationException(Id + " item is not acctive.");
        }
}
