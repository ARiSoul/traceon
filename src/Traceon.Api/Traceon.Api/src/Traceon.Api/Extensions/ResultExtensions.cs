using Traceon.Application.Common;

namespace Traceon.Api.Extensions;

internal static class ResultExtensions
{
    public static IResult ToHttpResult(this Result result)
    {
        return result.IsSuccess
            ? TypedResults.NoContent()
            : ToErrorResult(result.Error, result.ErrorType);
    }

    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        return result.IsSuccess
            ? TypedResults.Ok(result.Value)
            : ToErrorResult(result.Error, result.ErrorType);
    }

    public static IResult ToCreatedHttpResult<T>(this Result<T> result, Func<T, string> uriFactory)
    {
        return result.IsSuccess
            ? TypedResults.Created(uriFactory(result.Value), result.Value)
            : ToErrorResult(result.Error, result.ErrorType);
    }

    private static IResult ToErrorResult(string error, ResultErrorType errorType) => errorType switch
    {
        ResultErrorType.Conflict => TypedResults.Conflict(error),
        ResultErrorType.Validation => TypedResults.UnprocessableEntity(error),
        _ => TypedResults.NotFound(error)
    };
}
