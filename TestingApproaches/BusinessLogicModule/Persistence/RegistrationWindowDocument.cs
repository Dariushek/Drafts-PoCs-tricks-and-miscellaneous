using MongoDB.Bson.Serialization.Attributes;

namespace BusinessLogicModule.Persistence;

internal sealed class RegistrationWindowDocument
{
    [BsonId]
    public int Id { get; set; }

    public bool IsOpen { get; set; }
}