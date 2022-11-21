using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ApiLogic.Api.Managers;
using ApiLogic.Api.Validators;
using ApiLogic.Repositories;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Catalog;
using ApiObjects.Response;
using AutoFixture;
using Core.Api;
using Core.Catalog.CatalogManagement;
using DAL;
using Moq;
using NUnit.Framework;

namespace ApiLogic.Tests.Partner
{
    [TestFixture]
    public class CatalogPartnerConfigManagerTests
    {
        [TestCaseSource(nameof(UpdateTestCases))]
        public void CheckUpdate(CatalogPartnerConfig existCatalogPartnerConfig)
        {
            Fixture fixture = new Fixture();
            var objectToUpdate = fixture.Create<CatalogPartnerConfig>();

            var layeredCacheMock = LayeredCacheHelper.GetLayeredCacheMock(existCatalogPartnerConfig, true, true);

            var categoryCacheMock = new Mock<ICategoryCache>();
            var treeListResponse = new GenericListResponse<CategoryVersion>(Status.Ok, new List<CategoryVersion>() { fixture.Create<CategoryVersion>() });
            categoryCacheMock
                .Setup(x => x.ListCategoryVersionByTree(It.IsAny<ContextData>(), It.IsAny<CategoryVersionFilterByTree>(), null))
                .Returns(treeListResponse);

            var deviceFamiliesResponse = new GenericListResponse<DeviceFamily>(Status.Ok, objectToUpdate.CategoryManagement.DeviceFamilyToCategoryTree.Select(x => new DeviceFamily(x.Key, fixture.Create<string>())).ToList());
            var domainDeviceManagerMock = new Mock<IDomainDeviceManager>();
            var deviceFamilyRepositoryMock = new Mock<IDeviceFamilyRepository>();
            deviceFamilyRepositoryMock
                .Setup(x => x.List(1))
                .Returns(deviceFamiliesResponse);

            var topicManagerMock = new Mock<ITopicManager>();
            topicManagerMock
                .Setup(x => x.GetTopicsByIds(It.IsAny<int>(), It.IsAny<List<long>>(), MetaType.All))
                .Returns(new GenericListResponse<Topic>(Status.Ok, new List<Topic> { new Topic { Id = objectToUpdate.ShopMarkerMetaId.Value } }));

            var repositoryMock = new Mock<ICatalogPartnerRepository>();
            repositoryMock
                .Setup(x => x.SaveCatalogPartnerConfig(It.IsAny<int>(), It.IsAny<CatalogPartnerConfig>()))
                .Returns(true);

            var validator = new CatalogPartnerConfigValidator(categoryCacheMock.Object, deviceFamilyRepositoryMock.Object, topicManagerMock.Object);

            CatalogPartnerConfigManager manager = new CatalogPartnerConfigManager(repositoryMock.Object, layeredCacheMock.Object, domainDeviceManagerMock.Object, validator);
            var updateStatus = manager.UpdateCatalogConfig(1, objectToUpdate);
            Assert.That(updateStatus.Code, Is.EqualTo((int)eResponseStatus.OK));
        }

        private static IEnumerable UpdateTestCases()
        {
            Fixture fixture = new Fixture();

            // exist old
            CatalogPartnerConfig catalogPartnerConfig1 = fixture.Create<CatalogPartnerConfig>();
            yield return new TestCaseData(catalogPartnerConfig1).SetName("Update_Existing");

            // old not exist
            CatalogPartnerConfig catalogPartnerConfig2 = null;
            yield return new TestCaseData(catalogPartnerConfig2).SetName("Update_NotExisting");
        }

        [TestCaseSource(nameof(UpdateErrorTestCases))]
        public void CheckUpdateErrors(eResponseStatus expectedError,
                                      CatalogPartnerConfig objectToUpdate,
                                      List<int> deviceFamilyIds,
                                      Dictionary<long, List<CategoryVersion>> treeListMap,
                                      long shopMarkerMetaId)
        {
            Fixture fixture = new Fixture();

            var layeredCacheMock = LayeredCacheHelper.GetLayeredCacheMock(fixture.Create<CatalogPartnerConfig>(), true, false);

            var categoryCacheMock = new Mock<ICategoryCache>();
            var defaultTree = objectToUpdate.CategoryManagement.DefaultCategoryTree.Value;
            List<CategoryVersion> defaultTreeList = treeListMap.ContainsKey(defaultTree) ? treeListMap[defaultTree] : null;

            categoryCacheMock
                .Setup(x => x.ListCategoryVersionByTree(It.IsAny<ContextData>(), It.Is<CategoryVersionFilterByTree>(o => o.TreeId == defaultTree), null))
                .Returns(new GenericListResponse<CategoryVersion>(Status.Ok, defaultTreeList));

            foreach (var treeId in objectToUpdate.CategoryManagement.DeviceFamilyToCategoryTree.Values)
            {
                List<CategoryVersion> treeList = treeListMap.ContainsKey(treeId) ? treeListMap[treeId] : null;
                categoryCacheMock
                    .Setup(x => x.ListCategoryVersionByTree(It.IsAny<ContextData>(), It.Is<CategoryVersionFilterByTree>(o => o.TreeId == treeId), null))
                    .Returns(new GenericListResponse<CategoryVersion>(Status.Ok, treeList));
            }

            var deviceFamiliesResponse = new GenericListResponse<DeviceFamily>(Status.Ok, deviceFamilyIds.Select(x => new DeviceFamily(x, fixture.Create<string>())).ToList());
            var domainDeviceManagerMock = new Mock<IDomainDeviceManager>();
            var deviceFamilyRepositoryMock = new Mock<IDeviceFamilyRepository>();
            deviceFamilyRepositoryMock
                .Setup(x => x.List(1))
                .Returns(deviceFamiliesResponse);

            var topicManagerMock = new Mock<ITopicManager>();
            topicManagerMock
                .Setup(x => x.GetTopicsByIds(It.IsAny<int>(), It.IsAny<List<long>>(), MetaType.All))
                .Returns(() =>
                {
                    Topic topic;
                    if (shopMarkerMetaId == 101)
                    {
                        topic = new Topic { Id = 101 };
                    }
                    else if (shopMarkerMetaId == 102)
                    {
                        topic = new Topic { Id = 102, Type = MetaType.Tag };
                    }
                    else
                    {
                        topic = null;
                    }

                    return new GenericListResponse<Topic>(Status.Ok, new List<Topic> { topic });
                });

            var repositoryMock = new Mock<ICatalogPartnerRepository>();

            var validator = new CatalogPartnerConfigValidator(categoryCacheMock.Object, deviceFamilyRepositoryMock.Object, topicManagerMock.Object);

            CatalogPartnerConfigManager manager = new CatalogPartnerConfigManager(repositoryMock.Object, layeredCacheMock.Object, domainDeviceManagerMock.Object, validator);
            var updateStatus = manager.UpdateCatalogConfig(1, objectToUpdate);

            Assert.That(updateStatus.Code, Is.EqualTo((int)expectedError));
            repositoryMock.Verify(foo => foo.SaveCatalogPartnerConfig(It.IsAny<int>(), It.IsAny<CatalogPartnerConfig>()), Times.Never());
            layeredCacheMock.Verify(foo => foo.SetInvalidationKey(It.IsAny<string>(), null), Times.Never());
        }

        private static IEnumerable UpdateErrorTestCases()
        {
            var fixture = new Fixture();

            //CategoryTreeNotExist for default
            var catalogPartnerConfig1 = fixture.Create<CatalogPartnerConfig>();
            var treeListMap1 = new Dictionary<long, List<CategoryVersion>>();
            var other1 = catalogPartnerConfig1.CategoryManagement.DeviceFamilyToCategoryTree
                .FirstOrDefault(x => x.Value == catalogPartnerConfig1.CategoryManagement.DefaultCategoryTree.Value);
            if (!other1.Equals(default(KeyValuePair<int, long>)))
            {
                catalogPartnerConfig1.CategoryManagement.DeviceFamilyToCategoryTree.Remove(other1.Key);
            }

            var tree1 = fixture.Create<List<CategoryVersion>>();
            foreach (var pair in catalogPartnerConfig1.CategoryManagement.DeviceFamilyToCategoryTree)
            {
                if (!treeListMap1.ContainsKey(pair.Value))
                {
                    treeListMap1.Add(pair.Key, tree1);
                }
            }
            var deviceFamilyIds1 = catalogPartnerConfig1.CategoryManagement.DeviceFamilyToCategoryTree.Keys.ToList();
            yield return new TestCaseData(eResponseStatus.CategoryTreeDoesNotExist, catalogPartnerConfig1, deviceFamilyIds1, treeListMap1, 101).SetName("UpdateError_DefaultTreeNotExist");

            // CategoryTreeNotExist for non default
            var catalogPartnerConfig2 = fixture.Create<CatalogPartnerConfig>();
            var deviceFamilyIds2 = catalogPartnerConfig2.CategoryManagement.DeviceFamilyToCategoryTree.Keys.ToList();
            var treeListMap2 = new Dictionary<long, List<CategoryVersion>>();
            var tree2 = fixture.Create<List<CategoryVersion>>();
            treeListMap2.Add(catalogPartnerConfig2.CategoryManagement.DefaultCategoryTree.Value, tree2);
            long last = 0;
            foreach (var pair in catalogPartnerConfig2.CategoryManagement.DeviceFamilyToCategoryTree)
            {
                last = pair.Value;
                if (pair.Value % 2 == 0 && !treeListMap2.ContainsKey(pair.Value))
                {
                    treeListMap2.Add(pair.Key, tree1);
                }
            }

            if (treeListMap2.Count == catalogPartnerConfig2.CategoryManagement.DeviceFamilyToCategoryTree.Count)
            {
                treeListMap2.Remove(last);
            }

            yield return new TestCaseData(eResponseStatus.CategoryTreeDoesNotExist, catalogPartnerConfig2, deviceFamilyIds2, treeListMap2, 101).SetName("UpdateError_NonDefaultTreeNotExist");

            // NonExistingDeviceFamilyIds - no device family for partner
            var catalogPartnerConfig3 = fixture.Create<CatalogPartnerConfig>();
            var deviceFamilyIds3 = new List<int>();
            var treeListMap3 = new Dictionary<long, List<CategoryVersion>>();
            var tree3 = fixture.Create<List<CategoryVersion>>();
            treeListMap3.Add(catalogPartnerConfig3.CategoryManagement.DefaultCategoryTree.Value, tree3);
            foreach (var pair in catalogPartnerConfig3.CategoryManagement.DeviceFamilyToCategoryTree)
            {
                if (!treeListMap3.ContainsKey(pair.Value))
                {
                    treeListMap3.Add(pair.Key, tree1);
                }
            }
            yield return new TestCaseData(eResponseStatus.NonExistingDeviceFamilyIds, catalogPartnerConfig3, deviceFamilyIds3, treeListMap3, 101).SetName("UpdateError_NoDeviceFamilyForPartner");

            // NonExistingDeviceFamilyIds - missing device families in partner
            var catalogPartnerConfig4 = fixture.Create<CatalogPartnerConfig>();
            var treeListMap4 = new Dictionary<long, List<CategoryVersion>>();
            var tree4 = fixture.Create<List<CategoryVersion>>();
            treeListMap4.Add(catalogPartnerConfig4.CategoryManagement.DefaultCategoryTree.Value, tree3);
            foreach (var pair in catalogPartnerConfig4.CategoryManagement.DeviceFamilyToCategoryTree)
            {
                if (!treeListMap4.ContainsKey(pair.Value))
                {
                    treeListMap4.Add(pair.Key, tree1);
                }
            }
            var deviceFamilyIds4 = catalogPartnerConfig2.CategoryManagement.DeviceFamilyToCategoryTree.Keys.ToList();
            deviceFamilyIds4.RemoveAll(x => x % 2 == 0);
            if (deviceFamilyIds4.Count == catalogPartnerConfig2.CategoryManagement.DeviceFamilyToCategoryTree.Count)
            {
                deviceFamilyIds4.RemoveAt(0);
            }

            yield return new TestCaseData(eResponseStatus.NonExistingDeviceFamilyIds, catalogPartnerConfig4, deviceFamilyIds4, treeListMap4, 101).SetName("UpdateError_MissingDeviceFamilies");

            // var catalogPartnerConfig5 = fixture.Create<CatalogPartnerConfig>();
            // catalogPartnerConfig5.ShopMarkerMetaId = 102;
            // var deviceFamilyIds5 = catalogPartnerConfig5.CategoryManagement.DeviceFamilyToCategoryTree.Keys.ToList();
            // var tree5 = fixture.Create<List<CategoryVersion>>();
            // var treeListMap5 = new Dictionary<long, List<CategoryVersion>> { { catalogPartnerConfig5.CategoryManagement.DefaultCategoryTree.Value, tree5 } };
            // foreach (var pair in catalogPartnerConfig5.CategoryManagement.DeviceFamilyToCategoryTree)
            // {
            //     if (!treeListMap5.ContainsKey(pair.Value))
            //     {
            //         treeListMap5.Add(pair.Value, tree5);
            //     }
            // }
            //
            // yield return new TestCaseData(eResponseStatus.Error, catalogPartnerConfig5, deviceFamilyIds5, treeListMap5, catalogPartnerConfig5.ShopMarkerMetaId).SetName("UpdateError_MetaWithMultiValue");

            var catalogPartnerConfig6 = fixture.Create<CatalogPartnerConfig>();
            catalogPartnerConfig6.ShopMarkerMetaId = 103;
            var deviceFamilyIds6 = catalogPartnerConfig6.CategoryManagement.DeviceFamilyToCategoryTree.Keys.ToList();
            var tree6 = fixture.Create<List<CategoryVersion>>();
            var treeListMap6 = new Dictionary<long, List<CategoryVersion>> { { catalogPartnerConfig6.CategoryManagement.DefaultCategoryTree.Value, tree6 } };
            foreach (var pair in catalogPartnerConfig6.CategoryManagement.DeviceFamilyToCategoryTree)
            {
                if (!treeListMap6.ContainsKey(pair.Value))
                {
                    treeListMap6.Add(pair.Value, tree6);
                }
            }

            yield return new TestCaseData(eResponseStatus.MetaNotFound, catalogPartnerConfig6, deviceFamilyIds6, treeListMap6, catalogPartnerConfig6.ShopMarkerMetaId).SetName("UpdateError_MetaNotFound");
        }

        [TestCaseSource(nameof(GetCatalogConfigTestCases))]
        public void CheckGetCatalogConfig(CatalogPartnerConfig catalogPartnerConfig, eResponseStatus expectedResponse, bool cacheWork)
        {
            var fixture = new Fixture();
            var repositoryMock = Mock.Of<ICatalogPartnerRepository>();
            var layeredCacheMock = LayeredCacheHelper.GetLayeredCacheMock(catalogPartnerConfig, cacheWork, false);
            var categoryCacheMock = Mock.Of<ICategoryCache>();
            var domainDeviceManagerMock = Mock.Of<IDomainDeviceManager>();
            var deviceFamilyRepositoryMock = Mock.Of<IDeviceFamilyRepository>();
            var topicManagerMock = Mock.Of<ITopicManager>();

            var validator = new CatalogPartnerConfigValidator(categoryCacheMock, deviceFamilyRepositoryMock, topicManagerMock);

            CatalogPartnerConfigManager manager = new CatalogPartnerConfigManager(repositoryMock, layeredCacheMock.Object, domainDeviceManagerMock, validator);
            var response = manager.GetCatalogConfig(fixture.Create<int>());
            Assert.That(response.Status.Code, Is.EqualTo((int)expectedResponse));
        }

        private static IEnumerable GetCatalogConfigTestCases()
        {
            // cache fail
            yield return new TestCaseData(null, eResponseStatus.Error, false).SetName("GetCatalogConfig_CacheFail");

            // no config
            yield return new TestCaseData(null, eResponseStatus.PartnerConfigurationDoesNotExist, true).SetName("GetCatalogConfig_NoConfig");

            // config exist
            yield return new TestCaseData(new CatalogPartnerConfig(), eResponseStatus.OK, true).SetName("GetCatalogConfig_ConfigExist");
        }

        [TestCaseSource(nameof(GetCategoryVersionTreeIdTestCases))]
        public void CheckGetCategoryVersionTreeId(CatalogPartnerConfig catalogPartnerConfig, long expectedTreeId, int deviceFamilyId, int? deviceFamily)
        {
            var layeredCacheMock = LayeredCacheHelper.GetLayeredCacheMock(catalogPartnerConfig, true, false);

            var domainDeviceManagerMock = new Mock<IDomainDeviceManager>();
            domainDeviceManagerMock
                .Setup(x => x.GetDeviceFamilyIdByUdid(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                .Returns(deviceFamilyId);

            var topicManagerMock = new Mock<ITopicManager>();

            var fixture = new Fixture();
            var repositoryMock = Mock.Of<ICatalogPartnerRepository>();
            var categoryCacheMock = Mock.Of<ICategoryCache>();
            var deviceFamilyRepositoryMock = Mock.Of<IDeviceFamilyRepository>();
            var validator = new CatalogPartnerConfigValidator(categoryCacheMock, deviceFamilyRepositoryMock, topicManagerMock.Object);

            CatalogPartnerConfigManager manager = new CatalogPartnerConfigManager(repositoryMock, layeredCacheMock.Object, domainDeviceManagerMock.Object, validator);
            var treeId = manager.GetCategoryVersionTreeIdByDeviceFamilyId(fixture.Create<ContextData>(), deviceFamily);
            Assert.That(treeId, Is.EqualTo(expectedTreeId));
        }

        private static IEnumerable GetCategoryVersionTreeIdTestCases()
        {
            Fixture fixture = new Fixture();

            // no category management
            var config1 = fixture.Create<CatalogPartnerConfig>();
            config1.CategoryManagement = null;
            var realDeviceFamily1 = fixture.Create<int>();
            yield return new TestCaseData(config1, 0, realDeviceFamily1, null).SetName("GetCategoryVersionTreeId_NoCategoryManagement");

            // no default and no map 
            var config2 = fixture.Create<CatalogPartnerConfig>();
            config2.CategoryManagement = new CategoryManagement();
            var realDeviceFamily2 = fixture.Create<int>();
            yield return new TestCaseData(config2, 0, realDeviceFamily2, null).SetName("GetCategoryVersionTreeId_NoValues");

            // get tree by map
            var config3 = fixture.Create<CatalogPartnerConfig>();
            var realDeviceFamilyId3 = fixture.Create<int>();
            if (!config3.CategoryManagement.DeviceFamilyToCategoryTree.ContainsKey(realDeviceFamilyId3))
            {
                config3.CategoryManagement.DeviceFamilyToCategoryTree.Add(realDeviceFamilyId3, fixture.Create<long>());
            }
            var treeId3 = config3.CategoryManagement.DeviceFamilyToCategoryTree[realDeviceFamilyId3];
            yield return new TestCaseData(config3, treeId3, realDeviceFamilyId3, null).SetName("GetCategoryVersionTreeId_TreeByMap");

            // no device familiy in map - return default tree
            var config4 = fixture.Create<CatalogPartnerConfig>();
            var realDeviceFamilyId4 = fixture.Create<int>();
            if (config4.CategoryManagement.DeviceFamilyToCategoryTree.ContainsKey(realDeviceFamilyId4))
            {
                config4.CategoryManagement.DeviceFamilyToCategoryTree.Remove(realDeviceFamilyId4);
            }
            var treeId4 = config4.CategoryManagement.DefaultCategoryTree.Value;
            yield return new TestCaseData(config4, treeId4, realDeviceFamilyId4, null).SetName("GetCategoryVersionTreeId_DefaultTree");

            // get tree by map WithDeviceFamily
            var config5 = fixture.Create<CatalogPartnerConfig>();
            var realDeviceFamilyId5 = fixture.Create<int>();
            if (!config5.CategoryManagement.DeviceFamilyToCategoryTree.ContainsKey(realDeviceFamilyId5))
            {
                config5.CategoryManagement.DeviceFamilyToCategoryTree.Add(realDeviceFamilyId5, fixture.Create<long>());
            }
            var treeId5 = config5.CategoryManagement.DeviceFamilyToCategoryTree[realDeviceFamilyId5];
            yield return new TestCaseData(config5, treeId5, realDeviceFamilyId5, realDeviceFamilyId5).SetName("GetCategoryVersionTreeId_TreeByMapWithDeviceFamily");

            // no device familiy in map - return default tree WithDeviceFamily
            var config6 = fixture.Create<CatalogPartnerConfig>();
            var realDeviceFamilyId6 = fixture.Create<int>();
            if (config6.CategoryManagement.DeviceFamilyToCategoryTree.ContainsKey(realDeviceFamilyId6))
            {
                config6.CategoryManagement.DeviceFamilyToCategoryTree.Remove(realDeviceFamilyId6);
            }
            var treeId6 = config6.CategoryManagement.DefaultCategoryTree.Value;
            yield return new TestCaseData(config6, treeId6, realDeviceFamilyId6, realDeviceFamilyId6).SetName("GetCategoryVersionTreeId_DefaultTreeWithDeviceFamily");
        }
    }
}