using UI.Grains.Interfaces;
using Whaally.Domain.Abstractions;

namespace UI.Grains.Partners;

public record PartnerSnapshot(
    string emailAddress,
    string firstName,
    string lastName,
    List<String> skills,
    List<IVideoConferenceGrain> videoConferences): ISnapshot;

public class PartnerSnapshotFactory : ISnapshotFactory<PartnerAggregate, PartnerSnapshot>
{
  public PartnerSnapshot Instantiate(PartnerAggregate aggregate)
  {
    return new PartnerSnapshot(aggregate.emailAddress,aggregate.firstName,aggregate.lastName,aggregate.skills,aggregate.videoConferences);
  }
}
