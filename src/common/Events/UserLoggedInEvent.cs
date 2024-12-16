using org.fortium.fx.common.Types;
using Whaally.Domain.Abstractions;

namespace org.fortium.fx.common.Events {

  public class UserLoggedInEvent: IEvent {
    public User User { get; }

    public UserLoggedInEvent(User u) {
      this.User = u;
    }
  }
}
