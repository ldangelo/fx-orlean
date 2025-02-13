using FluentValidation;

namespace EventServer.Aggregates.Users.Events;

public interface IUserEvent;

[Serializable]
public record UserCreatedEvent(string FirstName, string LastName, string EmailAddress) : IUserEvent;

[Serializable]
public record VideoConferenceAddedToUserEvent(string userId, Guid? conferenceId) : IUserEvent;

public class UserCreatedEventValidator: AbstractValidator<UserCreatedEvent>
{

    public UserCreatedEventValidator()
    {
        RuleFor(x => x.FirstName)
            .NotNull()
            .NotEmpty()
            .WithMessage("First Name is Required");
        RuleFor(x => x.LastName)
            .NotNull()
            .NotEmpty()
            .WithMessage("Last Name is Required");
        RuleFor(x => x.EmailAddress)
            .NotNull()
            .NotEmpty()
            .EmailAddress()
            .WithMessage("Email address is required and must be valid.");
    }
}
