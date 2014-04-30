using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Text;
using com.llnw.mediavault;
using ConditionalAccess.TvinciAPI;

namespace ConditionalAccess
{
    class EutelsatConditionalAccess : TvinciConditionalAccess
    {
        private const int               LEFT_MARGIN = 3;
        private const int               RIGHT_MARGIN = 8;
        private static readonly string  UNREACHABLE_ERROR = "Unable to connect to the billing server";

        public EutelsatConditionalAccess(Int32 nGroupID)
            : base(nGroupID)
        {
        }

        public EutelsatConditionalAccess(Int32 nGroupID, string connKey)
            : base(nGroupID, connKey)
        {
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
                ret = ChargeUserForMediaFile(sSiteGUID, dPrice, sCurrency, nMediaFileID, nMediaID, sPPVModuleCode, sCouponCode, sUserIP, sExtraParameters, sCountryCd, sLANGUAGE_CODE, sDeviceUDID);
            }
            catch (Exception)
            {
                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                ret.m_sRecieptCode = "";
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
            //bool bDummy = true;

            #region User and Household validations

            TvinciBilling.BillingResponse ret = new ConditionalAccess.TvinciBilling.BillingResponse();
            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnown;
            ret.m_sRecieptCode = "";
            ret.m_sStatusDescription = "";


            if (string.IsNullOrEmpty(sSiteGUID))
            {
                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                ret.m_sRecieptCode = "";
                ret.m_sStatusDescription = "Cannot charge an unknown user";

                return ret;
            }

            // Get Household ID (UID) 
            string sHouseholdUID = DAL.DomainDal.GetDomainCoGuid(0, sSiteGUID);
            if (string.IsNullOrEmpty(sHouseholdUID))
            {
                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                ret.m_sRecieptCode = "";
                ret.m_sStatusDescription = "Cannot charge an unknown household";

                return ret;
            }


            // Check user validity

            TvinciUsers.UsersService u = new ConditionalAccess.TvinciUsers.UsersService();
            string sIP = "1.1.1.1";
            string sWSUserName = "";
            string sWSPass = "";
            TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetUserData", "users", sIP, ref sWSUserName, ref sWSPass);
            string sWSURL = Utils.GetWSURL("users_ws");
            if (sWSURL != "")
            {
                u.Url = sWSURL;
            }

            ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID);
            if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
            {
                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                ret.m_sRecieptCode = "";
                ret.m_sStatusDescription = "Cant charge an unknown user";

                return ret;
            }

            u.Dispose();


            int nDomainID = uObj.m_user.m_domianID;

            // Domain Description is used as Arvato Contract ID
            string sArvatoContractID = DAL.DomainDal.GetDomainDesc(m_nGroupID, nDomainID);

            #endregion


            // Check coupon validity
            if (!string.IsNullOrEmpty(sCouponCode) && !Utils.IsCouponValid(m_nGroupID, sCouponCode))
            {
                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                ret.m_sRecieptCode = "";
                ret.m_sStatusDescription = "Coupon not valid";
                try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription); }
                catch { }
                return ret;
            }


            #region Validate Media File PPV Module

            sIP = "1.1.1.1";
            sWSUserName = "";
            sWSPass = "";

            TvinciPricing.mdoule m = new global::ConditionalAccess.TvinciPricing.mdoule();
            sWSURL = Utils.GetWSURL("pricing_ws");
            if (sWSURL != "")
            {
                m.Url = sWSURL;
            }

            int[] nMediaFiles = { nMediaFileID };
            string sMediaFileForCache = Utils.ConvertArrayIntToStr(nMediaFiles);
            TvinciPricing.MediaFilePPVModule[] oModules = null;
            string sLocaleForCache = Utils.GetLocaleStringForCache(sCountryCd, sLANGUAGE_CODE, sDeviceUDID);
            if (CachingManager.CachingManager.Exist("GetPPVModuleListForMediaFiles" + sMediaFileForCache + "_" + m_nGroupID.ToString() + sLocaleForCache) == true)
            {
                oModules = (TvinciPricing.MediaFilePPVModule[])(CachingManager.CachingManager.GetCachedData("GetPPVModuleListForMediaFiles" + sMediaFileForCache + "_" + m_nGroupID.ToString() + sLocaleForCache));
            }
            else
            {
                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPPVModuleListForMediaFiles", "pricing", sIP, ref sWSUserName, ref sWSPass);
                oModules = m.GetPPVModuleListForMediaFiles(sWSUserName, sWSPass, nMediaFiles, sCountryCd, sLANGUAGE_CODE, sDeviceUDID);
                CachingManager.CachingManager.SetCachedData("GetPPVModuleListForMediaFiles" + sMediaFileForCache + "_" + m_nGroupID.ToString() + sLocaleForCache, oModules, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
            }

            m.Dispose();


            int nCount = 0;
            if (oModules[0].m_oPPVModules != null)
            {
                nCount = oModules[0].m_oPPVModules.Length;
            }

            bool bOK = false;
            string sPPVModuleVirtualName = string.Empty;

            var ppvModule = oModules[0].m_oPPVModules.First(pm =>
                                        ((string.Compare(pm.m_sObjectCode, sPPVModuleCode, true) == 0) || (string.Compare(pm.m_sObjectVirtualName, sPPVModuleCode, true) == 0)));

            if (ppvModule != null) // && ppvModules.Count() > 0)
            {
                bOK = true;
                sPPVModuleCode = ppvModule.m_sObjectCode;
                sPPVModuleVirtualName = ppvModule.m_sObjectVirtualName;
            }


            if (bOK == false)
            {
                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownPPVModule;
                ret.m_sRecieptCode = "";
                ret.m_sStatusDescription = "This PPV Module is not associated with the item";
                try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription); }
                catch { }

                return ret;
            }

            #endregion 



            // User and PPV Module OK, let's try to charge the user 

            if (bOK == true)
            {
                // Try retrieving Media's ContentID from CoGuid (= ContentID_FormatID)

                string sMediaFileCoGuid = Utils.GetMediaFileCoGuid(m_nGroupID, nMediaFileID);

                if (string.IsNullOrEmpty(sMediaFileCoGuid))
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                    ret.m_sRecieptCode = "";
                    ret.m_sStatusDescription = "The external identifier of the item is empty";
                    try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription); }
                    catch { }

                    return ret;
                }

                PriceReason theReason = PriceReason.UnKnown;

                TvinciPricing.Subscription relevantSub = null;
                TvinciPricing.Collection   relevantCol = null;
                TvinciPricing.PrePaidModule relevantPP = null;

                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPPVModuleData", "pricing", sIP, ref sWSUserName, ref sWSPass);
                TvinciPricing.PPVModule thePPVModule = m.GetPPVModuleData(sWSUserName, sWSPass, sPPVModuleCode, sCountryCd, sLANGUAGE_CODE, sDeviceUDID);

                if (thePPVModule != null)
                {

                    TvinciPricing.Price p =
                        Utils.GetMediaFileFinalPrice(nMediaFileID, thePPVModule, sSiteGUID, sCouponCode, m_nGroupID, ref theReason, ref relevantSub, ref relevantCol, ref relevantPP, sCountryCd, sLANGUAGE_CODE, sDeviceUDID);

                    if (theReason == PriceReason.ForPurchase || (theReason == PriceReason.SubscriptionPurchased && p.m_dPrice > 0)) // || bDummy)
                    {
                        if (p.m_dPrice != dPrice || p.m_oCurrency.m_sCurrencyCD3 != sCurrency)
                        {
                            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.PriceNotCorrect;
                            ret.m_sRecieptCode = "";
                            ret.m_sStatusDescription = "The price of the request is not the actual price";
                            try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription); }
                            catch { }

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

                            EutelsatTransactionResponse externalEntitledResponse =
                                IsUserTvodAllowed(sHouseholdUID, sSiteGUID, sArvatoContractID, dPrice, sCurrency, sMediaFileCoGuid);
                                //IsUserTvodAllowed(sHouseholdUID, sSiteGUID, sArvatoContractID, dPrice, sCurrency, nMediaExternalFileID);

                            if (externalEntitledResponse.Success == false)
                            {
                                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                ret.m_sRecieptCode = "";
                                ret.m_sStatusDescription =
                                    "Error " + externalEntitledResponse.ErrorCode + ">" + externalEntitledResponse.ErrorMessage; // "User is not entitled for VOD purchasing";

                                try
                                {
                                    WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() +
                                                              " error returned: " + ret.m_sStatusDescription);
                                }
                                catch { }
                                return ret;
                            }

                            ret.m_sRecieptCode = externalEntitledResponse.TransactionId;
                        }


                        string sCustomData = "";
                        if (p.m_dPrice != 0)
                        {
                            TvinciBilling.module bm = new ConditionalAccess.TvinciBilling.module();
                            sWSUserName = "";
                            sWSPass = "";
                            TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "CC_ChargeUser", "billing", sIP, ref sWSUserName, ref sWSPass);
                            sWSURL = Utils.GetWSURL("billing_ws");
                            if (sWSURL != "")
                            {
                                bm.Url = sWSURL;
                            }
                            //string sCustomData = "<customdata><user=\"" + sSiteGUID + "\"/><ppvmodule>" + sPPVModuleCode + "</ppvmodule><media_file>" + nMediaFileID.ToString() + "</media_file><payment num=\"1\" outof=\"1\"/><price>" + dPrice.ToString() + "</price><currency>" + sCurrency + "</currency><coupon>" + sCouponCode + "</coupon></customdata>";

                            if (string.IsNullOrEmpty(sCountryCd) && !string.IsNullOrEmpty(sUserIP))
                            {
                                sCountryCd = TVinciShared.WS_Utils.GetIP2CountryCode(sUserIP);
                            }

                            //Create the Custom Data
                            sCustomData = GetCustomData(relevantSub, thePPVModule, null, sSiteGUID, dPrice, sCurrency, nMediaFileID, nMediaID, sPPVModuleCode, string.Empty, sCouponCode, sUserIP,
                                                        sCountryCd, sLANGUAGE_CODE, sDeviceUDID);
                            Logger.Logger.Log("CustomData", sCustomData, "CustomData");


                            ret = bm.CC_DummyChargeUser(sWSUserName, sWSPass, sSiteGUID, dPrice, sCurrency, sUserIP, sCustomData, 1, 1, sExtraParameters);

                            bm.Dispose();

                            //ret = bm.CC_ChargeUser(sWSUserName, sWSPass, sSiteGUID, dPrice, sCurrency, sUserIP, sCustomData, 1, 1, sExtraParameters);

                            //customdata id
                            //if (bDummy == false)
                            //{
                            //    ret = bm.CC_ChargeUser(sWSUserName, sWSPass, sSiteGUID, dPrice, sCurrency, sUserIP, sCustomData, 1, 1, sExtraParameters);
                            //}
                            //else
                            //{
                            //    ret = bm.CC_DummyChargeUser(sWSUserName, sWSPass, sSiteGUID, dPrice, sCurrency, sUserIP, sCustomData, 1, 1, sExtraParameters);
                            //}
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
                            bool dbInsertPpvPurchase = DAL.ConditionalAccessDAL.InsertPPVPurchase(m_nGroupID, subCode, nMediaFileID, sSiteGUID, dPrice, sCurrency, 0, sCustomData, transactionID, sCountryCd, sLANGUAGE_CODE, sDeviceUDID, maxNumOfUses, 1, 1, dtEndDate);

                            try { WriteToUserLog(sSiteGUID, "Media file id: " + nMediaFileID.ToString() + " Purchased(CC): " + dPrice.ToString() + sCurrency); }
                            catch { }


                            //int nPurchaseID = 0;
                            int nPurchaseID = DAL.ConditionalAccessDAL.GetPPVPurchaseID(m_nGroupID, subCode, nMediaFileID, sSiteGUID, dPrice, sCurrency, 0, maxNumOfUses, 1, 1);

                            //Should update the PURCHASE_ID
                            string sReceipt = ret.m_sRecieptCode;
                            if (!string.IsNullOrEmpty(sReceipt))
                            {
                                bool updatedPurchaseId = DAL.ConditionalAccessDAL.UpdatePurchaseID(transactionID, nPurchaseID);
                            }
                            else
                            {
                                try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription); }
                                catch { }
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
                                    EutelsatTransactionResponse transNotificationRes =
                                        MakeTransactionNotification(sHouseholdUID, dPrice, sCurrency, nMediaFileID, sMediaFileCoGuid, sPPVModuleVirtualName, sCouponCode, sDeviceUDID, transactionID);

                                    if (transNotificationRes.Success == false)
                                    {
                                        bool canceledTransaction = DAL.ConditionalAccessDAL.CancelTransaction(transactionID);
                                        bool canceled = canceledTransaction && DAL.ConditionalAccessDAL.CancelPpvPurchase(m_nGroupID, nPurchaseID, sSiteGUID, nMediaFileID, 0, 2);

                                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                                        ret.m_sRecieptCode = sReceipt;
                                        ret.m_sStatusDescription = "Error " + transNotificationRes.ErrorCode + ">" + transNotificationRes.ErrorMessage; //"User is not entitled for VOD purchasing";

                                        WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription);
                                    }
                                }
                                catch { }
                            }

                            //return ret;   
                        }
                        else
                        {
                            try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription); }
                            catch { }
                        }
                    }
                    else
                    {
                        ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                        ret.m_sRecieptCode = "";

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

                        try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription); }
                        catch { }

                    }
                }
                else
                {
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                    ret.m_sRecieptCode = "";
                    ret.m_sStatusDescription = "The ppv module is unknown";
                    
                    try { WriteToUserLog(sSiteGUID, "While trying to purchase media file id(CC): " + nMediaFileID.ToString() + " error returned: " + ret.m_sStatusDescription); }
                    catch { }
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
                //object 3ssRes = Utils.MakeJsonRequest(checkTvodUrl, 
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

                //string serviceBaseURL = ConditionalAccess.Properties.Settings.Default.ConditionalAccess_Eutelsat_Transaction_Service;

                if (string.IsNullOrEmpty(sHouseholdUID) || string.IsNullOrEmpty(sWSURL))
                {
                    return res;
                }

                int nDeviceBrandID = 0;
                int nDeviceFamilyID = DAL.DeviceDal.GetDeviceFamilyID(m_nGroupID, sDeviceUDID, ref nDeviceBrandID);

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

                //string jsonTransactionContent = trans.Serialize();
                var jsonTransactionContent = Newtonsoft.Json.JsonConvert.SerializeObject(transRequest);

                //http://82.79.128.235:8080/TvinciService.svc/user/check_tvod.json?user_id={userId}&price={price}&currency={currency}&asset_id={assetId}

                //string requestURL = MakeTransNotificationURL(sWSURL, sHouseholdUID, dPrice, sCurrency, nExternalAssetID, sPpvModuleCode, sCouponCode, nRoviID, nTransactionID, nDeviceBrandID);
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

                //string serviceBaseURL = ConditionalAccess.Properties.Settings.Default.ConditionalAccess_Eutelsat_Transaction_Service;

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

                //string jsonTransactionContent = trans.Serialize();
                var jsonTransactionContent = Newtonsoft.Json.JsonConvert.SerializeObject(subRequest);

                //http://82.79.128.235:8080/TvinciService.svc/user/check_tvod.json?user_id={userId}&price={price}&currency={currency}&asset_id={assetId}

                //string requestURL = MakeTransNotificationURL(sWSURL, sHouseholdUID, dPrice, sCurrency, nExternalAssetID, sPpvModuleCode, sCouponCode, nRoviID, nTransactionID, nDeviceBrandID);
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
            catch (Exception e)
            {
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

            #region User and household validation

            if (string.IsNullOrEmpty(sSiteGUID))
            {
                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                ret.m_sRecieptCode = "";
                ret.m_sStatusDescription = "Cant charge an unknown user";
                
                return ret;
            }            
            
            // Get Household ID (UID) 
            string sHouseholdUID = DAL.DomainDal.GetDomainCoGuid(0, sSiteGUID);
            if (string.IsNullOrEmpty(sHouseholdUID))
            {
                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                ret.m_sRecieptCode = "";
                ret.m_sStatusDescription = "Cannot charge an unknown household";

                return ret;
            }

            TvinciUsers.UsersService u = new ConditionalAccess.TvinciUsers.UsersService();
            string sIP = "1.1.1.1";
            string sWSUserName = "";
            string sWSPass = "";
            TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetUserData", "users", sIP, ref sWSUserName, ref sWSPass);
            string sWSURL = Utils.GetWSURL("users_ws");
            if (sWSURL != "")
            {
                u.Url = sWSURL;
            }

            ConditionalAccess.TvinciUsers.UserResponseObject uObj = u.GetUserData(sWSUserName, sWSPass, sSiteGUID);
            if (uObj.m_RespStatus != ConditionalAccess.TvinciUsers.ResponseStatus.OK)
            {
                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.UnKnownUser;
                ret.m_sRecieptCode = "";
                ret.m_sStatusDescription = "Cant charge an unknown user";

                return ret;
            }

            #endregion

               
            if (Utils.IsCouponValid(m_nGroupID, sCouponCode) == false)
            {
                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                ret.m_sRecieptCode = "";
                ret.m_sStatusDescription = "Coupon not valid";
                try { WriteToUserLog(sSiteGUID, "While trying to purchase subscription(CC): " + sSubscriptionCode + " error returned: " + ret.m_sStatusDescription); }
                catch { }
                return ret;
            }

            PriceReason theReason = PriceReason.UnKnown;
            TvinciPricing.Subscription theSub = null;
            TvinciPricing.Price p = Utils.GetSubscriptionFinalPrice(m_nGroupID, sSubscriptionCode, sSiteGUID, sCouponCode, ref theReason, ref theSub, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                    
            if (p != null)
            {
                dPrice = p.m_dPrice;
                sCurrency = p.m_oCurrency.m_sCurrencyCD3;
            }

            switch (theReason)
            {
                case PriceReason.Free:
                    
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                    ret.m_sRecieptCode = "";
                    ret.m_sStatusDescription = "The subscription is free";
                    try { WriteToUserLog(sSiteGUID, "while trying to purchase subscription(CC): " + " error returned: " + ret.m_sStatusDescription); }
                    catch { }
                    
                    break;

                case PriceReason.SubscriptionPurchased:
                    
                    ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                    ret.m_sRecieptCode = "";
                    ret.m_sStatusDescription = "The subscription is already purchased";
                    try { WriteToUserLog(sSiteGUID, "while trying to purchase subscription(CC): " + " error returned: " + ret.m_sStatusDescription); }
                    catch { }

                    break;

                case PriceReason.ForPurchase:

                    ret = HandleSubPurchase(sSiteGUID, sHouseholdUID, dPrice, sCurrency, sSubscriptionCode, sCouponCode, sUserIP, sExtraParams, ref sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, sIP, theSub, p);
                    
                    break;

                default:
                    break;
            }                
            
            return ret;
        }

        private TvinciBilling.BillingResponse HandleSubPurchase(string sSiteGUID, string sHouseholdUID, double dPrice, string sCurrency, string sSubscriptionCode, string sCouponCode, string sUserIP, 
                                                            string sExtraParams, ref string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string sIP, TvinciPricing.Subscription theSub, 
                                                            TvinciPricing.Price p)
        {

            TvinciBilling.BillingResponse ret = new TvinciBilling.BillingResponse();

            if ((p == null) || (dPrice != p.m_dPrice) || (sCurrency != p.m_oCurrency.m_sCurrencyCD3))
            {
                ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.PriceNotCorrect;
                ret.m_sRecieptCode = "";
                ret.m_sStatusDescription = "The price of the request is not the actual price";

                try { WriteToUserLog(sSiteGUID, "while trying to purchase subscription(CC): " + " error returned: " + ret.m_sStatusDescription); }
                catch { }

                return ret;
            }

            
            string sCustomData = "";

            bool bIsRecurring = theSub.m_bIsRecurring;
            Int32 nRecPeriods = theSub.m_nNumberOfRecPeriods;

            if (string.IsNullOrEmpty(sCountryCd) && !string.IsNullOrEmpty(sUserIP))
            {
                sCountryCd = TVinciShared.WS_Utils.GetIP2CountryCode(sUserIP);
            }

            //Create the Custom Data
            sCustomData = GetCustomDataForSubscription(theSub, null, sSubscriptionCode, string.Empty, sSiteGUID, dPrice, sCurrency, sCouponCode, sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

            Logger.Logger.Log("CustomData", sCustomData, "CustomDataForSubsrpition");

            if (p.m_dPrice != 0)
            {
                TvinciBilling.module bm = new ConditionalAccess.TvinciBilling.module();
                string sWSUserName = "";
                string sWSPass = "";
                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "CC_ChargeUser", "billing", sIP, ref sWSUserName, ref sWSPass);
                string sWSURL = Utils.GetWSURL("billing_ws");

                if (sWSURL != "")
                {
                    bm.Url = sWSURL;
                }

                //customdata id
                //if (bDummy == false)
                //    ret = bm.CC_ChargeUser(sWSUserName, sWSPass, sSiteGUID, dPrice, sCurrency, sUserIP, sCustomData, 1, nRecPeriods, sExtraParams);
                //else

                ret = bm.CC_DummyChargeUser(sWSUserName, sWSPass, sSiteGUID, dPrice, sCurrency, sUserIP, sCustomData, 1, nRecPeriods, sExtraParams);

            }

            if (p.m_dPrice == 0 && !string.IsNullOrEmpty(sCouponCode))
            {
                ret.m_oStatus = TvinciBilling.BillingResponseStatus.Success;
            }

            if (ret.m_oStatus == ConditionalAccess.TvinciBilling.BillingResponseStatus.Success)
            {
                HandleCouponUses(theSub, string.Empty, sSiteGUID, dPrice, sCurrency, 0, sCouponCode, sUserIP, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME, true, 0, 0);

                int nIsRecurring = 0;
                int nIsRecurringStatus = (bIsRecurring == true) ? 1 : 0;

                bool dbRes = DAL.ConditionalAccessDAL.UpdateSubPurchase(m_nGroupID, sSiteGUID, sSubscriptionCode, nIsRecurring);

                int numOfUses = 0;
                int nMaxNumberOfViews = (theSub != null && theSub.m_oUsageModule != null) ? theSub.m_oUsageModule.m_nMaxNumberOfViews : 0;
                int nViewLifeCycleSec = (theSub != null && theSub.m_oUsageModule != null) ? theSub.m_oUsageModule.m_tsViewLifeCycle : 0;
                
                int nReceiptCode = (!string.IsNullOrEmpty(ret.m_sRecieptCode)) ? int.Parse(ret.m_sRecieptCode) : 0;
                int nIsActive = 1;
                int nStatus = 1;

                DateTime? dtEndDate = null;
                if (theSub != null && theSub.m_oSubscriptionUsageModule != null)
                {
                    dtEndDate = Utils.GetEndDateTime(DateTime.UtcNow, theSub.m_oSubscriptionUsageModule.m_tsMaxUsageModuleLifeCycle);
                }


                dbRes = DAL.ConditionalAccessDAL.InsertSubPurchase(m_nGroupID, sSubscriptionCode, sSiteGUID, dPrice, sCurrency, sCustomData, numOfUses, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME,
                                                                nMaxNumberOfViews, nViewLifeCycleSec, nIsRecurringStatus, nReceiptCode, nIsActive, nStatus, dtEndDate);

                try { WriteToUserLog(sSiteGUID, "Subscription purchase (CC): " + sSubscriptionCode); }
                catch { }


                int nPurchaseID = DAL.ConditionalAccessDAL.GetSubPurchaseID(m_nGroupID, sSubscriptionCode, sSiteGUID, dPrice, sCurrency, numOfUses, nMaxNumberOfViews, nViewLifeCycleSec,
                                                                            nIsRecurringStatus, nIsActive, nStatus);

                if (nReceiptCode > 0)
                {
                    dbRes = DAL.ConditionalAccessDAL.UpdatePurchaseID(nReceiptCode, nPurchaseID);
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

                        if (subNotificationRes.Success == false)
                        {
                            if (nReceiptCode > 0)
                            {
                                bool canceledTransaction = DAL.ConditionalAccessDAL.CancelTransaction(nReceiptCode);
                                bool canceled = canceledTransaction && DAL.ConditionalAccessDAL.CancelSubPurchase(m_nGroupID, sSubscriptionCode, nPurchaseID, sSiteGUID);
                            }

                            ret.m_oStatus = ConditionalAccess.TvinciBilling.BillingResponseStatus.Fail;
                            ret.m_sRecieptCode = nReceiptCode.ToString();
                            ret.m_sStatusDescription = "Error " + subNotificationRes.ErrorCode + ">" + subNotificationRes.ErrorMessage; //"User is not entitled for VOD purchasing";

                            WriteToUserLog(sSiteGUID, "While trying to purchase product id(CC): " + sSubscriptionCode + " error returned: " + ret.m_sStatusDescription);
                        }
                    }
                    catch { }
                }

                //#region Send purchase mail
                //try
                //{

                //}
                //catch (Exception ex)
                //{
                //    Logger.Logger.Log("Send purchase mail", ex.Message + " | " + ex.StackTrace, "mailer");
                //}

                //#endregion


            }
            else
            {
                try { WriteToUserLog(sSiteGUID, "while trying to purchase subscription(CC): " + sSubscriptionCode + " error returned: " + ret.m_sStatusDescription); }
                catch { }
            }

            return ret;
        }

        /// <summary>
        /// Cancel Subscription
        /// </summary>
        //public override bool CancelSubscription(string sSiteGUID, string sSubscriptionCode, int nSubscriptionPurchaseID)
        //{
        //    bool bRet = false;
        //    PriceReason theReason = PriceReason.UnKnown;
        //    TvinciPricing.Subscription theSub = null;
        //    TvinciPricing.Price p = Utils.GetSubscriptionFinalPrice(m_nGroupID, sSubscriptionCode, sSiteGUID, string.Empty, ref theReason, ref theSub, "", "", "");

        //    bool bIsRecurring = false;
        //    if (theSub != null && theSub.m_oUsageModule != null)
        //        bIsRecurring = theSub.m_bIsRecurring;

        //    try
        //    {
        //        List<int> lSubPurchasesIDs = DAL.ConditionalAccessDAL.GetSubscriptionPurchaseIDs(m_nGroupID, sSiteGUID, sSubscriptionCode, nSubscriptionPurchaseID).ToList();
                
        //        if (lSubPurchasesIDs == null || lSubPurchasesIDs.Count() == 0 || !lSubPurchasesIDs.Contains(nSubscriptionPurchaseID))
        //        {
        //            return false;
        //        }

        //        for (int i = 0; i < lSubPurchasesIDs.Count(); i++)
        //        {
        //            int nIsRecurring = 0;
        //            bRet = DAL.ConditionalAccessDAL.UpdateSubPurchase(m_nGroupID, sSiteGUID, sSubscriptionCode, nIsRecurring, lSubPurchasesIDs[i]);
        //        }

        //        if (!bRet)
        //        {
        //            return false;
        //        }

        //        int nIsActive = 1;
        //        int nStatus = 1;
        //        int nNewRenewableStatus = 0;
        //        bRet = DAL.ConditionalAccessDAL.InsertSubStatusChange(m_nGroupID, sSubscriptionCode, sSiteGUID, nIsActive, nStatus, nNewRenewableStatus, "", "", "");

        //        try { WriteToUserLog(sSiteGUID, "Subscription: " + sSubscriptionCode + " cancelled"); }
        //        catch (Exception ex)
        //        {
        //            WriteToUserLog(sSiteGUID, "while trying to cancel subscription(CC): " + sSubscriptionCode + " error returned: " + ex.Message);
        //        }                    
                
        //    }
        //    catch (Exception ex)
        //    {
        //        WriteToUserLog(sSiteGUID, "while trying to cancel subscription(CC): " + sSubscriptionCode + " error returned: " + ex.Message);
        //    }
            
        //    return bRet;
        //}

        public override string GetEPGLink(int nProgramId, DateTime startTime, eEPGFormatType format, string sSiteGUID, Int32 nMediaFileID, string sBasicLink, string sUserIP, string sRefferer, string sCOUNTRY_CODE, string sLANGUAGE_CODE, string sDEVICE_NAME, string sCouponCode)
        {
            string url = string.Empty;

            // Validate inputs
            if ((nProgramId <= 0) ||
                (string.IsNullOrEmpty(sBasicLink)) ||
                (string.IsNullOrEmpty(sSiteGUID)))
            {
                return string.Empty;
            }


            try
            {
                string sIP = "1.1.1.1";
                string sWSUserName = "";
                string sWSPass = "";

                string host = (new Uri(sBasicLink)).Host;
                if (string.IsNullOrEmpty(host))
                {
                    return string.Empty;
                }


                string sBaseLink = GetLicensedLink(sSiteGUID, nMediaFileID, sBasicLink, sUserIP, sRefferer, sCOUNTRY_CODE, sLANGUAGE_CODE, sDEVICE_NAME, sCouponCode);
                Logger.Logger.Log("LicensedLink", "Finished base link", "LicensedLink");

                if (string.IsNullOrEmpty(sBaseLink))
                {
                    return string.Empty;
                }

                string sRightMargin = Utils.GetValueFromConfig("right_margin");
                string sLeftMargin = Utils.GetValueFromConfig("left_margin");

                int nRightMargin = !string.IsNullOrEmpty(sRightMargin) ? int.Parse(sRightMargin) : RIGHT_MARGIN;
                int nLeftMargin = !string.IsNullOrEmpty(sLeftMargin) ? int.Parse(sLeftMargin) : LEFT_MARGIN;


                Dictionary<string, object> parametersToInjectInUrl = new Dictionary<string, object>();

                if (!string.IsNullOrEmpty(host))
                {
                    parametersToInjectInUrl.Add("host", host);
                }

                string sChannelName = string.Empty;

                TvinciAPI.API api = new TvinciAPI.API();
                string sApiWSUrl = Utils.GetWSURL("api_ws");

                if (!string.IsNullOrEmpty(sApiWSUrl))
                    api.Url = sApiWSUrl;

                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetEPGLink", "api", sIP, ref sWSUserName, ref sWSPass);

                Logger.Logger.Log("LicensedLink", "Started getting coguid for media file id", "LicensedLink");

                sChannelName = api.GetCoGuidByMediaFileId(sWSUserName, sWSPass, nMediaFileID);

                Logger.Logger.Log("LicensedLink", "Finished getting coguid for media file id", "LicensedLink");

                if (!string.IsNullOrEmpty(sChannelName))
                {
                    parametersToInjectInUrl.Add("name", sChannelName);

                    Logger.Logger.Log("LicensedLink", "Getting stream type", "LicensedLink");

                    eStreamType streamType = Utils.GetStreamType(sBaseLink);

                    Logger.Logger.Log("LicensedLink", "Finished getting stream type", "LicensedLink");

                    url = Utils.GetStreamTypeAndFormatLink(streamType, format); // Getting the url which matches both the epg format and the stream type

                    long nStartTime;
                    long nEndTime;

                    // Time Factor for aligment with Harmonic server (e.g. convert millisec -> 10Xmicrosec)
                    string sTimeMultFactor = Utils.GetValueFromConfig("time_mult_factor");
                    int timeMultFactor = 10000;

                    if (!string.IsNullOrEmpty(sTimeMultFactor))
                    {
                        int.TryParse(sTimeMultFactor, out timeMultFactor);
                    }

                    Scheduling scheduling = api.GetProgramSchedule(sWSUserName, sWSPass, nProgramId);

                    switch (format)
                    {
                        case eEPGFormatType.Catchup:
                        case eEPGFormatType.StartOver:
                            {
                                if (scheduling != null)
                                {
                                    nStartTime = (timeMultFactor * Utils.ConvertDateToEpochTimeInMilliseconds(scheduling.StartDate.ToUniversalTime().AddMinutes(nLeftMargin)));
                                    nEndTime = (timeMultFactor * Utils.ConvertDateToEpochTimeInMilliseconds(scheduling.EndTime.ToUniversalTime().AddMinutes(nRightMargin)));

                                    parametersToInjectInUrl.Add("start", nStartTime);
                                    parametersToInjectInUrl.Add("end", nEndTime);
                                }
                            }

                            break;

                        case eEPGFormatType.LivePause:

                            DateTime startTimeUTC = startTime.ToUniversalTime();
                            if (DateTime.Compare(startTimeUTC, DateTime.UtcNow) <= 0)
                            {
                                nStartTime = (timeMultFactor * Utils.ConvertDateToEpochTimeInMilliseconds(startTimeUTC.AddMinutes(nLeftMargin)));
                                nEndTime = (timeMultFactor * Utils.ConvertDateToEpochTimeInMilliseconds(scheduling.EndTime.AddMinutes(nRightMargin)));
                                parametersToInjectInUrl.Add("start", nStartTime);
                                parametersToInjectInUrl.Add("end", nEndTime);
                            }

                            break;

                        default:
                            url = string.Empty;
                            break;
                    }

                    // Injecting the parameters values to the url
                    if (!string.IsNullOrEmpty(url))
                    {
                        Utils.ReplaceSubStr(ref url, parametersToInjectInUrl);
                    }
                }

            }
            catch (Exception ex)
            {
                Logger.Logger.Log("LicensedLink", "ERROR: Failed getting epg license linked - " + ex.Message, "LicensedLink");
            }

            return url;
        }


        /// <summary>
        /// Get Items Prices
        /// </summary>
        public override MediaFileItemPricesContainer[] GetItemsPrices(Int32[] nMediaFiles, string sUserGUID, string sCouponCode, bool bOnlyLowest, string sCountryCd, string sLANGUAGE_CODE, string sDEVICE_NAME, string sClientIP)
        {
            string sIP = "1.1.1.1";
            string sWSUserName = "";
            string sWSPass = "";

            MediaFileItemPricesContainer[] ret = null;

            TvinciPricing.mdoule m = new global::ConditionalAccess.TvinciPricing.mdoule();
            string sWSURL = Utils.GetWSURL("pricing_ws");
            if (sWSURL != "")
                m.Url = sWSURL;

            string nMediasForCache = Utils.ConvertArrayIntToStr(nMediaFiles);
            
            TvinciPricing.MediaFilePPVModule[] oModules = null;
            string sLocaleForCache = Utils.GetLocaleStringForCache(sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

            if (CachingManager.CachingManager.Exist("GetPPVModuleListForMediaFiles" + nMediasForCache + "_" + m_nGroupID.ToString() + sLocaleForCache) == true)
            {
                oModules = TVinciShared.ObjectCopier.Clone<TvinciPricing.MediaFilePPVModule[]>((TvinciPricing.MediaFilePPVModule[])(CachingManager.CachingManager.GetCachedData("GetPPVModuleListForMediaFiles" + nMediasForCache + "_" + m_nGroupID.ToString() + sLocaleForCache)));
            }
            else
            {
                TVinciShared.WS_Utils.GetWSUNPass(m_nGroupID, "GetPPVModuleListForMediaFiles", "pricing", sIP, ref sWSUserName, ref sWSPass);
                oModules = m.GetPPVModuleListForMediaFiles(sWSUserName, sWSPass, nMediaFiles, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);
                TvinciPricing.MediaFilePPVModule[] oModulesCopy = TVinciShared.ObjectCopier.Clone<TvinciPricing.MediaFilePPVModule[]>(oModules);
                CachingManager.CachingManager.SetCachedData("GetPPVModuleListForMediaFiles" + nMediasForCache + "_" + m_nGroupID.ToString() + sLocaleForCache, oModulesCopy, 86400, System.Web.Caching.CacheItemPriority.Default, 0, false);
            }

            Int32 nCount = 0;
            if (oModules != null)
            {
                nCount = oModules.Length;
            }

            if (nCount > 0)
            {
                ret = new MediaFileItemPricesContainer[nCount];
            }
            else
            {
                ret = new MediaFileItemPricesContainer[1];
                MediaFileItemPricesContainer mc = new MediaFileItemPricesContainer();
             
                foreach (int mediaFileID in nMediaFiles)
                {
                    ItemPriceContainer freeContainer = new ItemPriceContainer();
                    freeContainer.m_PriceReason = PriceReason.Free;
                    ItemPriceContainer[] priceContainer = new ItemPriceContainer[1];
                    priceContainer[0] = freeContainer;

                    mc.Initialize(mediaFileID, priceContainer);
                }

                ret[0] = mc;
            }

            for (int i = 0; i < nCount; i++)
            {

                Int32 nMediaFileID = oModules[i].m_nMediaFileID;
                TvinciPricing.PPVModule[] ppvModules = oModules[i].m_oPPVModules;
                MediaFileItemPricesContainer mf = new MediaFileItemPricesContainer();
                
                if (ppvModules != null)
                {
                    ItemPriceContainer[] itemPriceCont = null;
                    if (ppvModules.Length > 0)
                    {
                        itemPriceCont = new ItemPriceContainer[ppvModules.Length];
                    }

                    Int32 nLowestIndex = 0;
                    double dLowest = -1;
                    TvinciPricing.Price pLowest = null;
                    PriceReason theLowestReason = PriceReason.UnKnown;
                    TvinciPricing.Subscription relevantLowestSub = null;
                    TvinciPricing.Collection relevantLowestCol = null;
                    TvinciPricing.PrePaidModule relevantLowestPrePaid = null;
                    string sProductCode = string.Empty;
                    
                    for (int j = 0; j < ppvModules.Length; j++)
                    {
                        string sPPVCode = ppvModules[j].m_sObjectCode + "|" + ppvModules[j].m_sObjectVirtualName;
                        PriceReason theReason = PriceReason.UnKnown;
                        TvinciPricing.Subscription relevantSub = null;
                        TvinciPricing.Collection   relevantCol = null;
                        TvinciPricing.PrePaidModule relevantPrePaid = null;

                        TvinciPricing.Price p = Utils.GetMediaFileFinalPrice(nMediaFileID, ppvModules[j], sUserGUID, sCouponCode, m_nGroupID, ref theReason, ref relevantSub, ref relevantCol, ref relevantPrePaid, sCountryCd, sLANGUAGE_CODE, sDEVICE_NAME);

                        sProductCode = oModules[i].m_sProductCode;
                        
                        if (bOnlyLowest == false)
                        {
                            itemPriceCont[j] = new ItemPriceContainer();
                            itemPriceCont[j].Initialize(p, ppvModules[j].m_oPriceCode.m_oPrise, sPPVCode, ppvModules[j].m_sDescription, theReason, relevantSub, relevantCol, ppvModules[j].m_bSubscriptionOnly);
                        }
                        else
                        {
                            if (p.m_dPrice < dLowest || j == 0)
                            {
                                nLowestIndex = j;
                                dLowest = p.m_dPrice;
                                pLowest = p;
                                theLowestReason = theReason;
                                relevantLowestSub = relevantSub;
                                relevantLowestCol = relevantCol;
                                relevantLowestPrePaid = relevantPrePaid;
                            }
                        }
                    }
                    if (ppvModules.Length > 0 && bOnlyLowest == true)
                    {
                        itemPriceCont[0] = new ItemPriceContainer();
                        itemPriceCont[0].Initialize(pLowest, ppvModules[nLowestIndex].m_oPriceCode.m_oPrise, ppvModules[nLowestIndex].m_sObjectCode, ppvModules[nLowestIndex].m_sDescription, theLowestReason, relevantLowestSub, relevantLowestCol, ppvModules[nLowestIndex].m_bSubscriptionOnly);
                    }

                    mf.Initialize(nMediaFileID, itemPriceCont, sProductCode);
                }
                else
                {
                    //mf.Initialize(nMediaFileID, null);
                    //ret = new MediaFileItemPricesContainer[1];
                    MediaFileItemPricesContainer mc = new MediaFileItemPricesContainer();
                    foreach (int mediaFileID in nMediaFiles)
                    {
                        ItemPriceContainer freeContainer = new ItemPriceContainer();
                        freeContainer.m_PriceReason = PriceReason.Free;
                        freeContainer.m_oPrice = new TvinciPricing.Price();
                        freeContainer.m_oPrice.m_dPrice = 0.0;
                        ItemPriceContainer[] priceContainer = new ItemPriceContainer[1];
                        priceContainer[0] = freeContainer;

                        mf.Initialize(mediaFileID, priceContainer);
                    }
                    ret[0] = mc;
                }
                ret[i] = mf;
            }

            return ret;
        }

    }
}
