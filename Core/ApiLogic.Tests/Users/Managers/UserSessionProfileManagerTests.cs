using System;
using System.Collections;
using System.Collections.Generic;
using ApiLogic.Users;
using ApiLogic.Users.Managers;
using ApiObjects.Rules;
using ApiObjects.User.SessionProfile;
using CachingProvider.LayeredCache;
using Core.Api.Managers;
using DAL.Users;
using FluentAssertions;
using Moq;
using NUnit.Framework;
using KeyValuePair = ApiObjects.KeyValuePair;

namespace ApiLogic.Tests.Users.Managers
{
    public class UserSessionProfileManagerTests
    {
        private const int GroupId = 1483;
        private MockRepository _mockRepository;
        private Mock<IUserSessionProfileRepository> _userSessionProfileRepository;
        private UserSessionProfileManager _userSessionProfileManager;

        [SetUp]
        public void SetUp()
        {
            _mockRepository = new MockRepository(MockBehavior.Strict);
            _userSessionProfileRepository = _mockRepository.Create<IUserSessionProfileRepository>();
            _userSessionProfileManager = new UserSessionProfileManager(
                _userSessionProfileRepository.Object,
                new MockLayeredCache(),
                Mock.Of<IUserSessionProfileExpressionValidator>(),
                Mock.Of<IAssetRuleManager>()
            );
        }

        [TearDown]
        public void TearDown()
        {
            _mockRepository.VerifyAll();
        }

        [TestCaseSource(nameof(Conditions))]
        public void TestConditions(List<UserSessionProfile> allProfiles, IEnumerable<int> matchedProfilesIds)
        {
            _userSessionProfileRepository
                .Setup(x => x.GetUserSessionProfiles(GroupId))
                .Returns(allProfiles);

            var userFromPhone = new UserSessionConditionScope
            {
                Model = "Huawei P20 Pro",
                BrandId = 323,
                FamilyId = 1,
                ManufacturerId = 123,
                FilterBySegments = true,
                SegmentIds = new List<long> { 10, 20, 30 },
                DeviceDynamicData = new List<KeyValuePair> { new KeyValuePair("codec", "h.265") },
                SessionCharacteristics = new Dictionary<string, List<string>> { { "network", new List<string> { "out of home", "street" } } }
            };

            _userSessionProfileManager.GetMatchedUserSessionProfiles(GroupId, userFromPhone)
                .Should().BeEquivalentTo(matchedProfilesIds);
        }

        private static IEnumerable Conditions()
        {
            yield return Match(new DeviceModelCondition { RegexEqual = "huawei .*" });
            yield return NotMatch(new DeviceModelCondition { RegexEqual = "iphone .*" });
            
            yield return Match(new DeviceBrandCondition { IdIn = new List<int> { 322, 323 } });
            yield return NotMatch(new DeviceBrandCondition { IdIn = new List<int> { 324, 325 } });
            
            yield return Match(new DeviceFamilyCondition { IdIn = new List<int> { 1, 2 } });
            yield return NotMatch(new DeviceFamilyCondition { IdIn = new List<int> { 2, 3 } });
            
            yield return Match(new DeviceManufacturerCondition { IdIn = new List<long> { 122, 123 } });
            yield return NotMatch(new DeviceManufacturerCondition { IdIn = new List<long> { 124, 125 } });

            yield return Match(new SegmentsCondition { SegmentIds = new List<long> { 10, 20 } });
            yield return NotMatch(new SegmentsCondition { SegmentIds = new List<long> { 20, 30, 40 } });

            yield return Match(new DeviceDynamicDataCondition { Key = "codec", Value = "h.265" });
            yield return NotMatch(new DeviceDynamicDataCondition { Key = "codec", Value = "H.265" });
            yield return NotMatch(new DeviceDynamicDataCondition { Key = "coDec", Value = "h.265" });
            yield return NotMatch(new DeviceDynamicDataCondition { Key = "fruit", Value = "banana" });

            yield return Match(new DynamicKeysCondition { Key = "network", Values = new List<string> { "street", "shop" } });
            yield return NotMatch(new DynamicKeysCondition { Key = "network", Values = new List<string> { "sTreet", "shop" } });
            yield return NotMatch(new DynamicKeysCondition { Key = "network", Values = new List<string> { "shop", "work" } });
            yield return NotMatch(new DynamicKeysCondition { Key = "neTwork", Values = new List<string> { "street", "shop" } });
            
            yield return Match(Or(new DeviceModelCondition { RegexEqual = "huawei .*" }, new DeviceBrandCondition { IdIn = new List<int> { 324, 325 } }), "Or");
            yield return NotMatch(Or(new DeviceModelCondition { RegexEqual = "iphone .*" }, new DeviceBrandCondition { IdIn = new List<int> { 324, 325 } }), "Or");
            
            yield return Match(And(new DeviceModelCondition { RegexEqual = "huawei .*" }, new DeviceBrandCondition { IdIn = new List<int> { 323, 324 } }), "And");
            yield return NotMatch(And(new DeviceModelCondition { RegexEqual = "huawei .*" }, new DeviceBrandCondition { IdIn = new List<int> { 324, 325 } }), "And");
            
            yield return Match(Not(new DeviceModelCondition { RegexEqual = "iphone .*" }), "Not");
            yield return NotMatch(Not(new DeviceModelCondition { RegexEqual = "huawei .*" }), "Not");
            
            yield return new TestCaseData(
                new List<UserSessionProfile>
                {
                    new UserSessionProfile { Id = 1, Expression = Single(new DeviceFamilyCondition { IdIn = new List<int> { 1, 2 } }) }, //match
                    new UserSessionProfile { Id = 2, Expression = Single(new DeviceModelCondition { RegexEqual = "iphone .*" }) }, // not match
                    new UserSessionProfile { Id = 3, Expression = Single(new DeviceBrandCondition { IdIn = new List<int> { 322, 323 } }) } // match
                }, new[] { 1, 3 }).SetName($"match several profiles");
        }

        private static TestCaseData Match(RuleCondition condition) => Match(SingleCondition(condition), condition.Type.ToString());
        private static TestCaseData NotMatch(RuleCondition condition) => NotMatch(SingleCondition(condition), condition.Type.ToString());
        
        private static TestCaseData Match(List<UserSessionProfile> profiles, string name) => new TestCaseData(profiles, new[] { 1 }).SetName($"{name} match");
        private static TestCaseData NotMatch(List<UserSessionProfile> profiles, string name) => new TestCaseData(profiles, Array.Empty<int>()).SetName($"{name} not match");

        private static List<UserSessionProfile> SingleCondition(RuleCondition condition) => new List<UserSessionProfile>
        {
            new UserSessionProfile { Id = 1, Expression = Single(condition) }
        };
        
        private static List<UserSessionProfile> And(RuleCondition condition1, RuleCondition condition2) => new List<UserSessionProfile>
        {
            new UserSessionProfile { Id = 1, Expression = new ExpressionAnd { Expressions = new List<IUserSessionProfileExpression> { Single(condition1), Single(condition2) } } }
        };
        
        private static List<UserSessionProfile> Or(RuleCondition condition1, RuleCondition condition2) => new List<UserSessionProfile>
        {
            new UserSessionProfile { Id = 1, Expression = new ExpressionOr { Expressions = new List<IUserSessionProfileExpression> { Single(condition1), Single(condition2) } } }
        };
        
        private static List<UserSessionProfile> Not(RuleCondition condition) => new List<UserSessionProfile>
        {
            new UserSessionProfile { Id = 1, Expression = new ExpressionNot { Expression = Single(condition) } }
        };

        private static IUserSessionProfileExpression Single(RuleCondition condition) => new UserSessionCondition
        {
            Condition = condition
        };
    }
}