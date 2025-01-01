using System.Collections.Concurrent;

namespace Queuesto;

public class BeaconQueue<T>
{
    private readonly string _queueDirectory;
    private readonly string _deadLetterDirectory;
    private readonly ConcurrentQueue<T> _queue = new();
    private readonly CancellationTokenSource _cancellationTokenSource = new();
    private readonly int _maxRetryAttempts;
    private readonly Action<T, int> _messageProcessor;

    public BeaconQueue(string queueName, int maxRetryAttempts, Action<T, int> messageProcessor)
    {
        _queueDirectory = queueName;
        _deadLetterDirectory = $"{queueName}-deadletter";
        _maxRetryAttempts = maxRetryAttempts;
        _messageProcessor = messageProcessor;

        Directory.CreateDirectory(_queueDirectory);
        Directory.CreateDirectory(_deadLetterDirectory);

        LoadQueueFromDisk();
        StartProcessingQueue();
    }

    private void LoadQueueFromDisk()
    {
        foreach (var file in Directory.GetFiles(_queueDirectory))
        {
            var content = File.ReadAllText(file);
            var message = DeserializeMessage(content);
            _queue.Enqueue(message);
        }
    }

    private void SaveMessageToDisk(T message)
    {
        var filePath = Path.Combine(_queueDirectory, Guid.NewGuid() + ".msg");
        File.WriteAllText(filePath, SerializeMessage(message));
    }

    private void MoveToDeadLetterQueue(T message)
    {
        var filePath = Path.Combine(_deadLetterDirectory, Guid.NewGuid() + ".msg");
        File.WriteAllText(filePath, SerializeMessage(message));
    }

    private string SerializeMessage(T message) => System.Text.Json.JsonSerializer.Serialize(message);

    private T DeserializeMessage(string content) => System.Text.Json.JsonSerializer.Deserialize<T>(content);

    public void Enqueue(T message)
    {
        _queue.Enqueue(message);
        SaveMessageToDisk(message);
    }

    private void StartProcessingQueue()
    {
        Task.Run(() =>
        {
            while (!_cancellationTokenSource.Token.IsCancellationRequested)
            {
                if (_queue.TryDequeue(out var message))
                {
                    ProcessMessage(message);
                }
                else
                {
                    Thread.Sleep(100); // Avoid busy-waiting
                }
            }
        }, _cancellationTokenSource.Token);
    }

    private void ProcessMessage(T message)
    {
        var attempts = 0;
        while (attempts < _maxRetryAttempts)
        {
            try
            {
                _messageProcessor(message, attempts + 1);
                return; // Processing succeeded
            }
            catch
            {
                attempts++;
                if (attempts >= _maxRetryAttempts)
                {
                    MoveToDeadLetterQueue(message);
                }
            }
        }
    }

    public void Stop()
    {
        _cancellationTokenSource.Cancel();
    }

    public void RetryDeadLetterMessages()
    {
        var deadLetterFiles = Directory.GetFiles(_deadLetterDirectory);

        foreach (var file in deadLetterFiles)
        {
            try
            {
                // Read the message from the dead-letter queue file
                var content = File.ReadAllText(file);
                var message = DeserializeMessage(content);

                // Enqueue the message back into the main queue
                Enqueue(message);

                // Delete the file from the dead-letter directory
                File.Delete(file);

                Console.WriteLine($"Retried message: {content}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Failed to retry message from {file}: {ex.Message}");
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
