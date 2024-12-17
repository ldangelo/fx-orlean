using UI.Grains.Interfaces;
using Whaally.Domain.Abstractions;

namespace UI.Grains.Users;

[GenerateSerializer, Alias(nameof(UserAggregate))]
public record class UserAggregate: IAggregate
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public List<IVideoConferenceGrain> VideoConferences { get; set; } = new List<IVideoConferenceGrain>();
}