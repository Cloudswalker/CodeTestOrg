using LogComponent;
using LogComponent.Interfaces;
using System.Collections.Concurrent;

public class AsyncLog : ILog
{
    private readonly ILogWriter _logWriter;
    private readonly BlockingCollection<LogLine> _queue = new();
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _processingTask;

    private bool _disposed = false;
    private readonly object _disposeLock = new();

    public AsyncLog() : this(new FileLogWriter(@"C:\LogTest")) { }

    public AsyncLog(ILogWriter logWriter)
    {
        _logWriter = logWriter;
        _processingTask = Task.Run(ProcessQueueAsync);
    }

    public void Write(string text)
    {
        if (!_disposed && !_cts.IsCancellationRequested && !_queue.IsAddingCompleted)
            _queue.Add(new LogLine(DateTime.Now, text));
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
        lock (_disposeLock)
        {
            if (_disposed)
                return;

            _disposed = true;

            try
            {
                StopWithFlush();
                _cts.Cancel();

                // wait for background task to finish
                _processingTask?.Wait(1000);
            }
            catch { /* swallow */ }
        }

        _logWriter?.Dispose();
        _cts.Dispose();
        _queue.Dispose();
    }
}
