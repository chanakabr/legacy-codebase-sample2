using ApiObjects.Response;
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
using WebAPI.Models.Pricing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [RoutePrefix("_service/couponGroup/action")]
    public class CouponGroupController : ApiController
    {
        /// <summary>
        /// Generate a coupon 
        /// </summary>
        /// <param name="id">Coupon group identifier</param>
        /// <param name="couponGenerationOptions">Coupon generation options</param>
        [Route("generate"), HttpPost]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        //[Throws(eResponseStatus.CouponNotValid)]
        public string Generate(long id, KalturaCouponGenerationOptions couponGenerationOptions)
        {
            string code = null;

            int groupId = KS.GetFromRequest().GroupId;

            //if (string.IsNullOrEmpty(code))
            //{
            //    throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "code");
            //}

            try
            {
                if (couponGenerationOptions is KalturaPublicCouponGenerationOptions)
                {
                    KalturaPublicCouponGenerationOptions couponGeneration = (KalturaPublicCouponGenerationOptions)couponGenerationOptions;

                    // call client
                    code = ClientsManager.PricingClient().GeneratePublicCode(groupId, id, HouseholdUtils.GetHouseholdIDByKS(groupId), couponGeneration.Code);
                }
                else if (couponGenerationOptions is KalturaRandomCouponGenerationOptions)
                {
                    KalturaRandomCouponGenerationOptions couponGeneration = (KalturaRandomCouponGenerationOptions)couponGenerationOptions;

                    // call client
                    code = ClientsManager.PricingClient().GenerateCode(groupId, id, HouseholdUtils.GetHouseholdIDByKS(groupId),
                        couponGeneration.NumberOfCoupons, couponGeneration.UseLetters, couponGeneration.UseNumbers, couponGeneration.UseSpecialCharacters);
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

            return code;
        }
    }
}