﻿using Xunit.Abstractions;
using Microsoft.Extensions.Logging;
namespace GameMarketAPIServer.Utilities
{
    public class Interfaces
    {
    }

    public interface APITracker
    {
        bool canRequest();

    }

    public sealed class XUnitLogger<T> : Microsoft.Extensions.Logging.ILogger<T>
    {
        private readonly ITestOutputHelper _output;
        private readonly string categoryName;

        public XUnitLogger(ITestOutputHelper output)
        {
            _output = output;
            categoryName = typeof(T).FullName;
        }

        public IDisposable? BeginScope<TState>(TState state) where TState : notnull => default;

        public bool IsEnabled(LogLevel logLevel) => true;

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel)) { return; }
            if (formatter != null)
            {
                _output.WriteLine(formatter(state, exception));
            }
        }
        private class NoOpDisposable:IDisposable
        {
            public static readonly NoOpDisposable Instance = new NoOpDisposable();
            public void Dispose() { }
        }
    }
    public sealed class XUnitLoggerProvider : ILoggerProvider
    {
        private readonly ITestOutputHelper _output;

        public XUnitLoggerProvider(ITestOutputHelper output)
        {
            _output = output;
        }

        public ILogger<T> CreateLogger<T>()
        {
            return new XUnitLogger<T>(_output);
        }
        public Microsoft.Extensions.Logging.ILogger CreateLogger(string categoryName)
        {
            Type loggerGenericType = typeof(XUnitLogger<>);
            Type loggerConstructedType = loggerGenericType.MakeGenericType(Type.GetType(categoryName));
            return (ILogger)Activator.CreateInstance(loggerConstructedType, _output);
        }

        public void Dispose() { }
    }
}
