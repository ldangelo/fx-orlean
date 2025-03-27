namespace Fortium.Types;

public enum ExperienceLevel
{
    Novice,
    Beginner,
    Proficient,
    Expert
}

[Serializable]
public record PartnerSkill
{
    public PartnerSkill(string skill, int yearsOfExperience, ExperienceLevel experienceLevel)
    {
        Skill = skill;
        YearsOfExperience = yearsOfExperience;
        ExperienceLevel = experienceLevel;
    }

    public string Skill { get; set; }
    public int YearsOfExperience { get; set; }
    public ExperienceLevel ExperienceLevel { get; set; }
}