namespace Woof.Api.DataAccess.Models.Definition;

public class WorkflowUnit
{
    private readonly string _functionPath;
    private readonly string? _args;

    public WorkflowUnit(string functionPath, string? args)
    {
        _functionPath = functionPath;
        _args = args;
    }

    public string FunctionPath { get => _functionPath; }
    public string? Args { get => _args; }
}
