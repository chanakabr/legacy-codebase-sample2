using ApiLogic.Pricing.Handlers;
using ApiObjects.Response;
using Core.Pricing;
using System;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;
using WebAPI.ModelsValidators;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("usageModule")]
    class UsageModuleController : IKalturaController
    {
        /// <summary>
        /// Returns the list of available usage module
        /// </summary>
        /// /// <param name="filter">Filter request</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaUsageModuleListResponse List(KalturaUsageModuleFilter filter = null)
        {
            int groupId = KS.GetFromRequest().GroupId;
            KalturaUsageModuleListResponse result = new KalturaUsageModuleListResponse();

            try
            {
                Func<GenericListResponse<UsageModule>> getListFunc = () =>
                  UsageModuleManager.Instance.GetUsageModules(groupId, filter?.IdEqual);

                KalturaGenericListResponse<KalturaUsageModule> response =
                    ClientUtils.GetResponseListFromWS<KalturaUsageModule, UsageModule>(getListFunc);

                result.UsageModules = response.Objects;
                result.TotalCount = response.TotalCount;
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

        /// <summary>
        /// Insert new UsageModule
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="usageModule">usage module Object</param>
        [Action("add")]
        [ApiAuthorize]
        static public KalturaUsageModule Add(KalturaUsageModule usageModule)
        {
            KalturaUsageModule result = null;

            usageModule.ValidateForAdd();

            var contextData = KS.GetContextData();

            try
            {
                Func<UsageModule, GenericResponse<UsageModule>> insertUsageModuleFunc = (UsageModule usageModuleToInsert) =>
                        UsageModuleManager.Instance.Add(contextData, usageModuleToInsert);

                result = ClientUtils.GetResponseFromWS<KalturaUsageModule, UsageModule>(usageModule, insertUsageModuleFunc);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

        /// <summary>
        /// Delete UsageModule
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">UsageModule id</param>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.UsageModuleDoesNotExist)]
        static public bool Delete(long id)
        {
            bool result = false;

            var contextData = KS.GetContextData();

            try
            {
                Func<Status> delete = () => UsageModuleManager.Instance.Delete(contextData, id);

                result = ClientUtils.GetResponseStatusFromWS(delete);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return result;
        }
        
        /// <summary>
        /// Update usage module
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">usage module id</param>
        /// <param name="usageModule">usage module Object</param>
        [Action("update")]
        [Throws(eResponseStatus.UsageModuleDoesNotExist)]
        [ApiAuthorize]
        static public KalturaUsageModule Update(int id, KalturaUsageModule usageModule)
        {
            KalturaUsageModule result = null;
            
            var contextData = KS.GetContextData();

            try
            {
                Func<UsageModuleForUpdate, GenericResponse<UsageModuleForUpdate>> updateUsageModuleFunc = (UsageModuleForUpdate usageModuleToUpdate) =>
                    UsageModuleManager.Instance.Update(id, contextData, usageModuleToUpdate);

                result = ClientUtils.GetResponseFromWS<KalturaUsageModule, UsageModuleForUpdate>(usageModule, updateUsageModuleFunc);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }
    }
}
