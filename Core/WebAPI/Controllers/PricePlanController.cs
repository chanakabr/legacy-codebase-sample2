using ApiLogic.Pricing.Handlers;
using ApiObjects.Response;
using Core.Pricing;
using KLogMonitor;
using System;
using System.Reflection;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("pricePlan")]
    public class PricePlanController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Returns a list of price plans by IDs
        /// </summary>
        /// <param name="filter">Filter request</param>
        [Action("list")] 
        [ApiAuthorize]
        static public KalturaPricePlanListResponse List(KalturaPricePlanFilter filter = null)
        {

            int groupId = KS.GetFromRequest().GroupId;
            KalturaPricePlanListResponse result = new KalturaPricePlanListResponse();

            try
            {
                Func<GenericListResponse<UsageModule>> getListFunc = () =>
                   PricePlanManager.Instance.GetPricePlans(groupId, filter?.GetIdIn());

                KalturaGenericListResponse<KalturaPricePlan> response =
                    ClientUtils.GetResponseListFromWS<KalturaPricePlan, UsageModule>(getListFunc);

                result.PricePlans = response.Objects;
                result.TotalCount = response.TotalCount;

            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

        /// <summary>
        /// Updates a price plan
        /// </summary>
        /// <param name="pricePlan">Price plan to update</param>
        /// <param name="id">Price plan ID</param>
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.PricePlanDoesNotExist)]
        [Throws(eResponseStatus.PriceDetailsDoesNotExist)]
        static public KalturaPricePlan Update(long id, KalturaPricePlan pricePlan)
        {
            KalturaPricePlan result = null;

            int groupId = KS.GetFromRequest().GroupId;

            if (id == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "id");
            }

            try
            {
                Func<UsageModule, GenericResponse<UsageModule>> insertPriceDetailsFunc = (UsageModule pricePlanToUpdate) =>
                        PricePlanManager.Instance.Update(groupId, (int)id, pricePlanToUpdate);

                result = ClientUtils.GetResponseFromWS<KalturaPricePlan, UsageModule>(pricePlan, insertPriceDetailsFunc);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

        /// <summary>
        /// Internal API !!! Delete PricePlan
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">PricePlan identifier</param>
        [Action("delete")]
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        [Throws(eResponseStatus.PricePlanDoesNotExist)]
        static public bool Delete(long id)
        {
            bool result = false;

            var contextData = KS.GetContextData();

            try
            {
                Func<Status> delete = () => PricePlanManager.Instance.Delete(contextData, id);
                return ClientUtils.GetResponseStatusFromWS(delete);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

        /// <summary>
        /// Internal API !!!  Insert new PriceDetails for partner
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="pricePlan">Price plan Object</param>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.InvalidArgumentValue)]
        [Throws(eResponseStatus.InvalidPriceCode)]
        [Throws(eResponseStatus.PriceCodeDoesNotExist)]
        static public KalturaPricePlan Add(KalturaPricePlan pricePlan)
        {
            KalturaPricePlan result = null;

            pricePlan.ValidateForAdd();

            var contextData = KS.GetContextData();

            try
            {
                Func<UsageModule, GenericResponse<UsageModule>> insertPriceDetailsFunc = (UsageModule pricePlanToInsert) =>
                        PricePlanManager.Instance.Add(contextData, pricePlanToInsert);

                result = ClientUtils.GetResponseFromWS<KalturaPricePlan, UsageModule>(pricePlan, insertPriceDetailsFunc);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }
    }
}