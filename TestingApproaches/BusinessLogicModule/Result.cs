namespace BusinessLogicModule;

public sealed class Result<TValue>
{
    private readonly TValue? _value;

    private Result(TValue value)
    {
        _value = value;
        IsSuccess = true;
        Errors = [];
    }

    private Result(IReadOnlyList<Error> errors)
    {
        Errors = errors;
        IsSuccess = false;
    }

    public bool IsSuccess { get; }

    public TValue Value => IsSuccess ? _value! : throw new InvalidOperationException("Result has no value.");

    public IReadOnlyList<Error> Errors { get; }

    public static Result<TValue> Success(TValue value)
    {
        return new Result<TValue>(value);
    }

    public static Result<TValue> Failure(params IReadOnlyList<Error> errors)
    {
        if (errors.Select(error => error.Type).Distinct().Count() > 1)
            throw new ArgumentException("All errors in a failed result must share the same error type.", nameof(errors));

        return new Result<TValue>(errors);
    }

    public TResult Match<TResult>(Func<TValue, TResult> onSuccess, Func<IReadOnlyList<Error>, TResult> onFailure)
    {
        return IsSuccess ? onSuccess(_value!) : onFailure(Errors);
    }
}