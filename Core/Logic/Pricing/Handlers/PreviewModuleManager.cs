using ApiObjects.Base;
using ApiObjects.Response;
using Core.Pricing;
using DAL;
using KLogMonitor;
using System;
using System.Data;
using System.Reflection;
using System.Threading;

namespace ApiLogic.Pricing.Handlers
{
    public class PreviewModuleManager
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());
        private static readonly Lazy<PreviewModuleManager> lazy = new Lazy<PreviewModuleManager>(() => new PreviewModuleManager(PricingDAL.Instance), LazyThreadSafetyMode.PublicationOnly);

        public static PreviewModuleManager Instance => lazy.Value;

        private readonly IPreviewModuleRepository _repository;

        public PreviewModuleManager (IPreviewModuleRepository repository)
        {
            _repository = repository;
        }

        public GenericListResponse<PreviewModule> GetPreviewModules(int groupId)
        {
            GenericListResponse<PreviewModule> response = new GenericListResponse<PreviewModule>();
            try
            {
                DataTable dt = _repository.Get_PreviewModulesByGroupID(groupId, true, true);
                response.Objects = Utils.BuildPreviewModulesFromDataTable(dt);

                if (response.Objects == null || response.Objects.Count == 0)
                {
                    response.Status.Set(new Status((int)eResponseStatus.OK, "There are no preview modules"));
                }
                else
                {
                    response.Status.Set(new Status((int)eResponseStatus.OK, eResponseStatus.OK.ToString()));
                }
            }
            catch (Exception ex)
            {
                log.Error($"Failed to get preview modules for groupID={groupId}", ex);
            }

            return response;
        }

        public GenericResponse<PreviewModule> Add(ContextData contextData, PreviewModule previewModule)
        {
            var response = new GenericResponse<PreviewModule>();
            long id = _repository.InsertPreviewModule(contextData.GroupId, previewModule.m_sName, previewModule.m_tsFullLifeCycle, previewModule.m_tsNonRenewPeriod, contextData.UserId.Value);

            if (id == 0)
            {
                log.Error($"Error while ADD PreviewModule. contextData: {contextData.ToString()}.");
                return response;
            }

            previewModule.m_nID = (int)id;
            response.Object = previewModule;
            response.Status.Set(eResponseStatus.OK);

            return response;
        }

        public Status Delete(ContextData contextData, long id)
        {
            Status result = new Status();

            try
            {
                if (!_repository.IsPreviewModuleExsitsd(contextData.GroupId, id))
                {
                    result.Set(eResponseStatus.PreviewModuleNotExist, $"PreviewModule {id} does not exist");
                    return result;
                }

                if (!_repository.DeletePreviewModule(contextData.GroupId, id, contextData.UserId.Value))
                {
                    log.Error($"Error while PreviewModule. contextData: {contextData.ToString()}.");
                    result.Set(eResponseStatus.Error);
                    return result;
                }

                result.Set(eResponseStatus.OK);
            }

            catch (Exception ex)
            {
                log.Error($"An Exception was occurred in  delete PreviewModule. contextData:{contextData.ToString()}, id:{id}.", ex);
            }

            return result;
        }
    }
}
