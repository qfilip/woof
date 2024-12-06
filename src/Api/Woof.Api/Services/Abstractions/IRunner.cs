using System.Diagnostics;
using Woof.Api.DataAccess.Models.Instance;

namespace Woof.Api.Services.Abstractions;

public interface IRunner
{
    Task<string> RunStepAsync<T>(T step) where T : WorkflowRunStep;
    static async Task<string> RunUnitAsync(string executablePath, string? arguments)
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
