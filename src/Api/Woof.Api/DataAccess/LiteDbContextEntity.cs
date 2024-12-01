using LiteDB;

namespace Woof.Api.DataAccess;

public class LiteDbContextEntity
{
    [BsonId(true)]
    public Guid Id { get; set; }
}
