using ApiObjects.Response;
using AutoMapper;
using CachingProvider.LayeredCache;
using DAL;
using DAL.DTO;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Threading;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Partner;

namespace WebAPI.ClientManagers
{
    public class GroupsManager
    {
        public static GroupsManager Instance => Lazy.Value;
        
        private static readonly KLogger Log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly Lazy<GroupsManager> Lazy = new Lazy<GroupsManager>(() =>
            new GroupsManager(LayeredCache.Instance,
                              GroupBaseConfigurationRepository.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        private readonly ILayeredCache _layeredCache;
        private readonly IGroupBaseConfigurationRepository _baseConfigurationRepository;

        public GroupsManager(ILayeredCache layeredCache, IGroupBaseConfigurationRepository baseConfigurationRepository)
        {
            _layeredCache = layeredCache;
            _baseConfigurationRepository = baseConfigurationRepository;
        }

        public Group GetGroup(int groupId)
        {
            Group group = null;
            var groupKey = _baseConfigurationRepository.GetGroupConfigKey(groupId);
            var invalidationKey = LayeredCacheKeys.PhoenixGroupsManagerInvalidationKey(groupId);
            
            if (!_layeredCache.Get(groupKey, 
                                           ref group, 
                                           BuildGroup,
                                           new Dictionary<string, object>() { { "groupId", groupId } }, 
                                           groupId, 
                                           LayeredCacheConfigNames.PHOENIX_GROUPS_MANAGER_CACHE_CONFIG_NAME,
                                           new List<string>() { invalidationKey }))
            {
                Log.ErrorFormat("Failed building Phoenix group object for groupId: {0}", groupId);
                throw new InternalServerErrorException(InternalServerErrorException.MISSING_CONFIGURATION, "Partner");
            }
            
            return group;
        }

        public Status AddBaseConfiguration(int groupId, Group group)
        {
            Status response = new Status(eResponseStatus.Error);
            group.SetDefaultValues();

            try
            {
                var groupDto = Mapper.Map<GroupDTO>(group);
                if (!_baseConfigurationRepository.SaveConfig(groupId, groupDto))
                {
                    Log.Error($"Error while add Group. groupId: {groupId}.");
                    return response;
                }

                response.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                Log.Error($"Unable to add base config. groupId:{groupId}.", ex);
            }

            return response;
        }

        // will be used later, when rollback or partner-delete will be implemented
        public Status DeleteBaseConfiguration(int groupId)
        {
            Status response = new Status(eResponseStatus.Error);

            try
            {
                var group = GetGroup(groupId);
                if (group == null)
                {
                    response.Set(eResponseStatus.Error, $"group {groupId} does not exist");
                    return response;
                }

                if (!_baseConfigurationRepository.DeleteConfig(groupId))
                {
                    Log.Error($"Unable to delete base config. groupId: {groupId}.");
                    return response;
                }

                SetInvalidationKeys(groupId);
                response.Set(eResponseStatus.OK);
            }
            catch(InternalServerErrorException)
            {
                response.Set(eResponseStatus.PartnerDoesNotExist, $"Partner {groupId} does not exist"); 
            }
            catch (Exception ex)
            {
                response.Set(eResponseStatus.Error);
                Log.Error($"An Exception was occurred in DeleteGroup. groupId:{groupId}.", ex);
            }

            return response;
        }

        public KalturaPartnerConfigurationListResponse GetBaseConfiguration(int groupId)
        {
            var result = new KalturaPartnerConfigurationListResponse();
            var group = GetGroup(groupId);

            if (group != null)
            {
                result.Objects = new List<KalturaPartnerConfiguration>() { Mapper.Map<KalturaBasePartnerConfiguration>(group) };
                result.TotalCount = 1;
            }

            return result;
        }

        private Tuple<Group, bool> BuildGroup(Dictionary<string, object> funcParams)
        {
            bool result = false;
            Group group = null;

            if (funcParams != null && funcParams.ContainsKey("groupId"))
            {
                int? groupId = funcParams["groupId"] as int?;
                if (groupId.HasValue && groupId.Value > 0)
                {
                    var groupDTO = _baseConfigurationRepository.GetConfig(groupId.Value);

                    if (groupDTO != null)
                    {
                        group = Mapper.Map<Group>(groupDTO);
                        result = true;
                    }
                }
            }

            return new Tuple<Group, bool>(group, result);
        }

        private void SetInvalidationKeys(int groupId)
        {
            var invalidationKey = LayeredCacheKeys.PhoenixGroupsManagerInvalidationKey(groupId);
            if (!_layeredCache.SetInvalidationKey(invalidationKey))
            {
                Log.Error($"Failed to set invalidation key for Group key: {invalidationKey}.");
            }
        }
    }
}