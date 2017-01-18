using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Security.Cryptography;
using System.Net;
using System.Configuration;
using APILogic.AdyenRecAPI;
using KLogMonitor;
using System.Reflection;
using System.Web;
using APILogic.AdyenPayAPI;
using ApiObjects.Billing;

namespace Core.Billing
{
    public class AdyenCreditCard : BaseCreditCard
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public AdyenCreditCard(int groupID)
            : base(groupID)
        {
        }


        public override BillingResponse ChargeUser(string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, int nPaymentNumber, int nNumberOfPayments, string sExtraParameters)//, int nPurchesType)
        {
            BillingResponse ret = new BillingResponse();
            try
            {
                string merchAcc = string.Empty;
                string merchPurchesAccount = string.Empty;
                string sUN = string.Empty;
                string sPass = string.Empty;

                log.Info("AdyenCreditCard ChargeUser start at " + DateTime.Now.ToString());
                AdyenUtils.GetWSCredentials(m_nGroupID, ref sUN, ref sPass, ref merchAcc, ref merchPurchesAccount, 2);
                RecurringDetailsResult recRes = GetAdyenContract(merchAcc, sSiteGUID, sUN, sPass);

                if (recRes.details == null || recRes.details.Length == 0)
                {
                    AdyenUtils.GetTvinciWSCredentials(ref sUN, ref sPass, ref merchAcc);
                    recRes = GetAdyenContract(merchAcc, sSiteGUID, sUN, sPass);
                }

                log.Info(string.Format("WSCredentials : GroupID={0}, sUN={1}, sPass={2},merchAcc={3},merchPurchesAccount={4},PurchesType={5}", m_nGroupID, sUN, sPass, merchAcc, merchPurchesAccount, 2));
                if (recRes.details != null && recRes.details.Length > 0)
                {
                    try
                    {
                        RecurringDetail det = null;
                        Core.Billing.AdyenUtils.BillingType bt = Core.Billing.AdyenUtils.BillingType.CreditCard;

                        det = AdyenUtils.GetRecurringDetailByLastFourDigits(sSiteGUID, recRes.details, bt, string.Empty);
                        int pm = (int)(ePaymentMethod.CreditCard);
                        if (!string.IsNullOrEmpty(det.variant.ToString()))
                        {
                            switch (det.variant.ToString().ToLower())
                            {
                                case "visa": pm = (int)(ePaymentMethod.Visa); break;
                                case "mc": pm = (int)(ePaymentMethod.MasterCard); break;
                            }

                        }

                        log.Info(string.Format("{0}: {1} bank={2},card={3},name={4},variant={5},recurringDetailReference={6},elv={7}", "ChargeAdyen with Params ", "RecurringDetail",
                                                        det.bank, det.card, det.name, det.variant, det.recurringDetailReference, det.elv));
                        log.Info(string.Format("sSiteGUID={0}, dChargePrice={1}, sCurrencyCode={2},sUN={3},sPass={4},sCustomData={5},merchPurchesAccount={6},nPaymentNumber={7},nNumberOfPayments={8}, pm={9},sExtraParameters={10}",
                            sSiteGUID, dChargePrice, sCurrencyCode, sUN, sPass, sCustomData, merchPurchesAccount, nPaymentNumber, nNumberOfPayments, pm, sExtraParameters));

                        ret = ChargeAdyen(det, sSiteGUID, dChargePrice, sCurrencyCode, sUN, sPass, sCustomData, merchPurchesAccount, nPaymentNumber, nNumberOfPayments, pm, sExtraParameters);
                        log.Info(string.Format("Billing Response ret : Status={0}, RecieptCode={1}, StatusDescription={2}", ret.m_oStatus, ret.m_sRecieptCode, ret.m_sStatusDescription));
                    }
                    catch (Exception ex)
                    {
                        ret.m_oStatus = BillingResponseStatus.Fail;
                        ret.m_sStatusDescription = "failed to charge user " + sSiteGUID;

                        log.Error(string.Format("Exception on renewal for user {0}, ex.Message {1}", sSiteGUID, ex.Message));
                    }
                }
            }
            catch (Exception ex)
            {
                ret.m_oStatus = BillingResponseStatus.Fail;
                ret.m_sStatusDescription = "No latest details found for user " + sSiteGUID;
                log.Error(string.Format("Exception on renewal for user {0}, ex.Message {1}", sSiteGUID, ex.Message));
            }
            return ret;
        }

        public override string GetClientCheckSum(string sUserIP, string sRandom)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Get Client Merchant Sig
        /// Expected order of parameters:
        /// "paymentAmount ; currencyCode; shipBeforeDate ; merchantReference ; skinCode ; merchantAccount ; sessionValidity ; shopperEmail ;shopperReference ;
        ///  recurringContract ; allowedMethods ; blockedMethods; shopperStatement ; merchantReturnData ; billingAddressType ; offset" 
        /// </summary>
        /// <param name="sParams">string parameter separate with ";" char</param>
        /// <returns>returns Base64 String using HMACSHA256 Cryptography by computing the hash value for the specified byte array.</returns>
        public override string GetClientMerchantSig(string sParams)
        {
            string retVal = string.Empty;
            string[] FIELDS_MERCHANT_SIG = { "paymentAmount", "currencyCode", "shipBeforeDate", "merchantReference", "skinCode", "merchantAccount", "sessionValidity", "shopperEmail",
                                            "shopperReference", "recurringContract", "allowedMethods", "blockedMethods", "shopperStatement", "merchantReturnData", "billingAddressType", "offset"};

            string[] FIELDS_MERCHANT_SIG_SHA256 = { "paymentAmount", "currencyCode", "shipBeforeDate", "merchantReference", "skinCode", "merchantAccount", "sessionValidity", "shopperEmail",
                                            "shopperReference", "recurringContract", "allowedMethods", "blockedMethods", "shopperStatement", "merchantReturnData", "billingAddressType", "offset",
                                            "brandCode", "shopperLocale", "orderData"};

            /// Expected order of parameters for SHA1:
            /// "paymentAmount ; currencyCode; shipBeforeDate ; merchantReference ; skinCode ; merchantAccount ; sessionValidity ; shopperEmail ;shopperReference ;
            ///  recurringContract ; allowedMethods ; blockedMethods; shopperStatement ; merchantReturnData ; billingAddressType ; offset"
            ///  
            /// Expected order of parameters for SHA256:
            /// "paymentAmount ; currencyCode; shipBeforeDate ; merchantReference ; skinCode ; merchantAccount ; sessionValidity ; shopperEmail ;shopperReference ;
            ///  recurringContract ; allowedMethods ; blockedMethods; shopperStatement ; merchantReturnData ; billingAddressType ; offset ; brandCode ; shopperLocale ; orderData"

            try
            {
                string[] sParamsArr = sParams.Split(';');
                if (sParamsArr != null && sParamsArr.Length > 4)
                {
                    string skinCode = sParamsArr[4];
                    string hmacKey = string.Empty;
                    string hmacSecret = string.Empty;

                    ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
                    selectQuery += "select top(1) hmac_key, group_secret from adyen_group_parameters where ";
                    selectQuery += ODBCWrapper.Parameter.NEW_PARAM("skin_code", "=", skinCode);
                    if (selectQuery.Execute("query", true) != null)
                    {
                        int count = selectQuery.Table("query").DefaultView.Count;
                        if (count > 0)
                        {
                            hmacKey = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "hmac_key", 0);
                            hmacSecret = ODBCWrapper.Utils.GetStrSafeVal(selectQuery, "group_secret", 0);
                        }
                    }

                    selectQuery.Finish();
                    selectQuery = null;

                    if (string.IsNullOrEmpty(hmacKey))
                    {
                        log.ErrorFormat("hmacKey is not valid for skinCode: {0}, will use old hash instead", skinCode);
                        StringBuilder signingStringSB = new StringBuilder();
                        for (int i = 0; i < FIELDS_MERCHANT_SIG.Length && i < sParamsArr.Length; i++)
                        {
                            signingStringSB.Append(sParamsArr[i]);
                        }
                        // Generate the signing string
                        string signingString = signingStringSB.ToString();

                        // Values are always transferred using UTF-8 encoding
                        System.Text.UTF8Encoding encoding = new System.Text.UTF8Encoding();

                        // Calculate the HMAC
                        HMACSHA1 myhmacsha1 = new HMACSHA1(encoding.GetBytes(hmacSecret));
                        retVal = System.Convert.ToBase64String(myhmacsha1.ComputeHash(encoding.GetBytes(signingString)));
                        myhmacsha1.Clear();
                    }
                    else
                    {
                        Dictionary<string, string> keyValueMap = new Dictionary<string, string>();
                        for (int i = 0; i < FIELDS_MERCHANT_SIG_SHA256.Length && i < sParamsArr.Length; i++)
                        {
                            if (!string.IsNullOrEmpty(sParamsArr[i]))
                            {
                                // replace ':' to '\\:' according to Adyen requirements
                                keyValueMap.Add(FIELDS_MERCHANT_SIG_SHA256[i], sParamsArr[i].Replace(":", "\\:"));
                            }
                        }

                        // get the keys ordered by abc asc according to Adyen requirements
                        keyValueMap = keyValueMap.OrderBy(x => x.Key).ToDictionary(x => x.Key, x => x.Value);
                        List<string> keysAndValues = new List<string>(keyValueMap.Keys);
                        keysAndValues.AddRange(keyValueMap.Values);
                        // Generate the signing string
                        string signingString = string.Join(":", keysAndValues.ToArray());

                        // Values are always transferred using UTF-8 encoding     
                        using (HMACSHA256 newHmacsha = new HMACSHA256(Utils.DecodeSecretKey(hmacKey)))
                        {
                            // Calculate the HMAC
                            retVal = System.Convert.ToBase64String(newHmacsha.ComputeHash(Encoding.UTF8.GetBytes(signingString)));
                        }
                    }
                }
            }

            catch (Exception ex)
            {
                log.ErrorFormat("Error on AdyenCreditCard.GetClientMerchantSig, Exception message: {0}, stackTrace: {1}, source: {2}", ex.Message, ex.StackTrace, ex.Source);
            }

            return retVal;
        }

        private BillingResponse ChargeAdyen(RecurringDetail recRes, string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUN, string sPass, string sCustomData, string sMerchAccount, int nPaymentNumber, int nNumberOfPayments, int nBillingMethod, string sExtraParameters)
        {
            BillingResponse ret = new BillingResponse();
            string sUserEmail = string.Empty;
            if (!Utils.IsUserExist(sSiteGUID, m_nGroupID, ref sUserEmail))
            {
                ret = new BillingResponse();
                ret.m_oStatus = BillingResponseStatus.UnKnownUser;
                ret.m_sRecieptCode = string.Empty;
                ret.m_sStatusDescription = "Unknown or active user";
                return ret;
            }

            using (Payment payApi = new Payment())
            {
                PaymentRequest payReq = new PaymentRequest();
                payReq.amount = new APILogic.AdyenPayAPI.Amount();
                payReq.amount.value = long.Parse((dChargePrice * 100).ToString());
                payReq.amount.currency = sCurrencyCode;
                payReq.merchantAccount = sMerchAccount;
                log.Debug("MerchantRef - " + AdyenUtils.GetItemID(sCustomData));
                payReq.reference = string.Format("{0}", AdyenUtils.GetItemID(sCustomData));
                log.Debug("MerchantRef - " + payReq.reference);
                payReq.shopperIP = "1.1.1.1";
                payReq.recurring = new APILogic.AdyenPayAPI.Recurring();
                payReq.recurring.contract = "RECURRING";
                payReq.selectedRecurringDetailReference = recRes.recurringDetailReference;
                payReq.shopperReference = sSiteGUID;
                payReq.shopperInteraction = "ContAuth";
                payReq.shopperEmail = sUserEmail;
                payApi.Credentials = new NetworkCredential(sUN, sPass);
                payApi.Url = AdyenUtils.GetWSPaymentUrl(m_nGroupID);
                PaymentResult payRes = payApi.authorise(payReq);

                int customDataID = Utils.AddCustomData(sCustomData);
                int adyenTransID = 1;
                string status = "Charge";
                string bankName = string.Empty;
                string bankAccount = string.Empty;
                string reason = string.Empty;

                if (!string.IsNullOrEmpty(payRes.refusalReason))
                {
                    reason = payRes.refusalReason;
                    status = payRes.resultCode;
                }
                long lRecieptCode = Core.Billing.Utils.InsertNewAdyenTransaction(m_nGroupID, sSiteGUID, recRes.card.number,
                    dChargePrice, sCurrencyCode, customDataID.ToString(),
                    sCustomData, payRes.pspReference, status, bankName, bankAccount, reason, string.Empty, nPaymentNumber, nNumberOfPayments, 3,
                    nBillingMethod, (int)eBillingProvider.Adyen, 2, ref adyenTransID, false, false);
                if (!string.IsNullOrEmpty(payRes.resultCode) && (payRes.resultCode.ToLower().Equals("refused") || payRes.resultCode.ToLower().Equals("error")))
                {
                    ret.m_oStatus = BillingResponseStatus.Fail;
                    ret.m_sStatusDescription = payRes.refusalReason;
                    string sPaymentMethod = string.Empty;
                    if (recRes != null && recRes.card != null)
                    {
                        try
                        {
                            string sBrand = recRes.variant;
                            if (!string.IsNullOrEmpty(recRes.variant))
                            {
                                if (recRes.variant.ToLower().Equals("mc"))
                                {
                                    sBrand = "MASTERCARD";
                                }
                            }
                            log.Debug("Adyen Charge - " + sSiteGUID + ":" + "Found details: " + recRes.card.number + " " + recRes.variant);
                            sPaymentMethod = string.Format("Credit Card {0} xxxx{1}", sBrand.ToUpper(), recRes.card.number);
                            AdyenUtils.SendAdyenPurchaseMail(m_nGroupID, sCustomData,
                                dChargePrice, sCurrencyCode, sPaymentMethod, sSiteGUID, lRecieptCode, payRes.pspReference, true);
                        }
                        catch
                        {
                        }
                    }

                }
                else
                {
                    ret.m_oStatus = BillingResponseStatus.Success;
                    ret.m_sExternalReceiptCode = payRes.pspReference;
                    string sPaymentMethod = string.Empty;
                    if (recRes != null && recRes.card != null)
                    {
                        try
                        {
                            string sBrand = recRes.variant;
                            if (!string.IsNullOrEmpty(recRes.variant))
                            {
                                if (recRes.variant.ToLower().Equals("mc"))
                                {
                                    sBrand = "MASTERCARD";
                                }
                            }
                            log.Info(string.Format("ChargeAdyen , for user {0} . Found details {1},{2}", sSiteGUID, recRes.card.number, recRes.variant));
                            sPaymentMethod = string.Format("Credit Card {0} xxxx{1}", sBrand.ToUpper(), recRes.card.number);
                            log.Info(string.Format("ChargeAdyen , for user {0} . sPaymentMethod {1},", sSiteGUID, sPaymentMethod));
                        }
                        catch (Exception ex)
                        {
                            log.Error(string.Format("ChargeAdyen , for user {0} . ex.Message {1}", sSiteGUID, ex.Message));
                        }
                    }
                    AdyenUtils.SendAdyenPurchaseMail(m_nGroupID, sCustomData, dChargePrice, sCurrencyCode, sPaymentMethod, sSiteGUID, lRecieptCode, payRes.pspReference, false);
                }
                ret.m_sRecieptCode = lRecieptCode.ToString();
                if (!string.IsNullOrEmpty(sExtraParameters))
                {
                    int subPurchaseID = int.Parse(sExtraParameters);
                    if (subPurchaseID > 0)
                    {
                        ODBCWrapper.UpdateQuery updateQuery = null;
                        try
                        {
                            updateQuery = new ODBCWrapper.UpdateQuery("adyen_transactions");
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_id", "=", subPurchaseID);
                            updateQuery += " where ";
                            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", adyenTransID);
                            updateQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                            updateQuery.Execute();
                        }
                        finally
                        {
                            if (updateQuery != null)
                            {
                                updateQuery.Finish();
                            }
                        }
                    }
                }
                log.Info(string.Format("ChargeAdyen , for user {0} psp reference {1}", sSiteGUID, payRes.pspReference));
                return ret;
            }
        }
        /// <summary>
        /// Get adyen contract
        /// </summary>
        /// <param name="merchAcc">set merchant account</param>
        /// <param name="sSiteGUID">set shopper reference</param>
        /// <param name="sUN">set adyen webservice user name network credential</param>
        /// <param name="sPass">set adyen webservice password network credential</param>
        /// <returns>return RecurringDetailsResult object</returns>
        protected RecurringDetailsResult GetAdyenContract(string merchAcc, string sSiteGUID, string sUN, string sPass)
        {
            RecurringDetailsResult retVal = null;
            APILogic.AdyenRecAPI.Recurring recApi = null;
            RecurringDetailsRequest recRequest = new RecurringDetailsRequest();

            try
            {
                log.Debug("Adyen_Logging - " + string.Format("Start AdyenCreditCard.GetAdyenContract() , for merchAcc: {0} , user: {1} , sUN: {2} , sPass: {3}", merchAcc, sSiteGUID, sUN, sPass));
                recApi = new APILogic.AdyenRecAPI.Recurring();
                recRequest.merchantAccount = merchAcc;
                recRequest.shopperReference = sSiteGUID;
                recRequest.recurring = new Recurring1();
                recRequest.recurring.recurringDetailName = "RECURRING";
                recRequest.recurring.contract = "RECURRING";
                recApi.Url = AdyenUtils.GetWSRecurringUrl(m_nGroupID);

                recApi.Credentials = new NetworkCredential(sUN, sPass);

                retVal = recApi.listRecurringDetails(recRequest);

                string strRecurringDetails = string.Empty;

                if (retVal != null && retVal.details != null && retVal.details.Length > 0)
                {
                    for (int i = 0; i < retVal.details.Length; i++)
                    {
                        strRecurringDetails += "recurringDetailReference" + (i + 1).ToString() + ": " + retVal.details[i].recurringDetailReference + " ; ";
                    }
                }

                log.Debug("Adyen_Logging - " + string.Format("Finished AdyenCreditCard.GetAdyenContract() , for merchAcc: {0} , user: {1} , sUN: {2} , sPass: {3}, recApi url: {4}, recurringDetails: {5}", merchAcc, sSiteGUID, sUN, sPass, recApi.Url, strRecurringDetails));
            }
            catch (Exception ex)
            {
                log.Error("Adyen_Logging - " + string.Format("AdyenCreditCard.GetAdyenContract() Error , for merchAcc: {0} , user: {1} , sUN: {2} , sPass: {3} , ex.Message: {4}", merchAcc, sSiteGUID, sUN, sPass, ex.ToString()), ex);
            }
            finally
            {
                if (recApi != null)
                {
                    recApi.Dispose();
                }
            }

            return retVal;
        }

        public override bool UpdatePurchaseIDInBillingTable(long purchaseID, long billingRefTransactionID)
        {
            ODBCWrapper.UpdateQuery updateQuery = null;
            try
            {
                updateQuery = new ODBCWrapper.UpdateQuery("adyen_transactions");
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_id", "=", purchaseID);
                updateQuery += " where ";
                updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", billingRefTransactionID);
                updateQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                updateQuery.Execute();
            }
            finally
            {
                if (updateQuery != null)
                {
                    updateQuery.Finish();
                }
            }


            return true;

        }
    }
}

namespace APILogic.AdyenPayAPI
{
    // adding request ID to header
    public partial class Payment
    {
        protected override WebRequest GetWebRequest(Uri uri)
        {
            HttpWebRequest request = (HttpWebRequest)base.GetWebRequest(uri);
            KlogMonitorHelper.MonitorLogsHelper.AddHeaderToWebService(request);
            return request;
        }
    }
}
