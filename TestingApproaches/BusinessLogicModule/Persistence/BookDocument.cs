using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BusinessLogicModule.Persistence;

// BSON-mapped DTOs, purely for (de)serialization - domain entities stay
// persistence-ignorant (see Book.FromPersistence for why). Guid must be
// stored as a string: MongoDB.Driver 3.x throws on Guid serialization
// without an explicit representation.
internal sealed class BookDocument
{
    [BsonId]
    [BsonRepresentation(BsonType.String)]
    public Guid Id { get; set; }

    public string Isbn { get; set; } = "";

    public string Title { get; set; } = "";

    public string Author { get; set; } = "";

    public int CopiesAvailable { get; set; }
}
