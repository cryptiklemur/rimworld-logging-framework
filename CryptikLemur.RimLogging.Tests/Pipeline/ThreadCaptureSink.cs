using System;
using CryptikLemur.RimLogging;

namespace CryptikLemur.RimLogging.Tests.Pipeline;

internal sealed class ThreadCaptureSink : ILogSink
{
    private readonly Action<int> _onWrite;

    public ThreadCaptureSink(Action<int> onWrite) => _onWrite = onWrite;

    public string Name => "ThreadCapture";
    public LogLevel MinLevel => LogLevel.Trace;
    public void Write(LogEntry entry) => _onWrite(Environment.CurrentManagedThreadId);
    public void Flush() { }
    public void Dispose() { }
}
