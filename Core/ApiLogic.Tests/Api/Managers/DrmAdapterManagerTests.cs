using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using AutoFixture;
using DAL;
using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace ApiLogic.Tests.Api.Managers
{
    [TestFixture]
    class DrmAdapterManagerTests
    {
        [TestCaseSource(nameof(DeleteCases))]
        public void CheckDelete(eResponseStatus expectedCode, bool deleteDrmAdapter, bool isDrmAdapterExists)
        {
            Fixture fixture = new Fixture();

            var repositoryMock = new Mock<IDrmAdapterRepository>();
            

            repositoryMock.Setup(x => x.DeleteDrmAdapter(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>()))
                                    .Returns(deleteDrmAdapter);

            repositoryMock.Setup(x => x.IsDrmAdapterExists(It.IsAny<int>(), It.IsAny<long>()))
                                    .Returns(isDrmAdapterExists);

            DrmAdapterManager manager = new DrmAdapterManager(repositoryMock.Object);

            var response = manager.Delete(fixture.Create<ContextData>(),It.IsAny<long>());

            Assert.That(response.Code, Is.EqualTo((int)expectedCode));
        }

        private static IEnumerable DeleteCases()
        {
            Fixture fixture = new Fixture();

            yield return new TestCaseData(eResponseStatus.OK, true, true).SetName("CheckDeleteSuccess");
            yield return new TestCaseData(eResponseStatus.DrmAdapterNotExist, false, false).SetName("CheckDeleteCodeNotExist");
            yield return new TestCaseData(eResponseStatus.Error, false, true).SetName("CheckDeleteFailed");
        }

        [TestCaseSource(nameof(InsertCases))]
        public void CheckAdd(eResponseStatus expectedCode, long id)
        {
            Fixture fixture = new Fixture();

            var repositoryMock = new Mock<IDrmAdapterRepository>();

            repositoryMock.Setup(x => x.InsertDrmAdapter(It.IsAny<DrmAdapter>(), It.IsAny<int>(), It.IsAny<long>())).Returns(id);

            DrmAdapterManager manager = new DrmAdapterManager(repositoryMock.Object);
            
            var response = manager.Add(fixture.Create<ContextData>(), fixture.Create<DrmAdapter>());

            Assert.That(response.Status.Code, Is.EqualTo((int)expectedCode));
        }

        private static IEnumerable InsertCases()
        {
            yield return new TestCaseData(eResponseStatus.OK, 12).SetName("CheckInsertSuccess");
            yield return new TestCaseData(eResponseStatus.Error, 0).SetName("CheckInsertFailed");
        }

        [TestCaseSource(nameof(ListCases))]
        public void CheckList(List<DrmAdapter> list, string expectedMessage)
        {
            Fixture fixture = new Fixture();

            var repositoryMock = new Mock<IDrmAdapterRepository>();

            repositoryMock.Setup(x => x.GetDrmAdapters(It.IsAny<int>())).Returns(list);

            DrmAdapterManager manager = new DrmAdapterManager(repositoryMock.Object);

            var response = manager.GetDrmAdapters(It.IsAny<int>());

            Assert.That(response.Status.Message, Is.EqualTo(expectedMessage));
        }

        private static IEnumerable ListCases()
        {
            Fixture fixture = new Fixture();

            yield return new TestCaseData(new List<DrmAdapter>(), "Adapter does not exist").SetName("CheckListEmpty");

            List<DrmAdapter> list = new List<DrmAdapter>();
            list.Add(fixture.Create<DrmAdapter>());
            yield return new TestCaseData(list, eResponseStatus.OK.ToString()).SetName("CheckList");
        }
    }
}
