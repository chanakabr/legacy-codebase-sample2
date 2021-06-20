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
    [Service("discountDetails")]
    public class DiscountDetailsController : IKalturaController
    {
        /// <summary>
        /// Returns the list of available discounts details, can be filtered by discount codes
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        [Throws(eResponseStatus.InvalidCurrency)]
        static public KalturaDiscountDetailsListResponse List(KalturaDiscountDetailsFilter filter = null)
        {
            int groupId = KS.GetFromRequest().GroupId;
            string currency = Utils.Utils.GetCurrencyFromRequest();
            KalturaDiscountDetailsListResponse result = new KalturaDiscountDetailsListResponse();

            try
            {
                Func<GenericListResponse<DiscountDetails>> getListFunc = () =>
                  DiscountDetailsManager.Instance.GetDiscounts(groupId, filter != null ? filter.GetIdIn() : null, currency);

                KalturaGenericListResponse<KalturaDiscountDetails> response =
                    ClientUtils.GetResponseListFromWS<KalturaDiscountDetails, DiscountDetails>(getListFunc);

                result.Discounts = response.Objects;
                result.TotalCount = response.TotalCount;
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }


        /// <summary>
        /// Internal API !!! Insert new DiscountDetails for partner
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="discountDetails">Discount details Object</param>
        [Action("add")]
        [ApiAuthorize]
        static public KalturaDiscountDetails Add(KalturaDiscountDetails discountDetails)
        {
            KalturaDiscountDetails result = null;

            discountDetails.ValidateForAdd();

            var contextData = KS.GetContextData();

            try
            {
                Func<DiscountDetails, GenericResponse<DiscountDetails>> insertDiscountDetailsFunc = (DiscountDetails discountDetailsToInsert) =>
                        DiscountDetailsManager.Instance.Add(contextData, discountDetailsToInsert);

                result = ClientUtils.GetResponseFromWS<KalturaDiscountDetails, DiscountDetails>(discountDetails, insertDiscountDetailsFunc);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

        /// <summary>
        /// Internal API !!! Delete DiscountDetails
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">DiscountDetails id</param>
        [Action("delete")]
        [ApiAuthorize]
        [Throws(eResponseStatus.DiscountCodeNotExist)]
        static public bool Delete(long id)
        {
            bool result = false;

            var contextData = KS.GetContextData();

            try
            {
                Func<Status> delete = () => DiscountDetailsManager.Instance.Delete(contextData, id);

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