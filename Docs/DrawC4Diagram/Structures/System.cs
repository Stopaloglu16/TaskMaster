using C4Sharp.Elements;

namespace DrawC4Diagram.Structures;

internal class System
{
    public static SoftwareSystem TaskMasterWeb =>
        new("WebApp", "TaskMaster Web System",
   "Allows user to manage tasks");

    public static SoftwareSystem TaskMasterApi =>
        new("WebApi", "TaskMaster WebApi System",
   "Allows task to CRUD");

    public static SoftwareSystem TaskMasterApiAuth =>
        new("WebApiAuth", "TaskMaster WebApi Auth System",
   "Keeps authentication");
}
