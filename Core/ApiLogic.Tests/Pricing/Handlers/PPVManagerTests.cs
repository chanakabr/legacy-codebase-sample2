//using ApiLogic.Pricing.Handlers;
//using ApiObjects;
//using ApiObjects.Base;
//using ApiObjects.Response;
//using AutoFixture;
//using CachingProvider.LayeredCache;
//using Core.Pricing;
//using DAL;
//using Moq;
//using NUnit.Framework;
//using System;
//using System.Collections;
//using System.Collections.Generic;
//using System.Data;
//using System.Text;

//namespace ApiLogic.Tests.Pricing.Handlers
//{
//    [TestFixture]
//    class PPVManagerTests
//    {
//        [TestCaseSource(nameof(ListCases))]
//        public void CheckList(DataTable ppvModule, DataTable dtPPVDescription, int expectedMessage)
//        {
//            Fixture fixture = new Fixture();

//            var repositoryMock = new Mock<IPPVManagerRepository>();
//            var layeredCacheMock = new Mock<ILayeredCache>();

//            repositoryMock.Setup(x => x.Get_PPVModuleData(It.IsAny<int>(), null)).Returns(ppvModule);

//            repositoryMock.Setup( x=>x.Get_PPVDescription(It.IsAny<int>())).Returns(dtPPVDescription);

//            PPVManager manager = new PPVManager(repositoryMock.Object, layeredCacheMock.Object);

//            var response = manager.GetPPVModuleList(fixture.Create<int>());

//            Assert.That(response == null ? 0 : response.Count, Is.EqualTo(expectedMessage));
//        }

//        private static IEnumerable ListCases()
//        {
//            Fixture fixture = new Fixture();

//            yield return new TestCaseData(new DataTable(), 0).SetName("CheckListEmpty");

//            DataTable table = new DataTable();
//            DataRow row = table.NewRow();

//            table.Columns.Add(new DataColumn { DataType = typeof(string), ColumnName = "PRICE_CODE" });
//            table.Columns.Add(new DataColumn { DataType = typeof(string), ColumnName = "USAGE_MODULE_CODE" });
//            table.Columns.Add(new DataColumn { DataType = typeof(string), ColumnName = "DISCOUNT_MODULE_CODE" });
//            table.Columns.Add(new DataColumn { DataType = typeof(string), ColumnName = "COUPON_GROUP_CODE" });
//            table.Columns.Add(new DataColumn { DataType = typeof(string), ColumnName = "NAME" });
//            table.Columns.Add(new DataColumn { DataType = typeof(bool), ColumnName = "SUBSCRIPTION_ONLY" });
//            table.Columns.Add(new DataColumn { DataType = typeof(bool), ColumnName = "FIRSTDEVICELIMITATION" });
//            table.Columns.Add(new DataColumn { DataType = typeof(string), ColumnName = "Product_Code" });
//            table.Columns.Add(new DataColumn { DataType = typeof(string), ColumnName = "ADS_PARAM" });
//            table.Columns.Add(new DataColumn { DataType = typeof(int), ColumnName = "ADS_POLICY" });
//            table.Columns.Add(new DataColumn { DataType = typeof(int), ColumnName = "ID" });

//            row["PRICE_CODE"] = "priceCode";
//            row["USAGE_MODULE_CODE"] = "USAGE_MODULE_CODE";
//            row["DISCOUNT_MODULE_CODE"] = "DISCOUNT_MODULE_CODE";
//            row["COUPON_GROUP_CODE"] = "COUPON_GROUP_CODE";
//            row["NAME"] = "name";
//            row["SUBSCRIPTION_ONLY"] = 0;
//            row["FIRSTDEVICELIMITATION"] = 1;
//            row["Product_Code"] = "Product_Code";
//            row["ADS_PARAM"] = "ADS_PARAM";
//            row["ADS_POLICY"] = (int)AdsPolicy.KeepAds;
//            row["ID"] = 1;
//            table.Rows.Add(row);


//            DataTable dtPPVDescription = new DataTable();
//            dtPPVDescription.Columns.Add(new DataColumn { DataType = typeof(string), ColumnName = "description" });
//            dtPPVDescription.Columns.Add(new DataColumn { DataType = typeof(string), ColumnName = "language_code3" });
            
//            row = dtPPVDescription.NewRow();
//            row["description"] = "description";
//            row["language_code3"] = "language_code3";
//            dtPPVDescription.Rows.Add(row);


//            yield return new TestCaseData(table, dtPPVDescription, 1).SetName("CheckList");
//        }
//    }
//}