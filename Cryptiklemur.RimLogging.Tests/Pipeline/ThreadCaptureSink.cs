using System;
using System.Threading;
using Cryptiklemur.RimLogging;

namespace Cryptiklemur.RimLogging.Tests.Pipeline;

internal sealed class ThreadCaptureSink : ILogSink
{
    private readonly Action<int> _onWrite;

    public ThreadCaptureSink(Action<int> onWrite) => _onWrite = onWrite;

    public string Name => "ThreadCapture";
    public LogLevel MinLevel => LogLevel.Trace;
    public void Write(LogEntry entry) => _onWrite(Thread.CurrentThread.ManagedThreadId);
    public void Flush() { }
    public void Dispose() { }
}
