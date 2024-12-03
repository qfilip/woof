using Woof.Api.DataAccess;
using Woof.Api.DataAccess.Entities;
using Woof.Api.DataAccess.Models.Definition;

namespace Woof.Api.Services;

public class WorkflowBuilderService
{
    private readonly LiteDbContext<Workflow> _dbContext;
    private readonly ExecSearchService _ess;

    public WorkflowBuilderService(LiteDbContext<Workflow> dbContext, ExecSearchService ess)
    {
        _dbContext = dbContext;
        _ess = ess;
    }

    public Workflow Create(string name)
    {
        var wf = new Workflow
        {
            Id = Guid.NewGuid(),
            Name = name,
            InitialStep = new InitialStep(new(string.Empty, null))
            {
                Id = Guid.NewGuid()
            },
        };
        
        _dbContext.Insert(wf);

        return wf;
    }

    public Opcode AddNextStep<T>(Guid workflowId, Guid targetStepId, WorkflowStep nextStep) where T : WorkflowStep
    {
        var wf = _dbContext.Find(x => x.Id == workflowId);
        if (wf == null) return Opcode.NotFound("Workflow not found.");

        var (hasExecutable, _) = _ess.TryFindExecutable(nextStep);
        if(!hasExecutable)
            return Opcode.NotFound("Executable not found.");

        nextStep.Id = Guid.NewGuid();
        var stepAdded = AddNextStep(wf.InitialStep, targetStepId, nextStep);
        _dbContext.Update(wf);

        return stepAdded ? Opcode.Ok(wf) : Opcode.NotFound("Parent step not found.");
    }

    public Opcode RemoveStep(Guid workflowId, Guid stepId)
    {
        var wf = _dbContext.Find(x => x.Id == workflowId);
        if (wf == null) return Opcode.NotFound("Workflow not found.");

        if(wf.InitialStep ==  null) return Opcode.NotFound();

        var stepRemoved = RemoveStep(wf.InitialStep, stepId);
        _dbContext.Update(wf);

        return stepRemoved ? Opcode.Ok(wf) : Opcode.NotFound("Step not found.");
    }

    private static bool AddNextStep<T>(WorkflowStep? step, Guid targetStepId, T nextStep) where T : WorkflowStep
    {
        if (step == null)
        {
            return false;
        }
        else if(step.Id == targetStepId)
        {
            step.Next = nextStep;
            return true;
        }
        else
        {
            return AddNextStep(step.Next, targetStepId, nextStep);
        }
    }

    private static bool RemoveStep(WorkflowStep parentStep, Guid targetStepId)
    {
        if (parentStep.Next == null)
        {
            return false;
        }
        else if(parentStep.Next.Id == targetStepId)
        {
            parentStep.Next = null;
            return true;
        }
        else
        {
            return RemoveStep(parentStep.Next, targetStepId);
        }
    }
}
