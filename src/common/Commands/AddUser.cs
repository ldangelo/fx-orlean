using Orleankka.Meta;

namespace org.fortium.fx.commands;

[Immutable]
[GenerateSerializer]
public record AddUser(Guid id, string userName) : Command;