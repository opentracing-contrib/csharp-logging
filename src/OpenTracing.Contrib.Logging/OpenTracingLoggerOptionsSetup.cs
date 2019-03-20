using Microsoft.Extensions.Logging.Configuration;
using Microsoft.Extensions.Options;

namespace OpenTracing.Contrib.Logging
{
    /// <summary>
    /// Loads settings for <see cref="OpenTracingLoggerProvider"/> into <see cref="OpenTracingLoggerOptions"/> type.
    /// </summary>
    /// <remarks>
    /// TODO: Cleanup, based on https://github.com/aspnet/Extensions/tree/3.0.0/src/Logging/Logging.Console/src once .NET Core 3.0 is released
    /// </remarks>
    internal class OpenTracingLoggerOptionsSetup : ConfigureFromConfigurationOptions<OpenTracingLoggerOptions>
    {
        public OpenTracingLoggerOptionsSetup(ILoggerProviderConfiguration<OpenTracingLoggerProvider> providerConfiguration)
            : base(providerConfiguration.Configuration)
        {
        }
    }
}