using FluentValidation;

namespace EventServer.Aggregates.Users.Commands;

public interface IUserCommand
{
}

public class CreateUserCommand : IUserCommand
{
    public readonly string EmailAddress;

    public readonly string FirstName;

    public readonly string LastName;

    public CreateUserCommand(string FirstName, string LastName, string EmailAddress)
    {
        this.FirstName = FirstName;
        this.LastName = LastName;
        this.EmailAddress = EmailAddress;
    }

    public override string ToString()
    {
        return "FirstName: " + FirstName + " LastName: " + LastName + " Email: " + EmailAddress;
    }
}

public class CreateUserCommandValdiator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValdiator()
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