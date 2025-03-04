namespace Fortium.Types;

[Serializable]
public record WorkHistory(
    DateOnly StartDate,
    DateOnly? EndDate,
    string CompanyName,
    string Title,
    string Description
);

