namespace org.fortium.fx.common;

[Serializable]
[GenerateSerializer]
public record WorkHistory(
    DateOnly startDate,
    DateOnly? endDate,
    string companyName,
    string title,
    string description
);
