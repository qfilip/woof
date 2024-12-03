using System.Threading.Channels;
using Woof.Api.DataAccess.Entities;
using Woof.Api.Services;

namespace Woof.Api.Messaging;

public class ChannelHostingService : BackgroundService
{
    private readonly ChannelReader<WorkflowRun> _reader;
    private readonly IServiceProvider _serviceProvider;

    public ChannelHostingService(
        ChannelReader<WorkflowRun> reader,
        IServiceProvider serviceProvider)
    {
        _reader = reader;
        _serviceProvider = serviceProvider;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!_reader.Completion.IsCompleted)
        {
            Console.WriteLine("Got message");

            var message = await _reader.ReadAsync();
            using var scope = _serviceProvider.CreateScope();
            var service = scope.ServiceProvider.GetRequiredService<WorkflowExecutionService>();

            try
            {
                await service.ExecuteNextStep(message);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Channel exception {ex.Message}");
            }
        }
    }
}
