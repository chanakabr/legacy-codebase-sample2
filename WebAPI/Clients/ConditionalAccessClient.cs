using AutoMapper;
using KLogMonitor;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using WebAPI.ClientManagers;
using WebAPI.ClientManagers.Client;
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
using ApiObjects.SearchObjects;
using ApiObjects.TimeShiftedTv;
using ApiObjects;
using ApiObjects.Response;
using ApiObjects.Billing;
using Core.ConditionalAccess.Response;
using ApiObjects.ConditionalAccess;
using Core.ConditionalAccess;

namespace WebAPI.Clients
{
    public class ConditionalAccessClient : BaseClient
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public ConditionalAccessClient()
        {

        }

        public bool CancelServiceNow(int groupId, int domain_id, int asset_id, Models.ConditionalAccess.KalturaTransactionType transaction_type, bool bIsForce)
        {
            Status response = null;
            
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // convert local enu, to ws enum
                    eTransactionType eTransactionType = Mapper.Map<eTransactionType>(transaction_type);

                    response = Core.ConditionalAccess.Module.CancelServiceNow(groupId, domain_id, asset_id, eTransactionType, bIsForce);
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
            Status response = null;
            
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.ConditionalAccess.Module.CancelSubscriptionRenewal(groupId, domain_id, subscription_code);
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
            Entitlements response = null;
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.ConditionalAccess.Module.GetUserSubscriptions(groupId, user_id);
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
            BillingTransactions response = null;
            
            TransactionHistoryOrderBy wsOrderBy = ConditionalAccessMappings.ConvertTransactionHistoryOrderBy(orderBy);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.ConditionalAccess.Module.GetUserBillingHistory(groupId, userid, page_number, page_size, wsOrderBy);
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
            BillingStatusResponse response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.ConditionalAccess.Module.CC_ChargeUserForMediaFile(groupId, siteGuid, price, currency, fileId,
                        ppvModuleCode, couponCode, Utils.Utils.GetClientIP(), extraParams, string.Empty, string.Empty, udid, string.Empty, encryptedCvv);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling conditional access service. exception: {1}", ex);
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
            BillingStatusResponse response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.ConditionalAccess.Module.CC_ChargeUserForSubscription(groupId, siteGuid, price, currency, subscriptionId,
                        couponCode, Utils.Utils.GetClientIP(), extraParams, string.Empty, string.Empty, udid, string.Empty, encryptedCvv);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling conditional access service. exception: {1}", ex);
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

        internal List<KalturaSubscriptionPrice> GetSubscriptionsPrices(int groupId, IEnumerable<int> subscriptionsIds, string userId, string couponCode, string udid, string languageCode, bool shouldGetOnlyLowest, string currencyCode)
        {
            SubscriptionsPricesResponse response = null;
            List<KalturaSubscriptionPrice> prices = new List<KalturaSubscriptionPrice>();

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.ConditionalAccess.Module.GetSubscriptionsPricesWithCurrency(groupId,
                        subscriptionsIds.Select(x => x.ToString()).ToArray(), userId, couponCode, string.Empty, languageCode, udid, Utils.Utils.GetClientIP(), currencyCode);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling web service. exception: {1}", ex);
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
            MediaFileItemPricesContainerResponse response = null;
            List<KalturaItemPrice> prices = new List<KalturaItemPrice>();

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.ConditionalAccess.Module.GetItemsPricesWithCoupons(groupId,
                        mediaFileIds.ToArray(), userId, couponCode, shouldGetOnlyLowest, string.Empty, languageCode, udid, Utils.Utils.GetClientIP());
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling web service. exception: {1}", ex);
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

        internal List<KalturaPpvPrice> GetPpvPrices(int groupId, List<int> mediaFileIds, string userId, string couponCode, string udid, string languageCode, bool shouldGetOnlyLowest, string currencyCode)
        {
            MediaFileItemPricesContainerResponse response = null;
            List<KalturaPpvPrice> prices = new List<KalturaPpvPrice>();

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.ConditionalAccess.Module.GetItemsPricesWithCurrency(groupId,
                        mediaFileIds.ToArray(), userId, couponCode, shouldGetOnlyLowest, languageCode, udid, Utils.Utils.GetClientIP(), currencyCode);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling web service. exception: {1}", ex);
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

            prices = PricingMappings.ConvertPpvPrice(response.ItemsPrices);

            return prices;
        }

        internal KalturaTransaction Purchase(int groupId, string siteguid, long houshold, double price, string currency, int contentId,
                                             int productId, KalturaTransactionType clientTransactionType, string coupon, string udid, int paymentGatewayId, int paymentMethodId, string adapterData)
        {
            KalturaTransaction clientResponse = null;
            TransactionResponse wsResponse = new TransactionResponse();

            // get group ID
            

            try
            {
                // convert local enumerator, to web service enumerator
                eTransactionType transactionType = ConditionalAccessMappings.ConvertTransactionType(clientTransactionType);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    wsResponse = Core.ConditionalAccess.Module.Purchase(groupId, siteguid, houshold, price,
                                                            currency, contentId, productId, transactionType, coupon, Utils.Utils.GetClientIP(), udid, paymentGatewayId, paymentMethodId, adapterData);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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
            

            try
            {
                // convert local enumerator, to web service enumerator
                eTransactionType transactionType = ConditionalAccessMappings.ConvertTransactionType(clientTransactionType);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    wsResponse = Core.ConditionalAccess.Module.ProcessReceipt(groupId, siteguid, household, contentId, productId, transactionType, Utils.Utils.GetClientIP(), udid, purchaseToken, paymentGatewayName);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    wsResponse = Core.ConditionalAccess.Module.UpdatePendingTransaction(groupId, paymentGatewayId,
                        adapterTransactionState, externalTransactionId, externalStatus, externalMessage, failReason, signature);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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
        //    

        //    try
        //    {
        //        using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
        //        {
        //            // fire request
        //            wsResponse = Core.ConditionalAccess.Module.GetDomainPermittedItems(groupId, domainId);
        //        }
        //    }
        //    catch (Exception ex)
        //    {
        //        log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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
            List<KalturaEntitlement> entitlements = new List<KalturaEntitlement>();
            Entitlements wsResponse = null;

            // convert WS eTransactionType to KalturaTransactionType
            eTransactionType wsType = ConditionalAccessMappings.ConvertTransactionType(type);

            // get group ID
            

            // convert order by
            EntitlementOrderBy wsOrderBy = ConditionalAccessMappings.ConvertEntitlementOrderBy(orderBy);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    wsResponse = Core.ConditionalAccess.Module.GetDomainEntitlements(groupId, domainId, wsType, isExpired,
                        pageSize, pageIndex, wsOrderBy);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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
            if (wsResponse.entitelments != null && wsResponse.entitelments.Count > 0)
            {
                foreach (Entitlement entitelment in wsResponse.entitelments)
                {
                    entitlements.Add(ConditionalAccessMappings.ConvertToKalturaEntitlement(entitelment));
                }
            }
            return entitlements;
        }

        internal List<KalturaEntitlement> GetUserEntitlements(int groupId, string userId, KalturaTransactionType type, bool isExpired = false, int pageSize = 50, int pageIndex = 0,
            KalturaEntitlementOrderBy orderBy = KalturaEntitlementOrderBy.PURCHASE_DATE_ASC)
        {
            List<KalturaEntitlement> entitlements = new List<KalturaEntitlement>();
            Entitlements wsResponse = null;

            // convert WS eTransactionType to KalturaTransactionType
            eTransactionType wsType = ConditionalAccessMappings.ConvertTransactionType(type);

            // get group ID
            

            // convert order by
            EntitlementOrderBy wsOrderBy = ConditionalAccessMappings.ConvertEntitlementOrderBy(orderBy);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    wsResponse = Core.ConditionalAccess.Module.GetUserEntitlements(groupId, userId, wsType, isExpired,
                        pageSize, pageIndex, wsOrderBy);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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
            if (wsResponse.entitelments != null && wsResponse.entitelments.Count > 0)
            {
                foreach (Entitlement entitelment in wsResponse.entitelments)
                {
                    entitlements.Add(ConditionalAccessMappings.ConvertToKalturaEntitlement(entitelment));
                }
            }

            return entitlements;
        }

        internal bool GrantEntitlements(int groupId, string user_id, long household_id, int content_id, int product_id, KalturaTransactionType product_type, bool history, string deviceName)
        {
            Status response = null;

            // get group ID
            

            try
            {
                // convert local enumerator, to web service enumerator
                eTransactionType transactionType = ConditionalAccessMappings.ConvertTransactionType(product_type);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = Core.ConditionalAccess.Module.GrantEntitlements(groupId, user_id, household_id, content_id,
                        product_id, transactionType, Utils.Utils.GetClientIP(), deviceName, history);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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
            
            TransactionHistoryOrderBy wsOrderBy = ConditionalAccessMappings.ConvertTransactionHistoryOrderBy(orderBy);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    wsResponse = Core.ConditionalAccess.Module.GetDomainTransactionsHistory(groupId, domainId, startDate, endDate, pageSize, pageIndex, wsOrderBy);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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
            AssetItemPriceResponse response = null;

            

            try
            {
                List<AssetFiles> assetFiles = AutoMapper.Mapper.Map<List<AssetFiles>>(assets);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.ConditionalAccess.Module.GetAssetPrices(groupId,
                        userId, couponCode, string.Empty, languageCode, udid, Utils.Utils.GetClientIP(), assetFiles);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling web service. exception: {1}", ex);
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
            Status response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.ConditionalAccess.Module.ReconcileEntitlements(groupId, userId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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
            DomainServicesResponse response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.ConditionalAccess.Module.GetDomainServices(groupId, domainId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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
                TotalCount = response.Services.Count
            };

            return result;
        }

        [Obsolete]
        internal List<KalturaPremiumService> GetDomainServicesOldStandart(int groupId, int domainId)
        {
            List<KalturaPremiumService> result;
            DomainServicesResponse response = null;

            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.ConditionalAccess.Module.GetDomainServices(groupId, domainId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling domains service. exception: {1}", ex);
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
            Status response = null;

            // get group ID
            

            try
            {
                // convert local enumerator, to web service enumerator
                eTransactionType transactionType = ConditionalAccessMappings.ConvertTransactionType(kalTuraTransactioType);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = Core.ConditionalAccess.Module.WaiverTransaction(groupId, userId, assetId, transactionType);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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
            PurchaseSessionIdResponse response = null;

            // get group ID
            

            try
            {
                // convert local enumerator, to web service enumerator
                eTransactionType transactionType = ConditionalAccessMappings.ConvertTransactionType(clientTransactionType);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = Core.ConditionalAccess.Module.GetPurchaseSessionID(groupId, userId, price, currency, contentId,
                        productId.ToString(), coupon, Utils.Utils.GetClientIP(), udid, transactionType, previewModuleId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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
            LicensedLinkResponse response = null;
            KalturaLicensedUrl urls = null;

            // get group ID
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = Core.ConditionalAccess.Module.GetLicensedLinks(groupId,
                        userId, contentId, basicLink, Utils.Utils.GetClientIP(), string.Empty, string.Empty, string.Empty, udid);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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
            LicensedLinkResponse response = null;
            KalturaLicensedUrl urls = null;

            // get group ID
            

            try
            {
                DateTime startTime = Utils.SerializationUtils.ConvertFromUnixTimestamp(startDate);

                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = Core.ConditionalAccess.Module.GetEPGLicensedLink(groupId,
                        userId, contentId, epgId, startTime, baseUrl, Utils.Utils.GetClientIP(), string.Empty, string.Empty, string.Empty, udid, (int)streamType);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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

        internal KalturaLicensedUrl GetRecordingLicensedLink(int groupId, string userId, string udid, int recordingId, string fileType)
        {
            LicensedLinkResponse response = null;
            KalturaLicensedUrl urls = null;

            // get group ID
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = Core.ConditionalAccess.Module.GetRecordingLicensedLink(groupId,
                        userId, recordingId, udid, Utils.Utils.GetClientIP(), fileType);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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

            

            CDVRAdapterResponseList response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.ConditionalAccess.Module.GetCDVRAdapters(groupId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling conditional access service. exception: {1}", ex);
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

            if (response.Adapters.Count > 0)
            {
                adapters = AutoMapper.Mapper.Map<List<KalturaCDVRAdapterProfile>>(response.Adapters);
            }

            return adapters;
        }

        internal bool DeleteCDVRAdapter(int groupId, int adapterId)
        {
            

            Status response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.ConditionalAccess.Module.DeleteCDVRAdapter(groupId, adapterId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling conditional access service. exception: {1}", ex);
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

            

            CDVRAdapterResponse response = null;

            CDVRAdapter wsAdapter = AutoMapper.Mapper.Map<CDVRAdapter>(cdvrAdapter);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.ConditionalAccess.Module.InsertCDVRAdapter(groupId, wsAdapter);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling conditional access service. exception: {1}", ex);
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

            

            CDVRAdapterResponse response = null;

            CDVRAdapter wsAdapter = AutoMapper.Mapper.Map<CDVRAdapter>(adapter);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.ConditionalAccess.Module.SetCDVRAdapter(groupId, wsAdapter);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling conditional access service. exception: {1}", ex);
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

            

            CDVRAdapterResponse response = null;
            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.ConditionalAccess.Module.GenerateCDVRSharedSecret(groupId, adapterId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling conditional access service. exception: {1}", ex);
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

        internal KalturaRecording GetRecord(int groupId, long domainID, long recordingID)
        {
            KalturaRecording recording = null;
            Recording response = null;

            // get group ID
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = Core.ConditionalAccess.Module.GetRecordingByID(groupId, domainID, recordingID);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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

        internal KalturaRecordingContextListResponse QueryRecords(int groupId, string userID, long[] assetIds)
        {
            KalturaRecordingContextListResponse result = null;
            RecordingResponse response = null;

            // get group ID
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = Core.ConditionalAccess.Module.QueryRecords(groupId, userID, assetIds);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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

            if (response.Recordings != null && response.Recordings.Count > 0)
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

        internal KalturaRecording Record(int groupId, string userID, long epgID)
        {
            KalturaRecording recording = null;
            Recording response = null;

            // get group ID
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = Core.ConditionalAccess.Module.Record(groupId, userID, epgID, RecordingType.Single);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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

        internal KalturaRecordingListResponse SearchRecordings(int groupId, string userID, long domainID, List<KalturaRecordingStatus> recordingStatuses, string ksqlFilter,
                                                                int pageIndex, int? pageSize, KalturaRecordingOrderBy? orderBy)
        {
            KalturaRecordingListResponse result = new KalturaRecordingListResponse() { TotalCount = 0 };
            RecordingResponse response = null;

            // Create catalog order object
            OrderObj order = new OrderObj();
            if (orderBy == null)
            {
                order.m_eOrderBy = OrderBy.START_DATE;
                order.m_eOrderDir = OrderDir.DESC;
            }
            else
            {
                order = ConditionalAccessMappings.ConvertOrderToOrderObj(orderBy.Value);
            }

            if (recordingStatuses == null || recordingStatuses.Count == 0)
            {
                recordingStatuses = new List<KalturaRecordingStatus>() { KalturaRecordingStatus.SCHEDULED, KalturaRecordingStatus.RECORDING, KalturaRecordingStatus.RECORDED, KalturaRecordingStatus.CANCELED, KalturaRecordingStatus.FAILED };
            }

            List<TstvRecordingStatus> convertedRecordingStatuses = recordingStatuses.Select(x => ConditionalAccessMappings.ConvertKalturaRecordingStatus(x)).ToList();

            // get group configuration
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = Core.ConditionalAccess.Module.SearchDomainRecordings(groupId, userID, domainID, convertedRecordingStatuses.ToArray(),
                                                                          ksqlFilter, pageIndex, pageSize.Value, order, false);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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

            if (response.Recordings != null && response.Recordings.Count > 0)
            {
                result.TotalCount = response.TotalItems;
                // convert recordings            
                result.Objects = Mapper.Map<List<WebAPI.Models.ConditionalAccess.KalturaRecording>>(response.Recordings);
            }

            return result;
        }

        internal bool RemovePaymentMethodHouseholdPaymentGateway(int payment_gateway_id, int groupId, string userID, long householdId, int paymentMethodId, bool force = false)
        {
            Status response = null;
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.ConditionalAccess.Module.RemovePaymentMethodHouseholdPaymentGateway(groupId, payment_gateway_id, userID,
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
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    webServiceResponse = Core.ConditionalAccess.Module.GetDomainQuota(groupId, userId, domainId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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

        internal KalturaRecording CancelRecord(int groupId, string userID, long domainID, long id)
        {
            KalturaRecording recording = null;
            Recording response = null;

            // get group ID
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = Core.ConditionalAccess.Module.CancelRecord(groupId, userID, domainID, id);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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

        internal KalturaRecording DeleteRecord(int groupId, string userID, long domainID, long id)
        {
            KalturaRecording recording = null;
            Recording response = null;

            // get group ID
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = Core.ConditionalAccess.Module.DeleteRecord(groupId, userID, domainID, id);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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

        internal KalturaRecording ProtectRecord(int groupId, string userID, long recordingID)
        {
            KalturaRecording recording = null;
            Recording response = null;

            // get group ID
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = Core.ConditionalAccess.Module.ProtectRecord(groupId, userID, recordingID);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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

        internal KalturaSeriesRecording CancelSeriesRecord(int groupId, string userId, long domainId, long id, long epgId = 0, long seasonNumber = 0)
        {
            KalturaSeriesRecording seriesRecording = null;
            SeriesRecording response = null;

            // get group ID
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = Core.ConditionalAccess.Module.CancelSeriesRecord(groupId, userId, domainId, id, epgId, seasonNumber);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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

        internal KalturaSeriesRecording DeleteSeriesRecord(int groupId, string userId, long domainId, long id, long epgId = 0, int seasonNumber = 0)
        {
            KalturaSeriesRecording seriesRecording = null;
            SeriesRecording response = null;

            // get group ID
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = Core.ConditionalAccess.Module.DeleteSeriesRecord(groupId, userId, domainId, id, epgId, seasonNumber);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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

        internal KalturaSeriesRecordingListResponse GetFollowSeries(int groupId, string userID, long domainID, KalturaSeriesRecordingOrderBy? orderBy)
        {
            KalturaSeriesRecordingListResponse result = new KalturaSeriesRecordingListResponse() { TotalCount = 0 };
            SeriesResponse response = null;

            // get group configuration
            
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
                response = Core.ConditionalAccess.Module.GetFollowSeries(groupId, userID, domainID, order);
            }
            if (response.SeriesRecordings != null && response.SeriesRecordings.Count > 0)
            {
                result.TotalCount = response.TotalItems;
                // convert recordings            
                result.Objects = Mapper.Map<List<WebAPI.Models.ConditionalAccess.KalturaSeriesRecording>>(response.SeriesRecordings);
            }

            return result;
        }

        internal KalturaSeriesRecording RecordSeasonOrSeries(int groupId, string userID, long epgID, KalturaRecordingType recordingType)
        {
            KalturaSeriesRecording recording = null;
            SeriesRecording response = null;

            // get group ID
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = Core.ConditionalAccess.Module.RecordSeasonOrSeries(groupId, userID, epgID, ConditionalAccessMappings.ConvertKalturaRecordingType(recordingType));
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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

        internal KalturaAssetFileContext GetAssetFileContext(int groupId, string userID, string fileId, string udid, string language,
                                                                WebAPI.Models.ConditionalAccess.KalturaAssetFileContext.KalturaContextType contextType)
        {
            KalturaAssetFileContext kalturaResponse = null;
            EntitlementResponse response = null;
            bool isRecording = false;
            if (contextType == KalturaAssetFileContext.KalturaContextType.recording)
            {
                isRecording = true;
            }

            // get group ID
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    // fire request
                    response = Core.ConditionalAccess.Module.GetEntitlement(groupId, fileId.ToString(),
                        userID, true, string.Empty, language, udid, isRecording);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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


        internal bool RemoveHouseholdEntitlements(int groupId, int householdId)
        {
            Status status = null;

            // get group ID
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    status = Core.ConditionalAccess.Module.RemoveHouseholdEntitlements(groupId, householdId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (status == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(status.Code, status.Message);
            }

            return true;
        }

        internal KalturaEntitlement UpdateEntitlement(int groupId, long domainID, int id, KalturaEntitlement kEntitlement)
        {
            KalturaEntitlement kalturaEntitlement = null;
            Entitlements response = null;
           

            // get group 
            

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    if (kEntitlement is KalturaSubscriptionEntitlement)
                    {
                        Entitlement entitlement = Mapper.Map<Entitlement>(kEntitlement);
                        entitlement.purchaseID = id;
                        // fire request                        
                        response = Core.ConditionalAccess.Module.UpdateEntitlement(groupId, (int)domainID, entitlement);
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.status.Code != (int)StatusCode.OK)
            {
                // internal web service exception
                throw new ClientException(response.status.Code, response.status.Message);
            }

            if (response.entitelments == null || response.entitelments.Count < 1)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            // convert response
            if (kEntitlement is KalturaSubscriptionEntitlement)
            {
                kalturaEntitlement = ConditionalAccessMappings.ConvertToKalturaEntitlement(response.entitelments[0]);
            }

            return kalturaEntitlement;
        }

        internal bool SwapEntitlements(int groupId, string userId, int currentProductId, int newProductId, bool history)
        {
            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
            
                    response = Core.ConditionalAccess.Module.SwapSubscription(groupId, userId, currentProductId, newProductId, Utils.Utils.GetClientIP(), string.Empty, history);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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

        internal KalturaPlaybackContext GetPlaybackContext(int groupId, string userId, string udid, string assetId, KalturaAssetType assetType, KalturaPlaybackContextOptions contextDataParams)
        {
            KalturaPlaybackContext kalturaPlaybackContext = null;
            PlaybackContextResponse response = null;

            contextDataParams.Validate();

            PlayContextType wsContext = ConditionalAccessMappings.ConvertPlayContextType(contextDataParams.Context.Value);

            StreamerType? streamerType = null;
            if (!string.IsNullOrEmpty(contextDataParams.StreamerType))
            {
                StreamerType type;
                if (!Enum.TryParse(contextDataParams.StreamerType, out type))
                {
                    throw new ClientException((int)StatusCode.Error, "Unknown streamerType");
                }
                streamerType = type;
            }

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.ConditionalAccess.Module.GetPlaybackContext(groupId, userId, udid, Utils.Utils.GetClientIP(), assetId, ApiMappings.ConvertAssetType(assetType), contextDataParams.GetMediaFileIds(), streamerType, contextDataParams.MediaProtocol, wsContext);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                // general exception
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)eResponseStatus.OK && response.Status.Code != (int)eResponseStatus.ServiceNotAllowed && response.Status.Code != (int)eResponseStatus.NotEntitled &&
                response.Status.Code != (int)eResponseStatus.RecordingPlaybackNotAllowedForNonExistingEpgChannel && 
                response.Status.Code != (int)eResponseStatus.RecordingPlaybackNotAllowedForNotEntitledEpgChannel &&
                response.Status.Code != (int)eResponseStatus.ConcurrencyLimitation && response.Status.Code != (int)eResponseStatus.MediaConcurrencyLimitation && 
                response.Status.Code != (int)eResponseStatus.DeviceTypeNotAllowed && response.Status.Code != (int)eResponseStatus.NoFilesFound)
            {
                throw new ClientException(response.Status.Code, response.Status.Message);
            }
            else if (response.Status.Code != (int)eResponseStatus.OK)
            {
                kalturaPlaybackContext = new KalturaPlaybackContext()
                {
                    Messages = new List<KalturaAccessControlMessage>()
                    {
                        new KalturaAccessControlMessage() {
                            Code = ((eResponseStatus)response.Status.Code).ToString(), Message = response.Status.Message
                        }
                    }
                };
            }

            kalturaPlaybackContext = Mapper.Map<KalturaPlaybackContext>(response);

            if (kalturaPlaybackContext.Sources == null || kalturaPlaybackContext.Sources.Count == 0)
            {
                kalturaPlaybackContext.Actions = new List<KalturaRuleAction>();
                kalturaPlaybackContext.Actions.Add(new KalturaRuleAction() { Type = KalturaRuleActionType.BLOCK });
            }

            return kalturaPlaybackContext;
        }

        internal string GetPlayManifest(int groupId, string userId, string assetId, KalturaAssetType assetType, long assetFileId, string udid, KalturaPlaybackContextType contextType)
        {
            PlayManifestResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.ConditionalAccess.Module.GetPlayManifest(groupId, userId, assetId, ApiMappings.ConvertAssetType(assetType),
                        assetFileId, Utils.Utils.GetClientIP(), udid, ConditionalAccessMappings.ConvertPlayContextType(contextType));
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
                ErrorUtils.HandleWSException(ex);
            }

            if (response == null)
            {
                throw new ClientException((int)StatusCode.Error, StatusCode.Error.ToString());
            }

            if (response.Status.Code != (int)StatusCode.OK)
            {
                throw new ClientException((int)response.Status.Code, response.Status.Message);
            }

            return response.Url;
        }

        internal KalturaCompensation AddCompensation(int groupId, string userId, KalturaCompensation compensation)
        {
            CompensationResponse response = null;
            
            Compensation wsCompensation = Mapper.Map<Compensation>(compensation);

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.ConditionalAccess.Module.AddCompensation(groupId, userId, wsCompensation);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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

            return Mapper.Map<WebAPI.Models.ConditionalAccess.KalturaCompensation>(response.Compensation);
        }

        internal void DeleteCompensation(int groupId, long compensationId)
        {
            Status response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.ConditionalAccess.Module.DeleteCompensation(groupId, compensationId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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

        internal KalturaCompensation GetCompensation(int groupId, long compensationId)
        {
            CompensationResponse response = null;

            try
            {
                using (KMonitor km = new KMonitor(Events.eEvent.EVENT_WS))
                {
                    response = Core.ConditionalAccess.Module.GetCompensation(groupId, compensationId);
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("Exception received while calling service. exception: {1}", ex);
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

            return Mapper.Map<WebAPI.Models.ConditionalAccess.KalturaCompensation>(response.Compensation);
        }
        
    }
}
