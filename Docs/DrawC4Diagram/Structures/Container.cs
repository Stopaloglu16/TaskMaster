using C4Sharp.Elements.Containers;

namespace DrawC4Diagram.Structures;

public static class Containers
{
    public static ServerSideWebApp WebApp => new(
      Alias: "Website",
      Label: "Website",
      Description: "Blazor user interface",
      Technology: "C#, blazor"
    );

    public static ServerSideWebApp Api => new(
      Alias: "API",
      Label: "API",
      Description: "ASP.Net controller",
      Technology: "C#, WebApi"
    );

    public static ServerSideWebApp ApiMin => new(
      Alias: "API",
      Label: "API",
      Description: "Minimal api",
      Technology: "C#, WebApi"
    );

    public static Database SqlDatabase => new(
       Alias: "Database",
       Label: "SqlDatabase",
       Description: "Stores user registration information, hashed auth credentials, access logs, etc.",
       Technology: "SQL Database"
    );
}