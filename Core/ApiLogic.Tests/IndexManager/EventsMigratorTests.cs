

using System;
using System.Diagnostics;
using System.Linq;
using ApiLogic.Catalog.IndexManager;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.Catalog.Response;
using EventBus.Abstraction;
using NUnit.Framework;
using Moq;

namespace ApiLogic.Tests.IndexManager
{
    [TestFixture]
    public class EventsMigratorTests
    {
        private MockRepository _mockRepository;
        private Mock<IIndexManager> _mockIManager;
        private Mock<IEventBusPublisher> _mockIEventBusPublisher;

        //[SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Loose);
            _mockIManager = _mockRepository.Create<IIndexManager>();
            _mockIEventBusPublisher = _mockRepository.Create<IEventBusPublisher>();
        }

        //[Test]                 
        public void TestCallingPublish()
        {
            var decorator = new IndexManagerEventsDecorator(_mockIManager.Object,
                _mockIEventBusPublisher.Object,
                IndexManagerVersion.EsV2,
                0);
            
            
            decorator.DeleteChannel(0);
            _mockIEventBusPublisher.Verify(x=>x.Publish(It.IsAny<ServiceEvent>()),Times.Once);
            _mockIManager.Verify(x=>x.DeleteChannel(0),Times.Once);
        }
        
    }
}