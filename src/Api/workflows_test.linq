<Query Kind="Program">
  <Namespace>System.Threading.Tasks</Namespace>
  <Namespace>System.Net.Http</Namespace>
  <Namespace>System.Text.Json</Namespace>
  <Namespace>static UserQuery.LoopStep</Namespace>
</Query>

async Task Main()
{
	var clinet = new HttpClient();
	var wflow = new Workflow() { Name = "crap" };
	
	wflow = await Send(wflow, clinet, "/definitions/create");
	var init = new
	{
	  WorkflowId = wflow.Id,
	  Step = new SequentialStep()
	  {
	  	Name = "mult",
		ExecutableName = "Exec.Multiply.exe",
		Arguments = "1 2"
	  }
	};

	wflow = await Send(init, clinet, "/definitions/add_sequential");
	var loop = new
	{
	  WorkflowId = wflow.Id,
	  ParentStepId = wflow.InitStep.Id,
	  Step = new LoopStep()
	  {
	  	Name = "mult",
		ExecutableName = "Exec.Multiply.exe",
		Arguments = "1 2",
		Parameters = new LoopStepParameters()
		{
			LoopCount = 3
		}
	  }
	};

	await Send(loop, clinet, "/definitions/add_loop");
	await Send(new { WorkflowId = wflow.Id }, clinet, "/runs");
}

public async Task<Workflow> Send(object data, HttpClient client, string path)
{
	var url = new Uri($"http://localhost:5198{path}");
	var jsonContent = JsonSerializer.Serialize(data);
	var msg = new HttpRequestMessage();

	msg.RequestUri = url;
	msg.Method = HttpMethod.Post;
	msg.Content = new StringContent(jsonContent, Encoding.UTF8, "application/json");

	var resp = await client.SendAsync(msg);
	resp.EnsureSuccessStatusCode();
	
	var json = await resp.Content.ReadAsStringAsync();

	return JsonSerializer.Deserialize<Workflow>(json, new JsonSerializerOptions()
	{
		PropertyNameCaseInsensitive = true
	});
}

public class FileEntity
{
	public Guid Id { get; set; }
}

public class Workflow : FileEntity
{
	public string? Name { get; set; }
	public SequentialStep? InitStep { get; set; }
}

public class WorkflowStep
{
	public Guid Id { get; set; }
	public string? Type { get; set; }
	public string? Name { get; set; }
	public string ExecutableName { get; set; }
	public string? Arguments { get; set; }
	public WorkflowStep? Next { get; set; }
}

public class SequentialStep : WorkflowStep
{
}

public class LoopStep : WorkflowStep
{
	public LoopStepParameters Parameters { get; set; }
	
	public class LoopStepParameters
	{
		public int LoopCount { get; set; }
	}
}