using ApiLogic.Api.Managers;
using ApiObjects;
using ApiObjects.Response;
using System;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.API;
using WebAPI.Models.General;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("drmProfile")]
    public class DrmProfileController : IKalturaController
    {
        /// <summary>
        /// Returns all DRM adapters for partner
        /// </summary>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaDrmProfileListResponse List()
        {
            KalturaDrmProfileListResponse result = new KalturaDrmProfileListResponse();

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                Func<GenericListResponse<DrmAdapter>> getListFunc = () =>
                     DrmAdapterManager.Instance.GetDrmAdapters(groupId);

                KalturaGenericListResponse<KalturaDrmProfile>  response = 
                    ClientUtils.GetResponseListFromWS<KalturaDrmProfile, DrmAdapter>(getListFunc);

                result.Adapters = response.Objects;
                result.TotalCount = response.TotalCount;
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

        /// <summary>
        /// Internal API !!! Insert new DrmProfile 
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="drmProfile">Drm adapter Object</param>
        [Action("add")]
        [ApiAuthorize]
        static public KalturaDrmProfile Add(KalturaDrmProfile drmProfile)
        {
            KalturaDrmProfile result = null;

            drmProfile.ValidateForAdd();

            var contextData = KS.GetContextData();

            Func<DrmAdapter, GenericResponse<DrmAdapter>> insertDrmProfileFunc = (DrmAdapter drmProfileToInsert) =>
                      DrmAdapterManager.Instance.Add(contextData, drmProfileToInsert);

            result = ClientUtils.GetResponseFromWS<KalturaDrmProfile, DrmAdapter>(drmProfile, insertDrmProfileFunc);

            return result;
        }

        /// <summary>
        /// Internal API !!! Delete DrmProfile 
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">Drm adapter id</param>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.DrmAdapterNotExist)]
        static public bool Delete(long id)
        {
            bool result = false;

            var contextData = KS.GetContextData();

            try
            {
                Func<Status> delete = () => DrmAdapterManager.Instance.Delete(contextData, id);

                result = ClientUtils.GetResponseStatusFromWS(delete);
            }

            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }
    }
}