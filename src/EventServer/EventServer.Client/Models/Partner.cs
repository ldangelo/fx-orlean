using Microsoft.AspNetCore.Components;

namespace EventServer.Client.Models;

public class Partner
{
    private readonly NavigationManager Navigation;
    public string? MeetingURL;

    public Partner(string? firstName, string? lastName, string? email, string? phone, NavigationManager navigation)
    {
        Navigation = navigation;
        FirstName = firstName;
        LastName = lastName;
        EmailAddress = email;
        PhoneNumber = phone;
        MeetingURL = Navigation?.BaseUri + "Meeting/" + email ?? "";
    }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? EmailAddress { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Bio { get; set; }
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