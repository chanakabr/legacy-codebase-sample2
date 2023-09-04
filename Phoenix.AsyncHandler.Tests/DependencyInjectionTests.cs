using System;
using System.Collections.Generic;
using System.Linq;
using ApiLogic.EPG;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;
using NUnit.Framework;
using OTT.Lib.Kafka;
using OTT.Service.TaskScheduler.Extensions.TaskHandler;
using Phoenix.AsyncHandler.Couchbase;
using Phoenix.AsyncHandler.Kafka;
using Phoenix.AsyncHandler.Kronos;

namespace Phoenix.AsyncHandler.Tests
{
    public class DependencyInjectionTests
    {
        [Test]
        public void AddDependencies_AllHandlersResolved()
        {
            var serviceCollection = new ServiceCollection()
                .AddLogging()
                .AddDependencies()
                // EpgV2PartnerConfigurationManager instance has dependency from CouchBase which requires configuration.
                .Replace(ServiceDescriptor.Singleton(new Mock<IEpgPartnerConfigurationManager>().Object))
                .Replace(ServiceDescriptor.Singleton((Func<IServiceProvider, IKafkaProducerClientFactory>) (
                    provider => new KafkaProducerClientFactory(
                        new Dictionary<string, string>(),
                        provider.GetService<ILogger<IKafkaProducerClientFactory>>()))))
                .Replace(ServiceDescriptor.Singleton(new Mock<ICouchbaseWorker>().Object));

            var assembly = typeof(HouseholdNpvrAccountHandler).Assembly;
            var handlers = assembly.GetTypes()
                .Where(t => t.IsClass && !t.IsAbstract && !t.IsGenericType
                    && (IsSubclassOfRawGeneric(typeof(CrudHandler<>), t)
                        || typeof(IKronosTaskHandler).IsAssignableFrom(t)))
                .ToList();
            foreach (var handlerType in handlers)
            {
                serviceCollection.AddScoped(handlerType);
            }

            var provider = serviceCollection.BuildServiceProvider();
            foreach (var handlerType in handlers)
            {
                var handlerInstance = provider.GetService(handlerType);
                handlerInstance.Should().NotBeNull();
            }
        }

        private static bool IsSubclassOfRawGeneric(Type generic, Type toCheck) {
            while (toCheck != null && toCheck != typeof(object)) {
                var cur = toCheck.IsGenericType ? toCheck.GetGenericTypeDefinition() : toCheck;
                if (generic == cur) {
                    return true;
                }
                toCheck = toCheck.BaseType;
            }
            return false;
        }
    }
}
