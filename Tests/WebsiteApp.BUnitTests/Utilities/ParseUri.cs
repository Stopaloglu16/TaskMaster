using System.Web;

namespace WebsiteApp.BUnitTests.Utilities;

public class ParseUri
{
    public static Tuple<int, int, string, bool> ParsePagingUrl(string requestUri)
    {

        // Extract the query string part
        var query = requestUri.Contains('?') ? requestUri.Split('?')[1] : requestUri;

        // Parse the query string
        var queryParams = HttpUtility.ParseQueryString(query);

        // Get individual values
        int pageNumber = int.Parse(queryParams["PageNumber"]);
        int pageSize = int.Parse(queryParams["PageSize"]);
        string orderBy = queryParams["OrderBy"];
        bool isDescending = bool.Parse(queryParams["IsDescending"]);

        Console.WriteLine($"PageNumber: {pageNumber}, PageSize: {pageSize}, OrderBy: {orderBy}, IsDescending: {isDescending}");

        return Tuple.Create(pageNumber, pageSize, orderBy, isDescending);
    }
}
