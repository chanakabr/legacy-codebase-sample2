using System;
using System.Collections.Generic;
using System.Linq;
using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Response;
using Core.Catalog.CatalogManagement;
using KLogMonitor;

namespace ApiLogic.Api.Validators
{
    internal class RegionValidator : IRegionValidator
    {
        private static readonly KLogger log = new KLogger(nameof(RegionValidator));

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

            if (!HasLinearChannelsValidFormat(regionToAdd, out var hasInvalidFormatStatus))
            {
                return hasInvalidFormatStatus;
            }

            if (HaveDuplicatedChannelsInParent(regionToAdd, parentRegion, out var duplicatedParentChannelsMessage))
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

            if (!HasLinearChannelsValidFormat(regionToUpdate, out var hasInvalidFormatStatus))
            {
                return hasInvalidFormatStatus;
            }

            if (HaveDuplicatedChannelsInParent(regionToUpdate, parentRegion, out var duplicatedParentChannelsMessage))
            {
                return new Status(eResponseStatus.ParentAlreadyContainsChannel, duplicatedParentChannelsMessage);
            }

            if (HaveDuplicatedChannelsInSubregions(groupId, regionToUpdate, out var duplicatedSubregionsChannelsMessage))
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

            var filter = new RegionFilter { RegionIds = regionUniqueIds };
            var regionsResult = RegionManager.GetRegions(groupId, filter);
            if (regionsResult.HasObjects())
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

            var status = listOfErrors.Any()
                ? new Status(eResponseStatus.Error, string.Join(" ", listOfErrors))
                : new Status(eResponseStatus.OK);

            return status;
        }

        private bool IsFound(int groupId, Region regionToUpdate, Region existingRegion)
        {
            if (existingRegion == null)
            {
                log.ErrorFormat("Region wasn't found. groupId:{0}, id:{1}", groupId, regionToUpdate.id);
                return false;
            }

            return true;
        }

        private bool IsUniqueExternalId(int groupId, Region regionToValidate, Region existingRegion)
        {
            if (!string.IsNullOrEmpty(regionToValidate.externalId) && regionToValidate.externalId != existingRegion?.externalId)
            {
                var filter = new RegionFilter { ExternalIds = new List<string> { regionToValidate.externalId } };
                var regionsResult = RegionManager.GetRegions(groupId, filter);
                if (regionsResult.HasObjects())
                {
                    log.ErrorFormat("Region external ID already exists. groupId:{0}, externalId:{1}", groupId, regionToValidate.externalId);
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
                var subRegionsResult = RegionManager.GetRegions(groupId, filterParent);
                if (subRegionsResult.HasObjects())
                {
                    log.ErrorFormat("Sub region cannot be parent region. groupId:{0}, regionId:{1}", groupId, existingRegion.id);
                    return false;
                }
            }

            return true;
        }

        private bool IsParentFound(int groupId, Region regionToValidate, Region parentRegion)
        {
            if (parentRegion == null)
            {
                log.ErrorFormat("Parent region wasn't found. groupId:{0}, id:{1}", groupId, regionToValidate.parentId);
                return false;
            }

            return true;
        }

        private bool IsAllowedToBeParent(int groupId, Region parentRegion)
        {
            if (parentRegion.parentId > 0)
            {
                log.ErrorFormat("Parent region cannot be parent. groupId:{0}, id:{1}", groupId, parentRegion.id);
                return false;
            }

            return true;
        }

        private bool CanParentBeRemoved(int groupId, Region regionToUpdate, Region existingRegion)
        {
            if (regionToUpdate.parentId == 0 && existingRegion.parentId > 0)
            {
                var filter = new RegionFilter { ParentId = existingRegion.parentId };
                var regionsResult = RegionManager.GetRegions(groupId, filter);
                if (regionsResult.HasObjects())
                {
                    return false;
                }
            }

            return true;
        }

        private bool HasLinearChannelsValidFormat(Region regionToValidate, out Status validationStatus)
        {
            validationStatus = Status.Ok;
            if (regionToValidate.linearChannels == null)
            {
                return true;
            }

            var linearChannels = new HashSet<string>();
            foreach (var item in regionToValidate.linearChannels)
            {
                if (!int.TryParse(item.key, out _))
                {
                    validationStatus = new Status(eResponseStatus.InputFormatIsInvalid, $"The channel id {item.key} is invalid");
                }

                if (!int.TryParse(item.value, out _))
                {
                    validationStatus = new Status(eResponseStatus.InputFormatIsInvalid, $"The channel number {item.value} is invalid");
                }

                if (linearChannels.Contains(item.key))
                {
                    validationStatus = new Status(eResponseStatus.DuplicateRegionChannel, $"Channel ID, {item.key}: the channel or its LCN already appears in this bouquet or one of its subbouquets.");
                }

                linearChannels.Add(item.key);
            }

            return validationStatus.IsOkStatusCode();
        }

        private bool HaveDuplicatedChannelsInParent(Region regionToValidate, Region parentRegion, out string message)
        {
            message = null;
            if (parentRegion == null || regionToValidate.linearChannels == null)
            {
                return false;
            }

            var duplicatedChannelIds = parentRegion.linearChannels
                .Where(x => regionToValidate.linearChannels.Any(validatedLinearChannel => validatedLinearChannel.key == x.key || validatedLinearChannel.value == x.value))
                .Select(x => x.key)
                .ToArray();
            if (duplicatedChannelIds.Any())
            {
                message = $"For the following channel(s), the channel or its LCN already appears in the parent bouquet: {string.Join(",", duplicatedChannelIds)}.";

                return true;
            }

            return false;
        }

        private bool HaveDuplicatedChannelsInSubregions(int groupId, Region regionToValidate, out string message)
        {
            message = null;

            if (regionToValidate.linearChannels == null)
            {
                return false;
            }

            var filterParent = new RegionFilter { ParentId = regionToValidate.id, ExclusiveLcn = true };
            var subRegionsResult = RegionManager.GetRegions(groupId, filterParent);
            if (subRegionsResult.HasObjects())
            {
                foreach (var subRegion in subRegionsResult.Objects)
                {
                    var duplicatedChannelIds = subRegion.linearChannels
                        .Where(x => regionToValidate.linearChannels.Any(validatedLinearChannel => validatedLinearChannel.key == x.key || validatedLinearChannel.value == x.value))
                        .Select(x => x.key)
                        .ToArray();

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
            var linearChannelIds = regionToValidate.linearChannels?.Select(x => long.Parse(x.key)).ToList();
            if (linearChannelIds?.Count > 0 && !ValidateLinearChannelsExist(groupId, linearChannelIds))
            {
                log.ErrorFormat("One or more of the assets in linear channel list does not exist. groupId:{0}, id:{1}", groupId, regionToValidate.id);
                return false;
            }

            return true;
        }

        private List<string> ValidateRegionsConsistency(int groupId, long linearChannelNumber, RegionChannelNumber regionChannelNumber, IReadOnlyCollection<Region> existingRegions)
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

                var parentRegion = region.parentId > 0
                    ? RegionManager.GetRegion(groupId, region.parentId)
                    : null;
                var channelNumberValidationResults = ValidateChannelNumbersAcrossRegions(groupId, linearChannelNumber, regionChannelNumber, parentRegion);
                if (channelNumberValidationResults.Any())
                {
                    listOfErrors.AddRange(channelNumberValidationResults);
                }
            }

            return listOfErrors;
        }

        private List<string> ValidateChannelNumbersAcrossRegions(int groupId, long linearChannelNumber, RegionChannelNumber regionChannelNumber, Region parentRegion)
        {
            var validationResults = new List<string>();

            var regionToUpdate = new Region { id = regionChannelNumber.RegionId };
            regionToUpdate.linearChannels.Add(new ApiObjects.KeyValuePair(linearChannelNumber.ToString(), regionChannelNumber.ChannelNumber.ToString()));

            if (HaveDuplicatedChannelsInParent(regionToUpdate, parentRegion, out var duplicatedParentChannelsMessage))
            {
                validationResults.Add(duplicatedParentChannelsMessage);
            }

            if (HaveDuplicatedChannelsInSubregions(groupId, regionToUpdate, out var duplicatedSubregionsChannelsMessage))
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
                var allAssets = AssetManager.GetAssets(groupId, assets, true);

                return allAssets?.Count == linearChannelIds.Count;
            }
            catch (Exception ex)
            {
                log.Error(ex.Message);
            }

            return false;
        }
    }
}