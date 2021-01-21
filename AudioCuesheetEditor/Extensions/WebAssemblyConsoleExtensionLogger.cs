﻿using Microsoft.Extensions.Logging;
using Microsoft.JSInterop;
using Microsoft.JSInterop.WebAssembly;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AudioCuesheetEditor.Extensions
{
    //Taken from https://github.com/dotnet/aspnetcore/blob/master/src/Components/WebAssembly/WebAssembly/src/Services/WebAssemblyConsoleLogger.cs
    public class WebAssemblyConsoleExtensionLogger<T> : ILogger<T>, ILogger
    {
        private static readonly string _loglevelPadding = ": ";
        private static readonly string _messagePadding;
        private static readonly string _newLineWithMessagePadding;
        private static readonly StringBuilder _logBuilder = new StringBuilder();

        private readonly string _name;

        public event EventHandler<KeyValuePair<String,String>> MessageLogged;

        static WebAssemblyConsoleExtensionLogger()
        {
            var logLevelString = GetLogLevelString(LogLevel.Information);
            _messagePadding = new string(' ', logLevelString.Length + _loglevelPadding.Length);
            _newLineWithMessagePadding = Environment.NewLine + _messagePadding;
        }

        public WebAssemblyConsoleExtensionLogger(string name)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
        }

        public IDisposable BeginScope<TState>(TState state)
        {
            return NoOpDisposable.Instance;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None;
        }

        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter)
        {
            if (!IsEnabled(logLevel))
            {
                return;
            }

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var message = formatter(state, exception);

            if (!string.IsNullOrEmpty(message) || exception != null)
            {
                WriteMessage(logLevel, _name, eventId.Id, message, exception);
            }
        }

        private void WriteMessage(LogLevel logLevel, string logName, int eventId, string message, Exception? exception)
        {
            lock (_logBuilder)
            {
                try
                {
                    CreateDefaultLogMessage(_logBuilder, logLevel, logName, eventId, message, exception);
                    var formattedMessage = _logBuilder.ToString();

                    MessageLogged?.Invoke(this, new KeyValuePair<string, string>(_name, formattedMessage));
                }
                finally
                {
                    _logBuilder.Clear();
                }
            }
        }

        private void CreateDefaultLogMessage(StringBuilder logBuilder, LogLevel logLevel, string logName, int eventId, string message, Exception? exception)
        {
            logBuilder.Append(GetLogLevelString(logLevel));
            logBuilder.Append(_loglevelPadding);
            logBuilder.Append(logName);
            logBuilder.Append("[");
            logBuilder.Append(eventId);
            logBuilder.Append("]");

            if (!string.IsNullOrEmpty(message))
            {
                // message
                logBuilder.AppendLine();
                logBuilder.Append(_messagePadding);

                var len = logBuilder.Length;
                logBuilder.Append(message);
                logBuilder.Replace(Environment.NewLine, _newLineWithMessagePadding, len, message.Length);
            }

            // Example:
            // System.InvalidOperationException
            //    at Namespace.Class.Function() in File:line X
            if (exception != null)
            {
                // exception message
                logBuilder.AppendLine();
                logBuilder.Append(exception.ToString());
            }
        }

        private static string GetLogLevelString(LogLevel logLevel)
        {
            switch (logLevel)
            {
                case LogLevel.Trace:
                    return "trce";
                case LogLevel.Debug:
                    return "dbug";
                case LogLevel.Information:
                    return "info";
                case LogLevel.Warning:
                    return "warn";
                case LogLevel.Error:
                    return "fail";
                case LogLevel.Critical:
                    return "crit";
                default:
                    throw new ArgumentOutOfRangeException(nameof(logLevel));
            }
        }

        private class NoOpDisposable : IDisposable
        {
            public static NoOpDisposable Instance = new NoOpDisposable();

            public void Dispose() { }
        }
    }
}
