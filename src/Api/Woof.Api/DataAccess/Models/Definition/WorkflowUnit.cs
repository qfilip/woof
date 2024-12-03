namespace Woof.Api.DataAccess.Models.Definition;

public class WorkflowUnit
{
    private readonly string _executableName;
    private readonly string? _args;

    public WorkflowUnit(string executableName, string? args)
    {
        _executableName = executableName;
        _args = args;
    }

    public string ExecutableName { get => _executableName; }
    public string? Args { get => _args; }
}
