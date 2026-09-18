namespace BusinessLogicModule.Books;

public sealed record BookAvailabilityResult(Guid BookId, bool IsAvailable, int CopiesAvailable);
