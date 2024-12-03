using System.Diagnostics;
using System.Reflection.Emit;
using System.Threading.Channels;
using Woof.Api.DataAccess;
using Woof.Api.DataAccess.Entities;
using Woof.Api.DataAccess.Models.Definition;
using Woof.Api.DataAccess.Models.Instance;
using Woof.Api.Enums;

namespace Woof.Api.Services;

public class WorkflowExecutionService
{
    private readonly ExecSearchService _ess;
    private readonly ChannelWriter<WorkflowRun> _writer;
    private readonly LiteDbContext<WorkflowRun> _runDbContext;
    private readonly LiteDbContext<Workflow> _definitionDbContext;

    public WorkflowExecutionService(
        ExecSearchService ess,
        ChannelWriter<WorkflowRun> writer,
        LiteDbContext<WorkflowRun> runDbContext,
        LiteDbContext<Workflow> definitionDbContext)
    {
        _ess = ess;
        _writer = writer;
        _runDbContext = runDbContext;
        _definitionDbContext = definitionDbContext;
    }

    public async Task<Opcode<WorkflowRun>> StartWorkflowAsync(Guid workflowId)
    {
        var wf = _definitionDbContext.Find(x => x.Id == workflowId);
        if (wf == null) return Opcode<WorkflowRun>.NotFound("Workflow not found.");

        var opcode = CreateWorkflowRun(wf);

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
            _runDbContext.Update(wfr);
            return;
        }

        var currentRunStep = FindRunStep(wfr, x => !x.State.Completed);
        if (currentRunStep == null)
        {
            wfr.RunStatus = eRunStatus.Done;
            _runDbContext.Update(wfr);
            return;
        }
        
        if(!File.Exists(currentRunStep.ExecutablePath))
        {
            currentRunStep.State.Completed = true;
            currentRunStep.State.Status = "Executable file not found";
            _runDbContext.Update(wfr);
            return;
        }

        var runTask = currentRunStep switch
        {
            InitialRunStep init => RunStepAsync(init),
            SequentialRunStep seq => RunStepAsync(seq),
            LoopRunStep loop => RunStepAsync(loop),
            _ => throw new UnreachableException("Unsupported step type.")
        };

        var stdErr = await runTask;
        _runDbContext.Update(wfr);

        var noError = stdErr?.Length == 0;
        
        if (noError)
            await _writer.WriteAsync(wfr);
    }

    private Task<string> RunStepAsync(InitialRunStep step)
    {
        step.State.Completed = true;
        return Task.FromResult(string.Empty);
    }

    private Task<string> RunStepAsync(SequentialRunStep step)
    {
        step.State.Completed = true;
        return RunUnitAsync(step.ExecutablePath, step.Arguments);
    }

    private async Task<string> RunStepAsync(LoopRunStep step)
    {
        if (step.CurrentLoopCount == step.LoopCount)
        {
            step.State.Completed = true;
            return string.Empty;
        }
        var stdErr = await RunUnitAsync(step.ExecutablePath, step.Arguments);
        step.CurrentLoopCount++;

        return stdErr;
    }

    private Opcode<WorkflowRun> CreateWorkflowRun(Workflow wf)
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

        _runDbContext.Insert(wfr);

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
    private async Task<string> RunUnitAsync(string executablePath, string? arguments)
    {
        var info = new ProcessStartInfo()
        {
            FileName = executablePath,
            Arguments = arguments,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
        };

        using var proc = Process.Start(info);
        await proc!.WaitForExitAsync();

        return await proc.StandardError.ReadToEndAsync();
    }

}
