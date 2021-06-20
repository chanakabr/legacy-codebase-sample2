using ApiLogic.Pricing.Handlers;
using ApiObjects.Pricing;
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

namespace WebAPI.Controllers
{
    [Service("ppv")]
    public class PpvController : IKalturaController
    {
        /// <summary>
        /// Returns ppv object by internal identifier
        /// </summary>
        /// <param name="id">ppv identifier</param>
        /// <returns></returns>
        /// <remarks>Possible status codes: ModuleNotExists = 9016</remarks>     
        [Action("get")]
        [ApiAuthorize]
        static public KalturaPpv Get(long id)
        {
            KalturaPpv response = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                Func<GenericResponse<PPVModule>> getFunc = () => PPVManager.Instance.Get(groupId, id);
                response = ClientUtils.GetResponseFromWS<KalturaPpv, PPVModule>(getFunc);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;          
        }

        /// <summary>
        /// Returns all ppv objects
        /// </summary>  
        /// <param name="filter">Filter parameters for filtering out the result</param>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaPpvListResponse List(KalturaPpvFilter filter = null)
        {
            KalturaPpvListResponse result = new KalturaPpvListResponse();

            try
            {
                int groupId = KS.GetFromRequest().GroupId;

                if (filter == null)
                    filter = new KalturaPpvFilter();

                var coreFilter = AutoMapper.Mapper.Map<PpvByIdInFilter>(filter);

                Func<GenericListResponse<PPVModule>> getListFunc = () => PPVManager.Instance.GetPPVModulesData(groupId, coreFilter);

                KalturaGenericListResponse<KalturaPpv> response =
                   ClientUtils.GetResponseListFromWS < KalturaPpv, PPVModule>(getListFunc);

                result.Ppvs = response.Objects;
                result.TotalCount = response.TotalCount;

            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

        /// <summary>
        /// Internal API !!! Insert new ppv for partner
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="ppv">ppv object</param>
        [Action("add")]
        [ApiAuthorize]
        static public KalturaPpv Add(KalturaPpv ppv)
        {

            KalturaPpv result = null;
            ppv.ValidateForAdd();
            var contextData = KS.GetContextData();

            Func<PPVModule, GenericResponse<PPVModule>> insertFunc = (PPVModule objectToInsert) =>
                      PPVManager.Instance.Add(contextData, objectToInsert);

            result = ClientUtils.GetResponseFromWS<KalturaPpv, PPVModule>(ppv, insertFunc);

            return result;
        }

        /// <summary>
        /// Internal API !!! Delete ppv 
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">PPV id</param>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.ModuleNotExists)]
        static public bool Delete(long id)
        {
            bool result = false;

            var contextData = KS.GetContextData();

            try
            {
                Func<Status> delete = () => PPVManager.Instance.Delete(contextData, id);

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