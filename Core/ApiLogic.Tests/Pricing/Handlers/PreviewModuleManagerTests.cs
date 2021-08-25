using ApiLogic.Pricing.Handlers;
using ApiObjects.Base;
using ApiObjects.Pricing.Dto;
using ApiObjects.Response;
using AutoFixture;
using Core.Pricing;
using DAL;
using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using Core.GroupManagers;

namespace ApiLogic.Tests.Pricing.Handlers
{
    [TestFixture]
    public class PreviewModuleManagerTests
    {
        [TestCaseSource(nameof(DeleteCases))]
        public void CheckDelete(DeleteTestCase deleteTestCase)
        {
            Fixture fixture = new Fixture();
            var repositoryMock = new Mock<IPreviewModuleRepository>();
            var CashMock = new Mock<IPreviewModuleCache>();
            var groupSettingsManagerMock = new Mock<IGroupSettingsManager>();
            groupSettingsManagerMock.Setup(x => x.IsOpc(It.IsAny<int>())).Returns(deleteTestCase.IsOPC);
            repositoryMock.Setup(x => x.DeletePreviewModule(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>()))
                                    .Returns(deleteTestCase.IsDeleted);
            CashMock.Setup(x => x.GetGroupPreviewModules(It.IsAny<int>())).Returns(deleteTestCase.PreviewModuleMap);

            PreviewModuleManager manager = new PreviewModuleManager(repositoryMock.Object, CashMock.Object, groupSettingsManagerMock.Object);

            var response = manager.Delete(fixture.Create<ContextData>(), deleteTestCase.IdToDelete);

            Assert.That(response.Code, Is.EqualTo((int)deleteTestCase.ResponseStatus));
        }

        private static IEnumerable DeleteCases()
        {
            yield return new TestCaseData(new DeleteTestCase(eResponseStatus.OK)).SetName("Delete_CheckDeleteSuccess");
            yield return new TestCaseData(new DeleteTestCase(eResponseStatus.AccountIsNotOpcSupported, isOPC: false)).SetName("Delete_CheckNotOpcSupported");
            yield return new TestCaseData(new DeleteTestCase(eResponseStatus.PreviewModuleNotExist, idExist: false)).SetName("Delete_CheckDeleteCodeNotExist");
            yield return new TestCaseData(new DeleteTestCase(eResponseStatus.Error, isDeleted: false)).SetName("Delete_CheckDeleteFailed");
        }

        public class DeleteTestCase
        {
            private static readonly Fixture fixture = new Fixture();
            internal bool IsDeleted { get; private set; }
            internal eResponseStatus ResponseStatus { get; private set; }
            internal Dictionary<long, PreviewModule> PreviewModuleMap { get; private set; }
            internal long IdToDelete { get; private set; }
            internal bool IsOPC { get; private set; }

            public DeleteTestCase(eResponseStatus responseStatus, bool isDeleted = true, bool idExist = true, bool isOPC = true)
            {
                ResponseStatus = responseStatus;
                IsDeleted = isDeleted;
                IsOPC = isOPC;
                PreviewModuleMap = fixture.Create<Dictionary<long, PreviewModule>>();
                foreach (var item in PreviewModuleMap)
                {
                    item.Value.m_nID = item.Key;
                }

                if (idExist)
                {
                    IdToDelete = PreviewModuleMap.First().Key;
                }
                else
                {
                    IdToDelete = fixture.Create<long>();
                    PreviewModuleMap.Remove(IdToDelete);
                }
            }
        }

        [TestCaseSource(nameof(AddCases))]
        public void CheckAdd(eResponseStatus expectedCode, long id, bool isOPC)
        {
            Fixture fixture = new Fixture();
            var repositoryMock = new Mock<IPreviewModuleRepository>();
            var CashMock = Mock.Of<IPreviewModuleCache>();
            repositoryMock.Setup(x => x.InsertPreviewModule(It.IsAny<int>(), It.IsAny<PreviewModuleDTO>(), It.IsAny<long>())).Returns(id);
            var groupSettingsManagerMock = new Mock<IGroupSettingsManager>();
            groupSettingsManagerMock.Setup(x => x.IsOpc(It.IsAny<int>())).Returns(isOPC);
            PreviewModuleManager manager = new PreviewModuleManager(repositoryMock.Object, CashMock, groupSettingsManagerMock.Object);
            var response = manager.Add(fixture.Create<ContextData>(), fixture.Create<PreviewModule>());

            Assert.That(response.Status.Code, Is.EqualTo((int)expectedCode));
        }

        private static IEnumerable AddCases()
        {
            Fixture fixture = new Fixture();
            yield return new TestCaseData(eResponseStatus.OK, fixture.Create<int>() + 1, true).SetName("Add_CheckSuccess");
            yield return new TestCaseData(eResponseStatus.AccountIsNotOpcSupported, fixture.Create<int>() + 1, false).SetName("Add_CheckNotOpcSupported");
            yield return new TestCaseData(eResponseStatus.Error, 0, true).SetName("Add_CheckFailed");
        }

        [TestCaseSource(nameof(ListCases))]
        public void CheckList(ListTestCase listTestCase)
        {
            var repositoryMock = new Mock<IPreviewModuleRepository>();
            var CashMock = new Mock<IPreviewModuleCache>();

            CashMock.Setup(x=> x.GetGroupPreviewModules(It.IsAny<int>())).Returns(listTestCase.PreviewModuleMap);

            PreviewModuleManager manager = new PreviewModuleManager(repositoryMock.Object, CashMock.Object ,Mock.Of<IGroupSettingsManager>());
            var response = manager.GetPreviewModules(It.IsAny<int>(), listTestCase.Filter);

            Assert.That(response.Objects.Count == listTestCase.ReturnListSize);
            Assert.That(response.Status.Code, Is.EqualTo((int)listTestCase.ResponseStatus));
        }

        private static IEnumerable ListCases()
        {
            yield return new TestCaseData(new ListTestCase(isFilterExists: true)).SetName("List_CheckFilterWithExistsIds");
            yield return new TestCaseData(new ListTestCase(isFilterExists: true, isFilterContainsNonExistentId: true)).SetName("List_CheckFilterWithExistsAndNotExistsIds");
            yield return new TestCaseData(new ListTestCase(isFilterExists: true, isFilterContainsExistsId: false, isFilterContainsNonExistentId: true)).SetName("List_Check=FilterWithNonExistentIds");
            yield return new TestCaseData(new ListTestCase(isPreviewModuleExists: false)).SetName("List_CheckEmpty");
            yield return new TestCaseData(new ListTestCase()).SetName("List_CheckSuccess");
        }

        public class ListTestCase
        {
            private static readonly Fixture fixture = new Fixture();
            internal eResponseStatus ResponseStatus { get; private set; }
            internal Dictionary<long, PreviewModule> PreviewModuleMap { get; private set; }
            internal List<long> Filter { get; private set; }
            internal int ReturnListSize { get; private set; }
  

            public ListTestCase(bool isPreviewModuleExists = true, bool isFilterExists = false, bool IsFilterEmpty = false, 
                bool isFilterContainsExistsId = true, bool isFilterContainsNonExistentId = false)
            {
                ResponseStatus = eResponseStatus.OK;
                PreviewModuleMap = new Dictionary<long, PreviewModule>();
                Filter = null;
                if (isPreviewModuleExists)
                {
                    PreviewModuleMap = fixture.Create<Dictionary<long, PreviewModule>>();
                    foreach (var item in PreviewModuleMap)
                    {
                        item.Value.m_nID = item.Key;
                    }
                    ReturnListSize = PreviewModuleMap.Count;
                }
                if (isFilterExists)
                {
                    Filter = new List<long>();
                    if (!IsFilterEmpty)
                    {
                        ReturnListSize = 0;
                        if (isFilterContainsExistsId)
                        {
                            Filter.Add(PreviewModuleMap.First().Key);
                            ReturnListSize = Filter.Count();
                        }
                        if (isFilterContainsNonExistentId)
                        {
                            var idNotExistsInFilter = PreviewModuleMap.Where(x => !Filter.Contains(x.Key)).First().Key;
                            Filter.Add(idNotExistsInFilter);
                            PreviewModuleMap.Remove(idNotExistsInFilter);
                        }
                        
                    }
                }
            }
        }

        [TestCaseSource(nameof(UpdateCases))]
        public void CheckUpdate(UpdateTestCase updateTestCase)
        {
            Fixture fixture = new Fixture();
            var repositoryMock = new Mock<IPreviewModuleRepository>();
            var CashMock = new Mock<IPreviewModuleCache>();
            repositoryMock.Setup(x => x.UpdatePreviewModule(It.IsAny<long>(), It.IsAny<PreviewModuleDTO>(), It.IsAny<long>())).Returns(updateTestCase.UpdatedRows);
            CashMock.Setup(x => x.GetGroupPreviewModules(It.IsAny<int>())).Returns(updateTestCase.PreviewModuleMap);
            var groupSettingsManagerMock = new Mock<IGroupSettingsManager>();
            groupSettingsManagerMock.Setup(x => x.IsOpc(It.IsAny<int>())).Returns(updateTestCase.IsOPC);
            PreviewModuleManager manager = new PreviewModuleManager(repositoryMock.Object, CashMock.Object, groupSettingsManagerMock.Object);
            var response = manager.Update(fixture.Create<ContextData>(), updateTestCase.IdToUpdate, updateTestCase.PreviewModuleToUpdate);
            Assert.That(response.Status.Code, Is.EqualTo((int)updateTestCase.ResponseStatus));
            repositoryMock.Verify(x => x.UpdatePreviewModule(It.IsAny<long>(), It.IsAny<PreviewModuleDTO>(), It.IsAny<long>()), Times.Exactly(updateTestCase.AmountCallToRepository));
        }

        private static IEnumerable UpdateCases()
        {
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.OK)).SetName("Update_CheckSeccess");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.AccountIsNotOpcSupported, needToUpdate: false, isOPC: false)).SetName("Update_CheckNotOpcSupported");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.OK, needToUpdate: false)).SetName("Update_ChecNotNeeded");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.PreviewModuleNotExist, idExists: false, needToUpdate: false)).SetName("Update_CheckPreviewModuleNotExist");
            yield return new TestCaseData(new UpdateTestCase(eResponseStatus.Error, isSucceeded: false)).SetName("Update_CheckError");
        }

        public class UpdateTestCase
        {
            private static readonly Fixture fixture = new Fixture();
            internal eResponseStatus ResponseStatus { get; private set; }
            internal Dictionary<long, PreviewModule> PreviewModuleMap { get; private set; }
            internal long IdToUpdate { get; private set; }
            internal long UpdatedRows { get; private set; }
            internal int AmountCallToRepository { get; private set; }
            internal PreviewModule PreviewModuleToUpdate { get; private set; }
            internal bool IsOPC { get; private set; }

            public UpdateTestCase(eResponseStatus responseStatus, bool isSucceeded = true, bool idExists = true, bool needToUpdate = true, bool isOPC = true)
            {
                ResponseStatus = responseStatus;
                UpdatedRows = isSucceeded ? 1 : 0;
                IsOPC = isOPC;
                PreviewModuleMap = fixture.Create<Dictionary<long, PreviewModule>>();
                foreach (var item in PreviewModuleMap)
                {
                    item.Value.m_nID = item.Key;
                }
                IdToUpdate = PreviewModuleMap.First().Key;
                PreviewModuleToUpdate = PreviewModuleMap.First().Value;

                if (!idExists)
                {
                    PreviewModuleMap.Remove(IdToUpdate);
                }

                AmountCallToRepository = needToUpdate ? 1 : 0;
                if (needToUpdate)
                {
                    PreviewModuleMap.Remove(IdToUpdate);
                    var pm = fixture.Create<PreviewModule>();
                    pm.m_nID = IdToUpdate;
                    PreviewModuleMap.Add(IdToUpdate, pm);
                }
            }
        }
    }
}
