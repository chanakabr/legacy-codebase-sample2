using ApiObjects.Response;
using Core.Pricing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Web.Http;
using System.Web.Http.Description;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/couponsGroup/action")]
    public class CouponsGroupController : ApiController
    {
        /// <summary>
        /// Generate a coupon 
        /// </summary>
        /// <param name="id">Coupon group identifier</param>
        /// <param name="couponGenerationOptions">Coupon generation options</param>
        [Route("generate"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.InvalidCouponGroup)]
        [Throws(eResponseStatus.CouponCodeAlreadyExists)]
        public KalturaStringValueArray Generate(long id, KalturaCouponGenerationOptions couponGenerationOptions)
        {
            KalturaStringValueArray result = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                if (couponGenerationOptions is KalturaPublicCouponGenerationOptions)
                {
                    KalturaPublicCouponGenerationOptions couponGeneration = (KalturaPublicCouponGenerationOptions)couponGenerationOptions;

                    if (string.IsNullOrEmpty(couponGeneration.Code))
                    {
                        throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "code");
                    }

                    // call client
                    result = ClientsManager.PricingClient().GeneratePublicCode(groupId, id, couponGeneration.Code);
                }
                else if (couponGenerationOptions is KalturaRandomCouponGenerationOptions)
                {
                    KalturaRandomCouponGenerationOptions couponGeneration = (KalturaRandomCouponGenerationOptions)couponGenerationOptions;

                    if (couponGeneration.NumberOfCoupons <= 0)
                    {
                        throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "numberOfCoupons");
                    }

                    // call client
                    result = ClientsManager.PricingClient().GenerateCode(groupId, id, couponGeneration.NumberOfCoupons, couponGeneration.UseLetters, couponGeneration.UseNumbers, couponGeneration.UseSpecialCharacters);
                }
                else
                {
                    throw new InternalServerErrorException();
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

        /// <summary>
        /// Returns information about coupons group
        /// </summary>
        /// <param name="id">Coupons group ID</param>
        [Route("get"), HttpPost]
        [ApiAuthorize]
        [SchemeArgument("id", MinLong = 1)]
        public KalturaCouponsGroup Get(long id)
        {
            KalturaCouponsGroup couponsGroup = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                couponsGroup = ClientsManager.PricingClient().GetCouponsGroup(groupId, id);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return couponsGroup;
        }

        /// <summary>
        /// Update coupons group 
        /// </summary>    
        /// <param name="id">Coupons group identifier</param>        
        /// <param name="couponsGroup">Coupons group</param>        
        [Route("update"), HttpPost]
        [ApiAuthorize]
        public KalturaCouponsGroup Update(int id, KalturaCouponsGroup couponsGroup)
        {
            KalturaCouponsGroup response = null;

            try
            {
                if (id <= 0)
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "id");
                }

                int groupId = KS.GetFromRequest().GroupId;
                // call client                
                response = ClientsManager.PricingClient().UpdateCouponsGroup(groupId, couponsGroup);

            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Returns information about partner coupons groups
        /// </summary>
        [Route("list"), HttpPost]
        [ApiAuthorize]
        public KalturaCouponsGroupListResponse List()
        {
            KalturaCouponsGroupListResponse couponsGroups = null;

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                couponsGroups = ClientsManager.PricingClient().GetCouponsGroups(groupId);

            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return couponsGroups;
        }
    }
}