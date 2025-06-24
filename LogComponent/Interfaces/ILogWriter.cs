namespace LogComponent.Interfaces
{
    public interface ILogWriter : IDisposable
    {
        Task WriteAsync(LogLine line, CancellationToken cancellationToken);
    }
}
