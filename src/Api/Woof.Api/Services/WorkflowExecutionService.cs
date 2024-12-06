using System.Diagnostics;
using System.Reflection.Emit;
using System.Threading.Channels;
using Woof.Api.DataAccess;
using Woof.Api.DataAccess.Entities;
using Woof.Api.DataAccess.Models.Definition;
using Woof.Api.DataAccess.Models.Instance;
using Woof.Api.Enums;
using Woof.Api.Services.Abstractions;

namespace Woof.Api.Services;

public class WorkflowExecutionService
{
    private readonly IRunner _runner;
    private readonly ExecSearchService _ess;
    private readonly ChannelWriter<WorkflowRun> _writer;
    private readonly YamlFileStore<WorkflowRun> _runStore;
    private readonly YamlFileStore<Workflow> _defStore;
    private readonly ILogger<WorkflowExecutionService> _logger;

    public WorkflowExecutionService(
        IRunner runner,
        ExecSearchService ess,
        ChannelWriter<WorkflowRun> writer,
        YamlFileStore<WorkflowRun> runStore,
        YamlFileStore<Workflow> defStore,
        ILogger<WorkflowExecutionService> logger)
    {
        _runner = runner;
        _ess = ess;
        _writer = writer;
        _runStore = runStore;
        _defStore = defStore;
        _logger = logger;
    }

    public async Task<Opcode<WorkflowRun>> StartWorkflowAsync(Guid workflowId)
    {
        var wf = await _defStore.QueryAsync(xs => xs.FirstOrDefault(x => x.Id == workflowId));
        if (wf == null) return Opcode<WorkflowRun>.NotFound("Workflow not found.");

        var opcode = await CreateWorkflowRunAsync(wf);

        if (!opcode.Errors.Any()) return opcode;

        await _writer.WriteAsync(opcode.Data!);

        return opcode;
    }

    public async Task ExecuteNextStep(WorkflowRun wfr)
    {
        if (wfr.RunStatus == eRunStatus.Done)
        {
            return;
        }

        var workflowError = FindRunStep(wfr, x => x.State.StdErr?.Length > 0);
        if (workflowError != null)
        {
            await _runStore.UpdateAsync(wfr);
            return;
        }

        var currentRunStep = FindRunStep(wfr, x => !x.State.Completed);
        if (currentRunStep == null)
        {
            wfr.RunStatus = eRunStatus.Done;
            await _runStore.UpdateAsync(wfr);
            return;
        }
        
        if(!File.Exists(currentRunStep.ExecutablePath))
        {
            currentRunStep.State.Completed = true;
            currentRunStep.State.Status = "Executable file not found";
            await _runStore.UpdateAsync(wfr);
            return;
        }

        var stdErr = await _runner.RunStepAsync(currentRunStep);
        await _runStore.UpdateAsync(wfr);

        var noError = stdErr?.Length == 0;
        
        if (noError)
            await _writer.WriteAsync(wfr);
    }

    private async Task<Opcode<WorkflowRun>> CreateWorkflowRunAsync(Workflow wf)
    {
        var errors = new List<string>();
        
        WorkflowRunStep? MapSubsteps(WorkflowStep? step)
        {
            if (step == null) return null;

            var (ok, pathOrError) = _ess.TryFindExecutable(step);
            if (!ok)
                errors.Add($"Executable {step.ExecutableName} not found.");

            WorkflowRunStep runStep = step switch
            {
                InitialStep init => new InitialRunStep
                {
                    Id = init.Id,
                    Name = init.Name,
                    Arguments = init.Arguments,
                    ExecutablePath = pathOrError
                },
                SequentialStep seq => new SequentialRunStep()
                {
                    Id = seq.Id,
                    Name = seq.Name,
                    Arguments = seq.Arguments,
                    ExecutablePath = pathOrError
                },
                LoopStep loop => new LoopRunStep()
                {
                    Id = loop.Id,
                    Name = loop.Name,
                    Arguments = loop.Arguments,
                    ExecutablePath = pathOrError,
                    LoopCount = loop.LoopCount
                },
                _ => throw new UnreachableException("Step type not supported")
            };

            runStep.Next = MapSubsteps(step.Next);

            return runStep;
        }

        var initRunStep = MapSubsteps(wf.InitialStep);
        
        if(errors.Any())
        {
            return Opcode<WorkflowRun>.Rejected(errors);
        }

        var wfr = new WorkflowRun
        {
            Id = Guid.NewGuid(),
            WorkflowId = wf.Id,
            InitialStep = initRunStep as InitialRunStep
        };

        _runStore.Command(xs => xs.Add(wfr));
        await _runStore.CompleteAsync();

        return Opcode<WorkflowRun>.Ok(wfr);
    }
    private WorkflowStep? FindStep(Workflow wf, Func<WorkflowStep, bool> predicate)
    {
        WorkflowStep? currentStep = wf.InitialStep;
        while (currentStep != null)
        {
            if (predicate(currentStep))
                return currentStep;
            else
                currentStep = currentStep.Next;
        }

        return null;
    }
    private WorkflowRunStep? FindRunStep(WorkflowRun wfr, Func<WorkflowRunStep, bool> predicate)
    {
        WorkflowRunStep? currentStep = wfr.InitialStep;
        while (currentStep != null)
        {
            if (predicate(currentStep))
                return currentStep;
            else
                currentStep = currentStep.Next;
        }

        return null;
    }
}
