using Orleans;

namespace org.fortium.fx.common;

[Serializable]
[GenerateSerializer
public class Partner
{
    // Parameterless constructor
    public Partner()
    {
    }

    // Optional: Constructor with parameters
    public Partner(
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

    [Id(0)] public string Id { get; set; } = "";
    [Id(1)] public string EmailAddress { get; set; } = "";
    [Id(2)] public string FirstName { get; set; } = "";
    [Id(3)] public string LastName { get; set; } = "";
    [Id(4)] public string PrimaryPhone { get; set; } = "";
    [Id(5)] public string PhotoUrl { get; init; } = "";
    [Id(6)] public string Bio { get; set; } = "";
    [Id(7)] public List<WorkHistory> WorkHistories { get; init; } = new();
    [Id(8)] public List<string> Skills { get; init; } = new();
    [Id(9)] public List<Guid?> VideoConferences { get; init; } = new();
    [Id(10)] public string Title { get; set; } = "Chief Technology Officer";
    [Id(11)] public string City { get; set; } = "Prosper";
    [Id(12)] public string State { get; set; } = "Tx";
    [Id(13)] public string Country { get; set; } = "United States";

    public string GetFullName()
    {
        return FirstName + " " + LastName;
    }

    public string GetLocation()
    {
        return City + ", " + State + ", " + Country;
    }
}