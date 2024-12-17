using UI.Grains.Interfaces;
using Whaally.Domain.Abstractions;

namespace UI.Grains.Partners;

public record Partner: IAggregate
{
    public string emailAddress { get; set; } = "";
    public string firstName { get; set; } = "";
    public string lastName { get; set; } = "";
    public List<String> skills { get; init; } = new List<String>();
    public List<IVideoConferenceGrain> videoConferences { get; set; } = new List<IVideoConferenceGrain>();
    public List<IEvent> events { get; set; } = new List<IEvent>();
}
