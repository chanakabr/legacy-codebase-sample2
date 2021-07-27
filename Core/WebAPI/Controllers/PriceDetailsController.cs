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
                   PriceDetailsManager.Instance.GetPriceCodesDataByCurrency(groupId, filter != null ? filter.GetIdIn() : null, currency);

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
        /// Internal API !!! Insert new PriceDetails for partner
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="priceDetails">PriceDetails Object</param>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.CurrencyIsMissing)]
        [Throws(eResponseStatus.InvalidCurrency)]
        [Throws(eResponseStatus.NameRequired)]
        [Throws(eResponseStatus.PriceIsMissing)]
        [Throws(eResponseStatus.AmountIsMissing)]
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
        /// Internal API !!! Delete PriceDetails 
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
    }
}