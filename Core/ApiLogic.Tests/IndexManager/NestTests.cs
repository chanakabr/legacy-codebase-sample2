using System;
using Nest;
using NUnit.Framework;

namespace ApiLogic.Tests.IndexManager
{
    [TestFixture]
    public class NestTests
    {
        private ElasticClient _client;

        [SetUp]
        public void Setup()
        {
             _client = new ElasticClient(new Uri("http://localhost:9201"));
        }
        
        [Test]
        public void TestCreateIndex()
        {
            var tweet = new Tweet
            {
                Id = 1,
                User = "kimchy",
                PostDate = new DateTime(2009, 11, 15),
                Message = "Trying out NEST, so far so good?"
            };

            var response = _client.Index(tweet, idx => idx.Index("mytweetindex"));
        }

        public class Tweet
        {
            public int Id { get; set; }
            public string User { get; set; }
            public DateTime PostDate { get; set; }
            public string Message { get; set; }
        }
    }
}