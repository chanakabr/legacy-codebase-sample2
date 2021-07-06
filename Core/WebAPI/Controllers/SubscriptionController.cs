using ApiLogic.Pricing.Handlers;
using ApiObjects;
using ApiObjects.Pricing;
using ApiObjects.Response;
using Core.Pricing;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WebAPI.ClientManagers.Client;
using WebAPI.Clients;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("subscription")]
    public class SubscriptionController : IKalturaController
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        /// <summary>
        /// Returns a list of subscriptions requested by Subscription ID or file ID
        /// </summary>
        /// <param name="filter">Filter request</param>
        /// <param name="pager">Page size and index</param>
        /// <remarks>Possible status codes:      
        ///   </remarks>
        [Action("list")]
        [ApiAuthorize]
        static public KalturaSubscriptionListResponse List(KalturaSubscriptionFilter filter = null, KalturaFilterPager pager = null)
        {
            KalturaSubscriptionListResponse response = new KalturaSubscriptionListResponse();

            bool isFilterValid = false;

            if (pager == null)
            {
                pager = new KalturaFilterPager();
            }

            if (filter == null)
            {
                filter = new KalturaSubscriptionFilter();
                isFilterValid = true;
            }
            else
            {
                isFilterValid = filter.Validate();
            }

            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;
            string language = Utils.Utils.GetLanguageFromRequest();
            long userId = Utils.Utils.GetUserIdFromKs();
            bool isAllowedToViewInactiveAssets = Utils.Utils.IsAllowedToViewInactiveAssets(groupId, userId.ToString(), true);
            AssetSearchDefinition assetSearchDefinition = new AssetSearchDefinition() { Filter = filter.Ksql, UserId = userId, IsAllowedToViewInactiveAssets = isAllowedToViewInactiveAssets };
            Func<GenericListResponse<Subscription>> getListFunc;
            KalturaGenericListResponse<KalturaSubscription> result = null;

            try
            {
                var coreFilter = AutoMapper.Mapper.Map<SubscriptionFilter>(filter);

                if (filter.MediaFileIdEqual.HasValue)
                {
                    // call client
                    Func<List<int>> getSubscriptionsIdsFunc = () => SubscriptionManager.Instance.GetSubscriptionIDsContainingMediaFile(groupId, (int)filter.MediaFileIdEqual);

                    List<int> subscriptionsIds = ClientUtils.GetListIntResponseFromWS(getSubscriptionsIdsFunc);

                    // get subscriptions
                    if (subscriptionsIds != null && subscriptionsIds.Count > 0)
                    {
                        getListFunc = () =>
                            SubscriptionManager.Instance.GetSubscriptionsData(groupId, new HashSet<long>(subscriptionsIds.Select(t => (long)t).ToList()),
                                        udid, language, coreFilter.OrderBy, assetSearchDefinition, pager.getPageIndex(), pager.PageSize, filter.CouponGroupIdEqual);

                        result = ClientUtils.GetResponseListFromWS<KalturaSubscription, Subscription>(getListFunc);
                    }
                }
                else if (!string.IsNullOrEmpty(filter.SubscriptionIdIn))
                {
                    getListFunc = () =>
                        SubscriptionManager.Instance.GetSubscriptionsData(groupId, new HashSet<long>(filter.getSubscriptionIdIn()),
                                    udid, language, coreFilter.OrderBy, assetSearchDefinition, pager.getPageIndex(), pager.PageSize, filter.CouponGroupIdEqual);

                    result = ClientUtils.GetResponseListFromWS<KalturaSubscription, Subscription>(getListFunc);
                }
                else if (!string.IsNullOrEmpty(filter.ExternalIdIn))
                {
                    getListFunc = () =>
                        SubscriptionManager.Instance.GetSubscriptionsByProductCodeList(groupId, filter.getExternalIdIn(), coreFilter.OrderBy);

                    result = ClientUtils.GetResponseListFromWS<KalturaSubscription, Subscription>(getListFunc);
                }
                else if (!string.IsNullOrEmpty(filter.Ksql))
                {
                    getListFunc = () =>
                       SubscriptionManager.Instance.GetSubscriptionsData(groupId, null, udid, language, coreFilter.OrderBy, assetSearchDefinition,
                            pager.getPageIndex(), pager.PageSize, filter.CouponGroupIdEqual);

                    result = ClientUtils.GetResponseListFromWS<KalturaSubscription, Subscription>(getListFunc);
                }
                else if (isFilterValid)
                {
                    getListFunc = () =>
                      SubscriptionManager.Instance.GetSubscriptionsData(groupId, udid, language, coreFilter.OrderBy, pager.getPageIndex(), pager.PageSize, filter.CouponGroupIdEqual);

                    result = ClientUtils.GetResponseListFromWS<KalturaSubscription, Subscription>(getListFunc);
                }

                if (result != null)
                {
                    response.Subscriptions = result.Objects;
                    response.TotalCount = result.TotalCount;
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Returns a list of subscriptions requested by Subscription ID or file ID
        /// </summary>
        /// <param name="filter">Filter request</param>
        /// <remarks>Possible status codes:      
        ///   </remarks>
        [Action("listOldStandard")]
        [OldStandardAction("list")]
        [ApiAuthorize]
        [Obsolete]
        static public List<KalturaSubscription> ListOldStandard(KalturaSubscriptionsFilter filter)
        {
            List<KalturaSubscription> subscruptions = null;
            List<int> subscriptionsIds = null;

            int groupId = KS.GetFromRequest().GroupId;
            string udid = KSUtils.ExtractKSPayload().UDID;
            string language = Utils.Utils.GetLanguageFromRequest();

            if (filter.Ids == null || filter.Ids.Count() == 0)
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "KalturaSubscriptionsFilter.ids");
            }

            try
            {
                if (filter.By == KalturaSubscriptionsFilterBy.media_file_id)
                {
                    // call client
                    subscriptionsIds = ClientsManager.PricingClient().GetSubscriptionIDsContainingMediaFile(groupId, filter.Ids[0].value);

                    // get subscriptions
                    if (subscriptionsIds != null && subscriptionsIds.Count > 0)
                    {
                        subscruptions = ClientsManager.PricingClient().GetSubscriptionsData(groupId, subscriptionsIds.Select(id => id.ToString()).ToArray(), udid, language, KalturaSubscriptionOrderBy.START_DATE_ASC, null);
                    }
                }

                else if (filter.By == KalturaSubscriptionsFilterBy.subscriptions_ids)
                {
                    // call client
                    subscruptions = ClientsManager.PricingClient().GetSubscriptionsData(groupId, filter.Ids.Select(x => x.value.ToString()).ToArray(), udid, language, KalturaSubscriptionOrderBy.START_DATE_ASC, null);
                }

            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return subscruptions;
        }


        /// <summary>
        /// Returns information about a coupon for subscription
        /// </summary>
        /// <param name="id">subscription id </param>
        /// <param name="code">coupon code </param>
        /// <remarks>Possible status codes: Coupon not valid = 3020,  Coupon promotion date expired = 3057, Coupon promotion date not started = 3058
        ///   </remarks>
        [Action("validateCoupon")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [ApiAuthorize]
        static public KalturaCoupon ValidateCoupon(int id, string code)
        {
            //filter.Validate();
            KalturaCoupon response = new KalturaCoupon();

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                response = ClientsManager.PricingClient().ValidateCouponForSubscription(groupId, id, code, HouseholdUtils.GetHouseholdIDByKS(groupId));
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }


        /// <summary>
        /// Internal API !!! Insert new subscription for partner
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="subscription">subscription object</param>
        [Action("add")]
        [ApiAuthorize]
        static public KalturaSubscription Add(KalturaSubscription subscription)
        {
            KalturaSubscription result = null;
            subscription.ValidateForAdd();
            var contextData = KS.GetContextData();
            try
            {
                Func<SubscriptionInternal, GenericResponse<SubscriptionInternal>> insertSubscriptionFunc = (SubscriptionInternal subscriptionToInsert) =>
                    SubscriptionManager.Instance.Add(contextData, subscriptionToInsert);

                result = ClientUtils.GetResponseFromWS<KalturaSubscription, SubscriptionInternal>(subscription, insertSubscriptionFunc);

                if (result != null)
                {

                    Func<GenericResponse<Subscription>> getFunc = () =>
                               SubscriptionManager.Instance.GetSubscription(contextData.GroupId, long.Parse(result.Id));

                    result = ClientUtils.GetResponseFromWS<KalturaSubscription, Subscription>(getFunc);
                }               
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

        /// <summary>
        /// Internal API !!! Delete subscription 
        /// </summary>
        /// <remarks>
        /// </remarks>
        /// <param name="id">Subscription id</param>
        [Action("delete")]
        [ApiAuthorize]
        //[Throws(eResponseStatus.SubscriptionNotExist)]
        static public bool Delete(long id)
        {
            bool result = false;

            var contextData = KS.GetContextData();

            try
            {
                Func<Status> delete = () => SubscriptionManager.Instance.Delete(contextData, id);

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