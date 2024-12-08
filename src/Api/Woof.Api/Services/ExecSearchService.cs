using Woof.Api.DataAccess.Models.Definition;

namespace Woof.Api.Services;

public class ExecSearchService
{
    private readonly string _execsRootPath;

    public ExecSearchService(string execsRootPath)
    {
        _execsRootPath = execsRootPath;
    }

    public (bool, string) MapExecutableFullPath<T>(T step) where T : WorkflowStep
    {
        var files = Directory.GetFiles(_execsRootPath, step.ExecutableName, SearchOption.AllDirectories);

        return files.Length switch
        {
            0 => (false, "Executable file not found"),
            > 1 => (false, "More than one executable file found"),
            _ => (true, files[0])
        };
    }
}
