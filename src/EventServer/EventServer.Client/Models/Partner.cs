namespace EventServer.Client.Models;

public class Partner
{
    public Partner(string? firstName, string? lastName, string? email, string? phone)
    {
        FirstName = firstName;
        LastName = lastName;
        EmailAddress = email;
        PhoneNumber = phone;
    }

    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? EmailAddress { get; set; }
    public string? PhoneNumber { get; set; }
}