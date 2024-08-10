namespace Application.Common.Models;

/// <summary>
/// The Result pattern introduces its own result type, which represents the success or failure of an operation. 
/// </summary>
/// <typeparam name="T"></typeparam>
public class CustomResult<T>
{
    private readonly T? _value;

    private CustomResult(T value)
    {
        Value = value;
        IsSuccess = true;
        CustomError = CustomError.None;
    }

    private CustomResult(CustomError customError)
    {
        if (customError == CustomError.None)
        {
            throw new ArgumentException("invalid error", nameof(customError));
        }
        IsSuccess = false;
        CustomError = customError;
    }

    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public T Value
    {
        get
        {
            if (IsFailure)
            {
                throw new InvalidOperationException("there is no value for failure");
            }
            return _value!;
        }
        private init => _value = value;
    }
    public CustomError CustomError { get; }
    public static CustomResult<T> Success(T value)
    {
        return new CustomResult<T>(value);
    }
    public static CustomResult<T> Failure(CustomError error)
    {
        return new CustomResult<T>(error);
    }
}
