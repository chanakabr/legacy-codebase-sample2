using ApiObjects.Rules;
using ApiObjects.User.SessionProfile;
using AutoFixture;
using Newtonsoft.Json;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;

namespace DAL.Tests
{
    [TestFixture]
    public class JsonTests
    {
        private static readonly JsonSerializerSettings UserSessionProfileSettings =
            new JsonSerializerSettings { TypeNameHandling = TypeNameHandling.All };

        [Test]
        public void DeserializeEmptyStringTest()
        {
            UserSessionProfile expected = null;
            string json = string.Empty;
            var isDeserialized = Json.TryDeserialize<UserSessionProfile>(json, out var deserialize);
            Assert.That(true, Is.EqualTo(isDeserialized));
            Assert.That(ItExtension.Validate(deserialize, expected));
        }

        [Test]
        public void DeserializeValidTest()
        {
            UserSessionProfile expected = GetUserSessionProfile();
            string json = JsonConvert.SerializeObject(expected, UserSessionProfileSettings);
            var isDeserialized = Json.TryDeserialize<UserSessionProfile>(json, out var deserialize);
            Assert.That(true, Is.EqualTo(isDeserialized));
            Assert.That(ItExtension.Validate(deserialize, expected));
        }

        [Test]
        public void DeserializeNonExistingClassTest()
        {
            UserSessionProfile expected = null;
            string json = GetNonExistingClassType();
            var isDeserialized = Json.TryDeserialize<UserSessionProfile>(json, out var deserialize);
            Assert.That(false, Is.EqualTo(isDeserialized));
            Assert.That(ItExtension.Validate(deserialize, expected));
        }

        [Test]
        public void DeserializeInvalidPropertyTypeTest()
        {
            UserSessionProfile expected = null;
            string json = GetInvalidPropertyType();
            var isDeserialized = Json.TryDeserialize<UserSessionProfile>(json, out var deserialize);
            Assert.That(false, Is.EqualTo(isDeserialized));
            Assert.That(ItExtension.Validate(deserialize, expected));
        }

        [Test]
        public void DeserializeUserSessionProfileExpressionTest()
        {
            var userSessionProfile = GetUserSessionProfile();
            var expressionJson = JsonConvert.SerializeObject(userSessionProfile.Expression, UserSessionProfileSettings);

            var deserialized = Json.TryDeserialize<ExpressionOr>(expressionJson, out var expression);
            Assert.That(true, Is.EqualTo(deserialized));
            var userSessionProfileExpression = userSessionProfile.Expression as ExpressionOr;
            Assert.That(ItExtension.Validate(expression, userSessionProfileExpression));
        }

        private static UserSessionProfile GetUserSessionProfile()
        {
            var fixture = new Fixture();
            var expression = new ExpressionOr()
            {
                Expressions = new List<IUserSessionProfileExpression>()
                {
                    new ExpressionNot() { Expression = new UserSessionCondition() {Condition = new DeviceBrandCondition() { IdIn = new List<int>() { 1, 2, 3 } }}},
                    new UserSessionCondition() { Condition = new DeviceFamilyCondition() { IdIn = new List<int>() { 4, 5, 6 } } },
                    new ExpressionAnd()
                    {
                        Expressions = new List<IUserSessionProfileExpression>()
                        {
                            new UserSessionCondition() {Condition = new SegmentsCondition() { SegmentIds = new List<long>() { 7, 8, 9 } } },
                            new UserSessionCondition() {Condition = new DeviceManufacturerCondition() { IdIn = new List<long>() { 1, 2, 3 } } }
                        }
                    },
                    new UserSessionCondition() {Condition = new DeviceModelCondition() {RegexEqual = fixture.Create<string>()} },
                    new UserSessionCondition() {Condition = new DynamicKeysCondition() { Key = fixture.Create<string>(), Values = fixture.Create<List<string>>() } }
                }
            };

            var userSessionProfile = new UserSessionProfile() { Expression = expression };
            return userSessionProfile;
        }

        private static string GetNonExistingClassType()
        {
            return @"{
                        'Expression':{
                            '$type': 'ApiObjects.User.SessionProfile.ExpressionOr, ApiObjects',
                            'Expressions': [
                                {
                                    '$type': 'ApiObjects.User.SessionProfile.UserSessionCondition, ApiObjects',
                                    'Condition': {
                                        '$type': 'ApiObjects.Rules.NonExistingCondition, ApiObjects',
                                        'IdIn': {
                                            '$type': 'System.Collections.Generic.List`1[[System.Int32, System.Private.CoreLib]], System.Private.CoreLib',
                                            '$values': [1,2,3]
                                        },
                                        'Type': 12,
                                        'Description': null
                                    }
                                },
                                {
                                    '$type': 'ApiObjects.User.SessionProfile.ExpressionAnd, ApiObjects',
                                    'Expressions': [
                                        {
                                            '$type': 'ApiObjects.User.SessionProfile.UserSessionCondition, ApiObjects',
                                            'Condition': {
                                                '$type': 'ApiObjects.Rules.DeviceFamilyCondition, ApiObjects',
                                                    'IdIn': {
                                                        '$type': 'System.Collections.Generic.List`1[[System.Int32, System.Private.CoreLib]], System.Private.CoreLib',
                                                        '$values': [4,5,6]
                                                    },
                                                    'Type': 13,
                                                    'Description': null
                                            }
                                        },
                                        {
                                            '$type': 'ApiObjects.User.SessionProfile.UserSessionCondition, ApiObjects',
                                            'Condition': {
                                                '$type': 'ApiObjects.Rules.SegmentsCondition, ApiObjects',
                                                    'SegmentIds': {
                                                        '$type': 'System.Collections.Generic.List`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib',
                                                        '$values': [7,8,9]
                                                    },
                                                    'Type': 5,
                                                    'Description': null
                                            }
                                        }
                                    ]
                                }
                            ]
                        }
                    }";
        }

        private static string GetInvalidPropertyType()
        {
            return @"{
                        'Expression':{
                            '$type': 'ApiObjects.User.SessionProfile.ExpressionOr, ApiObjects',
                            'Expressions': [
                                {
                                    '$type': 'ApiObjects.User.SessionProfile.UserSessionCondition, ApiObjects',
                                    'Condition': {
                                        '$type': 'ApiObjects.Rules.DeviceBrandCondition, ApiObjects',
                                        'IdIn': {
                                            '$type': 'System.Collections.Generic.List`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib',
                                            '$values': [1,2,3]
                                        },
                                        'Type': 12,
                                        'Description': null
                                    }
                                },
                                {
                                    '$type': 'ApiObjects.User.SessionProfile.ExpressionAnd, ApiObjects',
                                    'Expressions': [
                                        {
                                            '$type': 'ApiObjects.User.SessionProfile.UserSessionCondition, ApiObjects',
                                            'Condition': {
                                                '$type': 'ApiObjects.Rules.DeviceFamilyCondition, ApiObjects',
                                                    'IdIn': {
                                                        '$type': 'System.Collections.Generic.List`1[[System.Int32, System.Private.CoreLib]], System.Private.CoreLib',
                                                        '$values': [4,5,6]
                                                    },
                                                    'Type': 13,
                                                    'Description': null
                                            }
                                        },
                                        {
                                            '$type': 'ApiObjects.User.SessionProfile.UserSessionCondition, ApiObjects',
                                            'Condition': {
                                                '$type': 'ApiObjects.Rules.SegmentsCondition, ApiObjects',
                                                    'SegmentIds': {
                                                        '$type': 'System.Collections.Generic.List`1[[System.Int64, System.Private.CoreLib]], System.Private.CoreLib',
                                                        '$values': [7,8,9]
                                                    },
                                                    'Type': 5,
                                                    'Description': null
                                            }
                                        }
                                    ]
                                }
                            ]
                        }
                    }";
        }
    }
}