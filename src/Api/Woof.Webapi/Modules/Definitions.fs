module rec Definitions

open System
open Dtos
open Woof.Webapi.DataAccess

type ExecInfo = {
    FileName: string
    Arguments: string
}

type LoopStepParameters = {
    LoopCount: int
}

type Step<'a> = {
    Id: Guid
    Parameters: 'a
    ExecInfo: ExecInfo
    Next: WorkflowStep option
}

type InitStep = Step<unit>

type WorkflowStep =
| LoopStep of Step<LoopStepParameters>
| SequentialStep of Step<unit>

type Workflow = {
    Id: Guid
    Name: string
    Init: InitStep
}

let createWorkflow (dto: CreateWorkflowDto) (fs: YamlFileStore<Workflow>) = task {
    let init: InitStep = {
        Id = Guid.NewGuid()
        ExecInfo = {
            FileName = ""
            Arguments = ""
        }
        Parameters = ()
        Next = None
    }

    let wf: Workflow = {
        Id = Guid.NewGuid()
        Name = dto.Name
        Init = init
    }

    fs.Do(fun xs -> wf::xs)
    do! fs.Complete()

    return wf
}
    