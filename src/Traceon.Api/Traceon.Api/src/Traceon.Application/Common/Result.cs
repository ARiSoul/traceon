using System.Diagnostics.CodeAnalysis;

namespace Traceon.Application.Common;

public enum ResultErrorType
{
    None,
    NotFound,
    Conflict,
    Validation
}

public sealed class Result
{
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public string? Error { get; }

    public ResultErrorType ErrorType { get; }

    private Result(bool isSuccess, string? error, ResultErrorType errorType)
    {
        IsSuccess = isSuccess;
        Error = error;
        ErrorType = errorType;
    }

    public static Result Success() => new(true, null, ResultErrorType.None);
    public static Result Failure(string error, ResultErrorType errorType = ResultErrorType.NotFound) => new(false, error, errorType);
}

public sealed class Result<T>
{
    [MemberNotNullWhen(true, nameof(Value))]
    [MemberNotNullWhen(false, nameof(Error))]
    public bool IsSuccess { get; }

    public bool IsFailure => !IsSuccess;

    public T? Value { get; }
    public string? Error { get; }

    public ResultErrorType ErrorType { get; }

    private Result(bool isSuccess, T? value, string? error, ResultErrorType errorType)
    {
        IsSuccess = isSuccess;
        Value = value;
        Error = error;
        ErrorType = errorType;
    }

    public static Result<T> Success(T value) => new(true, value, null, ResultErrorType.None);
    public static Result<T> Failure(string error, ResultErrorType errorType = ResultErrorType.NotFound) => new(false, default, error, errorType);
}
