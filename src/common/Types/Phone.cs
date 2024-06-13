
namespace org.fortium.fx.common;

public enum PhoneType { Home, Work, Other }

public record Phone {
  public string PhoneNumber {get;}
  public PhoneType PhoneType {get;}

  public Phone(string number,PhoneType type) {
    this.PhoneNumber = number;
    this.PhoneType = type;
  }

  public Phone(Phone p) {
    this.PhoneNumber = p.PhoneNumber;
    this.PhoneType = p.PhoneType;
  }
}
