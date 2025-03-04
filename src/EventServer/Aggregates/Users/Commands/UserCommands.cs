using FluentValidation;

namespace EventServer.Aggregates.Users.Commands;

public interface IUserCommand
{
}

[Serializable]
public record CreateUserCommand(string FirstName, string LastName, string EmailAddress) : IUserCommand {}

[Serializable]
public record AddVideoConferenceToUserCommand(string EmailAddress, Guid? ConferenceId): IUserCommand {}

[Serializable]
public record UserLoggedInCommand(string EmailAddress, DateTime LoginDate): IUserCommand {}

[Serializable]
public record UserLoggedOutCommand(string EmailAddress, DateTime LogoutDate): IUserCommand {}

public class CreateUserCommandValdator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValdator()
    {
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
    }
}


public class AddVideoConferenceToUserCommandValidator
    : AbstractValidator<AddVideoConferenceToUserCommand>
{
    public AddVideoConferenceToUserCommandValidator()
    {
        RuleFor(command => command.ConferenceId)
            .NotNull()
            .NotNull()
            .WithMessage("Conference Id is required.");
    }
}
