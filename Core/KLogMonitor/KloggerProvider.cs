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
        public ConcurrentDictionary<string, ILogger> _LoggersRepo = new ConcurrentDictionary<string, ILogger>();

        public ILogger CreateLogger(string categoryName)
        {
            return _LoggersRepo.GetOrAdd(categoryName, c=> new KLogger(c));
        }

        public void Dispose()
        {
            _LoggersRepo.Clear();
        }
    }

}
