using System;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions.Internal;
using Microsoft.Extensions.Logging.Internal;
using OpenTracing.Tag;

namespace OpenTracing.Contrib.Logging
{
    /// <remarks>
    /// TODO: Cleanup, based on https://github.com/aspnet/Extensions/tree/3.0.0/src/Logging/Logging.Console/src once .NET Core 3.0 is released
    /// </remarks>
    internal class OpenTracingLogger : ILogger
    {
        private const string LOG_PREFIX = "log.";
        private const string LOGGER = "logger";

        private readonly string _name;
        private readonly Func<string, LogLevel, bool> _filter;

        private ISpan ActiveSpan => Options?.ScopeManager?.Active?.Span;

        internal OpenTracingLogger(string name, Func<string, LogLevel, bool> filter)
        {
            _name = name ?? throw new ArgumentNullException(nameof(name));
            _filter = filter ?? ((category, logLevel) => true);
        }

        internal OpenTracingLoggerOptions Options { get; set; }

        /// <inheritdoc />
        public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
        {
            if (!IsEnabled(logLevel)) return;

            if (formatter == null)
            {
                throw new ArgumentNullException(nameof(formatter));
            }

            var span = ActiveSpan;
            if (exception != null)
            {
                span = span.SetTag(Tags.Error, true)
                    .Log(new Dictionary<string, object>(5)
                    {
                        {LogFields.Event, Tags.Error.Key},
                        {LogFields.ErrorObject, exception},
                        {LogFields.ErrorKind, exception.GetType().Name},
                        {LogFields.Message, exception.Message},
                        {LogFields.Stack, exception.StackTrace}
                    });
            }

            var message = formatter(state, exception);
            if (string.IsNullOrEmpty(message)) return;

            span.Log(GetLogEntries(logLevel, state, message));
        }

        private Dictionary<string, object> GetLogEntries<TState>(LogLevel logLevel, TState state, string message)
        {
            var entries = new Dictionary<string, object>
            {
                {LogFields.Event, logLevel},
                {LogFields.Message, message}
            };

            if (Options.IncludeLoggerName)
            {
                entries.Add(OpenTracingLogger.LOGGER, _name);
            }

            if (Options.IncludeKeyValuePairs && state is FormattedLogValues keyValuePair)
            {
                foreach (var logValue in keyValuePair)
                {
                    var key = logValue.Key;
                    if (string.Equals(key, "{OriginalFormat}", StringComparison.InvariantCulture)) continue;

                    entries.Add(OpenTracingLogger.LOG_PREFIX + key, logValue.Value);
                }
            }

            return entries;
        }

        public bool IsEnabled(LogLevel logLevel)
        {
            return logLevel != LogLevel.None && ActiveSpan != null && _filter(_name, logLevel);
        }

        /// <inheritdoc />
        public IDisposable BeginScope<TState>(TState state)
        {
            return NullScope.Instance;
        }
    }
}
