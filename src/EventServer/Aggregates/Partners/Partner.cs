using System.Diagnostics;
using EventServer.Aggregates.Partners.Events;
using Marten.Schema;
using org.fortium.fx.common;
using Serilog;

namespace EventServer.Aggregates.Partners;

[Serializable]
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
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
        string? bio,
        List<WorkHistory> workHistories,
        List<string> skills,
        List<Guid?> videoConferences
    )
    {
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

    public bool Active { get; set; }
    public bool LoggedIn { get; set; } = false;
    public DateTime LastLogin { get; set; }
    public DateTime LastLogout { get; set; }
    public DateTime CreateDate { get; set; }
    public DateTime UpdateDate { get; set; }

//    public String Id { get; set; } = "";

    [Identity]
    public string EmailAddress { get; set; } = "";

    public string FirstName { get; set; } = "";

    public string LastName { get; set; } = "";

    public string PrimaryPhone { get; set; } = "";

    public string PhotoUrl { get; init; } = "";

    public string? Bio { get; set; } = "";

    public List<WorkHistory> WorkHistories { get; init; } = new();

    public List<string> Skills { get; init; } = new();

    public List<Guid?> VideoConferences { get; init; } = new();

    public string Title { get; set; } = "";

    public string City { get; set; } = "";

    public string State { get; set; } = "";

    public string Country { get; set; } = "";

    public string GetFullName()
    {
        return FirstName + " " + LastName;
    }

    public string GetLocation()
    {
        return City + ", " + State + ", " + Country;
    }

    public void Apply(PartnerLoggedInEvent evnt)
    {
        Log.Information("Partner: Applying login event to {EmailAddress} at {login}", evnt.partnerId, evnt.loginTime);
        LastLogin = evnt.loginTime;
        LoggedIn = true;
        UpdateDate = DateTime.Now;
    }

    public void Apply(PartnerLoggedOutEvent evnt)
    {
        Log.Information("Partner: Applying logout event to {EmailAddress}", evnt.partnerId);
        LastLogin = evnt.logoutTime;
        LoggedIn = false;
        UpdateDate = DateTime.Now;
    }

    public void Apply(PartnerCreatedEvent evnt)
        {
            Log.Information("Partner: Applying create event to {EmailAddress}", evnt.ToString());
            FirstName = evnt.firstName;
            LastName = evnt.lastName;
            EmailAddress = evnt.emailAddress;
            LoggedIn = false;
            CreateDate = DateTime.Now;
            Log.Information("Partner: Applied create event to {EmailAddress}", this.ToString());
        }

    internal bool IsLoggedIn()
    {
        return LoggedIn == true ;
    }

    private string GetDebuggerDisplay()
    {
        return ToString();
    }

    public override string ToString()
    {
        return $"EmailAddress: {EmailAddress}, FirstName: {FirstName}, LastName: {LastName}, PrimaryPhone: {PrimaryPhone}, PhotoUrl: {PhotoUrl}, Bio: {Bio}, WorkHistories: {WorkHistories}, Skills: {Skills}, VideoConferences: {VideoConferences}, Title: {Title}, City: {City}, State: {State}, Country: {Country}, CreateDate {CreateDate}";
    }
}
