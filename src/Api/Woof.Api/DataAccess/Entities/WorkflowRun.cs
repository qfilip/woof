using Woof.Api.DataAccess.Models.Instance;

namespace Woof.Api.DataAccess.Entities;

public class WorkflowRun : LiteDbContextEntity
{
    public Guid WorkflowId { get; set; }
    public InitialRunStep? InitialStep { get; set; }
}
