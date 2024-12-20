namespace UI.Grains.Users;

[Serializable, GenerateSerializer]
public record UserSnapshot(string emailAddress, string firstName, string lastName, List<string> videoConferences);