using System;
using System.Collections.Generic;

namespace Phoenix.AsyncHandler.Kronos
{
    public class KronosConfigurationProvider
    {
        private readonly Dictionary<string, Type> _taskNameToImplementation = new Dictionary<string, Type>();

        public KronosConfigurationProvider AddHandler<THandler>(string taskName) where THandler : IKronosTaskHandler
        {
            if (_taskNameToImplementation.ContainsKey(taskName))
            {
                throw new ArgumentException($"{taskName} already registered.");
            }

            _taskNameToImplementation[taskName] = typeof(THandler);

            return this;
        }

        public bool TryGetHandler(string taskName, out Type type) => _taskNameToImplementation.TryGetValue(taskName, out type);

        public IReadOnlyDictionary<string, Type> Handlers => _taskNameToImplementation;
    }
}