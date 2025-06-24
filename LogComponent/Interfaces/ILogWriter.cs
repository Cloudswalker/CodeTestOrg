namespace LogComponent.Interfaces
{
    internal interface ILogWriter : IDisposable
    {
        Task WriteAsync(LogLine line, CancellationToken cancellationToken);
    }
}
