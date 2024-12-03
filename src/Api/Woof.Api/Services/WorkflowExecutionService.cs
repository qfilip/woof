using System.Diagnostics;
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

    public async Task<Opcode> StartWorkflowAsync(Guid workflowId)
    {
        var wf = _definitionDbContext.Find(x => x.Id == workflowId);
        if (wf == null) return Opcode.NotFound("Workflow not found.");

        var wfr = CreateWorkflowRun(wf);

        await _writer.WriteAsync(wfr);

        return Opcode.Ok(wfr);
    }

    public async Task ExecuteNextStep(WorkflowRun wfr)
    {
        if (wfr.RunStatus == eRunStatus.Done)
        {
            Console.WriteLine("Workflow run done");
            return;
        }

        var workflowError = FindRunStep(wfr, x => x.State.StdErr?.Length > 0);
        if (workflowError != null)
        {
            Console.WriteLine("Workflow run done with error");
            _runDbContext.Update(wfr);
            return;
        }

        var currentRunStep = FindRunStep(wfr, x => !x.State.Completed);
        if (currentRunStep == null)
        {
            Console.WriteLine("Workflow run done");
            wfr.RunStatus = eRunStatus.Done;
            _runDbContext.Update(wfr);
            return;
        }

        var (found, pathOrError) = _ess.TryFindExecutable(currentRunStep.State.Step);
        
        if(!found)
        {
            currentRunStep.State.Completed = true;
            currentRunStep.State.Status = pathOrError;
            _runDbContext.Update(wfr);
            return;
        }

        Console.WriteLine($"Executing {currentRunStep.State.Step.Name}");

        var runTask = currentRunStep switch
        {
            InitialRunStep init => RunStepAsync(init),
            SequentialRunStep seq => RunStepAsync(seq, pathOrError),
            LoopRunStep loop => RunStepAsync(loop, pathOrError),
            _ => throw new UnreachableException("Unsupported step type.")
        };

        var stdErr = await runTask;
        currentRunStep.State.Completed = true;
        _runDbContext.Update(wfr);

        var noError = stdErr != null && stdErr.Length > 0;
        
        if (noError)
            await _writer.WriteAsync(wfr);
    }

    private Task<string> RunStepAsync(InitialRunStep step) => Task.FromResult(string.Empty);

    private Task<string> RunStepAsync(SequentialRunStep step, string executablePath)
    {
        return RunUnitAsync(executablePath, step.State.Step.Unit.Args);
    }

    private async Task<string> RunStepAsync(LoopRunStep step, string executablePath)
    {
        var stdErr = await RunUnitAsync(executablePath, step.State.Step.Unit.Args);
        step.CurrentLoopCount++;

        return stdErr;
    }

    private WorkflowRun CreateWorkflowRun(Workflow wf)
    {
        var wfr = new WorkflowRun
        {
            Id = Guid.NewGuid(),
            WorkflowId = wf.Id,
            InitialStep = new(wf.InitialStep!)
        };

        void AddNext(WorkflowStep? step, WorkflowRunStep? runStep)
        {
            if (step == null) return;
            runStep = step switch
            {
                SequentialStep seq => new SequentialRunStep(seq),
                LoopStep loop => new LoopRunStep(loop),
                _ => throw new UnreachableException("Step type not supported")
            };

            AddNext(step.Next, runStep.Next);
        }

        AddNext(wf.InitialStep!.Next, wfr.InitialStep.Next);

        _runDbContext.Insert(wfr);

        return wfr;
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
