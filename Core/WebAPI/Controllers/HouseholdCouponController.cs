using ApiObjects.Base;
using ApiObjects.Pricing;
using ApiObjects.Response;
using Core.Pricing.Handlers;
using System;
using System.Linq;
using WebAPI.Clients;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.Domains;
using WebAPI.Models.General;
using WebAPI.ModelsValidators;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("householdCoupon")]
    public class HouseholdCouponController : IKalturaController
    {
        /// <summary>
        /// householdCoupon add
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="objectToAdd">householdCoupon details</param>
        [Action("add")]
        [ApiAuthorize]
        [Throws(eResponseStatus.CouponCodeIsMissing)]
        [Throws(eResponseStatus.CouponNotValid)]
        [Throws(eResponseStatus.HouseholdRequired)]
        [Throws(eResponseStatus.CouponCodeAlreadyLoaded)]
        [Throws(eResponseStatus.ExceededHouseholdCouponLimit)]
        static public KalturaHouseholdCoupon Add(KalturaHouseholdCoupon objectToAdd)
        {
            var contextData = KS.GetContextData();
            objectToAdd.ValidateForAdd();
            Func<CouponWallet, GenericResponse <CouponWallet>> addFunc = (CouponWallet coreObject) =>
                CouponWalletHandler.Instance.Add(contextData, coreObject);
            var response = ClientUtils.GetResponseFromWS(objectToAdd, addFunc);
            return response;
        }

        /// <summary>
        /// Remove coupon from household
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">Coupon code</param>
        [Action("delete")]
        [ApiAuthorize]
        [SchemeArgument("id", MinLength = 1)]
        [Throws(eResponseStatus.CouponCodeNotInHousehold)]
        [Throws(eResponseStatus.HouseholdRequired)]
        [Throws(eResponseStatus.CouponCodeIsMissing)]
        static public void Delete(string id)
        {
            var contextData = KS.GetContextData();
            Func<Status> deleteFunc = () => CouponWalletHandler.Instance.Delete(contextData, id);
            ClientUtils.GetResponseStatusFromWS(deleteFunc);
        }

        private static readonly Type HouseholdCouponFilterRelatedProfileType = typeof(KalturaHouseholdCouponCodeFilter);

        /// <summary>
        /// Gets all HouseholdCoupon items for a household
        /// </summary>
        /// <param name="filter">Filter</param>
        /// <returns></returns>
        [Action("list")]
        [ApiAuthorize]
        [Throws(eResponseStatus.HouseholdRequired)]
        static public KalturaHouseholdCouponListResponse List(KalturaHouseholdCouponFilter filter = null)
        {
            var contextData = KS.GetContextData();
            if (filter == null)
            {
                filter = new KalturaHouseholdCouponFilter();
            }
            else
            {
                filter.Validate();
            }

            var coreFilter = AutoMapper.Mapper.Map<CouponWalletFilter>(filter);

            Func<GenericListResponse<CouponWallet>> listFunc = () =>
                CouponWalletHandler.Instance.List(contextData, coreFilter);

            KalturaGenericListResponse<KalturaHouseholdCoupon> result =
               ClientUtils.GetResponseListFromWS<KalturaHouseholdCoupon, CouponWallet>(listFunc);

            var response = new KalturaHouseholdCouponListResponse
            {
                Objects = result.Objects,
                TotalCount = result.TotalCount
            };

            var responseProfile = Utils.Utils.GetResponseProfileFromRequest();
            if (response.Objects.Count > 0 && responseProfile != null)
            {
                KalturaDetachedResponseProfile profile = null;
                if (responseProfile is KalturaDetachedResponseProfile detachedResponseProfile)
                {
                    profile = detachedResponseProfile.RelatedProfiles.FirstOrDefault(x => x.Filter.GetType() == HouseholdCouponFilterRelatedProfileType);
                }

                if (profile != null && !string.IsNullOrEmpty(profile.Name))
                {
                    SetRelatedObjectsInListResponse(response, contextData, profile);
                }
            }

            return response;
        }

        private static void SetRelatedObjectsInListResponse(KalturaHouseholdCouponListResponse listResponse, ContextData contextData, KalturaDetachedResponseProfile profile)
        {
            foreach (var householdCoupon in listResponse.Objects)
            {
                var res = PricingUtils.GetCouponListResponse(contextData, householdCoupon);
                if (res != null)
                {
                    if (householdCoupon.relatedObjects == null)
                    {
                        householdCoupon.relatedObjects = new SerializableDictionary<string, IKalturaListResponse>();
                    }

                    if (!householdCoupon.relatedObjects.ContainsKey(profile.Name))
                    {
                        householdCoupon.relatedObjects.Add(profile.Name, res);
                    }
                }
            }
        }
    }
}