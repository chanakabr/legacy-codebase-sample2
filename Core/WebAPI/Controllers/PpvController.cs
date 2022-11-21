using ApiLogic.Pricing.Handlers;
using ApiObjects.Pricing;
using ApiObjects.Response;
using Core.Pricing;
using System;
using WebAPI.ClientManagers.Client;
using WebAPI.Clients;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;
using WebAPI.ObjectsConvertor.Extensions;
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
        [Throws(eResponseStatus.ModuleNotExists)]
        public static KalturaPpv Get(long id)
        {
            KalturaPpv response = null;
            
            int groupId = KS.GetFromRequest().GroupId;
            // call client                
            response = ClientsManager.PricingClient().GetPPVModuleData(groupId, id);
 
            return response;
        }

        /// <summary>
        /// Returns all ppv objects
        /// </summary>  
        /// <param name="filter">Filter parameters for filtering out the result</param>
        /// <param name="pager">Page size and index</param>
        [Action("list")]
        [ApiAuthorize]
        public static KalturaPpvListResponse List(KalturaPpvFilter filter = null, KalturaFilterPager pager = null)
        {
            KalturaPpvListResponse result = new KalturaPpvListResponse();

            if (filter == null)
                filter = new KalturaPpvFilter();
            
            if (pager == null)
                pager = new KalturaFilterPager();
            
            var coreFilter = AutoMapper.Mapper.Map<PpvFilter>(filter);
            var contextData = KS.GetContextData();

            Func<GenericListResponse<PPVModule>> getListFunc = () =>
                PpvManager.Instance.GetPPVModules(contextData, filter?.GetIdIn(), false, filter.CouponGroupIdEqual,
                    filter.AlsoInactive.HasValue ? filter.AlsoInactive.Value : false, coreFilter.OrderBy,
                    pager.GetRealPageIndex(), pager.PageSize.Value, false);
            KalturaGenericListResponse<KalturaPpv> response =
                ClientUtils.GetResponseListFromWS<KalturaPpv, PPVModule>(getListFunc);

            result.Ppvs = response.Objects;
            result.TotalCount = response.TotalCount;

            return result;
    }
        
        /// <summary>
        /// Add new ppv
        /// </summary>  
        /// <param name="ppv">ppv objec</param>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.PriceDetailsDoesNotExist)]
        [Throws(eResponseStatus.UsageModuleDoesNotExist)]
        [Throws(eResponseStatus.DiscountCodeNotExist)]
        [Throws(eResponseStatus.AssetUserRuleDoesNotExists)]
        public static KalturaPpv Add(KalturaPpv ppv)
        {
            KalturaPpv result = null;
            ppv.ValidateForAdd();
            var contextData = KS.GetContextData();
            
            Func<PpvModuleInternal, GenericResponse<PpvModuleInternal>> insertPpvFunc = (ppvToInsert) =>
                PpvManager.Instance.Add(contextData, ppvToInsert);
            result = ClientUtils.GetResponseFromWS(ppv, insertPpvFunc);
            
            if (result != null)
            {
                contextData.UserId = null;
                Func<GenericResponse<PPVModule>> getFunc = () =>
                    PpvManager.Instance.GetPpvById(contextData, long.Parse(result.Id));
                 result = ClientUtils.GetResponseFromWS<KalturaPpv, PPVModule>(getFunc);
            }
            

            return result;
        }
        
        /// <summary>
        /// Delete Ppv
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">Ppv id</param>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.PpvModuleNotExist)]
        public static bool Delete(long id)
        {
            bool result = false;

            var contextData = KS.GetContextData();
            
            Func<Status> delete = () => PpvManager.Instance.Delete(contextData, id);

            result = ClientUtils.GetResponseStatusFromWS(delete);

            return result;
        }
        /// <summary>
        /// Update ppv
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">ppv id</param>
        /// <param name="ppv">ppv Object</param>
        [Action("update")]
        [Throws(eResponseStatus.PpvModuleNotExist)]
        [Throws(eResponseStatus.PriceDetailsDoesNotExist)]
        [Throws(eResponseStatus.UsageModuleDoesNotExist)]
        [Throws(eResponseStatus.DiscountCodeNotExist)]
        [ApiAuthorize]
        static public KalturaPpv Update(int id, KalturaPpv ppv)
        {
            KalturaPpv result = null;
            var contextData = KS.GetContextData();
            
            Func<PpvModuleInternal, GenericResponse<PpvModuleInternal>> updateppvModuleFunc = ppvModuleForUpdate =>
                PpvManager.Instance.Update(id, contextData, ppvModuleForUpdate);

            result = ClientUtils.GetResponseFromWS(ppv, updateppvModuleFunc);
            
            if (result != null)
            {
                contextData.UserId = null;
                Func<GenericResponse<PPVModule>> getFunc = () =>
                    PpvManager.Instance.GetPpvById(contextData, long.Parse(result.Id), true);
                result = ClientUtils.GetResponseFromWS<KalturaPpv, PPVModule>(getFunc);
            }

            return result;
        }   
    }
}
