using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Configuration;
using DAL;
using System.Text.RegularExpressions;
using System.Data;
using KLogMonitor;
using System.Reflection;
using System.Web;
using System.ServiceModel;
using Core.Users;
using ApiObjects;
using APILogic.AdyenRecAPI;
using APILogic.AdyenPayAPI;
using ApiObjects.Billing;

namespace Core.Billing
{
    public class AdyenDirectDebit : BaseDirectDebit
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public AdyenDirectDebit(int groupID)
            : base(groupID)
        {
        }

        public override bool CancelPayment(string sPSPReference, string sMerchantAccount, string sSiteGuid, int nGroupID, long lPurchaseID, int nType, int nNumOfCancelOrRefundAttempts, double? dChargePrice, string sCurrencyCode)
        {
            bool retVal = true;
            string sUN = string.Empty;
            string sPass = string.Empty;
            Payment payApi = null;
            try
            {
                string sAdyenEventCode = string.Empty;
                string sAdyenSuccess = string.Empty;
                string sAdyenReason = string.Empty;
                string sLast4Digits = string.Empty;
                bool bIsAdyenFetchedRequest = false;
                AdyenUtils.GetAccountCredentials(sMerchantAccount, ref sUN, ref sPass);
                payApi = new Payment();
                payApi.Credentials = new NetworkCredential(sUN, sPass);
                ModificationRequest modReq = new ModificationRequest();
                modReq.merchantAccount = sMerchantAccount;
                modReq.originalReference = sPSPReference;
                ModificationResult modRes = payApi.cancel(modReq);
                bIsAdyenFetchedRequest = modRes != null && modRes.response != null && modRes.response.Trim().ToLower() == "[cancel-received]" && !string.IsNullOrEmpty(modRes.pspReference);
                if (bIsAdyenFetchedRequest)
                {
                    // adyen fetched the request. adyen will send a notification once they know whether the request succeeded or not
                    log.Debug("Adyen Cancel - " + string.Format("Cancel request rcvd by Adyen. COR PSP Ref: {0} , Original PSP Ref: {1} , Site Guid: {2}", modRes.pspReference, sPSPReference, sSiteGuid));
                    BillingDAL.Insert_NewAdyenCancelOrRefund(modRes.pspReference, sPSPReference, (int)CancelOrRefundRequestStatus.FetchedByAdyen, (int)CancelOrRefundRequestType.Cancel, sSiteGuid, dChargePrice, sCurrencyCode, nGroupID, lPurchaseID, nType, string.Empty, nNumOfCancelOrRefundAttempts);
                }
                else
                {
                    log.Debug("Adyen Cancel - " + string.Format("Cancel request was not rcvd by Adyen. Original PSP Ref: {0} , SiteGuid: {1}", sPSPReference, sSiteGuid));
                    BillingDAL.Insert_NewAdyenCancelOrRefund(modRes.pspReference, sPSPReference, (int)CancelOrRefundRequestStatus.AdyenFailedToFetch, (int)CancelOrRefundRequestType.Cancel, sSiteGuid, dChargePrice, sCurrencyCode, nGroupID, lPurchaseID, nType, modRes.response, nNumOfCancelOrRefundAttempts);
                }

                if (bIsAdyenFetchedRequest && BillingDAL.Get_DataOfAdyenNotificationForAdyenCallback(modRes.pspReference, ref sAdyenEventCode, ref sAdyenSuccess, ref sLast4Digits, ref sAdyenReason))
                {
                    // notification already rcvd
                    bool? bIsSuccess = AdyenUtils.ConvertAdyenSuccess(sAdyenSuccess);
                    if (!bIsSuccess.HasValue)
                    {
                        #region Logging
                        log.Debug("Adyen Cancel - " + string.Format("No value in adyen notification success. Original PSP Ref: {0} , Cancel PSP Ref: {1} , Success value: {2}", sPSPReference, modRes.pspReference, sAdyenSuccess));
                        #endregion
                    }
                    else
                    {
                        bool bIsCancelSuccessful = bIsSuccess.Value;
                        if (bIsCancelSuccessful)
                        {
                            BillingDAL.Update_AdyenCancelOrRefundRequestStatus(modRes.pspReference, (int)CancelOrRefundRequestStatus.Authorised);
                        }
                        else
                        {
                            BillingDAL.Update_AdyenCancelOrRefundRequestStatus(modRes.pspReference, (int)CancelOrRefundRequestStatus.Refused);
                        }
                    }
                    BillingDAL.Update_AdyenNotification(modRes.pspReference, true);
                }
            }
            catch (Exception ex)
            {
                retVal = false;
                #region Logging
                log.Error("Exception - " + string.Format("Exception in CancelPayment. Exception msg: {0} , PSP Ref: {1} , Site Guid: {2} , Group ID: {3} , Purchase ID: {4}", ex.Message, sPSPReference, sSiteGuid, nGroupID, lPurchaseID), ex);
                #endregion
            }
            finally
            {
                if (payApi != null)
                {
                    payApi.Dispose();
                }
            }

            return retVal;
        }

        public override bool RefundPayment(string sPSPReference, string sSiteGuid, int nGroupID, double dChargePrice, string sCurrencyCode, long lPurchaseID, int nType, int nHowManyCancelOrRefundAttemptsSoFarNotIncludingThisOne)
        {
            bool retVal = true;
            Payment payApi = null;
            try
            {
                payApi = new Payment();
                string sUN = string.Empty;
                string sPass = string.Empty;
                string sMerchantAccount = string.Empty;
                string sAdyenEventCode = string.Empty;
                string sAdyenSuccess = string.Empty;
                string sAdyenReason = string.Empty;
                string sLast4Digits = string.Empty;
                bool bIsAdyenFetchedRequest = false;
                AdyenUtils.GetWSCredentials(m_nGroupID, ref sUN, ref sPass, ref sMerchantAccount);
                payApi.Credentials = new NetworkCredential(sUN, sPass);
                payApi.Url = AdyenUtils.GetWSPaymentUrl(m_nGroupID);
                ModificationRequest modReq = new ModificationRequest();
                modReq.originalReference = sPSPReference;
                modReq.merchantAccount = sMerchantAccount;
                modReq.modificationAmount = new APILogic.AdyenPayAPI.Amount();
                modReq.modificationAmount.currency = sCurrencyCode;
                modReq.modificationAmount.value = AdyenUtils.GetAdyenPriceFormat(dChargePrice);
                ModificationResult modRes = payApi.refund(modReq);
                bIsAdyenFetchedRequest = modRes != null && modRes.response != null && modRes.response.Trim().ToLower() == "[refund-received]" && !string.IsNullOrEmpty(modRes.pspReference);
                if (bIsAdyenFetchedRequest)
                {
                    // adyen fetched the request. adyen will send a notification once they know whether the request succeeded or not
                    log.Debug("Adyen Refund - " + string.Format("Refund request rcvd by Adyen. COR PSP Ref: {0} , Original PSP Ref: {1} , Site Guid: {2}", modRes.pspReference, sPSPReference, sSiteGuid));
                    BillingDAL.Insert_NewAdyenCancelOrRefund(modRes.pspReference, sPSPReference, (int)CancelOrRefundRequestStatus.FetchedByAdyen, (int)CancelOrRefundRequestType.Refund, sSiteGuid, dChargePrice, sCurrencyCode, nGroupID, lPurchaseID, nType, string.Empty, nHowManyCancelOrRefundAttemptsSoFarNotIncludingThisOne);
                }
                else
                {
                    log.Debug("Adyen Refund - " + string.Format("Refund request was not rcvd by Adyen. Original PSP Ref: {0} , SiteGuid: {1}", sPSPReference, sSiteGuid));
                    BillingDAL.Insert_NewAdyenCancelOrRefund(modRes.pspReference, sPSPReference, (int)CancelOrRefundRequestStatus.AdyenFailedToFetch, (int)CancelOrRefundRequestType.Refund, sSiteGuid, dChargePrice, sCurrencyCode, nGroupID, lPurchaseID, nType, modRes.response, nHowManyCancelOrRefundAttemptsSoFarNotIncludingThisOne);
                }
                if (bIsAdyenFetchedRequest && BillingDAL.Get_DataOfAdyenNotificationForAdyenCallback(modRes.pspReference, ref sAdyenEventCode, ref sAdyenSuccess, ref sLast4Digits, ref sAdyenReason))
                {
                    // notification already rcvd
                    bool? bIsSuccess = AdyenUtils.ConvertAdyenSuccess(sAdyenSuccess);
                    if (!bIsSuccess.HasValue)
                    {
                        #region Logging
                        log.Debug("Adyen Refund - " + string.Format("No value in adyen notification success. Original PSP Ref: {0} , Cancel PSP Ref: {1} , Success value: {2}", sPSPReference, modRes.pspReference, sAdyenSuccess));
                        #endregion
                    }
                    else
                    {
                        bool bIsRefundSuccessful = bIsSuccess.Value;
                        if (bIsRefundSuccessful)
                        {
                            BillingDAL.Update_AdyenCancelOrRefundRequestStatus(modRes.pspReference, (int)CancelOrRefundRequestStatus.Authorised);
                        }
                        else
                        {
                            BillingDAL.Update_AdyenCancelOrRefundRequestStatus(modRes.pspReference, (int)CancelOrRefundRequestStatus.Refused);
                        }
                    }
                    BillingDAL.Update_AdyenNotification(modRes.pspReference, true);
                }

            }
            catch (Exception ex)
            {
                retVal = false;
                #region Logging
                log.Error("Exception - " + string.Format("Exception in RefundPayment. Exception msg: {0} , PSP Ref: {1} , Site Guid: {2} , Group ID: {3} , Purchase ID: {4}", ex.Message, sPSPReference, sSiteGuid, nGroupID, lPurchaseID), ex);
                #endregion
            }
            finally
            {
                if (payApi != null)
                {
                    payApi.Dispose();
                }
            }
            return retVal;
        }


        public override BillingResponse ChargeUser(string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUserIP, string sCustomData, int nPaymentNumber, int nNumberOfPayments, string sExtraParameters, int nBillingMethod)
        {
            BillingResponse ret = new BillingResponse();
            try
            {
                string merchAcc = string.Empty;
                string merchPurchesAccount = string.Empty;
                string sUN = string.Empty;
                string sPass = string.Empty;

                log.Info("AdyenDirectDebit ChargeUser start at " + DateTime.Now.ToString());
                AdyenUtils.GetWSCredentials(m_nGroupID, ref sUN, ref sPass, ref merchAcc, ref merchPurchesAccount, 3);
                RecurringDetailsResult recRes = GetAdyenConract(merchAcc, sSiteGUID, sUN, sPass);
                log.Info(string.Format("WSCredentials : GroupID={0}, sUN={1}, sPass={2},merchAcc={3},merchPurchesAccount={4},PurchesType={5}", m_nGroupID, sUN, sPass, merchAcc, merchPurchesAccount, 3));
                if (recRes.details == null || recRes.details.Length == 0)
                {
                    AdyenUtils.GetTvinciWSCredentials(ref sUN, ref sPass, ref merchAcc);
                    recRes = GetAdyenConract(merchAcc, sSiteGUID, sUN, sPass);
                }
                if (recRes.details != null && recRes.details.Length > 0)
                {
                    try
                    {
                        RecurringDetail det = null;


                        Core.Billing.AdyenUtils.BillingType bt = default(Core.Billing.AdyenUtils.BillingType);


                        if (nBillingMethod != 0)
                        {
                            bt = Core.Billing.AdyenUtils.BillingType.DirectDebit;
                            if (nBillingMethod == (int)ePaymentMethod.CreditCard || nBillingMethod == (int)ePaymentMethod.Visa || nBillingMethod == (int)ePaymentMethod.MasterCard)
                            {
                                bt = Core.Billing.AdyenUtils.BillingType.CreditCard;
                            }
                            det = AdyenUtils.GetRecurringDetailByLastFourDigits(sSiteGUID, recRes.details, bt, string.Empty);
                        }
                        else // if nBillingMethod is "0" then set nBillingMethod according to the last contract at adyen
                        {
                            bt = AdyenUtils.BillingType.CreditCard;
                            det = AdyenUtils.GetRecurringDetailByLastFourDigits(sSiteGUID, recRes.details, bt, string.Empty);
                            if (det.variant.ToLower().Equals("mc"))
                            {
                                nBillingMethod = (int)ePaymentMethod.MasterCard;
                            }
                            else if (det.variant.ToLower().Equals("visa"))
                            {
                                nBillingMethod = (int)ePaymentMethod.Visa;
                            }
                        }

                        string lastFourDigits = (det.card != null ? det.card.number : string.Empty);
                        log.Info(string.Format("{0}: {1} bank={2},card={3},lastFourDigits={4},name={5},variant={6},recurringDetailReference={7},elv={8}", "ChargeAdyen with Params ", "RecurringDetail",
                                                       det.bank, det.card, lastFourDigits, det.name, det.variant, det.recurringDetailReference, det.elv));
                        log.Info(string.Format("sSiteGUID={0}, dChargePrice={1}, sCurrencyCode={2},sUN={3},sPass={4},sCustomData={5},merchPurchesAccount={6},nPaymentNumber={7},nNumberOfPayments={8}, BillingMethod={9},sExtraParameters={10}",
                            sSiteGUID, dChargePrice, sCurrencyCode, sUN, sPass, sCustomData, merchPurchesAccount, nPaymentNumber, nNumberOfPayments, nBillingMethod, sExtraParameters));



                        ret = ChargeAdyen(det, sSiteGUID, dChargePrice, sCurrencyCode, sUN, sPass, sCustomData, merchPurchesAccount, nPaymentNumber, nNumberOfPayments, nBillingMethod, sExtraParameters);
                        log.Info(string.Format("Billing Response ret : Status={0}, RecieptCode={1}, StatusDescription={2}", ret.m_oStatus, ret.m_sRecieptCode, ret.m_sStatusDescription));
                    }
                    catch (Exception ex)
                    {
                        ret.m_oStatus = BillingResponseStatus.Fail;
                        ret.m_sStatusDescription = "No latest details found for user " + sSiteGUID;
                        log.Error("Exception - Exception on renewal for user " + sSiteGUID + " ex: " + ex.Message, ex);
                    }
                }
            }
            catch (Exception ex)
            {
                ret.m_oStatus = BillingResponseStatus.Fail;
                ret.m_sStatusDescription = "No latest details found for user " + sSiteGUID;
                log.Error("Exception - Exception on renewal for user " + sSiteGUID + " ex: " + ex.Message, ex);
            }
            return ret;
        }


        private BillingResponse ChargeAdyen(RecurringDetail recRes, string sSiteGUID, double dChargePrice, string sCurrencyCode, string sUN, string sPass, string sCustomData, string sMerchAccount, int nPaymentNumber, int nNumberOfPayments, int nBillingMethod, string sExtraParameters)
        {
            BillingResponse ret = new BillingResponse();
            Payment payApi = null;
            ODBCWrapper.UpdateQuery updateQuery = null;
            try
            {
                UserResponseObject uObj = Core.Users.Module.GetUserData(m_nGroupID, sSiteGUID, string.Empty);
                if (uObj.m_RespStatus != ResponseStatus.OK)
                {
                    ret = new BillingResponse();
                    ret.m_oStatus = BillingResponseStatus.UnKnownUser;
                    ret.m_sRecieptCode = "";
                    ret.m_sStatusDescription = "Unknown or active user";
                    return ret;
                }
                payApi = new Payment();
                payApi.Url = AdyenUtils.GetWSPaymentUrl(m_nGroupID);
                //payApi.Url = "https://pal-test.adyen.com/pal/servlet/soap/Payment";
                PaymentRequest payReq = new PaymentRequest();
                payReq.amount = new APILogic.AdyenPayAPI.Amount();
                payReq.amount.value = AdyenUtils.GetAdyenPriceFormat(dChargePrice);
                payReq.amount.currency = sCurrencyCode;
                payReq.merchantAccount = sMerchAccount;
                payReq.fraudOffset = 100;
                payReq.reference = string.Format("{0}-Recurring", AdyenUtils.GetItemID(sCustomData));
                payReq.shopperIP = "1.1.1.1";
                payReq.recurring = new APILogic.AdyenPayAPI.Recurring();
                payReq.recurring.contract = "RECURRING";
                payReq.selectedRecurringDetailReference = recRes.recurringDetailReference;
                payReq.shopperReference = sSiteGUID;
                payReq.shopperInteraction = "ContAuth";
                payReq.shopperEmail = uObj.m_user.m_oBasicData.m_sEmail;
                payApi.Credentials = new NetworkCredential(sUN, sPass);
                PaymentResult payRes = payApi.authorise(payReq);

                int customDataID = Utils.AddCustomData(sCustomData);
                int adyenTransID = 1;
                string status = "Renewal";
                string bankName = string.Empty;
                string bankAccount = string.Empty;
                string reason = string.Empty;
                string sPaymentMethod = string.Empty;
                string cardNumber = string.Empty;
                if (recRes.bank != null)
                {
                    bankName = recRes.bank.bankName;
                    bankAccount = recRes.bank.bankAccountNumber;
                }

                if (recRes.card != null && !string.IsNullOrEmpty(recRes.card.number))
                {
                    cardNumber = recRes.card.number;
                }

                if (!string.IsNullOrEmpty(payRes.refusalReason))
                {
                    reason = payRes.refusalReason;
                    status = payRes.resultCode;
                }
                long lRecieptCode = Core.Billing.Utils.InsertNewAdyenTransaction(m_nGroupID, sSiteGUID, recRes.card.number, dChargePrice, sCurrencyCode, customDataID.ToString(),
                    sCustomData, payRes.pspReference, status, bankName, bankAccount, reason, string.Empty, nPaymentNumber, nNumberOfPayments, 3, nBillingMethod, (int)eBillingProvider.Adyen, 2, ref adyenTransID, false, false);
                if (!string.IsNullOrEmpty(payRes.resultCode) && payRes.resultCode.ToLower().Equals("refused"))
                {
                    ret.m_oStatus = BillingResponseStatus.Fail;
                    ret.m_sStatusDescription = reason;
                    if (recRes != null && recRes.card != null)
                    {
                        string sBrand = recRes.variant;
                        if (!string.IsNullOrEmpty(recRes.variant))
                        {
                            if (recRes.variant.ToLower().Equals("mc"))
                            {
                                sBrand = "MASTERCARD";
                            }
                        }
                        sPaymentMethod = string.Format("Credit Card {0} xxxx{1}", sBrand.ToUpper(), recRes.card.number);
                    }
                    AdyenUtils.SendAdyenPurchaseMail(m_nGroupID, sCustomData, dChargePrice, sCurrencyCode, sPaymentMethod, sSiteGUID, lRecieptCode, payRes.pspReference, true);
                }
                else
                {
                    ret.m_oStatus = BillingResponseStatus.Success;

                    if (recRes != null && recRes.card != null)
                    {
                        string sBrand = recRes.variant;
                        if (!string.IsNullOrEmpty(recRes.variant))
                        {
                            if (recRes.variant.ToLower().Equals("mc"))
                            {
                                sBrand = "MASTERCARD";
                            }
                        }
                        sPaymentMethod = string.Format("Credit Card {0} xxxx{1}", sBrand.ToUpper(), recRes.card.number);
                    }
                    AdyenUtils.SendAdyenPurchaseMail(m_nGroupID, sCustomData, dChargePrice, sCurrencyCode, sPaymentMethod, sSiteGUID, lRecieptCode, payRes.pspReference, false);
                }
                ret.m_sRecieptCode = lRecieptCode.ToString();
                if (!string.IsNullOrEmpty(sExtraParameters))
                {
                    int subPurchaseID = int.Parse(sExtraParameters);
                    if (subPurchaseID > 0)
                    {
                        updateQuery = new ODBCWrapper.UpdateQuery("adyen_transactions");
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("purchase_id", "=", subPurchaseID);
                        updateQuery += " where ";
                        updateQuery += ODBCWrapper.Parameter.NEW_PARAM("id", "=", adyenTransID);
                        updateQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
                        updateQuery.Execute();
                    }
                }
                log.Debug("Adyen Renewal - Adyen Renewal for user " + sSiteGUID + " psp reference :" + payRes.pspReference);
            }
            finally
            {
                if (payApi != null)
                {
                    payApi.Dispose();
                }
                if (updateQuery != null)
                {
                    updateQuery.Finish();
                }
            }
            return ret;
        }

        private RecurringDetailsResult GetAdyenConract(string merchAcc, string sSiteGUID, string sUN, string sPass)
        {
            RecurringDetailsResult retVal = null;
            using (APILogic.AdyenRecAPI.Recurring recApi = new APILogic.AdyenRecAPI.Recurring())
            {
                RecurringDetailsRequest recRequest = new RecurringDetailsRequest();
                recRequest.merchantAccount = merchAcc;
                recRequest.shopperReference = sSiteGUID;
                recRequest.recurring = new Recurring1();
                recRequest.recurring.recurringDetailName = "RECURRING";
                recRequest.recurring.contract = "RECURRING";

                recApi.Url = AdyenUtils.GetWSRecurringUrl(m_nGroupID);

                recApi.Credentials = new NetworkCredential(sUN, sPass);

                retVal = recApi.listRecurringDetails(recRequest);
                return retVal;
            }
        }

        public override AdyenBillingDetail GetLastBillingUserInfo(string sSiteGUID, int nBillingMethod)
        {
            AdyenBillingDetail res = new AdyenBillingDetail();
            UserResponseObject uObj = Core.Users.Module.GetUserData(m_nGroupID, sSiteGUID, string.Empty);
            if (uObj.m_RespStatus != ResponseStatus.OK)
            {
                return res;
            }

            string merchAcc = string.Empty;
            string sUN = string.Empty;
            string sPass = string.Empty;
            AdyenUtils.GetWSCredentials(m_nGroupID, ref sUN, ref sPass, ref merchAcc);
            RecurringDetailsResult recRes = GetAdyenConract(merchAcc, sSiteGUID, sUN, sPass);


            if (recRes.details != null && recRes.details.Length > 0)
            {
                try
                {
                    RecurringDetail det = null;
                    Core.Billing.AdyenUtils.BillingType bt = Core.Billing.AdyenUtils.BillingType.DirectDebit;
                    if (nBillingMethod == 1)
                    {
                        bt = Core.Billing.AdyenUtils.BillingType.CreditCard;
                    }


                    det = AdyenUtils.GetRecurringDetailByLastFourDigits(sSiteGUID, recRes.details, bt, string.Empty);
                    res.Initialize(det);
                }
                catch (Exception ex)
                {
                    log.Error("Exception - Exception Get Last Billing User Info for user " + sSiteGUID + " ex: " + ex.Message);
                }
            }
            return res;
        }

        public override bool CancelOrRefundPayment(string sPSPReference, string sSiteGuid, double? dPrice, string sCurrencyCode, long lPurchaseID, int nType, bool bIsCancelOrRefundResultOfPreviewModule, int nHowManyCancelOrRefundAttemptsSoFarNotIncludingThisOne)
        {

            bool retVal = true;
            int i = 0;
            Payment payApi = null;
            try
            {
                string sUN = string.Empty;
                string sPass = string.Empty;
                string sMerchantAccount = string.Empty;
                string sEventCode = string.Empty;
                string sAdyenSuccess = string.Empty;
                string sLast4Digits = string.Empty;
                string sAdyenReason = string.Empty;
                bool bIsAdyenResponseValid = false;
                AdyenUtils.GetWSCredentials(m_nGroupID, ref sUN, ref sPass, ref sMerchantAccount);
                payApi = new Payment();
                payApi.Credentials = new NetworkCredential(sUN, sPass);
                ModificationRequest modReq = new ModificationRequest();
                modReq.merchantAccount = sMerchantAccount;
                modReq.originalReference = sPSPReference;
                payApi.Url = AdyenUtils.GetWSPaymentUrl(m_nGroupID);

                int nBoundOfNumOfCancelOrRefundAttempts = Utils.GetPreviewModuleNumOfCancelOrRefundAttempts();
                ModificationResult modRes = null;
                for (i = nHowManyCancelOrRefundAttemptsSoFarNotIncludingThisOne; (i < nBoundOfNumOfCancelOrRefundAttempts && bIsCancelOrRefundResultOfPreviewModule) || (i < nHowManyCancelOrRefundAttemptsSoFarNotIncludingThisOne + 1 && !bIsCancelOrRefundResultOfPreviewModule); i++)
                {
                    modRes = payApi.cancelOrRefund(modReq);
                    bIsAdyenResponseValid = modRes != null && modRes.response != null && modRes.response.Trim().ToLower() == "[cancelorrefund-received]" && !string.IsNullOrEmpty(modRes.pspReference);
                    if (bIsAdyenResponseValid)
                    {
                        log.Debug("Adyen Cancel Or Refund - " + string.Format("COR request rcvd by Adyen. COR PSP Ref: {0} , Original PSP Ref: {1} , Site Guid: {2}", modRes.pspReference, sPSPReference, sSiteGuid));
                        BillingDAL.Insert_NewAdyenCancelOrRefund(modRes.pspReference, sPSPReference, (int)CancelOrRefundRequestStatus.FetchedByAdyen, (int)CancelOrRefundRequestType.CancelOrRefund, sSiteGuid, dPrice, sCurrencyCode, m_nGroupID, lPurchaseID, nType, string.Empty, i + 1);
                        break;
                    }
                    else
                    {
                        log.Debug("Adyen Cancel Or Refund - " + string.Format("COR request was not rcvd by Adyen. Original PSP Ref: {0} , SiteGuid: {1}", sPSPReference, sSiteGuid));
                        BillingDAL.Insert_NewAdyenCancelOrRefund(modRes.pspReference, sPSPReference, (int)CancelOrRefundRequestStatus.AdyenFailedToFetch, (int)CancelOrRefundRequestType.CancelOrRefund, sSiteGuid, dPrice, sCurrencyCode, m_nGroupID, lPurchaseID, nType, modRes.response, i + 1);
                    }

                }
                if (bIsAdyenResponseValid && BillingDAL.Get_DataOfAdyenNotificationForAdyenCallback(modRes.pspReference, ref sEventCode, ref sAdyenSuccess, ref sLast4Digits, ref sAdyenReason))
                {
                    // notification already rcvd
                    bool? bIsSuccess = AdyenUtils.ConvertAdyenSuccess(sAdyenSuccess);
                    if (!bIsSuccess.HasValue)
                    {
                        #region Logging
                        log.Debug("Adyen Cancel Or Refund - " + string.Format("No success value at adyen notification: Payment PSP Ref: {0} , Refund PSP Ref: {1}", sPSPReference, modRes.pspReference));
                        #endregion
                    }
                    else
                    {
                        bool bIsCancelOrRefundSuccessful = bIsSuccess.Value;
                        if (bIsCancelOrRefundSuccessful)
                        {
                            BillingDAL.Update_AdyenCancelOrRefundRequestStatus(modRes.pspReference, (int)CancelOrRefundRequestStatus.Authorised);
                            if (bIsCancelOrRefundResultOfPreviewModule)
                            {

                            }
                        }
                        else
                        {
                            BillingDAL.Update_AdyenCancelOrRefundRequestStatus(modRes.pspReference, (int)CancelOrRefundRequestStatus.Refused);
                            if (bIsCancelOrRefundResultOfPreviewModule)
                            {
                                if (i < nBoundOfNumOfCancelOrRefundAttempts)
                                {
                                    CancelOrRefundPayment(sPSPReference, sSiteGuid, dPrice, sCurrencyCode, lPurchaseID, nType, true, i + 1);
                                }
                                else
                                {
                                    #region Logging
                                    log.Debug("Adyen Cancel Or Refund - " + string.Format("cor request of preview module failed. Original PSP Ref: {0} , COR PSP Ref: {1}", sPSPReference, modRes.pspReference));
                                    #endregion
                                }
                            }
                        }
                    }
                    BillingDAL.Update_AdyenNotification(modRes.pspReference, true);
                }
            }
            catch (Exception ex)
            {
                retVal = false;
                #region Logging
                log.Error("Exception - " + string.Format("Exception at CancelOrRefundPayment. Exception msg: {0} , PSP Ref: {1} , Site Guid: {2} , Purchase ID: {3} , Number of COR attempts (number of for iterations trying to send COR): {4}", ex.Message, sPSPReference, sSiteGuid, lPurchaseID, i), ex);
                #endregion
            }
            finally
            {
                if (payApi != null)
                {
                    payApi.Dispose();
                }
            }

            return retVal;
        }

        protected internal AdyenBillingDetail GetBillingDetailsByUserInfoAndBillingTransactionID(string sSiteGUID, int nBillingMethod, long lIDInBillingTransactions)
        {
            AdyenBillingDetail res = new AdyenBillingDetail();
            UserResponseObject uObj = Core.Users.Module.GetUserData(m_nGroupID, sSiteGUID, string.Empty);
            if (uObj.m_RespStatus != ResponseStatus.OK)
            {
                return res;
            }

            string merchAcc = string.Empty;
            string sUN = string.Empty;
            string sPass = string.Empty;
            AdyenUtils.GetWSCredentials(m_nGroupID, ref sUN, ref sPass, ref merchAcc);
            RecurringDetailsResult recRes = GetAdyenConract(merchAcc, sSiteGUID, sUN, sPass);

            bool bIsResultsReturned = recRes != null && recRes.details != null && recRes.details.Length > 0;
            log.Debug("GetBillingDetailsByUserInfoAndBillingTransactionID - " + String.Concat("Request for: ", lIDInBillingTransactions, " returned results: ", bIsResultsReturned.ToString().ToLower()));

            if (recRes.details != null && recRes.details.Length > 0)
            {
                try
                {
                    RecurringDetail det = null;
                    Core.Billing.AdyenUtils.BillingType bt = Core.Billing.AdyenUtils.BillingType.DirectDebit;
                    if (nBillingMethod == 1)
                    {
                        bt = Core.Billing.AdyenUtils.BillingType.CreditCard;
                    }

                    string sLast4Digits = ApiDAL.Get_Last4DigitsByBillingTransctionID(lIDInBillingTransactions);
                    Regex regex = new Regex(@"^[0-9]{4}$");
                    if (string.IsNullOrEmpty(sLast4Digits) || !regex.IsMatch(sLast4Digits))
                        sLast4Digits = string.Empty;
                    det = AdyenUtils.GetRecurringDetailByLastFourDigits(sSiteGUID, recRes.details, bt, sLast4Digits);
                    res.Initialize(det);
                }
                catch (Exception ex)
                {
                    StringBuilder sb = new StringBuilder("Exception occurred at: GetBillingDetailsByUserInfoAndBillingTransactionID. Site Guid: ");
                    sb.Append(sSiteGUID);
                    sb.Append(String.Concat(" Billing trans ID: ", lIDInBillingTransactions));
                    sb.Append(String.Concat(" Ex Msg: ", ex.Message));
                    sb.Append(String.Concat(" Stack trace: ", ex.StackTrace));
                    log.Error("Exception - " + sb.ToString());
                }
            }
            return res;
        }


    }
}
