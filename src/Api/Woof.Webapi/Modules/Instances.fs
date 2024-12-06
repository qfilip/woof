module rec Instances

open System

type StepRunInfo = {
    Id: Guid
    Name: string
    ExecFile: string
    Arguments: string
    Next: WorkflowRunStep option
}

type InitRunStep = {
    StepInfo: StepRunInfo
}

type LoopRunStep = {
    StepInfo: StepRunInfo
    LoopCount: int
}

type SequentialRunStep = {
    StepInfo: StepRunInfo
}

type WorkflowRunStep = 
| InitRunStep of InitRunStep
| LoopRunStep of LoopRunStep
| SequentialRunStep of SequentialRunStep

type WorkflowRun = {
    Id: Guid
    Name: string
    Init: InitRunStep
}

let mapToRunStepInfo (x: Definitions.StepInfo): StepRunInfo = {
    Id = x.Id
    Name = x.Name

}

let mapWorkflowRun wf =
    let mutable errors: string list = []
    let mapSubsteps step =
        let substep =
            match step with
            | None -> None
            | Some s ->
                match s with
                | Definitions.InitStep x -> InitRunStep: {
                    
                }
            