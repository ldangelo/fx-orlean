namespace EventServer;

public class StartUpTask: IHostedServices
{
    public Task StartAsync(CancelationToken cancellationToken)
    {
        return Task.CompltedTask;
    }
}