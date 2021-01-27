using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Catalog;
using ApiObjects.Response;
using AutoFixture;
using CachingProvider.LayeredCache;
using Core.Api;
using Core.Catalog.CatalogManagement;
using DAL;
using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace ApiLogic.Tests.Partner
{
    [TestFixture]
    public class CatalogPartnerConfigManagerTests
    {
        delegate void MockGetFromCache(string key, ref CatalogPartnerConfig genericParameter, Func<Dictionary<string, object>, Tuple<CatalogPartnerConfig, bool>> fillObjectMethod,
                                       Dictionary<string, object> funcParameters, int groupId, string layeredCacheConfigName, List<string> inValidationKeys = null,
                                       bool shouldUseAutoNameTypeHandling = false);

        [TestCaseSource(nameof(UpdateTestCases))]
        public void CheckUpdate(CatalogPartnerConfig existCatalogPartnerConfig)
        {
            Fixture fixture = new Fixture();
            var objectToUpdate = fixture.Create<CatalogPartnerConfig>();

            var layeredCacheMock = GetLayeredCacheMock(existCatalogPartnerConfig, true, true);

            var categoryCacheMock = new Mock<ICategoryCache>();
            var treeListResponse = new GenericListResponse<CategoryVersion>(Status.Ok, new List<CategoryVersion>() { fixture.Create<CategoryVersion>() });
            categoryCacheMock.Setup(x => x.ListCategoryVersionByTree(It.IsAny<ContextData>(), It.IsAny<CategoryVersionFilterByTree>(), null))
                             .Returns(treeListResponse);

            var deviceFamilies = objectToUpdate.CategoryManagement.DeviceFamilyToCategoryTree.Select(x => new DeviceFamily(x.Key, fixture.Create<string>())).ToList();
            var deviceFamilyManagerMock = new Mock<IDeviceFamilyManager>();
            deviceFamilyManagerMock.Setup(x => x.GetDeviceFamilyList())
                                   .Returns(new DeviceFamilyResponse() { Status = Status.Ok, DeviceFamilies = deviceFamilies });

            var repositoryMock = new Mock<ICatalogPartnerRepository>();
            repositoryMock.Setup(x => x.SaveCatalogPartnerConfig(It.IsAny<int>(), It.IsAny<CatalogPartnerConfig>()))
                          .Returns(true);

            CatalogPartnerConfigManager manager = new CatalogPartnerConfigManager(repositoryMock.Object, layeredCacheMock.Object, categoryCacheMock.Object, deviceFamilyManagerMock.Object);
            var updateStatus = manager.UpdateCatalogConfig(fixture.Create<int>(), objectToUpdate);
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
                                      Dictionary<long, List<CategoryVersion>> treeListMap)
        {
            Fixture fixture = new Fixture();

            var layeredCacheMock = GetLayeredCacheMock(fixture.Create<CatalogPartnerConfig>(), true, false);

            var categoryCacheMock = new Mock<ICategoryCache>();
            var defaultTree = objectToUpdate.CategoryManagement.DefaultCategoryTree.Value;
            List<CategoryVersion> defaultTreeList = treeListMap.ContainsKey(defaultTree) ? treeListMap[defaultTree] : null;

            categoryCacheMock.Setup(x => x.ListCategoryVersionByTree(It.IsAny<ContextData>(), It.Is<CategoryVersionFilterByTree>(o => o.TreeId == defaultTree), null))
                             .Returns(new GenericListResponse<CategoryVersion>(Status.Ok, defaultTreeList));

            foreach (var treeId in objectToUpdate.CategoryManagement.DeviceFamilyToCategoryTree.Values)
            {
                List<CategoryVersion> treeList = treeListMap.ContainsKey(treeId) ? treeListMap[treeId] : null;
                categoryCacheMock.Setup(x => x.ListCategoryVersionByTree(It.IsAny<ContextData>(), It.Is<CategoryVersionFilterByTree>(o => o.TreeId == treeId), null))
                                 .Returns(new GenericListResponse<CategoryVersion>(Status.Ok, treeList));
            }

            var deviceFamilies = deviceFamilyIds.Select(x => new DeviceFamily(x, fixture.Create<string>())).ToList();
            var deviceFamilyManagerMock = new Mock<IDeviceFamilyManager>();
            deviceFamilyManagerMock.Setup(x => x.GetDeviceFamilyList())
                                   .Returns(new DeviceFamilyResponse() { Status = Status.Ok, DeviceFamilies = deviceFamilies });

            var repositoryMock = new Mock<ICatalogPartnerRepository>();

            CatalogPartnerConfigManager manager = new CatalogPartnerConfigManager(repositoryMock.Object, layeredCacheMock.Object, categoryCacheMock.Object, deviceFamilyManagerMock.Object);
            var updateStatus = manager.UpdateCatalogConfig(fixture.Create<int>(), objectToUpdate);

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
            yield return new TestCaseData(eResponseStatus.CategoryTreeDoesNotExist, catalogPartnerConfig1, deviceFamilyIds1, treeListMap1).SetName("UpdateError_DefaultTreeNotExist");
            
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
                if (pair.Value % 2 ==0 && !treeListMap2.ContainsKey(pair.Value))
                {
                    treeListMap2.Add(pair.Key, tree1);
                }
            }

            if (treeListMap2.Count == catalogPartnerConfig2.CategoryManagement.DeviceFamilyToCategoryTree.Count)
            {
                treeListMap2.Remove(last);
            }

            yield return new TestCaseData(eResponseStatus.CategoryTreeDoesNotExist, catalogPartnerConfig2, deviceFamilyIds2, treeListMap2).SetName("UpdateError_NonDefaultTreeNotExist");
            
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
            yield return new TestCaseData(eResponseStatus.NonExistingDeviceFamilyIds, catalogPartnerConfig3, deviceFamilyIds3, treeListMap3).SetName("UpdateError_NoDeviceFamilyForPartner");

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

            yield return new TestCaseData(eResponseStatus.NonExistingDeviceFamilyIds, catalogPartnerConfig4, deviceFamilyIds4, treeListMap4).SetName("UpdateError_MissingDeviceFamilies");
        }

        [TestCaseSource(nameof(GetCatalogConfigTestCases))]
        public void CheckGetCatalogConfig(CatalogPartnerConfig catalogPartnerConfig, eResponseStatus expectedResponse, bool cacheWork)
        {
            var fixture = new Fixture();
            var repositoryMock = Mock.Of<ICatalogPartnerRepository>();
            var layeredCacheMock = GetLayeredCacheMock(catalogPartnerConfig, cacheWork, false);
            var categoryCacheMock = Mock.Of<ICategoryCache>();
            var deviceFamilyManagerMock = Mock.Of<IDeviceFamilyManager>();
            CatalogPartnerConfigManager manager = new CatalogPartnerConfigManager(repositoryMock, layeredCacheMock.Object, categoryCacheMock, deviceFamilyManagerMock);
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
        public void CheckGetCategoryVersionTreeId(CatalogPartnerConfig catalogPartnerConfig, long expectedTreeId, int deviceFamilyId)
        {
            var layeredCacheMock = GetLayeredCacheMock(catalogPartnerConfig, true, false);

            var deviceFamilyManagerMock = new Mock<IDeviceFamilyManager>();
            deviceFamilyManagerMock.Setup(x => x.GetDeviceFamilyIdByUdid(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                                   .Returns(deviceFamilyId);

            var fixture = new Fixture();
            var repositoryMock = Mock.Of<ICatalogPartnerRepository>();
            var categoryCacheMock = Mock.Of<ICategoryCache>();
            CatalogPartnerConfigManager manager = new CatalogPartnerConfigManager(repositoryMock, layeredCacheMock.Object, categoryCacheMock, deviceFamilyManagerMock.Object);
            var treeId = manager.GetCategoryVersionTreeId(fixture.Create<ContextData>());
            Assert.That(treeId, Is.EqualTo(expectedTreeId));
        }

        private static IEnumerable GetCategoryVersionTreeIdTestCases()
        {
            Fixture fixture = new Fixture();

            // no category management
            var config1 = fixture.Create<CatalogPartnerConfig>();
            config1.CategoryManagement = null;
            yield return new TestCaseData(config1, 0, fixture.Create<int>()).SetName("GetCategoryVersionTreeId_NoCategoryManagement");

            // no default and no map 
            var config2 = fixture.Create<CatalogPartnerConfig>();
            config2.CategoryManagement = new CategoryManagement();
            yield return new TestCaseData(config2, 0, fixture.Create<int>()).SetName("GetCategoryVersionTreeId_NoValues");

            // get tree by map
            var config3 = fixture.Create<CatalogPartnerConfig>();
            var deviceFamilyId3 = fixture.Create<int>();
            if (!config3.CategoryManagement.DeviceFamilyToCategoryTree.ContainsKey(deviceFamilyId3))
            {
                config3.CategoryManagement.DeviceFamilyToCategoryTree.Add(deviceFamilyId3, fixture.Create<long>());
            }
            var treeId3 = config3.CategoryManagement.DeviceFamilyToCategoryTree[deviceFamilyId3];
            yield return new TestCaseData(config3, treeId3, deviceFamilyId3).SetName("GetCategoryVersionTreeId_TreeByMap");

            // no device familiy in map - return default tree
            var config4 = fixture.Create<CatalogPartnerConfig>();
            var deviceFamilyId4 = fixture.Create<int>();
            if (config4.CategoryManagement.DeviceFamilyToCategoryTree.ContainsKey(deviceFamilyId4))
            {
                config4.CategoryManagement.DeviceFamilyToCategoryTree.Remove(deviceFamilyId4);
            }
            var treeId4 = config4.CategoryManagement.DefaultCategoryTree.Value;
            yield return new TestCaseData(config4, treeId4, deviceFamilyId4).SetName("GetCategoryVersionTreeId_DefaultTree");
        }

        private Mock<ILayeredCache> GetLayeredCacheMock(CatalogPartnerConfig catalogPartnerConfig, bool cacheWork, bool setInvalidationKey)
        {
            var layeredCacheMock = new Mock<ILayeredCache>();
            layeredCacheMock.Setup(x => x.Get(It.IsAny<string>(),
                                             ref It.Ref<CatalogPartnerConfig>.IsAny,
                                             It.IsAny<Func<Dictionary<string, object>, Tuple<CatalogPartnerConfig, bool>>>(),
                                             It.IsAny<Dictionary<string, object>>(),
                                             It.IsAny<int>(),
                                             It.IsAny<string>(),
                                             It.IsAny<List<string>>(),
                                             It.IsAny<bool>()))
                           .Callback(new MockGetFromCache((string key,
                                                          ref CatalogPartnerConfig genericParameter,
                                                          Func<Dictionary<string, object>, Tuple<CatalogPartnerConfig, bool>> fillObjectMethod,
                                                          Dictionary<string, object> funcParameters,
                                                          int groupId,
                                                          string layeredCacheConfigName,
                                                          List<string> inValidationKeys,
                                                          bool shouldUseAutoNameTypeHandling) =>
                           {
                               genericParameter = catalogPartnerConfig;
                           }))
                          .Returns(cacheWork);

            if (setInvalidationKey)
            {
                layeredCacheMock.Setup(x => x.SetInvalidationKey(It.IsAny<string>(), null))
                            .Returns(true);
            }
            
            return layeredCacheMock;
        }
    }
}