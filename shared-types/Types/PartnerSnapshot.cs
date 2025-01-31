namespace org.fortium.fx.common;

[Serializable]
public record PartnerSnapshot
{
    // Parameterless constructor
    public PartnerSnapshot()
    {
    }

    // Optional: Constructor with parameters
    public PartnerSnapshot(
        string emailAddress,
        string firstName,
        string lastName,
        string primaryPhone,
        string photoUrl,
        string bio,
        List<WorkHistory> workHistories,
        List<string> skills,
        List<Guid?> videoConferences)
    {
        Id = emailAddress;
        EmailAddress = emailAddress;
        FirstName = firstName;
        LastName = lastName;
        PrimaryPhone = primaryPhone;
        PhotoUrl = photoUrl;
        Bio = bio;
        WorkHistories = workHistories;
        Skills = skills;
        VideoConferences = videoConferences;
    }

    public string Id { get; set; } = "";
    public string EmailAddress { get; set; } = "";
    public string FirstName { get; set; } = "";
    public string LastName { get; set; } = "";
    public string PrimaryPhone { get; set; } = "";
    public string PhotoUrl { get; init; } = "";
    public string Bio { get; set; } = "";
    public List<WorkHistory> WorkHistories { get; init; } = new();
    public List<string> Skills { get; init; } = new();
    public List<Guid?> VideoConferences { get; init; } = new();
    public string Title { get; set; } = "Chief Technology Officer";
    public string City { get; set; } = "Prosper";
    public string State { get; set; } = "Tx";
    public string Country { get; set; } = "United States";

    public string GetFullName()
    {
        return FirstName + " " + LastName;
    }

    public string GetLocation()
    {
        return City + ", " + State + ", " + Country;
    }
}