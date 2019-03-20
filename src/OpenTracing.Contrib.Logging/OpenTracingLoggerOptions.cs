using Microsoft.Extensions.Logging;

namespace OpenTracing.Contrib.Logging
{
    public class OpenTracingLoggerOptions
    {
        /// <summary>
        /// If set to true, a key named "logger" with the <see cref="ILogger"/>'s name will be added to every log entry.
        /// This increases the amount of logged information and should be kept false if different <see cref="ITracer"/> instances
        /// are used that already have distinct names.
        /// </summary>
        public bool IncludeLoggerName { get; set; }

        /// <summary>
        /// If set to true, each log entry will contain the key-value-pairs of <see cref="ILogger.Log{TState}"/> in addition to the formatted message.
        /// This increases the amount of logged information immensely and should be kept false if not intended to be filtered on.
        /// </summary>
        public bool IncludeKeyValuePairs { get; set; }

        /// <summary>
        /// The <see cref="IScopeManager"/> to use for logging. Each log entry will be written to the active span at <see cref="IScopeManager.Active"/>.
        /// If the scope has no active <see cref="ISpan"/>, logging will be skipped.
        /// </summary>
        public IScopeManager ScopeManager { get; set; }
    }
}