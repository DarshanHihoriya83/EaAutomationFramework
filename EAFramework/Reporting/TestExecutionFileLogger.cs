using System.Text;

namespace EAFramework.Reporting
{
    /// <summary>
    /// Append-only execution log for a single test artifact folder (thread-safe writer).
    /// </summary>
    public sealed class TestExecutionFileLogger : IDisposable
    {
        private readonly StreamWriter _writer;
        private readonly object _lock = new();

        public TestExecutionFileLogger(string logFilePath)
        {
            string? dir = Path.GetDirectoryName(logFilePath);

            if (!string.IsNullOrEmpty(dir))
            {
                Directory.CreateDirectory(dir);
            }

            _writer = new StreamWriter(
                new FileStream(
                    logFilePath,
                    FileMode.Append,
                    FileAccess.Write,
                    FileShare.Read),
                new UTF8Encoding(encoderShouldEmitUTF8Identifier: false))
            {
                AutoFlush = true
            };

            WriteLine($"===== Session start {DateTime.UtcNow:O} =====");
        }

        public void WriteLine(string message)
        {
            lock (_lock)
            {
                _writer.WriteLine($"[{DateTime.UtcNow:HH:mm:ss.fff}] {message}");
            }
        }

        public void Dispose()
        {
            WriteLine($"===== Session end {DateTime.UtcNow:O} =====");
            _writer.Dispose();
        }
    }
}
