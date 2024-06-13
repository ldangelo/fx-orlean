using org.fortium.fx.common;
using Whaally.Domain.Abstractions.Aggregate;

namespace org.fortium.fx.common.Types
{
    public record User: IAggregate {
      public Guid? Id { get; }
      public string? FirstName { get; }
      public string? LastName { get; }
      public Email[]?  Emails { get; } = Array.Empty<Email>();
      public Phone[]? Phones { get; } = Array.Empty<Phone>();
      public Address[]? Addresses { get; } = Array.Empty<Address>();

      public User(Guid? id, string firstName, string lastName, Email[] emails, Phone[] phones, Address[] addresses)
      {
        this.Id = id;
        this.FirstName = firstName;
        this.LastName = lastName;
        this.Emails = emails;
        this.Phones = phones;
        this.Addresses = addresses;
      }
    }
}
