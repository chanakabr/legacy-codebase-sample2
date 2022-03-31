using ApiLogic.Users.Managers;
using ApiObjects;
using ApiObjects.Base;
using ApiObjects.Response;
using ApiObjects.Rules;
using ApiObjects.User.SessionProfile;
using Core.Api;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using ApiLogic.Repositories;

namespace ApiLogic.Users
{
    public interface IUserSessionProfileExpressionValidator
    {
        Status Validate(int groupId, IUserSessionProfileExpression expression);
    }

    public class UserSessionProfileExpressionValidator : IUserSessionProfileExpressionValidator
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<UserSessionProfileExpressionValidator> LazyInstance = new Lazy<UserSessionProfileExpressionValidator>(() =>
                new UserSessionProfileExpressionValidator(DeviceBrandRepository.Instance,
                    DeviceFamilyRepository.Instance,
                    Core.Api.Module.Instance,
                    DeviceReferenceDataManager.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static IUserSessionProfileExpressionValidator Instance => LazyInstance.Value;

        private readonly IDeviceBrandRepository _deviceBrandRepository;
        private readonly IDeviceFamilyRepository _deviceFamilyRepository;
        private readonly ISegmentsManager _segmentsManager;
        private readonly IDeviceReferenceDataManager _deviceReferenceDataManager;

        public UserSessionProfileExpressionValidator(IDeviceBrandRepository deviceBrandRepository,
                                         IDeviceFamilyRepository deviceFamilyRepository,
                                         ISegmentsManager segmentsManager,
                                         IDeviceReferenceDataManager deviceReferenceDataManager)
        {
            _deviceBrandRepository = deviceBrandRepository;
            _deviceFamilyRepository = deviceFamilyRepository;
            _segmentsManager = segmentsManager;
            _deviceReferenceDataManager = deviceReferenceDataManager;
        }

        public Status Validate(int groupId, IUserSessionProfileExpression expression)
        {
            switch (expression)
            {
                case ExpressionAnd c: return ValidateExpressionList(groupId, c.Expressions);
                case ExpressionOr c: return ValidateExpressionList(groupId, c.Expressions);
                case ExpressionNot c: return Validate(groupId, c.Expression);
                case UserSessionCondition c: return ValidateCondition(groupId, c.Condition);
                default: throw new NotImplementedException($"validation for expression {expression.GetType().Name} is not implemented");
            }
        }

        private Status ValidateCondition(int groupId, RuleCondition condition)
        {
            switch (condition)
            {
                case DeviceBrandCondition c: return ValidateDeviceBrandIds(groupId, c.IdIn);
                case DeviceFamilyCondition c: return ValidateDeviceFamilyIds(groupId, c.IdIn);
                case SegmentsCondition c: return ValidateSegmentIds(groupId, c.SegmentIds);
                case DeviceManufacturerCondition c: return ValidateDeviceManufacturerIds(groupId, c.IdIn);
                case DeviceModelCondition c1:
                case DynamicKeysCondition c2:
                case DeviceDynamicDataCondition c3: return Status.Ok; // no logic validation
                default: throw new NotImplementedException($"validation for condition {condition.GetType().Name} is not implemented");
            }
        }

        private Status ValidateExpressionList(int groupId, List<IUserSessionProfileExpression> expressions)
        {
            return expressions
                .Select(expression => Validate(groupId, expression))
                .FirstOrDefault(status => !status.IsOkStatusCode()) ?? Status.Ok;
        }

        private Status ValidateDeviceBrandIds(long groupId, List<int> ids)
        {
            var listResponse = _deviceBrandRepository.List(groupId);
            if (listResponse.IsOkStatusCode())
            {
                var deviceBrandMap = listResponse.Objects.Select(x => x.Id).ToHashSet();
                return AllExists(ids, deviceBrandMap, "DeviceBrand", eResponseStatus.DeviceBrandIdsDoesNotExist);
            }

            return listResponse.Status;
        }

        private Status ValidateDeviceFamilyIds(long groupId, List<int> ids)
        {
            var listResponse = _deviceFamilyRepository.List(groupId);
            if (listResponse.IsOkStatusCode())
            {
                var deviceFamiliesMap = listResponse.Objects.Select(x => x.Id).ToHashSet();
                return AllExists(ids, deviceFamiliesMap, "DeviceFamily", eResponseStatus.NonExistingDeviceFamilyIds);
            }

            return listResponse.Status;
        }

        private Status ValidateSegmentIds(int groupId, List<long> ids)
        {
            var assetSearchDefinition = new AssetSearchDefinition { IsAllowedToViewInactiveAssets = true };
            var segmentationTypes = _segmentsManager.ListSegmentationTypes(groupId, null, 0, 0, assetSearchDefinition);
            if (!segmentationTypes.HasObjects())
            {
                if (!segmentationTypes.IsOkStatusCode())
                {
                    log.Warn($"could not get segmentationTypes for groupId:{groupId}, reason: {segmentationTypes.Status}");
                }

                return new Status(eResponseStatus.SegmentsIdsDoesNotExist, $"Segment ids {string.Join(", ", ids)} does not exist");
            }

            var nonExistingIds = ids.Where(x => !segmentationTypes.Objects.Any(y => y.Value.HasSegmentId(x))).ToList();
            return nonExistingIds.Count > 0 ?
                new Status(eResponseStatus.SegmentsIdsDoesNotExist, $"Segment ids {string.Join(", ", nonExistingIds)} does not exist") : Status.Ok;
        }

        private Status ValidateDeviceManufacturerIds(int groupId, List<long> ids)
        {
            DeviceManufacturersReferenceDataFilter filter = null; // filterring by ids will be in validator to return informetive error
            var deviceReferenceDatas = _deviceReferenceDataManager.ListByManufacturer(new ContextData(groupId), filter);
            var existingIds = deviceReferenceDatas.Objects.Select(x => x.Id).ToHashSet();
            return AllExists(ids, existingIds, "DeviceManufacturer", eResponseStatus.DeviceManufacturerIdsDoesNotExist);
        }

        private Status AllExists<T>(IEnumerable<T> idsToCheck, HashSet<T> exisitingIds, string objectName, eResponseStatus errorStatus)
        {
            var nonExistingIds = idsToCheck.Where(x => !exisitingIds.Contains(x)).ToList();
            return nonExistingIds.Count > 0 ?
                new Status(errorStatus, $"{objectName} ids {string.Join(", ", nonExistingIds)} does not exist") : Status.Ok;
        }
    }
}
