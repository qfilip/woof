using Woof.Api.DataAccess.Models.Definition;

namespace Woof.Api.DataAccess.Entities;

public class Workflow : LiteDbContextEntity
{
    public string? Name { get; set; }
    public required InitialStep InitialStep { get; set; }
}
