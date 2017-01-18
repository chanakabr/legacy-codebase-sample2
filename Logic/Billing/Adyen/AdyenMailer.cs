using DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using KLogMonitor;
using System.Reflection;
using ApiObjects;
using ApiObjects.Billing;

namespace Core.Billing
{
    public class AdyenMailer
    {
        private static readonly KLogger log = new KLogger(MethodBase.GetCurrentMethod().DeclaringType.ToString());

        public string PSPReference
        {
            get;
            set;
        }

        public string CORPSPReference
        {
            get;
            set;
        }

        public AdyenMailType MailType
        {
            get;
            set;
        }

        public AdyenMailer()
        {
            PSPReference = string.Empty;
            CORPSPReference = string.Empty;
            MailType = AdyenMailType.None;
        }

        public AdyenMailer(string pspReference, string corPspReference, AdyenMailType mailType)
        {
            PSPReference = pspReference;
            CORPSPReference = corPspReference;
            MailType = mailType;
        }

        public bool SendMail()
        {
            bool res = false;
            List<string[]> deletedData = null;
            if (IsSendMail(PSPReference, MailType, out deletedData))
            {
                log.Debug("Status - " + String.Concat("Sending mail for: ", ToString()));

                long billingID = 0;
                int groupID = 0;
                string currencyCode = string.Empty;
                string siteGuid = string.Empty;
                double realPrice = 0d;
                double totalPrice = 0d;
                int billingMethodID = 0;
                string last4Digits = string.Empty;
                string customData = string.Empty;
                string ppvModuleCode = string.Empty;
                string subCode = string.Empty;
                string ppCode = string.Empty;
                if (BillingDAL.Get_PurchaseMailData(PSPReference, ref billingID, ref groupID, ref currencyCode, ref siteGuid,
                    ref realPrice, ref totalPrice, ref billingMethodID, ref last4Digits, ref customData, ref ppvModuleCode, ref subCode,
                    ref ppCode))
                {
                    long itemCode = 0;
                    ItemType it = GetItemTypeAndCode(ppvModuleCode, subCode, ppCode, ref itemCode);
                    string itemName = string.Empty;
                    string paymentMethod = Utils.GetPaymentMethod(billingMethodID);
                    string price = string.Empty;
                    switch (MailType)
                    {
                        case AdyenMailType.PurchaseFail:
                            last4Digits = string.Empty;
                            goto case AdyenMailType.PurchaseSuccess;
                        case AdyenMailType.PurchaseSuccess:
                            {
                                itemName = GetItemName(it, itemCode, PSPReference);
                                price = totalPrice.ToString();
                                Utils.SendMail(paymentMethod, itemName, siteGuid, (int)billingID, price, currencyCode, PSPReference,
                                    groupID, last4Digits, string.Empty, MailType == AdyenMailType.PurchaseSuccess ? eMailTemplateType.Purchase : eMailTemplateType.PaymentFail);
                            }
                            break;
                        case AdyenMailType.PurchaseWithPreviewModuleSuccess:
                            {
                                itemName = GetItemName(it, itemCode, PSPReference);
                                price = realPrice.ToString();
                                string previewModulePeriod = GetPreviewModulePeriod(customData);
                                Utils.SendMail(paymentMethod, itemName, siteGuid, (int)billingID, price, currencyCode,
                                    PSPReference, groupID, last4Digits, previewModulePeriod, eMailTemplateType.Purchase);
                            }
                            break;
                        case AdyenMailType.PreviewModuleCORSuccess:
                            {
                                itemName = Utils.GetPreviewModuleItemName(groupID);
                                last4Digits = string.Empty;
                                price = realPrice.ToString();
                                Utils.SendMail(paymentMethod, itemName, siteGuid, (int)billingID, price, currencyCode, CORPSPReference,
                                    groupID, last4Digits, eMailTemplateType.PreviewModuleCancelOrRefund);
                            }
                            break;
                        default:
                            {
                                log.Error("Error - " + String.Concat("Unrecognized AdyenMailType. ", ToString()));
                                break;
                            }
                    }

                }
                else
                {
                    // failed to grab purchase mail data from db.
                    res = false;
                    log.Error("Error - " + String.Concat("Failed to grab purchase mail data from DB. ", ToString()));
                }
            }
            else
            {
                // mail has already been sent
                res = false;
                log.Error("Status - " + String.Concat("Mail has already been sent for: ", ToString()));
            }

            return res;
        }

        private string GetPreviewModulePeriod(string customData)
        {
            string res = string.Empty;
            try
            {
                XmlDocument xml = new XmlDocument();
                xml.LoadXml(customData);
                if (xml != null)
                {
                    XmlNode node = xml.FirstChild;
                    res = Utils.GetSafeValue("prevlc", ref node);
                }

            }
            catch (XmlException xmlEx)
            {
                StringBuilder sb = new StringBuilder(String.Concat("Failed to parse XML for: ", ToString()));
                sb.Append(String.Concat(" Msg: ", xmlEx.Message));
                sb.Append(String.Concat(" Line Num: ", xmlEx.LineNumber, " Line Pos: ", xmlEx.LinePosition));
                sb.Append(String.Concat(" CD Str: ", customData));
                log.Error("Exception - " + sb.ToString(), xmlEx);
            }

            return res;
        }

        private string GetItemName(ItemType it, long lItemCode, string sPSPReference)
        {
            string sTableName = string.Empty;
            string res = string.Empty;
            switch (it)
            {
                case (ItemType.Subscription):
                    sTableName = "subscriptions";
                    break;
                case (ItemType.PrePaid):
                    sTableName = "pre_paid_modules";
                    break;
                case (ItemType.PPV):
                    res = ApiDAL.Get_PPVNameForPurchaseMail(sPSPReference);
                    break;
                default:
                    break;
            }
            if (sTableName.Length > 0)
                res = PricingDAL.Get_ItemName(sTableName, lItemCode);

            return res;
        }

        private ItemType GetItemTypeAndCode(string ppvModuleCode, string subCode, string ppCode, ref long itemCode)
        {
            long temp = 0;
            if (!string.IsNullOrEmpty(subCode) && Int64.TryParse(subCode, out temp) && temp > 0)
            {
                itemCode = temp;
                return ItemType.Subscription;
            }
            if (!string.IsNullOrEmpty(ppvModuleCode) && Int64.TryParse(ppvModuleCode, out temp) && temp > 0)
            {
                itemCode = temp;
                return ItemType.PPV;
            }
            if (!string.IsNullOrEmpty(ppCode) && Int64.TryParse(ppCode, out temp) && temp > 0)
            {
                itemCode = temp;
                return ItemType.PrePaid;
            }
            itemCode = 0;
            return ItemType.Unknown;
        }

        private bool IsSendMail(string pspReference, AdyenMailType mailType, out List<string[]> deletedData)
        {
            bool res = false;
            return BillingDAL.Get_IsSendMail(pspReference, (byte)mailType, ref res, out deletedData) && res;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder("AdyenMailer. ");
            sb.Append(String.Concat(" PSP Ref: ", PSPReference));
            sb.Append(String.Concat(" COR PSP Ref: ", CORPSPReference));
            sb.Append(String.Concat(" MailType: ", MailType.ToString()));

            return sb.ToString();
        }
    }
}
