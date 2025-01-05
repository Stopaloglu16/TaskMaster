using C4Sharp.Elements;

namespace DrawC4Diagram.Structures;

public static class Components
{
    public static Component Sign =>
   new("sign", "Sign In Controller", "MVC Controller", "Allows users to sign in to the internet banking system");

    public static Component Security =>
      new("security", "Security Component", "Spring Bean", "Provides functionality related to singing in, changing passwords, etc.");

}