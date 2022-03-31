using ApiLogic.Users;
using ApiLogic.Users.Managers;
using ApiObjects;
using ApiObjects.Response;
using ApiObjects.Rules;
using ApiObjects.Segmentation;
using ApiObjects.User.SessionProfile;
using AutoFixture;
using Core.Api;
using Moq;
using NUnit.Framework;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using ApiLogic.Repositories;

namespace ApiLogic.Tests.Users
{
    [TestFixture]
    public class UserSessionProfileExpressionValidatorTests
    {
        private enum IdsToContain
        {
            all,
            some,
            partial,
            none
        }

        private static readonly Fixture fixture = new Fixture();

        [OneTimeSetUp]
        public void OneTimeSetUp()
        {
            fixture.Customizations.Add(MultipleTypeRelay.NewHierarchyRelay<SegmentAction>());
            fixture.Customizations.Add(MultipleTypeRelay.NewHierarchyRelay<SegmentBaseValue>());
            fixture.Behaviors.OfType<ThrowingRecursionBehavior>().ToList()
                .ForEach(b => fixture.Behaviors.Remove(b));
            fixture.Behaviors.Add(new OmitOnRecursionBehavior());
        }


        [TestCaseSource(nameof(ValidateTestCases))]
        public void CheckValidate(eResponseStatus expectedResponse, IUserSessionProfileExpression expression)
        {
            var allIds = GetIds(IdsToContain.all);
            
            var deviceBrandRepositoryMock = new Mock<IDeviceBrandRepository>();
            deviceBrandRepositoryMock
                .Setup(x => x.List(1))
                .Returns(GetDeviceBrandsGenericResponse(allIds));
            
            var deviceFamiliesRepositoryMock = new Mock<IDeviceFamilyRepository>();
            deviceFamiliesRepositoryMock
                .Setup(x => x.List(1))
                .Returns(GetDeviceFamiliesGenericResponse(allIds));

            var segmentsManagerMock = new Mock<ISegmentsManager>();
            var segmentationTypes = GetSegmentationType(allIds);
            segmentsManagerMock.Setup(x => x.ListSegmentationTypes(It.IsAny<int>(), It.IsAny<HashSet<long>>(), It.IsAny<int>(), It.IsAny<int>(), It.IsAny<AssetSearchDefinition>()))
                .Returns(new GenericListResponse<SegmentationType>(Status.Ok, segmentationTypes));

            var validator = new UserSessionProfileExpressionValidator(deviceBrandRepositoryMock.Object,
                                                        deviceFamiliesRepositoryMock.Object,
                                                        segmentsManagerMock.Object,
                                                        Mock.Of<IDeviceReferenceDataManager>());
            var response = validator.Validate(1, expression);
            Assert.That(response.Code, Is.EqualTo((int)expectedResponse));
        }

        private GenericListResponse<DeviceBrand> GetDeviceBrandsGenericResponse(List<int> allIds)
        {
            var deviceBrands = fixture.CreateMany<DeviceBrand>(allIds.Count).ToList();
            for (int i = 0; i < allIds.Count; i++)
            {
                deviceBrands[i].Id = allIds[i];
            }

            return new GenericListResponse<DeviceBrand>(Status.Ok, deviceBrands);
        }

        private GenericListResponse<DeviceFamily> GetDeviceFamiliesGenericResponse(List<int> allIds)
        {
            var deviceFamilies = fixture.CreateMany<DeviceFamily>(allIds.Count).ToList();
            for (int i = 0; i < allIds.Count; i++)
            {
                deviceFamilies[i].Id = allIds[i];
            }

            return new GenericListResponse<DeviceFamily>(Status.Ok, deviceFamilies);
        }

        private List<SegmentationType> GetSegmentationType(List<int> allIds)
        {
            var segmentationTypes = fixture.CreateMany<SegmentationType>(allIds.Count).ToList();
            for (var i = 0; i < allIds.Count; i++)
            {
                var id = allIds[i];
                SegmentBaseValue segmentValue;
                switch (i)
                {
                    case 0:
                        var segmentDummyValue = fixture.Create<SegmentDummyValue>();
                        segmentDummyValue.Id = id;
                        segmentValue = segmentDummyValue;
                        break;
                    case 1:
                        var segmentValues = fixture.Create<SegmentValues>();
                        segmentValues.Values[0].Id = id;
                        segmentValue = segmentValues;
                        break;
                    case 2:
                        var segmentAllValues = fixture.Create<SegmentAllValues>();
                        segmentAllValues.Values[0].Id = id;
                        segmentValue = segmentAllValues;
                        break;
                    default:
                        var segmentRanges = fixture.Create<SegmentRanges>();
                        segmentRanges.Ranges[0].Id = id;
                        segmentValue = segmentRanges;
                        break;
                }

                segmentationTypes[i].Value = segmentValue;
            }
            
            return segmentationTypes;
        }

        private static IEnumerable ValidateTestCases()
        {
            yield return new TestCaseData(eResponseStatus.OK, GetUserSessionCondition(RuleConditionType.DeviceBrand, IdsToContain.all)).SetName("DeviceBrandIdsDoesNotExist_all");
            yield return new TestCaseData(eResponseStatus.OK, GetUserSessionCondition(RuleConditionType.DeviceBrand, IdsToContain.some)).SetName("DeviceBrandIdsDoesNotExist_some");
            yield return new TestCaseData(eResponseStatus.DeviceBrandIdsDoesNotExist, GetUserSessionCondition(RuleConditionType.DeviceBrand, IdsToContain.partial)).SetName("DeviceBrandIdsDoesNotExist_partial");
            yield return new TestCaseData(eResponseStatus.DeviceBrandIdsDoesNotExist, GetUserSessionCondition(RuleConditionType.DeviceBrand, IdsToContain.none)).SetName("DeviceBrandIdsDoesNotExist_none");

            yield return new TestCaseData(eResponseStatus.OK, GetUserSessionCondition(RuleConditionType.DeviceFamily, IdsToContain.all)).SetName("NonExistingDeviceFamilyIds_all");
            yield return new TestCaseData(eResponseStatus.OK, GetUserSessionCondition(RuleConditionType.DeviceFamily, IdsToContain.some)).SetName("NonExistingDeviceFamilyIds_some");
            yield return new TestCaseData(eResponseStatus.NonExistingDeviceFamilyIds, GetUserSessionCondition(RuleConditionType.DeviceFamily, IdsToContain.partial)).SetName("NonExistingDeviceFamilyIds_partial");
            yield return new TestCaseData(eResponseStatus.NonExistingDeviceFamilyIds, GetUserSessionCondition(RuleConditionType.DeviceFamily, IdsToContain.none)).SetName("NonExistingDeviceFamilyIds_none");

            yield return new TestCaseData(eResponseStatus.OK, GetUserSessionCondition(RuleConditionType.Segments, IdsToContain.all)).SetName("SegmentsIdsDoesNotExist_all");
            yield return new TestCaseData(eResponseStatus.OK, GetUserSessionCondition(RuleConditionType.Segments, IdsToContain.some)).SetName("SegmentsIdsDoesNotExist_some");
            yield return new TestCaseData(eResponseStatus.SegmentsIdsDoesNotExist, GetUserSessionCondition(RuleConditionType.Segments, IdsToContain.partial)).SetName("SegmentsIdsDoesNotExist_partial");
            yield return new TestCaseData(eResponseStatus.SegmentsIdsDoesNotExist, GetUserSessionCondition(RuleConditionType.Segments, IdsToContain.none)).SetName("SegmentsIdsDoesNotExist_none");

            var expressionOrSome = new ExpressionOr()
            {
                Expressions = new List<IUserSessionProfileExpression>()
                {
                    GetUserSessionCondition(RuleConditionType.DeviceBrand, IdsToContain.some),
                    GetUserSessionCondition(RuleConditionType.DeviceFamily, IdsToContain.some),
                    GetUserSessionCondition(RuleConditionType.Segments, IdsToContain.some)
                }
            };
            yield return new TestCaseData(eResponseStatus.OK, expressionOrSome).SetName("expressionOr_some");

            var expressionOrPartial = new ExpressionOr()
            {
                Expressions = new List<IUserSessionProfileExpression>()
                {
                    GetUserSessionCondition(RuleConditionType.DeviceBrand, IdsToContain.partial),
                    GetUserSessionCondition(RuleConditionType.DeviceFamily, IdsToContain.some),
                    GetUserSessionCondition(RuleConditionType.Segments, IdsToContain.some)
                }
            };
            yield return new TestCaseData(eResponseStatus.DeviceBrandIdsDoesNotExist, expressionOrPartial).SetName("expressionOr_partial");

            var expressionAndSome = new ExpressionAnd()
            {
                Expressions = new List<IUserSessionProfileExpression>()
                {
                    GetUserSessionCondition(RuleConditionType.DeviceBrand, IdsToContain.some),
                    GetUserSessionCondition(RuleConditionType.DeviceFamily, IdsToContain.some),
                    GetUserSessionCondition(RuleConditionType.Segments, IdsToContain.some)
                }
            };
            yield return new TestCaseData(eResponseStatus.OK, expressionAndSome).SetName("ExpressionAnd_some");

            var expressionAndPartial = new ExpressionAnd()
            {
                Expressions = new List<IUserSessionProfileExpression>()
                {
                    GetUserSessionCondition(RuleConditionType.DeviceBrand, IdsToContain.partial),
                    GetUserSessionCondition(RuleConditionType.DeviceFamily, IdsToContain.some),
                    GetUserSessionCondition(RuleConditionType.Segments, IdsToContain.some)
                }
            };
            yield return new TestCaseData(eResponseStatus.DeviceBrandIdsDoesNotExist, expressionAndPartial).SetName("ExpressionAnd_partial");
        }

        private static UserSessionCondition GetUserSessionCondition(RuleConditionType conditionType, IdsToContain idsToContain)
        {
            RuleCondition condition = null;
            switch (conditionType)
            {
                case RuleConditionType.DeviceBrand:
                    condition = new DeviceBrandCondition() { IdIn = GetIds(idsToContain) };
                    break;
                case RuleConditionType.DeviceFamily:
                    condition = new DeviceFamilyCondition() { IdIn = GetIds(idsToContain) };
                    break;
                case RuleConditionType.Segments:
                    condition = new SegmentsCondition() { SegmentIds = GetIds(idsToContain).Select(x => (long)x).ToList() };
                    break;
                default:
                    break;
            }

            var userSessionCondition = new UserSessionCondition() { Condition = condition };
            return userSessionCondition;
        }

        private static eResponseStatus GetResponseStatusByIdsToContain(eResponseStatus current, eResponseStatus responseStatus, IdsToContain enumType)
        {
            if (enumType == IdsToContain.all || enumType == IdsToContain.partial)
                return current;

            return responseStatus;
        }

        private static List<int> GetIds(IdsToContain idsToContain)
        {
            switch (idsToContain)
            {
                case IdsToContain.all: return new List<int>() { 1, 2, 3, 4, 5 };
                case IdsToContain.some: return new List<int>() { 1, 2, 3 };
                case IdsToContain.partial: return new List<int>() { 4, 5, 6 };
                case IdsToContain.none: return new List<int>() { 6, 7, 8, 9, 10 };
                default: throw new NotImplementedException();
            }
        }
    }
}