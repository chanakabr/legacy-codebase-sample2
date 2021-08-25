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
    class UsageModuleManagerTests
    {
        [TestCaseSource(nameof(DeleteCases))]
        public void CheckDelete(eResponseStatus expectedCode, bool deleteUsageModule, bool isUsageModuleExists)
        {
            Fixture fixture = new Fixture();

            var repositoryMock = new Mock<IModuleManagerRepository>();


            repositoryMock.Setup(x => x.DeletePricePlan(It.IsAny<int>(), It.IsAny<long>(), It.IsAny<long>()))
                                    .Returns(deleteUsageModule);

            repositoryMock.Setup(x => x.IsUsageModuleExistsById(It.IsAny<int>(), It.IsAny<long>()))
                                    .Returns(isUsageModuleExists);

            UsageModuleManager manager = new UsageModuleManager(repositoryMock.Object);

            var response = manager.Delete(fixture.Create<ContextData>(), It.IsAny<long>());

            Assert.That(response.Code, Is.EqualTo((int)expectedCode));
        }

        private static IEnumerable DeleteCases()
        {
            Fixture fixture = new Fixture();

            yield return new TestCaseData(eResponseStatus.OK, true, true).SetName("CheckDeleteSuccess");
            yield return new TestCaseData(eResponseStatus.UsageModuleDoesNotExist, false, false).SetName("CheckDeleteCodeNotExist");
            yield return new TestCaseData(eResponseStatus.Error, false, true).SetName("CheckDeleteFailed");
        }

        [TestCaseSource(nameof(InsertCases))]
        public void CheckAdd(eResponseStatus expectedCode, int id)
        {
            Fixture fixture = new Fixture();

            var repositoryMock = new Mock<IModuleManagerRepository>();

            repositoryMock.Setup(x => x.InsertUsageModule(It.IsAny<long>(), It.IsAny<int>(), It.IsAny<string>(), It.IsAny<int>(),
                It.IsAny<int>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<bool>(), It.IsAny<bool>())).Returns(id);

            UsageModuleManager manager = new UsageModuleManager(repositoryMock.Object);

            var response = manager.Add(fixture.Create<ContextData>(), fixture.Create<UsageModule>());

            Assert.That(response.Status.Code, Is.EqualTo((int)expectedCode));
        }

        private static IEnumerable InsertCases()
        {
            yield return new TestCaseData(eResponseStatus.OK, 12).SetName("CheckInsertSuccess");
            yield return new TestCaseData(eResponseStatus.Error, 0).SetName("CheckInsertFailed");
        }

        [TestCaseSource(nameof(ListCases))]
        public void CheckList(DataTable usageModules, string expectedMessage)
        {
            Fixture fixture = new Fixture();

            var repositoryMock = new Mock<IModuleManagerRepository>();

            repositoryMock.Setup(x => x.GetPricePlansDT(It.IsAny<int>(), null)).Returns(usageModules);

            UsageModuleManager manager = new UsageModuleManager(repositoryMock.Object);

            var response = manager.GetUsageModules(fixture.Create<int>());

            Assert.That(response.Status.Message, Is.EqualTo(expectedMessage));
        }

        private static IEnumerable ListCases()
        {
            yield return new TestCaseData(new DataTable(), eResponseStatus.OK.ToString()).SetName("CheckListEmpty");

            DataTable table = new DataTable();
            DataRow row = table.NewRow();
            table.Columns.Add(new DataColumn { DataType = typeof(int), ColumnName = "coupon_id" });
            table.Columns.Add(new DataColumn { DataType = typeof(int), ColumnName = "WAIVER" });
            table.Columns.Add(new DataColumn { DataType = typeof(int), ColumnName = "OFFLINE_PLAYBACK" });
            table.Columns.Add(new DataColumn { DataType = typeof(int), ColumnName = "MAX_VIEWS_NUMBER" });
            table.Columns.Add(new DataColumn { DataType = typeof(int), ColumnName = "ext_discount_id" });
            table.Columns.Add(new DataColumn { DataType = typeof(int), ColumnName = "is_renew" });
            table.Columns.Add(new DataColumn { DataType = typeof(int), ColumnName = "ID" });
            table.Columns.Add(new DataColumn { DataType = typeof(int), ColumnName = "num_of_rec_periods" });
            table.Columns.Add(new DataColumn { DataType = typeof(int), ColumnName = "WAIVER_PERIOD" });
            table.Columns.Add(new DataColumn { DataType = typeof(int), ColumnName = "pricing_id" });
            table.Columns.Add(new DataColumn { DataType = typeof(int), ColumnName = "NAME" });
            table.Columns.Add(new DataColumn { DataType = typeof(int), ColumnName = "FULL_LIFE_CYCLE_MIN" });
            table.Columns.Add(new DataColumn { DataType = typeof(int), ColumnName = "VIEW_LIFE_CYCLE_MIN" });
            table.Columns.Add(new DataColumn { DataType = typeof(int), ColumnName = "type" });
            row["WAIVER"] = 1;
            row["coupon_id"] = 1;
            row["OFFLINE_PLAYBACK"] = 1;
            row["ext_discount_id"] = 1;
            row["is_renew"] = 1;
            row["MAX_VIEWS_NUMBER"] = 1;
            row["ID"] = 1;
            row["num_of_rec_periods"] = 1;
            row["WAIVER_PERIOD"] = 1;
            row["pricing_id"] = 1;
            row["NAME"] = 1;
            row["FULL_LIFE_CYCLE_MIN"] = 1;
            row["VIEW_LIFE_CYCLE_MIN"] = 1;
            row["type"] = 0;
            table.Rows.Add(row);
            yield return new TestCaseData(table, eResponseStatus.OK.ToString()).SetName("CheckList");
        }
    }
}