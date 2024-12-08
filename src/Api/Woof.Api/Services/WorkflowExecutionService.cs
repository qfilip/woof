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
    private readonly JsonFileStore<WorkflowRun> _runStore;
    private readonly JsonFileStore<Workflow> _defStore;
    private readonly ILogger<WorkflowExecutionService> _logger;

    public WorkflowExecutionService(
        IRunner runner,
        ExecSearchService ess,
        ChannelWriter<WorkflowRun> writer,
        JsonFileStore<WorkflowRun> runStore,
        JsonFileStore<Workflow> defStore,
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

        if (opcode.Errors.Any()) return opcode;

        await _writer.WriteAsync(opcode.Data!);

        return opcode;
    }

    public async Task ExecuteNextStep(WorkflowRun wfr)
    {
        if (wfr.RunStatus == eRunStatus.Done)
        {
            _logger.LogInformation("Workflow {WorkflowId} done", wfr.Id);
            return;
        }

        var workflowError = FindRunStep(wfr, x => x.State.StdErr?.Length > 0);
        if (workflowError != null)
        {
            _logger.LogInformation("Workflow {WorkflowId} ended with error", wfr.Id);
            await _runStore.UpdateAsync(wfr);
            return;
        }

        var currentRunStep = FindRunStep(wfr, x => !x.State.Completed);
        if (currentRunStep == null)
        {
            _logger.LogInformation("Workflow {WorkflowId} done", wfr.Id);
            wfr.RunStatus = eRunStatus.Done;
            await _runStore.UpdateAsync(wfr);
            return;
        }
        
        if(!File.Exists(currentRunStep.ExecutablePath))
        {
            _logger.LogInformation("Workflow {WorkflowId} step {StepId} no executable file found",
                wfr.Id,
                currentRunStep.Id);

            currentRunStep.State.Completed = true;
            currentRunStep.State.Status = "Executable file not found";
            await _runStore.UpdateAsync(wfr);
            return;
        }

        _logger.LogInformation("Worfklow {WorkflowId} running step {StepId}",
            wfr.Id,
            currentRunStep.Id);

        var stdErr = await _runner.RunStepAsync(currentRunStep);
        await _runStore.UpdateAsync(wfr);

        var noError = stdErr?.Length == 0;

        _logger.LogInformation("Worfklow {WorkflowId} finished step {StepId} without errors {NoErrors}",
            wfr.Id,
            currentRunStep.Id,
            noError);

        if (noError)
            await _writer.WriteAsync(wfr);
    }

    private async Task<Opcode<WorkflowRun>> CreateWorkflowRunAsync(Workflow wf)
    {
        var errors = new List<string>();
        
        WorkflowRunStep? MapSubsteps(WorkflowStep? step)
        {
            if (step == null) return null;

            var (ok, pathOrError) = _ess.MapExecutableFullPath(step);
            if (!ok)
                errors.Add($"{pathOrError} at step {step.Id}.");

            var runStep = new WorkflowRunStep
            {
                Id = step.Id,
                Name = step.Name,
                Arguments = step.Arguments,
                ExecutablePath = pathOrError,
                State = new(),
                LoopParameters = step.LoopParameters == null ? null : new LoopRunStepParameters
                {
                    LoopCount = step.LoopParameters.LoopCount
                },
                SequentialParameters = step.SequentialParameters == null ? null : new()
            };

            runStep.Next = MapSubsteps(step.Next);

            return runStep;
        }

        var initRunStep = MapSubsteps(wf.InitStep);
        
        if(errors.Any())
        {
            return Opcode<WorkflowRun>.Rejected(errors);
        }

        var wfr = new WorkflowRun
        {
            Id = Guid.NewGuid(),
            WorkflowId = wf.Id,
            InitStep = initRunStep!
        };

        _runStore.Command(xs => xs.Add(wfr));
        await _runStore.CompleteAsync();

        return Opcode<WorkflowRun>.Ok(wfr);
    }
    private WorkflowStep? FindStep(Workflow wf, Func<WorkflowStep, bool> predicate)
    {
        WorkflowStep? currentStep = wf.InitStep;
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
        WorkflowRunStep? currentStep = wfr.InitStep;
        
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
