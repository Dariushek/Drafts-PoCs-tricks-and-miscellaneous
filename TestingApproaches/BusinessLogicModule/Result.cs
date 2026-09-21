namespace BusinessLogicModule;

public sealed class Result<TValue>
{
    private readonly TValue? value;

    private Result(TValue value)
    {
        this.value = value;
        IsSuccess = true;
        Errors = [];
    }

    private Result(IReadOnlyList<Error> errors)
    {
        Errors = errors;
        IsSuccess = false;
    }

    public bool IsSuccess { get; }

    public TValue Value => IsSuccess ? value! : throw new InvalidOperationException("Result has no value.");

    public IReadOnlyList<Error> Errors { get; }

    public static Result<TValue> Success(TValue value) => new(value);

    public static Result<TValue> Failure(params IReadOnlyList<Error> errors) => new(errors);

    public TResult Match<TResult>
        (Func<TValue, TResult> onSuccess, Func<IReadOnlyList<Error>, TResult> onFailure) =>
        IsSuccess ? onSuccess(value!) : onFailure(Errors);
}