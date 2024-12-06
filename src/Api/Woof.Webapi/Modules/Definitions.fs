module rec Definitions

open System

type StepInfo = {
    Id: Guid
    Name: string
    ExecFile: string
    Arguments: string
    Next: WorkflowStep option
}

type InitStep = {
    StepInfo: StepInfo
}

type LoopStep = {
    StepInfo: StepInfo
    LoopCount: int
}

type SequentialStep = {
    StepInfo: StepInfo
}

type WorkflowStep = 
| InitStep of InitStep
| LoopStep of LoopStep
| SequentialStep of SequentialStep

type Workflow = {
    Id: Guid
    Name: string
    Init: InitStep
}