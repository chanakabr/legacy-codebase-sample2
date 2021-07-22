using System;
using ConfigurationManager;
using ConfigurationManager.ConfigurationSettings.ConfigurationBase;
using ConfigurationManager.Types;
using ElasticSearch.NEST;
using KLogMonitor;
using Moq;
using NUnit.Framework;

namespace ElasticSearch.Test
{
    public class NESTTests
    {
        public class TestDoc
        {
            public string Id { get; set; }
            public int Age { get; set; }
            public DateTime BirthDate { get; set; }
            
        }
        [SetUp]
        public void Setup()
        {
            KLogger.InitLogger("log4net.config", KLogEnums.AppType.WS, "/var/log/tests");
        }

        [Test]
        [Explicit("this tests is to run manually for testing logging")]
        public void TestLogging()
        {
            var config = new Mock<IApplicationConfiguration>();
            var esConfig = new ElasticSearchConfiguration();
            esConfig.URL_V7_13.Value = "http://localhost:9200";
            var esHttpConf = new ElasticSearchHttpClientConfiguration();
            config.SetupGet(c => c.ElasticSearchConfiguration).Returns(esConfig);
            config.SetupGet(c => c.ElasticSearchHttpClientConfiguration).Returns(esHttpConf);
            
            
            var esClient = NESTFactory.GetInstance(config.Object);
            _ = esClient.Cat.Indices(i=>i.Format("JSON"));

            for (int i = 0; i < 1000; i++)
            {
                var doc = new TestDoc()
                {
                    Id = $"{i}-id",
                    Age = i,
                    BirthDate = DateTime.Now.AddDays(i),
                };
                var r = esClient.Index(doc, d=>d.Index("test"));
            }

        }
    }
}