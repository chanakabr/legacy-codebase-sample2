using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using KLogMonitor;
using ApiObjects;
using Core.Users;
using System.Data;

namespace Core.Billing
{
    public static class BillingMailTemplateFactory
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public static PurchaseMailRequest GetMailTemplate(Int32 groupId, string siteGuid, string externalTransactionId, double totalAmount, string currency, string itemName, string paymentMethod, string last4Digits, 
            string previewModuleLifeCycle,  eMailTemplateType templateType, long billingTransID,  User houseHoldUser = null)
        {
            PurchaseMailRequest mailRequest;

            string sMailName = string.Empty;
            string sMailSubject = string.Empty;
            switch (templateType)
            {
                case eMailTemplateType.PaymentFail:
                    {
                        mailRequest = new PurchaseFailRequest();
                        sMailName = "PURCHASE_FAIL_MAIL";
                        sMailSubject = "PURCHASE_FAIL_MAIL_SUBJECT";
                        break;
                    }
                case eMailTemplateType.PreviewModuleCancelOrRefund:
                    {
                        mailRequest = new PreviewModuleCancelOrRefundRequest();
                        sMailName = "PREVIEW_MODULE_COR_MAIL";
                        sMailSubject = "PREVIEW_MODULE_COR_MAIL_SUBJECT";
                        break;
                    }
                case eMailTemplateType.PurchaseWithPreviewModule:
                    {
                        mailRequest = new PurchaseWithPreviewModuleRequest();
                        sMailName = "PURCHASE_WITH_PREVIEW_MODULE_MAIL";
                        sMailSubject = "PURCHASE_WITH_PREVIEW_MODULE_MAIL_SUBJECT";
                        break;
                    }
                case eMailTemplateType.Purchase:
                    {
                        mailRequest = new PurchaseMailRequest();
                        sMailName = "PURCHASE_MAIL";
                        sMailSubject = "PURCHASE_MAIL_SUBJECT";
                        break;
                    }
                default:
                    {
                        mailRequest = null;
                        sMailName = string.Empty;
                        sMailSubject = string.Empty;
                        break;
                    }
            }
            if (mailRequest != null)
            {
                DataRow groupsParameters = GetGroupsParameters(groupId);

                if (groupsParameters != null)
                {
                    object oPurchaseMail = groupsParameters[sMailName];
                    object oMailFromName = groupsParameters["MAIL_FROM_NAME"];
                    object oMailFromAdd = groupsParameters["MAIL_FROM_ADD"];
                    object oPurchaseMailSubject = groupsParameters[sMailSubject];
                    object oTaxVal = groupsParameters["tax_value"];
                    object oLastInvoiceNum = groupsParameters["last_invoice_num"];
                    object oBccAddress = groupsParameters["bcc_address"];

                    long lastInvoiceNum = 0;

                        if (oPurchaseMail != null && oPurchaseMail != DBNull.Value)
                            mailRequest.m_sTemplateName = oPurchaseMail.ToString();

                        if (oPurchaseMailSubject != null && oPurchaseMailSubject != DBNull.Value)
                            mailRequest.m_sSubject = oPurchaseMailSubject.ToString();

                        if (oMailFromName != null && oMailFromName != DBNull.Value)
                            mailRequest.m_sSenderName = oMailFromName.ToString();

                        if (oMailFromAdd != null && oMailFromAdd != DBNull.Value)
                            mailRequest.m_sSenderFrom = oMailFromAdd.ToString();

                        if (oBccAddress != null && oBccAddress != DBNull.Value)
                            mailRequest.m_sBCCAddress = oBccAddress.ToString();

                        if (oLastInvoiceNum != null && oLastInvoiceNum != DBNull.Value)
                        {
                            lastInvoiceNum = long.Parse(oLastInvoiceNum.ToString()) + 1;
                            if (lastInvoiceNum > 0)
                            {
                                UpdateInvoiceNum(lastInvoiceNum, groupId);
                            }                           
                    }

                    int hoursOffset = GetHoursOffset(groupId);

                    mailRequest.m_sPurchaseDate = GetPurchaseDateString(groupId, hoursOffset);

                    if (templateType == eMailTemplateType.PurchaseWithPreviewModule)
                    {
                            ((PurchaseWithPreviewModuleRequest)mailRequest).m_sPreviewModuleEndDate = Utils.GetEndDateTime(DateTime.UtcNow.AddHours(hoursOffset), int.Parse(previewModuleLifeCycle)).ToString("dd/MM/yyyy");
                        }
                        mailRequest.m_sTransactionNumber = billingTransID.ToString();
                        mailRequest.m_sInvoiceNum = lastInvoiceNum.ToString();
                        mailRequest.m_sExternalTransationNum = externalTransactionId;
                        mailRequest.m_sItemName = itemName;
                        mailRequest.m_sPaymentMethod = paymentMethod;
                        mailRequest.m_sPrice = string.Format("{0} {1}", getPriceStrForInvoice(totalAmount), currency);

                        mailRequest.m_sPaymentMethod = GetPaymentMethodMailVar(groupId, siteGuid, paymentMethod, last4Digits, lastInvoiceNum, externalTransactionId);

                        if (oTaxVal != null && oTaxVal != DBNull.Value)
                        {
                            double dTaxVal;
                            double taxDisc = 0;

                            double.TryParse(oTaxVal.ToString(), out dTaxVal);
                            double taxPrice = CalcPriceAfterTax(totalAmount, dTaxVal, ref taxDisc);
                            mailRequest.m_sTaxVal = dTaxVal.ToString();
                            mailRequest.m_sTaxSubtotal = getPriceStrForInvoice(taxPrice);
                            mailRequest.m_sTaxAmount = getPriceStrForInvoice(taxDisc);
                        }
                }

                string sHouseNumber = string.Empty;
                string sStreetName = string.Empty;
                string sBuildeingName = string.Empty;
                string sUnitNo = string.Empty;
                string sUnitNoEnd = string.Empty;
                string sPostalCode = string.Empty;
                string sZip = string.Empty;
                try
                {
                    if (houseHoldUser == null)
                    {
                        UserResponseObject uObj = Core.Users.Module.GetUserData(groupId, siteGuid, string.Empty);

                        if (uObj.m_RespStatus == ResponseStatus.OK)
                        {
                            if (uObj.m_user != null)
                            {
                                houseHoldUser = uObj.m_user;
                            }
                        }
                    }
                    if (houseHoldUser == null)
                    {
                        // add log here 
                        log.DebugFormat("GetMailTemplate failed houseHoldUser = null, siteGuid={0},groupId={1}, templateType={2} ", siteGuid, groupId, templateType.ToString());
                        return null;
                    }
                    mailRequest.m_sSenderTo = houseHoldUser.m_oBasicData.m_sEmail;
                    mailRequest.m_sUserEmail = houseHoldUser.m_oBasicData.m_sEmail;
                    sZip = houseHoldUser.m_oBasicData.m_sZip;
                    mailRequest.m_sLastName = houseHoldUser.m_oBasicData.m_sLastName;
                    mailRequest.m_sFirstName = houseHoldUser.m_oBasicData.m_sFirstName;

                    if (groupId == 147)
                    {
                        mailRequest.m_sAddress = BuildAddressFiled(houseHoldUser);
                    }
                }

                catch (Exception ex)
                {
                    #region Logging
                    log.Error("Exception in Billing.Utils.Initialize - " + string.Format("Exception msg: {0} , Group ID: {1} , Site Guid: {2}", ex.Message, siteGuid, groupId), ex);
                    #endregion
                }
            }
            return mailRequest;
        }

        private static int GetHoursOffset(Int32 groupId)
        {
            int hoursOffset = 0;
            string sGMTOffset = TVinciShared.WS_Utils.GetTcmConfigValue(string.Format("GMTOffset_{0}", groupId.ToString()));
            if (!string.IsNullOrEmpty(sGMTOffset))
            {
                hoursOffset = int.Parse(sGMTOffset);
            }
            return hoursOffset;
        }

        public static PurchaseViaGiftCardMailRequest GetGiftCardMailTemplate(int groupId, string userId, User user, string itemName, eTransactionType? transactionType)
        {
            PurchaseViaGiftCardMailRequest mailRequest = new PurchaseViaGiftCardMailRequest();

            DataRow groupsParameters = GetGroupsParameters(groupId);

            mailRequest.m_sTemplateName = ODBCWrapper.Utils.ExtractString(groupsParameters, "PURCHASE_WITH_GIFT_CARD_MAIL");
            mailRequest.m_sSenderName = ODBCWrapper.Utils.ExtractString(groupsParameters, "MAIL_FROM_NAME");
            mailRequest.m_sSenderFrom = ODBCWrapper.Utils.ExtractString(groupsParameters, "MAIL_FROM_ADD");
            mailRequest.m_sSubject = ODBCWrapper.Utils.ExtractString(groupsParameters, "PURCHASE_WITH_GIFT_CARD_MAIL_SUBJECT");

            mailRequest.offerType = transactionType.ToString();
            mailRequest.m_sItemName = itemName;

            mailRequest.m_sSenderTo = user.m_oBasicData.m_sEmail;
            mailRequest.m_sUserEmail = user.m_oBasicData.m_sEmail;
            mailRequest.m_sLastName = user.m_oBasicData.m_sLastName;
            mailRequest.m_sFirstName = user.m_oBasicData.m_sFirstName;

            int hoursOffset = GetHoursOffset(groupId);
            string purchaseDateString = GetPurchaseDateString(groupId, hoursOffset);

            mailRequest.m_sPurchaseDate = purchaseDateString;

            return mailRequest;
        }

        private static string GetPurchaseDateString(int groupId, int hoursOffset)
        {
            // get default email date format from DB  - get from cache 
            string dateEmailFormat = Utils.GetDateEmailFormat(groupId);
            string purchaseDateString = DateTime.UtcNow.AddHours(hoursOffset).ToString(dateEmailFormat);
            return purchaseDateString;
        }

        private static string BuildAddressFiled(User hhUser)
        {
            string sHouseNumber = string.Empty;
            string sStreetName = string.Empty;
            string sBuildeingName = string.Empty;
            string sUnitNo = string.Empty;
            string sUnitNoEnd = string.Empty;
            string sZip = string.Empty;
            if (hhUser.m_oDynamicData != null && hhUser.m_oDynamicData.m_sUserData != null)
            {
                foreach (UserDynamicDataContainer dynamicData in hhUser.m_oDynamicData.m_sUserData)
                {
                    if (dynamicData != null)
                    {
                        if (dynamicData.m_sDataType.ToLower().Trim().Equals("blockhousenumber"))
                        {
                            sHouseNumber = dynamicData.m_sValue;
                        }
                        else if (dynamicData.m_sDataType.ToLower().Trim().Equals("streetname"))
                        {
                            sStreetName = dynamicData.m_sValue;
                        }
                        else if (dynamicData.m_sDataType.ToLower().Trim().Equals("buildingname"))
                        {
                            sBuildeingName = dynamicData.m_sValue;
                        }
                        else if (dynamicData.m_sDataType.ToLower().Trim().Equals("unitstartnumber"))
                        {
                            sUnitNo = dynamicData.m_sValue;
                        }
                        else if (dynamicData.m_sDataType.ToLower().Trim().Equals("unitendnumber"))
                        {
                            sUnitNoEnd = dynamicData.m_sValue;
                        }

                    }
                }
            }
            string sUnitNumberAddress = string.Empty;
            if (!string.IsNullOrEmpty(sUnitNo))
            {
                sUnitNumberAddress = string.Format("#{0}", sUnitNo);
                if (!string.IsNullOrEmpty(sUnitNoEnd))
                {
                    sUnitNumberAddress = string.Format("{0}-", sUnitNumberAddress);
                }
            }
            if (!string.IsNullOrEmpty(sUnitNoEnd))
            {
                sUnitNumberAddress = string.Format("{0}{1}", sUnitNumberAddress, sUnitNoEnd);
            }
            return string.Format("{0} {1} {2} {3} {4} {5}", sHouseNumber, sStreetName, sBuildeingName, sUnitNumberAddress, "Singapore", sZip);
        }

        private static string getPriceStrForInvoice(double dChargePrice)
        {
            string retVal = string.Empty;
            if (dChargePrice > 10)
            {
                retVal = dChargePrice.ToString("00.00");
            }
            else
            {
                retVal = dChargePrice.ToString("0.00");
            }
            return retVal;
        }

        private static double CalcPriceAfterTax(double catalogPrice, double tax, ref double taxDisc)
        {
            double retVal = 0;
            retVal = Math.Round(catalogPrice / ((100 + tax) / 100), 2);
            taxDisc = Math.Round(catalogPrice - retVal, 2);
            return retVal;
        }

        private static string GetPaymentMethodMailVar(int nGroupID, string sSiteGUID, string sPaymentMethod, string sLast4Digits, long nBillingTransID, string sExternalNum)
        {
            string sLoweredPaymentMethod = sPaymentMethod.ToLower();
            if (sLoweredPaymentMethod.Equals("visa") || sLoweredPaymentMethod.Equals("mc") || sLoweredPaymentMethod.Equals("mastercard") || sLoweredPaymentMethod.Equals("master card"))
            {
                if (!string.IsNullOrEmpty(sLast4Digits))
                {
                    sPaymentMethod = string.Format("Credit Card " + sPaymentMethod.ToUpper() + " xxxx{0}", sLast4Digits);
                }
                else
                {
                    AdyenDirectDebit add = new AdyenDirectDebit(nGroupID);
                    /*AdyenBillingDetail detail = add.GetLastBillingUserInfo(sSiteGUID, 1);*/
                    AdyenBillingDetail detail = add.GetBillingDetailsByUserInfoAndBillingTransactionID(sSiteGUID, 1, (long)nBillingTransID);
                    if (detail != null && detail.billingInfo != null)
                    {
                        log.Debug("Visa Details - Details found for user :" + sSiteGUID);
                        sPaymentMethod = string.Format("Credit Card " + sPaymentMethod.ToUpper() + " xxxx{0}", detail.billingInfo.lastFourDigits);
                    }
                    else
                    {
                        log.Debug("Visa Details - Details not found for user :" + sSiteGUID);
                    }
                }
            }
            else if (sLoweredPaymentMethod.Equals("m1"))
            {
                sPaymentMethod = string.Format("{0} Billing {1}", sPaymentMethod.ToUpper(), sExternalNum);
                sExternalNum = string.Empty;
            }

            return sPaymentMethod;
        }


        private static void UpdateInvoiceNum(long invoiceNum, int nGroupID)
        {
            log.Debug("InvoiceNum - Invoice Num is + " + invoiceNum);
            ODBCWrapper.UpdateQuery updateQuery = new ODBCWrapper.UpdateQuery("groups_parameters");
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("last_invoice_num", "=", invoiceNum);
            updateQuery += " where ";
            updateQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", nGroupID);
            updateQuery.SetConnectionKey("BILLING_CONNECTION_STRING");
            updateQuery.Execute();
            updateQuery.Finish();
            updateQuery = null;
        }

        private static DataRow GetGroupsParameters(int groupId)
        {
            DataRow result = null;
            ODBCWrapper.DataSetSelectQuery selectQuery = new ODBCWrapper.DataSetSelectQuery();
            try
            {
                selectQuery += "select top 1 * from groups_parameters where status=1 and is_active=1 and ";
                selectQuery += ODBCWrapper.Parameter.NEW_PARAM("group_id", "=", groupId);
                selectQuery += " order by id";
                selectQuery.SetConnectionKey("BILLING_CONNECTION_STRING");

                if (selectQuery.Execute("query", true) != null)
                {
                    Int32 nCount = selectQuery.Table("query").DefaultView.Count;
                    if (nCount > 0)
                    {
                        result = selectQuery.Table("query").DefaultView[0].Row;
                    }
                }
            }
            catch (Exception ex)
            {
                log.ErrorFormat("GetGroupsParameters {0}", ex);
            }
            finally
            {
                selectQuery.Finish();
                selectQuery = null;
            }
            return result;
        }
    }
}
