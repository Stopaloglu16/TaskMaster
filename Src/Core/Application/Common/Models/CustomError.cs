namespace Application.Common.Models;

public sealed record CustomError(bool isSuccess, string error)
{
    public static readonly CustomError None = new(true, string.Empty);

    public static CustomError Success()
    {
        return new CustomError(true, null);
    }

    public static CustomError Failure(string error)
    {
        return new CustomError(false, error);
    }
}
