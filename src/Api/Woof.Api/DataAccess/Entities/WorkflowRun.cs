using Woof.Api.DataAccess.Models.Instance;
using Woof.Api.Enums;

namespace Woof.Api.DataAccess.Entities;

public class WorkflowRun : FileEntity
{
    public Guid WorkflowId { get; set; }
    public required WorkflowRunStep InitStep { get; set; }
    public eRunStatus RunStatus { get; set; }
}
