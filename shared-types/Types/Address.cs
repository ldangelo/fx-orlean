
namespace org.fortium.fx.common;

public enum AddressType { Home, Work, Other }

public record Address {
  public string? StreetAddress1 { get; }
  public string? StreetAddress2 { get; }
  public string? City { get; }
  public string? State { get; }
  public string? ZipCode { get; }
  public AddressType? AddressType { get; }

  public Address(string address1, string? address2, string city, string state, string zip, AddressType type) {
    StreetAddress1 = address1;
    StreetAddress2 = address2;
    City = city;
    State = state;
    ZipCode = zip;
    AddressType = type;
  }
}
