using ApiLogic.Api.Managers;
using ApiLogic.Catalog;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Catalog;
using ApiObjects.Response;
using AutoFixture;
using AutoFixture.Kernel;
using Core.Api;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using GroupsCacheManager;
using Moq;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;

namespace ApiLogic.Tests.Catalog.Category
{
    [TestFixture]
    public class CategoryItemHandlerTests
    {
        [TestCaseSource(nameof(GetTreeByVersionTestCases))]
        public void CheckGetTreeByVersion(long treeId, 
                                          List<CategoryVersion> defaultVersions, 
                                          long? versionId,
                                          int? deviceFamilyId,
                                          CategoryVersion categoryVersionOfId, 
                                          CategoryItem rootCategoryItem,
                                          Dictionary<long, CategoryItem> children)
        {
            Fixture fixture = new Fixture();
            
            var catalogPartnerConfigMock = new Mock<ICatalogPartnerConfigManager>();
            catalogPartnerConfigMock.Setup(x => x.GetCategoryVersionTreeIdByDeviceFamilyId(It.IsAny<ContextData>(), It.IsAny<int?>()))
                                    .Returns(treeId);

            var categoryCacheMock = new Mock<ICategoryCache>();
            var rootCategoryItemResponse = new GenericResponse<CategoryItem>(Status.Ok, rootCategoryItem);
            var itemId = categoryVersionOfId != null ? categoryVersionOfId.CategoryItemRootId : defaultVersions != null ? defaultVersions[0].CategoryItemRootId : 0;

            foreach (var child in rootCategoryItem.ChildrenIds)
            {
                var categoryItemResponse = new GenericResponse<CategoryItem>(Status.Ok, children[child]);
                categoryCacheMock.Setup(x => x.GetCategoryItem(It.IsAny<int>(), child))
                           .Returns(categoryItemResponse);
            }
           
            categoryCacheMock.Setup(x => x.GetCategoryItem(It.IsAny<int>(), itemId))
                            .Returns(rootCategoryItemResponse);

            if (categoryVersionOfId != null)
            {
                var categoryVersionResponse = new GenericResponse<CategoryVersion>(Status.Ok, categoryVersionOfId);
                categoryCacheMock.Setup(x => x.GetCategoryVersion(It.IsAny<int>(), It.IsAny<long>()))
                                .Returns(categoryVersionResponse);
            }

            if (defaultVersions != null)
            {
                var vaersionDefaultResponse = new GenericListResponse<CategoryVersion>(Status.Ok, defaultVersions);
                categoryCacheMock.Setup(x => x.ListCategoryVersionDefaults(It.IsAny<ContextData>(), It.IsAny<CategoryVersionFilter>(), null))
                                .Returns(vaersionDefaultResponse);
            }
            
            var externalChannelManagerMock = new Mock<IExternalChannelManager>();
            var externalChannelResponse = new GenericResponse<ExternalChannel>(Status.Ok, fixture.Create<ExternalChannel>());
            externalChannelManagerMock.Setup(x => x.GetChannelById(It.IsAny<ContextData>(), It.IsAny<int>(), It.IsAny<bool>()))
                             .Returns(externalChannelResponse);

            var channelManagerMock = new Mock<IChannelManager>();
            fixture.Customizations.Add(new TypeRelay(typeof(ApiObjects.SearchObjects.BooleanPhraseNode), typeof(ApiObjects.SearchObjects.BooleanLeaf)));
            var channelResponse = new GenericResponse<Channel>(Status.Ok, fixture.Create<Channel>());
            channelManagerMock.Setup(x => x.GetChannelById(It.IsAny<ContextData>(), It.IsAny<int>(), It.IsAny<bool>()))
                             .Returns(channelResponse);

            var imageManagerMock = new Mock<IImageManager>();
            var imageListResponse = new GenericListResponse<Image>(Status.Ok, fixture.Create<List<Image>>());
            imageManagerMock.Setup(x => x.GetImagesByObject(It.IsAny<int>(), It.IsAny<long>(), eAssetImageType.Category, null))
                             .Returns(imageListResponse);

            var catalogManagerMock = Mock.Of<ICatalogManager>();
            var virtualAssetPartnerManagerMock = Mock.Of<IVirtualAssetPartnerConfigManager>();
            var virtualAssetManagerMock = Mock.Of<IVirtualAssetManager>();
            var handler = new CategoryItemHandler(virtualAssetPartnerManagerMock, 
                                                  virtualAssetManagerMock, 
                                                  imageManagerMock.Object, 
                                                  catalogManagerMock,
                                                  channelManagerMock.Object,
                                                  externalChannelManagerMock.Object,
                                                  categoryCacheMock.Object,
                                                  catalogPartnerConfigMock.Object);
            
            var categoryTreeResponse = handler.GetTreeByVersion(fixture.Create<ContextData>(), versionId, deviceFamilyId);
            Assert.That(categoryTreeResponse.Status.Code, Is.EqualTo((int)eResponseStatus.OK));
            if (versionId.HasValue)
            {
                Assert.That(categoryTreeResponse.Object.VersionId.Value, Is.EqualTo(versionId.Value));
            }
            else
            {
                Assert.That(categoryTreeResponse.Object.VersionId.Value, Is.EqualTo(defaultVersions[0].Id));
            }
        }

        private static IEnumerable GetTreeByVersionTestCases()
        {
            Fixture fixture = new Fixture();

            // get tree by default 
            var categoryVersion1 = fixture.Create<CategoryVersion>();
            categoryVersion1.State = CategoryVersionState.Default;
            var treeId1 = categoryVersion1.TreeId;
            var categoryItem1 = fixture.Create<CategoryItem>();
            categoryItem1.Id = categoryVersion1.CategoryItemRootId;
            categoryItem1.ParentId = null;
            categoryItem1.VersionId = categoryVersion1.Id;

            Dictionary<long, CategoryItem> children1 = new Dictionary<long, CategoryItem>();
            foreach (var item in categoryItem1.ChildrenIds)
            {
                var child = fixture.Create<CategoryItem>();
                child.Id = item;
                child.ChildrenIds = null;
                child.ParentId = categoryItem1.Id;
                child.VersionId = categoryItem1.VersionId;

                if (!children1.ContainsKey(child.Id))
                {
                    children1.Add(child.Id, child);
                }
            }
            
            yield return new TestCaseData(treeId1,
                                          new List<CategoryVersion>() { categoryVersion1 },
                                          null,
                                          null, // deviceFamily
                                          null,
                                          categoryItem1,
                                          children1)
                .SetName("GetTreeByVersion_Default");

            // get tree by versionId
            var categoryVersion2 = fixture.Create<CategoryVersion>();
            var treeId2 = categoryVersion2.TreeId;
            var categoryItem2 = fixture.Create<CategoryItem>();
            categoryItem2.Id = categoryVersion2.CategoryItemRootId;
            categoryItem2.ParentId = null;
            categoryItem2.VersionId = categoryVersion2.Id;

            Dictionary<long, CategoryItem> children2 = new Dictionary<long, CategoryItem>();
            foreach (var item in categoryItem2.ChildrenIds)
            {
                var child = fixture.Create<CategoryItem>();
                child.Id = item;
                child.ChildrenIds = null;
                child.ParentId = categoryItem2.Id;
                child.VersionId = categoryItem2.VersionId;

                if (!children2.ContainsKey(child.Id))
                {
                    children2.Add(child.Id, child);
                }
            }

            yield return new TestCaseData(treeId2,
                                          null,
                                          categoryVersion2.Id,
                                          null, // deviceFamily
                                          categoryVersion2,
                                          categoryItem2,
                                          children2)
                .SetName("GetTreeByVersion_Version");

            // get tree by device family 
            var categoryVersion3 = fixture.Create<CategoryVersion>();
            categoryVersion3.State = CategoryVersionState.Default;
            var treeId3 = categoryVersion3.TreeId;
            var categoryItem3 = fixture.Create<CategoryItem>();
            categoryItem3.Id = categoryVersion3.CategoryItemRootId;
            categoryItem3.ParentId = null;
            categoryItem3.VersionId = categoryVersion3.Id;

            Dictionary<long, CategoryItem> children3 = new Dictionary<long, CategoryItem>();
            foreach (var item in categoryItem3.ChildrenIds)
            {
                var child = fixture.Create<CategoryItem>();
                child.Id = item;
                child.ChildrenIds = null;
                child.ParentId = categoryItem3.Id;
                child.VersionId = categoryItem3.VersionId;

                if (!children3.ContainsKey(child.Id))
                {
                    children3.Add(child.Id, child);
                }
            }

            yield return new TestCaseData(treeId3,
                                          new List<CategoryVersion>() { categoryVersion3 },
                                          null,
                                          fixture.Create<int>(), // deviceFamily
                                          null,
                                          categoryItem3,
                                          children3)
                .SetName("GetTreeByVersion_deviceFamily");
        }

        [TestCaseSource(nameof(DeleteTestCases))]
        public void CheckDelete(eResponseStatus exceptedResponse, CategoryItem categoryItem, CategoryVersion categoryVersion)
        {
            Fixture fixture = new Fixture();

            var categoryCacheMock = new Mock<ICategoryCache>();
            var categoryItemResponse = new GenericResponse<CategoryItem>(Status.Ok, categoryItem);
            categoryCacheMock.Setup(x => x.GetCategoryItem(It.IsAny<int>(), It.IsAny<long>()))
                           .Returns(categoryItemResponse);
            
            var categoryVersionResponse = new GenericResponse<CategoryVersion>(Status.Ok, categoryVersion);
            categoryCacheMock.Setup(x => x.GetCategoryVersion(It.IsAny<int>(), It.IsAny<long>()))
                           .Returns(categoryVersionResponse);

            categoryCacheMock.Setup(x => x.DeleteCategoryItem(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>(), It.IsAny<bool>()))
                           .Returns(true);

            categoryCacheMock.Setup(x => x.GetCategoryItemSuccessors(It.IsAny<int>(), It.IsAny<long>()))
                             .Returns(new List<long>());

            categoryCacheMock.Setup(x => x.InvalidateCategoryItem(It.IsAny<int>(), It.IsAny<long>()));

            var virtualAssetManagerMock = new Mock<IVirtualAssetManager>();
            virtualAssetManagerMock.Setup(x => x.DeleteVirtualAsset(It.IsAny<int>(), It.IsAny<VirtualAssetInfo>()))
                                   .Returns(new VirtualAssetInfoResponse() { Status = VirtualAssetInfoStatus.OK });

            var imageManagerMock = new Mock<IImageManager>();
            imageManagerMock.Setup(x => x.GetImagesByObject(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<eAssetImageType>(), It.IsAny<bool?>()))
                                   .Returns(new GenericListResponse<Image>(Status.Ok, new List<Image>()));

            var catalogManagerMock = Mock.Of<ICatalogManager>();
            var virtualAssetPartnerManagerMock = Mock.Of<IVirtualAssetPartnerConfigManager>();
            var channelManagerMock = Mock.Of<IChannelManager>();
            var externalChannelManagerMock = Mock.Of<IExternalChannelManager>();
            var catalogPartnerConfigMock = Mock.Of<ICatalogPartnerConfigManager>();
            var handler = new CategoryItemHandler(virtualAssetPartnerManagerMock,
                                                  virtualAssetManagerMock.Object,
                                                  imageManagerMock.Object,
                                                  catalogManagerMock,
                                                  channelManagerMock,
                                                  externalChannelManagerMock,
                                                  categoryCacheMock.Object,
                                                  catalogPartnerConfigMock);

            var response = handler.Delete(fixture.Create<ContextData>(), categoryItem.Id);
            Assert.That(response.Code, Is.EqualTo((int)exceptedResponse));
        }

        private static IEnumerable DeleteTestCases()
        {
            Fixture fixture = new Fixture();

            // delete with no version
            var categoryItem1 = fixture.Create<CategoryItem>();
            categoryItem1.VersionId = null;
            CategoryVersion categoryVersion1 = null;
            yield return new TestCaseData(eResponseStatus.OK, categoryItem1, categoryVersion1).SetName("Delete_NoVersion");

            //delete draft version
            var categoryItem2 = fixture.Create<CategoryItem>();
            var categoryVersion2 = fixture.Create<CategoryVersion>();
            categoryVersion2.State = CategoryVersionState.Draft;
            yield return new TestCaseData(eResponseStatus.OK, categoryItem2, categoryVersion2).SetName("Delete_DraftVersion");

            // delete default version
            var categoryItem3 = fixture.Create<CategoryItem>();
            var categoryVersion3 = fixture.Create<CategoryVersion>();
            categoryVersion3.State = CategoryVersionState.Default;
            yield return new TestCaseData(eResponseStatus.CategoryVersionIsNotDraft, categoryItem3, categoryVersion3).SetName("Delete_DefaultVersion");

            // delete Released version
            var categoryItem4 = fixture.Create<CategoryItem>();
            var categoryVersion4 = fixture.Create<CategoryVersion>();
            categoryVersion4.State = CategoryVersionState.Released;
            yield return new TestCaseData(eResponseStatus.CategoryVersionIsNotDraft, categoryItem4, categoryVersion4).SetName("Delete_ReleasedVersion");

            // delete root item of version
            var categoryItem5 = fixture.Create<CategoryItem>();
            categoryItem5.ParentId = null;
            var categoryVersion5 = fixture.Create<CategoryVersion>();
            categoryVersion5.State = CategoryVersionState.Draft;
            yield return new TestCaseData(eResponseStatus.CategoryItemIsRoot, categoryItem5, categoryVersion5).SetName("Delete_RootItemOfVersion");
        }

        [TestCaseSource(nameof(UpdateTestCases))]
        public void CheckUpdate(eResponseStatus exceptedResponse, 
                                CategoryItem oldCategoryItem,
                                CategoryItem newCategoryItem,
                                CategoryVersion categoryVersion, 
                                Dictionary<long, CategoryParentCache> groupChildCategories)
        {
            Fixture fixture = new Fixture();

            var categoryCacheMock = new Mock<ICategoryCache>();
            var categoryItemResponse = new GenericResponse<CategoryItem>(Status.Ok, oldCategoryItem);
            categoryCacheMock.Setup(x => x.GetCategoryItem(It.IsAny<int>(), It.IsAny<long>()))
                           .Returns(categoryItemResponse);

            var categoryVersionResponse = new GenericResponse<CategoryVersion>(Status.Ok, categoryVersion);
            categoryCacheMock.Setup(x => x.GetCategoryVersion(It.IsAny<int>(), It.IsAny<long>()))
                           .Returns(categoryVersionResponse);

            categoryCacheMock.Setup(x => x.GetGroupCategoriesIds(It.IsAny<int>(), It.IsAny<List<long>>(), It.IsAny<bool>()))
                             .Returns(groupChildCategories);

            var categoryItemSuccessors = new List<long>();
            categoryCacheMock.Setup(x => x.GetCategoryItemSuccessors(It.IsAny<int>(), It.IsAny<long>()))
                             .Returns(categoryItemSuccessors);

            categoryCacheMock.Setup(x => x.UpdateCategory(It.IsAny<int>(), It.IsAny<long?>(), It.IsAny<List<KeyValuePair<long, string>>>(), It.IsAny<CategoryItem>()))
                             .Returns(true);

            categoryCacheMock.Setup(x => x.UpdateCategoryOrderNum(It.IsAny<int>(), It.IsAny<long?>(), It.IsAny<long>(), It.IsAny<long?>(), It.IsAny<List<long>>(), It.IsAny<List<long>>()))
                             .Returns(true);

            var externalChannelManagerMock = new Mock<IExternalChannelManager>();
            var externalChannelResponse = new GenericResponse<ExternalChannel>(Status.Ok, fixture.Create<ExternalChannel>());
            externalChannelManagerMock.Setup(x => x.GetChannelById(It.IsAny<ContextData>(), It.IsAny<int>(), It.IsAny<bool>()))
                             .Returns(externalChannelResponse);

            var channelManagerMock = new Mock<IChannelManager>();
            fixture.Customizations.Add(new TypeRelay(typeof(ApiObjects.SearchObjects.BooleanPhraseNode), typeof(ApiObjects.SearchObjects.BooleanLeaf)));
            var channelResponse = new GenericResponse<Channel>(Status.Ok, fixture.Create<Channel>());
            channelManagerMock.Setup(x => x.GetChannelById(It.IsAny<ContextData>(), It.IsAny<int>(), It.IsAny<bool>()))
                             .Returns(channelResponse);

            var virtualAssetManagerMock = new Mock<IVirtualAssetManager>();
            virtualAssetManagerMock.Setup(x => x.UpdateVirtualAsset(It.IsAny<int>(), It.IsAny<VirtualAssetInfo>()))
                                   .Returns(new VirtualAssetInfoResponse() { Status = VirtualAssetInfoStatus.OK });

            var catalogManagerMock = Mock.Of<ICatalogManager>();
            var virtualAssetPartnerManagerMock = Mock.Of<IVirtualAssetPartnerConfigManager>();
            var catalogPartnerConfigMock = Mock.Of<ICatalogPartnerConfigManager>();
            var imageManagerMock = Mock.Of<IImageManager>();
            var handler = new CategoryItemHandler(virtualAssetPartnerManagerMock,
                                                  virtualAssetManagerMock.Object,
                                                  imageManagerMock,
                                                  catalogManagerMock,
                                                  channelManagerMock.Object,
                                                  externalChannelManagerMock.Object,
                                                  categoryCacheMock.Object,
                                                  catalogPartnerConfigMock);

            var response = handler.Update(fixture.Create<ContextData>(), newCategoryItem);
            Assert.That(response.Status.Code, Is.EqualTo((int)exceptedResponse));
        }

        private static IEnumerable UpdateTestCases()
        {
            Fixture fixture = new Fixture();

            // update with no version
            var oldCategoryItem1 = fixture.Create<CategoryItem>();
            oldCategoryItem1.NamesInOtherLanguages = null;
            oldCategoryItem1.VersionId = null;

            var newCategoryItem1 = fixture.Create<CategoryItem>();
            newCategoryItem1.NamesInOtherLanguages = null;

            CategoryVersion categoryVersion1 = null;

            var groupChildCategories1 = new Dictionary<long, CategoryParentCache>();
            var childCategory1 = fixture.Create<CategoryParentCache>();
            childCategory1.ParentId = 0;
            childCategory1.VersionId = null;
            foreach (var id in newCategoryItem1.ChildrenIds)
            {
                groupChildCategories1.Add(id, childCategory1);
            }

            yield return new TestCaseData(eResponseStatus.OK,
                                          oldCategoryItem1,
                                          newCategoryItem1, 
                                          categoryVersion1, 
                                          groupChildCategories1)
                .SetName("Update_NoVersion");

            //update draft version
            var oldCategoryItem2 = fixture.Create<CategoryItem>();
            oldCategoryItem2.NamesInOtherLanguages = null;

            var newCategoryItem2 = fixture.Create<CategoryItem>();
            newCategoryItem2.NamesInOtherLanguages = null;

            var categoryVersion2 = fixture.Create<CategoryVersion>();
            categoryVersion2.State = CategoryVersionState.Draft;

            var groupChildCategories2 = new Dictionary<long, CategoryParentCache>();
            var childCategory2 = fixture.Create<CategoryParentCache>();
            childCategory2.ParentId = 0;
            childCategory2.VersionId = null;
            foreach (var id in newCategoryItem2.ChildrenIds)
            {
                groupChildCategories2.Add(id, childCategory2);
            }

            yield return new TestCaseData(eResponseStatus.OK,
                                          oldCategoryItem2,
                                          newCategoryItem2, 
                                          categoryVersion2, 
                                          groupChildCategories2)
                .SetName("Update_DraftVersion");

            // update default version
            var oldCategoryItem3 = fixture.Create<CategoryItem>();
            oldCategoryItem3.NamesInOtherLanguages = null;

            var newCategoryItem3 = fixture.Create<CategoryItem>();
            newCategoryItem3.NamesInOtherLanguages = null;

            var categoryVersion3 = fixture.Create<CategoryVersion>();
            categoryVersion3.State = CategoryVersionState.Default;

            var groupChildCategories3 = new Dictionary<long, CategoryParentCache>();
            var childCategory3 = fixture.Create<CategoryParentCache>();
            childCategory3.ParentId = 0;
            childCategory3.VersionId = null;
            foreach (var id in newCategoryItem3.ChildrenIds)
            {
                groupChildCategories3.Add(id, childCategory3);
            }

            yield return new TestCaseData(eResponseStatus.CategoryVersionIsNotDraft,
                                          oldCategoryItem3,
                                          newCategoryItem3, 
                                          categoryVersion3, 
                                          groupChildCategories3)
                .SetName("Update_DefaultVersion");

            // update Released version
            var oldCategoryItem4 = fixture.Create<CategoryItem>();
            oldCategoryItem4.NamesInOtherLanguages = null;

            var newCategoryItem4 = fixture.Create<CategoryItem>();
            newCategoryItem4.NamesInOtherLanguages = null;

            var categoryVersion4 = fixture.Create<CategoryVersion>();
            categoryVersion4.State = CategoryVersionState.Released;

            var groupChildCategories4 = new Dictionary<long, CategoryParentCache>();
            var childCategory4 = fixture.Create<CategoryParentCache>();
            childCategory4.ParentId = 0;
            childCategory4.VersionId = null;
            foreach (var id in newCategoryItem4.ChildrenIds)
            {
                groupChildCategories4.Add(id, childCategory4);
            }

            yield return new TestCaseData(eResponseStatus.CategoryVersionIsNotDraft,
                                          oldCategoryItem4,
                                          newCategoryItem4, 
                                          categoryVersion4,
                                          groupChildCategories4)
                .SetName("Update_ReleasedVersion");

            // update with child Category Is Already Associated To other Version
            var oldCategoryItem5 = fixture.Create<CategoryItem>();
            oldCategoryItem5.NamesInOtherLanguages = null;

            var newCategoryItem5 = fixture.Create<CategoryItem>();
            newCategoryItem5.NamesInOtherLanguages = null;

            var categoryVersion5 = fixture.Create<CategoryVersion>();
            categoryVersion5.State = CategoryVersionState.Draft;

            var groupChildCategories5 = new Dictionary<long, CategoryParentCache>();
            var childCategory5 = fixture.Create<CategoryParentCache>();
            childCategory5.ParentId = 0;
            foreach (var id in newCategoryItem5.ChildrenIds)
            {
                groupChildCategories5.Add(id, childCategory5);
            }

            yield return new TestCaseData(eResponseStatus.CategoryIsAlreadyAssociatedToVersion,
                                          oldCategoryItem5,
                                          newCategoryItem5, 
                                          categoryVersion5, 
                                          groupChildCategories5)
                .SetName("Update_ChildCategoryAssociatedToOtherVersion");

            // update with child Category Is Already Associated To same Version
            var oldCategoryItem6 = fixture.Create<CategoryItem>();
            oldCategoryItem6.NamesInOtherLanguages = null;

            var newCategoryItem6 = fixture.Create<CategoryItem>();
            newCategoryItem6.NamesInOtherLanguages = null;

            var categoryVersion6 = fixture.Create<CategoryVersion>();
            categoryVersion6.State = CategoryVersionState.Draft;

            var groupChildCategories6 = new Dictionary<long, CategoryParentCache>();
            var childCategory6 = fixture.Create<CategoryParentCache>();
            childCategory6.ParentId = 0;
            childCategory6.VersionId = oldCategoryItem6.VersionId;
            foreach (var id in newCategoryItem6.ChildrenIds)
            {
                groupChildCategories6.Add(id, childCategory6);
            }

            yield return new TestCaseData(eResponseStatus.OK,
                                          oldCategoryItem6,
                                          newCategoryItem6, 
                                          categoryVersion6, 
                                          groupChildCategories6)
                .SetName("Update_ChildCategoryAssociatedToSameVersion");
        }
    }
}
