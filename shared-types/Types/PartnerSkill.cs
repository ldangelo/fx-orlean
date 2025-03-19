namespace Fortium.Types;

public enum ExperienceLevel { Novice, Beginner, Proficient, Expert }

[Serializable]
public record PartnerSkill(string Skill, int YearsOfExperience, ExperienceLevel ExperienceLevel);
