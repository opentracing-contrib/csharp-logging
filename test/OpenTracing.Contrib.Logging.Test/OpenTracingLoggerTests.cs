using Microsoft.Extensions.Logging;
using OpenTracing.Mock;
using OpenTracing.Tag;
using System;
using System.Collections.Generic;
using Xunit;

namespace OpenTracing.Contrib.Logging.Test
{
    public class OpenTracingLoggerTests
    {
        private (MockTracer tracer, ILogger logger) SetupLogger(OpenTracingLoggerOptions options = null)
        {
            options = options ?? new OpenTracingLoggerOptions();

            var tracer = new MockTracer();
            options.ScopeManager = tracer.ScopeManager;

            var factory = new LoggerFactory().AddOpenTracing(options);
            var logger = factory.CreateLogger<OpenTracingLoggerTests>();
            return (tracer, logger);
        }

        [Fact]
        public void LogInformation_ForActiveSpan_ShouldCreateEntry()
        {
            var (tracer, logger) = SetupLogger();
            using (tracer.BuildSpan("foo").StartActive(true))
            {
                logger.LogInformation("test");
            }

            Assert.Collection(tracer.FinishedSpans(),
                span => Assert.Single(span.LogEntries));
        }

        [Fact]
        public void LogDebug_ForActiveSpan_ShouldNotCreateEntry()
        {
            var (tracer, logger) = SetupLogger();
            using (tracer.BuildSpan("foo").StartActive(true))
            {
                logger.LogDebug("test");
            }

            Assert.Collection(tracer.FinishedSpans(),
                span => Assert.Empty(span.LogEntries));
        }

        [Fact]
        public void LogInformation_ForInactiveSpan_ShouldNotCreateEntry()
        {
            var (tracer, logger) = SetupLogger();
            var span = (MockSpan)tracer.BuildSpan("foo").Start();
            logger.LogInformation("test");
            span.Finish();

            Assert.Empty(span.LogEntries);
        }

        [Fact]
        public void LogInformation_WithIncludeLoggerNameFalse_ShouldNotIncludeName()
        {
            var (tracer, logger) = SetupLogger(new OpenTracingLoggerOptions { IncludeLoggerName = false });
            using (tracer.BuildSpan("foo").StartActive(true))
            {
                logger.LogInformation("test");
            }

            Assert.Collection(tracer.FinishedSpans(),
                span => Assert.Collection(span.LogEntries,
                    entry => Assert.DoesNotContain("logger", entry.Fields)));
        }

        [Fact]
        public void LogInformation_WithIncludeLoggerNameTrue_ShouldIncludeName()
        {
            var (tracer, logger) = SetupLogger(new OpenTracingLoggerOptions { IncludeLoggerName = true });
            using (tracer.BuildSpan("foo").StartActive(true))
            {
                logger.LogInformation("test");
            }

            Assert.Collection(tracer.FinishedSpans(),
                span => Assert.Collection(span.LogEntries,
                    entry => Assert.Contains("logger", entry.Fields)));
        }

        [Fact]
        public void LogInformation_WithIncludeKeyValuePairsFalse_ShouldNotIncludePairs()
        {
            var (tracer, logger) = SetupLogger(new OpenTracingLoggerOptions { IncludeKeyValuePairs = false });
            using (tracer.BuildSpan("foo").StartActive(true))
            {
                logger.LogInformation("test {constant}", 1);
            }

            Assert.Collection(tracer.FinishedSpans(),
                span => Assert.Collection(span.LogEntries,
                    entry => Assert.DoesNotContain("log.constant", entry.Fields)));
        }

        [Fact]
        public void LogInformation_WithIncludeKeyValuePairsTrue_ShouldIncludePairs()
        {
            var (tracer, logger) = SetupLogger(new OpenTracingLoggerOptions { IncludeKeyValuePairs = true });
            using (tracer.BuildSpan("foo").StartActive(true))
            {
                logger.LogInformation("test {constant}", 1);
            }

            Assert.Collection(tracer.FinishedSpans(),
                span => Assert.Collection(span.LogEntries,
                    entry => Assert.Contains("log.constant", entry.Fields)));
        }

        [Fact]
        public void LogInformation_OfException_ShouldSetErrorTag()
        {
            var (tracer, logger) = SetupLogger();
            using (tracer.BuildSpan("foo").StartActive(true))
            {
                logger.LogInformation(new Exception("exception message"), "log message");
            }

            Assert.Collection(tracer.FinishedSpans(),
                span => Assert.Contains(Tags.Error.Key, (IReadOnlyDictionary<string, object>)span.Tags));
        }

        [Fact]
        public void LogInformation_OfException_ShouldCreateTwoLogEntries()
        {
            var (tracer, logger) = SetupLogger();
            using (tracer.BuildSpan("foo").StartActive(true))
            {
                logger.LogInformation(new Exception("exception message"), "log message");
            }

            Assert.Collection(tracer.FinishedSpans(),
                span => Assert.Collection(span.LogEntries,
                    entry => Assert.Equal(Tags.Error.Key, entry.Fields[LogFields.Event]),
                    entry => Assert.Equal(LogLevel.Information, entry.Fields[LogFields.Event])));
        }
    }
}
