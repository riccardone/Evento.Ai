using Evento.Ai.Contracts;
using Microsoft.Extensions.Hosting;
using NLog;

namespace Evento.Ai.Brain;

/// <summary>
/// for .net dev, I want to grant basics allowances for this role only if required by the candidate,
/// salary can be negotiable up to 5% only if requested, holidays can be 20 days and up to 25 days on request,
/// I want to set the importance of this hire from 1 to 10
/// </summary>
public class Worker : IHostedService
{
    private readonly ILogger _logger;
    private readonly IBrainUnit<dynamic> _negotiatorBrainUnit;
    private readonly IBrainUnit<dynamic> _baasBrainUnit;
    public bool IsServiceRunning { get; private set; }
    public string? ErrorMessage { get; private set; }
    
    public Worker(ILogger logger, IBrainUnit<dynamic> negotiatorBrainUnit, IBrainUnit<dynamic> baasBrainUnit)
    {
        _logger = logger;
        _negotiatorBrainUnit = negotiatorBrainUnit;
        _baasBrainUnit = baasBrainUnit;
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await Run();
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    private async Task Run()
    {
        //Console.Title = nameof("put here the brain unit name");
        
        _logger.Info("Start configuring the brain...");
        try
        {
            await Task.WhenAll(_negotiatorBrainUnit.StartAsync(), _baasBrainUnit.StartAsync());
            IsServiceRunning = true;
            ErrorMessage = string.Empty;
        }
        catch (Exception e)
        {
            IsServiceRunning = false;
            ErrorMessage = e.GetBaseException().Message;
            _logger.Fatal(e, e.GetBaseException().Message);
        }
    }

    public async Task<bool> CheckStatusAsync()
    {
        return CheckStatus();
    }

    public bool CheckStatus()
    {
        return IsServiceRunning;
    }
}