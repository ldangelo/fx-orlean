using Whaally.Domain.Abstractions;

namespace UI.Grains.Partners;

public record PartnerAggregate: IAggregate
{
    public string emailAddress { get; set; } = "";
    public string firstName { get; set; } = "";
    public string lastName { get; set; } = "";
    public List<String> skills { get; init; } = new List<String>();
    public List<String> videoConferences { get; set; } = new List<String>();
    public List<IEvent> events { get; set; } = new List<IEvent>();
}
