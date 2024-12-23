namespace UI.Aggregates.Users;

[Serializable]
[GenerateSerializer]
public record UserSnapshot(string EmailAddress, string FirstName, string LastName, List<Guid?> VideoConferences);
