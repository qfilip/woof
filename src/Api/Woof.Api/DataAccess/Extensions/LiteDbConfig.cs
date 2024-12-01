namespace Woof.Api.DataAccess.Extensions;

public class LiteDbConfig
{
    public LiteDbConfig(IWebHostEnvironment env)
    {
        DatabasePath = Path.Combine(env.WebRootPath, "workflows.db");
    }

    public string DatabasePath { get; }
}
