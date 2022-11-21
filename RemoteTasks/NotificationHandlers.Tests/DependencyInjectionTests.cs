using ApiLogic.EPG;
using EpgNotificationHandler.Infrastructure;
using EventBus.Abstraction;
using FluentAssertions;
using IotGrpcClientWrapper;
using LineupNotificationHandler.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;
using OTT.Service.CloudfrontInvalidator;

namespace NotificationHandlers.Tests
{
    public class DependencyInjectionTests
    {
        [Test]
        public void EpgNotificationHandler_IsResolved()
        {
            var serviceProvider = new ServiceCollection()
                .AddEpgNotificationHandlerDependencies()
                .AddScoped<IEventContext>(p => new Mock<IEventContext>().Object)
                .AddScoped(typeof(EpgNotificationHandler.EpgNotificationHandler))
                .Replace(ServiceDescriptor.Singleton(new Mock<IEpgPartnerConfigurationManager>().Object)) // EpgV2PartnerConfigurationManager instance has dependency from CouchBase which requires configuration
                .Replace(ServiceDescriptor.Singleton(new Mock<IIotClient>().Object)) // IotClient instance has dependency from GRPC which requires grpc endpoint
                .Replace(ServiceDescriptor.Singleton(new Mock<CloudfrontInvalidator.CloudfrontInvalidatorClient>().Object)) //has dependency from GRPC which requires grpc endpoint
                .BuildServiceProvider();

            var epgNotificationHandler = serviceProvider.GetService(typeof(EpgNotificationHandler.EpgNotificationHandler));

            epgNotificationHandler.Should().NotBeNull();
            epgNotificationHandler.GetType().Should().Be(typeof(EpgNotificationHandler.EpgNotificationHandler));
        }

        [Test]
        public void LineupNotificationHandler_IsResolved()
        {
            var serviceProvider = new ServiceCollection()
                .AddLineupNotificationHandlerDependencies()
                .AddScoped<IEventContext>(p => new Mock<IEventContext>().Object)
                .AddScoped(typeof(LineupNotificationHandler.LineupNotificationRequestedHandler))
                .Replace(ServiceDescriptor.Singleton(new Mock<IEpgPartnerConfigurationManager>().Object)) // EpgV2PartnerConfigurationManager instance has dependency from CouchBase which requires configuration
                .Replace(ServiceDescriptor.Singleton(new Mock<IIotClient>().Object)) // IotClient instance has dependency from GRPC which requires grpc endpoint
                .Replace(ServiceDescriptor.Singleton(new Mock<CloudfrontInvalidator.CloudfrontInvalidatorClient>().Object)) //has dependency from GRPC which requires grpc endpoint
                .BuildServiceProvider();

            var epgNotificationHandler = serviceProvider.GetService(typeof(LineupNotificationHandler.LineupNotificationRequestedHandler));

            epgNotificationHandler.Should().NotBeNull();
            epgNotificationHandler.GetType().Should().Be(typeof(LineupNotificationHandler.LineupNotificationRequestedHandler));
        }
    }
}