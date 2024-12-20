using Orleankka.Meta;
using Serilog;
using Serilog.Context;
using UI.Grains.Partners;

namespace UI.Aggregates.Partners.Events;

public class VideoConferenceAddedToPartnerEvent : Event
{
    private string ConferenceId { get; init; } = "";

    public VideoConferenceAddedToPartnerEvent(string conferenceId) {
        ConferenceId = conferenceId;
    }
};
