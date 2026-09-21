namespace BusinessLogicModule.Books;

public sealed record BookAvailability(Guid BookId, bool IsAvailable, int CopiesAvailable);
