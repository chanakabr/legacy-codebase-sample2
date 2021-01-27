using ApiLogic.Catalog;
using ApiObjects.Base;
using ApiObjects.Catalog;
using ApiObjects.Response;
using AutoFixture;
using Core.Catalog.CatalogManagement;
using Moq;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TVinciShared;

namespace ApiLogic.Tests.Catalog.Category
{
    [TestFixture]
    public class CategoryVersionHandlerTests
    {
        [Test]
        public void CheckCreateTree()
        {
            Fixture fixture = new Fixture();
            var contextData = fixture.Create<ContextData>();
            var categoryItem = fixture.Create<CategoryItem>();
            categoryItem.VersionId = null;
            categoryItem.ParentId = null;

            var categoryCacheMock = new Mock<ICategoryCache>();
            categoryCacheMock.Setup(x => x.GetCategoryItem(contextData.GroupId, categoryItem.Id))
                             .Returns(new GenericResponse<CategoryItem>(Status.Ok, categoryItem));

            var categoryVersion = fixture.Create<CategoryVersion>();
            categoryVersion.BaseVersionId = 0;
            categoryVersion.CategoryItemRootId = categoryItem.Id;
            categoryVersion.State = CategoryVersionState.Default;
            categoryCacheMock.Setup(x => x.AddCategoryVersion(It.IsAny<int>(), It.IsAny<CategoryVersion>()))
                             .Returns(new GenericResponse<CategoryVersion>(Status.Ok, categoryVersion));

            categoryCacheMock.Setup(x => x.GetCategoryVersion(It.IsAny<int>(), It.IsAny<long>()))
                             .Returns(new GenericResponse<CategoryVersion>(Status.Ok, categoryVersion));

            categoryCacheMock.Setup(x => x.SetDefault(It.IsAny<ContextData>(), It.IsAny<long>(), It.IsAny<long>()))
                             .Returns(true);

            var categoryItemManager = Mock.Of<ICategoryItemManager>();
            CategoryVersionHandler handler = new CategoryVersionHandler(categoryCacheMock.Object, categoryItemManager);

            // call categoryVersion.createTree
            var name = fixture.Create<string>();
            var comment = fixture.Create<string>();
            GenericResponse<CategoryVersion> categoryVersionResponse = handler.CreateTree(contextData, categoryItem.Id, name, comment);

            categoryCacheMock.Verify(foo => foo.AddCategoryVersion(It.IsAny<int>(), It.IsAny<CategoryVersion>()), Times.Once());
            categoryCacheMock.Verify(foo => foo.SetDefault(It.IsAny<ContextData>(), It.IsAny<long>(), 0), Times.Once());
        }

        [TestCaseSource(nameof(CreateTreeErrorsTestCases))]
        public void CheckCreateTreeErrors(GenericResponse<CategoryItem> categoryItemResponse, eResponseStatus expectedError)
        {
            Fixture fixture = new Fixture();
            var categoryCacheMock = new Mock<ICategoryCache>();

            categoryCacheMock.Setup(x => x.GetCategoryItem(It.IsAny<int>(), It.IsAny<long>()))
                             .Returns(categoryItemResponse);

            var existCategoryVersionResponse = new GenericResponse<CategoryVersion>(Status.Ok, fixture.Create<CategoryVersion>());
            categoryCacheMock.Setup(x => x.GetCategoryVersion(It.IsAny<int>(), It.IsAny<long>()))
                             .Returns(existCategoryVersionResponse);

            var categoryItemManager = Mock.Of<ICategoryItemManager>();
            CategoryVersionHandler handler = new CategoryVersionHandler(categoryCacheMock.Object, categoryItemManager);

            // call categoryVersion.createTree
            GenericResponse<CategoryVersion> categoryVersionResponse = handler.CreateTree(fixture.Create<ContextData>(), It.IsAny<long>(), fixture.Create<string>(), fixture.Create<string>());
            
            Assert.That(categoryVersionResponse.Status.Code, Is.EqualTo((int)expectedError));
            categoryCacheMock.Verify(foo => foo.AddCategoryVersion(It.IsAny<int>(), It.IsAny<CategoryVersion>()), Times.Never());
            categoryCacheMock.Verify(foo => foo.SetDefault(It.IsAny<ContextData>(), It.IsAny<long>(), 0), Times.Never());
        }

        private static IEnumerable CreateTreeErrorsTestCases()
        {
            Fixture fixture = new Fixture();

            // CategoryIsAlreadyAssociatedToVersion
            CategoryItem categoryItem1 = fixture.Create<CategoryItem>();
            categoryItem1.ParentId = null;
            yield return new TestCaseData(new GenericResponse<CategoryItem>(Status.Ok, categoryItem1), 
                eResponseStatus.CategoryIsAlreadyAssociatedToVersionTree).SetName("CreateTreeError_CategoryIsAlreadyAssociatedToVersion");

            // CategoryIsNotRoot
            CategoryItem categoryItem2 = fixture.Create<CategoryItem>();
            categoryItem2.VersionId = null;
            yield return new TestCaseData(new GenericResponse<CategoryItem>(Status.Ok, categoryItem2), 
                eResponseStatus.CategoryIsNotRoot).SetName("CreateTreeError_CategoryIsNotRoot");

            // CategoryIsNotRoot
            yield return new TestCaseData(new GenericResponse<CategoryItem>(Status.Error, new CategoryItem()), 
                eResponseStatus.Error).SetName("CreateTreeError_CategoryIsNotRoot");
        }

        [Test]
        public void CheckAdd()
        {
            // create valid category version 
            Fixture fixture = new Fixture();
            var now = DateUtils.GetUtcUnixTimestampNow();

            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var categoryTree = fixture.Create<CategoryTree>();
            categoryTree.VersionId = null;

            var categoryVersionToAdd = new CategoryVersion()
            {
                BaseVersionId = fixture.Create<long>(),
                Name = fixture.Create<string>(),
                Comment = fixture.Create<string>()
            };

            var baseCategoryVersion = fixture.Create<CategoryVersion>();
            baseCategoryVersion.Id = categoryVersionToAdd.BaseVersionId;

            var categoryItemManager = new Mock<ICategoryItemManager>();
            categoryItemManager.Setup(x => x.Duplicate(It.IsAny<int>(), It.IsAny<long>(), baseCategoryVersion.CategoryItemRootId, null))
                               .Returns(new GenericResponse<CategoryTree>(Status.Ok, categoryTree));

            var categoryCacheMock = new Mock<ICategoryCache>();
            categoryCacheMock.Setup(x => x.GetCategoryVersion(It.IsAny<int>(), categoryVersionToAdd.BaseVersionId))
                             .Returns(new GenericResponse<CategoryVersion>(Status.Ok, baseCategoryVersion));

            categoryCacheMock.Setup(x => x.AddCategoryVersion(It.IsAny<int>(), It.IsAny<CategoryVersion>()))
                            .Callback<int, CategoryVersion>((groupId, version) =>
                            {
                                // validate all params ar correct in response
                                Assert.That(version.Id, Is.AtLeast(1));
                                Assert.That(version.Name, Is.EqualTo(categoryVersionToAdd.Name));
                                Assert.That(version.Comment, Is.EqualTo(categoryVersionToAdd.Comment));
                                Assert.That(version.TreeId, Is.EqualTo(baseCategoryVersion.TreeId));
                                Assert.That(version.State, Is.EqualTo(CategoryVersionState.Draft));
                                Assert.That(version.BaseVersionId, Is.EqualTo(baseCategoryVersion.Id));
                                Assert.That(version.CategoryItemRootId, Is.EqualTo(categoryTree.Id));
                                Assert.That(version.DefaultDate, Is.Null);
                                Assert.That(version.UpdaterId, Is.AtLeast(1));
                                Assert.That(version.CreateDate, Is.AtLeast(now));
                                Assert.That(version.UpdateDate, Is.EqualTo(version.CreateDate));
                            });

            CategoryVersionHandler handler = new CategoryVersionHandler(categoryCacheMock.Object, categoryItemManager.Object);

            // call categoryVersion.add
            GenericResponse<CategoryVersion> categoryVersionResponse = handler.Add(fixture.Create<ContextData>(), categoryVersionToAdd);
            categoryCacheMock.Verify(foo => foo.AddCategoryVersion(It.IsAny<int>(), It.IsAny<CategoryVersion>()), Times.Once());
        }

        [TestCaseSource(nameof(AddErrorsTestCases))]
        public void CheckAddErrors(GenericResponse<CategoryTree> duplicateResponse, GenericResponse<CategoryVersion> categoryVersionResponse)
        {
            // call categoryVersion.add with non-existing version
            Fixture fixture = new Fixture();
            var categoryItemManager = new Mock<ICategoryItemManager>();
            categoryItemManager.Setup(x => x.Duplicate(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>(), null))
                               .Returns(duplicateResponse);

            var categoryCacheMock = new Mock<ICategoryCache>();
            categoryCacheMock.Setup(x => x.GetCategoryVersion(It.IsAny<int>(), It.IsAny<long>()))
                             .Returns(categoryVersionResponse);

            
            CategoryVersionHandler handler = new CategoryVersionHandler(categoryCacheMock.Object, categoryItemManager.Object);

            // call categoryVersion.createTree
            GenericResponse<CategoryVersion> addResponse = handler.Add(fixture.Create<ContextData>(), fixture.Create<CategoryVersion>());

            Assert.That(addResponse.Status.Code, Is.EqualTo((int)eResponseStatus.Error));
            categoryCacheMock.Verify(foo => foo.AddCategoryVersion(It.IsAny<int>(), It.IsAny<CategoryVersion>()), Times.Never());
        }

        private static IEnumerable AddErrorsTestCases()
        {
            // DuplicateError
            var duplicateResponse1 = new GenericResponse<CategoryTree>(Status.Error);
            var categoryVersionResponse1 = new GenericResponse<CategoryVersion>(Status.Ok, new CategoryVersion());
            yield return new TestCaseData(duplicateResponse1, categoryVersionResponse1).SetName("AddError_DuplicateError");

            // CategoryVersionNotExist
            var duplicateResponse2 = new GenericResponse<CategoryTree>(Status.Ok, new CategoryTree());
            var categoryVersionResponse2 = new GenericResponse<CategoryVersion>(Status.Error);
            yield return new TestCaseData(duplicateResponse2, categoryVersionResponse2).SetName("AddError_CategoryVersionNotExist");
        }

        [TestCaseSource(nameof(SetDefaultTestCases))]
        public void CheckSetDefault(CategoryVersion categoryVersion, bool force)
        {
            Fixture fixture = new Fixture();

            var categoryCacheMock = new Mock<ICategoryCache>();
            categoryCacheMock.Setup(x => x.GetCategoryVersion(It.IsAny<int>(), It.IsAny<long>()))
                             .Returns(new GenericResponse<CategoryVersion>(Status.Ok, categoryVersion));

            var defaultList = new List<CategoryVersion>() { fixture.Create<CategoryVersion>() };
            defaultList[0].State = CategoryVersionState.Default;
            defaultList[0].TreeId = categoryVersion.TreeId;
            defaultList[0].DefaultDate = DateUtils.GetUtcUnixTimestampNow();

            categoryCacheMock.Setup(x => x.ListCategoryVersionDefaults(It.IsAny<ContextData>(), null, null))
                             .Returns(new GenericListResponse<CategoryVersion>(Status.Ok, defaultList));

            var categoryItemManager = Mock.Of<ICategoryItemManager>();
            CategoryVersionHandler handler = new CategoryVersionHandler(categoryCacheMock.Object, categoryItemManager);

            // call categoryVersion.setDefault
            var status = handler.SetDefault(fixture.Create<ContextData>(), fixture.Create<long>(), force);
            categoryCacheMock.Verify(foo => foo.SetDefault(It.IsAny<ContextData>(), It.IsAny<long>(), It.IsAny<long>()), Times.Once());
        }
        
        private static IEnumerable SetDefaultTestCases()
        {
            Fixture fixture = new Fixture();

            // valid date range
            var categoryVersion1 = fixture.Create<CategoryVersion>();
            categoryVersion1.State = CategoryVersionState.Draft;
            categoryVersion1.CreateDate = DateUtils.GetUtcUnixTimestampNow() + 1000;
            yield return new TestCaseData(categoryVersion1, false).SetName("SetDefault_ValidCreateDate");

            // in valid date range
            var categoryVersion2 = fixture.Create<CategoryVersion>();
            categoryVersion2.State = CategoryVersionState.Draft;
            categoryVersion2.CreateDate = DateUtils.GetUtcUnixTimestampNow() - 1000;
            yield return new TestCaseData(categoryVersion2, true).SetName("SetDefault_InvalidCreateDate");
        }

        [TestCaseSource(nameof(SetDefaultErrorsTestCases))]
        public void CheckSetDefaultErrors(GenericResponse<CategoryVersion> categoryVersion, bool force, eResponseStatus expectedError)
        {
            Fixture fixture = new Fixture();

            var categoryCacheMock = new Mock<ICategoryCache>();
            categoryCacheMock.Setup(x => x.GetCategoryVersion(It.IsAny<int>(), It.IsAny<long>()))
                             .Returns(categoryVersion);

            var defaultList = new List<CategoryVersion>() { fixture.Create<CategoryVersion>() };
            defaultList[0].State = CategoryVersionState.Default;
            defaultList[0].TreeId = categoryVersion.Object.TreeId;
            defaultList[0].DefaultDate = DateUtils.GetUtcUnixTimestampNow();

            categoryCacheMock.Setup(x => x.ListCategoryVersionDefaults(It.IsAny<ContextData>(), null, null))
                             .Returns(new GenericListResponse<CategoryVersion>(Status.Ok, defaultList));

            var categoryItemManager = Mock.Of<ICategoryItemManager>();
            CategoryVersionHandler handler = new CategoryVersionHandler(categoryCacheMock.Object, categoryItemManager);

            // call categoryVersion.setDefault
            var setDefaultStatus = handler.SetDefault(fixture.Create<ContextData>(), fixture.Create<long>(), force);
            Assert.That(setDefaultStatus.Code, Is.EqualTo((int)expectedError));
            categoryCacheMock.Verify(foo => foo.SetDefault(It.IsAny<ContextData>(), It.IsAny<long>(), It.IsAny<long>()), Times.Never());
        }

        private static IEnumerable SetDefaultErrorsTestCases()
        {
            Fixture fixture = new Fixture();

            // non-existing version
            yield return new TestCaseData(new GenericResponse<CategoryVersion>(Status.Error, new CategoryVersion()), false, 
                eResponseStatus.Error).SetName("SetDefaultError_NonExistingVersion");

            // old version
            var categoryVersion1 = fixture.Create<CategoryVersion>();
            categoryVersion1.State = CategoryVersionState.Draft;
            categoryVersion1.CreateDate = DateUtils.GetUtcUnixTimestampNow() - 1000;
            yield return new TestCaseData(new GenericResponse<CategoryVersion>(Status.Ok, categoryVersion1), false, 
                eResponseStatus.CategoryVersionIsOlderThanDefault).SetName("SetDefaultError_OldVersion");

            // vaersion is in default state
            var categoryVersion2 = fixture.Create<CategoryVersion>();
            categoryVersion2.State = CategoryVersionState.Default;
            categoryVersion2.CreateDate = DateUtils.GetUtcUnixTimestampNow() + 1000;
            yield return new TestCaseData(new GenericResponse<CategoryVersion>(Status.Ok, categoryVersion2), false, 
                eResponseStatus.CategoryVersionIsNotDraft).SetName("SetDefaultError_SetDefaultVersion");

            // vaersion is in Released state
            var categoryVersion3 = fixture.Create<CategoryVersion>();
            categoryVersion3.State = CategoryVersionState.Released;
            categoryVersion3.CreateDate = DateUtils.GetUtcUnixTimestampNow() + 1000;
            yield return new TestCaseData(new GenericResponse<CategoryVersion>(Status.Ok, categoryVersion3), false, 
                eResponseStatus.CategoryVersionIsNotDraft).SetName("SetDefaultError_SetReleasedVersion");
        }

        [Test]
        public void CheckDelete()
        {
            Fixture fixture = new Fixture();
            CategoryVersion categoryVersion = fixture.Create<CategoryVersion>();
            categoryVersion.State = CategoryVersionState.Draft;
            var categoryCacheMock = new Mock<ICategoryCache>();
            categoryCacheMock.Setup(x => x.GetCategoryVersion(It.IsAny<int>(), It.IsAny<long>()))
                             .Returns(new GenericResponse<CategoryVersion>(Status.Ok, categoryVersion));

            categoryCacheMock.Setup(x => x.DeleteCategoryVersion(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<CategoryVersion>()))
                             .Returns(Status.Ok);

            var categoryItemManager = Mock.Of<ICategoryItemManager>();
            CategoryVersionHandler handler = new CategoryVersionHandler(categoryCacheMock.Object, categoryItemManager);

            // call categoryVersion.delete
            var status = handler.Delete(fixture.Create<ContextData>(), fixture.Create<long>());
            categoryCacheMock.Verify(foo => foo.DeleteCategoryVersion(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<CategoryVersion>()), Times.Once());
        }

        [TestCaseSource(nameof(DeleteErrorsTestCases))]
        public void CheckDeleteErrors(GenericResponse<CategoryVersion> categoryVersionResponse, eResponseStatus expectedError)
        {
            Fixture fixture = new Fixture();
            var categoryCacheMock = new Mock<ICategoryCache>();
            categoryCacheMock.Setup(x => x.GetCategoryVersion(It.IsAny<int>(), It.IsAny<long>()))
                             .Returns(categoryVersionResponse);

            var categoryItemManager = Mock.Of<ICategoryItemManager>();
            CategoryVersionHandler handler = new CategoryVersionHandler(categoryCacheMock.Object, categoryItemManager);

            // call categoryVersion.delete
            var deleteStatus = handler.Delete(fixture.Create<ContextData>(), fixture.Create<long>());
            Assert.That(deleteStatus.Code, Is.EqualTo((int)expectedError));
            categoryCacheMock.Verify(foo => foo.DeleteCategoryVersion(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<CategoryVersion>()), Times.Never());
        }

        private static IEnumerable DeleteErrorsTestCases()
        {
            Fixture fixture = new Fixture();

            // non-existing version
            yield return new TestCaseData(new GenericResponse<CategoryVersion>(Status.Error, new CategoryVersion()), 
                eResponseStatus.Error).SetName("DeleteError_NonExistingVersion");

            // default state
            var categoryVersion1 = fixture.Create<CategoryVersion>();
            categoryVersion1.State = CategoryVersionState.Default;
            yield return new TestCaseData(new GenericResponse<CategoryVersion>(Status.Ok, categoryVersion1), 
                eResponseStatus.CategoryVersionIsNotDraft).SetName("DeleteError_DefaultState");

            // Released state
            var categoryVersion2 = fixture.Create<CategoryVersion>();
            categoryVersion1.State = CategoryVersionState.Released;
            yield return new TestCaseData(new GenericResponse<CategoryVersion>(Status.Ok, categoryVersion2), 
                eResponseStatus.CategoryVersionIsNotDraft).SetName("DeleteError_ReleasedState");
        }

        [TestCaseSource(nameof(UpdateTestCases))]
        public void CheckUpdate(CategoryVersion objectToUpdate, bool needTupdate)
        {
            // create valid category version 
            Fixture fixture = new Fixture();
            
            var oldCategoryVersion = fixture.Create<CategoryVersion>();
            oldCategoryVersion.State = CategoryVersionState.Draft;

            var categoryCacheMock = new Mock<ICategoryCache>();
            categoryCacheMock.Setup(x => x.GetCategoryVersion(It.IsAny<int>(), It.IsAny<long>()))
                             .Returns(new GenericResponse<CategoryVersion>(Status.Ok, oldCategoryVersion));

            var categoryItemManager = new Mock<ICategoryItemManager>();
            
            CategoryVersionHandler handler = new CategoryVersionHandler(categoryCacheMock.Object, categoryItemManager.Object);

            // call categoryVersion.update
            GenericResponse<CategoryVersion> categoryVersionResponse = handler.Update(fixture.Create<ContextData>(), objectToUpdate);
            if (needTupdate)
            {
                categoryCacheMock.Verify(foo => foo.UpdateCategoryVersion(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<CategoryVersion>()), Times.Once());
            }
            else
            {
                categoryCacheMock.Verify(foo => foo.UpdateCategoryVersion(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<CategoryVersion>()), Times.Never());
            }
        }

        private static IEnumerable UpdateTestCases()
        {
            Fixture fixture = new Fixture();

            // update only name
            var categoryVersion1 = fixture.Create<CategoryVersion>();
            categoryVersion1.State = CategoryVersionState.Draft;
            categoryVersion1.Comment = null;
            yield return new TestCaseData(categoryVersion1, true).SetName("Update_Name");

            // update only comment 
            var categoryVersion2 = fixture.Create<CategoryVersion>();
            categoryVersion2.State = CategoryVersionState.Draft;
            categoryVersion2.Name = null;
            yield return new TestCaseData(categoryVersion2, true).SetName("Update_Comment");

            // update name and comment
            var categoryVersion3 = fixture.Create<CategoryVersion>();
            categoryVersion3.State = CategoryVersionState.Draft;
            yield return new TestCaseData(categoryVersion3, true).SetName("Update_NameAndCommanr");

            // update none
            var categoryVersion4 = fixture.Create<CategoryVersion>();
            categoryVersion4.State = CategoryVersionState.Draft;
            categoryVersion4.Name = null;
            categoryVersion4.Comment = null;
            yield return new TestCaseData(categoryVersion4, false).SetName("Update_None");
        }

        [TestCaseSource(nameof(UpdateErrorsTestCases))]
        public void CheckUpdateErrors(GenericResponse<CategoryVersion> categoryVersionResponse, eResponseStatus expectedError)
        {
            Fixture fixture = new Fixture();
            var categoryCacheMock = new Mock<ICategoryCache>();
            categoryCacheMock.Setup(x => x.GetCategoryVersion(It.IsAny<int>(), It.IsAny<long>()))
                             .Returns(categoryVersionResponse);

            var categoryItemManager = Mock.Of<ICategoryItemManager>();
            CategoryVersionHandler handler = new CategoryVersionHandler(categoryCacheMock.Object, categoryItemManager);

            // call categoryVersion.update
            var updateStatus = handler.Update(fixture.Create<ContextData>(), categoryVersionResponse.Object);
            Assert.That(updateStatus.Status.Code, Is.EqualTo((int)expectedError));
            categoryCacheMock.Verify(foo => foo.UpdateCategoryVersion(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<CategoryVersion>()), Times.Never());
        }

        private static IEnumerable UpdateErrorsTestCases()
        {
            Fixture fixture = new Fixture();

            // non-existing version
            yield return new TestCaseData(new GenericResponse<CategoryVersion>(Status.Error, new CategoryVersion()),
                eResponseStatus.Error).SetName("UpdateError_NonExistingVersion");

            // default state
            var categoryVersion1 = fixture.Create<CategoryVersion>();
            categoryVersion1.State = CategoryVersionState.Default;
            yield return new TestCaseData(new GenericResponse<CategoryVersion>(Status.Ok, categoryVersion1),
                eResponseStatus.CategoryVersionIsNotDraft).SetName("UpdateError_DefaultState");

            // Released state
            var categoryVersion2 = fixture.Create<CategoryVersion>();
            categoryVersion1.State = CategoryVersionState.Released;
            yield return new TestCaseData(new GenericResponse<CategoryVersion>(Status.Ok, categoryVersion2),
                eResponseStatus.CategoryVersionIsNotDraft).SetName("UpdateError_ReleasedState");
        }
    }
}