using System;
using System.Collections.Concurrent;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace OpenTracing.Contrib.Logging
{
    /// <remarks>
    /// TODO: Cleanup, based on https://github.com/aspnet/Extensions/tree/3.0.0/src/Logging/Logging.Console/src once .NET Core 3.0 is released
    /// </remarks>
    [ProviderAlias("OpenTracing")]
    public class OpenTracingLoggerProvider : ILoggerProvider
    {
        private readonly Func<string, LogLevel, bool> _filter;
        private readonly IOptionsMonitor<OpenTracingLoggerOptions> _optionsMonitor;
        private readonly ConcurrentDictionary<string, OpenTracingLogger> _loggers;

        private static readonly Func<string, LogLevel, bool> TrueFilter = (cat, level) => true;

        private OpenTracingLoggerOptions _options;
        private IDisposable _optionsReloadToken;

        public OpenTracingLoggerProvider(IOptionsMonitor<OpenTracingLoggerOptions> optionsMonitor)
        {
            _filter = OpenTracingLoggerProvider.TrueFilter;
            _optionsMonitor = optionsMonitor;
            _loggers = new ConcurrentDictionary<string, OpenTracingLogger>();

            ReloadLoggerOptions(optionsMonitor.CurrentValue);
        }

        [Obsolete("This method is obsolete and will be removed in a future version. The recommended alternative is OpenTracingLoggerProvider(IOptionsMonitor<OpenTracingLoggerOptions> optionsMonitor).")]
        public OpenTracingLoggerProvider(Func<string, LogLevel, bool> filter, OpenTracingLoggerOptions options)
        {
            _filter = filter;
            _options = options;
            _loggers = new ConcurrentDictionary<string, OpenTracingLogger>();
        }

        private void ReloadLoggerOptions(OpenTracingLoggerOptions options)
        {
            foreach (var logger in _loggers)
            {
                logger.Value.Options = options;
            }

            _options = options;
            _optionsReloadToken = _optionsMonitor.OnChange(ReloadLoggerOptions);
        }

        /// <inheritdoc />
        public ILogger CreateLogger(string name)
        {
            return _loggers.GetOrAdd(name, loggerName => new OpenTracingLogger(name, _filter)
            {
                Options = _options
            });
        }

        /// <inheritdoc />
        public void Dispose()
        {
            _optionsReloadToken?.Dispose();
        }
    }
}
