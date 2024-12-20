namespace UI.Grains.Partners;

public record PartnerSnapshot(
    string emailAddress,
    string firstName,
    string lastName,
    List<string> skills,
    List<string> videoConferences);