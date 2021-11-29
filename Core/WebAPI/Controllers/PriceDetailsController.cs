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

namespace WebAPI.Controllers
{
    [Service("priceDetails")]
    public class PriceDetailsController : IKalturaController
    {
        /// <summary>
        /// Returns the list of available prices, can be filtered by price IDs
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        [Throws(eResponseStatus.InvalidCurrency)]
        static public KalturaPriceDetailsListResponse List(KalturaPriceDetailsFilter filter = null)
        {
            int groupId = KS.GetFromRequest().GroupId;
            string currency = Utils.Utils.GetCurrencyFromRequest();
            KalturaPriceDetailsListResponse result = new KalturaPriceDetailsListResponse();

            try
            {
                Func<GenericListResponse<PriceDetails>> getListFunc = () =>
                   PriceDetailsManager.Instance.GetPriceDetailsList(groupId, filter?.GetIdIn(), currency);

                KalturaGenericListResponse<KalturaPriceDetails> response =
                    ClientUtils.GetResponseListFromWS<KalturaPriceDetails, PriceDetails>(getListFunc);

                result.Prices = response.Objects;
                result.TotalCount = response.TotalCount;

            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

        /// <summary>
        /// Insert new PriceDetails for partner
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="priceDetails">PriceDetails Object</param>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.InvalidCurrency)]
        [Throws(eResponseStatus.CountryNotFound)]
        static public KalturaPriceDetails Add(KalturaPriceDetails priceDetails)
        {
            KalturaPriceDetails result = null;

            priceDetails.ValidateForAdd();

            var contextData = KS.GetContextData();

            try
            {
                Func<PriceDetails, GenericResponse<PriceDetails>> insertPriceDetailsFunc = (PriceDetails priceDetailsToInsert) =>
                        PriceDetailsManager.Instance.Add(contextData, priceDetailsToInsert);
                result = ClientUtils.GetResponseFromWS<KalturaPriceDetails, PriceDetails>(priceDetails, insertPriceDetailsFunc);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

        /// <summary>
        /// Delete PriceDetails 
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">PriceDetails identifier</param>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.PriceDetailsDoesNotExist)]
        [SchemeArgument("id", MinLong = 1)]
        static public bool Delete(long id)
        {
            bool result = false;

            var contextData = KS.GetContextData();

            try
            {
                Func<Status> delete = () => PriceDetailsManager.Instance.Delete(contextData, id);
                return ClientUtils.GetResponseStatusFromWS(delete);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

        /// <summary>
        /// update existing PriceDetails
        /// </summary>
        /// <param name="id">id of priceDetails</param>
        /// <param name="priceDetails">priceDetails to update</param>
        /// <returns></returns>
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.InvalidCurrency)]
        [Throws(eResponseStatus.CountryNotFound)]
        [Throws(eResponseStatus.PriceDetailsDoesNotExist)]
        [SchemeArgument("id", MinLong = 1)]
        static public KalturaPriceDetails Update(long id, KalturaPriceDetails priceDetails)
        {
            KalturaPriceDetails result = null;
            var contextData = KS.GetContextData();
            priceDetails.ValidateForUpdate();

            try
            {
                priceDetails.Id = (int)id;
                Func<PriceDetails, GenericResponse<PriceDetails>> updateFunc = (PriceDetails priceDetailsToUpdate) =>
                        PriceDetailsManager.Instance.Update(contextData, priceDetailsToUpdate);

                result = ClientUtils.GetResponseFromWS<KalturaPriceDetails, PriceDetails>(priceDetails, updateFunc);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }
    }
}