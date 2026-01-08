namespace Application.Common.Models;


public static class PagingExtersions
{
    //public static IQueryable<T> ApplySorting<T>(this IQueryable<T> query, string orderBy, bool isDescending)
    //{
    //    if (!string.IsNullOrWhiteSpace(orderBy))
    //    {
    //        return query.OrderBy($"{orderBy} {(isDescending ? "desc" : "asc")}");
    //    }

    //    return query;
    //}

    //public static  IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int pageNumber, int pageSize)
    //{
    //    return query.Skip((pageNumber - 1) * pageSize).Take(pageSize);
    //}
}
