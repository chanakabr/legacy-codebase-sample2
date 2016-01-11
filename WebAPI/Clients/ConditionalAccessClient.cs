using AutoMapper;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using WebAPI.Models.Pricing;
using WebAPI.Utils;
using WebAPI.ConditionalAccess;
using WebAPI.ObjectsConvertor.Mapping;
using WebAPI.Models.Catalog;

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

        public Models.ConditionalAccess.KalturaBillingTransactionListResponse GetUserTransactionHistory(int groupId, string userid, int page_number, int page_size)
        {
            Models.ConditionalAccess.KalturaBillingTransactionListResponse transactions = null;
            WebAPI.ConditionalAccess.BillingTransactions response = null;
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = ConditionalAccess.GetUserBillingHistory(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, userid, page_number, page_size);
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

            prices = AutoMapper.Mapper.Map<List<KalturaSubscriptionPrice>>(response.SubscriptionsPrices);

            return prices;
        }

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

        internal KalturaTransaction Purchase(int groupId, string siteguid, long houshold, double price, string currency, int contentId,
                                                     int productId, KalturaTransactionType clientTransactionType, string coupon, string udid, int paymentGwId)
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
                                                            currency, contentId, productId, transactionType, coupon, Utils.Utils.GetClientIP(), udid, paymentGwId);
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

        internal List<KalturaEntitlement> GetDomainEntitlements(int groupId, int domainId, KalturaTransactionType type)
        {
            List<KalturaEntitlement> entitlements = null;
            Entitlements wsResponse = null;

            // convert WS eTransactionType to KalturaTransactionType
            eTransactionType wsType = ConditionalAccessMappings.ConvertTransactionType(type);

            // get group ID
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    wsResponse = ConditionalAccess.GetDomainEntitlements(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, domainId, wsType);
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

        internal List<KalturaEntitlement> GetUserEntitlements(int groupId, string userId, KalturaTransactionType type)
        {
            List<KalturaEntitlement> entitlements = null;
            Entitlements wsResponse = null;

            // convert WS eTransactionType to KalturaTransactionType
            eTransactionType wsType = ConditionalAccessMappings.ConvertTransactionType(type);

            // get group ID
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    wsResponse = ConditionalAccess.GetUserEntitlements(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, userId, wsType);
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

        internal KalturaBillingTransactionListResponse GetDomainBillingHistory(int groupId, int domainId, DateTime startDate, DateTime endDate, int pageIndex, int pageSize)
        {
            KalturaBillingTransactionListResponse clientResponse = new KalturaBillingTransactionListResponse();
            DomainTransactionsHistoryResponse wsResponse = null;

            // get group by ID
            Group group = GroupsManager.GetGroup(groupId);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    wsResponse = ConditionalAccess.GetDomainTransactionsHistory(group.ConditionalAccessCredentials.Username, group.ConditionalAccessCredentials.Password, domainId, startDate, endDate);
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
            if (wsResponse.TransactionsHistory != null && wsResponse.TransactionsCount > 0)
            {
                // Set total count
                clientResponse.TotalCount = wsResponse.TransactionsCount;
                
                // Convert current user's list of transactions to List of Kaltura objects
                List<KalturaBillingTransaction> pageTransactions = Mapper.Map<List<KalturaBillingTransaction>>(wsResponse.TransactionsHistory);

                // Set paging if page size exists
                if (pageSize != 0)
                {
                    pageTransactions = pageTransactions.Skip(pageIndex * pageSize).Take(pageSize).ToList();
                }

                clientResponse.transactions = pageTransactions;
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
    }
}