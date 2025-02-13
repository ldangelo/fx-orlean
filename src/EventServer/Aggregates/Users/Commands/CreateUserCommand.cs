
using FluentValidation;
using Marten.Schema;

namespace EventServer.Aggregates.Users.Commands;

public interface IUserCommand
{
}

[Serializable]
public record CreateUserCommand(string FirstName, string LastName, string EmailAddress) : IUserCommand {}

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
