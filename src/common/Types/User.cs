using Whaally.Domain.Abstractions;

namespace org.fortium.fx.common.Types
{
    public record User: IAggregate {
      public Guid? Id { get; }
      public string? FirstName { get; set; }
      public string? LastName { get; set; }
      public Email[]?  Emails { get; } = Array.Empty<Email>();
      public Phone[]? Phones { get; } = Array.Empty<Phone>();
      public Address[]? Addresses { get; } = Array.Empty<Address>();
      public DateTime LastLogin { get; } = DateTime.Now;
      public DateTime Created { get; } = DateTime.Now;

      public User(Guid? id, string firstName, string lastName, Email[] emails, Phone[] phones, Address[] addresses)
      {
        this.Id = id;
        this.FirstName = firstName;
        this.LastName = lastName;
        this.Emails = emails;
        this.Phones = phones;
        this.Addresses = addresses;
      }

      public User()
      {
      }
    }
}
