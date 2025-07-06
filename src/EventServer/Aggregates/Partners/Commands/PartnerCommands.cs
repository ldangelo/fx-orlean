using FluentValidation;
using Fortium.Types;
using Marten.Schema;

namespace EventServer.Aggregates.Partners.Commands;

public interface IPartnerCommand
{
}

[Serializable]
public record CreatePartnerCommand(String FirstName, String LastName, String EmailAddress) : IPartnerCommand;

[Serializable]
public record PartnerLoggedInCommand([property: Identity]  string EmailAddress,DateTime LoginTime) : IPartnerCommand;

[Serializable]
public record PartnerLoggedOutCommand(string EmailAddress, DateTime LogoutTime) : IPartnerCommand;

[Serializable]
public record GetPartnerCommand(string EmailAddress) : IPartnerCommand;

[Serializable]
public record AddPartnerSkillCommand(string EmailAddress,Fortium.Types.PartnerSkill[] Skills) : IPartnerCommand;

[Serializable]
public record AddVideoConferenceToPartnerCommand(string EmailAddress,Guid? ConferenceId) : IPartnerCommand;

[Serializable]
public record SetPartnerBioCommand(string EmailAddress,string Bio);

[Serializable]
public record SetPartnerPhotoUrlCommand(string EmailAddress,string PhotoUrl): IPartnerCommand;

[Serializable]
public record SetPartnerPrimaryPhoneCommand(string EmailAddress,string PrimaryPhone): IPartnerCommand;

[Serializable]
public record SetPartnerWorkExperienceCommand(string EmailAddress, WorkHistory[] WorkHistory) : IPartnerCommand;

public class CreatePartnerCommandValidator: AbstractValidator<CreatePartnerCommand>
{
    public CreatePartnerCommandValidator() {
        RuleFor(command => command.FirstName)
            .NotNull()
            .NotEmpty()
            .WithMessage("First Name is Required");
        RuleFor(command => command.LastName)
            .NotNull()
            .NotEmpty()
            .WithMessage("Last Name is Required");
        RuleFor(command => command.EmailAddress)
            .NotNull()
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Email address is required and must be valid.");

        RuleFor(command => command.EmailAddress)
            .Must(x => x.EndsWith("@fortiumpartners.com"))
            .WithMessage("Partners must be from fortimpartners.com");
    }
}

public class PartnerLoggedInCommandValidator: AbstractValidator<PartnerLoggedInCommand>
{
    public PartnerLoggedInCommandValidator()
    {
        RuleFor(x => x.EmailAddress)
            .NotNull()
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Email address is required and must be valid");

        RuleFor(command => command.EmailAddress)
            .Must(x => x.EndsWith("@fortiumpartners.com"))
            .WithMessage("Partners must be from fortimpartners.com");

        RuleFor(x => x.LoginTime)
            .NotNull()
            .NotEmpty()
            .Must(x => x >= DateTime.Now)
            .WithMessage("LoginTime must be valid.");
    }
}

public class PartnerLoggedOutCommandValidator: AbstractValidator<PartnerLoggedOutCommand>
{
    public PartnerLoggedOutCommandValidator()
    {
        RuleFor(x => x.EmailAddress)
            .NotNull()
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Email address is required and must be valid");

        RuleFor(command => command.EmailAddress)
            .Must(x => x.EndsWith("@fortiumpartners.com"))
            .WithMessage("Partners must be from fortimpartners.com");

        RuleFor(x => x.LogoutTime)
            .NotNull()
            .NotEmpty()
            .Must(x => x >= DateTime.Now)
            .WithMessage("LogoutTime must be valid.");
    }
}

public class GetPartnerCommandValidator: AbstractValidator<GetPartnerCommand>
{
    public GetPartnerCommandValidator()
    {
        RuleFor(x => x.EmailAddress)
            .NotNull()
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Email address is required and must be valid");

        RuleFor(command => command.EmailAddress)
            .Must(x => x.EndsWith("@fortiumpartners.com"))
            .WithMessage("Partners must be from fortimpartners.com");
    }
}

public class AddPartnerSkillCommandValidator: AbstractValidator<AddPartnerSkillCommand>
{
    public AddPartnerSkillCommandValidator()
    {
       RuleFor(x => x.EmailAddress)
            .NotNull()
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Email address is required and must be valid");

        RuleFor(command => command.EmailAddress)
            .Must(x => x.EndsWith("@fortiumpartners.com"))
            .WithMessage("Partners must be from fortimpartners.com");

        RuleFor(command => command.Skills)
            .Must(x => x.Length > 0)
            .WithMessage("Must provide skills.");
    }
}

public class AddVideoConferenceToPartnerCommandValidator: AbstractValidator<AddVideoConferenceToPartnerCommand>
{
    public AddVideoConferenceToPartnerCommandValidator()
    {
       RuleFor(x => x.EmailAddress)
            .NotNull()
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Email address is required and must be valid");

        RuleFor(command => command.EmailAddress)
            .Must(x => x.EndsWith("@fortiumpartners.com"))
            .WithMessage("Partners must be from fortimpartners.com");

        RuleFor(command => command.ConferenceId)
            .NotNull()
            .NotEmpty()
            .WithMessage("Video Conference Id must be valid");
    }


}

public class SetPartnerBioCommandValidator: AbstractValidator<SetPartnerBioCommand>
{
    public SetPartnerBioCommandValidator()
    {
       RuleFor(x => x.EmailAddress)
            .NotNull()
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Email address is required and must be valid");

        RuleFor(command => command.EmailAddress)
            .Must(x => x.EndsWith("@fortiumpartners.com"))
            .WithMessage("Partners must be from fortimpartners.com");

        RuleFor(command => command.Bio)
            .NotNull()
            .WithMessage("Bio cannot be NULL.");
    }
}

public class SetPartnerPhotoUrlCommandValidator: AbstractValidator<SetPartnerPhotoUrlCommand>
{
    public SetPartnerPhotoUrlCommandValidator()
    {
       RuleFor(x => x.EmailAddress)
            .NotNull()
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Email address is required and must be valid");

        RuleFor(command => command.EmailAddress)
            .Must(x => x.EndsWith("@fortiumpartners.com"))
            .WithMessage("Partners must be from fortimpartners.com");


        RuleFor(command => command.PhotoUrl)
            .NotNull()
            .NotEmpty()
            .Matches(@"^(http|https|ftp|)\://|[a-zA-Z0-9\-\.]+\.[a-zA-Z](:[a-zA-Z0-9]*)?/?([a-zA-Z0-9\-\._\?\,\'/\\\+&amp;%\$#\=~])*[^\.\,\)\(\s]$")
            .WithMessage("PhotoUrl must be valid.");
    }
}

public class SetPartnerPrimaryPhoneCommandValidator: AbstractValidator<SetPartnerPrimaryPhoneCommand>
{
    public SetPartnerPrimaryPhoneCommandValidator()
    {
       RuleFor(x => x.EmailAddress)
            .NotNull()
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Email address is required and must be valid");

        RuleFor(command => command.EmailAddress)
            .Must(x => x.EndsWith("@fortiumpartners.com"))
            .WithMessage("Partners must be from fortimpartners.com");

        RuleFor(command => command.PrimaryPhone)
            .NotNull()
            .NotEmpty()
            .Matches(@"\(?\d{3}\)?-? *\d{3}-? *-?\d{4}")
            .WithMessage("Primary Phone must be a valid phone number.");
    }
}

public class SetPartnerWorkExperienceCommandValidator: AbstractValidator<SetPartnerWorkExperienceCommand>
{
    public SetPartnerWorkExperienceCommandValidator()
    {
       RuleFor(x => x.EmailAddress)
            .NotNull()
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Email address is required and must be valid");

        RuleFor(command => command.EmailAddress)
            .Must(x => x.EndsWith("@fortiumpartners.com"))
            .WithMessage("Partners must be from fortimpartners.com");


        RuleFor(command => command.WorkHistory)
            .NotNull()
            .NotEmpty()
            .WithMessage("WorkHistory needs to be valid.");

        RuleForEach(command => command.WorkHistory)
            .SetValidator(new WorkHistoryValidator())
            .WithMessage("WorkHistory needs to be valid.");

    }
}

public class WorkHistoryValidator: AbstractValidator<WorkHistory>
{
    private bool BeValidStartEnd(DateOnly? end, DateOnly start) {
        if (!end.HasValue) return true;
        if (end > start) return true;
        return false;
    }
    public WorkHistoryValidator()
    {
        RuleFor(wh => wh.StartDate)
            .NotNull()
            .NotEmpty()
            .WithMessage("WorkHistory startDate can not be null or empty.");
        RuleFor(wh => wh.EndDate)
            .Must((model, end) => BeValidStartEnd(end, model.StartDate))
            .WithMessage("WorkHistory endDate can not be less than start date.");

        RuleFor(wh => wh.Title)
            .NotNull()
            .NotEmpty()
            .WithMessage("WorkHistory title can not be null or empty");

        RuleFor(wh => wh.CompanyName)
            .NotNull()
            .NotEmpty()
            .WithMessage("WorkHistory companyName can not be null or empty");
        RuleFor(wh => wh.Description)
            .NotNull()
            .NotEmpty()
            .WithMessage("WorkHistory description can not be null or empty");
    }
}
