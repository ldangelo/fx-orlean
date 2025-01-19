namespace org.fortium.fx.common;

[Serializable]
[GenerateSerializer]
public record PartnerSnapshot(
    string emailAddress,
    string firstName,
    string lastName,
    string primaryPhone,
    string photoUrl,
    List<string> skills,
    List<Guid?> videoConferences);
