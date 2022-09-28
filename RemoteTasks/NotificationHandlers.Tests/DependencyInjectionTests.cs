using ApiLogic.EPG;
using EpgNotificationHandler.Infrastructure;
using FluentAssertions;
using IotGrpcClientWrapper;
using LineupNotificationHandler.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Moq;
using NUnit.Framework;

namespace NotificationHandlers.Tests
{
    public class DependencyInjectionTests
    {
        [Test]
        public void EpgNotificationHandler_IsResolved()
        {
            var serviceProvider = new ServiceCollection()
                .AddEpgNotificationHandlerDependencies()
                .AddScoped(typeof(EpgNotificationHandler.EpgNotificationHandler))
                // EpgV2PartnerConfigurationManager instance has dependency from CouchBase which requires configuration.
                .Replace(ServiceDescriptor.Singleton(new Mock<IEpgPartnerConfigurationManager>().Object))
                // IotClient instance has dependency from GRPC which requires grpc endpoint.
                .Replace(ServiceDescriptor.Singleton(new Mock<IIotClient>().Object))
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
                .AddScoped(typeof(LineupNotificationHandler.LineupNotificationRequestedHandler))
                .Replace(ServiceDescriptor.Singleton(new Mock<IEpgPartnerConfigurationManager>().Object))
                // IotClient instance has dependency from GRPC which requires grpc endpoint.
                .Replace(ServiceDescriptor.Singleton(new Mock<IIotClient>().Object))
                .BuildServiceProvider();

            var epgNotificationHandler = serviceProvider.GetService(typeof(LineupNotificationHandler.LineupNotificationRequestedHandler));

            epgNotificationHandler.Should().NotBeNull();
            epgNotificationHandler.GetType().Should().Be(typeof(LineupNotificationHandler.LineupNotificationRequestedHandler));
        }
    }
}