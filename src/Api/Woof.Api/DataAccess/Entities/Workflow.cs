using Woof.Api.DataAccess.Models.Definition;

namespace Woof.Api.DataAccess.Entities;

public class Workflow : YamlEntity
{
    public string? Name { get; set; }
    public WorkflowStep? InitStep { get; set; }
}
