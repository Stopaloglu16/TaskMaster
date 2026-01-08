using Domain.Enums;

namespace WebsiteApp.Utilities;

public class PageRedirect
{

    public static string RedirectTo(UserType userType)
    {
        if (userType == UserType.AdminUser)
        {
            return $"TaskManagerAdmin";
        }
        else if (userType == UserType.TaskUser)
        {
            return $"TaskManagerUser";
        }
        else
        {
            return $"home";
        }
    }


}
