namespace UI.Grains.Users;

[Serializable]
[GenerateSerializer]
public record UserSnapshot(string EmailAddress, string FirstName, string LastName, List<Guid?> VideoConferences);