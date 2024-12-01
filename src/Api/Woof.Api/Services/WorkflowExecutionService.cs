using System.Diagnostics;
using System.Threading.Channels;
using Woof.Api.DataAccess;
using Woof.Api.DataAccess.Entities;
using Woof.Api.DataAccess.Models.Definition;
using Woof.Api.DataAccess.Models.Instance;

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

    public async Task ExecuteStep(WorkflowRun wfr)
    {
        //var wf = _definitionDbContext.
    }

    public async Task<IResult> CreateWorkflowRun(Guid workflowId)
    {
        var wf = _definitionDbContext.Find(x => x.Id == workflowId);
        if (wf == null) return Opcode.NotFound("Workflow not found.");

        var wfr = new WorkflowRun
        {
            Id = Guid.NewGuid(),
            WorkflowId = workflowId,
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


    }

    public async Task<IResult> Start(Guid workflowId)
    {
        var wf = _repository.Data.FirstOrDefault(x => x.Id == workflowId);
        if (wf == null) return Opcode.NotFound();

        var currentStep = wf.InitialStep!.Next;
        
        while(currentStep != null)
        {
            var (ok, pathOrError) = _ess.TryFindExecutable(currentStep);
            if (!ok)
            {
                // log error
                break;
            }

            if (currentStep is SequentialStep seqStep)
            {
                var error = await RunUnitAsync(pathOrError, seqStep.Unit!.Args);
            }
            else if (currentStep is LoopStep loopStep)
            {
                for (var i = 0; i < loopStep.LoopCount; i++)
                {
                    var error = await RunUnitAsync(pathOrError, loopStep.Unit!.Args);
                }
            }
        }
    }

    public async Task<string> RunUnitAsync(string executablePath, string? arguments)
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
