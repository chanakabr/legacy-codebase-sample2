using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace KLogMonitor
{
    public class KLoggerProvider : ILoggerProvider
    {
        public IDictionary<string, ILogger> _LoggersRepo { get; set; }

        public KLoggerProvider()
        {
            _LoggersRepo = new Dictionary<string, ILogger>();
        }

        public ILogger CreateLogger(string categoryName)
        {
            if (_LoggersRepo.TryGetValue(categoryName, out var existingLogger))
            {
                return existingLogger;
            }
            else
            {
                var logger = new KLogger(categoryName);
                _LoggersRepo[categoryName] = logger;
                return logger;
            }
        }

        public void Dispose()
        {
        }
    }

}
