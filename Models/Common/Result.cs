namespace BlogMvc.Models.Common;
public class Result<T>
{
    public bool IsSuccess { get; private set;}
    public T? Value { get; private set; }
    public Error? Error { get; private set; }

    private Result(bool isSuccess, T? value = default, Error? error = null)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
    }
    public static Result<T> Success(T value) => new(true, value);
    public static Result<T> Failure(Error error) => new(false, default, error);

    public static implicit operator Result<T>(T value) => Success(value);
    public static implicit operator Result<T>(Error error) => Failure(error);
}
public class Result
{
    public bool IsSuccess { get; private set; }
    public Error? Error { get; private set; }

    private Result(bool isSuccess, Error? error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, null);
    public static Result Failure(Error error) => new(false, error);

    public static implicit operator Result(Error error) => Failure(error);
}
