namespace org.fortium.fx.common;

[Serializable]
[GenerateSerializer]
public record PartnerSnapshot(
    string emailAddress,
    string firstName,
    string lastName,
    List<string> skills,
    List<Guid?> videoConferences);