using Woof.Api.DataAccess.Models.Definition;

namespace Woof.Api.DataAccess.Entities;

public class Workflow : LiteDbContextEntity
{
    public Workflow(Guid id, string? name, InitialStep initialStep)
    {
        Id = id;
        Name = name;
        InitialStep = initialStep;
    }

    public string? Name { get; set; }
    public InitialStep InitialStep { get; set; }
}
