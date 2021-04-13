using System;
using System.Collections.Generic;
using ApiObjects.BulkUpload;
using Core.Catalog;
using CouchbaseManager;
using DAL;
using FluentAssertions;
using NUnit.Framework;

namespace ApiLogic.Tests.BulkUpload.Compression
{
    /// <summary>
    /// In order to run those tests, specific section for csproj file should be added in order to consume application settings in NUnit tests.
    /// Resharper:
    /// <Target Name="CopyCustomContent" AfterTargets="AfterBuild">
    /// <Copy SourceFiles="app.config" DestinationFiles="$(OutDir)\ReSharperTestRunner64.dll.config" />
    /// </Target>
    /// Visual Studio:
    /// <Target Name="CopyCustomContent" AfterTargets="AfterBuild">
    /// <Copy SourceFiles="app.config" DestinationFiles="$(OutDir)\testhost.dll.config" />
    /// </Target>
    /// </summary>
    [TestFixture]
    [Ignore("This test could be run only with created OneBox locally.")]
    public class BulkUploadCompressionTests
    {
        private ApiObjects.BulkUpload.BulkUpload _bulkUploadToSave;
        private string _bulkUploadKey;

        [OneTimeSetUp]
        public void OnetimeSetup()
        {
            ConfigurationManager.ApplicationConfiguration.Init();
        }
        
        [SetUp]
        public void SetUp()
        {
            _bulkUploadToSave = GenerateBulkUploadObject();
            _bulkUploadKey = $"bulk_upload_{_bulkUploadToSave.Id}";
        }

        private static ApiObjects.BulkUpload.BulkUpload GenerateBulkUploadObject()
        {
            return new ApiObjects.BulkUpload.BulkUpload
            {
                Action = BulkUploadJobAction.Upsert,
                AddedObjects = new List<IAffectedObject>(),
                AffectedObjects = new List<IAffectedObject>(),
                BulkObjectType = "customtype",
                ChangedFields = new List<string>(),
                CreateDate = DateTime.UtcNow,
                Id = 123,
                GroupId = 1483,
                JobData = new BulkUploadIngestJobData
                {
                    DisableEpgNotification = false,
                    DatesOfProgramsToIngest = new[]
                    {
                        DateTime.UtcNow.AddDays(-3)
                    }
                },
                ObjectData = new BulkUploadEpgAssetData
                {
                    GroupId = 123
                },
                Status = BulkUploadJobStatus.Queued
            };
        }

        [Test]
        [Ignore("This test could be run only with created OneBox locally.")]
        public void SaveBulkUploadUncompressedAndGetItUsingCompressionCouchbaseManager()
        {
            UtilsDal.SaveObjectInCB(eCouchbaseBucket.OTT_APPS, _bulkUploadKey, _bulkUploadToSave);

            var bulkUpload = UtilsDal.GetObjectFromCB<ApiObjects.BulkUpload.BulkUpload>(eCouchbaseBucket.OTT_APPS, _bulkUploadKey);

            bulkUpload.Should().BeEquivalentTo(_bulkUploadToSave);
        }
        
        [Test]
        [Ignore("This test could be run only with created OneBox locally.")]
        public void SaveBulkUploadCompressedAndGetItUsingCompressionCouchbaseManager()
        {
            UtilsDal.SaveObjectInCB(eCouchbaseBucket.OTT_APPS, _bulkUploadKey, _bulkUploadToSave, compress: true);

            var bulkUpload = UtilsDal.GetObjectFromCB<ApiObjects.BulkUpload.BulkUpload>(eCouchbaseBucket.OTT_APPS, _bulkUploadKey);

            bulkUpload.Should().BeEquivalentTo(_bulkUploadToSave);
        }
    }
}