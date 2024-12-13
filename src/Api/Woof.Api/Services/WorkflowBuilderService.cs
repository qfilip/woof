using Woof.Api.DataAccess;
using Woof.Api.DataAccess.Entities;
using Woof.Api.DataAccess.Models.Definition;
using Woof.Api.Dtos;

namespace Woof.Api.Services;

public class WorkflowBuilderService
{
    private readonly IFileStore<Workflow> _fs;
    private readonly ExecSearchService _ess;

    public WorkflowBuilderService(IFileStore<Workflow> fs, ExecSearchService ess)
    {
        _fs = fs;
        _ess = ess;
    }

    public async Task<Workflow> CreateAsync(string name)
    {
        var wf = new Workflow
        {
            Id = Guid.NewGuid(),
            Name = name
        };
        
        _fs.Command(xs => xs.Add(wf));
        await _fs.CompleteAsync();

        return wf;
    }

    public async Task<Opcode<Workflow>> AddNextStepAsync(AddNextStepDto dto)
    {
        var definedParametersCount = 0;

        if (dto.Step.LoopParameters != null) definedParametersCount++;
        if (dto.Step.SequentialParameters != null) definedParametersCount++;
        
        if (definedParametersCount == 0)
            return Opcode<Workflow>.Rejected("No defined parameters");

        if (definedParametersCount > 1)
            return Opcode<Workflow>.Rejected("More than one parameter defined");

        var wf = await _fs.QueryAsync(xs => xs.FirstOrDefault(x => x.Id == dto.WorkflowId));
        if (wf == null) return Opcode<Workflow>.NotFound("Workflow not found.");

        var (hasExecutable, pathOrError) = _ess.MapExecutableFullPath(dto.Step);
        if (!hasExecutable)
            return Opcode<Workflow>.NotFound(pathOrError);

        var nextStep = new WorkflowStep
        {
            Id = Guid.NewGuid(),
            Name = dto.Step.Name,
            Arguments = dto.Step.Arguments,
            ExecutableName = dto.Step.ExecutableName,
            LoopParameters = dto.Step.LoopParameters,
            SequentialParameters = dto.Step.SequentialParameters
        };
        
        if (dto.ParentStepId == null)
        {
            wf.InitStep = nextStep;
            await _fs.UpdateAsync(wf);
            return Opcode<Workflow>.Ok(wf);
        }

        var stepAdded = AddNextStep(wf.InitStep, dto.ParentStepId!.Value, nextStep);
        
        if(stepAdded)
            await _fs.UpdateAsync(wf);

        return stepAdded ? Opcode<Workflow>.Ok(wf) : Opcode<Workflow>.NotFound("Parent step not found.");
    }

    public async Task<Opcode<Workflow>> RemoveStepAsync(Guid workflowId, Guid stepId)
    {
        var wf = await _fs.QueryAsync(xs => xs.FirstOrDefault(x => x.Id == workflowId));
        if (wf == null) return Opcode<Workflow>.NotFound("Workflow not found.");

        if(wf.InitStep == null) return Opcode<Workflow>.NotFound();

        var stepRemoved = RemoveStep(wf.InitStep, stepId);

        if (stepRemoved)
        {
            await _fs.UpdateAsync(wf);
        }

        return stepRemoved ? Opcode<Workflow>.Ok(wf) : Opcode<Workflow>.NotFound("Step not found.");
    }

    private static bool AddNextStep<T>(WorkflowStep? step, Guid parentStepId, T nextStep) where T : WorkflowStep
    {
        if (step == null)
        {
            return false;
        }
        else if(step.Id == parentStepId)
        {
            step.Next = nextStep;
            return true;
        }
        else
        {
            return AddNextStep(step.Next, parentStepId, nextStep);
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
