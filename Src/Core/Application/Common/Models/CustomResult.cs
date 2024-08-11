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

public class CustomResult
{
    // Indicates whether the operation was successful
    public bool IsSuccess { get; private set; }

    // Holds an error message, if the operation failed
    public string Error { get; private set; }

    // Private constructor to prevent direct instantiation
    private CustomResult(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    // Factory method for success result
    public static CustomResult Success()
    {
        return new CustomResult(true, null);
    }

    // Factory method for failure result
    public static CustomResult Failure(string error)
    {
        return new CustomResult(false, error);
    }
}

