
﻿using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using ApiObjects;
using ApiObjects.Epg;
using com.llnw.mediavault;
using DAL;
using KLogMonitor;
using Pricing;
using WS_Pricing;
using WS_API;

namespace ConditionalAccess
{
    class EutelsatConditionalAccess : TvinciConditionalAccess
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        private const int LEFT_MARGIN = 3;
        private const int RIGHT_MARGIN = 8;
        private static readonly string UNREACHABLE_ERROR = "Unable to connect to the billing server";

        public EutelsatConditionalAccess(Int32 nGroupID)
            : base(nGroupID)
        {
        }

        public EutelsatConditionalAccess(Int32 nGroupID, string connKey)
            : base(nGroupID, connKey)
        {
        }

        protected override string GetPPVCodeForGetItemsPrices(string ppvObjectCode, string ppvObjectVirtualName)
        {
            return String.Concat(ppvObjectCode, "|", ppvObjectVirtualName);
        }

        /// <summary>
        /// Credit Card Charge User For Media File
        /// </summary>
        public override TvinciBilling.BillingResponse CC_ChargeUserForMediaFile(string sSiteGUID, double dPrice, string sCurrency, Int32 nMediaFileID, Int32 nMediaID, string sPPVModuleCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd, string sLANGUAGE_CODE, string sDeviceUDID, bool bDummy, string sPaymentMethodID, string sEncryptedCVV)
        {
            TvinciBilling.BillingResponse ret = new TvinciBilling.BillingResponse();

            try
            {
                log.Debug("CC_ChargeUserForMediaFile - " + string.Format("Entering CC_ChargeUserForMediaFile try block. Site Guid: {0} , Media File ID: {1} , Media ID: {2} , Price: {3} , PPV Module Code: {4} , Coupon: {5} , User IP: {6} , UDID: {7}", sSiteGUID, nMediaFileID, nMediaID, dPrice, sPPVModuleCode, sCouponCode, sUserIP, sDeviceUDID));
                ret = ChargeUserForMediaFile(sSiteGUID, dPrice, sCurrency, nMediaFileID, nMediaID, sPPVModuleCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd, sLANGUAGE_CODE, sDeviceUDID);
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder(String.Concat("Exception at Eutelsat CC_ChargeUserForMediaFile. Msg: ", ex.Message));
                sb.Append(String.Concat("Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" Media File ID: ", nMediaFileID));
                sb.Append(String.Concat(" Price: ", dPrice));
                sb.Append(String.Concat(" UDID: ", sDeviceUDID));
                sb.Append(String.Concat(" Currency: ", sCurrency));
                sb.Append(String.Concat(" Cntry CD: ", sCountryCd));
                sb.Append(String.Concat(" Lng Cd: ", sLANGUAGE_CODE));
                sb.Append(String.Concat(" Trace: ", ex.StackTrace));

                log.Error("Exception - " + sb.ToString(), ex);

                #endregion
                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                ret.m_sRecieptCode = string.Empty;
                ret.m_sStatusDescription = "Failed to purchase media";

                return ret;
            }

            return ret;
        }



        /// <summary>
        /// Credit Card Charge User For Media File
        /// </summary>
        protected TvinciBilling.BillingResponse ChargeUserForMediaFile(string sSiteGUID, double dPrice, string sCurrency, int nMediaFileID, int nMediaID, string sPPVModuleCode, string sCouponCode, string sUserIP, string sExtraParameters,
            string sCountryCd, string sLANGUAGE_CODE, string sDeviceUDID)
        {
            string sWSUserName = string.Empty;
            string sWSPass = string.Empty;
            string sWSURL = string.Empty;
            TvinciUsers.UsersService u = null;
            TvinciBilling.module bm = null;
            mdoule m = null;

            TvinciBilling.BillingResponse ret = new ConditionalAccess.TvinciBilling.BillingResponse();
            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnown;
            ret.m_sRecieptCode = string.Empty;
            ret.m_sStatusDescription = string.Empty;

            try
            {
                if (string.IsNullOrEmpty(sSiteGUID))
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    ret.m_sRecieptCode = string.Empty;
                    ret.m_sStatusDescription = "Cannot charge an unknown user";

                    return ret;
                }

                // Get Household ID (UID) 
                string sHouseholdUID = DomainDal.GetDomainCoGuid(0, sSiteGUID);
                if (string.IsNullOrEmpty(sHouseholdUID))
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    ret.m_sRecieptCode = string.Empty;
                    ret.m_sStatusDescription = "Cannot charge an unknown household";

                    return ret;
                }


                // Check user validity
                u = new ConditionalAccess.TvinciUsers.UsersService();
                Utils.GetWSCredentials(m_nGroupID, eWSModules.USERS, ref sWSUserName, ref sWSPass);
                sWSURL = Utils.GetWSURL("users_ws");
                if (!string.IsNullOrEmpty(sWSURL))
                {
                    u.Url = sWSURL;
                }

                ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID, string.Empty);
                if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    ret.m_sRecieptCode = string.Empty;
                    ret.m_sStatusDescription = "Cant charge an unknown user";

                    return ret;
                }
                else if (uObj != null && uObj.m_user != null && uObj.m_user.m_eSuspendState == TvinciUsers.DomainSuspentionStatus.Suspended)
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UserSuspended;
                    ret.m_sRecieptCode = string.Empty;
                    ret.m_sStatusDescription = "Cannot charge a suspended user";
                    WriteToUserLog(sSiteGUID, "while trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                    return ret;
                }

                int nDomainID = uObj.m_user.m_domianID;

                // Domain Description is used as Arvato Contract ID
                string sArvatoContractID = DAL.DomainDal.GetDomainDesc(m_nGroupID, nDomainID);


                // Check coupon validity
                if (!string.IsNullOrEmpty(sCouponCode) && !Utils.IsCouponValid(m_nGroupID, sCouponCode))
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                    ret.m_sRecieptCode = string.Empty;
                    ret.m_sStatusDescription = "Coupon not valid";
                    WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                    return ret;
                }


                sWSUserName = string.Empty;
                sWSPass = string.Empty;

                m = new mdoule();
                if (string.IsNullOrEmpty(sPPVModuleCode))
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                    ret.m_sRecieptCode = string.Empty;
                    ret.m_sStatusDescription = "Charge must have ppv module code";
                    WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                    return ret;
                }
                // chack if ppvModule related to mediaFile 
                long ppvModuleCode = 0;
                long.TryParse(sPPVModuleCode, out ppvModuleCode);

                PPVModule thePPVModule = m.ValidatePPVModuleForMediaFile(sWSUserName, sWSPass, nMediaFileID, ppvModuleCode);

                if (thePPVModule == null)
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                    ret.m_sRecieptCode = string.Empty;
                    ret.m_sStatusDescription = "The ppv module is unknown";
                    WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                    return ret;
                }

                if (!thePPVModule.m_sObjectCode.Equals(sPPVModuleCode))
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownPPVModule;
                    ret.m_sRecieptCode = string.Empty;
                    ret.m_sStatusDescription = "This PPVModule does not belong to item";
                    WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                    return ret;
                }

                // User and PPV Module OK, let's try to charge the user 
                // Try retrieving Media's ContentID from CoGuid (= ContentID_FormatID)

                string sMediaFileCoGuid = Utils.GetMediaFileCoGuid(m_nGroupID, nMediaFileID);

                if (string.IsNullOrEmpty(sMediaFileCoGuid))
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                    ret.m_sRecieptCode = string.Empty;
                    ret.m_sStatusDescription = "The external identifier of the item is empty";
                    WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);

                    return ret;
                }

                PriceReason theReason = PriceReason.UnKnown;

                Subscription relevantSub = null;
                Collection relevantCol = null;
                PrePaidModule relevantPP = null;

                Price p = Utils.GetMediaFileFinalPriceForNonGetItemsPrices(nMediaFileID, thePPVModule, sSiteGUID, sCouponCode, m_nGroupID, ref theReason, ref relevantSub, ref relevantCol, ref relevantPP, sCountryCd, sLANGUAGE_CODE, sDeviceUDID);

                if (theReason == PriceReason.ForPurchase || (theReason == PriceReason.SubscriptionPurchased && p.m_dPrice > 0))
                {
                    if (p.m_dPrice != dPrice || p.m_oCurrency.m_sCurrencyCD3 != sCurrency)
                    {
                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.PriceNotCorrect;
                        ret.m_sRecieptCode = string.Empty;
                        ret.m_sStatusDescription = "The price of the request is not the actual price";
                        WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);

                        return ret;
                    }


                    // Check user's entitlement via 3SS interface
                    bool skip3SSCheck = false;
                    if (TVinciShared.WS_Utils.GetTcmConfigValue("SKIP_3SS_CHECK") != string.Empty)
                    {
                        skip3SSCheck = bool.Parse(TVinciShared.WS_Utils.GetTcmConfigValue("SKIP_3SS_CHECK"));
                    }

                    if (!skip3SSCheck)
                    {
                        EutelsatTransactionResponse externalEntitledResponse = IsUserTvodAllowed(sHouseholdUID, sSiteGUID, sArvatoContractID, dPrice, sCurrency, sMediaFileCoGuid);

                        if (!externalEntitledResponse.Success)
                        {
                            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                            ret.m_sRecieptCode = string.Empty;
                            ret.m_sStatusDescription =
                                "Error " + externalEntitledResponse.ErrorCode + ">" + externalEntitledResponse.ErrorMessage; // "User is not entitled for VOD purchasing";

                            WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() +
                                                      " error returned: " + ret.m_sStatusDescription);
                            return ret;
                        }

                        ret.m_sRecieptCode = externalEntitledResponse.TransactionId;
                    }

                    string sCustomData = string.Empty;
                    if (p.m_dPrice != 0)
                    {
                        bm = new ConditionalAccess.TvinciBilling.module();
                        sWSUserName = string.Empty;
                        sWSPass = string.Empty;
                        Utils.GetWSCredentials(m_nGroupID, eWSModules.BILLING, ref sWSUserName, ref sWSPass);
                        sWSURL = Utils.GetWSURL("billing_ws");
                        if (!string.IsNullOrEmpty(sWSURL))
                        {
                            bm.Url = sWSURL;
                        }

                        if (string.IsNullOrEmpty(sCountryCd) && !string.IsNullOrEmpty(sUserIP))
                        {
                            sCountryCd = TVinciShared.WS_Utils.GetIP2CountryCode(sUserIP);
                        }

                        //Create the Custom Data
                        sCustomData = GetCustomData(relevantSub, thePPVModule, null, sSiteGUID, dPrice, sCurrency, nMediaFileID, nMediaID, sPPVModuleCode, string.Empty, sCouponCode, sUserIP,
                                                    sCountryCd, sLANGUAGE_CODE, sDeviceUDID);
                        log.Debug("CustomData - " + sCustomData);


                        ret = bm.CC_DummyChargeUser(sWSUserName, sWSPass, sSiteGUID, dPrice, sCurrency, sUserIP, sCustomData, 1, 1, sExtraParameters);
                    }

                    if (ret.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.Success)
                    {
                        HandleCouponUses(relevantSub, string.Empty, sSiteGUID, p.m_dPrice, sCurrency, nMediaFileID, sCouponCode, sUserIP, sCountryCd, sLANGUAGE_CODE, sDeviceUDID, true, 0, 0);

                        int transactionID = int.Parse(ret.m_sRecieptCode);

                        int maxNumOfUses = 0;
                        DateTime? dtEndDate = null;
                        if (thePPVModule != null && thePPVModule.m_oUsageModule != null)
                        {
                            maxNumOfUses = thePPVModule.m_oUsageModule.m_nMaxNumberOfViews;
                            dtEndDate = Utils.GetEndDateTime(DateTime.UtcNow, thePPVModule.m_oUsageModule.m_tsMaxUsageModuleLifeCycle);
                        }

                        string subCode = (relevantSub != null) ? relevantSub.m_sObjectCode : null;
                        bool dbInsertPpvPurchase = DAL.ConditionalAccessDAL.InsertPPVPurchase(m_nGroupID, subCode, nMediaFileID, sSiteGUID, dPrice, sCurrency, 0, sCustomData, transactionID, sCountryCd,
                                                                                                sLANGUAGE_CODE, sDeviceUDID, maxNumOfUses, 1, 1, dtEndDate, nDomainID);

                        WriteToUserLog(sSiteGUID, "Media file id: " + nMediaFileID.ToString() + " Purchased(CC): " + dPrice.ToString() + sCurrency);


                        //int nPurchaseID = 0;
                        int nPurchaseID = ConditionalAccessDAL.GetPPVPurchaseID(m_nGroupID, subCode, nMediaFileID, sSiteGUID, dPrice, sCurrency, 0, maxNumOfUses, 1, 1);

                        //Should update the PURCHASE_ID
                        string sReceipt = ret.m_sRecieptCode;
                        if (!string.IsNullOrEmpty(sReceipt))
                        {
                            bool updatedPurchaseId = ConditionalAccessDAL.UpdatePurchaseID(transactionID, nPurchaseID);
                        }
                        else
                        {
                            WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                        }


                        // If device is portal (3SS), do NOT notify!
                        //
                        string transDeviceFilter = string.Empty;
                        if (TVinciShared.WS_Utils.GetTcmConfigValue("Transaction_Device_Filter") != string.Empty)
                        {
                            transDeviceFilter = TVinciShared.WS_Utils.GetTcmConfigValue("Transaction_Device_Filter");
                        }

                        if (!string.IsNullOrEmpty(sDeviceUDID) && (!sDeviceUDID.Contains(transDeviceFilter)))
                        {

                            try
                            {
                                string sPPVModuleVirtualName = thePPVModule.m_sObjectVirtualName == sPPVModuleCode ? thePPVModule.m_sObjectVirtualName : string.Empty;
                                sPPVModuleCode = thePPVModule.m_sObjectCode;

                                EutelsatTransactionResponse transNotificationRes =
                                    MakeTransactionNotification(sHouseholdUID, dPrice, sCurrency, nMediaFileID, sMediaFileCoGuid, sPPVModuleVirtualName, sCouponCode, sDeviceUDID, transactionID);

                                if (!transNotificationRes.Success)
                                {
                                    bool canceledTransaction = ConditionalAccessDAL.CancelTransaction(transactionID);
                                    bool canceled = canceledTransaction && ConditionalAccessDAL.CancelPpvPurchase(m_nGroupID, nPurchaseID, sSiteGUID, nMediaFileID, 0, 2);

                                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                    ret.m_sRecieptCode = sReceipt;
                                    ret.m_sStatusDescription = "Error " + transNotificationRes.ErrorCode + ">" + transNotificationRes.ErrorMessage; //"User is not entitled for VOD purchasing";

                                    WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                                }
                            }
                            catch (Exception ex)
                            {
                                StringBuilder sbLog = new StringBuilder("Exception while trying to make transaction notification. ");
                                sbLog.Append(String.Concat("Site Guid: ", sSiteGUID));
                                sbLog.Append(String.Concat(" MF ID: ", nMediaFileID));
                                sbLog.Append(String.Concat(" UDID: ", sDeviceUDID));
                                sbLog.Append(String.Concat(" Msg: ", ex.Message));
                                sbLog.Append(String.Concat(" Trace: ", ex.StackTrace));
                                log.Error("Exception - " + sbLog.ToString(), ex);
                            }
                        }
                    }
                    else
                    {
                        WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                    }
                }
                else
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                    ret.m_sRecieptCode = string.Empty;

                    switch (theReason)
                    {
                        case PriceReason.PPVPurchased:
                            ret.m_sStatusDescription = "The media file is already purchased";
                            break;
                        case PriceReason.Free:
                            ret.m_sStatusDescription = "The media file is free";
                            break;
                        case PriceReason.ForPurchaseSubscriptionOnly:
                            ret.m_sStatusDescription = "The media file is for purchase with subscription only";
                            break;
                        case PriceReason.SubscriptionPurchased:
                            ret.m_sStatusDescription = "The media file is already purchased (subscription)";
                            break;
                        default:
                            break;
                    }

                    WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                }
            }
            finally
            {
                if (u != null)
                {
                    u.Dispose();
                }
                if (bm != null)
                {
                    bm.Dispose();
                }
                if (m != null)
                {
                    m.Dispose();
                }
            }
            return ret;
        }



        protected static EutelsatTransactionResponse IsUserTvodAllowed(string sHouseholdUID, string siteGUID, string sArvatoContractID, double dPrice, string sCurrency, string sExternalMediaFileID)
        {
            EutelsatTransactionResponse res = new EutelsatTransactionResponse();
            res.Success = false;

            string sWSURL = Utils.GetWSURL("Eutelsat_CheckTvod_ws");
            string sWSUsername = Utils.GetValueFromConfig("Eutelsat_3SS_WS_Username");
            string sWSPassword = Utils.GetValueFromConfig("Eutelsat_3SS_WS_Password");

            if (string.IsNullOrEmpty(sWSURL) || string.IsNullOrEmpty(sHouseholdUID))
            {
                return res;
            }

            EutelsatCheckTvod checkTvod = new EutelsatCheckTvod()
            {
                UserID = sHouseholdUID,
                Price = dPrice,
                Currency = sCurrency,
                AssetID = sExternalMediaFileID,
                IPNO_ID = sHouseholdUID.Substring(0, 8),    // Take first 8 digits
                SiteGUID = siteGUID,
                ContractID = sArvatoContractID
            };

            string jsonCheckTvodContent = Newtonsoft.Json.JsonConvert.SerializeObject(new EutelsatCheckTvodRequest() { CheckTvod = checkTvod });    //checkTvod.Serialize();


            Uri requestUri = null;
            bool isGoodUri = Uri.TryCreate(sWSURL, UriKind.Absolute, out requestUri) && requestUri.Scheme == Uri.UriSchemeHttp;

            if (isGoodUri)
            {
                res = MakeJsonRequest(requestUri, sWSUsername, sWSPassword, jsonCheckTvodContent);

            }

            return res;
        }

        protected EutelsatTransactionResponse MakeTransactionNotification
            (string sHouseholdUID, double dPrice, string sCurrency, int nAssetID, string sExternalAssetID, string sPpvModuleCode, string sCouponCode, string sDeviceUDID, int nTransactionID)
        {

            EutelsatTransactionResponse res = new EutelsatTransactionResponse();
            res.Success = false;

            try
            {
                string sWSURL = Utils.GetWSURL("Eutelsat_Transaction_ws");
                string sWSUsername = Utils.GetValueFromConfig("Eutelsat_3SS_WS_Username");
                string sWSPassword = Utils.GetValueFromConfig("Eutelsat_3SS_WS_Password");

                if (string.IsNullOrEmpty(sHouseholdUID) || string.IsNullOrEmpty(sWSURL))
                {
                    return res;
                }

                int nDeviceBrandID = 0;
                int nDeviceFamilyID = DeviceDal.GetDeviceFamilyID(m_nGroupID, sDeviceUDID, ref nDeviceBrandID);

                EutelsatTransaction trans = new EutelsatTransaction()
                {
                    UserID = sHouseholdUID,
                    Price = dPrice,
                    Currency = sCurrency,
                    AssetID = nAssetID,
                    PPVModuleCode = sPpvModuleCode,
                    CouponCode = sCouponCode,
                    RoviID = sExternalAssetID,
                    DeviceUDID = sDeviceUDID,
                    DeviceBrandID = nDeviceBrandID,
                    TransactionID = nTransactionID,
                    TransactionType = nTransactionID.ToString()
                };

                EutelsatTransactionRequest transRequest = new EutelsatTransactionRequest() { Transaction = trans };

                var jsonTransactionContent = Newtonsoft.Json.JsonConvert.SerializeObject(transRequest);

                //http://82.79.128.235:8080/TvinciService.svc/user/check_tvod.json?user_id={userId}&price={price}&currency={currency}&asset_id={assetId}

                Uri requestUri = null;
                bool isGoodUri = Uri.TryCreate(sWSURL, UriKind.Absolute, out requestUri) && requestUri.Scheme == Uri.UriSchemeHttp;

                if (isGoodUri)
                {
                    res = MakeJsonRequest(requestUri, sWSUsername, sWSPassword, jsonTransactionContent);

                    if (res == null)
                    {
                        res = new EutelsatTransactionResponse { Success = false, ErrorCode = 404, ErrorMessage = UNREACHABLE_ERROR, TransactionId = nTransactionID.ToString() };
                    }
                }

            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at MakeTransactionNotification. ");
                sb.Append(String.Concat("Household: ", sHouseholdUID));
                sb.Append(String.Concat(" Asset ID: ", nAssetID));
                sb.Append(String.Concat(" External Asset ID: ", sExternalAssetID));
                sb.Append(String.Concat(" PPV Module: ", sPpvModuleCode));
                sb.Append(String.Concat(" Coupon: ", sCouponCode));
                sb.Append(String.Concat(" UDID: ", sDeviceUDID));
                sb.Append(String.Concat(" Trans ID: ", nTransactionID));
                sb.Append(String.Concat(" Trace: ", ex.StackTrace));

                log.Error("Exception - " + sb.ToString(), ex);
                #endregion

                int errorCode = System.Runtime.InteropServices.Marshal.GetExceptionCode();
                res = new EutelsatTransactionResponse { Success = false, ErrorCode = errorCode, ErrorMessage = ex.Message, TransactionId = nTransactionID.ToString() };
            }

            return res;
        }

        protected EutelsatTransactionResponse MakeSubNotification(string sHouseholdUID, string sProductID, double dPrice, string sCurrency, string sDeviceUDID, int nTransactionID)
        {
            //"{"subscription":{"user_id" : "049003381530799", "product_id" : "11113", "price" : "12.5", "currency" : "Eur", "device_id" : "device"}}"

            EutelsatTransactionResponse res = new EutelsatTransactionResponse();
            res.Success = false;

            try
            {
                string sWSURL = Utils.GetWSURL("Eutelsat_Subscription_ws");
                string sWSUsername = Utils.GetValueFromConfig("Eutelsat_3SS_WS_Username");
                string sWSPassword = Utils.GetValueFromConfig("Eutelsat_3SS_WS_Password");

                if (string.IsNullOrEmpty(sHouseholdUID) || string.IsNullOrEmpty(sWSURL))
                {
                    return res;
                }

                EutelsatSubscription sub = new EutelsatSubscription()
                {
                    UserID = sHouseholdUID,
                    Price = dPrice,
                    Currency = sCurrency,
                    ProductID = sProductID,
                    DeviceUDID = sDeviceUDID
                };

                EutelsatSubRequest subRequest = new EutelsatSubRequest() { Subscription = sub };

                var jsonTransactionContent = Newtonsoft.Json.JsonConvert.SerializeObject(subRequest);

                //http://82.79.128.235:8080/TvinciService.svc/user/check_tvod.json?user_id={userId}&price={price}&currency={currency}&asset_id={assetId}

                Uri requestUri = null;
                bool isGoodUri = Uri.TryCreate(sWSURL, UriKind.Absolute, out requestUri) && requestUri.Scheme == Uri.UriSchemeHttp;

                if (isGoodUri)
                {
                    res = MakeJsonRequest(requestUri, sWSUsername, sWSPassword, jsonTransactionContent);

                    if (res == null)
                    {
                        res = new EutelsatTransactionResponse { Success = false, ErrorCode = 404, ErrorMessage = UNREACHABLE_ERROR, TransactionId = nTransactionID.ToString() };
                    }
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at MakeSubNotification. ");
                sb.Append(String.Concat("Exception: ", ex.Message));
                sb.Append(String.Concat(" Household ID: ", sHouseholdUID));
                sb.Append(String.Concat(" Product ID: ", sProductID));
                sb.Append(String.Concat(" Trans ID: ", nTransactionID));
                sb.Append(String.Concat(" UDID: ", sDeviceUDID));
                sb.Append(String.Concat(" Price: ", dPrice));
                sb.Append(String.Concat(" Currency: ", sCurrency));
                sb.Append(String.Concat(" Trace: ", ex.StackTrace));
                log.Error("Exception - " + sb.ToString(), ex);
                #endregion

                int errorCode = System.Runtime.InteropServices.Marshal.GetExceptionCode();
                res = new EutelsatTransactionResponse { Success = false, ErrorCode = errorCode, ErrorMessage = ex.Message, TransactionId = nTransactionID.ToString() };
            }

            return res;
        }

        public static EutelsatTransactionResponse MakeJsonRequest(Uri requestUri, string wsUsername, string wsPassword, string jsonContent = "")
        {
            try
            {
                string sRes = TVinciShared.WS_Utils.SendXMLHttpReq(requestUri.OriginalString, jsonContent, "", "application/json", "UserName", wsUsername, "Password", wsPassword);
                object objResponse = Newtonsoft.Json.JsonConvert.DeserializeObject(sRes, typeof(EutelsatTransactionResponse));

                return (EutelsatTransactionResponse)objResponse;
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sb = new StringBuilder("Exception at MakeJsonRequest. ");
                sb.Append(String.Concat(" Msg: ", ex.Message));
                sb.Append(String.Concat(" Req URI: ", requestUri != null ? requestUri.OriginalString : "null"));
                sb.Append(String.Concat(" JSON: ", jsonContent));
                sb.Append(String.Concat(" Trace: ", ex.StackTrace));

                log.Error("MakeJsonRequest - " + sb.ToString(), ex);
                #endregion
            }

            return null;
        }


        /// <summary>
        /// Credit Card Charge User For Subscription
        /// </summary>
        public override TvinciBilling.BillingResponse CC_ChargeUserForBundle(string sSiteGUID, double dPrice, string sCurrency, string sSubscriptionCode, string sCouponCode, string sUserIP,
            string sExtraParams, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, bool bDummy, string sPaymentMethodID, string sEncryptedCVV, eBundleType bundleType)
        {
            TvinciBilling.BillingResponse ret = new ConditionalAccess.TvinciBilling.BillingResponse();
            TvinciUsers.UsersService u = null;

            try
            {
                #region User and household validation

                log.Debug("CC_ChargeUserForBundle - " + string.Format("Entering CC_ChargeUserForBundle try block. Site Guid: {0} , Price: {1} , Sub Code: {2} , Coupon: {3} , User IP: {4} , Dummy {5} , Bundle: {6}", sSiteGUID, dPrice, sSubscriptionCode, sCouponCode, sUserIP, bDummy.ToString().ToLower(), bundleType.ToString()));
                if (string.IsNullOrEmpty(sSiteGUID))
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    ret.m_sRecieptCode = string.Empty;
                    ret.m_sStatusDescription = "Cant charge an unknown user";

                    return ret;
                }

                // Get Household ID (UID) 
                string sHouseholdUID = DAL.DomainDal.GetDomainCoGuid(0, sSiteGUID);
                if (string.IsNullOrEmpty(sHouseholdUID))
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    ret.m_sRecieptCode = string.Empty;
                    ret.m_sStatusDescription = "Cannot charge an unknown household";

                    return ret;
                }

                u = new ConditionalAccess.TvinciUsers.UsersService();
                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;
                Utils.GetWSCredentials(m_nGroupID, eWSModules.USERS, ref sWSUserName, ref sWSPass);
                string sWSURL = Utils.GetWSURL("users_ws");
                if (!string.IsNullOrEmpty(sWSURL))
                {
                    u.Url = sWSURL;
                }

                ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID, string.Empty);
                if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                    ret.m_sRecieptCode = string.Empty;
                    ret.m_sStatusDescription = "Cant charge an unknown user";

                    return ret;
                }

                else if (uObj != null && uObj.m_user != null && uObj.m_user.m_eSuspendState == TvinciUsers.DomainSuspentionStatus.Suspended)
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UserSuspended;
                    ret.m_sRecieptCode = string.Empty;
                    ret.m_sStatusDescription = "Cannot charge a suspended user";
                    WriteToUserLog(sSiteGUID, "while trying to purchase subscription(CC): " + sSubscriptionCode + " error returned: " + ret.m_sStatusDescription);
                }

                int domainId = uObj.m_user.m_domianID;

                #endregion


                if (!string.IsNullOrEmpty(sCouponCode) && !Utils.IsCouponValid(m_nGroupID, sCouponCode))
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                    ret.m_sRecieptCode = string.Empty;
                    ret.m_sStatusDescription = "Coupon not valid";
                    WriteToUserLog(sSiteGUID, "While trying to purchase subscription(CC): " + sSubscriptionCode + " error returned: " + ret.m_sStatusDescription);
                    return ret;
                }

                PriceReason theReason = PriceReason.UnKnown;
                Subscription theSub = null;
                Price p = Utils.GetSubscriptionFinalPrice(m_nGroupID, sSubscriptionCode, sSiteGUID, sCouponCode, ref theReason, ref theSub, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                if (p != null)
                {
                    dPrice = p.m_dPrice;
                    sCurrency = p.m_oCurrency.m_sCurrencyCD3;
                }

                switch (theReason)
                {
                    case PriceReason.Free:

                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                        ret.m_sRecieptCode = string.Empty;
                        ret.m_sStatusDescription = "The subscription is free";
                        WriteToUserLog(sSiteGUID, "while trying to purchase subscription(CC): " + " error returned: " + ret.m_sStatusDescription);

                        break;

                    case PriceReason.SubscriptionPurchased:

                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                        ret.m_sRecieptCode = string.Empty;
                        ret.m_sStatusDescription = "The subscription is already purchased";
                        WriteToUserLog(sSiteGUID, "while trying to purchase subscription(CC): " + " error returned: " + ret.m_sStatusDescription);

                        break;

                    case PriceReason.ForPurchase:

                        ret = HandleSubPurchase(sSiteGUID, sHouseholdUID, dPrice, sCurrency, sSubscriptionCode, sCouponCode, sUserIP, sExtraParams, ref sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, "1.1.1.1", theSub, p, domainId);

                        break;

                    default:
                        #region Logging
                        StringBuilder sb = new StringBuilder("Error. PriceReason not supported.");
                        sb.Append(String.Concat("PriceReason: ", theReason.ToString()));
                        sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                        sb.Append(String.Concat(" Sub Code: ", sSubscriptionCode));
                        sb.Append(String.Concat(" User IP: ", sUserIP));
                        sb.Append(String.Concat(" Price: ", dPrice));
                        sb.Append(String.Concat(" Bundle Type: ", bundleType.ToString()));

                        log.Error("Error - " + sb.ToString());
                        #endregion
                        break;
                }
            }
            catch (Exception ex)
            {
                #region Logging
                StringBuilder sbEx = new StringBuilder("Exception at CC_ChargeUserForBundle. ");
                sbEx.Append(String.Concat(" Msg: ", ex.Message));
                sbEx.Append(String.Concat(" Site Guid: ", sSiteGUID));
                sbEx.Append(String.Concat(" Sub Code: ", sSubscriptionCode));
                sbEx.Append(String.Concat(" Price: ", dPrice));
                sbEx.Append(String.Concat(" Coupon: ", sCouponCode));
                sbEx.Append(String.Concat(" User IP: ", sUserIP));
                sbEx.Append(String.Concat(" Dummy: ", bDummy.ToString().ToLower()));
                sbEx.Append(String.Concat(" Bundle Type: ", bundleType.ToString()));
                sbEx.Append(String.Concat(" Trace: ", ex.StackTrace));

                log.Error("Exception - " + sbEx.ToString(), ex);

                #endregion

            }
            finally
            {
                if (u != null)
                {
                    u.Dispose();
                }
            }
            return ret;
        }

        private TvinciBilling.BillingResponse HandleSubPurchase(string sSiteGUID, string sHouseholdUID, double dPrice, string sCurrency, string sSubscriptionCode, string sCouponCode, string sUserIP,
                                                            string sExtraParams, ref string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string sIP, Subscription theSub,
                                                            Price p, int domainId)
        {

            TvinciBilling.BillingResponse ret = new TvinciBilling.BillingResponse();

            if (p == null || dPrice != p.m_dPrice || sCurrency != p.m_oCurrency.m_sCurrencyCD3)
            {
                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.PriceNotCorrect;
                ret.m_sRecieptCode = "";
                ret.m_sStatusDescription = "The price of the request is not the actual price";

                WriteToUserLog(sSiteGUID, "while trying to purchase subscription(CC): " + " error returned: " + ret.m_sStatusDescription);

                return ret;
            }


            string sCustomData = string.Empty;

            bool bIsRecurring = theSub.m_bIsRecurring;
            Int32 nRecPeriods = theSub.m_nNumberOfRecPeriods;

            if (string.IsNullOrEmpty(sCountryCd) && !string.IsNullOrEmpty(sUserIP))
            {
                sCountryCd = TVinciShared.WS_Utils.GetIP2CountryCode(sUserIP);
            }

            //Create the Custom Data
            sCustomData = GetCustomDataForSubscription(theSub, null, sSubscriptionCode, string.Empty, sSiteGUID, dPrice, sCurrency, sCouponCode, sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

            log.Debug("CustomData - " + sCustomData);

            if (p.m_dPrice != 0)
            {
                using (TvinciBilling.module bm = new ConditionalAccess.TvinciBilling.module())
                {
                    string sWSUserName = string.Empty;
                    string sWSPass = string.Empty;
                    Utils.GetWSCredentials(m_nGroupID, eWSModules.BILLING, ref sWSUserName, ref sWSPass);
                    string sWSURL = Utils.GetWSURL("billing_ws");

                    if (!string.IsNullOrEmpty(sWSURL))
                    {
                        bm.Url = sWSURL;
                    }

                    ret = bm.CC_DummyChargeUser(sWSUserName, sWSPass, sSiteGUID, dPrice, sCurrency, sUserIP, sCustomData, 1, nRecPeriods, sExtraParams);
                }
            }

            if (p.m_dPrice == 0 && !string.IsNullOrEmpty(sCouponCode))
            {
                ret.m_oStatus = TvinciBilling.BillingResponseStatus.Success;
            }

            if (ret.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.Success)
            {
                HandleCouponUses(theSub, string.Empty, sSiteGUID, dPrice, sCurrency, 0, sCouponCode, sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, true, 0, 0);

                bool dbRes = DAL.ConditionalAccessDAL.UpdateSubPurchase(m_nGroupID, sSiteGUID, sSubscriptionCode, bIsRecurring ? 1 : 0);

                int nMaxNumberOfViews = (theSub != null && theSub.m_oUsageModule != null) ? theSub.m_oUsageModule.m_nMaxNumberOfViews : 0;
                int nViewLifeCycleSec = (theSub != null && theSub.m_oUsageModule != null) ? theSub.m_oUsageModule.m_tsViewLifeCycle : 0;

                int nReceiptCode = (!string.IsNullOrEmpty(ret.m_sRecieptCode)) ? int.Parse(ret.m_sRecieptCode) : 0;

                DateTime? dtEndDate = null;
                if (theSub != null && theSub.m_oSubscriptionUsageModule != null)
                {
                    dtEndDate = Utils.GetEndDateTime(DateTime.UtcNow, theSub.m_oSubscriptionUsageModule.m_tsMaxUsageModuleLifeCycle);
                }

                long purchaseId = ConditionalAccessDAL.Insert_NewMPPPurchase(m_nGroupID, sSubscriptionCode, sSiteGUID, dPrice, sCurrency, sCustomData, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, nMaxNumberOfViews,
                                                                             nViewLifeCycleSec, bIsRecurring, nReceiptCode, 0, DateTime.UtcNow, dtEndDate.Value, DateTime.UtcNow, string.Empty, domainId);

                WriteToUserLog(sSiteGUID, "Subscription purchase (CC): " + sSubscriptionCode);

                if (nReceiptCode > 0)
                {
                    dbRes = ConditionalAccessDAL.UpdatePurchaseID(nReceiptCode, (int)purchaseId);
                }


                // If device is portal (3SS), do NOT notify!
                //
                string transDeviceFilter = string.Empty;
                if (TVinciShared.WS_Utils.GetTcmConfigValue("Transaction_Device_Filter") != string.Empty)
                {
                    transDeviceFilter = TVinciShared.WS_Utils.GetTcmConfigValue("Transaction_Device_Filter");
                }

                if (!string.IsNullOrEmpty(sDEVICE_NAME) && (!sDEVICE_NAME.Contains(transDeviceFilter)))
                {

                    try
                    {
                        EutelsatTransactionResponse subNotificationRes = MakeSubNotification(sHouseholdUID, sSubscriptionCode, dPrice, sCurrency, sDEVICE_NAME, nReceiptCode);

                        if (!subNotificationRes.Success)
                        {
                            if (nReceiptCode > 0)
                            {
                                bool canceledTransaction = ConditionalAccessDAL.CancelTransaction(nReceiptCode);
                                bool canceled = canceledTransaction && ConditionalAccessDAL.CancelSubPurchase(m_nGroupID, sSubscriptionCode, (int)purchaseId, sSiteGUID);
                            }

                            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                            ret.m_sRecieptCode = nReceiptCode.ToString();
                            ret.m_sStatusDescription = "Error " + subNotificationRes.ErrorCode + ">" + subNotificationRes.ErrorMessage; //"User is not entitled for VOD purchasing";

                            WriteToUserLog(sSiteGUID, "While trying to purchase product id(CC): " + sSubscriptionCode + " error returned: " + ret.m_sStatusDescription);
                        }
                    }
                    catch (Exception ex)
                    {
                        StringBuilder sb = new StringBuilder("Exception while trying to make sub notification. ");
                        sb.Append(String.Concat(" Msg: ", ex.Message));
                        sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                        sb.Append(String.Concat(" Household: ", sHouseholdUID));
                        sb.Append(String.Concat(" Sub Code: ", sSubscriptionCode));
                        sb.Append(String.Concat(" Coupon Code: ", sCouponCode));
                        sb.Append(String.Concat(" Trace: ", ex.StackTrace));

                        log.Error("Exception - " + sb.ToString(), ex);
                    }
                }


            }
            else
            {
                WriteToUserLog(sSiteGUID, "while trying to purchase subscription(CC): " + sSubscriptionCode + " error returned: " + ret.m_sStatusDescription);
            }

            return ret;
        }

        public override LicensedLinkResponse GetEPGLink(string sProgramId, DateTime dStartTime, int format, string sSiteGUID, Int32 nMediaFileID, string sBasicLink, string sUserIP, string sRefferer, string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME, string sCouponCode)
        {
            LicensedLinkResponse oLicensedLinkResponse = new LicensedLinkResponse();
            API api = null;
            try
            {
                // Validate inputs
                int nProgramId = Int32.Parse(sProgramId);
                if ((nProgramId <= 0) || (string.IsNullOrEmpty(sBasicLink)) || (string.IsNullOrEmpty(sSiteGUID)))
                {
                    oLicensedLinkResponse.status = eLicensedLinkStatus.Error.ToString();
                    oLicensedLinkResponse.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                    return oLicensedLinkResponse;
                }

                int fileMainStreamingCoID = 0; // CDN Straming id
                int mediaId = 0;
                string fileType = string.Empty;
                oLicensedLinkResponse = GetLicensedLinks(sSiteGUID, nMediaFileID, sBasicLink, sUserIP, sRefferer, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, sCouponCode, eObjectType.EPG, 
                    ref fileMainStreamingCoID, ref mediaId, ref fileType);
                //GetLicensedLink return empty link no need to continue
                if (oLicensedLinkResponse == null || string.IsNullOrEmpty(oLicensedLinkResponse.mainUrl))
                {
                    log.Debug("LicensedLink - " +
                        string.Format("GetLicensedLink return empty basicLink siteGuid={0}, sBasicLink={1}, nMediaFileID={2}", sSiteGUID, sBasicLink, nMediaFileID));
                    return oLicensedLinkResponse;
                }

                Dictionary<string, object> dURLParams = new Dictionary<string, object>();

                string sRightMargin = Utils.GetValueFromConfig("right_margin");
                string sLeftMargin = Utils.GetValueFromConfig("left_margin");
                int nRightMargin = !string.IsNullOrEmpty(sRightMargin) ? int.Parse(sRightMargin) : RIGHT_MARGIN;
                int nLeftMargin = !string.IsNullOrEmpty(sLeftMargin) ? int.Parse(sLeftMargin) : LEFT_MARGIN;

                // Time Factor for aligment with Harmonic server (e.g. convert millisec -> 10Xmicrosec)
                string sTimeMultFactor = Utils.GetValueFromConfig("time_mult_factor");
                int timeMultFactor = 10000;
                if (!string.IsNullOrEmpty(sTimeMultFactor))
                {
                    int.TryParse(sTimeMultFactor, out timeMultFactor);
                }

                //call api service to get the epg_url_link 
                string sWSUserName = string.Empty;
                string sWSPass = string.Empty;
                api = new API();

                Utils.GetWSCredentials(m_nGroupID, eWSModules.API, ref sWSUserName, ref sWSPass);
                //get channel name 
                string channelName = api.GetCoGuidByMediaFileId(sWSUserName, sWSPass, nMediaFileID);

                eEPGFormatType eformat = (eEPGFormatType)format;
                Scheduling scheduling = api.GetProgramSchedule(sWSUserName, sWSPass, nProgramId);
                if (scheduling != null)
                {
                    dURLParams.Add(EpgLinkConstants.PROGRAM_END, scheduling.EndTime);

                    switch (eformat)
                    {
                        case eEPGFormatType.Catchup:
                        case eEPGFormatType.StartOver:
                            {
                                dURLParams.Add(EpgLinkConstants.PROGRAM_START, scheduling.StartDate);
                            }
                            break;
                        case eEPGFormatType.LivePause:
                            {
                                dURLParams.Add(EpgLinkConstants.PROGRAM_START, dStartTime);
                            }
                            break;
                        default:
                            {
                                oLicensedLinkResponse.status = eLicensedLinkStatus.Error.ToString();
                                oLicensedLinkResponse.Status.Code = (int)ApiObjects.Response.eResponseStatus.Error;
                                return oLicensedLinkResponse;
                            }
                    }

                }

                //call the right provider to get the epg link 

                string CdnStrID = string.Empty;
                bool bIsDynamic = Utils.GetStreamingUrlType(fileMainStreamingCoID, ref CdnStrID);

                dURLParams.Add(EpgLinkConstants.IS_DYNAMIC, bIsDynamic);
                dURLParams.Add(EpgLinkConstants.BASIC_LINK, sBasicLink);
                dURLParams.Add(EpgLinkConstants.RIGHT_MARGIN, nRightMargin);
                dURLParams.Add(EpgLinkConstants.LEFT_MARGIN, nLeftMargin);
                dURLParams.Add(EpgLinkConstants.TIME_MULT_FACTOR, timeMultFactor);
                dURLParams.Add(EpgLinkConstants.EPG_FORMAT_TYPE, eformat);
                dURLParams.Add(EpgLinkConstants.CHANNEL_NAME, channelName);


                StreamingProvider.ILSProvider provider = StreamingProvider.LSProviderFactory.GetLSProvidernstance(CdnStrID);
                if (provider != null)
                {
                    string liveUrl = provider.GenerateEPGLink(dURLParams);
                    if (!string.IsNullOrEmpty(liveUrl))
                    {
                        oLicensedLinkResponse.status = eLicensedLinkStatus.OK.ToString();
                        oLicensedLinkResponse.Status.Code = (int)ApiObjects.Response.eResponseStatus.OK;
                        oLicensedLinkResponse.mainUrl = liveUrl;
                    }
                }
                return oLicensedLinkResponse;
            }

            catch (Exception ex)
            {
                StringBuilder sb = new StringBuilder("Exception at GetEPGLink. ");
                sb.Append(String.Concat(" Msg: ", ex.Message));
                sb.Append(String.Concat(" Program ID: ", sProgramId));
                sb.Append(String.Concat(" Site Guid: ", sSiteGUID));
                sb.Append(String.Concat(" Media File ID: ", nMediaFileID));
                sb.Append(String.Concat(" Start time: ", dStartTime.ToString()));
                sb.Append(String.Concat(" User IP: ", sUserIP));
                sb.Append(String.Concat(" Coupon: ", sCouponCode));
                sb.Append(String.Concat(" Format: ", format.ToString()));
                sb.Append(String.Concat(" Trace: ", ex.StackTrace));
                log.Error("LicensedLink - " + sb.ToString(), ex);
            }
            finally
            {
                if (api != null)
                {
                    api.Dispose();
                }
            }

            return oLicensedLinkResponse;
        }
    }

}
