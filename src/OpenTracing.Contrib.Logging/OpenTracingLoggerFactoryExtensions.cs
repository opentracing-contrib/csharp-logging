using System;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace OpenTracing.Contrib.Logging
{
    /// <remarks>
    /// TODO: Cleanup, based on https://github.com/aspnet/Extensions/tree/3.0.0/src/Logging/Logging.Console/src once .NET Core 3.0 is released
    /// </remarks>
    public static class OpenTracingLoggerExtensions
    {
        /// <summary>
        /// Adds a OpenTracing logger named 'OpenTracing' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        public static ILoggingBuilder AddOpenTracing(this ILoggingBuilder builder)
        {
            builder.AddConfiguration();

            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<ILoggerProvider, OpenTracingLoggerProvider>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IConfigureOptions<OpenTracingLoggerOptions>, OpenTracingLoggerOptionsSetup>());
            builder.Services.TryAddEnumerable(ServiceDescriptor.Singleton<IOptionsChangeTokenSource<OpenTracingLoggerOptions>, LoggerProviderOptionsChangeTokenSource<OpenTracingLoggerOptions, OpenTracingLoggerProvider>>());
            return builder;
        }

        /// <summary>
        /// Adds a OpenTracing logger named 'OpenTracing' to the factory.
        /// </summary>
        /// <param name="builder">The <see cref="ILoggingBuilder"/> to use.</param>
        /// <param name="configure"></param>
        public static ILoggingBuilder AddOpenTracing(this ILoggingBuilder builder, Action<OpenTracingLoggerOptions> configure)
        {
            if (configure == null)
            {
                throw new ArgumentNullException(nameof(configure));
            }

            builder.AddOpenTracing();
            builder.Services.Configure(configure);

            return builder;
        }


        /// <summary>
        /// Adds a OpenTracing logger that is enabled for <see cref="LogLevel"/>.Information or higher.
        /// </summary>
        /// <param name="factory">The extension method argument.</param>
        /// <param name="options">The options to apply to created <see cref="OpenTracingLogger"/>'s.</param>
        [Obsolete("This method is obsolete and will be removed in a future version. The recommended alternative is AddOpenTracing(this ILoggingBuilder builder, Action<OpenTracingLoggerOptions> configure).")]
        public static ILoggerFactory AddOpenTracing(this ILoggerFactory factory, OpenTracingLoggerOptions options = default)
        {
            return OpenTracingLoggerExtensions.AddOpenTracing(factory, LogLevel.Information, options);
        }

        /// <summary>
        /// Adds a OpenTracing logger that is enabled as defined by the filter function.
        /// </summary>
        /// <param name="factory">The extension method argument.</param>
        /// <param name="filter">The function used to filter events based on the log level.</param>
        /// <param name="options">The options to apply to created <see cref="OpenTracingLogger"/>'s.</param>
        [Obsolete("This method is obsolete and will be removed in a future version. The recommended alternative is AddOpenTracing(this ILoggingBuilder builder, Action<OpenTracingLoggerOptions> configure).")]
        public static ILoggerFactory AddOpenTracing(this ILoggerFactory factory, Func<string, LogLevel, bool> filter, OpenTracingLoggerOptions options = default)
        {
            factory.AddProvider(new OpenTracingLoggerProvider(filter, options));
            return factory;
        }

        /// <summary>
        /// Adds a OpenTracing logger that is enabled for <see cref="LogLevel"/>s of minLevel or higher.
        /// </summary>
        /// <param name="factory">The extension method argument.</param>
        /// <param name="minLevel">The minimum <see cref="LogLevel"/> to be logged</param>
        /// <param name="options">The options to apply to created <see cref="OpenTracingLogger"/>'s.</param>
        [Obsolete("This method is obsolete and will be removed in a future version. The recommended alternative is AddOpenTracing(this ILoggingBuilder builder, Action<OpenTracingLoggerOptions> configure).")]
        public static ILoggerFactory AddOpenTracing(this ILoggerFactory factory, LogLevel minLevel, OpenTracingLoggerOptions options = default)
        {
            return OpenTracingLoggerExtensions.AddOpenTracing(
                factory,
                (_, logLevel) => logLevel >= minLevel,
                options);
        }
    }
}
