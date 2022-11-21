using ApiObjects.Base;
using ApiObjects.Pricing;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.Pricing;
using DAL;
using Phx.Lib.Log;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using ApiObjects.Pricing;
using CachingProvider.LayeredCache;

namespace ApiLogic.Pricing.Handlers
{
    public interface IUsageModuleManager
    {
        GenericResponse<UsageModule> GetUsageModuleById(int groupId, long usageModuleId);
    }
    public class UsageModuleManager : IUsageModuleManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<UsageModuleManager> lazy = new Lazy<UsageModuleManager>(() => new UsageModuleManager(
            PricingDAL.Instance, LayeredCache.Instance), LazyThreadSafetyMode.PublicationOnly);
        public static UsageModuleManager Instance => lazy.Value;

        private readonly IModuleManagerRepository _repository;
        private readonly ILayeredCache _layeredCache;

        public UsageModuleManager(IModuleManagerRepository repository, ILayeredCache layeredCache)
        {
            _repository = repository;
            _layeredCache = layeredCache;
        }

        public GenericListResponse<UsageModule> GetUsageModules(int groupId, int? usageModuleId)
        { 
            GenericListResponse<UsageModule> response = new GenericListResponse<UsageModule>();
            List<UsageModuleDTO> usageModules = null;
            
            string key = LayeredCacheKeys.GetUsageModulesKey(groupId);
            var funcParams = new Dictionary<string, object>() { { "groupId", groupId } };
            _layeredCache.Get(key, ref usageModules, GetUsageModule, funcParams, groupId,
                LayeredCacheConfigNames.GET_GROUP_USAGE_MODULE_LAYERED_CACHE_CONFIG_NAME, new List<string>()
                    { LayeredCacheKeys.GetGroupUsageModuleInvalidationKey(groupId) });

            if (usageModules != null && usageModules.Count > 0)
            {
                // UsageModules and Price plane are in the same table in the db type 2 is Price plane the rest is UsageModules
                var usageModulesList = usageModules.FindAll(us => us.Type != 2);
                if (usageModuleId.HasValue)
                {
                    usageModulesList = usageModulesList.Where(um => um.Id == usageModuleId.Value).ToList();
                }
                
                response.Objects = ConvertToUsageModule(usageModulesList);
            }
            
            response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());
            return response;
        }
        
        private Tuple<List<UsageModuleDTO>, bool> GetUsageModule(Dictionary<string, object> funcParams)
        {
            List<UsageModuleDTO> usageModules = null;

            try
            {
                if (funcParams != null && funcParams.Count == 1 && funcParams.ContainsKey("groupId"))
                {
                    int? groupId = funcParams["groupId"] as int?;
                    usageModules = _repository.GetUsageModule(groupId.Value);
                }
            }
            catch (Exception ex)
            {
                log.Error(string.Format("GetUsageModule failed, parameters : {0}", string.Join(";", funcParams.Keys)), ex);
            }
            bool res = usageModules != null;

            return new Tuple<List<UsageModuleDTO>, bool>(usageModules, res);
        }
        
        public GenericResponse<UsageModule> Add(ContextData contextData, UsageModule usageModule)
        {
            var response = new GenericResponse<UsageModule>();
            
            int id = _repository.InsertUsageModule(contextData.UserId.Value, contextData.GroupId,ConvertToUsageModuleDTO(usageModule));
            
            if (id == 0)
            {
                log.Error($"Error while InsertUsageModule. contextData: {contextData}.");
                return response;
            }

            usageModule.m_nObjectID = id;
            response.Object = usageModule;
            SetUsageModuleInvalidation(contextData.GroupId);
            response.Status.Set(eResponseStatus.OK);
            return response;
        }

        public Status Delete(ContextData contextData, long id)
        {
            Status result = new Status();

            var oldUsageModuleResponse = GetUsageModuleById(contextData.GroupId, id);
            
            if (!oldUsageModuleResponse.HasObject())
            {
                result.Set(oldUsageModuleResponse.Status);
                return result;
            }
            
            // check if usageModule exists in ppv 
            var ppvResponse = PpvManager.Instance.GetPPVModules(contextData);
            if (ppvResponse.HasObjects())
            {
                var UsageModulePpvs =ppvResponse.Objects.Where(ppv => ppv.m_oUsageModule.m_nObjectID == id).ToList();
                if (UsageModulePpvs.Count > 0)
                {
                    result.Set(eResponseStatus.UsageModuleExistInPpv);
                    return result;
                }
            }
            if (!_repository.DeletePricePlan(contextData.GroupId, id, contextData.UserId.Value))
            {
                log.Error($"Error while DeleteUsageModule. contextData: {contextData.ToString()}.");
                result.Set(eResponseStatus.Error);
                return result;
            }
            
            SetUsageModuleInvalidation(contextData.GroupId);
            result.Set(eResponseStatus.OK);
            return result;
        }
        
        public GenericResponse<UsageModule> GetUsageModuleById(int groupId, long usageModuleId)
        {
            GenericResponse<UsageModule> response = new GenericResponse<UsageModule>();
            var usageModuleResponse = GetUsageModules(groupId, (int)usageModuleId);

            if (!usageModuleResponse.HasObjects())
            {
                response.SetStatus(eResponseStatus.UsageModuleDoesNotExist, $"usageModule Code {usageModuleId} does not exist");
                return response;
            }
            response.Object = usageModuleResponse.Objects[0];
            response.SetStatus(eResponseStatus.OK);

            return response;
        }
        
        public GenericResponse<UsageModuleForUpdate> Update(int id, ContextData contextData, UsageModuleForUpdate usageModuleToUpdate)
        {
            var response = new GenericResponse<UsageModuleForUpdate>();
            var oldUsageModuleResponse = GetUsageModuleById(contextData.GroupId, id);
            if (!oldUsageModuleResponse.HasObject())
            {
                response.SetStatus(oldUsageModuleResponse.Status);
                return response;
            }
            var oldUsageModule = oldUsageModuleResponse.Object;
            Boolean shouldUpdate = usageModuleToUpdate.ShouldUpdate(oldUsageModule);
            usageModuleToUpdate.Id = id;
            
            if (shouldUpdate)
            {
                int updatedRow = _repository.UpdateUsageModule(contextData.GroupId, contextData.UserId.Value, id,
                    ConvertToUsageModuleDTO(usageModuleToUpdate));

                if (updatedRow > 0)
                {
                    SetUsageModuleInvalidation(contextData.GroupId);
                    response.Object = usageModuleToUpdate;
                    response.SetStatus(eResponseStatus.OK);
                }
            }
            else
            {
                response.Object = usageModuleToUpdate;
                response.SetStatus(eResponseStatus.OK);
            }
            
            return response;
        }

        private List<UsageModule> ConvertToUsageModule(List<UsageModuleDTO> usageModuleDTOList)
        {
            List<UsageModule> usageModuleList = new List<UsageModule>(usageModuleDTOList.Count);
            foreach (UsageModuleDTO usageModuleDto in usageModuleDTOList)
            {
                var usageModule = new UsageModule();
                usageModule.Initialize(usageModuleDto.MaxNumberOfViews, usageModuleDto.TsViewLifeCycle, 
                    usageModuleDto.TsMaxUsageModuleLifeCycle, usageModuleDto.Id, usageModuleDto.VirtualName, usageModuleDto.Waiver, usageModuleDto.WaiverPeriod, usageModuleDto.IsOfflinePlayBack);
                usageModuleList.Add(usageModule);
            }
            
            return usageModuleList;
        }
        
        private UsageModuleDTO ConvertToUsageModuleDTO(UsageModule usageModule)
        {
            return new UsageModuleDTO()
            {
                Id = usageModule.m_nObjectID,
                VirtualName  =  usageModule.m_sVirtualName,
                MaxNumberOfViews =  usageModule.m_nMaxNumberOfViews,
                TsViewLifeCycle  =  usageModule.m_tsViewLifeCycle,
                TsMaxUsageModuleLifeCycle =  usageModule.m_tsMaxUsageModuleLifeCycle,
                Waiver =  usageModule.m_bWaiver,
                WaiverPeriod = usageModule.m_nWaiverPeriod,
                IsOfflinePlayBack =  usageModule.m_bIsOfflinePlayBack
            };
        }
        
        private UsageModuleDTO ConvertToUsageModuleDTO(UsageModuleForUpdate usageModule)
        {
            var usageModuleDto = new UsageModuleDTO()
            {
                Id = usageModule.Id,
                VirtualName = usageModule.Name,
            };
            if (usageModule.MaxNumberOfViews.HasValue)
                usageModuleDto.MaxNumberOfViews = usageModule.MaxNumberOfViews.Value;
            if (usageModule.Waiver.HasValue)
                usageModuleDto.Waiver = usageModule.Waiver.Value;
            if (usageModule.TsMaxUsageModuleLifeCycle.HasValue)
                usageModuleDto.TsMaxUsageModuleLifeCycle = usageModule.TsMaxUsageModuleLifeCycle.Value;
            if (usageModule.TsViewLifeCycle.HasValue)
                usageModuleDto.TsViewLifeCycle = usageModule.TsViewLifeCycle.Value;
            if (usageModule.WaiverPeriod.HasValue)
                usageModuleDto.WaiverPeriod = usageModule.WaiverPeriod.Value;
            if (usageModule.IsOfflinePlayBack.HasValue)
                usageModuleDto.IsOfflinePlayBack = usageModule.IsOfflinePlayBack.Value;
            return usageModuleDto;
        }
        
        private void SetUsageModuleInvalidation(int groupId)
        {
            // invalidation keys
            string invalidationKey = LayeredCacheKeys.GetGroupUsageModuleInvalidationKey(groupId);
            if (!_layeredCache.SetInvalidationKey(invalidationKey))
            {
                log.ErrorFormat("Failed to set invalidation key for group UsageModule. key = {0}", invalidationKey);
            }
        }
    }
}
