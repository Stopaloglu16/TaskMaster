using C4Sharp.Elements;

namespace DrawC4Diagram.Structures;

public static class User
{
    public static Person AdminUser => new Person("AdminUser", "Admin",
      "Full access");

    public static Person TaskUser => new Person("TaskUser", "Task",
      "Manage tasks");

    public static Person ReadOnlyUser => new Person("ReadOnlyUser", "Read Only",
      "View tasks");
}
