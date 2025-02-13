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
public record AddPartnerSkillCommand(string EmailAddress,string[] Skills) : IPartnerCommand;

[Serializable]
public record SetPartnerDetalsCommand(string EmailAddress, string FirstName, string LastName) : IPartnerCommand;

[Serializable]
public record AddVideoConferenceToPartnerCommand(string EmailAddress,Guid? ConferenceId) : IPartnerCommand;

[Serializable]
public record SetPartnerBioCommand(string EmailAddress,string Bio);

[Serializable]
public record SetPartnerPhotoUrlCommand(string EmailAddress,string PhotoUrl): IPartnerCommand;

[Serializable]
public record SetPartnerPrimaryPhoneCommand(string EmailAddress,string PrimaryPhone): IPartnerCommand;

[Serializable]
public record SetPartnerWorkExperienceCommand(string EmailAddress, WorkHistory WorkHistory) : IPartnerCommand;

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
