using System;
using System.Collections.Generic;
using Core.Metrics.Internals;
using FluentAssertions;
using NUnit.Framework;
using Prometheus.DotNetRuntime;

namespace Core.Metrics.Tests
{
    [TestFixture]
    public class RuntimeMetricsEnvironmentParserTests
    {
        private RuntimeMetricsEnvironmentParser _parser;


        [SetUp]
        public void SetUp()
        {
            _parser = new RuntimeMetricsEnvironmentParser();
        }

        [TearDown]
        public void TearDown()
        {
            Environment.SetEnvironmentVariable(MetricsConfiguration.RUNTIME_PROM_METRICS, null);
            Environment.SetEnvironmentVariable(MetricsConfiguration.ENABLE_PROM_METRICS, null);
        }
        
        [Test]
        public void ParseRuntimeMetrics_ShouldReturnEmpty()
        {
            var runtimeMetrics = _parser.ParseRuntimeMetrics(MetricsConfiguration.RUNTIME_PROM_METRICS);
            runtimeMetrics.Should().BeEmpty();
        }
        
        [Test]
        [TestCaseSource(nameof(ParseRuntimeMetricsTestCaseSource))]
        public void ParseRuntimeMetrics_ShouldReturnParsedMetrics(string envVarValue, object[] expectedMetricsObject)
        {
            var expectedRuntimeMetrics = (RuntimeMetric[]) expectedMetricsObject;
            Environment.SetEnvironmentVariable(MetricsConfiguration.RUNTIME_PROM_METRICS, envVarValue);
            
            var runtimeMetrics = _parser.ParseRuntimeMetrics(MetricsConfiguration.RUNTIME_PROM_METRICS);
            
            runtimeMetrics.Should().BeEquivalentTo(expectedRuntimeMetrics);
        }

        [Test]
        [TestCaseSource(nameof(ParseMetricsEnvironmentVariableTestCaseSource))]
        public void ParseMetricsEnvironmentVariable_ShouldReturnCorrectValue(string envVarValue, bool expectedResult)
        {
            Environment.SetEnvironmentVariable(MetricsConfiguration.ENABLE_PROM_METRICS, envVarValue);

            var result = _parser.ParseMetricsEnvironmentVariable(MetricsConfiguration.ENABLE_PROM_METRICS);

            result.Should().Be(expectedResult);
        }

        private static IEnumerable<object> ParseMetricsEnvironmentVariableTestCaseSource()
        {
            yield return new TestCaseData("", false);
            yield return new TestCaseData("0", false);
            yield return new TestCaseData("false", false);
            yield return new TestCaseData("False", false);
            yield return new TestCaseData("FALSE", false);
            yield return new TestCaseData("true", true);
            yield return new TestCaseData("True", true);
            yield return new TestCaseData("TRUE", true);
            yield return new TestCaseData("1", true);
            yield return new TestCaseData("-1", false);
            yield return new TestCaseData("sdlkfjsdlkfjwe;l kfj", false);
        }

        private static IEnumerable<object> ParseRuntimeMetricsTestCaseSource()
        {
            yield return new TestCaseData("GC;ThreadPool", new[]
            {
                new RuntimeMetric("gc", CaptureLevel.Counters),
                new RuntimeMetric("threadpool", CaptureLevel.Counters)
            });
            
            yield return new TestCaseData("GC|I;ThreadPool|I", new[]
            {
                new RuntimeMetric("gc", CaptureLevel.Informational),
                new RuntimeMetric("threadpool", CaptureLevel.Informational)
            });
            
            yield return new TestCaseData("GC|I;ThreadPool|V", new[]
            {
                new RuntimeMetric("gc", CaptureLevel.Informational),
                new RuntimeMetric("threadpool", CaptureLevel.Verbose)
            });
            
            yield return new TestCaseData("GC;ThreadPool|V", new[]
            {
                new RuntimeMetric("gc", CaptureLevel.Counters),
                new RuntimeMetric("threadpool", CaptureLevel.Verbose)
            });
            
            yield return new TestCaseData("GC|i;ThreadPool|i", new[]
            {
                new RuntimeMetric("gc", CaptureLevel.Informational),
                new RuntimeMetric("threadpool", CaptureLevel.Informational)
            });
            
            yield return new TestCaseData("GC|I;ThreadPool|I;SomeStrangethere|fwef|fwef", new[]
            {
                new RuntimeMetric("gc", CaptureLevel.Informational),
                new RuntimeMetric("threadpool", CaptureLevel.Informational)
            });
            
            yield return new TestCaseData("SomeStrangethere|fwef|fwef;;;;;;sdfewf", new RuntimeMetric[] { });
        }
    }
}