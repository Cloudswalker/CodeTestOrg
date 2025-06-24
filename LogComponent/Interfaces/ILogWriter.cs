namespace LogComponent.Interfaces
{
    internal interface ILogWriter
    {
        Task WriteAsync(LogLine line, CancellationToken cancellationToken);
    }
}
