using Microsoft.Extensions.Logging;
using System;

namespace MultiTargetLibrary.Common
{
    /// <summary>
    /// Null logger implementation for cases where no logger is provided
    /// </summary>
    internal class NullLogger<T> : ILogger<T>
    {
        public IDisposable BeginScope<TState>(TState state) where TState : notnull => NullDisposable.Instance;
        public bool IsEnabled(LogLevel logLevel) => false;
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }

        private class NullDisposable : IDisposable
        {
            public static readonly NullDisposable Instance = new();
            public void Dispose() { }
        }
    }
}
