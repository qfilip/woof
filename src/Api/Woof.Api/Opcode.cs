using System.Diagnostics;

namespace Woof.Api;

public class Opcode<T>
{
    private Opcode(T? data, int code, IEnumerable<string> errors)
    {
        Data = data;
        Code = code;
        Errors = errors;
    }

    public int Code { get; }
    public T? Data { get; }
    public IEnumerable<string> Errors { get; set; }

    public static Opcode<T> Ok(T? data) => new Opcode<T>(data, 200, []);
    public static Opcode<T> NotFound() => new Opcode<T>(default, 404, []);
    public static Opcode<T> NotFound(string message) => new Opcode<T>(default, 404, [message]);
    public static Opcode<T> Rejected(string reason) => new Opcode<T>(default, 400, [reason]);
    public static Opcode<T> Rejected(IEnumerable<string> reasons) => new Opcode<T>(default, 400, reasons);
}

public static class OpcodeExtensions
{
    public static IResult ToResult<T>(this Opcode<T> opcode)
    {
        return opcode.Code switch
        {
            200 => Results.Ok(opcode.Data),
            400 => Results.Conflict(opcode.Errors),
            404 => Results.NotFound(opcode.Errors),
            _ => throw new UnreachableException($"Opcode {opcode.Code} not supported")
        };
    }
}
