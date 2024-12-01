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

    public async Task ExecuteNextStep(WorkflowRun wfr)
    {
        if (wfr.RunStatus == eRunStatus.Done) return;

        var workflowError = FindRunStep(wfr, x => x.StdErr?.Length > 0);
        if (workflowError != null)
        {
            _runDbContext.Update(wfr);
            return;
        }

        var currentRunStep = FindRunStep(wfr, x => !x.Completed);
        if (currentRunStep == null)
        {
            wfr.RunStatus = eRunStatus.Done;
            _runDbContext.Update(wfr);
            return;
        }

        var wf = _definitionDbContext.Find(x => x.Id == wfr.WorkflowId);
        if (wf == null)
            throw new InvalidOperationException($"Workflow {wfr.WorkflowId} not found.");

        if (currentRunStep is InitialRunStep irStep)
        {
            irStep.Completed = true;
        }
        else if (currentRunStep is SequentialRunStep seqStep)
        {
            var step = FindStep(wf, x => x.Id == currentRunStep.Id);
        }
        else if (currentStep is LoopStep loopStep)
        {
            for (var i = 0; i < loopStep.LoopCount; i++)
            {
                var error = await RunUnitAsync(pathOrError, loopStep.Unit!.Args);
            }
        }

        _runDbContext.Update(wfr);

        throw new NotImplementedException();
    }

    public async Task<Opcode> StartWorkflowAsync(Guid workflowId)
    {
        var wf = _definitionDbContext.Find(x => x.Id == workflowId);
        if (wf == null) return Opcode.NotFound("Workflow not found.");

        var wfr = CreateWorkflowRun(wf);
        _runDbContext.Insert(wfr);

        await _writer.WriteAsync(wfr);

        return Opcode.Ok(wfr);
    }

    private WorkflowRun CreateWorkflowRun(Workflow wf)
    {
        var wfr = new WorkflowRun
        {
            Id = Guid.NewGuid(),
            WorkflowId = wf.Id,
            InitialStep = new()
            {
                Id = wf.InitialStep!.Id
            }
        };

        var currentStep = wf.InitialStep.Next;
        var currentRunStep = wfr.InitialStep.Next;

        while (currentStep != null)
        {
            currentRunStep = currentStep switch
            {
                SequentialStep seq => new SequentialRunStep { Id = seq.Id },
                LoopStep seq => new LoopRunStep { Id = seq.Id, CurrentLoopCount = 0 },
            };

            currentStep = currentStep.Next;
            currentRunStep = currentRunStep.Next;
        }

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
