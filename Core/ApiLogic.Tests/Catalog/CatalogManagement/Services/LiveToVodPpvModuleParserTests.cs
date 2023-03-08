using System;
using System.Collections.Generic;
using System.Linq;
using ApiLogic.Catalog.CatalogManagement.Models;
using ApiLogic.Catalog.CatalogManagement.Services;
using ApiLogic.Pricing.Handlers;
using ApiObjects.Base;
using ApiObjects.Pricing;
using ApiObjects.Response;
using Core.Pricing;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using NUnit.Framework;
using Phoenix.Generated.Api.Events.Crud.ProgramAsset;
using Status = ApiObjects.Response.Status;

namespace ApiLogic.Tests.Catalog.CatalogManagement.Services
{
    public class LiveToVodPpvModuleParserTests
    {
        private Mock<IPpvManager> _ppvManagerMock;
        private ILogger<LiveToVodPpvModuleParser> _logger;
        private MockRepository _mockRepository;

        [SetUp]
        public void Setup()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _ppvManagerMock = _mockRepository.Create<IPpvManager>();
            _logger = NullLogger<LiveToVodPpvModuleParser>.Instance;
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [Test]
        public void GetParsedPpv_NoMeta()
        {
            var programAsset = new ProgramAsset();
            var parser = new LiveToVodPpvModuleParser(_ppvManagerMock.Object, _logger);
            var actualResult = parser.GetParsedPpv(programAsset);

            actualResult.Should().NotBeNull();
            actualResult.Should().BeEmpty();
        }

        [TestCase(null)]
        [TestCase("")]
        [TestCase("123")]
        [TestCase("aaa;ppv")]
        public void GetParsedPpv_InvalidFormat(string ppvMetaValue)
        {
            var programAsset = new ProgramAsset
            {
                Metas = new[] { new AssetMeta { Name = "l2v_ppv_meta", Value = ppvMetaValue } }
            };

            var parser = new LiveToVodPpvModuleParser(_ppvManagerMock.Object, _logger);
            var actualResult = parser.GetParsedPpv(programAsset);

            actualResult.Should().NotBeNull();
            actualResult.Should().BeEmpty();
        }

        [Test]
        public void GetParsedPpv_PPVModulesNotAvailable()
        {
            var programAsset = new ProgramAsset
            {
                PartnerId = 1,
                Metas = new[] { new AssetMeta { Name = "l2v_ppv_module", Value = "1;aaa" } }
            };

            _ppvManagerMock.Setup(x => x.GetPPVModules(
                    It.IsAny<ContextData>(),
                    null,
                    false,
                    null,
                    false,
                    PPVOrderBy.NameAsc,
                    0,
                    30,
                    true,
                    null))
                .Returns(new GenericListResponse<PPVModule>());

            var parser = new LiveToVodPpvModuleParser(_ppvManagerMock.Object, _logger);
            var actualResult = parser.GetParsedPpv(programAsset);

            actualResult.Should().NotBeNull();
            actualResult.Should().BeEmpty();
        }

        [Test]
        public void GetParsedPpv_PPVModulesDoNotExist()
        {
            var programAsset = new ProgramAsset
            {
                PartnerId = 1,
                Metas = new[] { new AssetMeta { Name = "l2v_ppv_module", Value = "1;aaa" } }
            };

            _ppvManagerMock.Setup(x => x.GetPPVModules(
                    It.IsAny<ContextData>(),
                    null,
                    false,
                    null,
                    false,
                    PPVOrderBy.NameAsc,
                    0,
                    30,
                    true,
                    null))
                .Returns(new GenericListResponse<PPVModule>(Status.Ok, Enumerable.Empty<PPVModule>().ToList()));

            var parser = new LiveToVodPpvModuleParser(_ppvManagerMock.Object, _logger);
            var actualResult = parser.GetParsedPpv(programAsset);

            actualResult.Should().NotBeNull();
            actualResult.Should().BeEmpty();
        }

        [TestCaseSource(nameof(ParsedPpvData))]
        public void GetParsedPpv_ReturnsExpectedResult(string ppvMetaValue, IEnumerable<PpvModuleInfo> expectedResults)
        {
            var programAsset = new ProgramAsset
            {
                PartnerId = 1,
                Metas = new[] { new AssetMeta { Name = "l2v_ppv_module", Value = ppvMetaValue } }
            };

            _ppvManagerMock.Setup(x => x.GetPPVModules(
                    It.IsAny<ContextData>(),
                    null,
                    false,
                    null,
                    false,
                    PPVOrderBy.NameAsc,
                    0,
                    30,
                    true,
                    null))
                .Returns(new GenericListResponse<PPVModule>(
                    Status.Ok,
                    new List<PPVModule>
                    {
                        new PPVModule { m_sObjectVirtualName = "ppv with discount", m_sObjectCode = "1" },
                        new PPVModule { m_sObjectVirtualName = "ppv for family", m_sObjectCode = "2" },
                        new PPVModule { m_sObjectVirtualName = "ppv", m_sObjectCode = "3" }
                    }));

            var parser = new LiveToVodPpvModuleParser(_ppvManagerMock.Object, _logger);
            var actualResult = parser.GetParsedPpv(programAsset);

            actualResult.Should().NotBeNull();
            actualResult.Should().BeEquivalentTo(expectedResults);
        }

        private static IEnumerable<TestCaseData> ParsedPpvData()
        {
            yield return new TestCaseData(
                "1;ppv with discount",
                new List<PpvModuleInfo> { new PpvModuleInfo { FileTypeId = 1, PpvModuleId = 1 } });
            yield return new TestCaseData(
                "1;ppv with discount,2;ppv for family",
                new List<PpvModuleInfo>
                {
                    new PpvModuleInfo { FileTypeId = 1, PpvModuleId = 1 },
                    new PpvModuleInfo { FileTypeId = 2, PpvModuleId = 2 }
                });
            yield return new TestCaseData(
                "1;ppv with discount;10/10/2010 12:00:00,2;ppv for family",
                new List<PpvModuleInfo>
                {
                    new PpvModuleInfo { FileTypeId = 1, PpvModuleId = 1 },
                    new PpvModuleInfo { FileTypeId = 2, PpvModuleId = 2 }
                });
            yield return new TestCaseData(
                "1;ppv with discount;10/10/2022 12:00:00;10/11/2022 12:00:00,2;ppv for family",
                new List<PpvModuleInfo>
                {
                    new PpvModuleInfo
                    {
                        FileTypeId = 1,
                        PpvModuleId = 1,
                        StartDate = new DateTime(2022, 10, 10, 12, 0, 0),
                        EndDate = new DateTime(2022, 11, 10, 12, 0, 0)
                    },
                    new PpvModuleInfo { FileTypeId = 2, PpvModuleId = 2 }
                });
        }
    }
}