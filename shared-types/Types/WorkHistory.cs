namespace Fortium.Types;

[Serializable]
public record WorkHistory
{
    public WorkHistory(DateOnly startDate, DateOnly? endDate,string companyName, string title, string description)
    {
        StartDate = startDate;
        CompanyName = companyName;
        Title = title;
        Description = description;
    }

    public DateOnly StartDate { get; set; }
    public DateOnly? EndDate { get; set; }
    public string CompanyName { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
}