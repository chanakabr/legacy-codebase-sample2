using ApiObjects.Base;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Pricing;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Threading;
using System.Linq;
using ApiObjects.Pricing.Dto;
using Core.GroupManagers;
using Core.GroupManagers.Adapters;

namespace ApiLogic.Pricing.Handlers
{
    public interface IPreviewModuleCache
    {
        Dictionary<long, PreviewModule> GetGroupPreviewModules(int groupId);
        void InvalidateGroupPreviewModules(int groupId);
    }

    public class PreviewModuleManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly Lazy<PreviewModuleManager> lazy = new Lazy<PreviewModuleManager>(() =>
            new PreviewModuleManager(PricingDAL.Instance,
                                     PreviewModuleCache.Instance,
                                     GroupSettingsManagerAdapter.Instance),
            LazyThreadSafetyMode.PublicationOnly);

        public static PreviewModuleManager Instance => lazy.Value;

        private readonly IPreviewModuleRepository _repository;
        private readonly IPreviewModuleCache _previewModuleCache;
        private readonly IGroupSettingsManager _groupSettingsManager;
        
        public PreviewModuleManager(IPreviewModuleRepository repository, IPreviewModuleCache previewModuleCache, IGroupSettingsManager groupSettingsManager)
        {
            _repository = repository;
            _previewModuleCache = previewModuleCache;
            _groupSettingsManager = groupSettingsManager;
        }

        public GenericListResponse<PreviewModule> GetPreviewModules(int groupId, List<long> previewModuleIds = null)
        {
            GenericListResponse<PreviewModule> response = new GenericListResponse<PreviewModule>();

            var modules = _previewModuleCache.GetGroupPreviewModules(groupId);

            if (modules?.Count > 0)
            {
                if (previewModuleIds == null || previewModuleIds.Count == 0)
                {
                    response.Objects = modules.Values.ToList();
                }
                else
                {
                    response.Objects = modules.Where(x => previewModuleIds.Contains(x.Key)).Select(x => x.Value).ToList();
                }
            } 

            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        public GenericResponse<PreviewModule> GetPreviewModule(int groupId, long id)
        {
            GenericResponse<PreviewModule> response = new GenericResponse<PreviewModule>();
            var modules = _previewModuleCache.GetGroupPreviewModules(groupId);

            if (modules?.Count > 0 && modules.ContainsKey(id))
            {
                response.Object = modules[id];
                response.SetStatus(eResponseStatus.OK);
            }
            else 
            {
                response.SetStatus(eResponseStatus.PreviewModuleNotExist, $"PreviewModule {id} does not exist");
                return response;
            }

            return response;
        }

        public GenericResponse<PreviewModule> Add(ContextData contextData, PreviewModule previewModule)
        {
            var response = new GenericResponse<PreviewModule>();
            if (!_groupSettingsManager.IsOpc(contextData.GroupId))
            {
                response.SetStatus(eResponseStatus.AccountIsNotOpcSupported, "Account Is Not OPC Supported");
                return response;
            }
            long id = _repository.InsertPreviewModule(contextData.GroupId, ConvertToPreviewModuleDTO(previewModule), contextData.UserId.Value);

            if (id == 0)
            {
                log.Error($"Error while ADD PreviewModule. GroupId: {contextData.GroupId}, UserId: {contextData.UserId}.");
                return response;
            }

            _previewModuleCache.InvalidateGroupPreviewModules(contextData.GroupId);
            previewModule.m_nID = (int)id;
            response.Object = previewModule;
            response.SetStatus(eResponseStatus.OK);

            return response;
        }

        public Status Delete(ContextData contextData, long id)
        {
            Status result = new Status(eResponseStatus.Error);
            if (!_groupSettingsManager.IsOpc(contextData.GroupId))
            { 
                result.Set(eResponseStatus.AccountIsNotOpcSupported, "Account Is Not OPC Supported");
                return result;
            }
            var previewModuleResponse = GetPreviewModule(contextData.GroupId, id);
            if (!previewModuleResponse.HasObject())
            {
                result.Set(previewModuleResponse.Status);
                return result;
            }
            var previewModule = previewModuleResponse.Object;
            if (!_repository.DeletePreviewModule(contextData.GroupId, id, contextData.UserId.Value))
            {
                log.Error($"Error while PreviewModule. id: {id}, UserId: {contextData.UserId.Value}.");
                return result;
            }
            _previewModuleCache.InvalidateGroupPreviewModules(contextData.GroupId);
            result.Set(eResponseStatus.OK);

            return result;
        }

        public GenericResponse<PreviewModule> Update(ContextData contextData, long id, PreviewModule previewModule)
        {
            GenericResponse<PreviewModule> response = new GenericResponse<PreviewModule>();
            if (!_groupSettingsManager.IsOpc(contextData.GroupId))
            {
                response.SetStatus(eResponseStatus.AccountIsNotOpcSupported, "Account Is Not OPC Supported");
                return response;
            }
            var oldPreviewModuleResult = GetPreviewModule(contextData.GroupId, id);
            if (!oldPreviewModuleResult.HasObject())
            {
                return oldPreviewModuleResult;
            }

            var oldPreviewModule = oldPreviewModuleResult.Object;
            previewModule.m_nID = id;
            if (previewModule.IsNeedToUpdate(oldPreviewModule))
            {
                var pv = _repository.UpdatePreviewModule(id, ConvertToPreviewModuleDTO(previewModule), contextData.UserId.Value);
                if (pv > 0)
                {
                    _previewModuleCache.InvalidateGroupPreviewModules(contextData.GroupId);
                    response.Object = previewModule;
                    response.SetStatus(eResponseStatus.OK);
                }
            }
            else
            {
                response.Object = previewModule;
                response.SetStatus(eResponseStatus.OK);
            }

            return response;
        }

        private PreviewModuleDTO ConvertToPreviewModuleDTO(PreviewModule previewModule)
        {
            return new PreviewModuleDTO(previewModule.m_sName, previewModule.m_tsFullLifeCycle, previewModule.m_tsNonRenewPeriod);
        }
        private class PreviewModuleCache : IPreviewModuleCache
        {
            private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

            private static readonly Lazy<PreviewModuleCache> lazy = new Lazy<PreviewModuleCache>(() =>
                new PreviewModuleCache(LayeredCache.Instance, PricingDAL.Instance),
                LazyThreadSafetyMode.PublicationOnly);

            public static PreviewModuleCache Instance { get { return lazy.Value; } }

            private readonly ILayeredCache _layeredCache;
            private readonly IPreviewModuleRepository _repository;

            public PreviewModuleCache(ILayeredCache layeredCache, IPreviewModuleRepository repository)
            {
                _layeredCache = layeredCache;
                _repository = repository;
            }

            #region PreviewModule

            public Dictionary<long, PreviewModule> GetGroupPreviewModules(int groupId)
            {
                Dictionary<long, PreviewModule> response = new Dictionary<long, PreviewModule>();
                    var key = LayeredCacheKeys.GetGroupPreviewModulesKey(groupId);
                   if(!_layeredCache.Get(key, ref response, GetGroupPreviewModules, new Dictionary<string, object>(){ { "groupId", groupId }},
                                        groupId, LayeredCacheConfigNames.GET_GROUP_PREVIEW_MODULES, new List<string>() { LayeredCacheKeys.GetGroupPreviewModulesInvalidationKey(groupId) }))
                    log.Error($"Failed to get preview modules for groupID={groupId}");
                return response;
            }

            public void InvalidateGroupPreviewModules(int groupId)
            {
                _layeredCache.SetInvalidationKey(LayeredCacheKeys.GetGroupPreviewModulesInvalidationKey(groupId));
            }

            private Tuple<Dictionary<long, PreviewModule>, bool> GetGroupPreviewModules(Dictionary<string, object> funcParams)
            {
                Dictionary<long, PreviewModule> result = new Dictionary<long, PreviewModule>();
                bool success = false;
                try
                {
                    if (funcParams != null && funcParams.ContainsKey("groupId"))
                    {
                        int? groupId = funcParams["groupId"] as int?;
                        if (groupId.HasValue)
                        {
                            var modulesDTO = _repository.Get_PreviewModules(groupId.Value);

                            if (modulesDTO?.Count > 0)
                            {
                                result = modulesDTO.ToDictionary(x => x.Id, x => new PreviewModule(x.Id, x.Name, x.FullLifeCycle, x.NonRenewPeriod));
                            }

                            success = true;
                        }
                    }
                }
                catch (Exception ex)
                {
                    log.Error($"GetGroupPreviewModules failed, parameters : {string.Join(";", funcParams.Keys)}", ex);
                }

                return new Tuple<Dictionary<long, PreviewModule>, bool>(result, success);
            }

            #endregion
        }
    }
}
