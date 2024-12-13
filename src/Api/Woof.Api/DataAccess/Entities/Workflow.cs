using Woof.Api.DataAccess.Models.Definition;

namespace Woof.Api.DataAccess.Entities;

public class Workflow : FileEntity
{
    public string? Name { get; set; }
    public WorkflowStep? InitStep { get; set; }
}
