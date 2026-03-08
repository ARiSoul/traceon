using FluentValidation;

namespace Traceon.Api.Filters;

internal sealed class ValidationFilter<T> : IEndpointFilter where T : class
{
    public async ValueTask<object?> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        var argument = context.Arguments.OfType<T>().FirstOrDefault();

        if (argument is null)
            return TypedResults.BadRequest("Request body is required.");

        var validator = context.HttpContext.RequestServices.GetService<IValidator<T>>();

        if (validator is null)
            return await next(context);

        var result = await validator.ValidateAsync(argument);

        if (!result.IsValid)
        {
            var errors = result.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            return TypedResults.ValidationProblem(errors);
        }

        return await next(context);
    }
}
