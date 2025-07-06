using System.Diagnostics;
using Marten.Schema;

namespace Fortium.Types;

[Serializable]
[DebuggerDisplay($"{{{nameof(GetDebuggerDisplay)}(),nq}}")]
public class Partner
{
  // Parameterless constructor
  public Partner() { }

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

  public string PhotoUrl { get; set; } = "";

  public DollarAmount Rate { get; set; } = 1000.00;
  public Double PartnerPercentage { get; set; } = 0.80;
  public string? Bio { get; set; } = "";

  public List<WorkHistory> WorkHistories { get; init; } = new();

  public List<PartnerSkill> Skills { get; init; } = new();

  public List<Guid?> VideoConferences { get; init; } = new();

  public string Title { get; set; } = "";

  public string City { get; set; } = "";

  public string State { get; set; } = "";

  public string Country { get; set; } = "";

  // used by the AI service to determine the reason this partner was suggested
  public string Reason { get; set; } = "";

  // used by the AI service to determine the rank
  public int rank { get; set; } = 0;

  public int AvailabilityNext30Days { get; set; } = 0;
   
  public string GetFullName()
  {
    return FirstName + " " + LastName;
  }

  public string GetLocation()
  {
    return City + ", " + State + ", " + Country;
  }

  public bool IsLoggedIn()
  {
    return LoggedIn;
  }

  private string GetDebuggerDisplay()
  {
    return ToString();
  }

  public override string ToString()
  {
    return $"IsActive: {Active}, EmailAddress: {EmailAddress}, FirstName: {FirstName}, LastName: {LastName}, PrimaryPhone: {PrimaryPhone}, PhotoUrl: {PhotoUrl}, Bio: {Bio}, WorkHistories: {WorkHistories}, Skills: {Skills.Count}, VideoConferences: {VideoConferences.Count}, Title: {Title}, City: {City}, State: {State}, Country: {Country}, CreateDate {CreateDate}";
  }
}
