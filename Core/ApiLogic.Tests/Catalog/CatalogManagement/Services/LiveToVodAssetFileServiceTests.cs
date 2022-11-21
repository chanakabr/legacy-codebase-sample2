using System;
using System.Collections.Generic;
using ApiLogic.Catalog.CatalogManagement.Models;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiLogic.Pricing.Handlers;
using ApiObjects.Base;
using ApiObjects.Response;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.Pricing;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;

namespace ApiLogic.Tests.Catalog.CatalogManagement.Services
{
    public class LiveToVodAssetFileServiceTests
    {
        private Mock<IMediaFileTypeManager> _mediaFileTypeManagerMock;
        private Mock<IPriceManager> _priceManagerMock;
        private ILogger<LiveToVodAssetFileService> _logger;
        private MockRepository _mockRepository;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _mediaFileTypeManagerMock = _mockRepository.Create<IMediaFileTypeManager>();
            _priceManagerMock = _mockRepository.Create<IPriceManager>();
            _logger = NullLogger<LiveToVodAssetFileService>.Instance;
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [Test]
        public void AssignPpvOnAssetCreated_NoMatchedPpvToAssign()
        {
            var ppv = new List<PpvModuleInfo>
            {
                new PpvModuleInfo { PpvModuleId = 1, FileTypeId = 1 }
            };

            var assetFiles = new List<AssetFile>
            {
                new AssetFile { TypeId = 2 }
            };

            var service =
                new LiveToVodAssetFileService(_mediaFileTypeManagerMock.Object, _priceManagerMock.Object, _logger);
            service.AssignPpvOnAssetCreated(1, 1, assetFiles, ppv);
        }

        [Test]
        public void AssignPpvOnAssetCreated_AssignPpv()
        {
            var ppv = new List<PpvModuleInfo>
            {
                new PpvModuleInfo { PpvModuleId = 1, FileTypeId = 1 }
            };

            var assetFiles = new List<AssetFile>
            {
                new AssetFile { Id = 1, TypeId = 1 }
            };

            _mediaFileTypeManagerMock.Setup(x => x.DoFreeItemIndexUpdateIfNeeded(1, 1, null, null, null, null));
            _priceManagerMock.Setup(x => x.AddAssetFilePPV(
                    It.IsAny<ContextData>(),
                    It.Is<AssetFilePpv>(x =>
                        x.AssetFileId == 1 && x.PpvModuleId == 1 && !x.StartDate.HasValue && !x.EndDate.HasValue)))
                .Returns(new GenericResponse<AssetFilePpv>(Status.Ok, new AssetFilePpv()));

            var service =
                new LiveToVodAssetFileService(_mediaFileTypeManagerMock.Object, _priceManagerMock.Object, _logger);
            service.AssignPpvOnAssetCreated(1, 1, assetFiles, ppv);
        }

        [Test]
        public void AssignPpvOnAssetUpdated_NoMatchedPpvToAssign()
        {
            var ppv = new List<PpvModuleInfo>
            {
                new PpvModuleInfo { PpvModuleId = 1, FileTypeId = 1 }
            };

            var assetFiles = new List<AssetFile>
            {
                new AssetFile { Id = 1, TypeId = 2 }
            };

            _priceManagerMock.Setup(x => x.GetAssetFilePPVList(It.IsAny<ContextData>(), 1, 0))
                .Returns(new GenericListResponse<AssetFilePpv>(Status.Ok, new List<AssetFilePpv>()));

            var service =
                new LiveToVodAssetFileService(_mediaFileTypeManagerMock.Object, _priceManagerMock.Object, _logger);
            service.AssignPpvOnAssetUpdated(1, 1, assetFiles, ppv);
        }

        [Test]
        public void AssignPpvOnAssetUpdated_NoMatchedPpvAndClearExisting()
        {
            var ppv = new List<PpvModuleInfo>
            {
                new PpvModuleInfo { PpvModuleId = 1, FileTypeId = 1 }
            };

            var assetFiles = new List<AssetFile>
            {
                new AssetFile { Id = 1, TypeId = 2 }
            };

            _priceManagerMock.Setup(x => x.GetAssetFilePPVList(It.IsAny<ContextData>(), 1, 0))
                .Returns(new GenericListResponse<AssetFilePpv>(Status.Ok, new List<AssetFilePpv>
                {
                    new AssetFilePpv { AssetFileId = 1, PpvModuleId = 2 }
                }));
            _priceManagerMock.Setup(x => x.DeleteAssetFilePPV(It.IsAny<ContextData>(), 1, 2))
                .Returns(Status.Ok);

            var service =
                new LiveToVodAssetFileService(_mediaFileTypeManagerMock.Object, _priceManagerMock.Object, _logger);
            service.AssignPpvOnAssetUpdated(1, 1, assetFiles, ppv);
        }

        [Test]
        public void AssignPpvOnAssetUpdated_AddNewPpv()
        {
            var ppv = new List<PpvModuleInfo>
            {
                new PpvModuleInfo { PpvModuleId = 1, FileTypeId = 1 }
            };

            var assetFiles = new List<AssetFile>
            {
                new AssetFile { Id = 1, TypeId = 1 }
            };

            _priceManagerMock.Setup(x => x.GetAssetFilePPVList(It.IsAny<ContextData>(), 1, 0))
                .Returns(new GenericListResponse<AssetFilePpv>(Status.Ok, new List<AssetFilePpv>()));
            _priceManagerMock.Setup(x => x.AddAssetFilePPV(
                    It.IsAny<ContextData>(),
                    It.Is<AssetFilePpv>(x =>
                        x.AssetFileId == 1 && x.PpvModuleId == 1 && !x.StartDate.HasValue && !x.EndDate.HasValue)))
                .Returns(new GenericResponse<AssetFilePpv>(Status.Ok, new AssetFilePpv()));
            _mediaFileTypeManagerMock.Setup(x => x.DoFreeItemIndexUpdateIfNeeded(1, 1, null, null, null, null));

            var service =
                new LiveToVodAssetFileService(_mediaFileTypeManagerMock.Object, _priceManagerMock.Object, _logger);
            service.AssignPpvOnAssetUpdated(1, 1, assetFiles, ppv);
        }

        [Test]
        public void AssignPpvOnAssetUpdated_DeleteAndUpdatePpv()
        {
            var ppvStartDate = new DateTime(2022, 5, 1);
            var ppvEndDate = new DateTime(2022, 6, 1);
            var ppv = new List<PpvModuleInfo>
            {
                new PpvModuleInfo { PpvModuleId = 1, FileTypeId = 1, StartDate = ppvStartDate, EndDate = ppvEndDate }
            };

            var assetFiles = new List<AssetFile>
            {
                new AssetFile { Id = 1, TypeId = 1 }
            };

            _priceManagerMock.Setup(x => x.GetAssetFilePPVList(It.IsAny<ContextData>(),1, 0))
                .Returns(new GenericListResponse<AssetFilePpv>(Status.Ok, new List<AssetFilePpv>
                {
                    new AssetFilePpv { AssetFileId = 1, PpvModuleId = 1 },
                    new AssetFilePpv { AssetFileId = 1, PpvModuleId = 2 }
                }));
            _priceManagerMock.Setup(x => x.DeleteAssetFilePPV(It.IsAny<ContextData>(), 1, 2))
                .Returns(Status.Ok);
            _priceManagerMock.Setup(x => x.UpdateAssetFilePPV(
                    It.IsAny<ContextData>(),
                    It.Is<AssetFilePpv>(x =>
                        x.AssetFileId == 1 && x.PpvModuleId == 1 && x.StartDate == ppvStartDate &&
                        x.EndDate == ppvEndDate)))
                .Returns(new GenericResponse<AssetFilePpv>(Status.Ok,
                    new AssetFilePpv { StartDate = ppvStartDate, EndDate = ppvEndDate }));
            _mediaFileTypeManagerMock.Setup(x =>
                x.DoFreeItemIndexUpdateIfNeeded(1, 1, null, ppvStartDate, null, ppvEndDate));

            var service =
                new LiveToVodAssetFileService(_mediaFileTypeManagerMock.Object, _priceManagerMock.Object, _logger);
            service.AssignPpvOnAssetUpdated(1, 1, assetFiles, ppv);
        }
    }
}