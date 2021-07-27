using System;
using System.Collections.Generic;
using ApiObjects.CanaryDeployment.Elasticsearch;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using CouchbaseManager;
using Moq;
using NUnit.Framework;
using OfficeOpenXml.FormulaParsing.Excel.Functions.Text;

namespace CanaryDeploymentManager.Tests
{
    
    
    
    public class Tests
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void ConfigManager_ShouldReturnError_IfConfigNotSetForPartner()
        {
            var cbMock = new Mock<ICouchbaseManager>();
            cbMock.Setup(x => x.IsKeyExists(It.IsAny<string>())).Returns(false);

            var cacheMock = new MockLayeredCache();
            
            var sut = new ElasticsearchCanaryDeploymentManager(cacheMock, cbMock.Object);

            var res = sut.GetPartnerConfiguration(999);
            
            Assert.That(res.Status, Is.Not.Null);
            Assert.That(res.Status.Code, Is.EqualTo((int)eResponseStatus.GroupCanaryDeploymentConfigurationNotSetYet));
            
        }
        
        [Test]
        public void ConfigManager_ShouldReturnDefault_IfConfigNotSetForPartner()
        {
            var cbMock = new Mock<ICouchbaseManager>();
            cbMock.Setup(x => x.IsKeyExists(It.IsAny<string>())).Returns(false);

            var cacheMock = new MockLayeredCache();
            
            var sut = new ElasticsearchCanaryDeploymentManager(cacheMock, cbMock.Object);

            var migrationEventsStatus = sut.IsMigrationEventsEnabled(999);
            var esActiveVersion = sut.GetActiveElasticsearchActiveVersion(999);
            
            Assert.That(migrationEventsStatus, Is.False);
            Assert.That(esActiveVersion, Is.EqualTo(ElasticsearchVersion.ES_2_3));
        }
        
        [Test]
        public void ConfigManager_ShouldReturn_ConfigOfPartner()
        {
            const int PARTNER_ID = 100;
            const string EXPECTED_CONFIG_KEY = "elasticsearch_canary_configuration_100";
            var expectedConfig = new ElasticsearchCanaryDeploymentConfiguration
            {
                ElasticsearchActiveVersion = ElasticsearchVersion.ES_7_13,
                EnableMigrationEvents = true
            };
            var cbMock = new Mock<ICouchbaseManager>();
            cbMock.Setup(x => x.IsKeyExists(It.Is<string>(s=>s == EXPECTED_CONFIG_KEY))).Returns(true);
            cbMock.Setup(x => x.IsKeyExists(It.Is<string>(s=> s != EXPECTED_CONFIG_KEY))).Returns(false);
            cbMock.Setup(x => 
                    x.Get<ElasticsearchCanaryDeploymentConfiguration>(
                        It.Is<string>(s => s == EXPECTED_CONFIG_KEY), 
                        It.IsAny<bool>()))
                .Returns(expectedConfig);


            var cacheMock = new MockLayeredCache();
            
            var sut = new ElasticsearchCanaryDeploymentManager(cacheMock, cbMock.Object);

            var migrationEventsStatus = sut.IsMigrationEventsEnabled(PARTNER_ID);
            var esActiveVersion = sut.GetActiveElasticsearchActiveVersion(PARTNER_ID);
            
            Assert.That(migrationEventsStatus, Is.EqualTo(expectedConfig.EnableMigrationEvents));
            Assert.That(esActiveVersion, Is.EqualTo(expectedConfig.ElasticsearchActiveVersion));
            
            var migrationEventsStatusForDefaultPartner = sut.IsMigrationEventsEnabled(0);
            var esActiveVersionForDefaultPartner = sut.GetActiveElasticsearchActiveVersion(0);
            Assert.That(migrationEventsStatusForDefaultPartner, Is.False);
            Assert.That(esActiveVersionForDefaultPartner, Is.EqualTo(ElasticsearchVersion.ES_2_3));
            
        }
    }
}