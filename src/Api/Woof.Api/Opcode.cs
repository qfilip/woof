using System.Diagnostics;

namespace Woof.Api;

public class Opcode
{
    private Opcode(object? data, int code)
    {
        Data = data;
        Code = code;
    }

    public int Code { get; }
    public object? Data { get; }

    public static Opcode Ok(object? data = null) => new Opcode(data, 200);
    public static Opcode NotFound() => new Opcode(null, 404);
    public static Opcode NotFound(string message) => new Opcode(message, 404);
    public static Opcode Rejected(string reason) => new Opcode(reason, 400);
    public static Opcode Rejected(IEnumerable<string> reasons) => new Opcode(reasons, 400);
}

public static class OpcodeExtensions
{
    public static IResult ToResult(this Opcode opcode)
    {
        return opcode.Code switch
        {
            200 => Results.Ok(opcode.Data),
            400 => Results.Conflict(opcode.Data),
            404 => Results.NotFound(opcode.Data),
            _ => throw new UnreachableException($"Opcode {opcode.Code} not supported")
        };
    }
}
