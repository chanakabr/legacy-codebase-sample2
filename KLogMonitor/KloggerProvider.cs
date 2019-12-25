using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace KLogMonitor
{
    public class KLoggerProvider : ILoggerProvider
    {
        private readonly ConcurrentDictionary<string, KLogger> _loggers = new ConcurrentDictionary<string, KLogger>();

        public ILogger CreateLogger(string categoryName)
        {
            return _loggers.GetOrAdd(categoryName, name => new KLogger(name));
        }

        public void Dispose()
        {
            _loggers.Clear();
        }
    }

}
