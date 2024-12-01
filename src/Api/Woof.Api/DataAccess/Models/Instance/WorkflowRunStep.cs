namespace Woof.Api.DataAccess.Models.Instance;

public class WorkflowRunStep
{
    public Guid Id { get; set; }
    public bool Completed { get; set; }
    public string? Status { get; set; }
    public string? StdErr { get; set; }
    public WorkflowRunStep? Next { get; set; }
}
