using AutoMapper;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.ConditionalAccess;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;
using WebAPI.ObjectsConvertor;
using WebAPI.ObjectsConvertor.Mapping;
using WebAPI.Utils;
using System.Net;
using System.Web;
using System.ServiceModel;

namespace WebAPI.Clients
{
    public class ConditionalAccessClient : BaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public ConditionalAccessClient()
        {

        }

        #region Properties

        protected WebAPI.ConditionalAccess.module ConditionalAccess
        {
            get
            {
                return (Module as WebAPI.ConditionalAccess.module);
            }
        }

        #endregion

        public bool CancelServiceNow(int groupId, int domain_id, int asset_id, Models.ConditionalAccess.KalturaTransactionType transaction_type, bool bIsForce)
        {
            WebAPI.ConditionalAccess.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // convert local enu, to ws enum
                    WebAPI.ConditionalAccess.eTransactionType eTransactionType = Mapper.Map<WebAPI.ConditionalAccess.eTransactionType>(transaction_type);

                    response = ConditionalAccess.CancelServiceNow(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, domain_id, asset_id, eTransactionType, bIsForce);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while CancelServiceNow. groupId: {0}, domain_id: {1}, asset_id: {2}, transaction_type: {3}, bIsForce: {4}, exception: {5}", groupId, domain_id, asset_id, transaction_type.ToString(), bIsForce, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Code, response.Message);
            }

            return true;
        }

        public void CancelSubscriptionRenewal(int groupId, int domain_id, string subscription_code)
        {
            WebAPI.ConditionalAccess.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = ConditionalAccess.CancelSubscriptionRenewal(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, domain_id, subscription_code);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while CancelServiceNow. groupId: {0}, domain_id: {1}, subscription_code: {2}, exception: {3}", groupId, domain_id, subscription_code, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Code, response.Message);
            }
        }

        public List<Models.ConditionalAccess.KalturaEntitlement> GetUserSubscriptions(int groupId, string user_id)
        {
            List<WebAPI.Models.ConditionalAccess.KalturaEntitlement> entitlements = null;
            WebAPI.ConditionalAccess.Entitlements response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = ConditionalAccess.GetUserSubscriptions(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, user_id);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetUserSubscriptions.  user_id: {0}, exception: {1}", user_id, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.entitelments == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.status.Code, response.status.Message);
            }

            entitlements = Mapper.Map<List<WebAPI.Models.ConditionalAccess.KalturaEntitlement>>(response.entitelments);

            return entitlements;
        }

        public Models.ConditionalAccess.KalturaBillingTransactionListResponse GetUserTransactionHistory(int groupId, string userid, int page_number, int page_size, KalturaTransactionHistoryOrderBy orderBy)
        {
            Models.ConditionalAccess.KalturaBillingTransactionListResponse transactions = null;
            WebAPI.ConditionalAccess.BillingTransactions response = null;
            Group group = GroupsManager.GetGroup(groupId);
            TransactionHistoryOrderBy wsOrderBy = ConditionalAccessMappings.ConvertTransactionHistoryOrderBy(orderBy);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = ConditionalAccess.GetUserBillingHistory(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, userid, page_number, page_size, wsOrderBy);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while GetUserSubscriptions.  user_id: {0}, exception: {1}", userid, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.resp.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.resp.Code, response.resp.Message);
            }

            transactions = Mapper.Map<WebAPI.Models.ConditionalAccess.KalturaBillingTransactionListResponse>(response.transactions);

            return transactions;
        }

        internal KalturaBillingResponse ChargeUserForMediaFile(int groupId, string siteGuid, double price, string currency, int fileId, string ppvModuleCode, string couponCode,
            string extraParams, string udid, string encryptedCvv)
        {
            KalturaBillingResponse result = null;
            WebAPI.ConditionalAccess.BillingStatusResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = ConditionalAccess.CC_ChargeUserForMediaFile(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, siteGuid, price, currency, fileId,
                        ppvModuleCode, couponCode, Utils.Utils.GetClientIP(), extraParams, string.Empty, string.Empty, udid, string.Empty, encryptedCvv);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling conditional access service. ws address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.BillingResponse == null || response.BillingResponse == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            result = Mapper.Map<KalturaBillingResponse>(response.BillingResponse);

            return result;
        }

        internal KalturaBillingResponse ChargeUserForSubscription(int groupId, string siteGuid, double price, string currency, string subscriptionId, string couponCode, string extraParams, string udid, string encryptedCvv)
        {
            KalturaBillingResponse result = null;
            WebAPI.ConditionalAccess.BillingStatusResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = ConditionalAccess.CC_ChargeUserForSubscription(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, siteGuid, price, currency, subscriptionId,
                        couponCode, Utils.Utils.GetClientIP(), extraParams, string.Empty, string.Empty, udid, string.Empty, encryptedCvv);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling conditional access service. ws address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.BillingResponse == null || response.BillingResponse == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            result = Mapper.Map<KalturaBillingResponse>(response.BillingResponse);

            return result;
        }

        internal List<KalturaSubscriptionPrice> GetSubscriptionsPrices(int groupId, IEnumerable<int> subscriptionsIds, string userId, string couponCode, string udid, string languageCode, bool shouldGetOnlyLowest)
        {
            WebAPI.ConditionalAccess.SubscriptionsPricesResponse response = null;
            List<KalturaSubscriptionPrice> prices = new List<KalturaSubscriptionPrice>();

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = ConditionalAccess.GetSubscriptionsPricesWithCoupon(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password,
                        subscriptionsIds.Select(x => x.ToString()).ToArray(), userId, couponCode, string.Empty, languageCode, udid, Utils.Utils.GetClientIP());
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling web service. ws address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.SubscriptionsPrices != null && response.SubscriptionsPrices.Length > 0)
            {
                prices = AutoMapper.Mapper.Map<List<KalturaSubscriptionPrice>>(response.SubscriptionsPrices);
            }

            return prices;
        }

        [Obsolete]
        internal List<KalturaItemPrice> GetItemsPrices(int groupId, List<int> mediaFileIds, string userId, string couponCode, string udid, string languageCode, bool shouldGetOnlyLowest)
        {
            WebAPI.ConditionalAccess.MediaFileItemPricesContainerResponse response = null;
            List<KalturaItemPrice> prices = new List<KalturaItemPrice>();

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = ConditionalAccess.GetItemsPricesWithCoupons(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password,
                        mediaFileIds.ToArray(), userId, couponCode, shouldGetOnlyLowest, string.Empty, languageCode, udid, Utils.Utils.GetClientIP());
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling web service. ws address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            prices = AutoMapper.Mapper.Map<List<WebAPI.Models.Pricing.KalturaItemPrice>>(response.ItemsPrices);

            return prices;
        }

        internal List<KalturaPpvPrice> GetPpvPrices(int groupId, List<int> mediaFileIds, string userId, string couponCode, string udid, string languageCode, bool shouldGetOnlyLowest)
        {
            WebAPI.ConditionalAccess.MediaFileItemPricesContainerResponse response = null;
            List<KalturaPpvPrice> prices = new List<KalturaPpvPrice>();

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = ConditionalAccess.GetItemsPricesWithCoupons(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password,
                        mediaFileIds.ToArray(), userId, couponCode, shouldGetOnlyLowest, string.Empty, languageCode, udid, Utils.Utils.GetClientIP());
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling web service. ws address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            prices = WebAPI.Mapping.ObjectsConvertor.PricingMappings.ConvertPpvPrice(response.ItemsPrices);

            return prices;
        }

        internal KalturaTransaction Purchase(int groupId, string siteguid, long houshold, double price, string currency, int contentId,
                                                     int productId, KalturaTransactionType clientTransactionType, string coupon, string udid, int paymentGatewayId, int paymentMethodId)
        {
            KalturaTransaction clientResponse = null;
            TransactionResponse wsResponse = new TransactionResponse();

            // get group ID
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                // convert local enumerator, to web service enumerator
                WebAPI.ConditionalAccess.eTransactionType transactionType = ConditionalAccessMappings.ConvertTransactionType(clientTransactionType);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    wsResponse = ConditionalAccess.Purchase(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, siteguid, houshold, price,
                                                            currency, contentId, productId, transactionType, coupon, Utils.Utils.GetClientIP(), udid, paymentGatewayId, paymentMethodId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. WS address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (wsResponse == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (wsResponse.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(wsResponse.Status.Code, wsResponse.Status.Message);
            }

            // convert response
            clientResponse = AutoMapper.Mapper.Map<KalturaTransaction>(wsResponse);

            return clientResponse;
        }

        internal KalturaTransaction ProcessReceipt(int groupId, string siteguid, long household, int contentId, int productId, KalturaTransactionType clientTransactionType,
                                                           string udid, string purchaseToken, string paymentGatewayName)
        {
            KalturaTransaction clientResponse = null;
            TransactionResponse wsResponse = new TransactionResponse();

            // get group ID
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                // convert local enumerator, to web service enumerator
                WebAPI.ConditionalAccess.eTransactionType transactionType = ConditionalAccessMappings.ConvertTransactionType(clientTransactionType);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    wsResponse = ConditionalAccess.ProcessReceipt(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, siteguid, household, contentId, productId, transactionType, Utils.Utils.GetClientIP(), udid, purchaseToken, paymentGatewayName);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. WS address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (wsResponse == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (wsResponse.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(wsResponse.Status.Code, wsResponse.Status.Message);
            }

            // convert response
            clientResponse = AutoMapper.Mapper.Map<KalturaTransaction>(wsResponse);

            return clientResponse;
        }


        internal void UpdatePendingTransaction(int groupId, string paymentGatewayId, int adapterTransactionState, string externalTransactionId, string externalStatus,
            string externalMessage, int failReason, string signature)
        {
            Status wsResponse = null;

            // get group 
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    wsResponse = ConditionalAccess.UpdatePendingTransaction(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, paymentGatewayId,
                        adapterTransactionState, externalTransactionId, externalStatus, externalMessage, failReason, signature);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. WS address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (wsResponse == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (wsResponse.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(wsResponse.Code, wsResponse.Message);
            }
        }

        //internal List<KalturaEntitlement> GetDomainPermittedItems(int groupId, int domainId)
        //{
        //    List<KalturaEntitlement> entitlements = null;
        //    PermittedMediaContainerResponse wsResponse = new PermittedMediaContainerResponse();

        //    // get group ID
        //    Group group = GroupsManager.GetGroup(groupId);

        //    try
        //    {
        //        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
        //        {
        //            // fire request
        //            wsResponse = ConditionalAccess.GetDomainPermittedItems(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, domainId);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.ErrorFormat("Exception received while calling service. WS address: {0}, exception: {1}", ConditionalAccess.Url, ex);
        //        ErrorUtils.HandleWSException(ex);
        //    }

        //    if (wsResponse == null)
        //    {
        //        // general exception
        //        throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
        //    }

        //    if (wsResponse.Status.Code != (int)StatusCode.OK)
        //    {
        //        // internal web service exception
        //        throw new ClientException(wsResponse.Status.Code, wsResponse.Status.Message);
        //    }

        //    // convert response
        //    entitlements = Mapper.Map<List<WebAPI.Models.ConditionalAccess.KalturaEntitlement>>(wsResponse.PermittedMediaContainer);

        //    return entitlements;
        //}
        internal List<KalturaEntitlement> GetDomainEntitlements(int groupId, int domainId, KalturaTransactionType type, bool isExpired = false, int pageSize = 500, int pageIndex = 0,
            KalturaEntitlementOrderBy orderBy = KalturaEntitlementOrderBy.PURCHASE_DATE_ASC)
        {
            List<KalturaEntitlement> entitlements = null;
            Entitlements wsResponse = null;

            // convert WS eTransactionType to KalturaTransactionType
            eTransactionType wsType = ConditionalAccessMappings.ConvertTransactionType(type);

            // get group ID
            Group group = GroupsManager.GetGroup(groupId);

            // convert order by
            EntitlementOrderBy wsOrderBy = ConditionalAccessMappings.ConvertEntitlementOrderBy(orderBy);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    wsResponse = ConditionalAccess.GetDomainEntitlements(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, domainId, wsType, isExpired,
                        pageSize, pageIndex, wsOrderBy);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. WS address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (wsResponse == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (wsResponse.status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(wsResponse.status.Code, wsResponse.status.Message);
            }

            // convert response
            entitlements = Mapper.Map<List<WebAPI.Models.ConditionalAccess.KalturaEntitlement>>(wsResponse.entitelments);

            return entitlements;
        }

        internal List<KalturaEntitlement> GetUserEntitlements(int groupId, string userId, KalturaTransactionType type, bool isExpired = false, int pageSize = 50, int pageIndex = 0,
            KalturaEntitlementOrderBy orderBy = KalturaEntitlementOrderBy.PURCHASE_DATE_ASC)
        {
            List<KalturaEntitlement> entitlements = null;
            Entitlements wsResponse = null;

            // convert WS eTransactionType to KalturaTransactionType
            eTransactionType wsType = ConditionalAccessMappings.ConvertTransactionType(type);

            // get group ID
            Group group = GroupsManager.GetGroup(groupId);

            // convert order by
            EntitlementOrderBy wsOrderBy = ConditionalAccessMappings.ConvertEntitlementOrderBy(orderBy);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    wsResponse = ConditionalAccess.GetUserEntitlements(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, userId, wsType, isExpired,
                        pageSize, pageIndex, wsOrderBy);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. WS address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (wsResponse == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (wsResponse.status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(wsResponse.status.Code, wsResponse.status.Message);
            }

            // convert response
            entitlements = Mapper.Map<List<WebAPI.Models.ConditionalAccess.KalturaEntitlement>>(wsResponse.entitelments);

            return entitlements;
        }

        internal bool GrantEntitlements(int groupId, string user_id, long household_id, int content_id, int product_id, KalturaTransactionType product_type, bool history, string deviceName)
        {
            WebAPI.ConditionalAccess.Status response = null;

            // get group ID
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                // convert local enumerator, to web service enumerator
                WebAPI.ConditionalAccess.eTransactionType transactionType = ConditionalAccessMappings.ConvertTransactionType(product_type);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = ConditionalAccess.GrantEntitlements(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, user_id, household_id, content_id,
                        product_id, transactionType, Utils.Utils.GetClientIP(), deviceName, history);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. WS address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Code, response.Message);
            }

            return true;
        }

        internal KalturaBillingTransactionListResponse GetDomainBillingHistory(int groupId, int domainId, DateTime startDate, DateTime endDate, int pageIndex, int pageSize, KalturaTransactionHistoryOrderBy orderBy)
        {
            KalturaBillingTransactionListResponse clientResponse = new KalturaBillingTransactionListResponse();
            DomainTransactionsHistoryResponse wsResponse = null;

            // get group by ID
            Group group = GroupsManager.GetGroup(groupId);
            TransactionHistoryOrderBy wsOrderBy = ConditionalAccessMappings.ConvertTransactionHistoryOrderBy(orderBy);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    wsResponse = ConditionalAccess.GetDomainTransactionsHistory(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, domainId, startDate, endDate, pageSize, pageIndex, wsOrderBy);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. WS address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (wsResponse == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (wsResponse.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(wsResponse.Status.Code, wsResponse.Status.Message);
            }

            // Conversion and paging

            // If received valid response
            if (wsResponse.TransactionsHistory != null)
            {
                // Set total count
                clientResponse.TotalCount = wsResponse.TransactionsCount;
                List<KalturaBillingTransaction> allTransactions = new List<KalturaBillingTransaction>();

                if (wsResponse.TransactionsCount > 0)
                {
                    // Convert current user's list of transactions to List of Kaltura objects
                    List<KalturaUserBillingTransaction> pageTransactions = Mapper.Map<List<KalturaUserBillingTransaction>>(wsResponse.TransactionsHistory);

                    if (pageTransactions != null && pageTransactions.Count > 0)
                    {
                        allTransactions.AddRange(pageTransactions);
                    }

                    // Set paging if page size exists
                    if (pageSize != 0)
                    {
                        pageTransactions = pageTransactions.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                    }
                }

                clientResponse.transactions = allTransactions;
            }

            return clientResponse;
        }

        internal List<KalturaAssetPrice> GetAssetPrices(int groupId,
            string userId, string couponCode, string languageCode, string udid, List<KalturaPersonalAssetRequest> assets)
        {
            List<KalturaAssetPrice> assetPrices = new List<KalturaAssetPrice>();
            WebAPI.ConditionalAccess.AssetItemPriceResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                List<AssetFiles> assetFiles = AutoMapper.Mapper.Map<List<AssetFiles>>(assets);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = ConditionalAccess.GetAssetPrices(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password,
                        userId, couponCode, string.Empty, languageCode, udid, Utils.Utils.GetClientIP(), assetFiles.ToArray());
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling web service. ws address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            assetPrices = AutoMapper.Mapper.Map<List<WebAPI.Models.Pricing.KalturaAssetPrice>>(response.Prices);

            return assetPrices;
        }

        internal bool ReconcileEntitlements(int groupId, string userId)
        {
            WebAPI.ConditionalAccess.Status response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = ConditionalAccess.ReconcileEntitlements(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. WS address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Code, response.Message);
            }

            return true;
        }

        internal KalturaHouseholdPremiumServiceListResponse GetDomainServices(int groupId, int domainId)
        {
            KalturaHouseholdPremiumServiceListResponse result;
            WebAPI.ConditionalAccess.DomainServicesResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = ConditionalAccess.GetDomainServices(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, domainId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. ws address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            result = new KalturaHouseholdPremiumServiceListResponse()
            {
                PremiumServices = Mapper.Map<List<KalturaHouseholdPremiumService>>(response.Services),
                TotalCount = response.Services.Length
            };

            return result;
        }

        [Obsolete]
        internal List<KalturaPremiumService> GetDomainServicesOldStandart(int groupId, int domainId)
        {
            List<KalturaPremiumService> result;
            WebAPI.ConditionalAccess.DomainServicesResponse response = null;

            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = ConditionalAccess.GetDomainServices(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, domainId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. ws address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            result = Mapper.Map<List<KalturaPremiumService>>(response.Services);

            return result;
        }

        internal bool WaiverTransaction(int groupId, int householdID, string userId, int assetId, KalturaTransactionType kalTuraTransactioType)
        {
            WebAPI.ConditionalAccess.Status response = null;

            // get group ID
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                // convert local enumerator, to web service enumerator
                WebAPI.ConditionalAccess.eTransactionType transactionType = ConditionalAccessMappings.ConvertTransactionType(kalTuraTransactioType);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = ConditionalAccess.WaiverTransaction(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, userId, assetId, transactionType);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. WS address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Code, response.Message);
            }

            return true;
        }

        internal long GetCustomDataId(int groupId, string userId, string udid, double price, string currency, int productId, int contentId, string coupon, KalturaTransactionType clientTransactionType, int previewModuleId)
        {
            WebAPI.ConditionalAccess.PurchaseSessionIdResponse response = null;

            // get group ID
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                // convert local enumerator, to web service enumerator
                WebAPI.ConditionalAccess.eTransactionType transactionType = ConditionalAccessMappings.ConvertTransactionType(clientTransactionType);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = ConditionalAccess.GetPurchaseSessionID(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, userId, price, currency, contentId,
                        productId.ToString(), coupon, Utils.Utils.GetClientIP(), udid, transactionType, previewModuleId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. WS address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null || response.PurchaseCustomDataId == 0)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            return response.PurchaseCustomDataId;
        }

        internal KalturaLicensedUrl GetLicensedLinks(int groupId, string userId, string udid, int contentId, string basicLink)
        {
            WebAPI.ConditionalAccess.LicensedLinkResponse response = null;
            KalturaLicensedUrl urls = null;

            // get group ID
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = ConditionalAccess.GetLicensedLinks(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password,
                        userId, contentId, basicLink, Utils.Utils.GetClientIP(), string.Empty, string.Empty, string.Empty, udid);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. WS address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            urls = Mapper.Map<KalturaLicensedUrl>(response);

            return urls;
        }

        internal KalturaLicensedUrl GetEPGLicensedLink(int groupId, string userId, string udid, int epgId, int contentId, string baseUrl, long startDate, KalturaStreamType streamType)
        {
            WebAPI.ConditionalAccess.LicensedLinkResponse response = null;
            KalturaLicensedUrl urls = null;

            // get group ID
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                DateTime startTime = Utils.SerializationUtils.ConvertFromUnixTimestamp(startDate);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = ConditionalAccess.GetEPGLicensedLink(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password,
                        userId, contentId, epgId, startTime, baseUrl, Utils.Utils.GetClientIP(), string.Empty, string.Empty, string.Empty, udid, (int)streamType);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. WS address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            urls = Mapper.Map<KalturaLicensedUrl>(response);

            return urls;
        }

        internal KalturaLicensedUrl GetRecordingLicensedLink(int groupId, string userId, string udid, int recordingId, long startDate, string fileType)
        {
            WebAPI.ConditionalAccess.LicensedLinkResponse response = null;
            KalturaLicensedUrl urls = null;

            // get group ID
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                DateTime startTime = Utils.SerializationUtils.ConvertFromUnixTimestamp(startDate);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = ConditionalAccess.GetRecordingLicensedLink(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password,
                        userId, recordingId, startTime, udid, Utils.Utils.GetClientIP(), fileType);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. WS address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null || response.Status == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            urls = Mapper.Map<KalturaLicensedUrl>(response);

            return urls;
        }

        internal List<KalturaCDVRAdapterProfile> GetCDVRAdapters(int groupId)
        {
            List<KalturaCDVRAdapterProfile> adapters = new List<KalturaCDVRAdapterProfile>();

            Group group = GroupsManager.GetGroup(groupId);

            CDVRAdapterResponseList response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = ConditionalAccess.GetCDVRAdapters(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling conditional access service. ws address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.Adapters.Length > 0)
            {
                adapters = AutoMapper.Mapper.Map<List<KalturaCDVRAdapterProfile>>(response.Adapters);
            }

            return adapters;
        }

        internal bool DeleteCDVRAdapter(int groupId, int adapterId)
        {
            Group group = GroupsManager.GetGroup(groupId);

            Status response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = ConditionalAccess.DeleteCDVRAdapter(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, adapterId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling conditional access service. ws address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Code, response.Message);
            }

            return true;
        }

        internal KalturaCDVRAdapterProfile InsertCDVRAdapter(int groupId, KalturaCDVRAdapterProfile cdvrAdapter)
        {
            KalturaCDVRAdapterProfile adapter = new KalturaCDVRAdapterProfile();

            Group group = GroupsManager.GetGroup(groupId);

            CDVRAdapterResponse response = null;

            CDVRAdapter wsAdapter = AutoMapper.Mapper.Map<CDVRAdapter>(cdvrAdapter);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = ConditionalAccess.InsertCDVRAdapter(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, wsAdapter);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling conditional access service. ws address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            adapter = AutoMapper.Mapper.Map<KalturaCDVRAdapterProfile>(response.Adapter);

            return adapter;
        }

        internal KalturaCDVRAdapterProfile SetCDVRAdapter(int groupId, KalturaCDVRAdapterProfile adapter)
        {
            KalturaCDVRAdapterProfile adapterResponse = new KalturaCDVRAdapterProfile();

            Group group = GroupsManager.GetGroup(groupId);

            CDVRAdapterResponse response = null;

            CDVRAdapter wsAdapter = AutoMapper.Mapper.Map<CDVRAdapter>(adapter);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = ConditionalAccess.SetCDVRAdapter(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, wsAdapter);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling conditional access service. ws address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            adapterResponse = AutoMapper.Mapper.Map<KalturaCDVRAdapterProfile>(response.Adapter);

            return adapterResponse;
        }

        internal KalturaCDVRAdapterProfile GenerateCDVRSharedSecret(int groupId, int adapterId)
        {
            KalturaCDVRAdapterProfile adapter = new KalturaCDVRAdapterProfile();

            Group group = GroupsManager.GetGroup(groupId);

            CDVRAdapterResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = ConditionalAccess.GenerateCDVRSharedSecret(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, adapterId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling conditional access service. ws address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            adapter = AutoMapper.Mapper.Map<KalturaCDVRAdapterProfile>(response.Adapter);

            return adapter;
        }

        internal KalturaRecording GetRecord(int groupID, long domainID, long recordingID)
        {
            KalturaRecording recording = null;
            Recording response = null;

            // get group ID
            Group group = GroupsManager.GetGroup(groupID);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = ConditionalAccess.GetRecordingByID(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, domainID, recordingID);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. WS address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            // convert response
            recording = Mapper.Map<WebAPI.Models.ConditionalAccess.KalturaRecording>(response);

            return recording;
        }

        internal KalturaRecordingContextListResponse QueryRecords(int groupID, string userID, long[] assetIds)
        {
            KalturaRecordingContextListResponse result = null;
            RecordingResponse response = null;

            // get group ID
            Group group = GroupsManager.GetGroup(groupID);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = ConditionalAccess.QueryRecords(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, userID, assetIds);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. WS address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.Recordings != null && response.Recordings.Length > 0)
            {
                result = new KalturaRecordingContextListResponse() { Objects = new List<KalturaRecordingContext>(), TotalCount = 0 };
                result.TotalCount = response.TotalItems;
                // convert recordings
                foreach (Recording recording in response.Recordings)
                {
                    KalturaRecordingContext recordingContext = new KalturaRecordingContext() { Code = recording.Status.Code, Message = recording.Status.Message, AssetId = recording.EpgId, Recording = null };
                    if (recording.Status.Code == (int)StatusCode.OK && recording.RecordingStatus != TstvRecordingStatus.OK)
                    {
                        recordingContext.Recording = Mapper.Map<WebAPI.Models.ConditionalAccess.KalturaRecording>(recording);
                    }
                    result.Objects.Add(recordingContext);
                }
            }

            return result;
        }

        internal KalturaRecording Record(int groupID, string userID, long epgID)
        {
            KalturaRecording recording = null;
            Recording response = null;

            // get group ID
            Group group = GroupsManager.GetGroup(groupID);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = ConditionalAccess.Record(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, userID, epgID, RecordingType.Single);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. WS address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            // convert response
            recording = Mapper.Map<WebAPI.Models.ConditionalAccess.KalturaRecording>(response);

            return recording;
        }

        internal KalturaRecordingListResponse SearchRecordings(int groupID, string userID, long domainID, List<KalturaRecordingStatus> recordingStatuses, string ksqlFilter,
                                                                int pageIndex, int? pageSize, KalturaRecordingOrderBy? orderBy)
        {
            KalturaRecordingListResponse result = new KalturaRecordingListResponse() { TotalCount = 0 };
            RecordingResponse response = null;

            // Create catalog order object
            OrderObj order = new OrderObj();
            if (orderBy == null)
            {
                order.m_eOrderBy = OrderBy.NAME;
                order.m_eOrderDir = OrderDir.ASC;
            }
            else
            {
                order = ConditionalAccessMappings.ConvertOrderToOrderObj(orderBy.Value);
            }

            if (recordingStatuses == null || recordingStatuses.Count == 0)
            {
                recordingStatuses = new List<KalturaRecordingStatus>() { KalturaRecordingStatus.SCHEDULED, KalturaRecordingStatus.RECORDING, KalturaRecordingStatus.RECORDED, KalturaRecordingStatus.CANCELED, KalturaRecordingStatus.FAILED };
            }

            List<WebAPI.ConditionalAccess.TstvRecordingStatus> convertedRecordingStatuses = recordingStatuses.Select(x => ConditionalAccessMappings.ConvertKalturaRecordingStatus(x)).ToList();

            // get group configuration
            Group group = GroupsManager.GetGroup(groupID);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = ConditionalAccess.SearchDomainRecordings(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, userID, domainID, convertedRecordingStatuses.ToArray(),
                                                                          ksqlFilter, pageIndex, pageSize.Value, order);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. WS address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            if (response.Recordings != null && response.Recordings.Length > 0)
            {
                result.TotalCount = response.TotalItems;
                // convert recordings            
                result.Objects = Mapper.Map<List<WebAPI.Models.ConditionalAccess.KalturaRecording>>(response.Recordings);
            }

            return result;
        }

        internal bool RemovePaymentMethodHouseholdPaymentGateway(int payment_gateway_id, int groupId, string userID, long householdId, int paymentMethodId, bool force = false)
        {
            WebAPI.ConditionalAccess.Status response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = ConditionalAccess.RemovePaymentMethodHouseholdPaymentGateway(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, payment_gateway_id, userID,
                        (int)householdId, paymentMethodId, force);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Error while RemovePaymentMethodHouseholdPaymentGateway.  groupID: {0}, paymentMethodId {1}, householdId {2},  exception: {3}", groupId,
                    paymentMethodId, householdId, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Code, response.Message);
            }

            return true;
        }

        internal KalturaHouseholdQuota GetDomainQuota(int groupId, string userId, long domainId)
        {
            DomainQuotaResponse webServiceResponse = null;

            // get group ID
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    webServiceResponse = ConditionalAccess.GetDomainQuota(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, userId, domainId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. WS address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (webServiceResponse == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (webServiceResponse.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(webServiceResponse.Status.Code, webServiceResponse.Status.Message);
            }

            KalturaHouseholdQuota response = null;

            // convert response
            response = Mapper.Map<WebAPI.Models.ConditionalAccess.KalturaHouseholdQuota>(webServiceResponse);
            response.HouseholdId = domainId;

            return response;
        }

        internal KalturaRecording CancelRecord(int groupID, string userID, long domainID, long id)
        {
            KalturaRecording recording = null;
            Recording response = null;

            // get group ID
            Group group = GroupsManager.GetGroup(groupID);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = ConditionalAccess.CancelRecord(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, userID, domainID, id);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. WS address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            // convert response
            recording = Mapper.Map<WebAPI.Models.ConditionalAccess.KalturaRecording>(response);

            return recording;
        }

        internal KalturaRecording DeleteRecord(int groupID, string userID, long domainID, long id)
        {
            KalturaRecording recording = null;
            Recording response = null;

            // get group ID
            Group group = GroupsManager.GetGroup(groupID);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = ConditionalAccess.DeleteRecord(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, userID, domainID, id);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. WS address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            // convert response
            recording = Mapper.Map<WebAPI.Models.ConditionalAccess.KalturaRecording>(response);

            return recording;
        }

        internal KalturaRecording ProtectRecord(int groupID, string userID, long recordingID)
        {
            KalturaRecording recording = null;
            Recording response = null;

            // get group ID
            Group group = GroupsManager.GetGroup(groupID);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = ConditionalAccess.ProtectRecord(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, userID, recordingID);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. WS address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            // convert response
            recording = Mapper.Map<WebAPI.Models.ConditionalAccess.KalturaRecording>(response);

            return recording;
        }

        internal KalturaSeriesRecording CancelSeriesRecord(int groupId, string userId, long domainId, long id, long epgId)
        {
            KalturaSeriesRecording seriesRecording = null;
            SeriesRecording response = null;

            // get group ID
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = ConditionalAccess.CancelSeriesRecord(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, userId, domainId, id, epgId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. WS address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            // convert response
            seriesRecording = Mapper.Map<WebAPI.Models.ConditionalAccess.KalturaSeriesRecording>(response);

            return seriesRecording;
        }

        internal KalturaSeriesRecording DeleteSeriesRecord(int groupId, string userId, long domainId, long id, long epgId = 0)
        {
            KalturaSeriesRecording seriesRecording = null;
            SeriesRecording response = null;

            // get group ID
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = ConditionalAccess.DeleteSeriesRecord(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, userId, domainId, id, epgId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. WS address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(response.Status.Code, response.Status.Message);
            }

            // convert response
            seriesRecording = Mapper.Map<WebAPI.Models.ConditionalAccess.KalturaSeriesRecording>(response);

            return seriesRecording;
        }

        internal KalturaSeriesRecordingListResponse GetFollowSeries(int groupID, string userID, long domainID, KalturaSeriesRecordingOrderBy? orderBy)
        {
            KalturaSeriesRecordingListResponse result = new KalturaSeriesRecordingListResponse() { TotalCount = 0 };
            SeriesResponse response = null;

            // get group configuration
            Group group = GroupsManager.GetGroup(groupID);
            // Create catalog order object
            SeriesRecordingOrderObj order = new SeriesRecordingOrderObj();
            if (orderBy == null)
            {
                order.OrderBy = SeriesOrderBy.ID;
                order.OrderDir = OrderDir.ASC;
            }
            else
            {
                order = ConditionalAccessMappings.ConvertOrderToSeriesOrderObj(orderBy.Value);
            }

            using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
            {
                // fire request
                response = ConditionalAccess.GetFollowSeries(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, userID, domainID, order);
            }
            if (response.SeriesRecordings != null && response.SeriesRecordings.Length > 0)
            {
                result.TotalCount = response.TotalItems;
                // convert recordings            
                result.Objects = Mapper.Map<List<WebAPI.Models.ConditionalAccess.KalturaSeriesRecording>>(response.SeriesRecordings);
            }

            return result;
        }

        internal KalturaSeriesRecording RecordSeasonOrSeries(int groupID, string userID, long epgID, KalturaRecordingType recordingType)
        {
            KalturaSeriesRecording recording = null;
            SeriesRecording response = null;

            // get group ID
            Group group = GroupsManager.GetGroup(groupID);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = ConditionalAccess.RecordSeasonOrSeries(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, userID, epgID, ConditionalAccessMappings.ConvertKalturaRecordingType(recordingType));
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. WS address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(response.Status.Code, response.Status.Message);
            }
            // convert response
            recording = Mapper.Map<WebAPI.Models.ConditionalAccess.KalturaSeriesRecording>(response);

            return recording;
        }

        internal KalturaAssetFileContext GetAssetFileContext(int groupId, string userID, string fileId, string udid, string language)
        {
            KalturaAssetFileContext kalturaResponse = null;
            EntitlementResponse response = null;

            // get group ID
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = ConditionalAccess.GetEntitlement(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, fileId.ToString(),
                        userID, true, string.Empty, language, udid);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. WS address: {0}, exception: {1}", ConditionalAccess.Url, ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }
            // convert response        
            kalturaResponse = Mapper.Map<WebAPI.Models.ConditionalAccess.KalturaAssetFileContext>(response);
            //kalturaResponse = new KalturaAssetFileContext()
            //    {
            //        Duration = response
            //    };

            return kalturaResponse;
        }
    }
}

namespace WebAPI.ConditionalAccess
{
    // adding request ID to header
    public partial class module
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(uri);
            KlogMonitorHelper.MonitorLogsHelper.AddHeaderToWebService(request);
            return request;
        }
    }
}