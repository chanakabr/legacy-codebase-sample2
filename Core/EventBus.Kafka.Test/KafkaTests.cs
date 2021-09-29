using CachingProvider.LayeredCache;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using EventBus.Abstraction;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ConfigurationManager;
using KLogMonitor;

namespace EventBus.Kafka.Test
{
    public class KafkaTests
    {
        private static readonly KLogger _log = new KLogger("KafkaTests");
        private KafkaPublisher _publisher;

        [SetUp]
        public void Setup()
        {
            // Tcm will not init twice and that way we make sure it is init from local file
            KLogger.InitLogger("log4net.config", KLogEnums.AppType.WS, "/var/log/kafka-tests/");
            TCMClient.Settings.Instance.Init(fromLocal: true);
            ApplicationConfiguration.Init();
            var kafkaConf = ApplicationConfiguration.Current.KafkaClientConfiguration;
            kafkaConf.BootstrapServers.Value = "localhost:9092";
            kafkaConf.SocketTimeoutMs.Value = (int)TimeSpan.FromSeconds(5).TotalMilliseconds;
           
            _publisher = KafkaPublisher.GetKafkaPublisher(kafkaConf, Partitioner.Consistent);
        }

        [Test]
        public void TestProducer()
        {
            var cacheInvalidationEvent = new ProductTestEvent(100, 100);
            _publisher.Publish(cacheInvalidationEvent);
            
            // sleep to allow publisher to publish async
            Thread.Sleep(TimeSpan.FromSeconds(2));
        }
        
        [Test]
        public void TestConsumer()
        {
            var consumedEvents = 0;
            Task.Run(()=>{
                var consumer = new EventBusConsumerKafka("TestConsumer", new List<string>{"ProductTestEvent"}, (result =>
                {
                    consumedEvents++;
                }));

                var cts = new CancellationTokenSource();
                consumer.StartConsumerAsync(cts.Token);
            });
            
            _log.Info($"sleeping for 5 sec to let consumer start");
            Thread.Sleep(5000);
            var testPartnerId = 999;
            var countOfEvents = 100;
            for (var i = 0; i < countOfEvents; i++)
            {
                var cacheInvalidationEvent = new ProductTestEvent(testPartnerId, i);
                _publisher.Publish(cacheInvalidationEvent);
            }
            
            _log.Info($"sleeping for 5 seconds to let consumer consume");
            Thread.Sleep(5000);
            Assert.That(countOfEvents, Is.EqualTo(consumedEvents));
        }
        
        [Test]
        public void TestBatchConsumer()
        {

            var batchSize = 50;
            var batchTimeout = 1000;
            var consumedBatches = 0;
            Task.Run(()=>{
                var consumer = new EventBusConsumerKafka("TestBatchConsumer", new List<string>{"ProductTestEvent"}, batchSize, batchTimeout, (results =>
                {
                    _log.Info($"consumed results.count{results.Count} at {DateTime.Now}");
                    Assert.That(results.Count, Is.EqualTo(50));
                    consumedBatches++;
                }));

                var cts = new CancellationTokenSource();
                consumer.StartConsumerAsync(cts.Token);
            });
            
            _log.Info($"sleeping for 5 sec to let consumer start");
            Thread.Sleep(5000);
            var testPartnerId = 999;
            var countOfEvents = 100;
            for (var i = 0; i < countOfEvents; i++)
            {
                var cacheInvalidationEvent = new ProductTestEvent(testPartnerId, i);
                _publisher.Publish(cacheInvalidationEvent);
            }
            
            _log.Info($"sleeping for 5 seconds to let consumer consume");
            Thread.Sleep(5000);
            Assert.That(countOfEvents/batchSize, Is.EqualTo(consumedBatches));
        }
        
        [Test]
        public void TestBatchConsumerWithSlowProducer()
        {

            var batchSize = 50;
            var batchTimeout = 1000;
            var consumedBatches = 0;
            Task.Run(()=>{
                var consumer = new EventBusConsumerKafka("TestBatchConsumer", new List<string>{"ProductTestEvent"}, batchSize, batchTimeout, (results =>
                {
                    _log.Info($"consumed results.count{results.Count} at {DateTime.Now}");
                    Assert.That(results.Count, Is.LessThan(50));
                    consumedBatches++;
                }));

                var cts = new CancellationTokenSource();
                consumer.StartConsumerAsync(cts.Token);
            });
            
            _log.Info($"sleeping for 5 sec to let consumer start");
            Thread.Sleep(5000);
            var testPartnerId = 999;
            var countOfEvents = 50;
            for (var i = 0; i < countOfEvents; i++)
            {
                // produce slower to avoid accumulating a full batch
                Thread.Sleep(100);
                var cacheInvalidationEvent = new ProductTestEvent(testPartnerId, i);
                _publisher.Publish(cacheInvalidationEvent);
            }
            
            _log.Info($"sleeping for 5 seconds to let consumer consume");
            Thread.Sleep(5000);
            Assert.That(consumedBatches, Is.AtLeast(5));
        }
        
        
    }
    
    

    [ServiceEventName("ProductTestEvent")]
    public class ProductTestEvent : ServiceEvent
    {
        public ProductTestEvent(int partnerId, long userId)
        {
            GroupId = partnerId;
            UserId = userId;
        }

        public override string EventKey => LayeredCacheKeys.GetUserInvalidationKey(GroupId, UserId.ToString());

    }

}