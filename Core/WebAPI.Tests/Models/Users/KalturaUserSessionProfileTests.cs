using NUnit.Framework;
using System.Collections.Generic;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Models.Users.UserSessionProfile;
using WebAPI.ModelsValidators;

namespace WebAPI.Tests.Models.Users
{
    class KalturaUserSessionProfileTests
    {
        [Test]
        public void CheckValidateMinConditionsSum()
        {
            var kalturaUserSessionProfile = new KalturaUserSessionProfile()
            {
                Name = "UserSessionProfile name",
                Expression = new KalturaExpressionAnd()
                {
                    Expressions = new List<KalturaUserSessionProfileExpression>()
                    {
                        new KalturaExpressionOr(){
                            Expressions = new List<KalturaUserSessionProfileExpression>()
                            {
                            }
                        }
                    }
                }
            };

            var addEx = Assert.Throws<BadRequestException>(() => kalturaUserSessionProfile.ValidateForAdd());
            StringAssert.Contains("[expression] must contain one argument from type [KalturaUserSessionCondition]", addEx.Message);
            Assert.That(addEx.Code, Is.EqualTo((int)StatusCode.MissingMandatoryArgumentInProperty));

            var updateEx = Assert.Throws<BadRequestException>(() => kalturaUserSessionProfile.ValidateForUpdate());
            StringAssert.Contains("[expression] must contain one argument from type [KalturaUserSessionCondition]", updateEx.Message);
            Assert.That(updateEx.Code, Is.EqualTo((int)StatusCode.MissingMandatoryArgumentInProperty));
        }

        [Test]
        public void CheckValidateMaxConditionsSum()
        {
            var kalturaUserSessionProfile = new KalturaUserSessionProfile()
            {
                Name = "UserSessionProfile name",
                Expression = new KalturaExpressionAnd()
                {
                    Expressions = new List<KalturaUserSessionProfileExpression>()
                    {
                        new KalturaUserSessionCondition(){ Condition = new KalturaSegmentsCondition() { SegmentsIds = "1,2,3" } },
                        new KalturaUserSessionCondition(){ Condition = new KalturaDynamicKeysCondition() { Key = "key1", Values = "value1" } },
                        new KalturaUserSessionCondition(){ Condition = new KalturaDeviceBrandCondition() { IdIn = "1,2,3" } },
                        new KalturaUserSessionCondition(){ Condition = new KalturaDeviceFamilyCondition() { IdIn = "1,2,3" } },
                        new KalturaUserSessionCondition(){ Condition = new KalturaDeviceManufacturerCondition() { IdIn = "1,2,3" } },
                        new KalturaUserSessionCondition(){ Condition = new KalturaDeviceModelCondition() { RegexEqual = @"[\x00-\x7F]" } },
                        new KalturaExpressionOr()
                        {
                            Expressions = new List<KalturaUserSessionProfileExpression>()
                            {
                                new KalturaUserSessionCondition(){ Condition = new KalturaSegmentsCondition() { SegmentsIds = "1,2,3" } },
                                new KalturaUserSessionCondition(){ Condition = new KalturaDynamicKeysCondition() { Key = "key1", Values = "value1" } },
                                new KalturaUserSessionCondition(){ Condition = new KalturaDeviceBrandCondition() { IdIn = "1,2,3" } },
                                new KalturaUserSessionCondition(){ Condition = new KalturaDeviceFamilyCondition() { IdIn = "1,2,3" } },
                                new KalturaUserSessionCondition(){ Condition = new KalturaDeviceManufacturerCondition() { IdIn = "1,2,3" }},
                                new KalturaExpressionNot() { Expression = new KalturaUserSessionCondition(){ Condition = new KalturaDeviceModelCondition() { RegexEqual = @"[\x00-\x7F]" } } }
                            }
                        }
                    }
                }
            };

            var addEx = Assert.Throws<BadRequestException>(() => kalturaUserSessionProfile.ValidateForAdd());
            StringAssert.Contains("Argument [expression's conditions] maximum items is [10]", addEx.Message);
            Assert.That(addEx.Code, Is.EqualTo((int)StatusCode.ArgumentMaxItemsCrossed));
            
            var updateEx = Assert.Throws<BadRequestException>(() => kalturaUserSessionProfile.ValidateForUpdate());
            StringAssert.Contains("Argument [expression's conditions] maximum items is [10]", updateEx.Message);
            Assert.That(updateEx.Code, Is.EqualTo((int)StatusCode.ArgumentMaxItemsCrossed));
        }
    }
}
