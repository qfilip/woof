using Woof.Api.DataAccess;
using Woof.Api.DataAccess.Entities;
using Woof.Api.DataAccess.Models.Definition;

namespace Woof.Api.Services;

public class WorkflowBuilderService
{
    private readonly YamlFileStore<Workflow> _fs;
    private readonly ExecSearchService _ess;

    public WorkflowBuilderService(YamlFileStore<Workflow> fs, ExecSearchService ess)
    {
        _fs = fs;
        _ess = ess;
    }

    public async Task<Workflow> CreateAsync(string name)
    {
        var wf = new Workflow
        {
            Id = Guid.NewGuid(),
            Name = name,
            InitialStep = new()
            { ExecutableName = string.Empty },
        };
        
        _fs.Command(xs => xs.Add(wf));
        await _fs.CompleteAsync();

        return wf;
    }

    public async Task<Opcode<Workflow>> AddNextStepAsync<T>(
        Guid workflowId,
        Guid targetStepId,
        WorkflowStep nextStep) where T : WorkflowStep
    {
        var wf = await _fs.QueryAsync(xs => xs.FirstOrDefault(x => x.Id == workflowId));
        if (wf == null) return Opcode<Workflow>.NotFound("Workflow not found.");

        var (hasExecutable, _) = _ess.TryFindExecutable(nextStep);
        if(!hasExecutable)
            return Opcode<Workflow>.NotFound("Executable not found.");

        nextStep.Id = Guid.NewGuid();
        var stepAdded = AddNextStep(wf.InitialStep, targetStepId, nextStep);
        
        if(stepAdded)
        {
            await _fs.UpdateAsync(wf);
        }

        return stepAdded ? Opcode<Workflow>.Ok(wf) : Opcode<Workflow>.NotFound("Parent step not found.");
    }

    public async Task<Opcode<Workflow>> RemoveStepAsync(Guid workflowId, Guid stepId)
    {
        var wf = await _fs.QueryAsync(xs => xs.FirstOrDefault(x => x.Id == workflowId));
        if (wf == null) return Opcode<Workflow>.NotFound("Workflow not found.");

        if(wf.InitialStep ==  null) return Opcode<Workflow>.NotFound();

        var stepRemoved = RemoveStep(wf.InitialStep, stepId);

        if (stepRemoved)
        {
            await _fs.UpdateAsync(wf);
        }

        return stepRemoved ? Opcode<Workflow>.Ok(wf) : Opcode<Workflow>.NotFound("Step not found.");
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
