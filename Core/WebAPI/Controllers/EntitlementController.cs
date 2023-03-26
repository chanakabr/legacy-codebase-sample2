using ApiObjects.Base;
using ApiObjects.Response;
using KalturaRequestContext;
using System;
using System.Linq;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers;
using WebAPI.Managers.Models;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.ModelsValidators;
using WebAPI.ObjectsConvertor.Extensions;
using WebAPI.Utils;

namespace WebAPI.Controllers
{
    [Service("entitlement")]
    public class EntitlementController : IKalturaController
    {
        /// <summary>
        /// Immediately cancel a subscription, PPV, collection or programAssetGroupOffer. Cancel is possible only if within cancellation window and content not already consumed
        /// </summary>                
        /// <param name="assetId">The mediaFileID to cancel</param>        
        /// <param name="productType">The product type for the cancelation</param>
        [Action("cancel")]
        [ApiAuthorize]
        [OldStandardArgument("assetId", "asset_id")]
        [OldStandardArgument("productType", "transaction_type")]
        [OldStandardArgument("productType", "transactionType", sinceVersion = "4.7.0.0")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [SchemeArgument("assetId", MinInteger = 1)]
        [Throws(eResponseStatus.DomainNotExists)]
        [Throws(eResponseStatus.DomainSuspended)]
        [Throws(eResponseStatus.InvalidPurchase)]
        [Throws(eResponseStatus.CancelationWindowPeriodExpired)]
        [Throws(eResponseStatus.ContentAlreadyConsumed)]
        [Throws(eResponseStatus.CanNotCancelSubscriptionWhileDowngradeIsPending)]
        [Throws(eResponseStatus.SubscriptionCancellationIsBlocked)]
        [Throws(eResponseStatus.PaymentGatewayNotExist)]
        [Throws(eResponseStatus.PaymentGatewayExternalVerification)]
        [Throws(eResponseStatus.PaymentGatewayNotValid)]
        [Throws(eResponseStatus.ActionBlocked)]
        [Throws(StatusCode.HouseholdForbidden)]
        static public bool Cancel(int assetId, KalturaTransactionType productType)
        {
            var response = false;

            int groupId = KS.GetFromRequest().GroupId;
            try
            {
                // get domain       
                var domain = (int)HouseholdUtils.GetHouseholdIDByKS();

                // check if the user performing the action is domain master
                if (domain == 0)
                {
                    throw new ForbiddenException(ForbiddenException.HOUSEHOLD_FORBIDDEN, domain);
                }

                var userId = KS.GetFromRequest().UserId;

                // call client
                response = ClientsManager.ConditionalAccessClient().CancelServiceNow(groupId, domain, assetId, productType, false, KSUtils.ExtractKSPayload().UDID, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            if (response == false)
            {
                throw new InternalServerErrorException();
            }
            return response;
        }

        /// <summary>
        /// Immediately cancel a subscription, PPV, collection or programAssetGroupOffer. Cancel applies regardless of cancellation window and content consumption status
        /// </summary>                
        /// <param name="assetId">The mediaFileID to cancel</param>        
        /// <param name="productType">The product type for the cancelation</param>
        [Action("forceCancel")]
        [ApiAuthorize]
        [OldStandardArgument("assetId", "asset_id")]
        [OldStandardArgument("productType", "transaction_type")]
        [OldStandardArgument("productType", "transactionType", sinceVersion = "4.7.0.0")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [SchemeArgument("assetId", MinInteger = 1)]
        [Throws(eResponseStatus.DomainSuspended)]
        [Throws(eResponseStatus.InvalidPurchase)]
        [Throws(eResponseStatus.CanNotCancelSubscriptionWhileDowngradeIsPending)]
        [Throws(StatusCode.HouseholdForbidden)]
        [Throws(eResponseStatus.DomainNotExists)]
        [Throws(eResponseStatus.SubscriptionCancellationIsBlocked)]
        [Throws(eResponseStatus.PaymentGatewayNotExist)]
        [Throws(eResponseStatus.PaymentGatewayExternalVerification)]
        [Throws(eResponseStatus.PaymentGatewayNotValid)]
        [Throws(eResponseStatus.ContentAlreadyConsumed)]
        [Throws(eResponseStatus.ActionBlocked)]
        [Throws(eResponseStatus.CancelationWindowPeriodExpired)]
        static public bool ForceCancel(int assetId, KalturaTransactionType productType)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;
            try
            {

                // get domain       
                var domain = (int)HouseholdUtils.GetHouseholdIDByKS();

                // check if the user performing the action is domain master
                if (domain == 0)
                {
                    throw new ForbiddenException(ForbiddenException.HOUSEHOLD_FORBIDDEN, domain);
                }

                var userId = KS.GetFromRequest().UserId;

                // call client
                response = ClientsManager.ConditionalAccessClient().CancelServiceNow(groupId, domain, assetId, productType, true, KSUtils.ExtractKSPayload().UDID, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            if (response == false)
            {
                throw new InternalServerErrorException();
            }
            return response;
        }

        /// <summary>
        /// Cancel a household service subscription at the next renewal. The subscription stays valid till the next renewal.        
        /// </summary>        
        /// <param name="subscriptionId">Subscription Code</param>
        [Action("cancelRenewal")]
        [ApiAuthorize]
        [OldStandardArgument("subscriptionId", "subscription_id")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(StatusCode.HouseholdForbidden)]
        [Throws(eResponseStatus.DomainNotExists)]
        [Throws(eResponseStatus.DomainSuspended)]
        [Throws(eResponseStatus.InvalidPurchase)]
        [Throws(eResponseStatus.SubscriptionNotRenewable)]
        [Throws(eResponseStatus.CanNotCancelSubscriptionRenewalWhileDowngradeIsPending)]
        [Throws(eResponseStatus.SubscriptionCancellationIsBlocked)]
        [Throws(eResponseStatus.SubscriptionSetDoesNotExist)]
        [Throws(eResponseStatus.ActionBlocked)]
        static public void CancelRenewal(string subscriptionId)
        {
            int groupId = KS.GetFromRequest().GroupId;

            if (string.IsNullOrEmpty(subscriptionId))
            {
                throw new BadRequestException(BadRequestException.ARGUMENT_CANNOT_BE_EMPTY, "subscriptionId");
            }
            try
            {
                // get domain       
                var domain = (int)HouseholdUtils.GetHouseholdIDByKS();

                // check if the user performing the action is domain master
                if (domain == 0)
                {
                    throw new ForbiddenException(ForbiddenException.HOUSEHOLD_FORBIDDEN, domain);
                }

                // call client
                ClientsManager.ConditionalAccessClient().CancelSubscriptionRenewal(groupId, domain, subscriptionId, KS.GetFromRequest().UserId, KSUtils.ExtractKSPayload().UDID);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }

        /// <summary>
        /// Gets all the entitled media items for a household
        /// </summary>        
        /// <param name="filter">Request filter</param>
        [Action("listOldStandard")]
        [ApiAuthorize]
        [OldStandardAction("list")]
        [Obsolete]
        static public KalturaEntitlementListResponse ListOldStandard(KalturaEntitlementsFilter filter)
        {
            KalturaEntitlementListResponse response = new KalturaEntitlementListResponse();

            int groupId = KS.GetFromRequest().GroupId;

            try
            {
                // call client
                switch (filter.By)
                {
                    case KalturaEntityReferenceBy.user:
                        {
                            response = ClientsManager.ConditionalAccessClient().GetUserEntitlements(groupId, KS.GetFromRequest().UserId, filter.EntitlementType);
                        }
                        break;
                    case KalturaEntityReferenceBy.household:
                        {
                            response = ClientsManager.ConditionalAccessClient().GetDomainEntitlements(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(), filter.EntitlementType);
                        }
                        break;
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            if (response != null && response.TotalCount == 0)
            {
                response.TotalCount = response.Entitlements.Count;
            }

            return response;
        }

        /// <summary>
        /// Gets all the entitled media items for a household
        /// </summary>        
        /// <param name="filter">Request filter</param>
        /// <param name="pager">Request pager</param>1
        [Action("list")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_ARGUMENTS)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        static public KalturaEntitlementListResponse List(KalturaEntitlementFilter filter, KalturaFilterPager pager = null)
        {
            var contextData = KS.GetContextData();
            long? shopUserId = RequestContextUtilsInstance.Get().IsImpersonateRequest() ? contextData.GetCallerUserId() : (long?)null;

            if (pager == null)
            {
                pager = new KalturaFilterPager();
            }
            
            filter.Validate();

            KalturaEntitlementListResponse response = null;
            switch (filter)
            {
                case KalturaProgramAssetGroupOfferEntitlementFilter f:
                    response = ListByPagoEntitlementFilter(contextData, pager, f); break;
                case KalturaEntitlementFilter f:
                    response = ListByEntitlementFilter(contextData, pager, f, shopUserId); break;
                default: throw new NotImplementedException($"List for {filter.objectType} is not implemented");
            }

            if (response != null && response.TotalCount == 0)
            {
                response.TotalCount = response.Entitlements.Count;
            }

            return response;
        }

        private static KalturaEntitlementListResponse ListByPagoEntitlementFilter(ContextData contextData, KalturaFilterPager pager, KalturaProgramAssetGroupOfferEntitlementFilter filter)
        {
            return ClientsManager.ConditionalAccessClient().GetDomainEntitlements(contextData.GroupId, (int)contextData.DomainId.Value,
                           KalturaTransactionType.programAssetGroupOffer, false, pager.PageSize.Value, pager.GetRealPageIndex(), filter.OrderBy);
        }

        private static KalturaEntitlementListResponse ListByEntitlementFilter(ContextData contextData, KalturaFilterPager pager, KalturaEntitlementFilter filter, long? shopUserId)
        {
            switch (filter.EntityReferenceEqual)
            {
                case KalturaEntityReferenceBy.user:
                    {
                        return ClientsManager.ConditionalAccessClient().GetUserEntitlements(contextData.GroupId, contextData.UserId.ToString(),
                            filter.EntitlementTypeEqual ?? filter.ProductTypeEqual.Value,
                            filter.getIsExpiredEqual(), pager.PageSize.Value, pager.GetRealPageIndex(), filter.OrderBy, shopUserId);
                    }
                case KalturaEntityReferenceBy.household:
                    {
                        return ClientsManager.ConditionalAccessClient().GetDomainEntitlements(contextData.GroupId, (int)contextData.DomainId.Value,
                            filter.EntitlementTypeEqual ?? filter.ProductTypeEqual.Value,
                            filter.getIsExpiredEqual(), pager.PageSize.Value, pager.GetRealPageIndex(), filter.OrderBy, shopUserId);
                    }
            }

            return null;
        }

        /// <summary>
        /// Retrieve the household’s expired entitlements – PPV and subscriptions. Response is ordered by expiry date
        /// </summary>        
        /// <param name="filter">Request filter</param>
        /// <param name="pager">Paging the request</param>
        [Action("listExpired")]
        [ApiAuthorize]
        [Obsolete]
        static public KalturaEntitlementListResponse ListExpired(KalturaEntitlementsFilter filter, KalturaFilterPager pager = null)
        {
            KalturaEntitlementListResponse response = new KalturaEntitlementListResponse();

            int groupId = KS.GetFromRequest().GroupId;

            if (pager == null)
                pager = new KalturaFilterPager();

            try
            {
                // call client
                switch (filter.By)
                {
                    case KalturaEntityReferenceBy.user:
                        {
                            response = ClientsManager.ConditionalAccessClient().GetUserEntitlements(groupId, KS.GetFromRequest().UserId, filter.EntitlementType, true, pager.PageSize.Value, pager.GetRealPageIndex());
                        }
                        break;
                    case KalturaEntityReferenceBy.household:
                        {
                            response = ClientsManager.ConditionalAccessClient().GetDomainEntitlements(groupId, (int)HouseholdUtils.GetHouseholdIDByKS(), filter.EntitlementType, true, pager.PageSize.Value, pager.GetRealPageIndex());
                        }
                        break;
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            if (response != null && response.TotalCount == 0)
            {
                response.TotalCount = response.Entitlements.Count;
            }

            return response;
        }

        /// <summary>        
        /// Grant household for an entitlement for a PPV, Subscription or programAssetGroupOffer.
        /// </summary>
        /// <param name="contentId">Identifier for the content. Relevant only if Product type = PPV</param>
        /// <param name="productId">Identifier for the product package from which this content is offered  </param>
        /// <param name="productType">Product package type. Possible values: PPV, Subscription, Collection</param>
        /// <param name="history">Controls if the new entitlements grant will appear in the user’s history. True – will add a history entry. False (or if ommited) – no history entry will be added</param>
        [Action("grant")]
        [ApiAuthorize]
        [OldStandardArgument("productId", "product_id")]
        [OldStandardArgument("productType", "product_type")]
        [OldStandardArgument("contentId", "content_id")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [SchemeArgument("productId", MinInteger = 1)]
        [Throws(eResponseStatus.UserNotInDomain)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.UnableToPurchasePPVPurchased)]
        [Throws(eResponseStatus.UnableToPurchaseFree)]
        [Throws(eResponseStatus.UnableToPurchaseForPurchaseSubscriptionOnly)]
        [Throws(eResponseStatus.UnableToPurchaseSubscriptionPurchased)]
        [Throws(eResponseStatus.NotForPurchase)]
        [Throws(eResponseStatus.UnableToPurchaseCollectionPurchased)]
        [Throws(eResponseStatus.UnKnownPPVModule)]
        [Throws(eResponseStatus.MissingBasePackage)]
        [Throws(eResponseStatus.InvalidProductType)]
        [Throws(eResponseStatus.InvalidContentId)]
        [Throws(eResponseStatus.InvalidOffer)]
        [Throws(eResponseStatus.PurchaseFailed)]
        [Throws(eResponseStatus.PurchasePassedEntitlementFailed)]
        [Throws(eResponseStatus.IncorrectPrice)]
        [Throws(eResponseStatus.NoMediaRelatedToFile)]
        [Throws(eResponseStatus.InvalidUser)]
        [Throws(eResponseStatus.SubscriptionSetDoesNotExist)]
        [Throws(eResponseStatus.ServiceAlreadyExists)]
        [Throws(eResponseStatus.DlmExist)]
        [Throws(eResponseStatus.UnableToPurchaseProgramAssetGroupOfferPurchased)]
        static public bool Grant(int productId, KalturaTransactionType productType, bool history, int contentId = 0)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

            long domainID = HouseholdUtils.GetHouseholdIDByKS();

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().GrantEntitlements
                    (groupId, userId, domainID, contentId, productId, productType, history, string.Empty);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// (Deprecated - use Transaction.purchase) Charges a user for subscription or PPV      
        /// </summary>
        /// <remarks>
        /// Possible status codes:
        /// Price not correct = 6000, Unknown PPV module = 6001, Expired credit card = 6002, Cellular permissions error (for cellular charge) = 6003, Unknown billing provider = 6004
        /// </remarks>
        /// <param name="udid">Device UDID</param>
        /// <param name="itemId">The identifier of the item to buy, can be PPV identifier or subscription identifier</param>
        /// <param name="fileId">File identifier</param>
        /// <param name="isSubscription">True for buying subscription, false for buying ppv</param>        
        /// <param name="price">Price</param>
        /// <param name="currency">Currency</param>
        /// <param name="couponCode">Coupon code</param>
        /// <param name="extraParams">Custom extra parameters (changes between different billing providers)</param>
        /// <param name="encryptedCvv">Encrypted credit card CVV</param>
        [Action("buy")]
        [Obsolete]
        [ApiAuthorize]
        [OldStandardArgument("itemId", "item_id")]
        [OldStandardArgument("isSubscription", "is_subscription")]
        [OldStandardArgument("couponCode", "coupon_code")]
        [OldStandardArgument("extraParams", "extra_params")]
        [OldStandardArgument("encryptedCvv", "encrypted_cvv")]
        [OldStandardArgument("fileId", "file_id")]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.IncorrectPrice)]
        [Throws(eResponseStatus.UnKnownPPVModule)]
        [Throws(eResponseStatus.ExpiredCard)]
        [Throws(eResponseStatus.CellularPermissionsError)]
        [Throws(eResponseStatus.UnKnownBillingProvider)]
        static public KalturaBillingResponse Buy(string itemId, bool isSubscription, double price, string currency, string couponCode, string extraParams,
            string encryptedCvv, int fileId = 0, string udid = null)
        {
            KalturaBillingResponse response = null;

            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

            try
            {
                if (isSubscription)
                {
                    // call client
                    response = ClientsManager.ConditionalAccessClient().ChargeUserForSubscription(groupId, userId, price, currency, itemId, couponCode,
                        extraParams, udid, encryptedCvv);
                }
                else
                {
                    // call client
                    response = ClientsManager.ConditionalAccessClient().ChargeUserForMediaFile(groupId, userId, price, currency, fileId, itemId, couponCode,
                        extraParams, udid, encryptedCvv);
                }
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Reconcile the user household's entitlements with an external entitlements source. This request is frequency protected to avoid too frequent calls per household. 
        /// </summary>
        [Action("externalReconcile")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.UserNotInDomain)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.ReconciliationFrequencyLimitation)]
        [Throws(eResponseStatus.AdapterAppFailure)]
        [Throws(eResponseStatus.OSSAdapterNotExist)]
        static public bool ExternalReconcile()
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

            try
            {
                response = ClientsManager.ConditionalAccessClient().ReconcileEntitlements(groupId, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Update Kaltura Entitelment by Purchase id
        /// </summary>                
        /// <param name="id">Purchase Id</param>
        /// <param name="entitlement">KalturaEntitlement object</param>
        [Action("update")]
        [ApiAuthorize]
        [Throws(eResponseStatus.DomainNotExists)]
        [Throws(eResponseStatus.InvalidPurchase)]
        [Throws(eResponseStatus.SubscriptionNotRenewable)]
        [Throws(eResponseStatus.PaymentGatewayNotExist)]
        [Throws(eResponseStatus.PaymentGatewayNotSetForHousehold)]
        [Throws(eResponseStatus.PaymentMethodIdRequired)]
        [Throws(eResponseStatus.PaymentMethodNotSetForHousehold)]
        [Throws(eResponseStatus.PaymentMethodNotExist)]
        [Throws(eResponseStatus.PaymentGatewayNotSupportPaymentMethod)]
        [Throws(eResponseStatus.PaymentGatewayNotValid)]
        [Throws(eResponseStatus.PaymentGatewayChargeIdRequired)]
        static public KalturaEntitlement Update(int id, KalturaEntitlement entitlement)
        {
            int groupId = KS.GetFromRequest().GroupId;
            long domainID = HouseholdUtils.GetHouseholdIDByKS();

            try
            {
                if (entitlement is KalturaSubscriptionEntitlement)
                {
                    var subscriptionEntitlement = (KalturaSubscriptionEntitlement)entitlement;

                    if (subscriptionEntitlement.PaymentGatewayId == null && !subscriptionEntitlement.EndDate.HasValue)
                    {
                        throw new ClientException((int)eResponseStatus.Error, "PaymentGateway Id or End date Required");
                    }
                }
                else if (!entitlement.EndDate.HasValue)
                {
                    throw new ClientException((int)eResponseStatus.Error, "End date Required");
                }



                // call client
                return ClientsManager.ConditionalAccessClient().UpdateEntitlement(groupId, domainID, id, entitlement);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return null;
        }

        /// <summary>        
        /// Swap current entitlement (subscription) with new entitlement (subscription) - only Grant
        /// </summary>
        /// <param name="currentProductId">Identifier for the current product package</param>
        /// <param name="newProductId">Identifier for the new product package </param>
        /// <param name="history">Controls if the new entitlements swap will appear in the user’s history. True – will add a history entry. False (or if ommited) – no history entry will be added</param>
        [Action("swap")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.UserDoesNotExist)]
        [Throws(eResponseStatus.UserSuspended)]
        [Throws(eResponseStatus.SubscriptionNotRenewable)]
        [Throws(eResponseStatus.UnableToPurchaseSubscriptionPurchased)]
        [Throws(eResponseStatus.ServiceAlreadyExists)]
        [Throws(eResponseStatus.DlmExist)]
        static public bool Swap(int currentProductId, int newProductId, bool history)
        {
            bool response = false;

            int groupId = KS.GetFromRequest().GroupId;
            string userId = KS.GetFromRequest().UserId;

            try
            {
                // call client
                response = ClientsManager.ConditionalAccessClient().SwapEntitlements(groupId, userId, currentProductId, newProductId, history);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return response;
        }

        /// <summary>
        /// Cancel Scheduled Subscription
        /// </summary>
        /// <param name="scheduledSubscriptionId">Scheduled Subscription Identifier</param>
        /// <returns></returns>
        [Action("cancelScheduledSubscription")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [SchemeArgument("scheduledSubscriptionId", MinLong = 1)]
        [Throws(eResponseStatus.ScheduledSubscriptionNotFound)]
        [Throws(eResponseStatus.SubscriptionCancellationIsBlocked)]
        static public bool CancelScheduledSubscription(long scheduledSubscriptionId)
        {
            bool result = false;
            int groupId = KS.GetFromRequest().GroupId;
            long domainId = HouseholdUtils.GetHouseholdIDByKS();

            try
            {
                // call client
                result = ClientsManager.ConditionalAccessClient().CancelScheduledSubscription(groupId, domainId, scheduledSubscriptionId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }

            return result;
        }

        /// <summary>
        /// Returns the data about the next renewal 
        /// </summary>                
        /// <param name="id">Purchase Id</param>
        [Action("getNextRenewal")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.SubscriptionSetDoesNotExist)]
        [Throws(eResponseStatus.MissingBasePackage)]
        static public KalturaEntitlementRenewal GetNextRenewal(int id)
        {
            int groupId = KS.GetFromRequest().GroupId;
            long domainID = HouseholdUtils.GetHouseholdIDByKS();
            long userId = long.Parse(KS.GetFromRequest().UserId);

            try
            {

                return ClientsManager.ConditionalAccessClient().GetEntitlementNextRenewal(groupId, domainID, id, userId);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
            return null;
        }

        /// <summary>
        /// Apply new coupon for existing subscription
        /// </summary>
        /// <param name="purchaseId">purchase Id</param>
        /// <param name="couponCode">coupon Code</param>
        [Action("applyCoupon")]
        [ApiAuthorize]
        [ValidationException(SchemeValidationType.ACTION_NAME)]
        [Throws(eResponseStatus.InvalidPurchase)]
        [Throws(eResponseStatus.SubscriptionNotRenewable)]
        [Throws(eResponseStatus.SubscriptionDoesNotExist)]
        [Throws(eResponseStatus.CouponNotValid)]
        [Throws(eResponseStatus.OtherCouponIsAlreadyAppliedForSubscription)]
        [Throws(eResponseStatus.CampaignIsAlreadyAppliedForSubscription)]
        static public void ApplyCoupon(long purchaseId, string couponCode)
        {
            var groupId = KS.GetFromRequest().GroupId;
            var householdId = HouseholdUtils.GetHouseholdIDByKS();
            var userId = KS.GetFromRequest().UserId;

            try
            {
                ClientsManager.ConditionalAccessClient().ApplyCoupon(groupId, householdId, userId, purchaseId, couponCode);
            }
            catch (ClientException ex)
            {
                ErrorUtils.HandleClientException(ex);
            }
        }
    }
}