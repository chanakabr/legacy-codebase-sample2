using ApiLogic.Pricing.Handlers;
using ApiObjects.Base;
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
using System.Text;

namespace ApiLogic.Tests.Pricing.Handlers
{
    [TestFixture]
    public class PreviewModuleManagerTests
    {
        [TestCaseSource(nameof(DeleteCases))]
        public void CheckDelete(eResponseStatus expectedCode, bool isExists, bool isDeleted)
        {
            Fixture fixture = new Fixture();

            var repositoryMock = new Mock<IPreviewModuleRepository>();


            repositoryMock.Setup(x => x.DeletePreviewModule(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>()))
                                    .Returns(isDeleted);

            repositoryMock.Setup(x => x.IsPreviewModuleExsitsd(It.IsAny<int>(), It.IsAny<long>()))
                                    .Returns(isExists);

            PreviewModuleManager manager = new PreviewModuleManager(repositoryMock.Object);

            var response = manager.Delete(fixture.Create<ContextData>(), It.IsAny<long>());

            Assert.That(response.Code, Is.EqualTo((int)expectedCode));
        }

        private static IEnumerable DeleteCases()
        {
            yield return new TestCaseData(eResponseStatus.OK, true, true).SetName("CheckDeleteSuccess");
            yield return new TestCaseData(eResponseStatus.PreviewModuleNotExist, false, false).SetName("CheckDeleteCodeNotExist");
            yield return new TestCaseData(eResponseStatus.Error, true, false).SetName("CheckDeleteFailed");
        }

        [TestCaseSource(nameof(AddCases))]
        public void CheckAdd(eResponseStatus expectedCode, long id)
        {
            Fixture fixture = new Fixture();

            var repositoryMock = new Mock<IPreviewModuleRepository>();

            repositoryMock.Setup(x => x.InsertPreviewModule(It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<long>())).Returns(id);

            PreviewModuleManager manager = new PreviewModuleManager(repositoryMock.Object);

            var response = manager.Add(fixture.Create<ContextData>(), fixture.Create<PreviewModule>());

            Assert.That(response.Status.Code, Is.EqualTo((int)expectedCode));
        }

        private static IEnumerable AddCases()
        {
            yield return new TestCaseData(eResponseStatus.OK, 12).SetName("CheckAddSuccess");
            yield return new TestCaseData(eResponseStatus.Error, 0).SetName("CheckAddFailed");
        }

        [TestCaseSource(nameof(ListCases))]
        public void CheckList(DataTable table, string expectedMessage)
        {
            var repositoryMock = new Mock<IPreviewModuleRepository>();
            repositoryMock.Setup(x => x.Get_PreviewModulesByGroupID(It.IsAny<int>(), true, true)).Returns(table);

            PreviewModuleManager manager = new PreviewModuleManager(repositoryMock.Object);
            var response = manager.GetPreviewModules(It.IsAny<int>());

            Assert.That(response.Status.Message, Is.EqualTo(expectedMessage));
        }

        private static IEnumerable ListCases()
        {
            yield return new TestCaseData(new DataTable(), "There are no preview modules").SetName("CheckListEmpty");

            DataTable table = new DataTable();
            table.Columns.Add(new DataColumn { DataType = typeof(int), ColumnName = "id" });
            table.Columns.Add(new DataColumn { DataType = typeof(String), ColumnName = "name" });
            table.Columns.Add(new DataColumn { DataType = typeof(int), ColumnName = "FULL_LIFE_CYCLE_ID" });
            table.Columns.Add(new DataColumn { DataType = typeof(int), ColumnName = "NON_RENEWING_PERIOD_ID" });
            DataRow row = table.NewRow();
            row["id"] = 1;
            row["name"] = "name";
            row["FULL_LIFE_CYCLE_ID"] = 1;
            row["NON_RENEWING_PERIOD_ID"] = 1;
            table.Rows.Add(row);

            yield return new TestCaseData(table, eResponseStatus.OK.ToString()).SetName("CheckListSeccess");
           
        }
    }
}
