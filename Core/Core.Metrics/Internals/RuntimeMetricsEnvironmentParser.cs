using System;
using System.Collections.Generic;
using System.Linq;
using Prometheus.DotNetRuntime;

namespace Core.Metrics.Internals
{
    internal class RuntimeMetricsEnvironmentParser
    {
        private readonly string[] _possibleMetrics;
        
        public RuntimeMetricsEnvironmentParser()
        {
            _possibleMetrics = Enum.GetNames(typeof(DotNetMetric));
        }
        
        internal RuntimeMetric[] ParseRuntimeMetrics(string env)
        {
            var envVariable = Environment.GetEnvironmentVariable(env);
            if (string.IsNullOrEmpty(envVariable))
            {
                return new RuntimeMetric[] { };
            }
            
            var metrics = new List<RuntimeMetric>();
            var metricsSetup = envVariable.Split(';', StringSplitOptions.RemoveEmptyEntries);
            foreach (var metricSetup in metricsSetup)
            {
                if (TryExtractMetric(metricSetup, out var metric))
                {
                    metrics.Add(metric);
                }
            }

            return metrics.ToArray();
        }
        
        internal bool ParseMetricsEnvironmentVariable(string env)
        {
            var envVariable = Environment.GetEnvironmentVariable(env);
            if (!string.IsNullOrEmpty(envVariable))
            {
                if (envVariable == "1")
                {
                    return true;
                }
                
                if (bool.TryParse(envVariable, out var result))
                {
                    return result;
                }
            }

            return false;
        }
        
        private bool TryExtractMetric(string metricSetup, out RuntimeMetric metric)
        {
            var level = CaptureLevel.Counters;
            var metricDetails = metricSetup.Split('|', StringSplitOptions.RemoveEmptyEntries);
            if (!CheckIfMetricSupported(metricDetails[0]))
            {
                metric = null;
                return false;
            }
            
            if (metricDetails.Length != 1)
            {
                level = ExtractCaptureLevel(metricDetails[1]);
            }

            metric = new RuntimeMetric(metricDetails[0], level);
            return true;
        }

        private bool CheckIfMetricSupported(string metricName)
        {
            return _possibleMetrics.Any(possibleMetric => possibleMetric.Equals(metricName, StringComparison.InvariantCultureIgnoreCase));
        }

        private CaptureLevel ExtractCaptureLevel(string captureLevel)
        {
            switch (captureLevel)
            {
                case "I":
                case "i":
                    return CaptureLevel.Informational;
                case "V":
                case "v":
                    return CaptureLevel.Verbose;
                default:
                    return CaptureLevel.Counters;
            }
        }
    }
}