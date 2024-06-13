
namespace org.fortium.fx.common;

public enum EmailType { Home, Work, Other }

public record Email {
  public string? Address { get; }
  public EmailType? EmailType { get; }

  public Email(string address, EmailType type) {
    Address = address;
    EmailType = type;
  }
}
