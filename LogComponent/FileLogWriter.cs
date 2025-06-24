using LogComponent.Interfaces;
using System.Text;

namespace LogComponent
{
    public class FileLogWriter : ILogWriter
    {
        private StreamWriter _writer;
        private DateOnly _currentDate;
        private readonly string _directory;
        private readonly Func<DateTime> _getNow;

        public FileLogWriter(string directory, Func<DateTime>? getNow = null)
        {
            _directory = directory;
            _getNow = getNow ?? (() => DateTime.Now);

            if (!Directory.Exists(_directory))
                Directory.CreateDirectory(_directory);

            _currentDate = DateOnly.FromDateTime(_getNow());
            _writer = CreateWriter(_currentDate);
        }

        public async Task WriteAsync(LogLine line, CancellationToken cancellationToken)
        {
            var logDate = DateOnly.FromDateTime(line.Timestamp);

            if (logDate != _currentDate)
            {
                _writer.Dispose();
                _currentDate = logDate;
                _writer = CreateWriter(_currentDate);
            }

            var logText = FormatLogLine(line);

            try
            {
                await _writer.WriteLineAsync(logText.AsMemory(), cancellationToken);
                await _writer.FlushAsync();
            }
            catch
            {
                // swallow exception, don't crash
            }
        }

        private StreamWriter CreateWriter(DateOnly date)
        {
            string filename = $"Log_{Guid.NewGuid().GetHashCode()}_{date:yyyy-MM-dd}.log";
            string path = Path.Combine(_directory, filename);
            var stream = new FileStream(path, FileMode.Append, FileAccess.Write, FileShare.Read);
            var writer = new StreamWriter(stream, Encoding.UTF8);
            writer.WriteLine("Timestamp".PadRight(25) + '\t' + "Data".PadRight(15));
            return writer;
        }

        private string FormatLogLine(LogLine line)
        {
            StringBuilder sb = new StringBuilder();
            sb.Append(line.Timestamp.ToString("yyyy-MM-dd HH:mm:ss.fff"));
            sb.Append('\t');
            sb.Append(line.LineText());
            return sb.ToString();
        }
        
        public void Dispose()
        {
            _writer?.Dispose();
        }
    }
}
