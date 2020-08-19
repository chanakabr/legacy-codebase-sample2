using Confluent.Kafka;
using Confluent.Kafka.Admin;
using EventBus.Models;
using NUnit.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EventBus.Kafka.Test
{
    public class Tests
    {

        private static readonly TopicSpecification[] _TestTopics = new TopicSpecification[]
        {
            new TopicSpecification{Name="test-topic-1",NumPartitions=1},
            new TopicSpecification{Name="test-topic-2",NumPartitions=1},
            new TopicSpecification{Name="test-topic-3",NumPartitions=1},
            new TopicSpecification{Name="test-topic-4",NumPartitions=1},
        };
        private KafkaPublisher _Publisher;
        private IAdminClient _AdminClient;

        [SetUp]
        public async Task Setup()
        {
            var config = new ProducerConfig{
                BootstrapServers = "localhost:9092",
                SocketTimeoutMs = (int)TimeSpan.FromSeconds(5).TotalMilliseconds,
                //MessageTimeoutMs = (int)TimeSpan.FromSeconds(5).TotalMilliseconds,
                //MetadataRequestTimeoutMs = (int)TimeSpan.FromSeconds(5).TotalMilliseconds,
                //ApiVersionRequestTimeoutMs = (int)TimeSpan.FromSeconds(5).TotalMilliseconds,
                //RequestTimeoutMs = (int)TimeSpan.FromSeconds(5).TotalMilliseconds,
                //TransactionTimeoutMs= (int)TimeSpan.FromSeconds(5).TotalMilliseconds,
            };
            var producerFactory = new KafkaProducerFactory<string, string>(config);
            _Publisher = new EventBus.Kafka.KafkaPublisher(producerFactory);
            _AdminClient = new AdminClientBuilder(config).Build();

            TestContext.Out.WriteLine("Initilizing topics");
            var existingTopics = _AdminClient.GetMetadata(TimeSpan.FromSeconds(5)).Topics;
            var topictoCreate = _TestTopics.ToList();
            foreach (var topicMd in existingTopics)
            {
                topictoCreate.RemoveAll(t => t.Name == topicMd.Topic);
            }

            if (topictoCreate.Any())
            {

                await _AdminClient.CreateTopicsAsync(topictoCreate);
            }
            else
            {
                TestContext.Out.WriteLine("all test topic already exist skipping creation");
            }
        }

        [Test]
        public void TestProducer()
        {
            var ottUserInvalidationEvent = new OTTUserInvalidationEvent(100);
            _Publisher.Publish(ottUserInvalidationEvent);
        }
    }
}