using org.fortium.fx.common.Types;
using Whaally.Domain.Abstractions.Event;

namespace org.fortium.fx.common.Events {


public class UserCreatedEvent: IEvent {
  public User User { get; }

  public UserCreatedEvent(User u) {
    this.User = u;
  }
}
}
