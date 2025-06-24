using LogComponent;

namespace LogUnitTests
{
    public class FaultyLogWriter : LogComponent.Interfaces.ILogWriter
    {
        public Task WriteAsync(LogLine line, CancellationToken cancellationToken)
        {
            throw new IOException("Simulated failure");
        }

        public void Dispose() { }
    }
}
