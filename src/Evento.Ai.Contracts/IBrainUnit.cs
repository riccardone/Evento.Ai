namespace Evento.Ai.Contracts;

public interface IBrainUnit<T>
{
    Task StartAsync();
    Task StopAsync();
}