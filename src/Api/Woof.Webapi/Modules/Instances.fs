module rec Instances

open System
open Definitions

type ExecRunInfo = {
    FilePath: string
    Arguments: string
}

type LoopStepRunParameters = {
    LoopCount: int
    LoopsCompleted: int
}

type StepRun<'a> = {
    Id: Guid
    Parameters: 'a
    ExecInfo: ExecRunInfo
    Next: WorkflowStepRun option
}

type InitStepRun = StepRun<unit>

type WorkflowStepRun =
| LoopStepRun of StepRun<LoopStepRunParameters>
| SequentialStepRun of StepRun<unit>

type WorkflowRun = {
    Id: Guid
    WorkflowId: Guid
    Name: string
    Init: InitStepRun
}

let private mapStepRun<'a, 'b> (step: Step<'a>) (execFilePath: string) (parameters: 'b) (next: WorkflowStepRun option) =
    let result: StepRun<'b> = {
        Id = step.Id
        Parameters = parameters
        ExecInfo = {
            FilePath = execFilePath
            Arguments = step.ExecInfo.Arguments
        }
        Next = next
    }

    result

let private getStepExecutableFile workflowStep =
    let fileName =
        match workflowStep with
        | Definitions.WorkflowStep.LoopStep loop -> loop.ExecInfo.FileName
        | Definitions.WorkflowStep.SequentialStep seqs -> seqs.ExecInfo.FileName

    Executables.findExecutable fileName
        
let mapWorkflowRun (wf: Workflow) =
    let mutable errors: string list = []

    let rec mapSubsteps workflowStep =
        match workflowStep with
        | None -> None
        | Some step ->
            let execFile = getStepExecutableFile step
            match execFile with
            | Error e ->
                errors <- e::errors
                None
            | Ok file ->
                match step with
                | LoopStep loop ->
                    let parameters = {
                        LoopCount = loop.Parameters.LoopCount
                        LoopsCompleted = 0
                    }
                    let next = mapSubsteps loop.Next
                    let runStep = mapStepRun loop file parameters next
                    Some (LoopStepRun runStep)

                | SequentialStep seqs ->
                    let next = mapSubsteps seqs.Next
                    let runStep = mapStepRun seqs file () next
                    
                    Some (SequentialStepRun runStep)
    
    let substeps = mapSubsteps wf.Init.Next

    match errors.Length with
    | 0 ->
        let init: InitStepRun = {
            Id = wf.Init.Id
            Parameters = ()
            ExecInfo = {
                FilePath = ""
                Arguments = ""
            }
            Next = substeps
        }

        let wfr: WorkflowRun = {
            Id = Guid.NewGuid()
            WorkflowId = wf.Id
            Name = wf.Name
            Init = init
        }
    
        Ok wfr
    | _ -> Error errors
            