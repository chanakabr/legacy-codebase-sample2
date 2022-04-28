using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Response;
using Core.Catalog.CatalogManagement;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace ApiLogic.Api.Validators
{
    public class RegionValidator : IRegionValidator
    {
        private static readonly KLogger Log = new KLogger(nameof(RegionValidator));

        private static readonly Lazy<RegionValidator> Lazy = new Lazy<RegionValidator>(
            () => new RegionValidator(GeneralPartnerConfigManager.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        private readonly IGeneralPartnerConfigManager _generalPartnerConfigManager;

        public static RegionValidator Instance => Lazy.Value;

        public RegionValidator(IGeneralPartnerConfigManager generalPartnerConfigManager)
        {
            _generalPartnerConfigManager = generalPartnerConfigManager;
        }

        public Status IsValidToAdd(int groupId, Region regionToAdd)
        {
            if (!IsUniqueExternalId(groupId, regionToAdd, null))
            {
                return new Status((int)eResponseStatus.ExternalIdAlreadyExists, "Region external ID already exists");
            }

            Region parentRegion = null;
            if (regionToAdd.parentId > 0)
            {
                parentRegion = RegionManager.GetRegion(groupId, regionToAdd.parentId);

                if (!IsParentFound(groupId, regionToAdd, parentRegion))
                {
                    return new Status((int)eResponseStatus.RegionNotFound, "Parent region was not found");
                }

                if (!IsAllowedToBeParent(groupId, parentRegion))
                {
                    return new Status((int)eResponseStatus.RegionCannotBeParent, "Parent region cannot be parent");
                }
            }

            var enableChannelDuplicates = _generalPartnerConfigManager.GetGeneralPartnerConfig(groupId)?.EnableMultiLcns == true;
            if (!HasLinearChannelsValidFormat(regionToAdd, enableChannelDuplicates, out var hasInvalidFormatStatus))
            {
                return hasInvalidFormatStatus;
            }

            if (HaveDuplicatedChannelsInParent(regionToAdd, parentRegion, enableChannelDuplicates, out var duplicatedParentChannelsMessage))
            {
                return new Status(eResponseStatus.ParentAlreadyContainsChannel, duplicatedParentChannelsMessage);
            }

            if (!AreLinearChannelsFound(groupId, regionToAdd))
            {
                return new Status(eResponseStatus.Error, "One or more of the assets in linear channel list does not exist");
            }

            return Status.Ok;
        }

        public Status IsValidToUpdate(int groupId, Region regionToUpdate)
        {
            var existingRegion = RegionManager.GetRegion(groupId, regionToUpdate.id);

            if (!IsFound(groupId, regionToUpdate, existingRegion))
            {
                return new Status((int)eResponseStatus.RegionNotFound, "Region was not found");
            }

            if (!IsUniqueExternalId(groupId, regionToUpdate, existingRegion))
            {
                return new Status((int)eResponseStatus.ExternalIdAlreadyExists, "Region external ID already exists");
            }

            if (!CanParentBeSet(groupId, regionToUpdate, existingRegion))
            {
                return new Status((int)eResponseStatus.RegionCannotBeParent, "Sub region cannot be parent");
            }

            var parentRegion = regionToUpdate.parentId > 0
                ? RegionManager.GetRegion(groupId, regionToUpdate.parentId)
                : null;
            if (regionToUpdate.parentId > 0 && regionToUpdate.parentId != existingRegion.parentId)
            {
                if (!IsParentFound(groupId, regionToUpdate, parentRegion))
                {
                    return new Status((int)eResponseStatus.RegionNotFound, "Parent region was not found");
                }

                if (!IsAllowedToBeParent(groupId, parentRegion))
                {
                    return new Status((int)eResponseStatus.RegionCannotBeParent, "Parent region cannot be sub region");
                }
            }

            if (!CanParentBeRemoved(groupId, regionToUpdate, existingRegion))
            {
                return new Status((int)eResponseStatus.Error, "Cannot set region to sub region");
            }

            var enableChannelDuplicates = _generalPartnerConfigManager.GetGeneralPartnerConfig(groupId)?.EnableMultiLcns == true;
            if (!HasLinearChannelsValidFormat(regionToUpdate, enableChannelDuplicates, out var hasInvalidFormatStatus))
            {
                return hasInvalidFormatStatus;
            }

            if (HaveDuplicatedChannelsInParent(regionToUpdate, parentRegion, enableChannelDuplicates, out var duplicatedParentChannelsMessage))
            {
                return new Status(eResponseStatus.ParentAlreadyContainsChannel, duplicatedParentChannelsMessage);
            }

            if (HaveDuplicatedChannelsInSubregions(groupId, regionToUpdate, enableChannelDuplicates, out var duplicatedSubregionsChannelsMessage))
            {
                return new Status(eResponseStatus.DuplicateRegionChannel, duplicatedSubregionsChannelsMessage);
            }

            if (!AreLinearChannelsFound(groupId, regionToUpdate))
            {
                return new Status(eResponseStatus.Error, "One or more of the assets in linear channel list does not exist");
            }

            return Status.Ok;
        }

        public Status IsValidToBulkUpdate(int groupId, long linearChannelId, IReadOnlyCollection<RegionChannelNumber> regionChannelNumbers)
        {
            var listOfErrors = new List<string>();

            if (!ValidateLinearChannelsExist(groupId, new[] { linearChannelId }))
            {
                listOfErrors.Add($"Asset doest not exist: groupId={groupId}, linearChannelId={linearChannelId}.");
            }

            var regionUniqueIds = regionChannelNumbers.Select(x => x.RegionId).Distinct().ToList();
            if (regionUniqueIds.Count != regionChannelNumbers.Count)
            {
                listOfErrors.Add("Region can not appear twice.");
            }

            var filter = new RegionFilter { RegionIds = regionUniqueIds, ExclusiveLcn = true };
            var regionsResult = RegionManager.Instance.GetRegions(groupId, filter);
            if (regionsResult.IsOkStatusCode())
            {
                foreach (var regionChannelNumber in regionChannelNumbers)
                {
                    var consistencyErrors = ValidateRegionsConsistency(groupId, linearChannelId, regionChannelNumber, regionsResult.Objects);
                    if (consistencyErrors.Any())
                    {
                        listOfErrors.AddRange(consistencyErrors);
                    }
                }
            }
            else
            {
                listOfErrors.Add($"Regions with id {string.Join(",", regionUniqueIds)} were not found.");
            }

            var status = listOfErrors.Any()
                ? new Status(eResponseStatus.Error, string.Join(" ", listOfErrors))
                : new Status(eResponseStatus.OK);

            return status;
        }

        private bool IsFound(int groupId, Region regionToUpdate, Region existingRegion)
        {
            if (existingRegion == null)
            {
                Log.ErrorFormat("Region wasn't found. groupId:{0}, id:{1}", groupId, regionToUpdate.id);
                return false;
            }

            return true;
        }

        private bool IsUniqueExternalId(int groupId, Region regionToValidate, Region existingRegion)
        {
            if (!string.IsNullOrEmpty(regionToValidate.externalId) && regionToValidate.externalId != existingRegion?.externalId)
            {
                var filter = new RegionFilter { ExternalIds = new List<string> { regionToValidate.externalId } };
                var regionsResult = RegionManager.Instance.GetRegions(groupId, filter);
                if (regionsResult.HasObjects())
                {
                    Log.ErrorFormat("Region external ID already exists. groupId:{0}, externalId:{1}", groupId, regionToValidate.externalId);
                    return false;
                }
            }

            return true;
        }

        private bool CanParentBeSet(int groupId, Region regionToUpdate, Region existingRegion)
        {
            if (regionToUpdate.parentId > 0 && existingRegion.parentId == 0)
            {
                var filterParent = new RegionFilter { ParentId = existingRegion.id };
                var subRegionsResult = RegionManager.Instance.GetRegions(groupId, filterParent);
                if (subRegionsResult.HasObjects())
                {
                    Log.ErrorFormat("Sub region cannot be parent region. groupId:{0}, regionId:{1}", groupId, existingRegion.id);
                    return false;
                }
            }

            return true;
        }

        private bool IsParentFound(int groupId, Region regionToValidate, Region parentRegion)
        {
            if (parentRegion == null)
            {
                Log.ErrorFormat("Parent region wasn't found. groupId:{0}, id:{1}", groupId, regionToValidate.parentId);
                return false;
            }

            return true;
        }

        private bool IsAllowedToBeParent(int groupId, Region parentRegion)
        {
            if (parentRegion.parentId > 0)
            {
                Log.ErrorFormat("Parent region cannot be parent. groupId:{0}, id:{1}", groupId, parentRegion.id);
                return false;
            }

            return true;
        }

        private bool CanParentBeRemoved(int groupId, Region regionToUpdate, Region existingRegion)
        {
            if (regionToUpdate.parentId == 0 && existingRegion.parentId > 0)
            {
                var filter = new RegionFilter { ParentId = existingRegion.parentId };
                var regionsResult = RegionManager.Instance.GetRegions(groupId, filter);
                if (regionsResult.HasObjects())
                {
                    return false;
                }
            }

            return true;
        }

        private bool HasLinearChannelsValidFormat(Region regionToValidate, bool enableChannelDuplicates, out Status validationStatus)
        {
            validationStatus = Status.Ok;
            if (regionToValidate.linearChannels == null)
            {
                return true;
            }

            var linearChannels = new HashSet<long>();
            var linearChannelNumbers = new HashSet<int>();
            foreach (var item in regionToValidate.linearChannels)
            {
                var isChannelDuplicated = linearChannels.Contains(item.Key);
                var isChannelNumberDuplicated = linearChannelNumbers.Contains(item.Value);
                if (isChannelDuplicated && !enableChannelDuplicates || isChannelNumberDuplicated)
                {
                    validationStatus = new Status(eResponseStatus.DuplicateRegionChannel, $"Channel ID, {item.Key}: the channel or its LCN already appears in this bouquet or one of its subbouquets.");
                    break;
                }

                linearChannels.Add(item.Key);
                linearChannelNumbers.Add(item.Value);
            }

            return validationStatus.IsOkStatusCode();
        }

        private bool HaveDuplicatedChannelsInParent(Region regionToValidate, Region parentRegion, bool enableChannelDuplicates, out string message)
        {
            message = null;
            if (parentRegion == null || regionToValidate.linearChannels == null)
            {
                return false;
            }

            var duplicatedChannelIds = GetDuplicatedChannelIds(parentRegion, regionToValidate, enableChannelDuplicates);
            if (duplicatedChannelIds.Any())
            {
                message = $"For the following channel(s), the channel or its LCN already appears in the parent bouquet: {string.Join(",", duplicatedChannelIds)}.";

                return true;
            }

            return false;
        }

        private bool HaveDuplicatedChannelsInSubregions(int groupId, Region regionToValidate, bool enableChannelDuplicates, out string message)
        {
            message = null;

            if (regionToValidate.linearChannels == null)
            {
                return false;
            }

            var filterParent = new RegionFilter { ParentId = regionToValidate.id, ExclusiveLcn = true };
            var subRegionsResult = RegionManager.Instance.GetRegions(groupId, filterParent);
            if (subRegionsResult.HasObjects())
            {
                foreach (var subRegion in subRegionsResult.Objects)
                {
                    var duplicatedChannelIds = GetDuplicatedChannelIds(regionToValidate, subRegion, enableChannelDuplicates);
                    if (duplicatedChannelIds.Any())
                    {
                        message = $"For the following channel(s), the channel or its LCN already appears in this bouquet or one of its subbouquets: {string.Join(",", duplicatedChannelIds)}.";

                        return true;
                    }
                }
            }

            return false;
        }

        private bool AreLinearChannelsFound(int groupId, Region regionToValidate)
        {
            var linearChannelIds = regionToValidate.linearChannels?.Select(x => x.Key).Distinct().ToList();
            if (linearChannelIds?.Count > 0 && !ValidateLinearChannelsExist(groupId, linearChannelIds))
            {
                Log.ErrorFormat("One or more of the assets in linear channel list does not exist. groupId:{0}, id:{1}", groupId, regionToValidate.id);
                return false;
            }

            return true;
        }

        private List<string> ValidateRegionsConsistency(int groupId, long linearChannelId, RegionChannelNumber regionChannelNumber, IReadOnlyCollection<Region> existingRegions)
        {
            var listOfErrors = new List<string>();

            var region = existingRegions.FirstOrDefault(x => x.id == regionChannelNumber.RegionId);
            if (region == null)
            {
                listOfErrors.Add($"Region with id {regionChannelNumber.RegionId} was not found.");
            }
            else if (regionChannelNumber.ChannelNumber > 0)
            {
                var subRegions = existingRegions.Where(x => x.parentId == region.id).ToArray();
                if (subRegions.Any())
                {
                    listOfErrors.Add($"Linear channel can not be added simultaneously into a region {region.id} and its subregions {string.Join(",", subRegions.Select(x => x.id))}.");
                }

                if (region.linearChannels.Any(x => x.Key == linearChannelId))
                {
                    listOfErrors.Add($"Linear channel already exists in the region with id {region.id}.");
                }

                var parentRegion = region.parentId > 0
                    ? RegionManager.GetRegion(groupId, region.parentId)
                    : null;
                var channelNumberValidationResults = ValidateChannelNumbersAcrossRegions(groupId, linearChannelId, regionChannelNumber, Clone(region), parentRegion);
                if (channelNumberValidationResults.Any())
                {
                    listOfErrors.AddRange(channelNumberValidationResults);
                }
            }

            return listOfErrors;
        }

        private List<string> ValidateChannelNumbersAcrossRegions(int groupId, long linearChannelId, RegionChannelNumber regionChannelNumber, Region regionToUpdate, Region parentRegion)
        {
            var validationResults = new List<string>();

            regionToUpdate.linearChannels.Add(new KeyValuePair<long, int>(linearChannelId, regionChannelNumber.ChannelNumber));
            if (regionToUpdate.linearChannels.Any(x => x.Key != linearChannelId && x.Value == regionChannelNumber.ChannelNumber))
            {
                validationResults.Add($"For the following channel, its LCN {regionChannelNumber.ChannelNumber} already appears in the region with id {regionToUpdate.id}.");
            }

            var enableChannelDuplicates = _generalPartnerConfigManager.GetGeneralPartnerConfig(groupId)?.EnableMultiLcns == true;
            if (HaveDuplicatedChannelsInParent(regionToUpdate, parentRegion, enableChannelDuplicates, out var duplicatedParentChannelsMessage))
            {
                validationResults.Add(duplicatedParentChannelsMessage);
            }

            if (HaveDuplicatedChannelsInSubregions(groupId, regionToUpdate, enableChannelDuplicates, out var duplicatedSubregionsChannelsMessage))
            {
                validationResults.Add(duplicatedSubregionsChannelsMessage);
            }

            return validationResults;
        }

        private bool ValidateLinearChannelsExist(int groupId, IReadOnlyCollection<long> linearChannelIds)
        {
            try
            {
                var assets = linearChannelIds.Select(x => new KeyValuePair<eAssetTypes, long>(eAssetTypes.MEDIA, x)).ToList();
                var allAssets = AssetManager.GetAssets(groupId, assets, true, false);

                return allAssets?.Count == linearChannelIds.Count;
            }
            catch (Exception ex)
            {
                Log.Error(ex.Message);
            }

            return false;
        }

        private static IReadOnlyCollection<long> GetDuplicatedChannelIds(Region region, Region subRegion, bool enableChannelDuplicates)
        {
            return subRegion.linearChannels
                .Where(x => region.linearChannels.Any(_ => _.Key == x.Key && !enableChannelDuplicates || _.Value == x.Value))
                .Select(x => x.Key)
                .Distinct()
                .ToArray();
        }

        private Region Clone(Region region)
        {
            return region == null
                ? null
                : new Region
                {
                    id = region.id,
                    name = region.name,
                    externalId = region.name,
                    isDefault = region.isDefault,
                    linearChannels = region.linearChannels.Select(x => new KeyValuePair<long, int>(x.Key, x.Value)).ToList(),
                    groupId = region.groupId,
                    parentId = region.parentId,
                    createDate = region.createDate,
                    childrenCount = region.childrenCount
                };
        }
    }
}