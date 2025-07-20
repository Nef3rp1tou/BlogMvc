using BlogMvc.Models.Common;
using FluentValidation;

namespace BlogMvc.Extensions;
public static class ValidationExtensions
{
    public static Result<T> ValidateAndReturn<T>(this IValidator<T> validator, T instance)
    {
        var validationResult = validator.Validate(instance);

        if (!validationResult.IsValid)
        {
            var errorMessages = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .ToList();

            var combinedMessage = string.Join("; ", errorMessages);
            return Result<T>.Failure(Error.Validation(combinedMessage));
        }

        return Result<T>.Success(instance);
    }

    public static async Task<Result<T>> ValidateAndReturnAsync<T>(this IValidator<T> validator, T instance)
    {
        var validationResult = await validator.ValidateAsync(instance);

        if (!validationResult.IsValid)
        {
            var errorMessages = validationResult.Errors
                .Select(e => e.ErrorMessage)
                .ToList();

            var combinedMessage = string.Join("; ", errorMessages);
            return Result<T>.Failure(Error.Validation(combinedMessage));
        }

        return Result<T>.Success(instance);
    }
}