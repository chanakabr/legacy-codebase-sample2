using AutoFixture;
using KalturaRequestContext;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.ModelsValidators;
using WebAPI.Reflection;

namespace WebAPI.Tests.Models.Catalog
{
    [TestFixture]
    public class KalturaCategoryVersionTests
    {
        [OneTimeSetUp]
        public void SetUp()
        {
            Utils.SetUp();
        }

        [Test]
        public void CheckValidateForAdd()
        {
            KalturaCategoryVersion kalturaCategoryVersion = new KalturaCategoryVersion();
            // validate empty name
            Assert.Throws(Is.TypeOf<BadRequestException>()
                .And.Property(nameof(BadRequestException.Code)).EqualTo((int)StatusCode.ArgumentCannotBeEmpty),
                () =>
                {
                    kalturaCategoryVersion.ValidateForAdd();
                });
        }

        [TestCaseSource(nameof(AddBadRequestExceptionTestCases), new object[] { "Add" })]
        public void CheckAddBadRequestException(Dictionary<string, object> actionParams, string exceptionMessage)
        {
            HttpContext.Current.Items[RequestContextConstants.REQUEST_TYPE] = RequestType.INSERT;
            var methodArgs = DataModel.getMethodParams("categoryVersion", "add");
            var reqParams = new Dictionary<string, object>(){
                    { "objectToAdd", actionParams }
                };

            Assert.That(() => RequestParsingHelpers.BuildActionArguments(methodArgs, reqParams),
               Throws.TypeOf<BadRequestException>()
               .And.Message.EqualTo(exceptionMessage));
        }

        private static IEnumerable AddBadRequestExceptionTestCases(string action)
        {
            Fixture fixture = new Fixture();
            Dictionary<string, object> actionParams1 = new Dictionary<string, object>()
            {
                { "objectType", "KalturaCategoryVersion" },
                { "id", 1 }
            };
            yield return new TestCaseData(actionParams1, "Argument [KalturaCategoryVersion.id] is not writeable").SetName($"{action}BadRequestException_readonlyid");

            Dictionary<string, object> actionParams2 = new Dictionary<string, object>()
            {
                { "objectType", "KalturaCategoryVersion" },
                { "name", new string(fixture.CreateMany<char>(256).ToArray()) }
            };
            yield return new TestCaseData(actionParams2, "Argument [KalturaCategoryVersion.name] maximum length is [255]").SetName($"{action}BadRequestException_maxname");

            Dictionary<string, object> actionParams3 = new Dictionary<string, object>()
            {
                { "objectType", "KalturaCategoryVersion" },
                { "comment", new string(fixture.CreateMany<char>(256).ToArray()) }
            };
            yield return new TestCaseData(actionParams3, "Argument [KalturaCategoryVersion.comment] maximum length is [255]").SetName($"{action}BadRequestException_maxcomment");

            Dictionary<string, object> actionParams4 = new Dictionary<string, object>()
            {
                { "objectType", "KalturaCategoryVersion" },
                { "treeId", 1 }
            };
            yield return new TestCaseData(actionParams4, "Argument [KalturaCategoryVersion.treeId] is not writeable").SetName($"{action}BadRequestException_readonlytreeId");

            Dictionary<string, object> actionParams5 = new Dictionary<string, object>()
            {
                { "objectType", "KalturaCategoryVersion" },
                { "state", KalturaCategoryVersionState.DRAFT }
            };
            yield return new TestCaseData(actionParams5, "Argument [KalturaCategoryVersion.state] is not writeable").SetName($"{action}BadRequestException_readonlystate");

            Dictionary<string, object> actionParams6 = new Dictionary<string, object>()
            {
                { "objectType", "KalturaCategoryVersion" },
                { "baseVersionId", 0 }
            };
            yield return new TestCaseData(actionParams6, "Argument [KalturaCategoryVersion.baseVersionId] minimum value is [1]").SetName($"{action}BadRequestException_minbaseVersionId");

            Dictionary<string, object> actionParams7 = new Dictionary<string, object>()
            {
                { "objectType", "KalturaCategoryVersion" },
                { "categoryRootId", 1 }
            };
            yield return new TestCaseData(actionParams7, "Argument [KalturaCategoryVersion.categoryRootId] is not writeable").SetName($"{action}BadRequestException_readonlycategoryRootId");

            Dictionary<string, object> actionParams8 = new Dictionary<string, object>()
            {
                { "objectType", "KalturaCategoryVersion" },
                { "defaultDate", 1 }
            };
            yield return new TestCaseData(actionParams8, "Argument [KalturaCategoryVersion.defaultDate] is not writeable").SetName($"{action}BadRequestException_readonlydefaultDate");

            Dictionary<string, object> actionParams9 = new Dictionary<string, object>()
            {
                { "objectType", "KalturaCategoryVersion" },
                { "updaterId", 1 }
            };
            yield return new TestCaseData(actionParams9, "Argument [KalturaCategoryVersion.updaterId] is not writeable").SetName($"{action}BadRequestException_readonlyupdaterId");

            Dictionary<string, object> actionParams10 = new Dictionary<string, object>()
            {
                { "objectType", "KalturaCategoryVersion" },
                { "createDate", 1 }
            };
            yield return new TestCaseData(actionParams10, "Argument [KalturaCategoryVersion.createDate] is not writeable").SetName($"{action}BadRequestException_readonlycreateDate");

            Dictionary<string, object> actionParams11 = new Dictionary<string, object>()
            {
                { "objectType", "KalturaCategoryVersion" },
                { "updateDate", 1 }
            };
            yield return new TestCaseData(actionParams11, "Argument [KalturaCategoryVersion.updateDate] is not writeable").SetName($"{action}BadRequestException_readonlyupdateDate");
        }

        [TestCaseSource(nameof(AddBadRequestExceptionTestCases), new object[] { "Update" })]
        [TestCaseSource(nameof(UpdateBadRequestExceptionTestCases))]
        public void CheckUpdateBadRequestException(Dictionary<string, object> actionParams, string exceptionMessage)
        {
            HttpContext.Current.Items[RequestContextConstants.REQUEST_TYPE] = RequestType.UPDATE;
            var methodArgs = DataModel.getMethodParams("categoryVersion", "update");
            var reqParams = new Dictionary<string, object>(){
                    { "objectToUpdate", actionParams },
                    { "id", 1 }
                };

            Assert.That(() => RequestParsingHelpers.BuildActionArguments(methodArgs, reqParams),
               Throws.TypeOf<BadRequestException>()
               .And.Message.EqualTo(exceptionMessage));
        }

        private static IEnumerable UpdateBadRequestExceptionTestCases()
        {
            Dictionary<string, object> actionParams6 = new Dictionary<string, object>()
            {
                { "objectType", "KalturaCategoryVersion" },
                { "baseVersionId", 1 }
            };
            yield return new TestCaseData(actionParams6, "Argument [KalturaCategoryVersion.baseVersionId] is not updateable").SetName("UpdateBadRequestException_InsertOnlybaseVersionId");
        }
    }
}