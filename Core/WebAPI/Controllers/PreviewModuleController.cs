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
using WebAPI.Utils;
using static WebAPI.Models.Pricing.KalturaPreviewModule;

namespace WebAPI.Controllers
{
    [Service("previewModule")]
    public class PreviewModuleController : IKalturaController
    {
        /// <summary>
        /// Returns all PreviewModule 
        /// </summary>
         /// <remarks>
        /// </remarks>
        /// <param name="filter">Filter</param>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaPreviewModuleListResponse List(KalturaPreviewModuleFilter filter = null)
        {
            KalturaPreviewModuleListResponse result = new KalturaPreviewModuleListResponse();
            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                Func<GenericListResponse<PreviewModule>> getListFunc = () =>
                     PreviewModuleManager.Instance.GetPreviewModules(groupId, filter != null ? filter.GetIdIn() : null);

                KalturaGenericListResponse<KalturaPreviewModule> response =
                    ClientUtils.GetResponseListFromWS<KalturaPreviewModule, PreviewModule>(getListFunc);

                result.PreviewModule = response.Objects;
                result.TotalCount = response.TotalCount;
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

        /// <summary>
        /// Insert new PreviewModule for partner
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="previewModule">Preview module object</param>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.AccountIsNotOpcSupported)]
        static public KalturaPreviewModule Add(KalturaPreviewModule previewModule)
        {
            KalturaPreviewModule result = null;
            previewModule.ValidateForAdd();
            var contextData = KS.GetContextData();

            Func<PreviewModule, GenericResponse<PreviewModule>> insertPreviewModuleFunc = (PreviewModule previewModuleToInsert) =>
                      PreviewModuleManager.Instance.Add(contextData, previewModuleToInsert);

            result = ClientUtils.GetResponseFromWS<KalturaPreviewModule, PreviewModule>(previewModule, insertPreviewModuleFunc);

            return result;
        }

        /// <summary>
        /// Internal API !!! Delete PreviewModule
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">PreviewModule id</param>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.PreviewModuleNotExist)]
        [Throws(eResponseStatus.AccountIsNotOpcSupported)]
        static public bool Delete(long id)
        {
            bool result = false;

            var contextData = KS.GetContextData();

            try
            {
                Func<Status> delete = () => PreviewModuleManager.Instance.Delete(contextData, id);

                result = ClientUtils.GetResponseStatusFromWS(delete);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

        /// <summary>
        /// Update PreviewModule
        /// </summary>
        /// <param name="id">PreviewModule id</param>
        /// <param name="previewModule">PreviewModule</param>
        [Action("update")]
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        [Throws(eResponseStatus.PreviewModuleNotExist)]
        [Throws(eResponseStatus.AccountIsNotOpcSupported)]
        static public KalturaPreviewModule Update(long id, KalturaPreviewModule previewModule)
        {
            KalturaPreviewModule result = null;

            var contextData = KS.GetContextData();

            try
            {
                Func<PreviewModule, GenericResponse<PreviewModule>> updatePreviewModuleFunc = (PreviewModule previewModuleToUpdate) =>
                        PreviewModuleManager.Instance.Update(contextData, id, previewModuleToUpdate);

                result = ClientUtils.GetResponseFromWS<KalturaPreviewModule, PreviewModule>(previewModule, updatePreviewModuleFunc);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }
    }
}
