namespace Woof.Api.DataAccess.Models.Instance;

public class LoopRunStepParameters : IRunStepParameter
{
    public int LoopCount { get; set; }
    public int CurrentLoopCount { get; set; }
}
