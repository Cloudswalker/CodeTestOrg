using LogComponent;
using LogComponent.Interfaces;
using System.Collections.Concurrent;

public class AsyncLog : ILog, IDisposable
{
    private readonly ILogWriter _logWriter;
    private readonly BlockingCollection<LogLine> _queue = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _processingTask;

    public AsyncLog()
    {
        _logWriter = new FileLogWriter("C:\\LogTest");
        _processingTask = Task.Run(ProcessQueueAsync);
    }

    public void Write(string text)
    {
        if (!_queue.IsAddingCompleted)
            _queue.Add(new LogLine { Text = text, Timestamp = DateTime.Now });
    }

    public void StopWithoutFlush()
    {
        _queue.CompleteAdding();
        _cts.Cancel();
        // don't wait for _processingTask
    }

    public void StopWithFlush()
    {
        _queue.CompleteAdding();
        _processingTask.Wait();
    }

    private async Task ProcessQueueAsync()
    {
        try
        {
            foreach (var logLine in _queue.GetConsumingEnumerable(_cts.Token))
            {
                await _logWriter.WriteAsync(logLine, _cts.Token);
            }
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
    }

    public void Dispose()
    {
        StopWithFlush();
        _logWriter?.Dispose();
        _cts.Dispose();
        _queue.Dispose();
    }
}
