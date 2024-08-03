using System.Reflection;

namespace Users;

public static class UserAssembly {
    public static Assembly Get() { return typeof(UserAssembly).Assembly; }
}
