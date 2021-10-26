using ApiObjects.Base;
using ApiObjects.Pricing;
using ApiObjects.Response;
using CachingProvider.LayeredCache;
using Core.GroupManagers;
using DAL;
using DAL.Catalog;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace ApiLogic.Pricing.Handlers
{
    public interface IPartnerPremiumServicesManager
    {
        List<ServiceObject> GetAllPremiumServices();
    }

    public class PartnerPremiumServicesManager : IPartnerPremiumServicesManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<PartnerPremiumServicesManager> lazy = new Lazy<PartnerPremiumServicesManager>(() =>
            new PartnerPremiumServicesManager(PremiumServicesDal.Instance,
                                      LayeredCache.Instance,
                                      GroupSettingsManager.Instance),    
            LazyThreadSafetyMode.PublicationOnly);

        private readonly IPremiumServiceRepository _repository;
        private readonly ILayeredCache _layeredCache;
        private readonly IGroupSettingsManager _groupSettingsManager;
        
        public static PartnerPremiumServicesManager Instance => lazy.Value;
        public PartnerPremiumServicesManager(IPremiumServiceRepository repository,
                                     ILayeredCache layeredCache,
                                     IGroupSettingsManager groupSettingsManager)
        {
            _repository = repository;
            _layeredCache = layeredCache;
            _groupSettingsManager = groupSettingsManager;
        }

        public PartnerPremiumServices Get(int groupId)
        {
            var response = new PartnerPremiumServices();

            List<PartnerPremiumService> allPartnerPremiumService = new List<PartnerPremiumService>();
            _layeredCache.Get(LayeredCacheKeys.GetGroupServicesKey(groupId),
                                   ref allPartnerPremiumService,
                                   GetGroupPremiumServices,
                                   new Dictionary<string, object>() { { "groupId", groupId } },
                                   groupId,
                                   LayeredCacheConfigNames.GET_GROUP_SERVICES_LAYERED_CACHE_CONFIG_NAME,
                                   new List<string>() { LayeredCacheKeys.GetGroupPremiumServicesInvalidationKey(groupId) });

            return new PartnerPremiumServices() { Services = allPartnerPremiumService.OrderBy(r => r.Id).ToList() };
        }

        private Tuple<List<PartnerPremiumService>, bool> GetGroupPremiumServices(Dictionary<string, object> funcParams)
        {
            int? groupId = funcParams["groupId"] as int?;
            var groupServices = _repository.GetGroupPremiumServiceIds(groupId.Value);
            List<ServiceObject> allServices = GetAllPremiumServices();

            List<PartnerPremiumService> servicesList = allServices.Select(x => new PartnerPremiumService() { Id = x.ID, Name = x.Name, IsApplied = groupServices.Count == 0 ? false : groupServices.Contains(x.ID) }).ToList();

            return new Tuple<List<PartnerPremiumService>, bool>(servicesList, true);
        }

        public List<ServiceObject> GetAllPremiumServices()
        {
            List<ServiceObject> allServices = new List<ServiceObject>();

            LayeredCache.Instance.Get<List<ServiceObject>>(LayeredCacheKeys.GET_PREMIUM_SERVICES_KEY,
                                                                    ref allServices,
                                                                    GetAllPremiumServices,
                                                                    null,
                                                                    0,
                                                                    LayeredCacheConfigNames.GET_SERVICES_LAYERED_CACHE_CONFIG_NAME);
            return allServices;
        }

        private Tuple<List<ServiceObject>, bool> GetAllPremiumServices(Dictionary<string, object> arg)
        {
            List<ServiceObject> services = _repository.GetAllPremiumServices();
            return new Tuple<List<ServiceObject>, bool>(services, true);
        }

        public GenericResponse<PartnerPremiumServices> Update(ContextData contextData, PartnerPremiumServices partnerPremiumServicesToUpdate)
        {
            GenericResponse<PartnerPremiumServices> response = new GenericResponse<PartnerPremiumServices>();
            if (!_groupSettingsManager.IsOpc(contextData.GroupId))
            {
                response.SetStatus(eResponseStatus.AccountIsNotOpcSupported, eResponseStatus.AccountIsNotOpcSupported.ToString());
                return response;
            }
            if (partnerPremiumServicesToUpdate != null && partnerPremiumServicesToUpdate.Services != null)
            {
                bool needToUpdate = false;
                bool needToValidate = true;

                PartnerPremiumServices currentPartnerServices = Get(contextData.GroupId);

                if (partnerPremiumServicesToUpdate.Services.Count == 0 && currentPartnerServices?.Services.Count > 0)
                {
                    needToUpdate = true;
                    needToValidate = false;
                }

                if (needToValidate)
                {
                    response.SetStatus(ValidateServicesForUpdate(partnerPremiumServicesToUpdate.Services.Where(x => x.IsApplied).Select(x => x.Id).ToList(),
                                                                currentPartnerServices.Services,
                                                                out needToUpdate));

                    if (!response.Status.IsOkStatusCode())
                    {
                        return response;
                    }
                }

                if (needToUpdate)
                {
                    if (!_repository.UpdateGroupPremiumServices(contextData.GroupId, contextData.UserId.Value, partnerPremiumServicesToUpdate))
                    {
                        log.Error($"Error while save GroupServices. groupId: {contextData.GroupId}.");
                        return response;
                    }

                    SetPremiumServicesValidation(contextData.GroupId);
                }
            }

            response.Object = Get(contextData.GroupId);
            response.SetStatus(eResponseStatus.OK);
            return response;
        }

        private void SetPremiumServicesValidation(int groupId)
        {
            // invalidation keys
            string invalidationKey = LayeredCacheKeys.GetGroupPremiumServicesInvalidationKey(groupId);
            if (!_layeredCache.SetInvalidationKey(invalidationKey))
            {
                log.WarnFormat("Failed to set invalidation key for group PremiumServices. key = {0}", invalidationKey);
            }
        }

        private Status ValidateServicesForUpdate(List<long> servicesIds, List<PartnerPremiumService> currentServices, out bool needToUpdate)
        {
            Status status = new Status(eResponseStatus.OK);
            needToUpdate = false;

            var currServices = currentServices.Where(x => x.IsApplied).Select(x => x.Id);

            // compare current services with updated list
            if (servicesIds.Count == currServices.ToList().Count && servicesIds.All(currServices.Contains))
            {
                return status;
            }

            needToUpdate = true;

            status = ValidateServices(servicesIds);
            if (!status.IsOkStatusCode())
            {
                return status;
            }

            return status;
        }

        private Status ValidateServices(List<long> newIdsInList)
        {
            var allServices = GetAllPremiumServices();
            List<long> allServiceIds = allServices.Select(x => x.ID).ToList();

            foreach (var id in newIdsInList)
            {
                if (!allServiceIds.Contains(id))
                {
                    return new Status(eResponseStatus.PremiumServiceDoesNotExist, $"Premium Service {id} does not exist");
                }
            }

            return new Status(eResponseStatus.OK);
        }
    }
}