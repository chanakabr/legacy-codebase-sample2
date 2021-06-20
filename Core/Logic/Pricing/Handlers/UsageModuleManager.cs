using ApiObjects.Base;
using ApiObjects.Response;
using Core.Pricing;
using DAL;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Data;
using System.Reflection;
using System.Text;
using System.Threading;

namespace ApiLogic.Pricing.Handlers
{
    public class UsageModuleManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private static readonly Lazy<UsageModuleManager> lazy = new Lazy<UsageModuleManager>(() => new UsageModuleManager(
            PricingDAL.Instance), LazyThreadSafetyMode.PublicationOnly);
        public static UsageModuleManager Instance => lazy.Value;

        private readonly IModuleManagerRepository _repository;

        public UsageModuleManager(IModuleManagerRepository repository)
        {
            _repository = repository;
        }

        public GenericListResponse<UsageModule> GetUsageModules(int groupId)
        { 
            GenericListResponse<UsageModule> response = new GenericListResponse<UsageModule>();

            DataTable usageModules = _repository.GetPricePlans(groupId);

            if (usageModules != null && usageModules.Rows != null && usageModules.Rows.Count > 0)
            {
                response.Objects = Utils.BuildUsageModulesFromDataTable(usageModules).FindAll(us => us.m_type != 2);                
            }

            response.SetStatus(eResponseStatus.OK, eResponseStatus.OK.ToString());

            return response;
        }

        public GenericResponse<UsageModule> Add(ContextData contextData, UsageModule usageModule)
        {
            var response = new GenericResponse<UsageModule>();
            try
            {
                int id = _repository.InsertUsageModule(contextData.UserId.Value, contextData.GroupId, usageModule.m_sVirtualName, usageModule.m_nMaxNumberOfViews, usageModule.m_tsMaxUsageModuleLifeCycle,
                    usageModule.m_tsViewLifeCycle, usageModule.m_nWaiverPeriod, usageModule.m_bWaiver, usageModule.m_bIsOfflinePlayBack);


                if (id == 0)
                {
                    log.Error($"Error while InsertUsageModule. contextData: {contextData.ToString()}.");
                    return response;
                }

                usageModule.m_nObjectID = id;
                response.Object = usageModule;
                response.Status.Set(eResponseStatus.OK);
            }
            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in UsageModule. contextData:{contextData.ToString()}, name:{usageModule.m_sVirtualName}.", ex);
            }

            return response;
        }

        public Status Delete(ContextData contextData, long id)
        {
            Status result = new Status();

            if (!_repository.IsUsageModuleExistsById(contextData.GroupId, id))
            {
                result.Set(eResponseStatus.UsageModuleDoesNotExist, $"usage module {id} does not exist");
                return result;
            }

            if (!_repository.DeletePricePlan(contextData.GroupId, id, contextData.UserId.Value))
            {
                log.Error($"Error while DeleteUsageModule. contextData: {contextData.ToString()}.");
                result.Set(eResponseStatus.Error);
                return result;
            }

            result.Set(eResponseStatus.OK);

            return result;
        }
    }
}
