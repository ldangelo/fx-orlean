using Whaally.Domain.Abstractions;

namespace UI.Grains.Users;

public record UserSnapshot(string emailAddress, string firstName, string lastName, List<String> videoConferences) : ISnapshot;

public class PartnerSnapshotFactory : ISnapshotFactory<UserAggregate, UserSnapshot>
{
  public UserSnapshot Instantiate(UserAggregate aggregate)
  {
    return new UserSnapshot(aggregate.Email,aggregate.FirstName,aggregate.LastName, aggregate.VideoConferences);
  }
}