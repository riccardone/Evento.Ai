using System.Collections.Concurrent;
using System.Text.Json;
using NLog;

namespace Queuesto;

public class BeaconQueue<T>
{
    private static readonly ILogger Logger = LogManager.GetCurrentClassLogger();

    private readonly string _queueDirectory;
    private readonly string _deadLetterDirectory;
    private readonly ConcurrentQueue<MessageWrapper<T>> _queue = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly int _maxRetryAttempts;
    private readonly Action<MessageWrapper<T>, int> _messageProcessor;
    private readonly int _baseDelay;

    public BeaconQueue(string queueName, int maxRetryAttempts, Action<MessageWrapper<T>, int> messageProcessor, int baseDelay = 100)
    {
        _queueDirectory = queueName;
        _deadLetterDirectory = $"{queueName}-deadletter";
        _maxRetryAttempts = maxRetryAttempts;
        _messageProcessor = messageProcessor;
        _baseDelay = baseDelay;

        Directory.CreateDirectory(_queueDirectory);
        Directory.CreateDirectory(_deadLetterDirectory);

        LoadQueueFromDisk();
        StartProcessingQueue();
    }

    private void LoadQueueFromDisk()
    {
        try
        {
            foreach (var file in Directory.GetFiles(_queueDirectory))
            {
                var content = File.ReadAllText(file);
                var message = DeserializeMessage(content);
                _queue.Enqueue(message);
            }

            Logger.Info($"Loaded {Directory.GetFiles(_queueDirectory).Length} messages from disk.");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to load queue from disk.");
        }
    }

    private void SaveMessageToDisk(MessageWrapper<T> message)
    {
        try
        {
            var filePath = Path.Combine(_queueDirectory, Guid.NewGuid() + ".msg");
            File.WriteAllText(filePath, SerializeMessage(message));
            Logger.Debug($"Message saved to disk: {filePath}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to save message to disk.");
        }
    }

    private void MoveToDeadLetterQueue(MessageWrapper<T> message)
    {
        try
        {
            var filePath = Path.Combine(_deadLetterDirectory, Guid.NewGuid() + ".msg");
            File.WriteAllText(filePath, SerializeMessage(message));
            Logger.Warn($"Message moved to dead letter queue: {filePath}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to move message to dead letter queue.");
        }
    }

    private string SerializeMessage(MessageWrapper<T> message) => JsonSerializer.Serialize(message);

    private MessageWrapper<T> DeserializeMessage(string content) => JsonSerializer.Deserialize<MessageWrapper<T>>(content);

    public void Enqueue(MessageWrapper<T> message)
    {
        try
        {
            _queue.Enqueue(message);
            SaveMessageToDisk(message);
            Logger.Info($"Message enqueued: {message.Id}");
        }
        catch (Exception ex)
        {
            Logger.Error(ex, "Failed to enqueue message.");
        }
    }

    private void StartProcessingQueue()
    {
        Task.Run(async () =>
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (_queue.TryDequeue(out var message))
                {
                    ProcessMessage(message);
                }
                else
                {
                    int waitTime = CalculateAdaptiveDelay();
                    await Task.Delay(waitTime, _cancellationTokenSource.Token);
                }
            }
        }, _cancellationTokenSource.Token);
    }

    private int CalculateAdaptiveDelay()
    {
        int queueSize = _queue.Count;

        if (queueSize > 100)
        {
            return _baseDelay / 10; // High traffic, shorter delay
        }
        else if (queueSize > 0)
        {
            return _baseDelay / 2; // Moderate traffic
        }
        else
        {
            return _baseDelay; // Low traffic, standard delay
        }
    }

    private void ProcessMessage(MessageWrapper<T> message)
    {
        var attempts = 0;
        while (attempts < _maxRetryAttempts)
        {
            try
            {
                _messageProcessor(message, attempts + 1);
                Logger.Info($"Message processed successfully: {message.Id}");
                return;
            }
            catch (Exception ex)
            {
                attempts++;
                Logger.Warn(ex, $"Processing failed for message: {message.Id}, Attempt: {attempts}");

                if (attempts >= _maxRetryAttempts)
                {
                    MoveToDeadLetterQueue(message);
                    Logger.Error($"Message moved to dead letter queue after {attempts} attempts: {message.Id}");
                }
            }
        }
    }

    public void Stop()
    {
        Logger.Info("Stopping the queue service...");
        _cancellationTokenSource.Cancel();
    }

    public void RetryDeadLetterMessages()
    {
        var deadLetterFiles = Directory.GetFiles(_deadLetterDirectory);

        foreach (var file in deadLetterFiles)
        {
            try
            {
                var content = File.ReadAllText(file);
                var message = DeserializeMessage(content);

                Enqueue(message);

                File.Delete(file);

                Logger.Info($"Retried message from dead letter queue: {file}");
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"Failed to retry message from dead letter queue file: {file}");
            }
        }
    }
}

//class Program
//{
//    static void Main()
//    {
//        var queueService = new BeaconQueue<string>(
//            queueDirectory: "QueueStorage",
//            deadLetterDirectory: "DeadLetterStorage",
//            maxRetryAttempts: 3,
//            messageProcessor: (message, attempt) =>
//            {
//                Console.WriteLine($"Processing message: {message} (Attempt {attempt})");
//                if (message.Contains("fail")) throw new Exception("Processing failed!");
//            });

//        queueService.Enqueue("Message 1");
//        queueService.Enqueue("Message fail"); // This will fail and move to dead-letter queue
//        queueService.Enqueue("Message 2");

//        Console.WriteLine("Press Enter to retry dead-letter messages...");
//        Console.ReadLine();
//        queueService.RetryDeadLetterMessages();

//        Console.WriteLine("Press Enter to stop the service...");
//        Console.ReadLine();
//        queueService.Stop();
//    }
//}
