namespace Finstar.TaskManagement.Api.Handlers;

public class Result
{
    public Result(bool isSuccess, Error error)
    {
        if (isSuccess && error != Error.None ||
            !isSuccess && error == Error.None)
        {
            throw new ArgumentException("Invalid error", nameof(error));
        }

        IsSuccess = isSuccess;
        Error = error;
    }

    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public Error Error { get; }

    public static Result Success() => new(true, Error.None);

    public static Result Failure(Error error) => new(false, error);

    public static Result<T> Success<T>(T data) => new(true, Error.None, data); //new (true, Error.None);

    public static Result<T> Failure<T>(Error error) => new(false, error, default);
}


public class Result<T> : Result
{
    public Result(bool isSuccess, Error error, T? data) : base(isSuccess, error)
    {
        Data = data;
    }

    public T? Data { get; init; }
}