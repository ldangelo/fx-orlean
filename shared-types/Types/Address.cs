
using System.Diagnostics;

namespace Fortium.Types;

public enum AddressType { Home, Work, Other }

[Serializable]
[DebuggerDisplay("{DebuggerDisplay,nq}")]
public class Address {
  public string? Street1 { get; set; }
  public string? Street2 { get; set; }
  public string? City { get; set; }
  public string? State { get; set; }
  public string? ZipCode { get; set; }
  public string? Country { get; set; }
  public AddressType AddressType { get; set; } = AddressType.Home;

  public Address() { }

  public Address(string street1, string? street2, string city, string state, string zipCode, string? country = null, AddressType addressType = AddressType.Home) {
    Street1 = street1;
    Street2 = street2;
    City = city;
    State = state;
    ZipCode = zipCode;
    Country = country;
    AddressType = addressType;
  }
  
  private string DebuggerDisplay => $"{Street1}, {City}, {State} {ZipCode}";
}
