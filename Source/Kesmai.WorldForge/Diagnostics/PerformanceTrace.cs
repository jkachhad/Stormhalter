using System;
using System.Diagnostics;

namespace Kesmai.WorldForge.Diagnostics;

internal static class PerformanceTrace
{
    [Conditional("DEBUG")]
    public static void Write(string operation, TimeSpan elapsed, string details = null)
    {
        Debug.WriteLine(
            $"[WorldForge Timing] {operation}: {elapsed.TotalMilliseconds:N1} ms" +
            (String.IsNullOrWhiteSpace(details) ? String.Empty : $" ({details})"));
    }

    public static IDisposable Measure(string operation, Func<string> details = null)
    {
#if DEBUG
        return new Measurement(operation, details);
#else
        return EmptyMeasurement.Instance;
#endif
    }

#if DEBUG
    private sealed class Measurement : IDisposable
    {
        private readonly string _operation;
        private readonly Func<string> _details;
        private readonly Stopwatch _stopwatch = Stopwatch.StartNew();

        public Measurement(string operation, Func<string> details)
        {
            _operation = operation;
            _details = details;
        }

        public void Dispose()
        {
            _stopwatch.Stop();
            Write(_operation, _stopwatch.Elapsed, _details?.Invoke());
        }
    }
#endif

    private sealed class EmptyMeasurement : IDisposable
    {
        public static readonly EmptyMeasurement Instance = new();
        public void Dispose()
        {
        }
    }
}
