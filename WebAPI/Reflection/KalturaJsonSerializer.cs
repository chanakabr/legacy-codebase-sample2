// NOTICE: This is a generated file, to modify it, edit Program.cs in Reflector project
using System;
using System.Linq;
using System.Collections.Generic;
using WebAPI.Managers.Scheme;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.Social;
using WebAPI.Models.General;
using WebAPI.Models.Notifications;
using WebAPI.Models.Notification;
using WebAPI.App_Start;
using WebAPI.Models.Catalog;
using WebAPI.Models.API;
using WebAPI.Models.Pricing;
using WebAPI.Models.Users;
using WebAPI.Models.Partner;
using WebAPI.Models.Upload;
using WebAPI.Models.DMS;
using WebAPI.Models.Domains;
using WebAPI.Models.Billing;
using WebAPI.EventNotifications;
using WebAPI.Managers.Models;

namespace WebAPI.Models.ConditionalAccess
{
    public partial class KalturaAccessControlBlockAction
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaAccessControlMessage
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Code != null)
            {
                ret.Add("\"code\": " + "\"" + EscapeJson(Code) + "\"");
            }
            if(Message != null)
            {
                ret.Add("\"message\": " + "\"" + EscapeJson(Message) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Code != null)
            {
                ret += "<code>" + EscapeXml(Code) + "</code>";
            }
            if(Message != null)
            {
                ret += "<message>" + EscapeXml(Message) + "</message>";
            }
            return ret;
        }
    }
    public partial class KalturaAdsContext
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Sources != null)
            {
                propertyValue = "[" + String.Join(", ", Sources.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"sources\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Sources != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Sources.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<sources>" + propertyValue + "</sources>";
            }
            return ret;
        }
    }
    public partial class KalturaAdsSource
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AdsParams != null)
            {
                ret.Add("\"adsParam\": " + "\"" + EscapeJson(AdsParams) + "\"");
            }
            if(AdsPolicy.HasValue)
            {
                ret.Add("\"adsPolicy\": " + "\"" + Enum.GetName(typeof(KalturaAdsPolicy), AdsPolicy) + "\"");
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Type != null)
            {
                ret.Add("\"type\": " + "\"" + EscapeJson(Type) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AdsParams != null)
            {
                ret += "<adsParam>" + EscapeXml(AdsParams) + "</adsParam>";
            }
            if(AdsPolicy.HasValue)
            {
                ret += "<adsPolicy>" + "\"" + Enum.GetName(typeof(KalturaAdsPolicy), AdsPolicy) + "\"" + "</adsPolicy>";
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(Type != null)
            {
                ret += "<type>" + EscapeXml(Type) + "</type>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetFileContext
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(FullLifeCycle != null)
            {
                ret.Add("\"fullLifeCycle\": " + "\"" + EscapeJson(FullLifeCycle) + "\"");
            }
            ret.Add("\"isOfflinePlayBack\": " + IsOfflinePlayBack.ToString().ToLower());
            if(ViewLifeCycle != null)
            {
                ret.Add("\"viewLifeCycle\": " + "\"" + EscapeJson(ViewLifeCycle) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(FullLifeCycle != null)
            {
                ret += "<fullLifeCycle>" + EscapeXml(FullLifeCycle) + "</fullLifeCycle>";
            }
            ret += "<isOfflinePlayBack>" + IsOfflinePlayBack.ToString().ToLower() + "</isOfflinePlayBack>";
            if(ViewLifeCycle != null)
            {
                ret += "<viewLifeCycle>" + EscapeXml(ViewLifeCycle) + "</viewLifeCycle>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetRuleAction
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaAssetUserRuleAction
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaAssetUserRuleBlockAction
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaBillingResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ExternalReceiptCode != null)
            {
                ret.Add("\"externalReceiptCode\": " + "\"" + EscapeJson(ExternalReceiptCode) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"external_receipt_code\": " + "\"" + EscapeJson(ExternalReceiptCode) + "\"");
                }
            }
            if(ReceiptCode != null)
            {
                ret.Add("\"receiptCode\": " + "\"" + EscapeJson(ReceiptCode) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"receipt_code\": " + "\"" + EscapeJson(ReceiptCode) + "\"");
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ExternalReceiptCode != null)
            {
                ret += "<externalReceiptCode>" + EscapeXml(ExternalReceiptCode) + "</externalReceiptCode>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<external_receipt_code>" + EscapeXml(ExternalReceiptCode) + "</external_receipt_code>";
                }
            }
            if(ReceiptCode != null)
            {
                ret += "<receiptCode>" + EscapeXml(ReceiptCode) + "</receiptCode>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<receipt_code>" + EscapeXml(ReceiptCode) + "</receipt_code>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaBillingTransaction
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(actionDate.HasValue)
            {
                ret.Add("\"actionDate\": " + actionDate);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"action_date\": " + actionDate);
                }
            }
            ret.Add("\"billingAction\": " + "\"" + Enum.GetName(typeof(KalturaBillingAction), billingAction) + "\"");
            if (currentVersion == null || isOldVersion)
            {
                ret.Add("\"billing_action\": " + "\"" + Enum.GetName(typeof(KalturaBillingAction), billingAction) + "\"");
            }
            ret.Add("\"billingPriceType\": " + "\"" + Enum.GetName(typeof(KalturaBillingPriceType), billingPriceType) + "\"");
            if(billingProviderRef.HasValue)
            {
                ret.Add("\"billingProviderRef\": " + billingProviderRef);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"billing_provider_ref\": " + billingProviderRef);
                }
            }
            if(endDate.HasValue)
            {
                ret.Add("\"endDate\": " + endDate);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"end_date\": " + endDate);
                }
            }
            if(isRecurring.HasValue)
            {
                ret.Add("\"isRecurring\": " + isRecurring.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"is_recurring\": " + isRecurring.ToString().ToLower());
                }
            }
            ret.Add("\"itemType\": " + "\"" + Enum.GetName(typeof(KalturaBillingItemsType), itemType) + "\"");
            if (currentVersion == null || isOldVersion)
            {
                ret.Add("\"item_type\": " + "\"" + Enum.GetName(typeof(KalturaBillingItemsType), itemType) + "\"");
            }
            ret.Add("\"paymentMethod\": " + "\"" + Enum.GetName(typeof(KalturaPaymentMethodType), paymentMethod) + "\"");
            if (currentVersion == null || isOldVersion)
            {
                ret.Add("\"payment_method\": " + "\"" + Enum.GetName(typeof(KalturaPaymentMethodType), paymentMethod) + "\"");
            }
            if(paymentMethodExtraDetails != null)
            {
                ret.Add("\"paymentMethodExtraDetails\": " + "\"" + EscapeJson(paymentMethodExtraDetails) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"payment_method_extra_details\": " + "\"" + EscapeJson(paymentMethodExtraDetails) + "\"");
                }
            }
            if(price != null)
            {
                propertyValue = price.ToJson(currentVersion, omitObsolete);
                ret.Add("\"price\": " + propertyValue);
            }
            if(purchasedItemCode != null)
            {
                ret.Add("\"purchasedItemCode\": " + "\"" + EscapeJson(purchasedItemCode) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"purchased_item_code\": " + "\"" + EscapeJson(purchasedItemCode) + "\"");
                }
            }
            if(purchasedItemName != null)
            {
                ret.Add("\"purchasedItemName\": " + "\"" + EscapeJson(purchasedItemName) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"purchased_item_name\": " + "\"" + EscapeJson(purchasedItemName) + "\"");
                }
            }
            if(purchaseID.HasValue)
            {
                ret.Add("\"purchaseId\": " + purchaseID);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"purchase_id\": " + purchaseID);
                }
            }
            if(recieptCode != null)
            {
                ret.Add("\"recieptCode\": " + "\"" + EscapeJson(recieptCode) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"reciept_code\": " + "\"" + EscapeJson(recieptCode) + "\"");
                }
            }
            if(remarks != null)
            {
                ret.Add("\"remarks\": " + "\"" + EscapeJson(remarks) + "\"");
            }
            if(startDate.HasValue)
            {
                ret.Add("\"startDate\": " + startDate);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"start_date\": " + startDate);
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(actionDate.HasValue)
            {
                ret += "<actionDate>" + actionDate + "</actionDate>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<action_date>" + actionDate + "</action_date>";
                }
            }
            ret += "<billingAction>" + "\"" + Enum.GetName(typeof(KalturaBillingAction), billingAction) + "\"" + "</billingAction>";
            if (currentVersion == null || isOldVersion)
            {
            ret += "<billing_action>" + "\"" + Enum.GetName(typeof(KalturaBillingAction), billingAction) + "\"" + "</billing_action>";
            }
            ret += "<billingPriceType>" + "\"" + Enum.GetName(typeof(KalturaBillingPriceType), billingPriceType) + "\"" + "</billingPriceType>";
            if(billingProviderRef.HasValue)
            {
                ret += "<billingProviderRef>" + billingProviderRef + "</billingProviderRef>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<billing_provider_ref>" + billingProviderRef + "</billing_provider_ref>";
                }
            }
            if(endDate.HasValue)
            {
                ret += "<endDate>" + endDate + "</endDate>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<end_date>" + endDate + "</end_date>";
                }
            }
            if(isRecurring.HasValue)
            {
                ret += "<isRecurring>" + isRecurring.ToString().ToLower() + "</isRecurring>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<is_recurring>" + isRecurring.ToString().ToLower() + "</is_recurring>";
                }
            }
            ret += "<itemType>" + "\"" + Enum.GetName(typeof(KalturaBillingItemsType), itemType) + "\"" + "</itemType>";
            if (currentVersion == null || isOldVersion)
            {
            ret += "<item_type>" + "\"" + Enum.GetName(typeof(KalturaBillingItemsType), itemType) + "\"" + "</item_type>";
            }
            ret += "<paymentMethod>" + "\"" + Enum.GetName(typeof(KalturaPaymentMethodType), paymentMethod) + "\"" + "</paymentMethod>";
            if (currentVersion == null || isOldVersion)
            {
            ret += "<payment_method>" + "\"" + Enum.GetName(typeof(KalturaPaymentMethodType), paymentMethod) + "\"" + "</payment_method>";
            }
            if(paymentMethodExtraDetails != null)
            {
                ret += "<paymentMethodExtraDetails>" + EscapeXml(paymentMethodExtraDetails) + "</paymentMethodExtraDetails>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<payment_method_extra_details>" + EscapeXml(paymentMethodExtraDetails) + "</payment_method_extra_details>";
                }
            }
            if(price != null)
            {
                propertyValue = price.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<price>" + propertyValue + "</price>";
            }
            if(purchasedItemCode != null)
            {
                ret += "<purchasedItemCode>" + EscapeXml(purchasedItemCode) + "</purchasedItemCode>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<purchased_item_code>" + EscapeXml(purchasedItemCode) + "</purchased_item_code>";
                }
            }
            if(purchasedItemName != null)
            {
                ret += "<purchasedItemName>" + EscapeXml(purchasedItemName) + "</purchasedItemName>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<purchased_item_name>" + EscapeXml(purchasedItemName) + "</purchased_item_name>";
                }
            }
            if(purchaseID.HasValue)
            {
                ret += "<purchaseId>" + purchaseID + "</purchaseId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<purchase_id>" + purchaseID + "</purchase_id>";
                }
            }
            if(recieptCode != null)
            {
                ret += "<recieptCode>" + EscapeXml(recieptCode) + "</recieptCode>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<reciept_code>" + EscapeXml(recieptCode) + "</reciept_code>";
                }
            }
            if(remarks != null)
            {
                ret += "<remarks>" + EscapeXml(remarks) + "</remarks>";
            }
            if(startDate.HasValue)
            {
                ret += "<startDate>" + startDate + "</startDate>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<start_date>" + startDate + "</start_date>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaBillingTransactionListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(transactions != null)
            {
                propertyValue = "[" + String.Join(", ", transactions.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(transactions != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", transactions.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaCDVRAdapterProfile
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AdapterUrl != null)
            {
                ret.Add("\"adapterUrl\": " + "\"" + EscapeJson(AdapterUrl) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"adapter_url\": " + "\"" + EscapeJson(AdapterUrl) + "\"");
                }
            }
            if(DynamicLinksSupport.HasValue)
            {
                ret.Add("\"dynamicLinksSupport\": " + DynamicLinksSupport.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"dynamic_links_support\": " + DynamicLinksSupport.ToString().ToLower());
                }
            }
            if(ExternalIdentifier != null)
            {
                ret.Add("\"externalIdentifier\": " + "\"" + EscapeJson(ExternalIdentifier) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"external_identifier\": " + "\"" + EscapeJson(ExternalIdentifier) + "\"");
                }
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(IsActive.HasValue)
            {
                ret.Add("\"isActive\": " + IsActive.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"is_active\": " + IsActive.ToString().ToLower());
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(Settings != null)
            {
                propertyValue = "{" + String.Join(", ", Settings.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"settings\": " + propertyValue);
            }
            if(SharedSecret != null)
            {
                ret.Add("\"sharedSecret\": " + "\"" + EscapeJson(SharedSecret) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"shared_secret\": " + "\"" + EscapeJson(SharedSecret) + "\"");
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AdapterUrl != null)
            {
                ret += "<adapterUrl>" + EscapeXml(AdapterUrl) + "</adapterUrl>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<adapter_url>" + EscapeXml(AdapterUrl) + "</adapter_url>";
                }
            }
            if(DynamicLinksSupport.HasValue)
            {
                ret += "<dynamicLinksSupport>" + DynamicLinksSupport.ToString().ToLower() + "</dynamicLinksSupport>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<dynamic_links_support>" + DynamicLinksSupport.ToString().ToLower() + "</dynamic_links_support>";
                }
            }
            if(ExternalIdentifier != null)
            {
                ret += "<externalIdentifier>" + EscapeXml(ExternalIdentifier) + "</externalIdentifier>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<external_identifier>" + EscapeXml(ExternalIdentifier) + "</external_identifier>";
                }
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(IsActive.HasValue)
            {
                ret += "<isActive>" + IsActive.ToString().ToLower() + "</isActive>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<is_active>" + IsActive.ToString().ToLower() + "</is_active>";
                }
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(Settings != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Settings.Select(pair => "<itemKey>" + pair.Key + "</itemKey>" + pair.Value.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<settings>" + propertyValue + "</settings>";
            }
            if(SharedSecret != null)
            {
                ret += "<sharedSecret>" + EscapeXml(SharedSecret) + "</sharedSecret>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<shared_secret>" + EscapeXml(SharedSecret) + "</shared_secret>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaCDVRAdapterProfileListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaCollectionEntitlement
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaCompensation
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"amount\": " + Amount);
            ret.Add("\"appliedRenewalIterations\": " + AppliedRenewalIterations);
            ret.Add("\"compensationType\": " + "\"" + Enum.GetName(typeof(KalturaCompensationType), CompensationType) + "\"");
            ret.Add("\"id\": " + Id);
            ret.Add("\"purchaseId\": " + PurchaseId);
            ret.Add("\"subscriptionId\": " + SubscriptionId);
            ret.Add("\"totalRenewalIterations\": " + TotalRenewalIterations);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<amount>" + Amount + "</amount>";
            ret += "<appliedRenewalIterations>" + AppliedRenewalIterations + "</appliedRenewalIterations>";
            ret += "<compensationType>" + "\"" + Enum.GetName(typeof(KalturaCompensationType), CompensationType) + "\"" + "</compensationType>";
            ret += "<id>" + Id + "</id>";
            ret += "<purchaseId>" + PurchaseId + "</purchaseId>";
            ret += "<subscriptionId>" + SubscriptionId + "</subscriptionId>";
            ret += "<totalRenewalIterations>" + TotalRenewalIterations + "</totalRenewalIterations>";
            return ret;
        }
    }
    public partial class KalturaCustomDrmPlaybackPluginData
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Data != null)
            {
                ret.Add("\"data\": " + "\"" + EscapeJson(Data) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Data != null)
            {
                ret += "<data>" + EscapeXml(Data) + "</data>";
            }
            return ret;
        }
    }
    public partial class KalturaDrmPlaybackPluginData
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(LicenseURL != null)
            {
                ret.Add("\"licenseURL\": " + "\"" + EscapeJson(LicenseURL) + "\"");
            }
            ret.Add("\"scheme\": " + "\"" + Enum.GetName(typeof(KalturaDrmSchemeName), Scheme) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(LicenseURL != null)
            {
                ret += "<licenseURL>" + EscapeXml(LicenseURL) + "</licenseURL>";
            }
            ret += "<scheme>" + "\"" + Enum.GetName(typeof(KalturaDrmSchemeName), Scheme) + "\"" + "</scheme>";
            return ret;
        }
    }
    public partial class KalturaEndDateOffsetRuleAction
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaEntitlement
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(CurrentDate.HasValue)
            {
                ret.Add("\"currentDate\": " + CurrentDate);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"current_date\": " + CurrentDate);
                }
            }
            if(CurrentUses.HasValue)
            {
                ret.Add("\"currentUses\": " + CurrentUses);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"current_uses\": " + CurrentUses);
                }
            }
            if(DeviceName != null)
            {
                ret.Add("\"deviceName\": " + "\"" + EscapeJson(DeviceName) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"device_name\": " + "\"" + EscapeJson(DeviceName) + "\"");
                }
            }
            if(DeviceUDID != null)
            {
                ret.Add("\"deviceUdid\": " + "\"" + EscapeJson(DeviceUDID) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"device_udid\": " + "\"" + EscapeJson(DeviceUDID) + "\"");
                }
            }
            if(EndDate.HasValue)
            {
                ret.Add("\"endDate\": " + EndDate);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"end_date\": " + EndDate);
                }
            }
            if(!DeprecatedAttribute.IsDeprecated("4.8.0.0", currentVersion) && EntitlementId != null)
            {
                ret.Add("\"entitlementId\": " + "\"" + EscapeJson(EntitlementId) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"entitlement_id\": " + "\"" + EscapeJson(EntitlementId) + "\"");
                }
            }
            ret.Add("\"householdId\": " + HouseholdId);
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(IsCancelationWindowEnabled.HasValue)
            {
                ret.Add("\"isCancelationWindowEnabled\": " + IsCancelationWindowEnabled.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"is_cancelation_window_enabled\": " + IsCancelationWindowEnabled.ToString().ToLower());
                }
            }
            if(!omitObsolete && IsInGracePeriod.HasValue)
            {
                ret.Add("\"isInGracePeriod\": " + IsInGracePeriod.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"is_in_grace_period\": " + IsInGracePeriod.ToString().ToLower());
                }
            }
            if(!omitObsolete && IsRenewable.HasValue)
            {
                ret.Add("\"isRenewable\": " + IsRenewable.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"is_renewable\": " + IsRenewable.ToString().ToLower());
                }
            }
            if(!omitObsolete && IsRenewableForPurchase.HasValue)
            {
                ret.Add("\"isRenewableForPurchase\": " + IsRenewableForPurchase.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"is_renewable_for_purchase\": " + IsRenewableForPurchase.ToString().ToLower());
                }
            }
            if(LastViewDate.HasValue)
            {
                ret.Add("\"lastViewDate\": " + LastViewDate);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"last_view_date\": " + LastViewDate);
                }
            }
            if(MaxUses.HasValue)
            {
                ret.Add("\"maxUses\": " + MaxUses);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"max_uses\": " + MaxUses);
                }
            }
            if(!omitObsolete && MediaFileId.HasValue)
            {
                ret.Add("\"mediaFileId\": " + MediaFileId);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"media_file_id\": " + MediaFileId);
                }
            }
            if(!omitObsolete && MediaId.HasValue)
            {
                ret.Add("\"mediaId\": " + MediaId);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"media_id\": " + MediaId);
                }
            }
            if(!omitObsolete && NextRenewalDate.HasValue)
            {
                ret.Add("\"nextRenewalDate\": " + NextRenewalDate);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"next_renewal_date\": " + NextRenewalDate);
                }
            }
            ret.Add("\"paymentMethod\": " + "\"" + Enum.GetName(typeof(KalturaPaymentMethodType), PaymentMethod) + "\"");
            if (currentVersion == null || isOldVersion)
            {
                ret.Add("\"payment_method\": " + "\"" + Enum.GetName(typeof(KalturaPaymentMethodType), PaymentMethod) + "\"");
            }
            if(ProductId != null)
            {
                ret.Add("\"productId\": " + "\"" + EscapeJson(ProductId) + "\"");
            }
            if(PurchaseDate.HasValue)
            {
                ret.Add("\"purchaseDate\": " + PurchaseDate);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"purchase_date\": " + PurchaseDate);
                }
            }
            if(!omitObsolete && PurchaseId.HasValue)
            {
                ret.Add("\"purchaseId\": " + PurchaseId);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"purchase_id\": " + PurchaseId);
                }
            }
            if(!omitObsolete)
            {
                ret.Add("\"type\": " + "\"" + Enum.GetName(typeof(KalturaTransactionType), Type) + "\"");
            }
            if(UserId != null)
            {
                ret.Add("\"userId\": " + "\"" + EscapeJson(UserId) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(CurrentDate.HasValue)
            {
                ret += "<currentDate>" + CurrentDate + "</currentDate>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<current_date>" + CurrentDate + "</current_date>";
                }
            }
            if(CurrentUses.HasValue)
            {
                ret += "<currentUses>" + CurrentUses + "</currentUses>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<current_uses>" + CurrentUses + "</current_uses>";
                }
            }
            if(DeviceName != null)
            {
                ret += "<deviceName>" + EscapeXml(DeviceName) + "</deviceName>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<device_name>" + EscapeXml(DeviceName) + "</device_name>";
                }
            }
            if(DeviceUDID != null)
            {
                ret += "<deviceUdid>" + EscapeXml(DeviceUDID) + "</deviceUdid>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<device_udid>" + EscapeXml(DeviceUDID) + "</device_udid>";
                }
            }
            if(EndDate.HasValue)
            {
                ret += "<endDate>" + EndDate + "</endDate>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<end_date>" + EndDate + "</end_date>";
                }
            }
            if(!DeprecatedAttribute.IsDeprecated("4.8.0.0", currentVersion) && EntitlementId != null)
            {
                ret += "<entitlementId>" + EscapeXml(EntitlementId) + "</entitlementId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<entitlement_id>" + EscapeXml(EntitlementId) + "</entitlement_id>";
                }
            }
            ret += "<householdId>" + HouseholdId + "</householdId>";
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(IsCancelationWindowEnabled.HasValue)
            {
                ret += "<isCancelationWindowEnabled>" + IsCancelationWindowEnabled.ToString().ToLower() + "</isCancelationWindowEnabled>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<is_cancelation_window_enabled>" + IsCancelationWindowEnabled.ToString().ToLower() + "</is_cancelation_window_enabled>";
                }
            }
            if(!omitObsolete && IsInGracePeriod.HasValue)
            {
                ret += "<isInGracePeriod>" + IsInGracePeriod.ToString().ToLower() + "</isInGracePeriod>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<is_in_grace_period>" + IsInGracePeriod.ToString().ToLower() + "</is_in_grace_period>";
                }
            }
            if(!omitObsolete && IsRenewable.HasValue)
            {
                ret += "<isRenewable>" + IsRenewable.ToString().ToLower() + "</isRenewable>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<is_renewable>" + IsRenewable.ToString().ToLower() + "</is_renewable>";
                }
            }
            if(!omitObsolete && IsRenewableForPurchase.HasValue)
            {
                ret += "<isRenewableForPurchase>" + IsRenewableForPurchase.ToString().ToLower() + "</isRenewableForPurchase>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<is_renewable_for_purchase>" + IsRenewableForPurchase.ToString().ToLower() + "</is_renewable_for_purchase>";
                }
            }
            if(LastViewDate.HasValue)
            {
                ret += "<lastViewDate>" + LastViewDate + "</lastViewDate>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<last_view_date>" + LastViewDate + "</last_view_date>";
                }
            }
            if(MaxUses.HasValue)
            {
                ret += "<maxUses>" + MaxUses + "</maxUses>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<max_uses>" + MaxUses + "</max_uses>";
                }
            }
            if(!omitObsolete && MediaFileId.HasValue)
            {
                ret += "<mediaFileId>" + MediaFileId + "</mediaFileId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<media_file_id>" + MediaFileId + "</media_file_id>";
                }
            }
            if(!omitObsolete && MediaId.HasValue)
            {
                ret += "<mediaId>" + MediaId + "</mediaId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<media_id>" + MediaId + "</media_id>";
                }
            }
            if(!omitObsolete && NextRenewalDate.HasValue)
            {
                ret += "<nextRenewalDate>" + NextRenewalDate + "</nextRenewalDate>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<next_renewal_date>" + NextRenewalDate + "</next_renewal_date>";
                }
            }
            ret += "<paymentMethod>" + "\"" + Enum.GetName(typeof(KalturaPaymentMethodType), PaymentMethod) + "\"" + "</paymentMethod>";
            if (currentVersion == null || isOldVersion)
            {
            ret += "<payment_method>" + "\"" + Enum.GetName(typeof(KalturaPaymentMethodType), PaymentMethod) + "\"" + "</payment_method>";
            }
            if(ProductId != null)
            {
                ret += "<productId>" + EscapeXml(ProductId) + "</productId>";
            }
            if(PurchaseDate.HasValue)
            {
                ret += "<purchaseDate>" + PurchaseDate + "</purchaseDate>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<purchase_date>" + PurchaseDate + "</purchase_date>";
                }
            }
            if(!omitObsolete && PurchaseId.HasValue)
            {
                ret += "<purchaseId>" + PurchaseId + "</purchaseId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<purchase_id>" + PurchaseId + "</purchase_id>";
                }
            }
            if(!omitObsolete)
            {
                ret += "<type>" + "\"" + Enum.GetName(typeof(KalturaTransactionType), Type) + "\"" + "</type>";
            }
            if(UserId != null)
            {
                ret += "<userId>" + EscapeXml(UserId) + "</userId>";
            }
            return ret;
        }
    }
    public partial class KalturaEntitlementCancellation
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"householdId\": " + HouseholdId);
            ret.Add("\"id\": " + Id);
            if(ProductId != null)
            {
                ret.Add("\"productId\": " + "\"" + EscapeJson(ProductId) + "\"");
            }
            if(!omitObsolete)
            {
                ret.Add("\"type\": " + "\"" + Enum.GetName(typeof(KalturaTransactionType), Type) + "\"");
            }
            if(UserId != null)
            {
                ret.Add("\"userId\": " + "\"" + EscapeJson(UserId) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<householdId>" + HouseholdId + "</householdId>";
            ret += "<id>" + Id + "</id>";
            if(ProductId != null)
            {
                ret += "<productId>" + EscapeXml(ProductId) + "</productId>";
            }
            if(!omitObsolete)
            {
                ret += "<type>" + "\"" + Enum.GetName(typeof(KalturaTransactionType), Type) + "\"" + "</type>";
            }
            if(UserId != null)
            {
                ret += "<userId>" + EscapeXml(UserId) + "</userId>";
            }
            return ret;
        }
    }
    public partial class KalturaEntitlementFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(!DeprecatedAttribute.IsDeprecated("4.8.0.0", currentVersion) && EntitlementTypeEqual.HasValue)
            {
                ret.Add("\"entitlementTypeEqual\": " + "\"" + Enum.GetName(typeof(KalturaTransactionType), EntitlementTypeEqual) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"entitlement_type\": " + "\"" + Enum.GetName(typeof(KalturaTransactionType), EntitlementTypeEqual) + "\"");
                }
            }
            ret.Add("\"entityReferenceEqual\": " + "\"" + Enum.GetName(typeof(KalturaEntityReferenceBy), EntityReferenceEqual) + "\"");
            if(IsExpiredEqual.HasValue)
            {
                ret.Add("\"isExpiredEqual\": " + IsExpiredEqual.ToString().ToLower());
            }
            if(ProductTypeEqual.HasValue)
            {
                ret.Add("\"productTypeEqual\": " + "\"" + Enum.GetName(typeof(KalturaTransactionType), ProductTypeEqual) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(!DeprecatedAttribute.IsDeprecated("4.8.0.0", currentVersion) && EntitlementTypeEqual.HasValue)
            {
                ret += "<entitlementTypeEqual>" + "\"" + Enum.GetName(typeof(KalturaTransactionType), EntitlementTypeEqual) + "\"" + "</entitlementTypeEqual>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<entitlement_type>" + "\"" + Enum.GetName(typeof(KalturaTransactionType), EntitlementTypeEqual) + "\"" + "</entitlement_type>";
                }
            }
            ret += "<entityReferenceEqual>" + "\"" + Enum.GetName(typeof(KalturaEntityReferenceBy), EntityReferenceEqual) + "\"" + "</entityReferenceEqual>";
            if(IsExpiredEqual.HasValue)
            {
                ret += "<isExpiredEqual>" + IsExpiredEqual.ToString().ToLower() + "</isExpiredEqual>";
            }
            if(ProductTypeEqual.HasValue)
            {
                ret += "<productTypeEqual>" + "\"" + Enum.GetName(typeof(KalturaTransactionType), ProductTypeEqual) + "\"" + "</productTypeEqual>";
            }
            return ret;
        }
    }
    public partial class KalturaEntitlementListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Entitlements != null)
            {
                propertyValue = "[" + String.Join(", ", Entitlements.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Entitlements != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Entitlements.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaEntitlementRenewal
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"date\": " + Date);
            if(Price != null)
            {
                propertyValue = Price.ToJson(currentVersion, omitObsolete);
                ret.Add("\"price\": " + propertyValue);
            }
            ret.Add("\"purchaseId\": " + PurchaseId);
            ret.Add("\"subscriptionId\": " + SubscriptionId);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<date>" + Date + "</date>";
            if(Price != null)
            {
                propertyValue = Price.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<price>" + propertyValue + "</price>";
            }
            ret += "<purchaseId>" + PurchaseId + "</purchaseId>";
            ret += "<subscriptionId>" + SubscriptionId + "</subscriptionId>";
            return ret;
        }
    }
    public partial class KalturaEntitlementRenewalBase
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"price\": " + Price);
            ret.Add("\"purchaseId\": " + PurchaseId);
            ret.Add("\"subscriptionId\": " + SubscriptionId);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<price>" + Price + "</price>";
            ret += "<purchaseId>" + PurchaseId + "</purchaseId>";
            ret += "<subscriptionId>" + SubscriptionId + "</subscriptionId>";
            return ret;
        }
    }
    public partial class KalturaEntitlementsFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"by\": " + "\"" + Enum.GetName(typeof(KalturaEntityReferenceBy), By) + "\"");
            ret.Add("\"entitlementType\": " + "\"" + Enum.GetName(typeof(KalturaTransactionType), EntitlementType) + "\"");
            if (currentVersion == null || isOldVersion)
            {
                ret.Add("\"entitlement_type\": " + "\"" + Enum.GetName(typeof(KalturaTransactionType), EntitlementType) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<by>" + "\"" + Enum.GetName(typeof(KalturaEntityReferenceBy), By) + "\"" + "</by>";
            ret += "<entitlementType>" + "\"" + Enum.GetName(typeof(KalturaTransactionType), EntitlementType) + "\"" + "</entitlementType>";
            if (currentVersion == null || isOldVersion)
            {
            ret += "<entitlement_type>" + "\"" + Enum.GetName(typeof(KalturaTransactionType), EntitlementType) + "\"" + "</entitlement_type>";
            }
            return ret;
        }
    }
    public partial class KalturaExternalReceipt
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(PaymentGatewayName != null)
            {
                ret.Add("\"paymentGatewayName\": " + "\"" + EscapeJson(PaymentGatewayName) + "\"");
            }
            if(ReceiptId != null)
            {
                ret.Add("\"receiptId\": " + "\"" + EscapeJson(ReceiptId) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(PaymentGatewayName != null)
            {
                ret += "<paymentGatewayName>" + EscapeXml(PaymentGatewayName) + "</paymentGatewayName>";
            }
            if(ReceiptId != null)
            {
                ret += "<receiptId>" + EscapeXml(ReceiptId) + "</receiptId>";
            }
            return ret;
        }
    }
    public partial class KalturaFairPlayPlaybackPluginData
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Certificate != null)
            {
                ret.Add("\"certificate\": " + "\"" + EscapeJson(Certificate) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Certificate != null)
            {
                ret += "<certificate>" + EscapeXml(Certificate) + "</certificate>";
            }
            return ret;
        }
    }
    public partial class KalturaHouseholdPremiumService
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaHouseholdPremiumServiceListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(PremiumServices != null)
            {
                propertyValue = "[" + String.Join(", ", PremiumServices.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(PremiumServices != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", PremiumServices.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaHouseholdQuota
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"availableQuota\": " + AvailableQuota);
            ret.Add("\"householdId\": " + HouseholdId);
            ret.Add("\"totalQuota\": " + TotalQuota);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<availableQuota>" + AvailableQuota + "</availableQuota>";
            ret += "<householdId>" + HouseholdId + "</householdId>";
            ret += "<totalQuota>" + TotalQuota + "</totalQuota>";
            return ret;
        }
    }
    public partial class KalturaLicensedUrl
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AltUrl != null)
            {
                ret.Add("\"altUrl\": " + "\"" + EscapeJson(AltUrl) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"alt_url\": " + "\"" + EscapeJson(AltUrl) + "\"");
                }
            }
            if(MainUrl != null)
            {
                ret.Add("\"mainUrl\": " + "\"" + EscapeJson(MainUrl) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"main_url\": " + "\"" + EscapeJson(MainUrl) + "\"");
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AltUrl != null)
            {
                ret += "<altUrl>" + EscapeXml(AltUrl) + "</altUrl>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<alt_url>" + EscapeXml(AltUrl) + "</alt_url>";
                }
            }
            if(MainUrl != null)
            {
                ret += "<mainUrl>" + EscapeXml(MainUrl) + "</mainUrl>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<main_url>" + EscapeXml(MainUrl) + "</main_url>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaLicensedUrlBaseRequest
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetId != null)
            {
                ret.Add("\"assetId\": " + "\"" + EscapeJson(AssetId) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetId != null)
            {
                ret += "<assetId>" + EscapeXml(AssetId) + "</assetId>";
            }
            return ret;
        }
    }
    public partial class KalturaLicensedUrlEpgRequest
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"startDate\": " + StartDate);
            ret.Add("\"streamType\": " + "\"" + Enum.GetName(typeof(KalturaStreamType), StreamType) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<startDate>" + StartDate + "</startDate>";
            ret += "<streamType>" + "\"" + Enum.GetName(typeof(KalturaStreamType), StreamType) + "\"" + "</streamType>";
            return ret;
        }
    }
    public partial class KalturaLicensedUrlMediaRequest
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(BaseUrl != null)
            {
                ret.Add("\"baseUrl\": " + "\"" + EscapeJson(BaseUrl) + "\"");
            }
            ret.Add("\"contentId\": " + ContentId);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(BaseUrl != null)
            {
                ret += "<baseUrl>" + EscapeXml(BaseUrl) + "</baseUrl>";
            }
            ret += "<contentId>" + ContentId + "</contentId>";
            return ret;
        }
    }
    public partial class KalturaLicensedUrlRecordingRequest
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(FileType != null)
            {
                ret.Add("\"fileType\": " + "\"" + EscapeJson(FileType) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(FileType != null)
            {
                ret += "<fileType>" + EscapeXml(FileType) + "</fileType>";
            }
            return ret;
        }
    }
    public partial class KalturaNpvrPremiumService
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(QuotaInMinutes.HasValue)
            {
                ret.Add("\"quotaInMinutes\": " + QuotaInMinutes);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(QuotaInMinutes.HasValue)
            {
                ret += "<quotaInMinutes>" + QuotaInMinutes + "</quotaInMinutes>";
            }
            return ret;
        }
    }
    public partial class KalturaPlaybackContext
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Actions != null)
            {
                propertyValue = "[" + String.Join(", ", Actions.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"actions\": " + propertyValue);
            }
            if(Messages != null)
            {
                propertyValue = "[" + String.Join(", ", Messages.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"messages\": " + propertyValue);
            }
            if(Sources != null)
            {
                propertyValue = "[" + String.Join(", ", Sources.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"sources\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Actions != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Actions.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<actions>" + propertyValue + "</actions>";
            }
            if(Messages != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Messages.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<messages>" + propertyValue + "</messages>";
            }
            if(Sources != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Sources.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<sources>" + propertyValue + "</sources>";
            }
            return ret;
        }
    }
    public partial class KalturaPlaybackContextOptions
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetFileIds != null)
            {
                ret.Add("\"assetFileIds\": " + "\"" + EscapeJson(AssetFileIds) + "\"");
            }
            if(Context.HasValue)
            {
                ret.Add("\"context\": " + "\"" + Enum.GetName(typeof(KalturaPlaybackContextType), Context) + "\"");
            }
            if(MediaProtocol != null)
            {
                ret.Add("\"mediaProtocol\": " + "\"" + EscapeJson(MediaProtocol) + "\"");
            }
            if(StreamerType != null)
            {
                ret.Add("\"streamerType\": " + "\"" + EscapeJson(StreamerType) + "\"");
            }
            ret.Add("\"urlType\": " + "\"" + Enum.GetName(typeof(KalturaUrlType), UrlType) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetFileIds != null)
            {
                ret += "<assetFileIds>" + EscapeXml(AssetFileIds) + "</assetFileIds>";
            }
            if(Context.HasValue)
            {
                ret += "<context>" + "\"" + Enum.GetName(typeof(KalturaPlaybackContextType), Context) + "\"" + "</context>";
            }
            if(MediaProtocol != null)
            {
                ret += "<mediaProtocol>" + EscapeXml(MediaProtocol) + "</mediaProtocol>";
            }
            if(StreamerType != null)
            {
                ret += "<streamerType>" + EscapeXml(StreamerType) + "</streamerType>";
            }
            ret += "<urlType>" + "\"" + Enum.GetName(typeof(KalturaUrlType), UrlType) + "\"" + "</urlType>";
            return ret;
        }
    }
    public partial class KalturaPlaybackSource
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(!DeprecatedAttribute.IsDeprecated("4.6.0.0", currentVersion) && AdsParams != null)
            {
                ret.Add("\"adsParam\": " + "\"" + EscapeJson(AdsParams) + "\"");
            }
            if(!DeprecatedAttribute.IsDeprecated("4.6.0.0", currentVersion) && AdsPolicy.HasValue)
            {
                ret.Add("\"adsPolicy\": " + "\"" + Enum.GetName(typeof(KalturaAdsPolicy), AdsPolicy) + "\"");
            }
            if(Drm != null)
            {
                propertyValue = "[" + String.Join(", ", Drm.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"drm\": " + propertyValue);
            }
            if(Format != null)
            {
                ret.Add("\"format\": " + "\"" + EscapeJson(Format) + "\"");
            }
            if(Protocols != null)
            {
                ret.Add("\"protocols\": " + "\"" + EscapeJson(Protocols) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(!DeprecatedAttribute.IsDeprecated("4.6.0.0", currentVersion) && AdsParams != null)
            {
                ret += "<adsParam>" + EscapeXml(AdsParams) + "</adsParam>";
            }
            if(!DeprecatedAttribute.IsDeprecated("4.6.0.0", currentVersion) && AdsPolicy.HasValue)
            {
                ret += "<adsPolicy>" + "\"" + Enum.GetName(typeof(KalturaAdsPolicy), AdsPolicy) + "\"" + "</adsPolicy>";
            }
            if(Drm != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Drm.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<drm>" + propertyValue + "</drm>";
            }
            if(Format != null)
            {
                ret += "<format>" + EscapeXml(Format) + "</format>";
            }
            if(Protocols != null)
            {
                ret += "<protocols>" + EscapeXml(Protocols) + "</protocols>";
            }
            return ret;
        }
    }
    public partial class KalturaPluginData
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaPpvEntitlement
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(MediaFileId.HasValue)
            {
                ret.Add("\"mediaFileId\": " + MediaFileId);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"media_file_id\": " + MediaFileId);
                }
            }
            if(MediaId.HasValue)
            {
                ret.Add("\"mediaId\": " + MediaId);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"media_id\": " + MediaId);
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(MediaFileId.HasValue)
            {
                ret += "<mediaFileId>" + MediaFileId + "</mediaFileId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<media_file_id>" + MediaFileId + "</media_file_id>";
                }
            }
            if(MediaId.HasValue)
            {
                ret += "<mediaId>" + MediaId + "</mediaId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<media_id>" + MediaId + "</media_id>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaPremiumService
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            return ret;
        }
    }
    public partial class KalturaPricesFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(FilesIds != null)
            {
                propertyValue = "[" + String.Join(", ", FilesIds.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"filesIds\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"files_ids\": " + propertyValue);
                }
            }
            if(ShouldGetOnlyLowest.HasValue)
            {
                ret.Add("\"shouldGetOnlyLowest\": " + ShouldGetOnlyLowest.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"should_get_only_lowest\": " + ShouldGetOnlyLowest.ToString().ToLower());
                }
            }
            if(SubscriptionsIds != null)
            {
                propertyValue = "[" + String.Join(", ", SubscriptionsIds.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"subscriptionsIds\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"subscriptions_ids\": " + propertyValue);
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(FilesIds != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", FilesIds.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<filesIds>" + propertyValue + "</filesIds>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<files_ids>" + propertyValue + "</files_ids>";
                }
            }
            if(ShouldGetOnlyLowest.HasValue)
            {
                ret += "<shouldGetOnlyLowest>" + ShouldGetOnlyLowest.ToString().ToLower() + "</shouldGetOnlyLowest>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<should_get_only_lowest>" + ShouldGetOnlyLowest.ToString().ToLower() + "</should_get_only_lowest>";
                }
            }
            if(SubscriptionsIds != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", SubscriptionsIds.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<subscriptionsIds>" + propertyValue + "</subscriptionsIds>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<subscriptions_ids>" + propertyValue + "</subscriptions_ids>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaProductPriceFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(CollectionIdIn != null)
            {
                ret.Add("\"collectionIdIn\": " + "\"" + EscapeJson(CollectionIdIn) + "\"");
            }
            if(CouponCodeEqual != null)
            {
                ret.Add("\"couponCodeEqual\": " + "\"" + EscapeJson(CouponCodeEqual) + "\"");
            }
            if(FileIdIn != null)
            {
                ret.Add("\"fileIdIn\": " + "\"" + EscapeJson(FileIdIn) + "\"");
            }
            if(isLowest.HasValue)
            {
                ret.Add("\"isLowest\": " + isLowest.ToString().ToLower());
            }
            if(SubscriptionIdIn != null)
            {
                ret.Add("\"subscriptionIdIn\": " + "\"" + EscapeJson(SubscriptionIdIn) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(CollectionIdIn != null)
            {
                ret += "<collectionIdIn>" + EscapeXml(CollectionIdIn) + "</collectionIdIn>";
            }
            if(CouponCodeEqual != null)
            {
                ret += "<couponCodeEqual>" + EscapeXml(CouponCodeEqual) + "</couponCodeEqual>";
            }
            if(FileIdIn != null)
            {
                ret += "<fileIdIn>" + EscapeXml(FileIdIn) + "</fileIdIn>";
            }
            if(isLowest.HasValue)
            {
                ret += "<isLowest>" + isLowest.ToString().ToLower() + "</isLowest>";
            }
            if(SubscriptionIdIn != null)
            {
                ret += "<subscriptionIdIn>" + EscapeXml(SubscriptionIdIn) + "</subscriptionIdIn>";
            }
            return ret;
        }
    }
    public partial class KalturaPurchase
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AdapterData != null)
            {
                ret.Add("\"adapterData\": " + "\"" + EscapeJson(AdapterData) + "\"");
            }
            if(Coupon != null)
            {
                ret.Add("\"coupon\": " + "\"" + EscapeJson(Coupon) + "\"");
            }
            if(Currency != null)
            {
                ret.Add("\"currency\": " + "\"" + EscapeJson(Currency) + "\"");
            }
            if(PaymentGatewayId.HasValue)
            {
                ret.Add("\"paymentGatewayId\": " + PaymentGatewayId);
            }
            if(PaymentMethodId.HasValue)
            {
                ret.Add("\"paymentMethodId\": " + PaymentMethodId);
            }
            ret.Add("\"price\": " + Price);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AdapterData != null)
            {
                ret += "<adapterData>" + EscapeXml(AdapterData) + "</adapterData>";
            }
            if(Coupon != null)
            {
                ret += "<coupon>" + EscapeXml(Coupon) + "</coupon>";
            }
            if(Currency != null)
            {
                ret += "<currency>" + EscapeXml(Currency) + "</currency>";
            }
            if(PaymentGatewayId.HasValue)
            {
                ret += "<paymentGatewayId>" + PaymentGatewayId + "</paymentGatewayId>";
            }
            if(PaymentMethodId.HasValue)
            {
                ret += "<paymentMethodId>" + PaymentMethodId + "</paymentMethodId>";
            }
            ret += "<price>" + Price + "</price>";
            return ret;
        }
    }
    public partial class KalturaPurchaseBase
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ContentId.HasValue)
            {
                ret.Add("\"contentId\": " + ContentId);
            }
            ret.Add("\"productId\": " + ProductId);
            ret.Add("\"productType\": " + "\"" + Enum.GetName(typeof(KalturaTransactionType), ProductType) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ContentId.HasValue)
            {
                ret += "<contentId>" + ContentId + "</contentId>";
            }
            ret += "<productId>" + ProductId + "</productId>";
            ret += "<productType>" + "\"" + Enum.GetName(typeof(KalturaTransactionType), ProductType) + "\"" + "</productType>";
            return ret;
        }
    }
    public partial class KalturaPurchaseSession
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(PreviewModuleId.HasValue)
            {
                ret.Add("\"previewModuleId\": " + PreviewModuleId);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(PreviewModuleId.HasValue)
            {
                ret += "<previewModuleId>" + PreviewModuleId + "</previewModuleId>";
            }
            return ret;
        }
    }
    public partial class KalturaRecording
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"assetId\": " + AssetId);
            ret.Add("\"createDate\": " + CreateDate);
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            ret.Add("\"isProtected\": " + IsProtected.ToString().ToLower());
            ret.Add("\"status\": " + "\"" + Enum.GetName(typeof(KalturaRecordingStatus), Status) + "\"");
            ret.Add("\"type\": " + "\"" + Enum.GetName(typeof(KalturaRecordingType), Type) + "\"");
            ret.Add("\"updateDate\": " + UpdateDate);
            if(ViewableUntilDate.HasValue)
            {
                ret.Add("\"viewableUntilDate\": " + ViewableUntilDate);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<assetId>" + AssetId + "</assetId>";
            ret += "<createDate>" + CreateDate + "</createDate>";
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            ret += "<isProtected>" + IsProtected.ToString().ToLower() + "</isProtected>";
            ret += "<status>" + "\"" + Enum.GetName(typeof(KalturaRecordingStatus), Status) + "\"" + "</status>";
            ret += "<type>" + "\"" + Enum.GetName(typeof(KalturaRecordingType), Type) + "\"" + "</type>";
            ret += "<updateDate>" + UpdateDate + "</updateDate>";
            if(ViewableUntilDate.HasValue)
            {
                ret += "<viewableUntilDate>" + ViewableUntilDate + "</viewableUntilDate>";
            }
            return ret;
        }
    }
    public partial class KalturaRecordingContext
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"assetId\": " + AssetId);
            ret.Add("\"code\": " + Code);
            if(Message != null)
            {
                ret.Add("\"message\": " + "\"" + EscapeJson(Message) + "\"");
            }
            if(Recording != null)
            {
                propertyValue = Recording.ToJson(currentVersion, omitObsolete);
                ret.Add("\"recording\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<assetId>" + AssetId + "</assetId>";
            ret += "<code>" + Code + "</code>";
            if(Message != null)
            {
                ret += "<message>" + EscapeXml(Message) + "</message>";
            }
            if(Recording != null)
            {
                propertyValue = Recording.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<recording>" + propertyValue + "</recording>";
            }
            return ret;
        }
    }
    public partial class KalturaRecordingContextFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetIdIn != null)
            {
                ret.Add("\"assetIdIn\": " + "\"" + EscapeJson(AssetIdIn) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetIdIn != null)
            {
                ret += "<assetIdIn>" + EscapeXml(AssetIdIn) + "</assetIdIn>";
            }
            return ret;
        }
    }
    public partial class KalturaRecordingContextListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaRecordingFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(FilterExpression != null)
            {
                ret.Add("\"filterExpression\": " + "\"" + EscapeJson(FilterExpression) + "\"");
            }
            if(StatusIn != null)
            {
                ret.Add("\"statusIn\": " + "\"" + EscapeJson(StatusIn) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(FilterExpression != null)
            {
                ret += "<filterExpression>" + EscapeXml(FilterExpression) + "</filterExpression>";
            }
            if(StatusIn != null)
            {
                ret += "<statusIn>" + EscapeXml(StatusIn) + "</statusIn>";
            }
            return ret;
        }
    }
    public partial class KalturaRecordingListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaRuleAction
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Description != null)
            {
                ret.Add("\"description\": " + "\"" + EscapeJson(Description) + "\"");
            }
            ret.Add("\"type\": " + "\"" + Enum.GetName(typeof(KalturaRuleActionType), Type) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Description != null)
            {
                ret += "<description>" + EscapeXml(Description) + "</description>";
            }
            ret += "<type>" + "\"" + Enum.GetName(typeof(KalturaRuleActionType), Type) + "\"" + "</type>";
            return ret;
        }
    }
    public partial class KalturaSeriesRecording
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"channelId\": " + ChannelId);
            ret.Add("\"createDate\": " + CreateDate);
            ret.Add("\"epgId\": " + EpgId);
            if(ExcludedSeasons != null)
            {
                propertyValue = "[" + String.Join(", ", ExcludedSeasons.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"excludedSeasons\": " + propertyValue);
            }
            ret.Add("\"id\": " + Id);
            if(SeasonNumber.HasValue)
            {
                ret.Add("\"seasonNumber\": " + SeasonNumber);
            }
            if(SeriesId != null)
            {
                ret.Add("\"seriesId\": " + "\"" + EscapeJson(SeriesId) + "\"");
            }
            ret.Add("\"type\": " + "\"" + Enum.GetName(typeof(KalturaRecordingType), Type) + "\"");
            ret.Add("\"updateDate\": " + UpdateDate);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<channelId>" + ChannelId + "</channelId>";
            ret += "<createDate>" + CreateDate + "</createDate>";
            ret += "<epgId>" + EpgId + "</epgId>";
            if(ExcludedSeasons != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", ExcludedSeasons.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<excludedSeasons>" + propertyValue + "</excludedSeasons>";
            }
            ret += "<id>" + Id + "</id>";
            if(SeasonNumber.HasValue)
            {
                ret += "<seasonNumber>" + SeasonNumber + "</seasonNumber>";
            }
            if(SeriesId != null)
            {
                ret += "<seriesId>" + EscapeXml(SeriesId) + "</seriesId>";
            }
            ret += "<type>" + "\"" + Enum.GetName(typeof(KalturaRecordingType), Type) + "\"" + "</type>";
            ret += "<updateDate>" + UpdateDate + "</updateDate>";
            return ret;
        }
    }
    public partial class KalturaSeriesRecordingFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaSeriesRecordingListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaStartDateOffsetRuleAction
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaSubscriptionEntitlement
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(IsInGracePeriod.HasValue)
            {
                ret.Add("\"isInGracePeriod\": " + IsInGracePeriod.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"is_in_grace_period\": " + IsInGracePeriod.ToString().ToLower());
                }
            }
            if(IsRenewable.HasValue)
            {
                ret.Add("\"isRenewable\": " + IsRenewable.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"is_renewable\": " + IsRenewable.ToString().ToLower());
                }
            }
            if(IsRenewableForPurchase.HasValue)
            {
                ret.Add("\"isRenewableForPurchase\": " + IsRenewableForPurchase.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"is_renewable_for_purchase\": " + IsRenewableForPurchase.ToString().ToLower());
                }
            }
            ret.Add("\"isSuspended\": " + IsSuspended.ToString().ToLower());
            if(NextRenewalDate.HasValue)
            {
                ret.Add("\"nextRenewalDate\": " + NextRenewalDate);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"next_renewal_date\": " + NextRenewalDate);
                }
            }
            if(PaymentGatewayId.HasValue)
            {
                ret.Add("\"paymentGatewayId\": " + PaymentGatewayId);
            }
            if(PaymentMethodId.HasValue)
            {
                ret.Add("\"paymentMethodId\": " + PaymentMethodId);
            }
            if(ScheduledSubscriptionId.HasValue)
            {
                ret.Add("\"scheduledSubscriptionId\": " + ScheduledSubscriptionId);
            }
            if(UnifiedPaymentId.HasValue)
            {
                ret.Add("\"unifiedPaymentId\": " + UnifiedPaymentId);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(IsInGracePeriod.HasValue)
            {
                ret += "<isInGracePeriod>" + IsInGracePeriod.ToString().ToLower() + "</isInGracePeriod>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<is_in_grace_period>" + IsInGracePeriod.ToString().ToLower() + "</is_in_grace_period>";
                }
            }
            if(IsRenewable.HasValue)
            {
                ret += "<isRenewable>" + IsRenewable.ToString().ToLower() + "</isRenewable>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<is_renewable>" + IsRenewable.ToString().ToLower() + "</is_renewable>";
                }
            }
            if(IsRenewableForPurchase.HasValue)
            {
                ret += "<isRenewableForPurchase>" + IsRenewableForPurchase.ToString().ToLower() + "</isRenewableForPurchase>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<is_renewable_for_purchase>" + IsRenewableForPurchase.ToString().ToLower() + "</is_renewable_for_purchase>";
                }
            }
            ret += "<isSuspended>" + IsSuspended.ToString().ToLower() + "</isSuspended>";
            if(NextRenewalDate.HasValue)
            {
                ret += "<nextRenewalDate>" + NextRenewalDate + "</nextRenewalDate>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<next_renewal_date>" + NextRenewalDate + "</next_renewal_date>";
                }
            }
            if(PaymentGatewayId.HasValue)
            {
                ret += "<paymentGatewayId>" + PaymentGatewayId + "</paymentGatewayId>";
            }
            if(PaymentMethodId.HasValue)
            {
                ret += "<paymentMethodId>" + PaymentMethodId + "</paymentMethodId>";
            }
            if(ScheduledSubscriptionId.HasValue)
            {
                ret += "<scheduledSubscriptionId>" + ScheduledSubscriptionId + "</scheduledSubscriptionId>";
            }
            if(UnifiedPaymentId.HasValue)
            {
                ret += "<unifiedPaymentId>" + UnifiedPaymentId + "</unifiedPaymentId>";
            }
            return ret;
        }
    }
    public partial class KalturaTimeOffsetRuleAction
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"offset\": " + Offset);
            ret.Add("\"timeZone\": " + TimeZone.ToString().ToLower());
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<offset>" + Offset + "</offset>";
            ret += "<timeZone>" + TimeZone.ToString().ToLower() + "</timeZone>";
            return ret;
        }
    }
    public partial class KalturaTransaction
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(CreatedAt.HasValue)
            {
                ret.Add("\"createdAt\": " + CreatedAt);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"created_at\": " + CreatedAt);
                }
            }
            if(FailReasonCode.HasValue)
            {
                ret.Add("\"failReasonCode\": " + FailReasonCode);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"fail_reason_code\": " + FailReasonCode);
                }
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + EscapeJson(Id) + "\"");
            }
            if(PGReferenceID != null)
            {
                ret.Add("\"paymentGatewayReferenceId\": " + "\"" + EscapeJson(PGReferenceID) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"payment_gateway_reference_id\": " + "\"" + EscapeJson(PGReferenceID) + "\"");
                }
            }
            if(PGResponseID != null)
            {
                ret.Add("\"paymentGatewayResponseId\": " + "\"" + EscapeJson(PGResponseID) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"payment_gateway_response_id\": " + "\"" + EscapeJson(PGResponseID) + "\"");
                }
            }
            if(State != null)
            {
                ret.Add("\"state\": " + "\"" + EscapeJson(State) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(CreatedAt.HasValue)
            {
                ret += "<createdAt>" + CreatedAt + "</createdAt>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<created_at>" + CreatedAt + "</created_at>";
                }
            }
            if(FailReasonCode.HasValue)
            {
                ret += "<failReasonCode>" + FailReasonCode + "</failReasonCode>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<fail_reason_code>" + FailReasonCode + "</fail_reason_code>";
                }
            }
            if(Id != null)
            {
                ret += "<id>" + EscapeXml(Id) + "</id>";
            }
            if(PGReferenceID != null)
            {
                ret += "<paymentGatewayReferenceId>" + EscapeXml(PGReferenceID) + "</paymentGatewayReferenceId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<payment_gateway_reference_id>" + EscapeXml(PGReferenceID) + "</payment_gateway_reference_id>";
                }
            }
            if(PGResponseID != null)
            {
                ret += "<paymentGatewayResponseId>" + EscapeXml(PGResponseID) + "</paymentGatewayResponseId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<payment_gateway_response_id>" + EscapeXml(PGResponseID) + "</payment_gateway_response_id>";
                }
            }
            if(State != null)
            {
                ret += "<state>" + EscapeXml(State) + "</state>";
            }
            return ret;
        }
    }
    public partial class KalturaTransactionHistoryFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(EndDateLessThanOrEqual.HasValue)
            {
                ret.Add("\"endDateLessThanOrEqual\": " + EndDateLessThanOrEqual);
            }
            ret.Add("\"entityReferenceEqual\": " + "\"" + Enum.GetName(typeof(KalturaEntityReferenceBy), EntityReferenceEqual) + "\"");
            if(StartDateGreaterThanOrEqual.HasValue)
            {
                ret.Add("\"startDateGreaterThanOrEqual\": " + StartDateGreaterThanOrEqual);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(EndDateLessThanOrEqual.HasValue)
            {
                ret += "<endDateLessThanOrEqual>" + EndDateLessThanOrEqual + "</endDateLessThanOrEqual>";
            }
            ret += "<entityReferenceEqual>" + "\"" + Enum.GetName(typeof(KalturaEntityReferenceBy), EntityReferenceEqual) + "\"" + "</entityReferenceEqual>";
            if(StartDateGreaterThanOrEqual.HasValue)
            {
                ret += "<startDateGreaterThanOrEqual>" + StartDateGreaterThanOrEqual + "</startDateGreaterThanOrEqual>";
            }
            return ret;
        }
    }
    public partial class KalturaTransactionsFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"by\": " + "\"" + Enum.GetName(typeof(KalturaEntityReferenceBy), By) + "\"");
            if(EndDate.HasValue)
            {
                ret.Add("\"endDate\": " + EndDate);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"end_date\": " + EndDate);
                }
            }
            if(StartDate.HasValue)
            {
                ret.Add("\"startDate\": " + StartDate);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"start_date\": " + StartDate);
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<by>" + "\"" + Enum.GetName(typeof(KalturaEntityReferenceBy), By) + "\"" + "</by>";
            if(EndDate.HasValue)
            {
                ret += "<endDate>" + EndDate + "</endDate>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<end_date>" + EndDate + "</end_date>";
                }
            }
            if(StartDate.HasValue)
            {
                ret += "<startDate>" + StartDate + "</startDate>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<start_date>" + StartDate + "</start_date>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaTransactionStatus
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"adapterTransactionStatus\": " + "\"" + Enum.GetName(typeof(KalturaTransactionAdapterStatus), AdapterStatus) + "\"");
            if(ExternalId != null)
            {
                ret.Add("\"externalId\": " + "\"" + EscapeJson(ExternalId) + "\"");
            }
            if(ExternalMessage != null)
            {
                ret.Add("\"externalMessage\": " + "\"" + EscapeJson(ExternalMessage) + "\"");
            }
            if(ExternalStatus != null)
            {
                ret.Add("\"externalStatus\": " + "\"" + EscapeJson(ExternalStatus) + "\"");
            }
            ret.Add("\"failReason\": " + FailReason);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<adapterTransactionStatus>" + "\"" + Enum.GetName(typeof(KalturaTransactionAdapterStatus), AdapterStatus) + "\"" + "</adapterTransactionStatus>";
            if(ExternalId != null)
            {
                ret += "<externalId>" + EscapeXml(ExternalId) + "</externalId>";
            }
            if(ExternalMessage != null)
            {
                ret += "<externalMessage>" + EscapeXml(ExternalMessage) + "</externalMessage>";
            }
            if(ExternalStatus != null)
            {
                ret += "<externalStatus>" + EscapeXml(ExternalStatus) + "</externalStatus>";
            }
            ret += "<failReason>" + FailReason + "</failReason>";
            return ret;
        }
    }
    public partial class KalturaUnifiedPaymentRenewal
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"date\": " + Date);
            if(Entitlements != null)
            {
                propertyValue = "[" + String.Join(", ", Entitlements.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"entitlements\": " + propertyValue);
            }
            if(Price != null)
            {
                propertyValue = Price.ToJson(currentVersion, omitObsolete);
                ret.Add("\"price\": " + propertyValue);
            }
            ret.Add("\"unifiedPaymentId\": " + UnifiedPaymentId);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<date>" + Date + "</date>";
            if(Entitlements != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Entitlements.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<entitlements>" + propertyValue + "</entitlements>";
            }
            if(Price != null)
            {
                propertyValue = Price.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<price>" + propertyValue + "</price>";
            }
            ret += "<unifiedPaymentId>" + UnifiedPaymentId + "</unifiedPaymentId>";
            return ret;
        }
    }
    public partial class KalturaUserBillingTransaction
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(UserFullName != null)
            {
                ret.Add("\"userFullName\": " + "\"" + EscapeJson(UserFullName) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"user_full_name\": " + "\"" + EscapeJson(UserFullName) + "\"");
                }
            }
            if(UserID != null)
            {
                ret.Add("\"userId\": " + "\"" + EscapeJson(UserID) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"user_id\": " + "\"" + EscapeJson(UserID) + "\"");
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(UserFullName != null)
            {
                ret += "<userFullName>" + EscapeXml(UserFullName) + "</userFullName>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<user_full_name>" + EscapeXml(UserFullName) + "</user_full_name>";
                }
            }
            if(UserID != null)
            {
                ret += "<userId>" + EscapeXml(UserID) + "</userId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<user_id>" + EscapeXml(UserID) + "</user_id>";
                }
            }
            return ret;
        }
    }
}

namespace WebAPI.Models.Social
{
    public partial class KalturaActionPermissionItem
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Action != null)
            {
                ret.Add("\"action\": " + "\"" + EscapeJson(Action) + "\"");
            }
            ret.Add("\"actionPrivacy\": " + "\"" + Enum.GetName(typeof(KalturaSocialActionPrivacy), ActionPrivacy) + "\"");
            if(Network.HasValue)
            {
                ret.Add("\"network\": " + "\"" + Enum.GetName(typeof(KalturaSocialNetwork), Network) + "\"");
            }
            ret.Add("\"privacy\": " + "\"" + Enum.GetName(typeof(KalturaSocialPrivacy), Privacy) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Action != null)
            {
                ret += "<action>" + EscapeXml(Action) + "</action>";
            }
            ret += "<actionPrivacy>" + "\"" + Enum.GetName(typeof(KalturaSocialActionPrivacy), ActionPrivacy) + "\"" + "</actionPrivacy>";
            if(Network.HasValue)
            {
                ret += "<network>" + "\"" + Enum.GetName(typeof(KalturaSocialNetwork), Network) + "\"" + "</network>";
            }
            ret += "<privacy>" + "\"" + Enum.GetName(typeof(KalturaSocialPrivacy), Privacy) + "\"" + "</privacy>";
            return ret;
        }
    }
    public partial class KalturaFacebookPost
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Comments != null)
            {
                propertyValue = "[" + String.Join(", ", Comments.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"comments\": " + propertyValue);
            }
            if(Link != null)
            {
                ret.Add("\"link\": " + "\"" + EscapeJson(Link) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Comments != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Comments.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<comments>" + propertyValue + "</comments>";
            }
            if(Link != null)
            {
                ret += "<link>" + EscapeXml(Link) + "</link>";
            }
            return ret;
        }
    }
    public partial class KalturaFacebookSocial
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaNetworkActionStatus
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Network.HasValue)
            {
                ret.Add("\"network\": " + "\"" + Enum.GetName(typeof(KalturaSocialNetwork), Network) + "\"");
            }
            ret.Add("\"status\": " + "\"" + Enum.GetName(typeof(KalturaSocialStatus), Status) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Network.HasValue)
            {
                ret += "<network>" + "\"" + Enum.GetName(typeof(KalturaSocialNetwork), Network) + "\"" + "</network>";
            }
            ret += "<status>" + "\"" + Enum.GetName(typeof(KalturaSocialStatus), Status) + "\"" + "</status>";
            return ret;
        }
    }
    public partial class KalturaSocial
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Birthday != null)
            {
                ret.Add("\"birthday\": " + "\"" + EscapeJson(Birthday) + "\"");
            }
            if(Email != null)
            {
                ret.Add("\"email\": " + "\"" + EscapeJson(Email) + "\"");
            }
            if(FirstName != null)
            {
                ret.Add("\"firstName\": " + "\"" + EscapeJson(FirstName) + "\"");
            }
            if(Gender != null)
            {
                ret.Add("\"gender\": " + "\"" + EscapeJson(Gender) + "\"");
            }
            if(ID != null)
            {
                ret.Add("\"id\": " + "\"" + EscapeJson(ID) + "\"");
            }
            if(LastName != null)
            {
                ret.Add("\"lastName\": " + "\"" + EscapeJson(LastName) + "\"");
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(PictureUrl != null)
            {
                ret.Add("\"pictureUrl\": " + "\"" + EscapeJson(PictureUrl) + "\"");
            }
            if(Status != null)
            {
                ret.Add("\"status\": " + "\"" + EscapeJson(Status) + "\"");
            }
            if(UserId != null)
            {
                ret.Add("\"userId\": " + "\"" + EscapeJson(UserId) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Birthday != null)
            {
                ret += "<birthday>" + EscapeXml(Birthday) + "</birthday>";
            }
            if(Email != null)
            {
                ret += "<email>" + EscapeXml(Email) + "</email>";
            }
            if(FirstName != null)
            {
                ret += "<firstName>" + EscapeXml(FirstName) + "</firstName>";
            }
            if(Gender != null)
            {
                ret += "<gender>" + EscapeXml(Gender) + "</gender>";
            }
            if(ID != null)
            {
                ret += "<id>" + EscapeXml(ID) + "</id>";
            }
            if(LastName != null)
            {
                ret += "<lastName>" + EscapeXml(LastName) + "</lastName>";
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(PictureUrl != null)
            {
                ret += "<pictureUrl>" + EscapeXml(PictureUrl) + "</pictureUrl>";
            }
            if(Status != null)
            {
                ret += "<status>" + EscapeXml(Status) + "</status>";
            }
            if(UserId != null)
            {
                ret += "<userId>" + EscapeXml(UserId) + "</userId>";
            }
            return ret;
        }
    }
    public partial class KalturaSocialAction
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"actionType\": " + "\"" + Enum.GetName(typeof(KalturaSocialActionType), ActionType) + "\"");
            if(AssetId.HasValue)
            {
                ret.Add("\"assetId\": " + AssetId);
            }
            ret.Add("\"assetType\": " + "\"" + Enum.GetName(typeof(KalturaAssetType), AssetType) + "\"");
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + EscapeJson(Id) + "\"");
            }
            if(Time.HasValue)
            {
                ret.Add("\"time\": " + Time);
            }
            if(Url != null)
            {
                ret.Add("\"url\": " + "\"" + EscapeJson(Url) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<actionType>" + "\"" + Enum.GetName(typeof(KalturaSocialActionType), ActionType) + "\"" + "</actionType>";
            if(AssetId.HasValue)
            {
                ret += "<assetId>" + AssetId + "</assetId>";
            }
            ret += "<assetType>" + "\"" + Enum.GetName(typeof(KalturaAssetType), AssetType) + "\"" + "</assetType>";
            if(Id != null)
            {
                ret += "<id>" + EscapeXml(Id) + "</id>";
            }
            if(Time.HasValue)
            {
                ret += "<time>" + Time + "</time>";
            }
            if(Url != null)
            {
                ret += "<url>" + EscapeXml(Url) + "</url>";
            }
            return ret;
        }
    }
    public partial class KalturaSocialActionFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ActionTypeIn != null)
            {
                ret.Add("\"actionTypeIn\": " + "\"" + EscapeJson(ActionTypeIn) + "\"");
            }
            if(AssetIdIn != null)
            {
                ret.Add("\"assetIdIn\": " + "\"" + EscapeJson(AssetIdIn) + "\"");
            }
            ret.Add("\"assetTypeEqual\": " + "\"" + Enum.GetName(typeof(KalturaAssetType), AssetTypeEqual) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ActionTypeIn != null)
            {
                ret += "<actionTypeIn>" + EscapeXml(ActionTypeIn) + "</actionTypeIn>";
            }
            if(AssetIdIn != null)
            {
                ret += "<assetIdIn>" + EscapeXml(AssetIdIn) + "</assetIdIn>";
            }
            ret += "<assetTypeEqual>" + "\"" + Enum.GetName(typeof(KalturaAssetType), AssetTypeEqual) + "\"" + "</assetTypeEqual>";
            return ret;
        }
    }
    public partial class KalturaSocialActionListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaSocialActionRate
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"rate\": " + Rate);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<rate>" + Rate + "</rate>";
            return ret;
        }
    }
    public partial class KalturaSocialComment
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"createDate\": " + CreateDate);
            if(Header != null)
            {
                ret.Add("\"header\": " + "\"" + EscapeJson(Header) + "\"");
            }
            if(Text != null)
            {
                ret.Add("\"text\": " + "\"" + EscapeJson(Text) + "\"");
            }
            if(Writer != null)
            {
                ret.Add("\"writer\": " + "\"" + EscapeJson(Writer) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<createDate>" + CreateDate + "</createDate>";
            if(Header != null)
            {
                ret += "<header>" + EscapeXml(Header) + "</header>";
            }
            if(Text != null)
            {
                ret += "<text>" + EscapeXml(Text) + "</text>";
            }
            if(Writer != null)
            {
                ret += "<writer>" + EscapeXml(Writer) + "</writer>";
            }
            return ret;
        }
    }
    public partial class KalturaSocialCommentFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"assetIdEqual\": " + AssetIdEqual);
            ret.Add("\"assetTypeEqual\": " + "\"" + Enum.GetName(typeof(KalturaAssetType), AssetTypeEqual) + "\"");
            ret.Add("\"createDateGreaterThan\": " + CreateDateGreaterThan);
            ret.Add("\"socialPlatformEqual\": " + "\"" + Enum.GetName(typeof(KalturaSocialPlatform), SocialPlatformEqual) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<assetIdEqual>" + AssetIdEqual + "</assetIdEqual>";
            ret += "<assetTypeEqual>" + "\"" + Enum.GetName(typeof(KalturaAssetType), AssetTypeEqual) + "\"" + "</assetTypeEqual>";
            ret += "<createDateGreaterThan>" + CreateDateGreaterThan + "</createDateGreaterThan>";
            ret += "<socialPlatformEqual>" + "\"" + Enum.GetName(typeof(KalturaSocialPlatform), SocialPlatformEqual) + "\"" + "</socialPlatformEqual>";
            return ret;
        }
    }
    public partial class KalturaSocialCommentListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaSocialConfig
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaSocialFacebookConfig
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AppId != null)
            {
                ret.Add("\"appId\": " + "\"" + EscapeJson(AppId) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"app_id\": " + "\"" + EscapeJson(AppId) + "\"");
                }
            }
            if(Permissions != null)
            {
                ret.Add("\"permissions\": " + "\"" + EscapeJson(Permissions) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AppId != null)
            {
                ret += "<appId>" + EscapeXml(AppId) + "</appId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<app_id>" + EscapeXml(AppId) + "</app_id>";
                }
            }
            if(Permissions != null)
            {
                ret += "<permissions>" + EscapeXml(Permissions) + "</permissions>";
            }
            return ret;
        }
    }
    public partial class KalturaSocialFriendActivity
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(SocialAction != null)
            {
                propertyValue = SocialAction.ToJson(currentVersion, omitObsolete);
                ret.Add("\"socialAction\": " + propertyValue);
            }
            if(UserFullName != null)
            {
                ret.Add("\"userFullName\": " + "\"" + EscapeJson(UserFullName) + "\"");
            }
            if(UserPictureUrl != null)
            {
                ret.Add("\"userPictureUrl\": " + "\"" + EscapeJson(UserPictureUrl) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(SocialAction != null)
            {
                propertyValue = SocialAction.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<socialAction>" + propertyValue + "</socialAction>";
            }
            if(UserFullName != null)
            {
                ret += "<userFullName>" + EscapeXml(UserFullName) + "</userFullName>";
            }
            if(UserPictureUrl != null)
            {
                ret += "<userPictureUrl>" + EscapeXml(UserPictureUrl) + "</userPictureUrl>";
            }
            return ret;
        }
    }
    public partial class KalturaSocialFriendActivityFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ActionTypeIn != null)
            {
                ret.Add("\"actionTypeIn\": " + "\"" + EscapeJson(ActionTypeIn) + "\"");
            }
            if(AssetIdEqual.HasValue)
            {
                ret.Add("\"assetIdEqual\": " + AssetIdEqual);
            }
            if(AssetTypeEqual.HasValue)
            {
                ret.Add("\"assetTypeEqual\": " + "\"" + Enum.GetName(typeof(KalturaAssetType), AssetTypeEqual) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ActionTypeIn != null)
            {
                ret += "<actionTypeIn>" + EscapeXml(ActionTypeIn) + "</actionTypeIn>";
            }
            if(AssetIdEqual.HasValue)
            {
                ret += "<assetIdEqual>" + AssetIdEqual + "</assetIdEqual>";
            }
            if(AssetTypeEqual.HasValue)
            {
                ret += "<assetTypeEqual>" + "\"" + Enum.GetName(typeof(KalturaAssetType), AssetTypeEqual) + "\"" + "</assetTypeEqual>";
            }
            return ret;
        }
    }
    public partial class KalturaSocialFriendActivityListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaSocialNetworkComment
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AuthorImageUrl != null)
            {
                ret.Add("\"authorImageUrl\": " + "\"" + EscapeJson(AuthorImageUrl) + "\"");
            }
            if(LikeCounter != null)
            {
                ret.Add("\"likeCounter\": " + "\"" + EscapeJson(LikeCounter) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AuthorImageUrl != null)
            {
                ret += "<authorImageUrl>" + EscapeXml(AuthorImageUrl) + "</authorImageUrl>";
            }
            if(LikeCounter != null)
            {
                ret += "<likeCounter>" + EscapeXml(LikeCounter) + "</likeCounter>";
            }
            return ret;
        }
    }
    public partial class KalturaSocialResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Data != null)
            {
                ret.Add("\"data\": " + "\"" + EscapeJson(Data) + "\"");
            }
            if(KalturaName != null)
            {
                ret.Add("\"kalturaUsername\": " + "\"" + EscapeJson(KalturaName) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"kaltura_username\": " + "\"" + EscapeJson(KalturaName) + "\"");
                }
            }
            if(MinFriends != null)
            {
                ret.Add("\"minFriendsLimitation\": " + "\"" + EscapeJson(MinFriends) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"min_friends_limitation\": " + "\"" + EscapeJson(MinFriends) + "\"");
                }
            }
            if(Pic != null)
            {
                ret.Add("\"pic\": " + "\"" + EscapeJson(Pic) + "\"");
            }
            if(SocialNetworkUsername != null)
            {
                ret.Add("\"socialUsername\": " + "\"" + EscapeJson(SocialNetworkUsername) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"social_username\": " + "\"" + EscapeJson(SocialNetworkUsername) + "\"");
                }
            }
            if(SocialUser != null)
            {
                propertyValue = SocialUser.ToJson(currentVersion, omitObsolete);
                ret.Add("\"socialUser\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"social_user\": " + propertyValue);
                }
            }
            if(Status != null)
            {
                ret.Add("\"status\": " + "\"" + EscapeJson(Status) + "\"");
            }
            if(Token != null)
            {
                ret.Add("\"token\": " + "\"" + EscapeJson(Token) + "\"");
            }
            if(UserId != null)
            {
                ret.Add("\"userId\": " + "\"" + EscapeJson(UserId) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"user_id\": " + "\"" + EscapeJson(UserId) + "\"");
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Data != null)
            {
                ret += "<data>" + EscapeXml(Data) + "</data>";
            }
            if(KalturaName != null)
            {
                ret += "<kalturaUsername>" + EscapeXml(KalturaName) + "</kalturaUsername>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<kaltura_username>" + EscapeXml(KalturaName) + "</kaltura_username>";
                }
            }
            if(MinFriends != null)
            {
                ret += "<minFriendsLimitation>" + EscapeXml(MinFriends) + "</minFriendsLimitation>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<min_friends_limitation>" + EscapeXml(MinFriends) + "</min_friends_limitation>";
                }
            }
            if(Pic != null)
            {
                ret += "<pic>" + EscapeXml(Pic) + "</pic>";
            }
            if(SocialNetworkUsername != null)
            {
                ret += "<socialUsername>" + EscapeXml(SocialNetworkUsername) + "</socialUsername>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<social_username>" + EscapeXml(SocialNetworkUsername) + "</social_username>";
                }
            }
            if(SocialUser != null)
            {
                propertyValue = SocialUser.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<socialUser>" + propertyValue + "</socialUser>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<social_user>" + propertyValue + "</social_user>";
                }
            }
            if(Status != null)
            {
                ret += "<status>" + EscapeXml(Status) + "</status>";
            }
            if(Token != null)
            {
                ret += "<token>" + EscapeXml(Token) + "</token>";
            }
            if(UserId != null)
            {
                ret += "<userId>" + EscapeXml(UserId) + "</userId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<user_id>" + EscapeXml(UserId) + "</user_id>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaSocialUser
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Birthday != null)
            {
                ret.Add("\"birthday\": " + "\"" + EscapeJson(Birthday) + "\"");
            }
            if(Email != null)
            {
                ret.Add("\"email\": " + "\"" + EscapeJson(Email) + "\"");
            }
            if(FirstName != null)
            {
                ret.Add("\"firstName\": " + "\"" + EscapeJson(FirstName) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"first_name\": " + "\"" + EscapeJson(FirstName) + "\"");
                }
            }
            if(Gender != null)
            {
                ret.Add("\"gender\": " + "\"" + EscapeJson(Gender) + "\"");
            }
            if(ID != null)
            {
                ret.Add("\"id\": " + "\"" + EscapeJson(ID) + "\"");
            }
            if(LastName != null)
            {
                ret.Add("\"lastName\": " + "\"" + EscapeJson(LastName) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"last_name\": " + "\"" + EscapeJson(LastName) + "\"");
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(UserId != null)
            {
                ret.Add("\"userId\": " + "\"" + EscapeJson(UserId) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"user_id\": " + "\"" + EscapeJson(UserId) + "\"");
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Birthday != null)
            {
                ret += "<birthday>" + EscapeXml(Birthday) + "</birthday>";
            }
            if(Email != null)
            {
                ret += "<email>" + EscapeXml(Email) + "</email>";
            }
            if(FirstName != null)
            {
                ret += "<firstName>" + EscapeXml(FirstName) + "</firstName>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<first_name>" + EscapeXml(FirstName) + "</first_name>";
                }
            }
            if(Gender != null)
            {
                ret += "<gender>" + EscapeXml(Gender) + "</gender>";
            }
            if(ID != null)
            {
                ret += "<id>" + EscapeXml(ID) + "</id>";
            }
            if(LastName != null)
            {
                ret += "<lastName>" + EscapeXml(LastName) + "</lastName>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<last_name>" + EscapeXml(LastName) + "</last_name>";
                }
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(UserId != null)
            {
                ret += "<userId>" + EscapeXml(UserId) + "</userId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<user_id>" + EscapeXml(UserId) + "</user_id>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaSocialUserConfig
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(PermissionItems != null)
            {
                propertyValue = "[" + String.Join(", ", PermissionItems.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"actionPermissionItems\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(PermissionItems != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", PermissionItems.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<actionPermissionItems>" + propertyValue + "</actionPermissionItems>";
            }
            return ret;
        }
    }
    public partial class KalturaTwitterTwit
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaUserSocialActionResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(NetworkStatus != null)
            {
                propertyValue = "[" + String.Join(", ", NetworkStatus.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"failStatus\": " + propertyValue);
            }
            if(SocialAction != null)
            {
                propertyValue = SocialAction.ToJson(currentVersion, omitObsolete);
                ret.Add("\"socialAction\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(NetworkStatus != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", NetworkStatus.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<failStatus>" + propertyValue + "</failStatus>";
            }
            if(SocialAction != null)
            {
                propertyValue = SocialAction.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<socialAction>" + propertyValue + "</socialAction>";
            }
            return ret;
        }
    }
}

namespace WebAPI.Models.General
{
    public partial class KalturaAggregationCountFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaApiActionPermissionItem
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Action != null)
            {
                ret.Add("\"action\": " + "\"" + EscapeJson(Action) + "\"");
            }
            if(Service != null)
            {
                ret.Add("\"service\": " + "\"" + EscapeJson(Service) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Action != null)
            {
                ret += "<action>" + EscapeXml(Action) + "</action>";
            }
            if(Service != null)
            {
                ret += "<service>" + EscapeXml(Service) + "</service>";
            }
            return ret;
        }
    }
    public partial class KalturaApiArgumentPermissionItem
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Action != null)
            {
                ret.Add("\"action\": " + "\"" + EscapeJson(Action) + "\"");
            }
            if(Parameter != null)
            {
                ret.Add("\"parameter\": " + "\"" + EscapeJson(Parameter) + "\"");
            }
            if(Service != null)
            {
                ret.Add("\"service\": " + "\"" + EscapeJson(Service) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Action != null)
            {
                ret += "<action>" + EscapeXml(Action) + "</action>";
            }
            if(Parameter != null)
            {
                ret += "<parameter>" + EscapeXml(Parameter) + "</parameter>";
            }
            if(Service != null)
            {
                ret += "<service>" + EscapeXml(Service) + "</service>";
            }
            return ret;
        }
    }
    public partial class KalturaApiParameterPermissionItem
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"action\": " + "\"" + Enum.GetName(typeof(KalturaApiParameterPermissionItemAction), Action) + "\"");
            if(Object != null)
            {
                ret.Add("\"object\": " + "\"" + EscapeJson(Object) + "\"");
            }
            if(Parameter != null)
            {
                ret.Add("\"parameter\": " + "\"" + EscapeJson(Parameter) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<action>" + "\"" + Enum.GetName(typeof(KalturaApiParameterPermissionItemAction), Action) + "\"" + "</action>";
            if(Object != null)
            {
                ret += "<object>" + EscapeXml(Object) + "</object>";
            }
            if(Parameter != null)
            {
                ret += "<parameter>" + EscapeXml(Parameter) + "</parameter>";
            }
            return ret;
        }
    }
    public partial class KalturaAppToken
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"createDate\": " + CreateDate);
            if(Expiry.HasValue)
            {
                ret.Add("\"expiry\": " + Expiry);
            }
            if(HashType.HasValue)
            {
                ret.Add("\"hashType\": " + "\"" + Enum.GetName(typeof(KalturaAppTokenHashType), HashType) + "\"");
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + EscapeJson(Id) + "\"");
            }
            if(PartnerId.HasValue)
            {
                ret.Add("\"partnerId\": " + PartnerId);
            }
            if(SessionDuration.HasValue)
            {
                ret.Add("\"sessionDuration\": " + SessionDuration);
            }
            if(SessionPrivileges != null)
            {
                ret.Add("\"sessionPrivileges\": " + "\"" + EscapeJson(SessionPrivileges) + "\"");
            }
            if(!DeprecatedAttribute.IsDeprecated("4.5.0.0", currentVersion) && SessionType.HasValue)
            {
                ret.Add("\"sessionType\": " + "\"" + Enum.GetName(typeof(KalturaSessionType), SessionType) + "\"");
            }
            if(SessionUserId != null)
            {
                ret.Add("\"sessionUserId\": " + "\"" + EscapeJson(SessionUserId) + "\"");
            }
            if(!DeprecatedAttribute.IsDeprecated("4.5.0.0", currentVersion))
            {
                ret.Add("\"status\": " + "\"" + Enum.GetName(typeof(KalturaAppTokenStatus), Status) + "\"");
            }
            if(Token != null)
            {
                ret.Add("\"token\": " + "\"" + EscapeJson(Token) + "\"");
            }
            ret.Add("\"updateDate\": " + UpdateDate);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<createDate>" + CreateDate + "</createDate>";
            if(Expiry.HasValue)
            {
                ret += "<expiry>" + Expiry + "</expiry>";
            }
            if(HashType.HasValue)
            {
                ret += "<hashType>" + "\"" + Enum.GetName(typeof(KalturaAppTokenHashType), HashType) + "\"" + "</hashType>";
            }
            if(Id != null)
            {
                ret += "<id>" + EscapeXml(Id) + "</id>";
            }
            if(PartnerId.HasValue)
            {
                ret += "<partnerId>" + PartnerId + "</partnerId>";
            }
            if(SessionDuration.HasValue)
            {
                ret += "<sessionDuration>" + SessionDuration + "</sessionDuration>";
            }
            if(SessionPrivileges != null)
            {
                ret += "<sessionPrivileges>" + EscapeXml(SessionPrivileges) + "</sessionPrivileges>";
            }
            if(!DeprecatedAttribute.IsDeprecated("4.5.0.0", currentVersion) && SessionType.HasValue)
            {
                ret += "<sessionType>" + "\"" + Enum.GetName(typeof(KalturaSessionType), SessionType) + "\"" + "</sessionType>";
            }
            if(SessionUserId != null)
            {
                ret += "<sessionUserId>" + EscapeXml(SessionUserId) + "</sessionUserId>";
            }
            if(!DeprecatedAttribute.IsDeprecated("4.5.0.0", currentVersion))
            {
                ret += "<status>" + "\"" + Enum.GetName(typeof(KalturaAppTokenStatus), Status) + "\"" + "</status>";
            }
            if(Token != null)
            {
                ret += "<token>" + EscapeXml(Token) + "</token>";
            }
            ret += "<updateDate>" + UpdateDate + "</updateDate>";
            return ret;
        }
    }
    public partial class KalturaBaseResponseProfile
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaBooleanValue
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"value\": " + value.ToString().ToLower());
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<value>" + value.ToString().ToLower() + "</value>";
            return ret;
        }
    }
    public partial class KalturaClientConfiguration
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ApiVersion != null)
            {
                ret.Add("\"apiVersion\": " + "\"" + EscapeJson(ApiVersion) + "\"");
            }
            if(ClientTag != null)
            {
                ret.Add("\"clientTag\": " + "\"" + EscapeJson(ClientTag) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ApiVersion != null)
            {
                ret += "<apiVersion>" + EscapeXml(ApiVersion) + "</apiVersion>";
            }
            if(ClientTag != null)
            {
                ret += "<clientTag>" + EscapeXml(ClientTag) + "</clientTag>";
            }
            return ret;
        }
    }
    public partial class KalturaDetachedResponseProfile
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"filter\": " + Filter);
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(RelatedProfiles != null)
            {
                propertyValue = "[" + String.Join(", ", RelatedProfiles.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"relatedProfiles\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<filter>" + Filter + "</filter>";
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(RelatedProfiles != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", RelatedProfiles.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<relatedProfiles>" + propertyValue + "</relatedProfiles>";
            }
            return ret;
        }
    }
    public partial class KalturaDoubleValue
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"value\": " + value);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<value>" + value + "</value>";
            return ret;
        }
    }
    public partial class KalturaFilter<KalturaT>
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"orderBy\": " + OrderBy);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<orderBy>" + OrderBy + "</orderBy>";
            return ret;
        }
    }
    public partial class KalturaFilterPager
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(PageIndex.HasValue)
            {
                ret.Add("\"pageIndex\": " + PageIndex);
            }
            if(PageSize.HasValue)
            {
                ret.Add("\"pageSize\": " + PageSize);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(PageIndex.HasValue)
            {
                ret += "<pageIndex>" + PageIndex + "</pageIndex>";
            }
            if(PageSize.HasValue)
            {
                ret += "<pageSize>" + PageSize + "</pageSize>";
            }
            return ret;
        }
    }
    public partial class KalturaGenericListResponse<KalturaT>
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + EscapeJson(objectType) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            if(objectType != null)
            {
                ret += "<objectType>" + EscapeXml(objectType) + "</objectType>";
            }
            return ret;
        }
    }
    public partial class KalturaGroupPermission
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Group != null)
            {
                ret.Add("\"group\": " + "\"" + EscapeJson(Group) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Group != null)
            {
                ret += "<group>" + EscapeXml(Group) + "</group>";
            }
            return ret;
        }
    }
    public partial class KalturaIdentifierTypeFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"by\": " + "\"" + Enum.GetName(typeof(KalturaIdentifierTypeBy), By) + "\"");
            if(Identifier != null)
            {
                ret.Add("\"identifier\": " + "\"" + EscapeJson(Identifier) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<by>" + "\"" + Enum.GetName(typeof(KalturaIdentifierTypeBy), By) + "\"" + "</by>";
            if(Identifier != null)
            {
                ret += "<identifier>" + EscapeXml(Identifier) + "</identifier>";
            }
            return ret;
        }
    }
    public partial class KalturaIntegerValue
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"value\": " + value);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<value>" + value + "</value>";
            return ret;
        }
    }
    public partial class KalturaIntegerValueListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Values != null)
            {
                propertyValue = "[" + String.Join(", ", Values.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Values != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Values.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaKeyValue
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(key != null)
            {
                ret.Add("\"key\": " + "\"" + EscapeJson(key) + "\"");
            }
            if(value != null)
            {
                ret.Add("\"value\": " + "\"" + EscapeJson(value) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(key != null)
            {
                ret += "<key>" + EscapeXml(key) + "</key>";
            }
            if(value != null)
            {
                ret += "<value>" + EscapeXml(value) + "</value>";
            }
            return ret;
        }
    }
    public partial class KalturaListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"totalCount\": " + TotalCount);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<totalCount>" + TotalCount + "</totalCount>";
            return ret;
        }
    }
    public partial class KalturaLongValue
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"value\": " + value);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<value>" + value + "</value>";
            return ret;
        }
    }
    public partial class KalturaMultilingualStringValue
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add(value.ToCustomJson(currentVersion, omitObsolete, "value"));
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += value.ToCustomXml(currentVersion, omitObsolete, "value");
            return ret;
        }
    }
    public partial class KalturaMultilingualStringValueArray
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaNotification
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(eventObject != null)
            {
                propertyValue = eventObject.ToJson(currentVersion, omitObsolete);
                ret.Add("\"object\": " + propertyValue);
            }
            if(eventObjectType != null)
            {
                ret.Add("\"eventObjectType\": " + "\"" + EscapeJson(eventObjectType) + "\"");
            }
            if(eventType.HasValue)
            {
                ret.Add("\"eventType\": " + "\"" + Enum.GetName(typeof(KalturaEventAction), eventType) + "\"");
            }
            ret.Add("\"partnerId\": " + partnerId);
            if(systemName != null)
            {
                ret.Add("\"systemName\": " + "\"" + EscapeJson(systemName) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(eventObject != null)
            {
                propertyValue = eventObject.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<object>" + propertyValue + "</object>";
            }
            if(eventObjectType != null)
            {
                ret += "<eventObjectType>" + EscapeXml(eventObjectType) + "</eventObjectType>";
            }
            if(eventType.HasValue)
            {
                ret += "<eventType>" + "\"" + Enum.GetName(typeof(KalturaEventAction), eventType) + "\"" + "</eventType>";
            }
            ret += "<partnerId>" + partnerId + "</partnerId>";
            if(systemName != null)
            {
                ret += "<systemName>" + EscapeXml(systemName) + "</systemName>";
            }
            return ret;
        }
    }
    public partial class KalturaPersistedFilter<KalturaT>
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            return ret;
        }
    }
    public partial class KalturaReport
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaReportFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaReportListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaRequestConfiguration
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Currency != null)
            {
                ret.Add("\"currency\": " + "\"" + EscapeJson(Currency) + "\"");
            }
            if(KS != null)
            {
                ret.Add("\"ks\": " + "\"" + EscapeJson(KS) + "\"");
            }
            if(Language != null)
            {
                ret.Add("\"language\": " + "\"" + EscapeJson(Language) + "\"");
            }
            if(PartnerID.HasValue)
            {
                ret.Add("\"partnerId\": " + PartnerID);
            }
            if(ResponseProfile != null)
            {
                propertyValue = ResponseProfile.ToJson(currentVersion, omitObsolete);
                ret.Add("\"responseProfile\": " + propertyValue);
            }
            if(UserID.HasValue)
            {
                ret.Add("\"userId\": " + UserID);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Currency != null)
            {
                ret += "<currency>" + EscapeXml(Currency) + "</currency>";
            }
            if(KS != null)
            {
                ret += "<ks>" + EscapeXml(KS) + "</ks>";
            }
            if(Language != null)
            {
                ret += "<language>" + EscapeXml(Language) + "</language>";
            }
            if(PartnerID.HasValue)
            {
                ret += "<partnerId>" + PartnerID + "</partnerId>";
            }
            if(ResponseProfile != null)
            {
                propertyValue = ResponseProfile.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<responseProfile>" + propertyValue + "</responseProfile>";
            }
            if(UserID.HasValue)
            {
                ret += "<userId>" + UserID + "</userId>";
            }
            return ret;
        }
    }
    public partial class KalturaStringValue
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(value != null)
            {
                ret.Add("\"value\": " + "\"" + EscapeJson(value) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(value != null)
            {
                ret += "<value>" + EscapeXml(value) + "</value>";
            }
            return ret;
        }
    }
    public partial class KalturaStringValueArray
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaTranslationToken
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Language != null)
            {
                ret.Add("\"language\": " + "\"" + EscapeJson(Language) + "\"");
            }
            if(Value != null)
            {
                ret.Add("\"value\": " + "\"" + EscapeJson(Value) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Language != null)
            {
                ret += "<language>" + EscapeXml(Language) + "</language>";
            }
            if(Value != null)
            {
                ret += "<value>" + EscapeXml(Value) + "</value>";
            }
            return ret;
        }
    }
    public partial class KalturaValue
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(description != null)
            {
                ret.Add("\"description\": " + "\"" + EscapeJson(description) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(description != null)
            {
                ret += "<description>" + EscapeXml(description) + "</description>";
            }
            return ret;
        }
    }
}

namespace WebAPI.Models.Notifications
{
    public partial class KalturaAnnouncement
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Enabled.HasValue)
            {
                ret.Add("\"enabled\": " + Enabled.ToString().ToLower());
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(ImageUrl != null)
            {
                ret.Add("\"imageUrl\": " + "\"" + EscapeJson(ImageUrl) + "\"");
            }
            ret.Add("\"includeMail\": " + IncludeMail.ToString().ToLower());
            ret.Add("\"includeSms\": " + IncludeSms.ToString().ToLower());
            if(MailSubject != null)
            {
                ret.Add("\"mailSubject\": " + "\"" + EscapeJson(MailSubject) + "\"");
            }
            if(MailTemplate != null)
            {
                ret.Add("\"mailTemplate\": " + "\"" + EscapeJson(MailTemplate) + "\"");
            }
            if(Message != null)
            {
                ret.Add("\"message\": " + "\"" + EscapeJson(Message) + "\"");
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            ret.Add("\"recipients\": " + "\"" + Enum.GetName(typeof(KalturaAnnouncementRecipientsType), Recipients) + "\"");
            if(StartTime.HasValue)
            {
                ret.Add("\"startTime\": " + StartTime);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"start_time\": " + StartTime);
                }
            }
            ret.Add("\"status\": " + "\"" + Enum.GetName(typeof(KalturaAnnouncementStatus), Status) + "\"");
            if(Timezone != null)
            {
                ret.Add("\"timezone\": " + "\"" + EscapeJson(Timezone) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Enabled.HasValue)
            {
                ret += "<enabled>" + Enabled.ToString().ToLower() + "</enabled>";
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(ImageUrl != null)
            {
                ret += "<imageUrl>" + EscapeXml(ImageUrl) + "</imageUrl>";
            }
            ret += "<includeMail>" + IncludeMail.ToString().ToLower() + "</includeMail>";
            ret += "<includeSms>" + IncludeSms.ToString().ToLower() + "</includeSms>";
            if(MailSubject != null)
            {
                ret += "<mailSubject>" + EscapeXml(MailSubject) + "</mailSubject>";
            }
            if(MailTemplate != null)
            {
                ret += "<mailTemplate>" + EscapeXml(MailTemplate) + "</mailTemplate>";
            }
            if(Message != null)
            {
                ret += "<message>" + EscapeXml(Message) + "</message>";
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            ret += "<recipients>" + "\"" + Enum.GetName(typeof(KalturaAnnouncementRecipientsType), Recipients) + "\"" + "</recipients>";
            if(StartTime.HasValue)
            {
                ret += "<startTime>" + StartTime + "</startTime>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<start_time>" + StartTime + "</start_time>";
                }
            }
            ret += "<status>" + "\"" + Enum.GetName(typeof(KalturaAnnouncementStatus), Status) + "\"" + "</status>";
            if(Timezone != null)
            {
                ret += "<timezone>" + EscapeXml(Timezone) + "</timezone>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetReminder
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"assetId\": " + AssetId);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<assetId>" + AssetId + "</assetId>";
            return ret;
        }
    }
    public partial class KalturaReminder
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            ret.Add("\"type\": " + "\"" + Enum.GetName(typeof(KalturaReminderType), Type) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            ret += "<type>" + "\"" + Enum.GetName(typeof(KalturaReminderType), Type) + "\"" + "</type>";
            return ret;
        }
    }
    public partial class KalturaSeriesReminder
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"epgChannelId\": " + EpgChannelId);
            if(SeasonNumber.HasValue)
            {
                ret.Add("\"seasonNumber\": " + SeasonNumber);
            }
            if(SeriesId != null)
            {
                ret.Add("\"seriesId\": " + "\"" + EscapeJson(SeriesId) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<epgChannelId>" + EpgChannelId + "</epgChannelId>";
            if(SeasonNumber.HasValue)
            {
                ret += "<seasonNumber>" + SeasonNumber + "</seasonNumber>";
            }
            if(SeriesId != null)
            {
                ret += "<seriesId>" + EscapeXml(SeriesId) + "</seriesId>";
            }
            return ret;
        }
    }
}

namespace WebAPI.Models.Notification
{
    public partial class KalturaAnnouncementFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaAnnouncementListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Announcements != null)
            {
                propertyValue = "[" + String.Join(", ", Announcements.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Announcements != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Announcements.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetReminderFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaEmailMessage
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(BccAddress != null)
            {
                ret.Add("\"bccAddress\": " + "\"" + EscapeJson(BccAddress) + "\"");
            }
            if(ExtraParameters != null)
            {
                propertyValue = "[" + String.Join(", ", ExtraParameters.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"extraParameters\": " + propertyValue);
            }
            if(FirstName != null)
            {
                ret.Add("\"firstName\": " + "\"" + EscapeJson(FirstName) + "\"");
            }
            if(LastName != null)
            {
                ret.Add("\"lastName\": " + "\"" + EscapeJson(LastName) + "\"");
            }
            if(SenderFrom != null)
            {
                ret.Add("\"senderFrom\": " + "\"" + EscapeJson(SenderFrom) + "\"");
            }
            if(SenderName != null)
            {
                ret.Add("\"senderName\": " + "\"" + EscapeJson(SenderName) + "\"");
            }
            if(SenderTo != null)
            {
                ret.Add("\"senderTo\": " + "\"" + EscapeJson(SenderTo) + "\"");
            }
            if(Subject != null)
            {
                ret.Add("\"subject\": " + "\"" + EscapeJson(Subject) + "\"");
            }
            if(TemplateName != null)
            {
                ret.Add("\"templateName\": " + "\"" + EscapeJson(TemplateName) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(BccAddress != null)
            {
                ret += "<bccAddress>" + EscapeXml(BccAddress) + "</bccAddress>";
            }
            if(ExtraParameters != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", ExtraParameters.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<extraParameters>" + propertyValue + "</extraParameters>";
            }
            if(FirstName != null)
            {
                ret += "<firstName>" + EscapeXml(FirstName) + "</firstName>";
            }
            if(LastName != null)
            {
                ret += "<lastName>" + EscapeXml(LastName) + "</lastName>";
            }
            if(SenderFrom != null)
            {
                ret += "<senderFrom>" + EscapeXml(SenderFrom) + "</senderFrom>";
            }
            if(SenderName != null)
            {
                ret += "<senderName>" + EscapeXml(SenderName) + "</senderName>";
            }
            if(SenderTo != null)
            {
                ret += "<senderTo>" + EscapeXml(SenderTo) + "</senderTo>";
            }
            if(Subject != null)
            {
                ret += "<subject>" + EscapeXml(Subject) + "</subject>";
            }
            if(TemplateName != null)
            {
                ret += "<templateName>" + EscapeXml(TemplateName) + "</templateName>";
            }
            return ret;
        }
    }
    public partial class KalturaEngagement
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AdapterDynamicData != null)
            {
                ret.Add("\"adapterDynamicData\": " + "\"" + EscapeJson(AdapterDynamicData) + "\"");
            }
            ret.Add("\"adapterId\": " + AdapterId);
            ret.Add("\"couponGroupId\": " + CouponGroupId);
            ret.Add("\"id\": " + Id);
            ret.Add("\"intervalSeconds\": " + IntervalSeconds);
            ret.Add("\"sendTimeInSeconds\": " + SendTimeInSeconds);
            ret.Add("\"totalNumberOfRecipients\": " + TotalNumberOfRecipients);
            ret.Add("\"type\": " + "\"" + Enum.GetName(typeof(KalturaEngagementType), Type) + "\"");
            if(UserList != null)
            {
                ret.Add("\"userList\": " + "\"" + EscapeJson(UserList) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AdapterDynamicData != null)
            {
                ret += "<adapterDynamicData>" + EscapeXml(AdapterDynamicData) + "</adapterDynamicData>";
            }
            ret += "<adapterId>" + AdapterId + "</adapterId>";
            ret += "<couponGroupId>" + CouponGroupId + "</couponGroupId>";
            ret += "<id>" + Id + "</id>";
            ret += "<intervalSeconds>" + IntervalSeconds + "</intervalSeconds>";
            ret += "<sendTimeInSeconds>" + SendTimeInSeconds + "</sendTimeInSeconds>";
            ret += "<totalNumberOfRecipients>" + TotalNumberOfRecipients + "</totalNumberOfRecipients>";
            ret += "<type>" + "\"" + Enum.GetName(typeof(KalturaEngagementType), Type) + "\"" + "</type>";
            if(UserList != null)
            {
                ret += "<userList>" + EscapeXml(UserList) + "</userList>";
            }
            return ret;
        }
    }
    public partial class KalturaEngagementAdapter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AdapterUrl != null)
            {
                ret.Add("\"adapterUrl\": " + "\"" + EscapeJson(AdapterUrl) + "\"");
            }
            if(IsActive.HasValue)
            {
                ret.Add("\"isActive\": " + IsActive.ToString().ToLower());
            }
            if(ProviderUrl != null)
            {
                ret.Add("\"providerUrl\": " + "\"" + EscapeJson(ProviderUrl) + "\"");
            }
            if(Settings != null)
            {
                propertyValue = "{" + String.Join(", ", Settings.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"engagementAdapterSettings\": " + propertyValue);
            }
            if(SharedSecret != null)
            {
                ret.Add("\"sharedSecret\": " + "\"" + EscapeJson(SharedSecret) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AdapterUrl != null)
            {
                ret += "<adapterUrl>" + EscapeXml(AdapterUrl) + "</adapterUrl>";
            }
            if(IsActive.HasValue)
            {
                ret += "<isActive>" + IsActive.ToString().ToLower() + "</isActive>";
            }
            if(ProviderUrl != null)
            {
                ret += "<providerUrl>" + EscapeXml(ProviderUrl) + "</providerUrl>";
            }
            if(Settings != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Settings.Select(pair => "<itemKey>" + pair.Key + "</itemKey>" + pair.Value.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<engagementAdapterSettings>" + propertyValue + "</engagementAdapterSettings>";
            }
            if(SharedSecret != null)
            {
                ret += "<sharedSecret>" + EscapeXml(SharedSecret) + "</sharedSecret>";
            }
            return ret;
        }
    }
    public partial class KalturaEngagementAdapterBase
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            return ret;
        }
    }
    public partial class KalturaEngagementAdapterListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(EngagementAdapters != null)
            {
                propertyValue = "[" + String.Join(", ", EngagementAdapters.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(EngagementAdapters != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", EngagementAdapters.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaEngagementFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(SendTimeGreaterThanOrEqual.HasValue)
            {
                ret.Add("\"sendTimeGreaterThanOrEqual\": " + SendTimeGreaterThanOrEqual);
            }
            if(TypeIn != null)
            {
                ret.Add("\"typeIn\": " + "\"" + EscapeJson(TypeIn) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(SendTimeGreaterThanOrEqual.HasValue)
            {
                ret += "<sendTimeGreaterThanOrEqual>" + SendTimeGreaterThanOrEqual + "</sendTimeGreaterThanOrEqual>";
            }
            if(TypeIn != null)
            {
                ret += "<typeIn>" + EscapeXml(TypeIn) + "</typeIn>";
            }
            return ret;
        }
    }
    public partial class KalturaEngagementListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Engagements != null)
            {
                propertyValue = "[" + String.Join(", ", Engagements.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Engagements != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Engagements.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaFeed
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"assetId\": " + AssetId);
            if (currentVersion == null || isOldVersion)
            {
                ret.Add("\"asset_id\": " + AssetId);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<assetId>" + AssetId + "</assetId>";
            if (currentVersion == null || isOldVersion)
            {
            ret += "<asset_id>" + AssetId + "</asset_id>";
            }
            return ret;
        }
    }
    public partial class KalturaFollowDataBase
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"announcementId\": " + AnnouncementId);
            if (currentVersion == null || isOldVersion)
            {
                ret.Add("\"announcement_id\": " + AnnouncementId);
            }
            if(FollowPhrase != null)
            {
                ret.Add("\"followPhrase\": " + "\"" + EscapeJson(FollowPhrase) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"follow_phrase\": " + "\"" + EscapeJson(FollowPhrase) + "\"");
                }
            }
            ret.Add("\"status\": " + Status);
            ret.Add("\"timestamp\": " + Timestamp);
            if(Title != null)
            {
                ret.Add("\"title\": " + "\"" + EscapeJson(Title) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<announcementId>" + AnnouncementId + "</announcementId>";
            if (currentVersion == null || isOldVersion)
            {
            ret += "<announcement_id>" + AnnouncementId + "</announcement_id>";
            }
            if(FollowPhrase != null)
            {
                ret += "<followPhrase>" + EscapeXml(FollowPhrase) + "</followPhrase>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<follow_phrase>" + EscapeXml(FollowPhrase) + "</follow_phrase>";
                }
            }
            ret += "<status>" + Status + "</status>";
            ret += "<timestamp>" + Timestamp + "</timestamp>";
            if(Title != null)
            {
                ret += "<title>" + EscapeXml(Title) + "</title>";
            }
            return ret;
        }
    }
    public partial class KalturaFollowDataTvSeries
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"assetId\": " + AssetId);
            if (currentVersion == null || isOldVersion)
            {
                ret.Add("\"asset_id\": " + AssetId);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<assetId>" + AssetId + "</assetId>";
            if (currentVersion == null || isOldVersion)
            {
            ret += "<asset_id>" + AssetId + "</asset_id>";
            }
            return ret;
        }
    }
    public partial class KalturaFollowTvSeries
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"assetId\": " + AssetId);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<assetId>" + AssetId + "</assetId>";
            return ret;
        }
    }
    public partial class KalturaFollowTvSeriesFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaFollowTvSeriesListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(FollowDataList != null)
            {
                propertyValue = "[" + String.Join(", ", FollowDataList.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(FollowDataList != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", FollowDataList.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaInboxMessage
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"createdAt\": " + CreatedAt);
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + EscapeJson(Id) + "\"");
            }
            if(Message != null)
            {
                ret.Add("\"message\": " + "\"" + EscapeJson(Message) + "\"");
            }
            ret.Add("\"status\": " + "\"" + Enum.GetName(typeof(KalturaInboxMessageStatus), Status) + "\"");
            ret.Add("\"type\": " + "\"" + Enum.GetName(typeof(KalturaInboxMessageType), Type) + "\"");
            if(Url != null)
            {
                ret.Add("\"url\": " + "\"" + EscapeJson(Url) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<createdAt>" + CreatedAt + "</createdAt>";
            if(Id != null)
            {
                ret += "<id>" + EscapeXml(Id) + "</id>";
            }
            if(Message != null)
            {
                ret += "<message>" + EscapeXml(Message) + "</message>";
            }
            ret += "<status>" + "\"" + Enum.GetName(typeof(KalturaInboxMessageStatus), Status) + "\"" + "</status>";
            ret += "<type>" + "\"" + Enum.GetName(typeof(KalturaInboxMessageType), Type) + "\"" + "</type>";
            if(Url != null)
            {
                ret += "<url>" + EscapeXml(Url) + "</url>";
            }
            return ret;
        }
    }
    public partial class KalturaInboxMessageFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(CreatedAtGreaterThanOrEqual.HasValue)
            {
                ret.Add("\"createdAtGreaterThanOrEqual\": " + CreatedAtGreaterThanOrEqual);
            }
            if(CreatedAtLessThanOrEqual.HasValue)
            {
                ret.Add("\"createdAtLessThanOrEqual\": " + CreatedAtLessThanOrEqual);
            }
            if(TypeIn != null)
            {
                ret.Add("\"typeIn\": " + "\"" + EscapeJson(TypeIn) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(CreatedAtGreaterThanOrEqual.HasValue)
            {
                ret += "<createdAtGreaterThanOrEqual>" + CreatedAtGreaterThanOrEqual + "</createdAtGreaterThanOrEqual>";
            }
            if(CreatedAtLessThanOrEqual.HasValue)
            {
                ret += "<createdAtLessThanOrEqual>" + CreatedAtLessThanOrEqual + "</createdAtLessThanOrEqual>";
            }
            if(TypeIn != null)
            {
                ret += "<typeIn>" + EscapeXml(TypeIn) + "</typeIn>";
            }
            return ret;
        }
    }
    public partial class KalturaInboxMessageListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(InboxMessages != null)
            {
                propertyValue = "[" + String.Join(", ", InboxMessages.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(InboxMessages != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", InboxMessages.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaInboxMessageResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(InboxMessages != null)
            {
                propertyValue = "[" + String.Join(", ", InboxMessages.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(InboxMessages != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", InboxMessages.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaInboxMessageTypeHolder
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"type\": " + "\"" + Enum.GetName(typeof(KalturaInboxMessageType), type) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<type>" + "\"" + Enum.GetName(typeof(KalturaInboxMessageType), type) + "\"" + "</type>";
            return ret;
        }
    }
    public partial class KalturaListFollowDataTvSeriesResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(FollowDataList != null)
            {
                propertyValue = "[" + String.Join(", ", FollowDataList.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(FollowDataList != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", FollowDataList.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaMessageAnnouncementListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Announcements != null)
            {
                propertyValue = "[" + String.Join(", ", Announcements.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Announcements != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Announcements.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaMessageTemplate
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Action != null)
            {
                ret.Add("\"action\": " + "\"" + EscapeJson(Action) + "\"");
            }
            if(DateFormat != null)
            {
                ret.Add("\"dateFormat\": " + "\"" + EscapeJson(DateFormat) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"date_format\": " + "\"" + EscapeJson(DateFormat) + "\"");
                }
            }
            if(MailSubject != null)
            {
                ret.Add("\"mailSubject\": " + "\"" + EscapeJson(MailSubject) + "\"");
            }
            if(MailTemplate != null)
            {
                ret.Add("\"mailTemplate\": " + "\"" + EscapeJson(MailTemplate) + "\"");
            }
            if(Message != null)
            {
                ret.Add("\"message\": " + "\"" + EscapeJson(Message) + "\"");
            }
            ret.Add("\"messageType\": " + "\"" + Enum.GetName(typeof(KalturaMessageTemplateType), MessageType) + "\"");
            if (currentVersion == null || isOldVersion)
            {
                ret.Add("\"asset_type\": " + "\"" + Enum.GetName(typeof(KalturaMessageTemplateType), MessageType) + "\"");
            }
            if (currentVersion == null || isOldVersion || currentVersion.CompareTo(new Version("3.6.2094.15157")) > 0)
            {
                ret.Add("\"assetType\": " + "\"" + Enum.GetName(typeof(KalturaMessageTemplateType), MessageType) + "\"");
            }
            if(RatioId != null)
            {
                ret.Add("\"ratioId\": " + "\"" + EscapeJson(RatioId) + "\"");
            }
            if(Sound != null)
            {
                ret.Add("\"sound\": " + "\"" + EscapeJson(Sound) + "\"");
            }
            if(URL != null)
            {
                ret.Add("\"url\": " + "\"" + EscapeJson(URL) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Action != null)
            {
                ret += "<action>" + EscapeXml(Action) + "</action>";
            }
            if(DateFormat != null)
            {
                ret += "<dateFormat>" + EscapeXml(DateFormat) + "</dateFormat>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<date_format>" + EscapeXml(DateFormat) + "</date_format>";
                }
            }
            if(MailSubject != null)
            {
                ret += "<mailSubject>" + EscapeXml(MailSubject) + "</mailSubject>";
            }
            if(MailTemplate != null)
            {
                ret += "<mailTemplate>" + EscapeXml(MailTemplate) + "</mailTemplate>";
            }
            if(Message != null)
            {
                ret += "<message>" + EscapeXml(Message) + "</message>";
            }
            ret += "<messageType>" + "\"" + Enum.GetName(typeof(KalturaMessageTemplateType), MessageType) + "\"" + "</messageType>";
            if (currentVersion == null || isOldVersion)
            {
            ret += "<asset_type>" + "\"" + Enum.GetName(typeof(KalturaMessageTemplateType), MessageType) + "\"" + "</asset_type>";
            }
            if (currentVersion == null || isOldVersion || currentVersion.CompareTo(new Version("3.6.2094.15157")) > 0)
            {
            ret += "<assetType>" + "\"" + Enum.GetName(typeof(KalturaMessageTemplateType), MessageType) + "\"" + "</assetType>";
            }
            if(RatioId != null)
            {
                ret += "<ratioId>" + EscapeXml(RatioId) + "</ratioId>";
            }
            if(Sound != null)
            {
                ret += "<sound>" + EscapeXml(Sound) + "</sound>";
            }
            if(URL != null)
            {
                ret += "<url>" + EscapeXml(URL) + "</url>";
            }
            return ret;
        }
    }
    public partial class KalturaNotificationSettings
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaNotificationsPartnerSettings
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AutomaticIssueFollowNotification.HasValue)
            {
                ret.Add("\"automaticIssueFollowNotification\": " + AutomaticIssueFollowNotification.ToString().ToLower());
            }
            if(ChurnMailSubject != null)
            {
                ret.Add("\"churnMailSubject\": " + "\"" + EscapeJson(ChurnMailSubject) + "\"");
            }
            if(ChurnMailTemplateName != null)
            {
                ret.Add("\"churnMailTemplateName\": " + "\"" + EscapeJson(ChurnMailTemplateName) + "\"");
            }
            if(InboxEnabled.HasValue)
            {
                ret.Add("\"inboxEnabled\": " + InboxEnabled.ToString().ToLower());
            }
            if(MailNotificationAdapterId.HasValue)
            {
                ret.Add("\"mailNotificationAdapterId\": " + MailNotificationAdapterId);
            }
            if(MailSenderName != null)
            {
                ret.Add("\"mailSenderName\": " + "\"" + EscapeJson(MailSenderName) + "\"");
            }
            if(MessageTTLDays.HasValue)
            {
                ret.Add("\"messageTTLDays\": " + MessageTTLDays);
            }
            if(PushAdapterUrl != null)
            {
                ret.Add("\"pushAdapterUrl\": " + "\"" + EscapeJson(PushAdapterUrl) + "\"");
            }
            if(PushEndHour.HasValue)
            {
                ret.Add("\"pushEndHour\": " + PushEndHour);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"push_end_hour\": " + PushEndHour);
                }
            }
            if(PushNotificationEnabled.HasValue)
            {
                ret.Add("\"pushNotificationEnabled\": " + PushNotificationEnabled.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"push_notification_enabled\": " + PushNotificationEnabled.ToString().ToLower());
                }
            }
            if(PushStartHour.HasValue)
            {
                ret.Add("\"pushStartHour\": " + PushStartHour);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"push_start_hour\": " + PushStartHour);
                }
            }
            if(PushSystemAnnouncementsEnabled.HasValue)
            {
                ret.Add("\"pushSystemAnnouncementsEnabled\": " + PushSystemAnnouncementsEnabled.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"push_system_announcements_enabled\": " + PushSystemAnnouncementsEnabled.ToString().ToLower());
                }
            }
            if(ReminderEnabled.HasValue)
            {
                ret.Add("\"reminderEnabled\": " + ReminderEnabled.ToString().ToLower());
            }
            if(ReminderOffset.HasValue)
            {
                ret.Add("\"reminderOffsetSec\": " + ReminderOffset);
            }
            if(SenderEmail != null)
            {
                ret.Add("\"senderEmail\": " + "\"" + EscapeJson(SenderEmail) + "\"");
            }
            if(SmsEnabled.HasValue)
            {
                ret.Add("\"smsEnabled\": " + SmsEnabled.ToString().ToLower());
            }
            if(TopicExpirationDurationDays.HasValue)
            {
                ret.Add("\"topicExpirationDurationDays\": " + TopicExpirationDurationDays);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AutomaticIssueFollowNotification.HasValue)
            {
                ret += "<automaticIssueFollowNotification>" + AutomaticIssueFollowNotification.ToString().ToLower() + "</automaticIssueFollowNotification>";
            }
            if(ChurnMailSubject != null)
            {
                ret += "<churnMailSubject>" + EscapeXml(ChurnMailSubject) + "</churnMailSubject>";
            }
            if(ChurnMailTemplateName != null)
            {
                ret += "<churnMailTemplateName>" + EscapeXml(ChurnMailTemplateName) + "</churnMailTemplateName>";
            }
            if(InboxEnabled.HasValue)
            {
                ret += "<inboxEnabled>" + InboxEnabled.ToString().ToLower() + "</inboxEnabled>";
            }
            if(MailNotificationAdapterId.HasValue)
            {
                ret += "<mailNotificationAdapterId>" + MailNotificationAdapterId + "</mailNotificationAdapterId>";
            }
            if(MailSenderName != null)
            {
                ret += "<mailSenderName>" + EscapeXml(MailSenderName) + "</mailSenderName>";
            }
            if(MessageTTLDays.HasValue)
            {
                ret += "<messageTTLDays>" + MessageTTLDays + "</messageTTLDays>";
            }
            if(PushAdapterUrl != null)
            {
                ret += "<pushAdapterUrl>" + EscapeXml(PushAdapterUrl) + "</pushAdapterUrl>";
            }
            if(PushEndHour.HasValue)
            {
                ret += "<pushEndHour>" + PushEndHour + "</pushEndHour>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<push_end_hour>" + PushEndHour + "</push_end_hour>";
                }
            }
            if(PushNotificationEnabled.HasValue)
            {
                ret += "<pushNotificationEnabled>" + PushNotificationEnabled.ToString().ToLower() + "</pushNotificationEnabled>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<push_notification_enabled>" + PushNotificationEnabled.ToString().ToLower() + "</push_notification_enabled>";
                }
            }
            if(PushStartHour.HasValue)
            {
                ret += "<pushStartHour>" + PushStartHour + "</pushStartHour>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<push_start_hour>" + PushStartHour + "</push_start_hour>";
                }
            }
            if(PushSystemAnnouncementsEnabled.HasValue)
            {
                ret += "<pushSystemAnnouncementsEnabled>" + PushSystemAnnouncementsEnabled.ToString().ToLower() + "</pushSystemAnnouncementsEnabled>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<push_system_announcements_enabled>" + PushSystemAnnouncementsEnabled.ToString().ToLower() + "</push_system_announcements_enabled>";
                }
            }
            if(ReminderEnabled.HasValue)
            {
                ret += "<reminderEnabled>" + ReminderEnabled.ToString().ToLower() + "</reminderEnabled>";
            }
            if(ReminderOffset.HasValue)
            {
                ret += "<reminderOffsetSec>" + ReminderOffset + "</reminderOffsetSec>";
            }
            if(SenderEmail != null)
            {
                ret += "<senderEmail>" + EscapeXml(SenderEmail) + "</senderEmail>";
            }
            if(SmsEnabled.HasValue)
            {
                ret += "<smsEnabled>" + SmsEnabled.ToString().ToLower() + "</smsEnabled>";
            }
            if(TopicExpirationDurationDays.HasValue)
            {
                ret += "<topicExpirationDurationDays>" + TopicExpirationDurationDays + "</topicExpirationDurationDays>";
            }
            return ret;
        }
    }
    public partial class KalturaNotificationsSettings
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(MailEnabled.HasValue)
            {
                ret.Add("\"mailEnabled\": " + MailEnabled.ToString().ToLower());
            }
            if(PushFollowEnabled.HasValue)
            {
                ret.Add("\"pushFollowEnabled\": " + PushFollowEnabled.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"push_follow_enabled\": " + PushFollowEnabled.ToString().ToLower());
                }
            }
            if(PushNotificationEnabled.HasValue)
            {
                ret.Add("\"pushNotificationEnabled\": " + PushNotificationEnabled.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"push_notification_enabled\": " + PushNotificationEnabled.ToString().ToLower());
                }
            }
            if(SmsEnabled.HasValue)
            {
                ret.Add("\"smsEnabled\": " + SmsEnabled.ToString().ToLower());
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(MailEnabled.HasValue)
            {
                ret += "<mailEnabled>" + MailEnabled.ToString().ToLower() + "</mailEnabled>";
            }
            if(PushFollowEnabled.HasValue)
            {
                ret += "<pushFollowEnabled>" + PushFollowEnabled.ToString().ToLower() + "</pushFollowEnabled>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<push_follow_enabled>" + PushFollowEnabled.ToString().ToLower() + "</push_follow_enabled>";
                }
            }
            if(PushNotificationEnabled.HasValue)
            {
                ret += "<pushNotificationEnabled>" + PushNotificationEnabled.ToString().ToLower() + "</pushNotificationEnabled>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<push_notification_enabled>" + PushNotificationEnabled.ToString().ToLower() + "</push_notification_enabled>";
                }
            }
            if(SmsEnabled.HasValue)
            {
                ret += "<smsEnabled>" + SmsEnabled.ToString().ToLower() + "</smsEnabled>";
            }
            return ret;
        }
    }
    public partial class KalturaPartnerNotificationSettings
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaPersonalFeed
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaPersonalFeedFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaPersonalFeedListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(PersonalFollowFeed != null)
            {
                propertyValue = "[" + String.Join(", ", PersonalFollowFeed.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(PersonalFollowFeed != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", PersonalFollowFeed.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaPersonalFollowFeed
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaPersonalFollowFeedResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(PersonalFollowFeed != null)
            {
                propertyValue = "[" + String.Join(", ", PersonalFollowFeed.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(PersonalFollowFeed != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", PersonalFollowFeed.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaPersonalList
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"createDate\": " + CreateDate);
            ret.Add("\"id\": " + Id);
            if(Ksql != null)
            {
                ret.Add("\"ksql\": " + "\"" + EscapeJson(Ksql) + "\"");
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            ret.Add("\"partnerListType\": " + PartnerListType);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<createDate>" + CreateDate + "</createDate>";
            ret += "<id>" + Id + "</id>";
            if(Ksql != null)
            {
                ret += "<ksql>" + EscapeXml(Ksql) + "</ksql>";
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            ret += "<partnerListType>" + PartnerListType + "</partnerListType>";
            return ret;
        }
    }
    public partial class KalturaPersonalListFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(PartnerListTypeIn != null)
            {
                ret.Add("\"partnerListTypeIn\": " + "\"" + EscapeJson(PartnerListTypeIn) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(PartnerListTypeIn != null)
            {
                ret += "<partnerListTypeIn>" + EscapeXml(PartnerListTypeIn) + "</partnerListTypeIn>";
            }
            return ret;
        }
    }
    public partial class KalturaPersonalListListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(PersonalListList != null)
            {
                propertyValue = "[" + String.Join(", ", PersonalListList.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(PersonalListList != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", PersonalListList.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaPushMessage
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Action != null)
            {
                ret.Add("\"action\": " + "\"" + EscapeJson(Action) + "\"");
            }
            if(Message != null)
            {
                ret.Add("\"message\": " + "\"" + EscapeJson(Message) + "\"");
            }
            if(Sound != null)
            {
                ret.Add("\"sound\": " + "\"" + EscapeJson(Sound) + "\"");
            }
            if(Url != null)
            {
                ret.Add("\"url\": " + "\"" + EscapeJson(Url) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Action != null)
            {
                ret += "<action>" + EscapeXml(Action) + "</action>";
            }
            if(Message != null)
            {
                ret += "<message>" + EscapeXml(Message) + "</message>";
            }
            if(Sound != null)
            {
                ret += "<sound>" + EscapeXml(Sound) + "</sound>";
            }
            if(Url != null)
            {
                ret += "<url>" + EscapeXml(Url) + "</url>";
            }
            return ret;
        }
    }
    public partial class KalturaRegistryResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"announcementId\": " + AnnouncementId);
            if(Key != null)
            {
                ret.Add("\"key\": " + "\"" + EscapeJson(Key) + "\"");
            }
            if(Url != null)
            {
                ret.Add("\"url\": " + "\"" + EscapeJson(Url) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<announcementId>" + AnnouncementId + "</announcementId>";
            if(Key != null)
            {
                ret += "<key>" + EscapeXml(Key) + "</key>";
            }
            if(Url != null)
            {
                ret += "<url>" + EscapeXml(Url) + "</url>";
            }
            return ret;
        }
    }
    public partial class KalturaReminderFilter<KalturaT>
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(!omitObsolete && KSql != null)
            {
                ret.Add("\"kSql\": " + "\"" + EscapeJson(KSql) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(!omitObsolete && KSql != null)
            {
                ret += "<kSql>" + EscapeXml(KSql) + "</kSql>";
            }
            return ret;
        }
    }
    public partial class KalturaReminderListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Reminders != null)
            {
                propertyValue = "[" + String.Join(", ", Reminders.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Reminders != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Reminders.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaSeasonsReminderFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(EpgChannelIdEqual.HasValue)
            {
                ret.Add("\"epgChannelIdEqual\": " + EpgChannelIdEqual);
            }
            if(SeasonNumberIn != null)
            {
                ret.Add("\"seasonNumberIn\": " + "\"" + EscapeJson(SeasonNumberIn) + "\"");
            }
            if(SeriesIdEqual != null)
            {
                ret.Add("\"seriesIdEqual\": " + "\"" + EscapeJson(SeriesIdEqual) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(EpgChannelIdEqual.HasValue)
            {
                ret += "<epgChannelIdEqual>" + EpgChannelIdEqual + "</epgChannelIdEqual>";
            }
            if(SeasonNumberIn != null)
            {
                ret += "<seasonNumberIn>" + EscapeXml(SeasonNumberIn) + "</seasonNumberIn>";
            }
            if(SeriesIdEqual != null)
            {
                ret += "<seriesIdEqual>" + EscapeXml(SeriesIdEqual) + "</seriesIdEqual>";
            }
            return ret;
        }
    }
    public partial class KalturaSeriesReminderFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(EpgChannelIdEqual.HasValue)
            {
                ret.Add("\"epgChannelIdEqual\": " + EpgChannelIdEqual);
            }
            if(SeriesIdIn != null)
            {
                ret.Add("\"seriesIdIn\": " + "\"" + EscapeJson(SeriesIdIn) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(EpgChannelIdEqual.HasValue)
            {
                ret += "<epgChannelIdEqual>" + EpgChannelIdEqual + "</epgChannelIdEqual>";
            }
            if(SeriesIdIn != null)
            {
                ret += "<seriesIdIn>" + EscapeXml(SeriesIdIn) + "</seriesIdIn>";
            }
            return ret;
        }
    }
    public partial class KalturaTopic
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"automaticIssueNotification\": " + "\"" + Enum.GetName(typeof(KalturaTopicAutomaticIssueNotification), AutomaticIssueNotification) + "\"");
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + EscapeJson(Id) + "\"");
            }
            ret.Add("\"lastMessageSentDateSec\": " + LastMessageSentDateSec);
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(SubscribersAmount != null)
            {
                ret.Add("\"subscribersAmount\": " + "\"" + EscapeJson(SubscribersAmount) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<automaticIssueNotification>" + "\"" + Enum.GetName(typeof(KalturaTopicAutomaticIssueNotification), AutomaticIssueNotification) + "\"" + "</automaticIssueNotification>";
            if(Id != null)
            {
                ret += "<id>" + EscapeXml(Id) + "</id>";
            }
            ret += "<lastMessageSentDateSec>" + LastMessageSentDateSec + "</lastMessageSentDateSec>";
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(SubscribersAmount != null)
            {
                ret += "<subscribersAmount>" + EscapeXml(SubscribersAmount) + "</subscribersAmount>";
            }
            return ret;
        }
    }
    public partial class KalturaTopicFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaTopicListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Topics != null)
            {
                propertyValue = "[" + String.Join(", ", Topics.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Topics != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Topics.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaTopicResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Topics != null)
            {
                propertyValue = "[" + String.Join(", ", Topics.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Topics != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Topics.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
}

namespace WebAPI.App_Start
{
    public partial class KalturaAPIException
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(args != null)
            {
                propertyValue = "[" + String.Join(", ", args.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"args\": " + propertyValue);
            }
            if(code != null)
            {
                ret.Add("\"code\": " + "\"" + EscapeJson(code) + "\"");
            }
            if(message != null)
            {
                ret.Add("\"message\": " + "\"" + EscapeJson(message) + "\"");
            }
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + EscapeJson(objectType) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(args != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", args.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<args>" + propertyValue + "</args>";
            }
            if(code != null)
            {
                ret += "<code>" + EscapeXml(code) + "</code>";
            }
            if(message != null)
            {
                ret += "<message>" + EscapeXml(message) + "</message>";
            }
            if(objectType != null)
            {
                ret += "<objectType>" + EscapeXml(objectType) + "</objectType>";
            }
            return ret;
        }
    }
    public partial class KalturaApiExceptionArg
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(name) + "\"");
            }
            if(value != null)
            {
                ret.Add("\"value\": " + "\"" + EscapeJson(value) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(name != null)
            {
                ret += "<name>" + EscapeXml(name) + "</name>";
            }
            if(value != null)
            {
                ret += "<value>" + EscapeXml(value) + "</value>";
            }
            return ret;
        }
    }
    public partial class KalturaAPIExceptionWrapper
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(error != null)
            {
                propertyValue = error.ToJson(currentVersion, omitObsolete);
                ret.Add("\"error\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(error != null)
            {
                propertyValue = error.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<error>" + propertyValue + "</error>";
            }
            return ret;
        }
    }
}

namespace WebAPI.Models.Catalog
{
    public partial class KalturaAsset
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"createDate\": " + CreateDate);
            ret.Add(Description.ToCustomJson(currentVersion, omitObsolete, "description"));
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && EnableCatchUp.HasValue)
            {
                ret.Add("\"enableCatchUp\": " + EnableCatchUp.ToString().ToLower());
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && EnableCdvr.HasValue)
            {
                ret.Add("\"enableCdvr\": " + EnableCdvr.ToString().ToLower());
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && EnableStartOver.HasValue)
            {
                ret.Add("\"enableStartOver\": " + EnableStartOver.ToString().ToLower());
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && EnableTrickPlay.HasValue)
            {
                ret.Add("\"enableTrickPlay\": " + EnableTrickPlay.ToString().ToLower());
            }
            if(EndDate.HasValue)
            {
                ret.Add("\"endDate\": " + EndDate);
            }
            if(ExternalId != null)
            {
                ret.Add("\"externalId\": " + "\"" + EscapeJson(ExternalId) + "\"");
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Images != null)
            {
                propertyValue = "[" + String.Join(", ", Images.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"images\": " + propertyValue);
            }
            if(MediaFiles != null)
            {
                propertyValue = "[" + String.Join(", ", MediaFiles.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"mediaFiles\": " + propertyValue);
            }
            if(Metas != null)
            {
                propertyValue = "{" + String.Join(", ", Metas.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"metas\": " + propertyValue);
            }
            ret.Add(Name.ToCustomJson(currentVersion, omitObsolete, "name"));
            if(StartDate.HasValue)
            {
                ret.Add("\"startDate\": " + StartDate);
            }
            if(!omitObsolete && Statistics != null)
            {
                propertyValue = Statistics.ToJson(currentVersion, omitObsolete);
                ret.Add("\"stats\": " + propertyValue);
            }
            if(Tags != null)
            {
                propertyValue = "{" + String.Join(", ", Tags.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"tags\": " + propertyValue);
            }
            if(Type.HasValue)
            {
                ret.Add("\"type\": " + Type);
            }
            ret.Add("\"updateDate\": " + UpdateDate);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<createDate>" + CreateDate + "</createDate>";
            ret += Description.ToCustomXml(currentVersion, omitObsolete, "description");
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && EnableCatchUp.HasValue)
            {
                ret += "<enableCatchUp>" + EnableCatchUp.ToString().ToLower() + "</enableCatchUp>";
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && EnableCdvr.HasValue)
            {
                ret += "<enableCdvr>" + EnableCdvr.ToString().ToLower() + "</enableCdvr>";
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && EnableStartOver.HasValue)
            {
                ret += "<enableStartOver>" + EnableStartOver.ToString().ToLower() + "</enableStartOver>";
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && EnableTrickPlay.HasValue)
            {
                ret += "<enableTrickPlay>" + EnableTrickPlay.ToString().ToLower() + "</enableTrickPlay>";
            }
            if(EndDate.HasValue)
            {
                ret += "<endDate>" + EndDate + "</endDate>";
            }
            if(ExternalId != null)
            {
                ret += "<externalId>" + EscapeXml(ExternalId) + "</externalId>";
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(Images != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Images.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<images>" + propertyValue + "</images>";
            }
            if(MediaFiles != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", MediaFiles.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<mediaFiles>" + propertyValue + "</mediaFiles>";
            }
            if(Metas != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Metas.Select(pair => "<itemKey>" + pair.Key + "</itemKey>" + pair.Value.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<metas>" + propertyValue + "</metas>";
            }
            ret += Name.ToCustomXml(currentVersion, omitObsolete, "name");
            if(StartDate.HasValue)
            {
                ret += "<startDate>" + StartDate + "</startDate>";
            }
            if(!omitObsolete && Statistics != null)
            {
                propertyValue = Statistics.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<stats>" + propertyValue + "</stats>";
            }
            if(Tags != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Tags.Select(pair => "<itemKey>" + pair.Key + "</itemKey>" + pair.Value.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<tags>" + propertyValue + "</tags>";
            }
            if(Type.HasValue)
            {
                ret += "<type>" + Type + "</type>";
            }
            ret += "<updateDate>" + UpdateDate + "</updateDate>";
            return ret;
        }
    }
    public partial class KalturaAssetBookmark
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(IsFinishedWatching.HasValue)
            {
                ret.Add("\"finishedWatching\": " + IsFinishedWatching.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"finished_watching\": " + IsFinishedWatching.ToString().ToLower());
                }
            }
            if(Position.HasValue)
            {
                ret.Add("\"position\": " + Position);
            }
            ret.Add("\"positionOwner\": " + "\"" + Enum.GetName(typeof(KalturaPositionOwner), PositionOwner) + "\"");
            if (currentVersion == null || isOldVersion)
            {
                ret.Add("\"position_owner\": " + "\"" + Enum.GetName(typeof(KalturaPositionOwner), PositionOwner) + "\"");
            }
            if(User != null)
            {
                propertyValue = User.ToJson(currentVersion, omitObsolete);
                ret.Add("\"user\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(IsFinishedWatching.HasValue)
            {
                ret += "<finishedWatching>" + IsFinishedWatching.ToString().ToLower() + "</finishedWatching>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<finished_watching>" + IsFinishedWatching.ToString().ToLower() + "</finished_watching>";
                }
            }
            if(Position.HasValue)
            {
                ret += "<position>" + Position + "</position>";
            }
            ret += "<positionOwner>" + "\"" + Enum.GetName(typeof(KalturaPositionOwner), PositionOwner) + "\"" + "</positionOwner>";
            if (currentVersion == null || isOldVersion)
            {
            ret += "<position_owner>" + "\"" + Enum.GetName(typeof(KalturaPositionOwner), PositionOwner) + "\"" + "</position_owner>";
            }
            if(User != null)
            {
                propertyValue = User.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<user>" + propertyValue + "</user>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetBookmarks
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Bookmarks != null)
            {
                propertyValue = "[" + String.Join(", ", Bookmarks.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Bookmarks != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Bookmarks.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetComment
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"assetId\": " + AssetId);
            ret.Add("\"assetType\": " + "\"" + Enum.GetName(typeof(KalturaAssetType), AssetType) + "\"");
            ret.Add("\"id\": " + Id);
            if(SubHeader != null)
            {
                ret.Add("\"subHeader\": " + "\"" + EscapeJson(SubHeader) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<assetId>" + AssetId + "</assetId>";
            ret += "<assetType>" + "\"" + Enum.GetName(typeof(KalturaAssetType), AssetType) + "\"" + "</assetType>";
            ret += "<id>" + Id + "</id>";
            if(SubHeader != null)
            {
                ret += "<subHeader>" + EscapeXml(SubHeader) + "</subHeader>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetCommentFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"assetIdEqual\": " + AssetIdEqual);
            ret.Add("\"assetTypeEqual\": " + "\"" + Enum.GetName(typeof(KalturaAssetType), AssetTypeEqual) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<assetIdEqual>" + AssetIdEqual + "</assetIdEqual>";
            ret += "<assetTypeEqual>" + "\"" + Enum.GetName(typeof(KalturaAssetType), AssetTypeEqual) + "\"" + "</assetTypeEqual>";
            return ret;
        }
    }
    public partial class KalturaAssetCommentListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetCount
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"count\": " + Count);
            if(SubCounts != null)
            {
                propertyValue = "[" + String.Join(", ", SubCounts.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"subs\": " + propertyValue);
            }
            if(Value != null)
            {
                ret.Add("\"value\": " + "\"" + EscapeJson(Value) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<count>" + Count + "</count>";
            if(SubCounts != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", SubCounts.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<subs>" + propertyValue + "</subs>";
            }
            if(Value != null)
            {
                ret += "<value>" + EscapeXml(Value) + "</value>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetCountListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"assetsCount\": " + AssetsCount);
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<assetsCount>" + AssetsCount + "</assetsCount>";
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetFieldGroupBy
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"value\": " + "\"" + Enum.GetName(typeof(KalturaGroupByField), Value) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<value>" + "\"" + Enum.GetName(typeof(KalturaGroupByField), Value) + "\"" + "</value>";
            return ret;
        }
    }
    public partial class KalturaAssetFile
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Url != null)
            {
                ret.Add("\"url\": " + "\"" + EscapeJson(Url) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Url != null)
            {
                ret += "<url>" + EscapeXml(Url) + "</url>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(DynamicOrderBy != null)
            {
                propertyValue = DynamicOrderBy.ToJson(currentVersion, omitObsolete);
                ret.Add("\"dynamicOrderBy\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(DynamicOrderBy != null)
            {
                propertyValue = DynamicOrderBy.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<dynamicOrderBy>" + propertyValue + "</dynamicOrderBy>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetGroupBy
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaAssetHistory
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"assetId\": " + AssetId);
            ret.Add("\"assetType\": " + "\"" + Enum.GetName(typeof(KalturaAssetType), AssetType) + "\"");
            if(Duration.HasValue)
            {
                ret.Add("\"duration\": " + Duration);
            }
            if(IsFinishedWatching.HasValue)
            {
                ret.Add("\"finishedWatching\": " + IsFinishedWatching.ToString().ToLower());
            }
            if(LastWatched.HasValue)
            {
                ret.Add("\"watchedDate\": " + LastWatched);
            }
            if(Position.HasValue)
            {
                ret.Add("\"position\": " + Position);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<assetId>" + AssetId + "</assetId>";
            ret += "<assetType>" + "\"" + Enum.GetName(typeof(KalturaAssetType), AssetType) + "\"" + "</assetType>";
            if(Duration.HasValue)
            {
                ret += "<duration>" + Duration + "</duration>";
            }
            if(IsFinishedWatching.HasValue)
            {
                ret += "<finishedWatching>" + IsFinishedWatching.ToString().ToLower() + "</finishedWatching>";
            }
            if(LastWatched.HasValue)
            {
                ret += "<watchedDate>" + LastWatched + "</watchedDate>";
            }
            if(Position.HasValue)
            {
                ret += "<position>" + Position + "</position>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetHistoryFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetIdIn != null)
            {
                ret.Add("\"assetIdIn\": " + "\"" + EscapeJson(AssetIdIn) + "\"");
            }
            if(DaysLessThanOrEqual.HasValue)
            {
                ret.Add("\"daysLessThanOrEqual\": " + DaysLessThanOrEqual);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"days\": " + DaysLessThanOrEqual);
                }
            }
            if(!omitObsolete && filterTypes != null)
            {
                propertyValue = "[" + String.Join(", ", filterTypes.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"filterTypes\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"filter_types\": " + propertyValue);
                }
            }
            if(StatusEqual.HasValue)
            {
                ret.Add("\"statusEqual\": " + "\"" + Enum.GetName(typeof(KalturaWatchStatus), StatusEqual) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"filter_status\": " + "\"" + Enum.GetName(typeof(KalturaWatchStatus), StatusEqual) + "\"");
                }
            }
            if(TypeIn != null)
            {
                ret.Add("\"typeIn\": " + "\"" + EscapeJson(TypeIn) + "\"");
            }
            if(!omitObsolete && with != null)
            {
                propertyValue = "[" + String.Join(", ", with.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"with\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetIdIn != null)
            {
                ret += "<assetIdIn>" + EscapeXml(AssetIdIn) + "</assetIdIn>";
            }
            if(DaysLessThanOrEqual.HasValue)
            {
                ret += "<daysLessThanOrEqual>" + DaysLessThanOrEqual + "</daysLessThanOrEqual>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<days>" + DaysLessThanOrEqual + "</days>";
                }
            }
            if(!omitObsolete && filterTypes != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", filterTypes.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<filterTypes>" + propertyValue + "</filterTypes>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<filter_types>" + propertyValue + "</filter_types>";
                }
            }
            if(StatusEqual.HasValue)
            {
                ret += "<statusEqual>" + "\"" + Enum.GetName(typeof(KalturaWatchStatus), StatusEqual) + "\"" + "</statusEqual>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<filter_status>" + "\"" + Enum.GetName(typeof(KalturaWatchStatus), StatusEqual) + "\"" + "</filter_status>";
                }
            }
            if(TypeIn != null)
            {
                ret += "<typeIn>" + EscapeXml(TypeIn) + "</typeIn>";
            }
            if(!omitObsolete && with != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", with.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<with>" + propertyValue + "</with>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetHistoryListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetInfo
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(EndDate.HasValue)
            {
                ret.Add("\"endDate\": " + EndDate);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"end_date\": " + EndDate);
                }
            }
            if(ExtraParams != null)
            {
                propertyValue = "{" + String.Join(", ", ExtraParams.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"extraParams\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"extra_params\": " + propertyValue);
                }
            }
            if(Metas != null)
            {
                propertyValue = "{" + String.Join(", ", Metas.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"metas\": " + propertyValue);
            }
            if(StartDate.HasValue)
            {
                ret.Add("\"startDate\": " + StartDate);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"start_date\": " + StartDate);
                }
            }
            if(Tags != null)
            {
                propertyValue = "{" + String.Join(", ", Tags.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"tags\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(EndDate.HasValue)
            {
                ret += "<endDate>" + EndDate + "</endDate>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<end_date>" + EndDate + "</end_date>";
                }
            }
            if(ExtraParams != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", ExtraParams.Select(pair => "<itemKey>" + pair.Key + "</itemKey>" + pair.Value.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<extraParams>" + propertyValue + "</extraParams>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<extra_params>" + propertyValue + "</extra_params>";
                }
            }
            if(Metas != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Metas.Select(pair => "<itemKey>" + pair.Key + "</itemKey>" + pair.Value.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<metas>" + propertyValue + "</metas>";
            }
            if(StartDate.HasValue)
            {
                ret += "<startDate>" + StartDate + "</startDate>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<start_date>" + StartDate + "</start_date>";
                }
            }
            if(Tags != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Tags.Select(pair => "<itemKey>" + pair.Key + "</itemKey>" + pair.Value.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<tags>" + propertyValue + "</tags>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetInfoFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"cut_with\": " + "\"" + Enum.GetName(typeof(KalturaCutWith), cutWith) + "\"");
            if(FilterTags != null)
            {
                propertyValue = "{" + String.Join(", ", FilterTags.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"filter_tags\": " + propertyValue);
            }
            if(IDs != null)
            {
                propertyValue = "[" + String.Join(", ", IDs.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"ids\": " + propertyValue);
            }
            ret.Add("\"referenceType\": " + "\"" + Enum.GetName(typeof(KalturaCatalogReferenceBy), ReferenceType) + "\"");
            if (currentVersion == null || isOldVersion)
            {
                ret.Add("\"reference_type\": " + "\"" + Enum.GetName(typeof(KalturaCatalogReferenceBy), ReferenceType) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<cut_with>" + "\"" + Enum.GetName(typeof(KalturaCutWith), cutWith) + "\"" + "</cut_with>";
            if(FilterTags != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", FilterTags.Select(pair => "<itemKey>" + pair.Key + "</itemKey>" + pair.Value.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<filter_tags>" + propertyValue + "</filter_tags>";
            }
            if(IDs != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", IDs.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<ids>" + propertyValue + "</ids>";
            }
            ret += "<referenceType>" + "\"" + Enum.GetName(typeof(KalturaCatalogReferenceBy), ReferenceType) + "\"" + "</referenceType>";
            if (currentVersion == null || isOldVersion)
            {
            ret += "<reference_type>" + "\"" + Enum.GetName(typeof(KalturaCatalogReferenceBy), ReferenceType) + "\"" + "</reference_type>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetInfoListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            if(RequestId != null)
            {
                ret.Add("\"requestId\": " + "\"" + EscapeJson(RequestId) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"request_id\": " + "\"" + EscapeJson(RequestId) + "\"");
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            if(RequestId != null)
            {
                ret += "<requestId>" + EscapeXml(RequestId) + "</requestId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<request_id>" + EscapeXml(RequestId) + "</request_id>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaAssetListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetMetaOrTagGroupBy
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Value != null)
            {
                ret.Add("\"value\": " + "\"" + EscapeJson(Value) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Value != null)
            {
                ret += "<value>" + EscapeXml(Value) + "</value>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetsBookmarksResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetsBookmarks != null)
            {
                propertyValue = "[" + String.Join(", ", AssetsBookmarks.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetsBookmarks != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", AssetsBookmarks.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetsCount
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Field != null)
            {
                ret.Add("\"field\": " + "\"" + EscapeJson(Field) + "\"");
            }
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Field != null)
            {
                ret += "<field>" + EscapeXml(Field) + "</field>";
            }
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetsFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Assets != null)
            {
                propertyValue = "[" + String.Join(", ", Assets.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"assets\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"Assets\": " + propertyValue);
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Assets != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Assets.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<assets>" + propertyValue + "</assets>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<Assets>" + propertyValue + "</Assets>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaAssetStatistics
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"assetId\": " + AssetId);
            if (currentVersion == null || isOldVersion)
            {
                ret.Add("\"asset_id\": " + AssetId);
            }
            if(BuzzAvgScore != null)
            {
                propertyValue = BuzzAvgScore.ToJson(currentVersion, omitObsolete);
                ret.Add("\"buzzScore\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"buzz_score\": " + propertyValue);
                }
            }
            ret.Add("\"likes\": " + Likes);
            ret.Add("\"rating\": " + Rating);
            ret.Add("\"ratingCount\": " + RatingCount);
            if (currentVersion == null || isOldVersion)
            {
                ret.Add("\"rating_count\": " + RatingCount);
            }
            ret.Add("\"views\": " + Views);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<assetId>" + AssetId + "</assetId>";
            if (currentVersion == null || isOldVersion)
            {
            ret += "<asset_id>" + AssetId + "</asset_id>";
            }
            if(BuzzAvgScore != null)
            {
                propertyValue = BuzzAvgScore.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<buzzScore>" + propertyValue + "</buzzScore>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<buzz_score>" + propertyValue + "</buzz_score>";
                }
            }
            ret += "<likes>" + Likes + "</likes>";
            ret += "<rating>" + Rating + "</rating>";
            ret += "<ratingCount>" + RatingCount + "</ratingCount>";
            if (currentVersion == null || isOldVersion)
            {
            ret += "<rating_count>" + RatingCount + "</rating_count>";
            }
            ret += "<views>" + Views + "</views>";
            return ret;
        }
    }
    public partial class KalturaAssetStatisticsListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetsStatistics != null)
            {
                propertyValue = "[" + String.Join(", ", AssetsStatistics.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetsStatistics != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", AssetsStatistics.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetStatisticsQuery
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetIdIn != null)
            {
                ret.Add("\"assetIdIn\": " + "\"" + EscapeJson(AssetIdIn) + "\"");
            }
            ret.Add("\"assetTypeEqual\": " + "\"" + Enum.GetName(typeof(KalturaAssetType), AssetTypeEqual) + "\"");
            ret.Add("\"endDateGreaterThanOrEqual\": " + EndDateGreaterThanOrEqual);
            ret.Add("\"startDateGreaterThanOrEqual\": " + StartDateGreaterThanOrEqual);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetIdIn != null)
            {
                ret += "<assetIdIn>" + EscapeXml(AssetIdIn) + "</assetIdIn>";
            }
            ret += "<assetTypeEqual>" + "\"" + Enum.GetName(typeof(KalturaAssetType), AssetTypeEqual) + "\"" + "</assetTypeEqual>";
            ret += "<endDateGreaterThanOrEqual>" + EndDateGreaterThanOrEqual + "</endDateGreaterThanOrEqual>";
            ret += "<startDateGreaterThanOrEqual>" + StartDateGreaterThanOrEqual + "</startDateGreaterThanOrEqual>";
            return ret;
        }
    }
    public partial class KalturaAssetStruct
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ConnectedParentMetaId.HasValue)
            {
                ret.Add("\"connectedParentMetaId\": " + ConnectedParentMetaId);
            }
            if(ConnectingMetaId.HasValue)
            {
                ret.Add("\"connectingMetaId\": " + ConnectingMetaId);
            }
            ret.Add("\"createDate\": " + CreateDate);
            if(Features != null)
            {
                ret.Add("\"features\": " + "\"" + EscapeJson(Features) + "\"");
            }
            ret.Add("\"id\": " + Id);
            if(IsProtected.HasValue)
            {
                ret.Add("\"isProtected\": " + IsProtected.ToString().ToLower());
            }
            if(MetaIds != null)
            {
                ret.Add("\"metaIds\": " + "\"" + EscapeJson(MetaIds) + "\"");
            }
            ret.Add(Name.ToCustomJson(currentVersion, omitObsolete, "name"));
            if(ParentId.HasValue)
            {
                ret.Add("\"parentId\": " + ParentId);
            }
            if(PluralName != null)
            {
                ret.Add("\"pluralName\": " + "\"" + EscapeJson(PluralName) + "\"");
            }
            if(SystemName != null)
            {
                ret.Add("\"systemName\": " + "\"" + EscapeJson(SystemName) + "\"");
            }
            ret.Add("\"updateDate\": " + UpdateDate);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ConnectedParentMetaId.HasValue)
            {
                ret += "<connectedParentMetaId>" + ConnectedParentMetaId + "</connectedParentMetaId>";
            }
            if(ConnectingMetaId.HasValue)
            {
                ret += "<connectingMetaId>" + ConnectingMetaId + "</connectingMetaId>";
            }
            ret += "<createDate>" + CreateDate + "</createDate>";
            if(Features != null)
            {
                ret += "<features>" + EscapeXml(Features) + "</features>";
            }
            ret += "<id>" + Id + "</id>";
            if(IsProtected.HasValue)
            {
                ret += "<isProtected>" + IsProtected.ToString().ToLower() + "</isProtected>";
            }
            if(MetaIds != null)
            {
                ret += "<metaIds>" + EscapeXml(MetaIds) + "</metaIds>";
            }
            ret += Name.ToCustomXml(currentVersion, omitObsolete, "name");
            if(ParentId.HasValue)
            {
                ret += "<parentId>" + ParentId + "</parentId>";
            }
            if(PluralName != null)
            {
                ret += "<pluralName>" + EscapeXml(PluralName) + "</pluralName>";
            }
            if(SystemName != null)
            {
                ret += "<systemName>" + EscapeXml(SystemName) + "</systemName>";
            }
            ret += "<updateDate>" + UpdateDate + "</updateDate>";
            return ret;
        }
    }
    public partial class KalturaAssetStructFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(IdIn != null)
            {
                ret.Add("\"idIn\": " + "\"" + EscapeJson(IdIn) + "\"");
            }
            if(IsProtectedEqual.HasValue)
            {
                ret.Add("\"isProtectedEqual\": " + IsProtectedEqual.ToString().ToLower());
            }
            if(MetaIdEqual.HasValue)
            {
                ret.Add("\"metaIdEqual\": " + MetaIdEqual);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(IdIn != null)
            {
                ret += "<idIn>" + EscapeXml(IdIn) + "</idIn>";
            }
            if(IsProtectedEqual.HasValue)
            {
                ret += "<isProtectedEqual>" + IsProtectedEqual.ToString().ToLower() + "</isProtectedEqual>";
            }
            if(MetaIdEqual.HasValue)
            {
                ret += "<metaIdEqual>" + MetaIdEqual + "</metaIdEqual>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetStructListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetStructs != null)
            {
                propertyValue = "[" + String.Join(", ", AssetStructs.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetStructs != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", AssetStructs.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetStructMeta
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"assetStructId\": " + AssetStructId);
            ret.Add("\"createDate\": " + CreateDate);
            if(DefaultIngestValue != null)
            {
                ret.Add("\"defaultIngestValue\": " + "\"" + EscapeJson(DefaultIngestValue) + "\"");
            }
            if(IngestReferencePath != null)
            {
                ret.Add("\"ingestReferencePath\": " + "\"" + EscapeJson(IngestReferencePath) + "\"");
            }
            ret.Add("\"metaId\": " + MetaId);
            if(ProtectFromIngest.HasValue)
            {
                ret.Add("\"protectFromIngest\": " + ProtectFromIngest.ToString().ToLower());
            }
            ret.Add("\"updateDate\": " + UpdateDate);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<assetStructId>" + AssetStructId + "</assetStructId>";
            ret += "<createDate>" + CreateDate + "</createDate>";
            if(DefaultIngestValue != null)
            {
                ret += "<defaultIngestValue>" + EscapeXml(DefaultIngestValue) + "</defaultIngestValue>";
            }
            if(IngestReferencePath != null)
            {
                ret += "<ingestReferencePath>" + EscapeXml(IngestReferencePath) + "</ingestReferencePath>";
            }
            ret += "<metaId>" + MetaId + "</metaId>";
            if(ProtectFromIngest.HasValue)
            {
                ret += "<protectFromIngest>" + ProtectFromIngest.ToString().ToLower() + "</protectFromIngest>";
            }
            ret += "<updateDate>" + UpdateDate + "</updateDate>";
            return ret;
        }
    }
    public partial class KalturaAssetStructMetaFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetStructIdEqual.HasValue)
            {
                ret.Add("\"assetStructIdEqual\": " + AssetStructIdEqual);
            }
            if(MetaIdEqual.HasValue)
            {
                ret.Add("\"metaIdEqual\": " + MetaIdEqual);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetStructIdEqual.HasValue)
            {
                ret += "<assetStructIdEqual>" + AssetStructIdEqual + "</assetStructIdEqual>";
            }
            if(MetaIdEqual.HasValue)
            {
                ret += "<metaIdEqual>" + MetaIdEqual + "</metaIdEqual>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetStructMetaListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetStructMetas != null)
            {
                propertyValue = "[" + String.Join(", ", AssetStructMetas.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetStructMetas != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", AssetStructMetas.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaBaseAssetInfo
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Description != null)
            {
                ret.Add("\"description\": " + "\"" + EscapeJson(Description) + "\"");
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Images != null)
            {
                propertyValue = "[" + String.Join(", ", Images.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"images\": " + propertyValue);
            }
            if(MediaFiles != null)
            {
                propertyValue = "[" + String.Join(", ", MediaFiles.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"mediaFiles\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"media_files\": " + propertyValue);
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(!omitObsolete && Statistics != null)
            {
                propertyValue = Statistics.ToJson(currentVersion, omitObsolete);
                ret.Add("\"stats\": " + propertyValue);
            }
            if(Type.HasValue)
            {
                ret.Add("\"type\": " + Type);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Description != null)
            {
                ret += "<description>" + EscapeXml(Description) + "</description>";
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(Images != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Images.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<images>" + propertyValue + "</images>";
            }
            if(MediaFiles != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", MediaFiles.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<mediaFiles>" + propertyValue + "</mediaFiles>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<media_files>" + propertyValue + "</media_files>";
                }
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(!omitObsolete && Statistics != null)
            {
                propertyValue = Statistics.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<stats>" + propertyValue + "</stats>";
            }
            if(Type.HasValue)
            {
                ret += "<type>" + Type + "</type>";
            }
            return ret;
        }
    }
    public partial class KalturaBaseChannel
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            return ret;
        }
    }
    public partial class KalturaBaseSearchAssetFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(GroupBy != null)
            {
                propertyValue = "[" + String.Join(", ", GroupBy.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"groupBy\": " + propertyValue);
            }
            if(Ksql != null)
            {
                ret.Add("\"kSql\": " + "\"" + EscapeJson(Ksql) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(GroupBy != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", GroupBy.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<groupBy>" + propertyValue + "</groupBy>";
            }
            if(Ksql != null)
            {
                ret += "<kSql>" + EscapeXml(Ksql) + "</kSql>";
            }
            return ret;
        }
    }
    public partial class KalturaBookmark
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(IsFinishedWatching.HasValue)
            {
                ret.Add("\"finishedWatching\": " + IsFinishedWatching.ToString().ToLower());
            }
            ret.Add("\"isReportingMode\": " + IsReportingMode.ToString().ToLower());
            if(PlayerData != null)
            {
                propertyValue = PlayerData.ToJson(currentVersion, omitObsolete);
                ret.Add("\"playerData\": " + propertyValue);
            }
            if(Position.HasValue)
            {
                ret.Add("\"position\": " + Position);
            }
            ret.Add("\"positionOwner\": " + "\"" + Enum.GetName(typeof(KalturaPositionOwner), PositionOwner) + "\"");
            ret.Add("\"programId\": " + ProgramId);
            if(!omitObsolete && User != null)
            {
                propertyValue = User.ToJson(currentVersion, omitObsolete);
                ret.Add("\"user\": " + propertyValue);
            }
            if(UserId != null)
            {
                ret.Add("\"userId\": " + "\"" + EscapeJson(UserId) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(IsFinishedWatching.HasValue)
            {
                ret += "<finishedWatching>" + IsFinishedWatching.ToString().ToLower() + "</finishedWatching>";
            }
            ret += "<isReportingMode>" + IsReportingMode.ToString().ToLower() + "</isReportingMode>";
            if(PlayerData != null)
            {
                propertyValue = PlayerData.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<playerData>" + propertyValue + "</playerData>";
            }
            if(Position.HasValue)
            {
                ret += "<position>" + Position + "</position>";
            }
            ret += "<positionOwner>" + "\"" + Enum.GetName(typeof(KalturaPositionOwner), PositionOwner) + "\"" + "</positionOwner>";
            ret += "<programId>" + ProgramId + "</programId>";
            if(!omitObsolete && User != null)
            {
                propertyValue = User.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<user>" + propertyValue + "</user>";
            }
            if(UserId != null)
            {
                ret += "<userId>" + EscapeXml(UserId) + "</userId>";
            }
            return ret;
        }
    }
    public partial class KalturaBookmarkFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetIdIn != null)
            {
                ret.Add("\"assetIdIn\": " + "\"" + EscapeJson(AssetIdIn) + "\"");
            }
            if(!omitObsolete && AssetIn != null)
            {
                propertyValue = "[" + String.Join(", ", AssetIn.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"assetIn\": " + propertyValue);
            }
            if(AssetTypeEqual.HasValue)
            {
                ret.Add("\"assetTypeEqual\": " + "\"" + Enum.GetName(typeof(KalturaAssetType), AssetTypeEqual) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetIdIn != null)
            {
                ret += "<assetIdIn>" + EscapeXml(AssetIdIn) + "</assetIdIn>";
            }
            if(!omitObsolete && AssetIn != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", AssetIn.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<assetIn>" + propertyValue + "</assetIn>";
            }
            if(AssetTypeEqual.HasValue)
            {
                ret += "<assetTypeEqual>" + "\"" + Enum.GetName(typeof(KalturaAssetType), AssetTypeEqual) + "\"" + "</assetTypeEqual>";
            }
            return ret;
        }
    }
    public partial class KalturaBookmarkListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetsBookmarks != null)
            {
                propertyValue = "[" + String.Join(", ", AssetsBookmarks.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetsBookmarks != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", AssetsBookmarks.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaBookmarkPlayerData
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"action\": " + "\"" + Enum.GetName(typeof(KalturaBookmarkActionType), action) + "\"");
            if(averageBitRate.HasValue)
            {
                ret.Add("\"averageBitrate\": " + averageBitRate);
            }
            if(currentBitRate.HasValue)
            {
                ret.Add("\"currentBitrate\": " + currentBitRate);
            }
            if(FileId.HasValue)
            {
                ret.Add("\"fileId\": " + FileId);
            }
            if(totalBitRate.HasValue)
            {
                ret.Add("\"totalBitrate\": " + totalBitRate);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<action>" + "\"" + Enum.GetName(typeof(KalturaBookmarkActionType), action) + "\"" + "</action>";
            if(averageBitRate.HasValue)
            {
                ret += "<averageBitrate>" + averageBitRate + "</averageBitrate>";
            }
            if(currentBitRate.HasValue)
            {
                ret += "<currentBitrate>" + currentBitRate + "</currentBitrate>";
            }
            if(FileId.HasValue)
            {
                ret += "<fileId>" + FileId + "</fileId>";
            }
            if(totalBitRate.HasValue)
            {
                ret += "<totalBitrate>" + totalBitRate + "</totalBitrate>";
            }
            return ret;
        }
    }
    public partial class KalturaBundleFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"bundleTypeEqual\": " + "\"" + Enum.GetName(typeof(KalturaBundleType), BundleTypeEqual) + "\"");
            ret.Add("\"idEqual\": " + IdEqual);
            if(TypeIn != null)
            {
                ret.Add("\"typeIn\": " + "\"" + EscapeJson(TypeIn) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<bundleTypeEqual>" + "\"" + Enum.GetName(typeof(KalturaBundleType), BundleTypeEqual) + "\"" + "</bundleTypeEqual>";
            ret += "<idEqual>" + IdEqual + "</idEqual>";
            if(TypeIn != null)
            {
                ret += "<typeIn>" + EscapeXml(TypeIn) + "</typeIn>";
            }
            return ret;
        }
    }
    public partial class KalturaBuzzScore
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AvgScore.HasValue)
            {
                ret.Add("\"avgScore\": " + AvgScore);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"avg_score\": " + AvgScore);
                }
            }
            if(NormalizedAvgScore.HasValue)
            {
                ret.Add("\"normalizedAvgScore\": " + NormalizedAvgScore);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"normalized_avg_score\": " + NormalizedAvgScore);
                }
            }
            if(UpdateDate.HasValue)
            {
                ret.Add("\"updateDate\": " + UpdateDate);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"update_date\": " + UpdateDate);
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AvgScore.HasValue)
            {
                ret += "<avgScore>" + AvgScore + "</avgScore>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<avg_score>" + AvgScore + "</avg_score>";
                }
            }
            if(NormalizedAvgScore.HasValue)
            {
                ret += "<normalizedAvgScore>" + NormalizedAvgScore + "</normalizedAvgScore>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<normalized_avg_score>" + NormalizedAvgScore + "</normalized_avg_score>";
                }
            }
            if(UpdateDate.HasValue)
            {
                ret += "<updateDate>" + UpdateDate + "</updateDate>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<update_date>" + UpdateDate + "</update_date>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaCatalogWithHolder
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"type\": " + "\"" + Enum.GetName(typeof(KalturaCatalogWith), type) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<type>" + "\"" + Enum.GetName(typeof(KalturaCatalogWith), type) + "\"" + "</type>";
            return ret;
        }
    }
    public partial class KalturaChannel
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(!omitObsolete && !DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && AssetTypes != null)
            {
                propertyValue = "[" + String.Join(", ", AssetTypes.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"assetTypes\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"asset_types\": " + propertyValue);
                }
            }
            ret.Add("\"createDate\": " + CreateDate);
            ret.Add(Description.ToCustomJson(currentVersion, omitObsolete, "description"));
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && FilterExpression != null)
            {
                ret.Add("\"filterExpression\": " + "\"" + EscapeJson(FilterExpression) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"filter_expression\": " + "\"" + EscapeJson(FilterExpression) + "\"");
                }
            }
            if(!omitObsolete && !DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && GroupBy != null)
            {
                propertyValue = GroupBy.ToJson(currentVersion, omitObsolete);
                ret.Add("\"groupBy\": " + propertyValue);
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && Images != null)
            {
                propertyValue = "[" + String.Join(", ", Images.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"images\": " + propertyValue);
            }
            if(IsActive.HasValue)
            {
                ret.Add("\"isActive\": " + IsActive.ToString().ToLower());
            }
            if(!omitObsolete && !DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && MediaTypes != null)
            {
                propertyValue = "[" + String.Join(", ", MediaTypes.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"media_types\": " + propertyValue);
            }
            ret.Add(Name.ToCustomJson(currentVersion, omitObsolete, "name"));
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && Order.HasValue)
            {
                ret.Add("\"order\": " + "\"" + Enum.GetName(typeof(KalturaAssetOrderBy), Order) + "\"");
            }
            if(OrderBy != null)
            {
                propertyValue = OrderBy.ToJson(currentVersion, omitObsolete);
                ret.Add("\"orderBy\": " + propertyValue);
            }
            if(SystemName != null)
            {
                ret.Add("\"systemName\": " + "\"" + EscapeJson(SystemName) + "\"");
            }
            ret.Add("\"updateDate\": " + UpdateDate);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(!omitObsolete && !DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && AssetTypes != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", AssetTypes.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<assetTypes>" + propertyValue + "</assetTypes>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<asset_types>" + propertyValue + "</asset_types>";
                }
            }
            ret += "<createDate>" + CreateDate + "</createDate>";
            ret += Description.ToCustomXml(currentVersion, omitObsolete, "description");
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && FilterExpression != null)
            {
                ret += "<filterExpression>" + EscapeXml(FilterExpression) + "</filterExpression>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<filter_expression>" + EscapeXml(FilterExpression) + "</filter_expression>";
                }
            }
            if(!omitObsolete && !DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && GroupBy != null)
            {
                propertyValue = GroupBy.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<groupBy>" + propertyValue + "</groupBy>";
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && Images != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Images.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<images>" + propertyValue + "</images>";
            }
            if(IsActive.HasValue)
            {
                ret += "<isActive>" + IsActive.ToString().ToLower() + "</isActive>";
            }
            if(!omitObsolete && !DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && MediaTypes != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", MediaTypes.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<media_types>" + propertyValue + "</media_types>";
            }
            ret += Name.ToCustomXml(currentVersion, omitObsolete, "name");
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && Order.HasValue)
            {
                ret += "<order>" + "\"" + Enum.GetName(typeof(KalturaAssetOrderBy), Order) + "\"" + "</order>";
            }
            if(OrderBy != null)
            {
                propertyValue = OrderBy.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<orderBy>" + propertyValue + "</orderBy>";
            }
            if(SystemName != null)
            {
                ret += "<systemName>" + EscapeXml(SystemName) + "</systemName>";
            }
            ret += "<updateDate>" + UpdateDate + "</updateDate>";
            return ret;
        }
    }
    public partial class KalturaChannelExternalFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(FreeText != null)
            {
                ret.Add("\"freeText\": " + "\"" + EscapeJson(FreeText) + "\"");
            }
            ret.Add("\"idEqual\": " + IdEqual);
            ret.Add("\"utcOffsetEqual\": " + UtcOffsetEqual);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(FreeText != null)
            {
                ret += "<freeText>" + EscapeXml(FreeText) + "</freeText>";
            }
            ret += "<idEqual>" + IdEqual + "</idEqual>";
            ret += "<utcOffsetEqual>" + UtcOffsetEqual + "</utcOffsetEqual>";
            return ret;
        }
    }
    public partial class KalturaChannelFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"excludeWatched\": " + ExcludeWatched.ToString().ToLower());
            ret.Add("\"idEqual\": " + IdEqual);
            if(KSql != null)
            {
                ret.Add("\"kSql\": " + "\"" + EscapeJson(KSql) + "\"");
            }
            ret.Add("\"orderBy\": " + "\"" + Enum.GetName(typeof(KalturaAssetOrderBy), OrderBy) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<excludeWatched>" + ExcludeWatched.ToString().ToLower() + "</excludeWatched>";
            ret += "<idEqual>" + IdEqual + "</idEqual>";
            if(KSql != null)
            {
                ret += "<kSql>" + EscapeXml(KSql) + "</kSql>";
            }
            ret += "<orderBy>" + "\"" + Enum.GetName(typeof(KalturaAssetOrderBy), OrderBy) + "\"" + "</orderBy>";
            return ret;
        }
    }
    public partial class KalturaChannelListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Channels != null)
            {
                propertyValue = "[" + String.Join(", ", Channels.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Channels != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Channels.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaChannelOrder
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(DynamicOrderBy != null)
            {
                propertyValue = DynamicOrderBy.ToJson(currentVersion, omitObsolete);
                ret.Add("\"dynamicOrderBy\": " + propertyValue);
            }
            if(orderBy.HasValue)
            {
                ret.Add("\"orderBy\": " + "\"" + Enum.GetName(typeof(KalturaChannelOrderBy), orderBy) + "\"");
            }
            if(SlidingWindowPeriod.HasValue)
            {
                ret.Add("\"period\": " + SlidingWindowPeriod);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(DynamicOrderBy != null)
            {
                propertyValue = DynamicOrderBy.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<dynamicOrderBy>" + propertyValue + "</dynamicOrderBy>";
            }
            if(orderBy.HasValue)
            {
                ret += "<orderBy>" + "\"" + Enum.GetName(typeof(KalturaChannelOrderBy), orderBy) + "\"" + "</orderBy>";
            }
            if(SlidingWindowPeriod.HasValue)
            {
                ret += "<period>" + SlidingWindowPeriod + "</period>";
            }
            return ret;
        }
    }
    public partial class KalturaChannelsFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"idEqual\": " + IdEqual);
            ret.Add("\"mediaIdEqual\": " + MediaIdEqual);
            if(NameEqual != null)
            {
                ret.Add("\"nameEqual\": " + "\"" + EscapeJson(NameEqual) + "\"");
            }
            if(NameStartsWith != null)
            {
                ret.Add("\"nameStartsWith\": " + "\"" + EscapeJson(NameStartsWith) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<idEqual>" + IdEqual + "</idEqual>";
            ret += "<mediaIdEqual>" + MediaIdEqual + "</mediaIdEqual>";
            if(NameEqual != null)
            {
                ret += "<nameEqual>" + EscapeXml(NameEqual) + "</nameEqual>";
            }
            if(NameStartsWith != null)
            {
                ret += "<nameStartsWith>" + EscapeXml(NameStartsWith) + "</nameStartsWith>";
            }
            return ret;
        }
    }
    public partial class KalturaContentResource
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaDynamicChannel
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(!isOldVersion && AssetTypes != null)
            {
                propertyValue = "[" + String.Join(", ", AssetTypes.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"assetTypes\": " + propertyValue);
            }
            if(!isOldVersion && GroupBy != null)
            {
                propertyValue = GroupBy.ToJson(currentVersion, omitObsolete);
                ret.Add("\"groupBy\": " + propertyValue);
            }
            if(Ksql != null)
            {
                ret.Add("\"kSql\": " + "\"" + EscapeJson(Ksql) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(!isOldVersion && AssetTypes != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", AssetTypes.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<assetTypes>" + propertyValue + "</assetTypes>";
            }
            if(!isOldVersion && GroupBy != null)
            {
                propertyValue = GroupBy.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<groupBy>" + propertyValue + "</groupBy>";
            }
            if(Ksql != null)
            {
                ret += "<kSql>" + EscapeXml(Ksql) + "</kSql>";
            }
            return ret;
        }
    }
    public partial class KalturaDynamicOrderBy
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(OrderBy.HasValue)
            {
                ret.Add("\"orderBy\": " + "\"" + Enum.GetName(typeof(KalturaMetaTagOrderBy), OrderBy) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(OrderBy.HasValue)
            {
                ret += "<orderBy>" + "\"" + Enum.GetName(typeof(KalturaMetaTagOrderBy), OrderBy) + "\"" + "</orderBy>";
            }
            return ret;
        }
    }
    public partial class KalturaEPGChannelAssets
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Assets != null)
            {
                propertyValue = "[" + String.Join(", ", Assets.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            if(ChannelID.HasValue)
            {
                ret.Add("\"channelId\": " + ChannelID);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"channel_id\": " + ChannelID);
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Assets != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Assets.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            if(ChannelID.HasValue)
            {
                ret += "<channelId>" + ChannelID + "</channelId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<channel_id>" + ChannelID + "</channel_id>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaEPGChannelAssetsListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Channels != null)
            {
                propertyValue = "[" + String.Join(", ", Channels.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"assets\": " + propertyValue);
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Channels != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Channels.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<assets>" + propertyValue + "</assets>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaEpgChannelFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(EndTime.HasValue)
            {
                ret.Add("\"endTime\": " + EndTime);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"end_time\": " + EndTime);
                }
            }
            if(IDs != null)
            {
                propertyValue = "[" + String.Join(", ", IDs.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"ids\": " + propertyValue);
            }
            if(StartTime.HasValue)
            {
                ret.Add("\"startTime\": " + StartTime);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"start_time\": " + StartTime);
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(EndTime.HasValue)
            {
                ret += "<endTime>" + EndTime + "</endTime>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<end_time>" + EndTime + "</end_time>";
                }
            }
            if(IDs != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", IDs.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<ids>" + propertyValue + "</ids>";
            }
            if(StartTime.HasValue)
            {
                ret += "<startTime>" + StartTime + "</startTime>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<start_time>" + StartTime + "</start_time>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaImage
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ContentId != null)
            {
                ret.Add("\"contentId\": " + "\"" + EscapeJson(ContentId) + "\"");
            }
            ret.Add("\"id\": " + Id);
            ret.Add("\"imageObjectId\": " + ImageObjectId);
            if(ImageObjectType.HasValue)
            {
                ret.Add("\"imageObjectType\": " + "\"" + Enum.GetName(typeof(KalturaImageObjectType), ImageObjectType) + "\"");
            }
            ret.Add("\"imageTypeId\": " + ImageTypeId);
            if(IsDefault.HasValue)
            {
                ret.Add("\"isDefault\": " + IsDefault.ToString().ToLower());
            }
            ret.Add("\"status\": " + "\"" + Enum.GetName(typeof(KalturaImageStatus), Status) + "\"");
            if(Url != null)
            {
                ret.Add("\"url\": " + "\"" + EscapeJson(Url) + "\"");
            }
            if(Version != null)
            {
                ret.Add("\"version\": " + "\"" + EscapeJson(Version) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ContentId != null)
            {
                ret += "<contentId>" + EscapeXml(ContentId) + "</contentId>";
            }
            ret += "<id>" + Id + "</id>";
            ret += "<imageObjectId>" + ImageObjectId + "</imageObjectId>";
            if(ImageObjectType.HasValue)
            {
                ret += "<imageObjectType>" + "\"" + Enum.GetName(typeof(KalturaImageObjectType), ImageObjectType) + "\"" + "</imageObjectType>";
            }
            ret += "<imageTypeId>" + ImageTypeId + "</imageTypeId>";
            if(IsDefault.HasValue)
            {
                ret += "<isDefault>" + IsDefault.ToString().ToLower() + "</isDefault>";
            }
            ret += "<status>" + "\"" + Enum.GetName(typeof(KalturaImageStatus), Status) + "\"" + "</status>";
            if(Url != null)
            {
                ret += "<url>" + EscapeXml(Url) + "</url>";
            }
            if(Version != null)
            {
                ret += "<version>" + EscapeXml(Version) + "</version>";
            }
            return ret;
        }
    }
    public partial class KalturaImageFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(IdIn != null)
            {
                ret.Add("\"idIn\": " + "\"" + EscapeJson(IdIn) + "\"");
            }
            if(ImageObjectIdEqual.HasValue)
            {
                ret.Add("\"imageObjectIdEqual\": " + ImageObjectIdEqual);
            }
            if(ImageObjectTypeEqual.HasValue)
            {
                ret.Add("\"imageObjectTypeEqual\": " + "\"" + Enum.GetName(typeof(KalturaImageObjectType), ImageObjectTypeEqual) + "\"");
            }
            if(IsDefaultEqual.HasValue)
            {
                ret.Add("\"isDefaultEqual\": " + IsDefaultEqual.ToString().ToLower());
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(IdIn != null)
            {
                ret += "<idIn>" + EscapeXml(IdIn) + "</idIn>";
            }
            if(ImageObjectIdEqual.HasValue)
            {
                ret += "<imageObjectIdEqual>" + ImageObjectIdEqual + "</imageObjectIdEqual>";
            }
            if(ImageObjectTypeEqual.HasValue)
            {
                ret += "<imageObjectTypeEqual>" + "\"" + Enum.GetName(typeof(KalturaImageObjectType), ImageObjectTypeEqual) + "\"" + "</imageObjectTypeEqual>";
            }
            if(IsDefaultEqual.HasValue)
            {
                ret += "<isDefaultEqual>" + IsDefaultEqual.ToString().ToLower() + "</isDefaultEqual>";
            }
            return ret;
        }
    }
    public partial class KalturaImageListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Images != null)
            {
                propertyValue = "[" + String.Join(", ", Images.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Images != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Images.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaImageType
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(DefaultImageId.HasValue)
            {
                ret.Add("\"defaultImageId\": " + DefaultImageId);
            }
            if(HelpText != null)
            {
                ret.Add("\"helpText\": " + "\"" + EscapeJson(HelpText) + "\"");
            }
            ret.Add("\"id\": " + Id);
            if (currentVersion == null || isOldVersion)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(RatioId.HasValue)
            {
                ret.Add("\"ratioId\": " + RatioId);
            }
            if(SystemName != null)
            {
                ret.Add("\"systemName\": " + "\"" + EscapeJson(SystemName) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(DefaultImageId.HasValue)
            {
                ret += "<defaultImageId>" + DefaultImageId + "</defaultImageId>";
            }
            if(HelpText != null)
            {
                ret += "<helpText>" + EscapeXml(HelpText) + "</helpText>";
            }
            ret += "<id>" + Id + "</id>";
            if (currentVersion == null || isOldVersion)
            {
            ret += "<id>" + Id + "</id>";
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(RatioId.HasValue)
            {
                ret += "<ratioId>" + RatioId + "</ratioId>";
            }
            if(SystemName != null)
            {
                ret += "<systemName>" + EscapeXml(SystemName) + "</systemName>";
            }
            return ret;
        }
    }
    public partial class KalturaImageTypeFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(IdIn != null)
            {
                ret.Add("\"idIn\": " + "\"" + EscapeJson(IdIn) + "\"");
            }
            if(RatioIdIn != null)
            {
                ret.Add("\"ratioIdIn\": " + "\"" + EscapeJson(RatioIdIn) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(IdIn != null)
            {
                ret += "<idIn>" + EscapeXml(IdIn) + "</idIn>";
            }
            if(RatioIdIn != null)
            {
                ret += "<ratioIdIn>" + EscapeXml(RatioIdIn) + "</ratioIdIn>";
            }
            return ret;
        }
    }
    public partial class KalturaImageTypeListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ImageTypes != null)
            {
                propertyValue = "[" + String.Join(", ", ImageTypes.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ImageTypes != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", ImageTypes.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaLastPosition
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"position\": " + Position);
            ret.Add("\"position_owner\": " + "\"" + Enum.GetName(typeof(KalturaPositionOwner), PositionOwner) + "\"");
            if(UserId != null)
            {
                ret.Add("\"user_id\": " + "\"" + EscapeJson(UserId) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<position>" + Position + "</position>";
            ret += "<position_owner>" + "\"" + Enum.GetName(typeof(KalturaPositionOwner), PositionOwner) + "\"" + "</position_owner>";
            if(UserId != null)
            {
                ret += "<user_id>" + EscapeXml(UserId) + "</user_id>";
            }
            return ret;
        }
    }
    public partial class KalturaLastPositionFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"by\": " + "\"" + Enum.GetName(typeof(KalturaEntityReferenceBy), By) + "\"");
            if(Ids != null)
            {
                propertyValue = "[" + String.Join(", ", Ids.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"ids\": " + propertyValue);
            }
            ret.Add("\"type\": " + "\"" + Enum.GetName(typeof(KalturaLastPositionAssetType), Type) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<by>" + "\"" + Enum.GetName(typeof(KalturaEntityReferenceBy), By) + "\"" + "</by>";
            if(Ids != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Ids.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<ids>" + propertyValue + "</ids>";
            }
            ret += "<type>" + "\"" + Enum.GetName(typeof(KalturaLastPositionAssetType), Type) + "\"" + "</type>";
            return ret;
        }
    }
    public partial class KalturaLastPositionListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(LastPositions != null)
            {
                propertyValue = "[" + String.Join(", ", LastPositions.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(LastPositions != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", LastPositions.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaLinearMediaAsset
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(BufferCatchUp.HasValue)
            {
                ret.Add("\"bufferCatchUp\": " + BufferCatchUp);
            }
            if(BufferTrickPlay.HasValue)
            {
                ret.Add("\"bufferTrickPlay\": " + BufferTrickPlay);
            }
            ret.Add("\"catchUpEnabled\": " + CatchUpEnabled.ToString().ToLower());
            ret.Add("\"cdvrEnabled\": " + CdvrEnabled.ToString().ToLower());
            if(ChannelType.HasValue)
            {
                ret.Add("\"channelType\": " + "\"" + Enum.GetName(typeof(KalturaLinearChannelType), ChannelType) + "\"");
            }
            if(EnableCatchUpState.HasValue)
            {
                ret.Add("\"enableCatchUpState\": " + "\"" + Enum.GetName(typeof(KalturaTimeShiftedTvState), EnableCatchUpState) + "\"");
            }
            if(EnableCdvrState.HasValue)
            {
                ret.Add("\"enableCdvrState\": " + "\"" + Enum.GetName(typeof(KalturaTimeShiftedTvState), EnableCdvrState) + "\"");
            }
            if(EnableRecordingPlaybackNonEntitledChannelState.HasValue)
            {
                ret.Add("\"enableRecordingPlaybackNonEntitledChannelState\": " + "\"" + Enum.GetName(typeof(KalturaTimeShiftedTvState), EnableRecordingPlaybackNonEntitledChannelState) + "\"");
            }
            if(EnableStartOverState.HasValue)
            {
                ret.Add("\"enableStartOverState\": " + "\"" + Enum.GetName(typeof(KalturaTimeShiftedTvState), EnableStartOverState) + "\"");
            }
            if(EnableTrickPlayState.HasValue)
            {
                ret.Add("\"enableTrickPlayState\": " + "\"" + Enum.GetName(typeof(KalturaTimeShiftedTvState), EnableTrickPlayState) + "\"");
            }
            if(ExternalCdvrId != null)
            {
                ret.Add("\"externalCdvrId\": " + "\"" + EscapeJson(ExternalCdvrId) + "\"");
            }
            if(ExternalEpgIngestId != null)
            {
                ret.Add("\"externalEpgIngestId\": " + "\"" + EscapeJson(ExternalEpgIngestId) + "\"");
            }
            ret.Add("\"recordingPlaybackNonEntitledChannelEnabled\": " + RecordingPlaybackNonEntitledChannelEnabled.ToString().ToLower());
            ret.Add("\"startOverEnabled\": " + StartOverEnabled.ToString().ToLower());
            ret.Add("\"summedCatchUpBuffer\": " + SummedCatchUpBuffer);
            ret.Add("\"summedTrickPlayBuffer\": " + SummedTrickPlayBuffer);
            ret.Add("\"trickPlayEnabled\": " + TrickPlayEnabled.ToString().ToLower());
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(BufferCatchUp.HasValue)
            {
                ret += "<bufferCatchUp>" + BufferCatchUp + "</bufferCatchUp>";
            }
            if(BufferTrickPlay.HasValue)
            {
                ret += "<bufferTrickPlay>" + BufferTrickPlay + "</bufferTrickPlay>";
            }
            ret += "<catchUpEnabled>" + CatchUpEnabled.ToString().ToLower() + "</catchUpEnabled>";
            ret += "<cdvrEnabled>" + CdvrEnabled.ToString().ToLower() + "</cdvrEnabled>";
            if(ChannelType.HasValue)
            {
                ret += "<channelType>" + "\"" + Enum.GetName(typeof(KalturaLinearChannelType), ChannelType) + "\"" + "</channelType>";
            }
            if(EnableCatchUpState.HasValue)
            {
                ret += "<enableCatchUpState>" + "\"" + Enum.GetName(typeof(KalturaTimeShiftedTvState), EnableCatchUpState) + "\"" + "</enableCatchUpState>";
            }
            if(EnableCdvrState.HasValue)
            {
                ret += "<enableCdvrState>" + "\"" + Enum.GetName(typeof(KalturaTimeShiftedTvState), EnableCdvrState) + "\"" + "</enableCdvrState>";
            }
            if(EnableRecordingPlaybackNonEntitledChannelState.HasValue)
            {
                ret += "<enableRecordingPlaybackNonEntitledChannelState>" + "\"" + Enum.GetName(typeof(KalturaTimeShiftedTvState), EnableRecordingPlaybackNonEntitledChannelState) + "\"" + "</enableRecordingPlaybackNonEntitledChannelState>";
            }
            if(EnableStartOverState.HasValue)
            {
                ret += "<enableStartOverState>" + "\"" + Enum.GetName(typeof(KalturaTimeShiftedTvState), EnableStartOverState) + "\"" + "</enableStartOverState>";
            }
            if(EnableTrickPlayState.HasValue)
            {
                ret += "<enableTrickPlayState>" + "\"" + Enum.GetName(typeof(KalturaTimeShiftedTvState), EnableTrickPlayState) + "\"" + "</enableTrickPlayState>";
            }
            if(ExternalCdvrId != null)
            {
                ret += "<externalCdvrId>" + EscapeXml(ExternalCdvrId) + "</externalCdvrId>";
            }
            if(ExternalEpgIngestId != null)
            {
                ret += "<externalEpgIngestId>" + EscapeXml(ExternalEpgIngestId) + "</externalEpgIngestId>";
            }
            ret += "<recordingPlaybackNonEntitledChannelEnabled>" + RecordingPlaybackNonEntitledChannelEnabled.ToString().ToLower() + "</recordingPlaybackNonEntitledChannelEnabled>";
            ret += "<startOverEnabled>" + StartOverEnabled.ToString().ToLower() + "</startOverEnabled>";
            ret += "<summedCatchUpBuffer>" + SummedCatchUpBuffer + "</summedCatchUpBuffer>";
            ret += "<summedTrickPlayBuffer>" + SummedTrickPlayBuffer + "</summedTrickPlayBuffer>";
            ret += "<trickPlayEnabled>" + TrickPlayEnabled.ToString().ToLower() + "</trickPlayEnabled>";
            return ret;
        }
    }
    public partial class KalturaManualChannel
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(MediaIds != null)
            {
                ret.Add("\"mediaIds\": " + "\"" + EscapeJson(MediaIds) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(MediaIds != null)
            {
                ret += "<mediaIds>" + EscapeXml(MediaIds) + "</mediaIds>";
            }
            return ret;
        }
    }
    public partial class KalturaMediaAsset
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && CatchUpBuffer.HasValue)
            {
                ret.Add("\"catchUpBuffer\": " + CatchUpBuffer);
            }
            if(DeviceRule != null)
            {
                ret.Add("\"deviceRule\": " + "\"" + EscapeJson(DeviceRule) + "\"");
            }
            if(DeviceRuleId.HasValue)
            {
                ret.Add("\"deviceRuleId\": " + DeviceRuleId);
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && EnableRecordingPlaybackNonEntitledChannel.HasValue)
            {
                ret.Add("\"enableRecordingPlaybackNonEntitledChannel\": " + EnableRecordingPlaybackNonEntitledChannel.ToString().ToLower());
            }
            if(EntryId != null)
            {
                ret.Add("\"entryId\": " + "\"" + EscapeJson(EntryId) + "\"");
            }
            if(ExternalIds != null)
            {
                ret.Add("\"externalIds\": " + "\"" + EscapeJson(ExternalIds) + "\"");
            }
            if(GeoBlockRule != null)
            {
                ret.Add("\"geoBlockRule\": " + "\"" + EscapeJson(GeoBlockRule) + "\"");
            }
            if(GeoBlockRuleId.HasValue)
            {
                ret.Add("\"geoBlockRuleId\": " + GeoBlockRuleId);
            }
            if(Status.HasValue)
            {
                ret.Add("\"status\": " + Status.ToString().ToLower());
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && TrickPlayBuffer.HasValue)
            {
                ret.Add("\"trickPlayBuffer\": " + TrickPlayBuffer);
            }
            if(TypeDescription != null)
            {
                ret.Add("\"typeDescription\": " + "\"" + EscapeJson(TypeDescription) + "\"");
            }
            if(WatchPermissionRule != null)
            {
                ret.Add("\"watchPermissionRule\": " + "\"" + EscapeJson(WatchPermissionRule) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && CatchUpBuffer.HasValue)
            {
                ret += "<catchUpBuffer>" + CatchUpBuffer + "</catchUpBuffer>";
            }
            if(DeviceRule != null)
            {
                ret += "<deviceRule>" + EscapeXml(DeviceRule) + "</deviceRule>";
            }
            if(DeviceRuleId.HasValue)
            {
                ret += "<deviceRuleId>" + DeviceRuleId + "</deviceRuleId>";
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && EnableRecordingPlaybackNonEntitledChannel.HasValue)
            {
                ret += "<enableRecordingPlaybackNonEntitledChannel>" + EnableRecordingPlaybackNonEntitledChannel.ToString().ToLower() + "</enableRecordingPlaybackNonEntitledChannel>";
            }
            if(EntryId != null)
            {
                ret += "<entryId>" + EscapeXml(EntryId) + "</entryId>";
            }
            if(ExternalIds != null)
            {
                ret += "<externalIds>" + EscapeXml(ExternalIds) + "</externalIds>";
            }
            if(GeoBlockRule != null)
            {
                ret += "<geoBlockRule>" + EscapeXml(GeoBlockRule) + "</geoBlockRule>";
            }
            if(GeoBlockRuleId.HasValue)
            {
                ret += "<geoBlockRuleId>" + GeoBlockRuleId + "</geoBlockRuleId>";
            }
            if(Status.HasValue)
            {
                ret += "<status>" + Status.ToString().ToLower() + "</status>";
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && TrickPlayBuffer.HasValue)
            {
                ret += "<trickPlayBuffer>" + TrickPlayBuffer + "</trickPlayBuffer>";
            }
            if(TypeDescription != null)
            {
                ret += "<typeDescription>" + EscapeXml(TypeDescription) + "</typeDescription>";
            }
            if(WatchPermissionRule != null)
            {
                ret += "<watchPermissionRule>" + EscapeXml(WatchPermissionRule) + "</watchPermissionRule>";
            }
            return ret;
        }
    }
    public partial class KalturaMediaFile
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AdditionalData != null)
            {
                ret.Add("\"additionalData\": " + "\"" + EscapeJson(AdditionalData) + "\"");
            }
            if(AltCdnCode != null)
            {
                ret.Add("\"altCdnCode\": " + "\"" + EscapeJson(AltCdnCode) + "\"");
            }
            if(AlternativeCdnAdapaterProfileId.HasValue)
            {
                ret.Add("\"alternativeCdnAdapaterProfileId\": " + AlternativeCdnAdapaterProfileId);
            }
            if(AltExternalId != null)
            {
                ret.Add("\"altExternalId\": " + "\"" + EscapeJson(AltExternalId) + "\"");
            }
            if(AltStreamingCode != null)
            {
                ret.Add("\"altStreamingCode\": " + "\"" + EscapeJson(AltStreamingCode) + "\"");
            }
            if(AssetId.HasValue)
            {
                ret.Add("\"assetId\": " + AssetId);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"asset_id\": " + AssetId);
                }
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && BillingType != null)
            {
                ret.Add("\"billingType\": " + "\"" + EscapeJson(BillingType) + "\"");
            }
            if(CdnAdapaterProfileId.HasValue)
            {
                ret.Add("\"cdnAdapaterProfileId\": " + CdnAdapaterProfileId);
            }
            if(CdnCode != null)
            {
                ret.Add("\"cdnCode\": " + "\"" + EscapeJson(CdnCode) + "\"");
            }
            if(CdnName != null)
            {
                ret.Add("\"cdnName\": " + "\"" + EscapeJson(CdnName) + "\"");
            }
            if(Duration.HasValue)
            {
                ret.Add("\"duration\": " + Duration);
            }
            if(EndDate.HasValue)
            {
                ret.Add("\"endDate\": " + EndDate);
            }
            if(ExternalId != null)
            {
                ret.Add("\"externalId\": " + "\"" + EscapeJson(ExternalId) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"external_id\": " + "\"" + EscapeJson(ExternalId) + "\"");
                }
            }
            if(ExternalStoreId != null)
            {
                ret.Add("\"externalStoreId\": " + "\"" + EscapeJson(ExternalStoreId) + "\"");
            }
            if(FileSize.HasValue)
            {
                ret.Add("\"fileSize\": " + FileSize);
            }
            if(HandlingType != null)
            {
                ret.Add("\"handlingType\": " + "\"" + EscapeJson(HandlingType) + "\"");
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(IsDefaultLanguage.HasValue)
            {
                ret.Add("\"isDefaultLanguage\": " + IsDefaultLanguage.ToString().ToLower());
            }
            if(Language != null)
            {
                ret.Add("\"language\": " + "\"" + EscapeJson(Language) + "\"");
            }
            if(OrderNum.HasValue)
            {
                ret.Add("\"orderNum\": " + OrderNum);
            }
            if(OutputProtecationLevel != null)
            {
                ret.Add("\"outputProtecationLevel\": " + "\"" + EscapeJson(OutputProtecationLevel) + "\"");
            }
            if(PPVModules != null)
            {
                propertyValue = PPVModules.ToJson(currentVersion, omitObsolete);
                ret.Add("\"ppvModules\": " + propertyValue);
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && ProductCode != null)
            {
                ret.Add("\"productCode\": " + "\"" + EscapeJson(ProductCode) + "\"");
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && Quality != null)
            {
                ret.Add("\"quality\": " + "\"" + EscapeJson(Quality) + "\"");
            }
            if(StartDate.HasValue)
            {
                ret.Add("\"startDate\": " + StartDate);
            }
            if(Status.HasValue)
            {
                ret.Add("\"status\": " + Status.ToString().ToLower());
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && Type != null)
            {
                ret.Add("\"type\": " + "\"" + EscapeJson(Type) + "\"");
            }
            if(TypeId.HasValue)
            {
                ret.Add("\"typeId\": " + TypeId);
            }
            if(Url != null)
            {
                ret.Add("\"url\": " + "\"" + EscapeJson(Url) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AdditionalData != null)
            {
                ret += "<additionalData>" + EscapeXml(AdditionalData) + "</additionalData>";
            }
            if(AltCdnCode != null)
            {
                ret += "<altCdnCode>" + EscapeXml(AltCdnCode) + "</altCdnCode>";
            }
            if(AlternativeCdnAdapaterProfileId.HasValue)
            {
                ret += "<alternativeCdnAdapaterProfileId>" + AlternativeCdnAdapaterProfileId + "</alternativeCdnAdapaterProfileId>";
            }
            if(AltExternalId != null)
            {
                ret += "<altExternalId>" + EscapeXml(AltExternalId) + "</altExternalId>";
            }
            if(AltStreamingCode != null)
            {
                ret += "<altStreamingCode>" + EscapeXml(AltStreamingCode) + "</altStreamingCode>";
            }
            if(AssetId.HasValue)
            {
                ret += "<assetId>" + AssetId + "</assetId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<asset_id>" + AssetId + "</asset_id>";
                }
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && BillingType != null)
            {
                ret += "<billingType>" + EscapeXml(BillingType) + "</billingType>";
            }
            if(CdnAdapaterProfileId.HasValue)
            {
                ret += "<cdnAdapaterProfileId>" + CdnAdapaterProfileId + "</cdnAdapaterProfileId>";
            }
            if(CdnCode != null)
            {
                ret += "<cdnCode>" + EscapeXml(CdnCode) + "</cdnCode>";
            }
            if(CdnName != null)
            {
                ret += "<cdnName>" + EscapeXml(CdnName) + "</cdnName>";
            }
            if(Duration.HasValue)
            {
                ret += "<duration>" + Duration + "</duration>";
            }
            if(EndDate.HasValue)
            {
                ret += "<endDate>" + EndDate + "</endDate>";
            }
            if(ExternalId != null)
            {
                ret += "<externalId>" + EscapeXml(ExternalId) + "</externalId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<external_id>" + EscapeXml(ExternalId) + "</external_id>";
                }
            }
            if(ExternalStoreId != null)
            {
                ret += "<externalStoreId>" + EscapeXml(ExternalStoreId) + "</externalStoreId>";
            }
            if(FileSize.HasValue)
            {
                ret += "<fileSize>" + FileSize + "</fileSize>";
            }
            if(HandlingType != null)
            {
                ret += "<handlingType>" + EscapeXml(HandlingType) + "</handlingType>";
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(IsDefaultLanguage.HasValue)
            {
                ret += "<isDefaultLanguage>" + IsDefaultLanguage.ToString().ToLower() + "</isDefaultLanguage>";
            }
            if(Language != null)
            {
                ret += "<language>" + EscapeXml(Language) + "</language>";
            }
            if(OrderNum.HasValue)
            {
                ret += "<orderNum>" + OrderNum + "</orderNum>";
            }
            if(OutputProtecationLevel != null)
            {
                ret += "<outputProtecationLevel>" + EscapeXml(OutputProtecationLevel) + "</outputProtecationLevel>";
            }
            if(PPVModules != null)
            {
                propertyValue = PPVModules.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<ppvModules>" + propertyValue + "</ppvModules>";
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && ProductCode != null)
            {
                ret += "<productCode>" + EscapeXml(ProductCode) + "</productCode>";
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && Quality != null)
            {
                ret += "<quality>" + EscapeXml(Quality) + "</quality>";
            }
            if(StartDate.HasValue)
            {
                ret += "<startDate>" + StartDate + "</startDate>";
            }
            if(Status.HasValue)
            {
                ret += "<status>" + Status.ToString().ToLower() + "</status>";
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && Type != null)
            {
                ret += "<type>" + EscapeXml(Type) + "</type>";
            }
            if(TypeId.HasValue)
            {
                ret += "<typeId>" + TypeId + "</typeId>";
            }
            if(Url != null)
            {
                ret += "<url>" + EscapeXml(Url) + "</url>";
            }
            return ret;
        }
    }
    public partial class KalturaMediaFileFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"assetIdEqual\": " + AssetIdEqual);
            ret.Add("\"idEqual\": " + IdEqual);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<assetIdEqual>" + AssetIdEqual + "</assetIdEqual>";
            ret += "<idEqual>" + IdEqual + "</idEqual>";
            return ret;
        }
    }
    public partial class KalturaMediaFileListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Files != null)
            {
                propertyValue = "[" + String.Join(", ", Files.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Files != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Files.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaMediaFileType
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AudioCodecs != null)
            {
                ret.Add("\"audioCodecs\": " + "\"" + EscapeJson(AudioCodecs) + "\"");
            }
            if(CreateDate.HasValue)
            {
                ret.Add("\"createDate\": " + CreateDate);
            }
            if(Description != null)
            {
                ret.Add("\"description\": " + "\"" + EscapeJson(Description) + "\"");
            }
            ret.Add("\"drmProfileId\": " + DrmProfileId);
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(IsTrailer.HasValue)
            {
                ret.Add("\"isTrailer\": " + IsTrailer.ToString().ToLower());
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(Quality.HasValue)
            {
                ret.Add("\"quality\": " + "\"" + Enum.GetName(typeof(KalturaMediaFileTypeQuality), Quality) + "\"");
            }
            if(Status.HasValue)
            {
                ret.Add("\"status\": " + Status.ToString().ToLower());
            }
            if(StreamerType.HasValue)
            {
                ret.Add("\"streamerType\": " + "\"" + Enum.GetName(typeof(KalturaMediaFileStreamerType), StreamerType) + "\"");
            }
            if(UpdateDate.HasValue)
            {
                ret.Add("\"updateDate\": " + UpdateDate);
            }
            if(VideoCodecs != null)
            {
                ret.Add("\"videoCodecs\": " + "\"" + EscapeJson(VideoCodecs) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AudioCodecs != null)
            {
                ret += "<audioCodecs>" + EscapeXml(AudioCodecs) + "</audioCodecs>";
            }
            if(CreateDate.HasValue)
            {
                ret += "<createDate>" + CreateDate + "</createDate>";
            }
            if(Description != null)
            {
                ret += "<description>" + EscapeXml(Description) + "</description>";
            }
            ret += "<drmProfileId>" + DrmProfileId + "</drmProfileId>";
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(IsTrailer.HasValue)
            {
                ret += "<isTrailer>" + IsTrailer.ToString().ToLower() + "</isTrailer>";
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(Quality.HasValue)
            {
                ret += "<quality>" + "\"" + Enum.GetName(typeof(KalturaMediaFileTypeQuality), Quality) + "\"" + "</quality>";
            }
            if(Status.HasValue)
            {
                ret += "<status>" + Status.ToString().ToLower() + "</status>";
            }
            if(StreamerType.HasValue)
            {
                ret += "<streamerType>" + "\"" + Enum.GetName(typeof(KalturaMediaFileStreamerType), StreamerType) + "\"" + "</streamerType>";
            }
            if(UpdateDate.HasValue)
            {
                ret += "<updateDate>" + UpdateDate + "</updateDate>";
            }
            if(VideoCodecs != null)
            {
                ret += "<videoCodecs>" + EscapeXml(VideoCodecs) + "</videoCodecs>";
            }
            return ret;
        }
    }
    public partial class KalturaMediaFileTypeListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Types != null)
            {
                propertyValue = "[" + String.Join(", ", Types.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Types != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Types.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaMediaImage
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Height.HasValue)
            {
                ret.Add("\"height\": " + Height);
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + EscapeJson(Id) + "\"");
            }
            if(IsDefault.HasValue)
            {
                ret.Add("\"isDefault\": " + IsDefault.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"is_default\": " + IsDefault.ToString().ToLower());
                }
            }
            if(Ratio != null)
            {
                ret.Add("\"ratio\": " + "\"" + EscapeJson(Ratio) + "\"");
            }
            if(Url != null)
            {
                ret.Add("\"url\": " + "\"" + EscapeJson(Url) + "\"");
            }
            if(Version.HasValue)
            {
                ret.Add("\"version\": " + Version);
            }
            if(Width.HasValue)
            {
                ret.Add("\"width\": " + Width);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Height.HasValue)
            {
                ret += "<height>" + Height + "</height>";
            }
            if(Id != null)
            {
                ret += "<id>" + EscapeXml(Id) + "</id>";
            }
            if(IsDefault.HasValue)
            {
                ret += "<isDefault>" + IsDefault.ToString().ToLower() + "</isDefault>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<is_default>" + IsDefault.ToString().ToLower() + "</is_default>";
                }
            }
            if(Ratio != null)
            {
                ret += "<ratio>" + EscapeXml(Ratio) + "</ratio>";
            }
            if(Url != null)
            {
                ret += "<url>" + EscapeXml(Url) + "</url>";
            }
            if(Version.HasValue)
            {
                ret += "<version>" + Version + "</version>";
            }
            if(Width.HasValue)
            {
                ret += "<width>" + Width + "</width>";
            }
            return ret;
        }
    }
    public partial class KalturaOTTCategory
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Channels != null)
            {
                propertyValue = "[" + String.Join(", ", Channels.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"channels\": " + propertyValue);
            }
            if(ChildCategories != null)
            {
                propertyValue = "[" + String.Join(", ", ChildCategories.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"childCategories\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"child_categories\": " + propertyValue);
                }
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Images != null)
            {
                propertyValue = "[" + String.Join(", ", Images.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"images\": " + propertyValue);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(ParentCategoryId.HasValue)
            {
                ret.Add("\"parentCategoryId\": " + ParentCategoryId);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"parent_category_id\": " + ParentCategoryId);
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Channels != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Channels.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<channels>" + propertyValue + "</channels>";
            }
            if(ChildCategories != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", ChildCategories.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<childCategories>" + propertyValue + "</childCategories>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<child_categories>" + propertyValue + "</child_categories>";
                }
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(Images != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Images.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<images>" + propertyValue + "</images>";
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(ParentCategoryId.HasValue)
            {
                ret += "<parentCategoryId>" + ParentCategoryId + "</parentCategoryId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<parent_category_id>" + ParentCategoryId + "</parent_category_id>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaPersonalAsset
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Bookmarks != null)
            {
                propertyValue = "[" + String.Join(", ", Bookmarks.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"bookmarks\": " + propertyValue);
            }
            if(Files != null)
            {
                propertyValue = "[" + String.Join(", ", Files.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"files\": " + propertyValue);
            }
            ret.Add("\"following\": " + Following.ToString().ToLower());
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            ret.Add("\"type\": " + "\"" + Enum.GetName(typeof(KalturaAssetType), Type) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Bookmarks != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Bookmarks.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<bookmarks>" + propertyValue + "</bookmarks>";
            }
            if(Files != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Files.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<files>" + propertyValue + "</files>";
            }
            ret += "<following>" + Following.ToString().ToLower() + "</following>";
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            ret += "<type>" + "\"" + Enum.GetName(typeof(KalturaAssetType), Type) + "\"" + "</type>";
            return ret;
        }
    }
    public partial class KalturaPersonalAssetListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaPersonalAssetRequest
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(FileIds != null)
            {
                propertyValue = "[" + String.Join(", ", FileIds.Select(item => item.ToString())) + "]";
                ret.Add("\"fileIds\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"file_ids\": " + propertyValue);
                }
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            ret.Add("\"type\": " + "\"" + Enum.GetName(typeof(KalturaAssetType), Type) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(FileIds != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", FileIds.Select(item => item.ToString())) + "</item>";
                ret += "<fileIds>" + propertyValue + "</fileIds>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<file_ids>" + propertyValue + "</file_ids>";
                }
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            ret += "<type>" + "\"" + Enum.GetName(typeof(KalturaAssetType), Type) + "\"" + "</type>";
            return ret;
        }
    }
    public partial class KalturaPersonalAssetWithHolder
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"type\": " + "\"" + Enum.GetName(typeof(KalturaPersonalAssetWith), type) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<type>" + "\"" + Enum.GetName(typeof(KalturaPersonalAssetWith), type) + "\"" + "</type>";
            return ret;
        }
    }
    public partial class KalturaPersonalFile
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Discounted.HasValue)
            {
                ret.Add("\"discounted\": " + Discounted.ToString().ToLower());
            }
            if(Entitled.HasValue)
            {
                ret.Add("\"entitled\": " + Entitled.ToString().ToLower());
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Offer != null)
            {
                ret.Add("\"offer\": " + "\"" + EscapeJson(Offer) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Discounted.HasValue)
            {
                ret += "<discounted>" + Discounted.ToString().ToLower() + "</discounted>";
            }
            if(Entitled.HasValue)
            {
                ret += "<entitled>" + Entitled.ToString().ToLower() + "</entitled>";
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(Offer != null)
            {
                ret += "<offer>" + EscapeXml(Offer) + "</offer>";
            }
            return ret;
        }
    }
    public partial class KalturaPersonalListSearchFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(PartnerListTypeIn != null)
            {
                ret.Add("\"partnerListTypeIn\": " + "\"" + EscapeJson(PartnerListTypeIn) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(PartnerListTypeIn != null)
            {
                ret += "<partnerListTypeIn>" + EscapeXml(PartnerListTypeIn) + "</partnerListTypeIn>";
            }
            return ret;
        }
    }
    public partial class KalturaPlayerAssetData
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(action != null)
            {
                ret.Add("\"action\": " + "\"" + EscapeJson(action) + "\"");
            }
            if(averageBitRate.HasValue)
            {
                ret.Add("\"averageBitrate\": " + averageBitRate);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"average_bitrate\": " + averageBitRate);
                }
            }
            if(currentBitRate.HasValue)
            {
                ret.Add("\"currentBitrate\": " + currentBitRate);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"current_bitrate\": " + currentBitRate);
                }
            }
            if(location.HasValue)
            {
                ret.Add("\"location\": " + location);
            }
            if(totalBitRate.HasValue)
            {
                ret.Add("\"totalBitrate\": " + totalBitRate);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"total_bitrate\": " + totalBitRate);
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(action != null)
            {
                ret += "<action>" + EscapeXml(action) + "</action>";
            }
            if(averageBitRate.HasValue)
            {
                ret += "<averageBitrate>" + averageBitRate + "</averageBitrate>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<average_bitrate>" + averageBitRate + "</average_bitrate>";
                }
            }
            if(currentBitRate.HasValue)
            {
                ret += "<currentBitrate>" + currentBitRate + "</currentBitrate>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<current_bitrate>" + currentBitRate + "</current_bitrate>";
                }
            }
            if(location.HasValue)
            {
                ret += "<location>" + location + "</location>";
            }
            if(totalBitRate.HasValue)
            {
                ret += "<totalBitrate>" + totalBitRate + "</totalBitrate>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<total_bitrate>" + totalBitRate + "</total_bitrate>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaProgramAsset
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Crid != null)
            {
                ret.Add("\"crid\": " + "\"" + EscapeJson(Crid) + "\"");
            }
            if(EpgChannelId.HasValue)
            {
                ret.Add("\"epgChannelId\": " + EpgChannelId);
            }
            if(EpgId != null)
            {
                ret.Add("\"epgId\": " + "\"" + EscapeJson(EpgId) + "\"");
            }
            if(LinearAssetId.HasValue)
            {
                ret.Add("\"linearAssetId\": " + LinearAssetId);
            }
            if(RelatedMediaId.HasValue)
            {
                ret.Add("\"relatedMediaId\": " + RelatedMediaId);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Crid != null)
            {
                ret += "<crid>" + EscapeXml(Crid) + "</crid>";
            }
            if(EpgChannelId.HasValue)
            {
                ret += "<epgChannelId>" + EpgChannelId + "</epgChannelId>";
            }
            if(EpgId != null)
            {
                ret += "<epgId>" + EscapeXml(EpgId) + "</epgId>";
            }
            if(LinearAssetId.HasValue)
            {
                ret += "<linearAssetId>" + LinearAssetId + "</linearAssetId>";
            }
            if(RelatedMediaId.HasValue)
            {
                ret += "<relatedMediaId>" + RelatedMediaId + "</relatedMediaId>";
            }
            return ret;
        }
    }
    public partial class KalturaRatio
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"height\": " + Height);
            ret.Add("\"id\": " + Id);
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            ret.Add("\"precisionPrecentage\": " + PrecisionPrecentage);
            ret.Add("\"width\": " + Width);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<height>" + Height + "</height>";
            ret += "<id>" + Id + "</id>";
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            ret += "<precisionPrecentage>" + PrecisionPrecentage + "</precisionPrecentage>";
            ret += "<width>" + Width + "</width>";
            return ret;
        }
    }
    public partial class KalturaRatioListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Ratios != null)
            {
                propertyValue = "[" + String.Join(", ", Ratios.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Ratios != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Ratios.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaRecordingAsset
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(RecordingId != null)
            {
                ret.Add("\"recordingId\": " + "\"" + EscapeJson(RecordingId) + "\"");
            }
            if(RecordingType.HasValue)
            {
                ret.Add("\"recordingType\": " + "\"" + Enum.GetName(typeof(KalturaRecordingType), RecordingType) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(RecordingId != null)
            {
                ret += "<recordingId>" + EscapeXml(RecordingId) + "</recordingId>";
            }
            if(RecordingType.HasValue)
            {
                ret += "<recordingType>" + "\"" + Enum.GetName(typeof(KalturaRecordingType), RecordingType) + "\"" + "</recordingType>";
            }
            return ret;
        }
    }
    public partial class KalturaRelatedExternalFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(FreeText != null)
            {
                ret.Add("\"freeText\": " + "\"" + EscapeJson(FreeText) + "\"");
            }
            ret.Add("\"idEqual\": " + IdEqual);
            if(TypeIn != null)
            {
                ret.Add("\"typeIn\": " + "\"" + EscapeJson(TypeIn) + "\"");
            }
            ret.Add("\"utcOffsetEqual\": " + UtcOffsetEqual);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(FreeText != null)
            {
                ret += "<freeText>" + EscapeXml(FreeText) + "</freeText>";
            }
            ret += "<idEqual>" + IdEqual + "</idEqual>";
            if(TypeIn != null)
            {
                ret += "<typeIn>" + EscapeXml(TypeIn) + "</typeIn>";
            }
            ret += "<utcOffsetEqual>" + UtcOffsetEqual + "</utcOffsetEqual>";
            return ret;
        }
    }
    public partial class KalturaRelatedFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"excludeWatched\": " + ExcludeWatched.ToString().ToLower());
            if(IdEqual.HasValue)
            {
                ret.Add("\"idEqual\": " + IdEqual);
            }
            if(KSql != null)
            {
                ret.Add("\"kSql\": " + "\"" + EscapeJson(KSql) + "\"");
            }
            if(TypeIn != null)
            {
                ret.Add("\"typeIn\": " + "\"" + EscapeJson(TypeIn) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<excludeWatched>" + ExcludeWatched.ToString().ToLower() + "</excludeWatched>";
            if(IdEqual.HasValue)
            {
                ret += "<idEqual>" + IdEqual + "</idEqual>";
            }
            if(KSql != null)
            {
                ret += "<kSql>" + EscapeXml(KSql) + "</kSql>";
            }
            if(TypeIn != null)
            {
                ret += "<typeIn>" + EscapeXml(TypeIn) + "</typeIn>";
            }
            return ret;
        }
    }
    public partial class KalturaScheduledRecordingProgramFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ChannelsIn != null)
            {
                ret.Add("\"channelsIn\": " + "\"" + EscapeJson(ChannelsIn) + "\"");
            }
            if(EndDateLessThanOrNull.HasValue)
            {
                ret.Add("\"endDateLessThanOrNull\": " + EndDateLessThanOrNull);
            }
            ret.Add("\"recordingTypeEqual\": " + "\"" + Enum.GetName(typeof(KalturaScheduledRecordingAssetType), RecordingTypeEqual) + "\"");
            if(StartDateGreaterThanOrNull.HasValue)
            {
                ret.Add("\"startDateGreaterThanOrNull\": " + StartDateGreaterThanOrNull);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ChannelsIn != null)
            {
                ret += "<channelsIn>" + EscapeXml(ChannelsIn) + "</channelsIn>";
            }
            if(EndDateLessThanOrNull.HasValue)
            {
                ret += "<endDateLessThanOrNull>" + EndDateLessThanOrNull + "</endDateLessThanOrNull>";
            }
            ret += "<recordingTypeEqual>" + "\"" + Enum.GetName(typeof(KalturaScheduledRecordingAssetType), RecordingTypeEqual) + "\"" + "</recordingTypeEqual>";
            if(StartDateGreaterThanOrNull.HasValue)
            {
                ret += "<startDateGreaterThanOrNull>" + StartDateGreaterThanOrNull + "</startDateGreaterThanOrNull>";
            }
            return ret;
        }
    }
    public partial class KalturaSearchAssetFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(IdIn != null)
            {
                ret.Add("\"idIn\": " + "\"" + EscapeJson(IdIn) + "\"");
            }
            if(KSql != null)
            {
                ret.Add("\"kSql\": " + "\"" + EscapeJson(KSql) + "\"");
            }
            if(TypeIn != null)
            {
                ret.Add("\"typeIn\": " + "\"" + EscapeJson(TypeIn) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(IdIn != null)
            {
                ret += "<idIn>" + EscapeXml(IdIn) + "</idIn>";
            }
            if(KSql != null)
            {
                ret += "<kSql>" + EscapeXml(KSql) + "</kSql>";
            }
            if(TypeIn != null)
            {
                ret += "<typeIn>" + EscapeXml(TypeIn) + "</typeIn>";
            }
            return ret;
        }
    }
    public partial class KalturaSearchAssetListFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"excludeWatched\": " + ExcludeWatched.ToString().ToLower());
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<excludeWatched>" + ExcludeWatched.ToString().ToLower() + "</excludeWatched>";
            return ret;
        }
    }
    public partial class KalturaSearchExternalFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Query != null)
            {
                ret.Add("\"query\": " + "\"" + EscapeJson(Query) + "\"");
            }
            if(TypeIn != null)
            {
                ret.Add("\"typeIn\": " + "\"" + EscapeJson(TypeIn) + "\"");
            }
            ret.Add("\"utcOffsetEqual\": " + UtcOffsetEqual);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Query != null)
            {
                ret += "<query>" + EscapeXml(Query) + "</query>";
            }
            if(TypeIn != null)
            {
                ret += "<typeIn>" + EscapeXml(TypeIn) + "</typeIn>";
            }
            ret += "<utcOffsetEqual>" + UtcOffsetEqual + "</utcOffsetEqual>";
            return ret;
        }
    }
    public partial class KalturaSlimAsset
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + EscapeJson(Id) + "\"");
            }
            ret.Add("\"type\": " + "\"" + Enum.GetName(typeof(KalturaAssetType), Type) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Id != null)
            {
                ret += "<id>" + EscapeXml(Id) + "</id>";
            }
            ret += "<type>" + "\"" + Enum.GetName(typeof(KalturaAssetType), Type) + "\"" + "</type>";
            return ret;
        }
    }
    public partial class KalturaSlimAssetInfoWrapper
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaTag
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"id\": " + Id);
            ret.Add(Tag.ToCustomJson(currentVersion, omitObsolete, "tag"));
            ret.Add("\"type\": " + TagTypeId);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<id>" + Id + "</id>";
            ret += Tag.ToCustomXml(currentVersion, omitObsolete, "tag");
            ret += "<type>" + TagTypeId + "</type>";
            return ret;
        }
    }
    public partial class KalturaTagFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(LanguageEqual != null)
            {
                ret.Add("\"languageEqual\": " + "\"" + EscapeJson(LanguageEqual) + "\"");
            }
            if(TagEqual != null)
            {
                ret.Add("\"tagEqual\": " + "\"" + EscapeJson(TagEqual) + "\"");
            }
            if(TagStartsWith != null)
            {
                ret.Add("\"tagStartsWith\": " + "\"" + EscapeJson(TagStartsWith) + "\"");
            }
            ret.Add("\"typeEqual\": " + TypeEqual);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(LanguageEqual != null)
            {
                ret += "<languageEqual>" + EscapeXml(LanguageEqual) + "</languageEqual>";
            }
            if(TagEqual != null)
            {
                ret += "<tagEqual>" + EscapeXml(TagEqual) + "</tagEqual>";
            }
            if(TagStartsWith != null)
            {
                ret += "<tagStartsWith>" + EscapeXml(TagStartsWith) + "</tagStartsWith>";
            }
            ret += "<typeEqual>" + TypeEqual + "</typeEqual>";
            return ret;
        }
    }
    public partial class KalturaTagListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Tags != null)
            {
                propertyValue = "[" + String.Join(", ", Tags.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Tags != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Tags.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaUploadedFileTokenResource
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Token != null)
            {
                ret.Add("\"token\": " + "\"" + EscapeJson(Token) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Token != null)
            {
                ret += "<token>" + EscapeXml(Token) + "</token>";
            }
            return ret;
        }
    }
    public partial class KalturaUrlResource
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Url != null)
            {
                ret.Add("\"url\": " + "\"" + EscapeJson(Url) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Url != null)
            {
                ret += "<url>" + EscapeXml(Url) + "</url>";
            }
            return ret;
        }
    }
    public partial class KalturaWatchHistoryAsset
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Asset != null)
            {
                propertyValue = Asset.ToJson(currentVersion, omitObsolete);
                ret.Add("\"asset\": " + propertyValue);
            }
            if(Duration.HasValue)
            {
                ret.Add("\"duration\": " + Duration);
            }
            if(IsFinishedWatching.HasValue)
            {
                ret.Add("\"finishedWatching\": " + IsFinishedWatching.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"finished_watching\": " + IsFinishedWatching.ToString().ToLower());
                }
            }
            if(LastWatched.HasValue)
            {
                ret.Add("\"watchedDate\": " + LastWatched);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"watched_date\": " + LastWatched);
                }
            }
            if(Position.HasValue)
            {
                ret.Add("\"position\": " + Position);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Asset != null)
            {
                propertyValue = Asset.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<asset>" + propertyValue + "</asset>";
            }
            if(Duration.HasValue)
            {
                ret += "<duration>" + Duration + "</duration>";
            }
            if(IsFinishedWatching.HasValue)
            {
                ret += "<finishedWatching>" + IsFinishedWatching.ToString().ToLower() + "</finishedWatching>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<finished_watching>" + IsFinishedWatching.ToString().ToLower() + "</finished_watching>";
                }
            }
            if(LastWatched.HasValue)
            {
                ret += "<watchedDate>" + LastWatched + "</watchedDate>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<watched_date>" + LastWatched + "</watched_date>";
                }
            }
            if(Position.HasValue)
            {
                ret += "<position>" + Position + "</position>";
            }
            return ret;
        }
    }
    public partial class KalturaWatchHistoryAssetWrapper
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
}

namespace WebAPI.Models.API
{
    public partial class KalturaAssetCondition
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Ksql != null)
            {
                ret.Add("\"ksql\": " + "\"" + EscapeJson(Ksql) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Ksql != null)
            {
                ret += "<ksql>" + EscapeXml(Ksql) + "</ksql>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetRule
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Actions != null)
            {
                propertyValue = "[" + String.Join(", ", Actions.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"actions\": " + propertyValue);
            }
            if(Conditions != null)
            {
                propertyValue = "[" + String.Join(", ", Conditions.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"conditions\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Actions != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Actions.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<actions>" + propertyValue + "</actions>";
            }
            if(Conditions != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Conditions.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<conditions>" + propertyValue + "</conditions>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetRuleBase
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Description != null)
            {
                ret.Add("\"description\": " + "\"" + EscapeJson(Description) + "\"");
            }
            ret.Add("\"id\": " + Id);
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Description != null)
            {
                ret += "<description>" + EscapeXml(Description) + "</description>";
            }
            ret += "<id>" + Id + "</id>";
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetRuleFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetApplied != null)
            {
                propertyValue = AssetApplied.ToJson(currentVersion, omitObsolete);
                ret.Add("\"assetApplied\": " + propertyValue);
            }
            ret.Add("\"conditionsContainType\": " + "\"" + Enum.GetName(typeof(KalturaRuleConditionType), ConditionsContainType) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetApplied != null)
            {
                propertyValue = AssetApplied.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<assetApplied>" + propertyValue + "</assetApplied>";
            }
            ret += "<conditionsContainType>" + "\"" + Enum.GetName(typeof(KalturaRuleConditionType), ConditionsContainType) + "\"" + "</conditionsContainType>";
            return ret;
        }
    }
    public partial class KalturaAssetRuleListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetUserRule
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Actions != null)
            {
                propertyValue = "[" + String.Join(", ", Actions.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"actions\": " + propertyValue);
            }
            if(Conditions != null)
            {
                propertyValue = "[" + String.Join(", ", Conditions.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"conditions\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Actions != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Actions.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<actions>" + propertyValue + "</actions>";
            }
            if(Conditions != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Conditions.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<conditions>" + propertyValue + "</conditions>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetUserRuleFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AttachedUserIdEqualCurrent.HasValue)
            {
                ret.Add("\"attachedUserIdEqualCurrent\": " + AttachedUserIdEqualCurrent.ToString().ToLower());
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AttachedUserIdEqualCurrent.HasValue)
            {
                ret += "<attachedUserIdEqualCurrent>" + AttachedUserIdEqualCurrent.ToString().ToLower() + "</attachedUserIdEqualCurrent>";
            }
            return ret;
        }
    }
    public partial class KalturaAssetUserRuleListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaCDNAdapterProfile
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AdapterUrl != null)
            {
                ret.Add("\"adapterUrl\": " + "\"" + EscapeJson(AdapterUrl) + "\"");
            }
            if(BaseUrl != null)
            {
                ret.Add("\"baseUrl\": " + "\"" + EscapeJson(BaseUrl) + "\"");
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(IsActive.HasValue)
            {
                ret.Add("\"isActive\": " + IsActive.ToString().ToLower());
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(Settings != null)
            {
                propertyValue = "{" + String.Join(", ", Settings.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"settings\": " + propertyValue);
            }
            if(SharedSecret != null)
            {
                ret.Add("\"sharedSecret\": " + "\"" + EscapeJson(SharedSecret) + "\"");
            }
            if(SystemName != null)
            {
                ret.Add("\"systemName\": " + "\"" + EscapeJson(SystemName) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AdapterUrl != null)
            {
                ret += "<adapterUrl>" + EscapeXml(AdapterUrl) + "</adapterUrl>";
            }
            if(BaseUrl != null)
            {
                ret += "<baseUrl>" + EscapeXml(BaseUrl) + "</baseUrl>";
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(IsActive.HasValue)
            {
                ret += "<isActive>" + IsActive.ToString().ToLower() + "</isActive>";
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(Settings != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Settings.Select(pair => "<itemKey>" + pair.Key + "</itemKey>" + pair.Value.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<settings>" + propertyValue + "</settings>";
            }
            if(SharedSecret != null)
            {
                ret += "<sharedSecret>" + EscapeXml(SharedSecret) + "</sharedSecret>";
            }
            if(SystemName != null)
            {
                ret += "<systemName>" + EscapeXml(SystemName) + "</systemName>";
            }
            return ret;
        }
    }
    public partial class KalturaCDNAdapterProfileListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Adapters != null)
            {
                propertyValue = "[" + String.Join(", ", Adapters.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Adapters != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Adapters.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaCDNPartnerSettings
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(DefaultAdapterId.HasValue)
            {
                ret.Add("\"defaultAdapterId\": " + DefaultAdapterId);
            }
            if(DefaultRecordingAdapterId.HasValue)
            {
                ret.Add("\"defaultRecordingAdapterId\": " + DefaultRecordingAdapterId);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(DefaultAdapterId.HasValue)
            {
                ret += "<defaultAdapterId>" + DefaultAdapterId + "</defaultAdapterId>";
            }
            if(DefaultRecordingAdapterId.HasValue)
            {
                ret += "<defaultRecordingAdapterId>" + DefaultRecordingAdapterId + "</defaultRecordingAdapterId>";
            }
            return ret;
        }
    }
    public partial class KalturaChannelEnrichmentHolder
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"type\": " + "\"" + Enum.GetName(typeof(KalturaChannelEnrichment), type) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<type>" + "\"" + Enum.GetName(typeof(KalturaChannelEnrichment), type) + "\"" + "</type>";
            return ret;
        }
    }
    public partial class KalturaChannelProfile
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetTypes != null)
            {
                propertyValue = "[" + String.Join(", ", AssetTypes.Select(item => item.ToString())) + "]";
                ret.Add("\"assetTypes\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"asset_types\": " + propertyValue);
                }
            }
            if(Description != null)
            {
                ret.Add("\"description\": " + "\"" + EscapeJson(Description) + "\"");
            }
            if(FilterExpression != null)
            {
                ret.Add("\"filterExpression\": " + "\"" + EscapeJson(FilterExpression) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"filter_expression\": " + "\"" + EscapeJson(FilterExpression) + "\"");
                }
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(IsActive.HasValue)
            {
                ret.Add("\"isActive\": " + IsActive.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"is_active\": " + IsActive.ToString().ToLower());
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            ret.Add("\"order\": " + "\"" + Enum.GetName(typeof(KalturaOrder), Order) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetTypes != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", AssetTypes.Select(item => item.ToString())) + "</item>";
                ret += "<assetTypes>" + propertyValue + "</assetTypes>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<asset_types>" + propertyValue + "</asset_types>";
                }
            }
            if(Description != null)
            {
                ret += "<description>" + EscapeXml(Description) + "</description>";
            }
            if(FilterExpression != null)
            {
                ret += "<filterExpression>" + EscapeXml(FilterExpression) + "</filterExpression>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<filter_expression>" + EscapeXml(FilterExpression) + "</filter_expression>";
                }
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(IsActive.HasValue)
            {
                ret += "<isActive>" + IsActive.ToString().ToLower() + "</isActive>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<is_active>" + IsActive.ToString().ToLower() + "</is_active>";
                }
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            ret += "<order>" + "\"" + Enum.GetName(typeof(KalturaOrder), Order) + "\"" + "</order>";
            return ret;
        }
    }
    public partial class KalturaConcurrencyCondition
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"concurrencyLimitationType\": " + "\"" + Enum.GetName(typeof(KalturaConcurrencyLimitationType), ConcurrencyLimitationType) + "\"");
            ret.Add("\"limit\": " + Limit);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<concurrencyLimitationType>" + "\"" + Enum.GetName(typeof(KalturaConcurrencyLimitationType), ConcurrencyLimitationType) + "\"" + "</concurrencyLimitationType>";
            ret += "<limit>" + Limit + "</limit>";
            return ret;
        }
    }
    public partial class KalturaCondition
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Description != null)
            {
                ret.Add("\"description\": " + "\"" + EscapeJson(Description) + "\"");
            }
            ret.Add("\"type\": " + "\"" + Enum.GetName(typeof(KalturaRuleConditionType), Type) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Description != null)
            {
                ret += "<description>" + EscapeXml(Description) + "</description>";
            }
            ret += "<type>" + "\"" + Enum.GetName(typeof(KalturaRuleConditionType), Type) + "\"" + "</type>";
            return ret;
        }
    }
    public partial class KalturaCountryCondition
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Countries != null)
            {
                ret.Add("\"countries\": " + "\"" + EscapeJson(Countries) + "\"");
            }
            if(Not.HasValue)
            {
                ret.Add("\"not\": " + Not.ToString().ToLower());
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Countries != null)
            {
                ret += "<countries>" + EscapeXml(Countries) + "</countries>";
            }
            if(Not.HasValue)
            {
                ret += "<not>" + Not.ToString().ToLower() + "</not>";
            }
            return ret;
        }
    }
    public partial class KalturaCountryFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(IdIn != null)
            {
                ret.Add("\"idIn\": " + "\"" + EscapeJson(IdIn) + "\"");
            }
            if(IpEqual != null)
            {
                ret.Add("\"ipEqual\": " + "\"" + EscapeJson(IpEqual) + "\"");
            }
            if(IpEqualCurrent.HasValue)
            {
                ret.Add("\"ipEqualCurrent\": " + IpEqualCurrent.ToString().ToLower());
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(IdIn != null)
            {
                ret += "<idIn>" + EscapeXml(IdIn) + "</idIn>";
            }
            if(IpEqual != null)
            {
                ret += "<ipEqual>" + EscapeXml(IpEqual) + "</ipEqual>";
            }
            if(IpEqualCurrent.HasValue)
            {
                ret += "<ipEqualCurrent>" + IpEqualCurrent.ToString().ToLower() + "</ipEqualCurrent>";
            }
            return ret;
        }
    }
    public partial class KalturaCountryListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaCurrency
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Code != null)
            {
                ret.Add("\"code\": " + "\"" + EscapeJson(Code) + "\"");
            }
            ret.Add("\"isDefault\": " + IsDefault.ToString().ToLower());
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(Sign != null)
            {
                ret.Add("\"sign\": " + "\"" + EscapeJson(Sign) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Code != null)
            {
                ret += "<code>" + EscapeXml(Code) + "</code>";
            }
            ret += "<isDefault>" + IsDefault.ToString().ToLower() + "</isDefault>";
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(Sign != null)
            {
                ret += "<sign>" + EscapeXml(Sign) + "</sign>";
            }
            return ret;
        }
    }
    public partial class KalturaCurrencyFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(CodeIn != null)
            {
                ret.Add("\"codeIn\": " + "\"" + EscapeJson(CodeIn) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(CodeIn != null)
            {
                ret += "<codeIn>" + EscapeXml(CodeIn) + "</codeIn>";
            }
            return ret;
        }
    }
    public partial class KalturaCurrencyListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaDeviceBrandListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaDeviceFamilyListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaDrmProfile
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AdapterUrl != null)
            {
                ret.Add("\"adapterUrl\": " + "\"" + EscapeJson(AdapterUrl) + "\"");
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(IsActive.HasValue)
            {
                ret.Add("\"isActive\": " + IsActive.ToString().ToLower());
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(Settings != null)
            {
                ret.Add("\"settings\": " + "\"" + EscapeJson(Settings) + "\"");
            }
            if(SharedSecret != null)
            {
                ret.Add("\"sharedSecret\": " + "\"" + EscapeJson(SharedSecret) + "\"");
            }
            if(SystemName != null)
            {
                ret.Add("\"systemName\": " + "\"" + EscapeJson(SystemName) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AdapterUrl != null)
            {
                ret += "<adapterUrl>" + EscapeXml(AdapterUrl) + "</adapterUrl>";
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(IsActive.HasValue)
            {
                ret += "<isActive>" + IsActive.ToString().ToLower() + "</isActive>";
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(Settings != null)
            {
                ret += "<settings>" + EscapeXml(Settings) + "</settings>";
            }
            if(SharedSecret != null)
            {
                ret += "<sharedSecret>" + EscapeXml(SharedSecret) + "</sharedSecret>";
            }
            if(SystemName != null)
            {
                ret += "<systemName>" + EscapeXml(SystemName) + "</systemName>";
            }
            return ret;
        }
    }
    public partial class KalturaDrmProfileListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Adapters != null)
            {
                propertyValue = "[" + String.Join(", ", Adapters.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Adapters != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Adapters.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaExportFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ids != null)
            {
                propertyValue = "[" + String.Join(", ", ids.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"ids\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ids != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", ids.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<ids>" + propertyValue + "</ids>";
            }
            return ret;
        }
    }
    public partial class KalturaExportTask
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Alias != null)
            {
                ret.Add("\"alias\": " + "\"" + EscapeJson(Alias) + "\"");
            }
            ret.Add("\"dataType\": " + "\"" + Enum.GetName(typeof(KalturaExportDataType), DataType) + "\"");
            if (currentVersion == null || isOldVersion)
            {
                ret.Add("\"data_type\": " + "\"" + Enum.GetName(typeof(KalturaExportDataType), DataType) + "\"");
            }
            ret.Add("\"exportType\": " + "\"" + Enum.GetName(typeof(KalturaExportType), ExportType) + "\"");
            if (currentVersion == null || isOldVersion)
            {
                ret.Add("\"export_type\": " + "\"" + Enum.GetName(typeof(KalturaExportType), ExportType) + "\"");
            }
            if(Filter != null)
            {
                ret.Add("\"filter\": " + "\"" + EscapeJson(Filter) + "\"");
            }
            if(Frequency.HasValue)
            {
                ret.Add("\"frequency\": " + Frequency);
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(IsActive.HasValue)
            {
                ret.Add("\"isActive\": " + IsActive.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"is_active\": " + IsActive.ToString().ToLower());
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(NotificationUrl != null)
            {
                ret.Add("\"notificationUrl\": " + "\"" + EscapeJson(NotificationUrl) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"notification_url\": " + "\"" + EscapeJson(NotificationUrl) + "\"");
                }
            }
            if(VodTypes != null)
            {
                propertyValue = "[" + String.Join(", ", VodTypes.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"vodTypes\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"vod_types\": " + propertyValue);
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Alias != null)
            {
                ret += "<alias>" + EscapeXml(Alias) + "</alias>";
            }
            ret += "<dataType>" + "\"" + Enum.GetName(typeof(KalturaExportDataType), DataType) + "\"" + "</dataType>";
            if (currentVersion == null || isOldVersion)
            {
            ret += "<data_type>" + "\"" + Enum.GetName(typeof(KalturaExportDataType), DataType) + "\"" + "</data_type>";
            }
            ret += "<exportType>" + "\"" + Enum.GetName(typeof(KalturaExportType), ExportType) + "\"" + "</exportType>";
            if (currentVersion == null || isOldVersion)
            {
            ret += "<export_type>" + "\"" + Enum.GetName(typeof(KalturaExportType), ExportType) + "\"" + "</export_type>";
            }
            if(Filter != null)
            {
                ret += "<filter>" + EscapeXml(Filter) + "</filter>";
            }
            if(Frequency.HasValue)
            {
                ret += "<frequency>" + Frequency + "</frequency>";
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(IsActive.HasValue)
            {
                ret += "<isActive>" + IsActive.ToString().ToLower() + "</isActive>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<is_active>" + IsActive.ToString().ToLower() + "</is_active>";
                }
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(NotificationUrl != null)
            {
                ret += "<notificationUrl>" + EscapeXml(NotificationUrl) + "</notificationUrl>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<notification_url>" + EscapeXml(NotificationUrl) + "</notification_url>";
                }
            }
            if(VodTypes != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", VodTypes.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<vodTypes>" + propertyValue + "</vodTypes>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<vod_types>" + propertyValue + "</vod_types>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaExportTaskFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(IdIn != null)
            {
                ret.Add("\"idIn\": " + "\"" + EscapeJson(IdIn) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(IdIn != null)
            {
                ret += "<idIn>" + EscapeXml(IdIn) + "</idIn>";
            }
            return ret;
        }
    }
    public partial class KalturaExportTaskListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaExternalChannelProfile
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Enrichments != null)
            {
                propertyValue = "[" + String.Join(", ", Enrichments.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"enrichments\": " + propertyValue);
            }
            if(ExternalIdentifier != null)
            {
                ret.Add("\"externalIdentifier\": " + "\"" + EscapeJson(ExternalIdentifier) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"external_identifier\": " + "\"" + EscapeJson(ExternalIdentifier) + "\"");
                }
            }
            if(FilterExpression != null)
            {
                ret.Add("\"filterExpression\": " + "\"" + EscapeJson(FilterExpression) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"filter_expression\": " + "\"" + EscapeJson(FilterExpression) + "\"");
                }
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(IsActive.HasValue)
            {
                ret.Add("\"isActive\": " + IsActive.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"is_active\": " + IsActive.ToString().ToLower());
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(RecommendationEngineId.HasValue)
            {
                ret.Add("\"recommendationEngineId\": " + RecommendationEngineId);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"recommendation_engine_id\": " + RecommendationEngineId);
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Enrichments != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Enrichments.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<enrichments>" + propertyValue + "</enrichments>";
            }
            if(ExternalIdentifier != null)
            {
                ret += "<externalIdentifier>" + EscapeXml(ExternalIdentifier) + "</externalIdentifier>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<external_identifier>" + EscapeXml(ExternalIdentifier) + "</external_identifier>";
                }
            }
            if(FilterExpression != null)
            {
                ret += "<filterExpression>" + EscapeXml(FilterExpression) + "</filterExpression>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<filter_expression>" + EscapeXml(FilterExpression) + "</filter_expression>";
                }
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(IsActive.HasValue)
            {
                ret += "<isActive>" + IsActive.ToString().ToLower() + "</isActive>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<is_active>" + IsActive.ToString().ToLower() + "</is_active>";
                }
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(RecommendationEngineId.HasValue)
            {
                ret += "<recommendationEngineId>" + RecommendationEngineId + "</recommendationEngineId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<recommendation_engine_id>" + RecommendationEngineId + "</recommendation_engine_id>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaExternalChannelProfileListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaGenericRule
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Description != null)
            {
                ret.Add("\"description\": " + "\"" + EscapeJson(Description) + "\"");
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            ret.Add("\"ruleType\": " + "\"" + Enum.GetName(typeof(KalturaRuleType), RuleType) + "\"");
            if (currentVersion == null || isOldVersion)
            {
                ret.Add("\"rule_type\": " + "\"" + Enum.GetName(typeof(KalturaRuleType), RuleType) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Description != null)
            {
                ret += "<description>" + EscapeXml(Description) + "</description>";
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            ret += "<ruleType>" + "\"" + Enum.GetName(typeof(KalturaRuleType), RuleType) + "\"" + "</ruleType>";
            if (currentVersion == null || isOldVersion)
            {
            ret += "<rule_type>" + "\"" + Enum.GetName(typeof(KalturaRuleType), RuleType) + "\"" + "</rule_type>";
            }
            return ret;
        }
    }
    public partial class KalturaGenericRuleFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetId.HasValue)
            {
                ret.Add("\"assetId\": " + AssetId);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"asset_id\": " + AssetId);
                }
            }
            if(AssetType.HasValue)
            {
                ret.Add("\"assetType\": " + AssetType);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"asset_type\": " + AssetType);
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetId.HasValue)
            {
                ret += "<assetId>" + AssetId + "</assetId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<asset_id>" + AssetId + "</asset_id>";
                }
            }
            if(AssetType.HasValue)
            {
                ret += "<assetType>" + AssetType + "</assetType>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<asset_type>" + AssetType + "</asset_type>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaGenericRuleListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(GenericRules != null)
            {
                propertyValue = "[" + String.Join(", ", GenericRules.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(GenericRules != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", GenericRules.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaLanguage
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Code != null)
            {
                ret.Add("\"code\": " + "\"" + EscapeJson(Code) + "\"");
            }
            if(Direction != null)
            {
                ret.Add("\"direction\": " + "\"" + EscapeJson(Direction) + "\"");
            }
            ret.Add("\"isDefault\": " + IsDefault.ToString().ToLower());
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(SystemName != null)
            {
                ret.Add("\"systemName\": " + "\"" + EscapeJson(SystemName) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Code != null)
            {
                ret += "<code>" + EscapeXml(Code) + "</code>";
            }
            if(Direction != null)
            {
                ret += "<direction>" + EscapeXml(Direction) + "</direction>";
            }
            ret += "<isDefault>" + IsDefault.ToString().ToLower() + "</isDefault>";
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(SystemName != null)
            {
                ret += "<systemName>" + EscapeXml(SystemName) + "</systemName>";
            }
            return ret;
        }
    }
    public partial class KalturaLanguageFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(CodeIn != null)
            {
                ret.Add("\"codeIn\": " + "\"" + EscapeJson(CodeIn) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(CodeIn != null)
            {
                ret += "<codeIn>" + EscapeXml(CodeIn) + "</codeIn>";
            }
            return ret;
        }
    }
    public partial class KalturaLanguageListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaMeta
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(!DeprecatedAttribute.IsDeprecated("5.6.0.0", currentVersion) && AssetType.HasValue)
            {
                ret.Add("\"assetType\": " + "\"" + Enum.GetName(typeof(KalturaAssetType), AssetType) + "\"");
            }
            ret.Add("\"createDate\": " + CreateDate);
            if(DataType.HasValue)
            {
                ret.Add("\"dataType\": " + "\"" + Enum.GetName(typeof(KalturaMetaDataType), DataType) + "\"");
            }
            if(Features != null)
            {
                ret.Add("\"features\": " + "\"" + EscapeJson(Features) + "\"");
            }
            if(!DeprecatedAttribute.IsDeprecated("5.6.0.0", currentVersion) && FieldName.HasValue)
            {
                ret.Add("\"fieldName\": " + "\"" + Enum.GetName(typeof(KalturaMetaFieldName), FieldName) + "\"");
            }
            if(HelpText != null)
            {
                ret.Add("\"helpText\": " + "\"" + EscapeJson(HelpText) + "\"");
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + EscapeJson(Id) + "\"");
            }
            if(IsProtected.HasValue)
            {
                ret.Add("\"isProtected\": " + IsProtected.ToString().ToLower());
            }
            if(MultipleValue.HasValue)
            {
                ret.Add("\"multipleValue\": " + MultipleValue.ToString().ToLower());
            }
            ret.Add(Name.ToCustomJson(currentVersion, omitObsolete, "name"));
            if(ParentId != null)
            {
                ret.Add("\"parentId\": " + "\"" + EscapeJson(ParentId) + "\"");
            }
            if(!DeprecatedAttribute.IsDeprecated("5.6.0.0", currentVersion) && PartnerId.HasValue)
            {
                ret.Add("\"partnerId\": " + PartnerId);
            }
            if(SystemName != null)
            {
                ret.Add("\"systemName\": " + "\"" + EscapeJson(SystemName) + "\"");
            }
            if(!DeprecatedAttribute.IsDeprecated("5.6.0.0", currentVersion) && Type.HasValue)
            {
                ret.Add("\"type\": " + "\"" + Enum.GetName(typeof(KalturaMetaType), Type) + "\"");
            }
            ret.Add("\"updateDate\": " + UpdateDate);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(!DeprecatedAttribute.IsDeprecated("5.6.0.0", currentVersion) && AssetType.HasValue)
            {
                ret += "<assetType>" + "\"" + Enum.GetName(typeof(KalturaAssetType), AssetType) + "\"" + "</assetType>";
            }
            ret += "<createDate>" + CreateDate + "</createDate>";
            if(DataType.HasValue)
            {
                ret += "<dataType>" + "\"" + Enum.GetName(typeof(KalturaMetaDataType), DataType) + "\"" + "</dataType>";
            }
            if(Features != null)
            {
                ret += "<features>" + EscapeXml(Features) + "</features>";
            }
            if(!DeprecatedAttribute.IsDeprecated("5.6.0.0", currentVersion) && FieldName.HasValue)
            {
                ret += "<fieldName>" + "\"" + Enum.GetName(typeof(KalturaMetaFieldName), FieldName) + "\"" + "</fieldName>";
            }
            if(HelpText != null)
            {
                ret += "<helpText>" + EscapeXml(HelpText) + "</helpText>";
            }
            if(Id != null)
            {
                ret += "<id>" + EscapeXml(Id) + "</id>";
            }
            if(IsProtected.HasValue)
            {
                ret += "<isProtected>" + IsProtected.ToString().ToLower() + "</isProtected>";
            }
            if(MultipleValue.HasValue)
            {
                ret += "<multipleValue>" + MultipleValue.ToString().ToLower() + "</multipleValue>";
            }
            ret += Name.ToCustomXml(currentVersion, omitObsolete, "name");
            if(ParentId != null)
            {
                ret += "<parentId>" + EscapeXml(ParentId) + "</parentId>";
            }
            if(!DeprecatedAttribute.IsDeprecated("5.6.0.0", currentVersion) && PartnerId.HasValue)
            {
                ret += "<partnerId>" + PartnerId + "</partnerId>";
            }
            if(SystemName != null)
            {
                ret += "<systemName>" + EscapeXml(SystemName) + "</systemName>";
            }
            if(!DeprecatedAttribute.IsDeprecated("5.6.0.0", currentVersion) && Type.HasValue)
            {
                ret += "<type>" + "\"" + Enum.GetName(typeof(KalturaMetaType), Type) + "\"" + "</type>";
            }
            ret += "<updateDate>" + UpdateDate + "</updateDate>";
            return ret;
        }
    }
    public partial class KalturaMetaFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetStructIdEqual.HasValue)
            {
                ret.Add("\"assetStructIdEqual\": " + AssetStructIdEqual);
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && AssetTypeEqual.HasValue)
            {
                ret.Add("\"assetTypeEqual\": " + "\"" + Enum.GetName(typeof(KalturaAssetType), AssetTypeEqual) + "\"");
            }
            if(DataTypeEqual.HasValue)
            {
                ret.Add("\"dataTypeEqual\": " + "\"" + Enum.GetName(typeof(KalturaMetaDataType), DataTypeEqual) + "\"");
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && FeaturesIn != null)
            {
                ret.Add("\"featuresIn\": " + "\"" + EscapeJson(FeaturesIn) + "\"");
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && FieldNameEqual.HasValue)
            {
                ret.Add("\"fieldNameEqual\": " + "\"" + Enum.GetName(typeof(KalturaMetaFieldName), FieldNameEqual) + "\"");
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && FieldNameNotEqual.HasValue)
            {
                ret.Add("\"fieldNameNotEqual\": " + "\"" + Enum.GetName(typeof(KalturaMetaFieldName), FieldNameNotEqual) + "\"");
            }
            if(IdIn != null)
            {
                ret.Add("\"idIn\": " + "\"" + EscapeJson(IdIn) + "\"");
            }
            if(MultipleValueEqual.HasValue)
            {
                ret.Add("\"multipleValueEqual\": " + MultipleValueEqual.ToString().ToLower());
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && TypeEqual.HasValue)
            {
                ret.Add("\"typeEqual\": " + "\"" + Enum.GetName(typeof(KalturaMetaType), TypeEqual) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetStructIdEqual.HasValue)
            {
                ret += "<assetStructIdEqual>" + AssetStructIdEqual + "</assetStructIdEqual>";
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && AssetTypeEqual.HasValue)
            {
                ret += "<assetTypeEqual>" + "\"" + Enum.GetName(typeof(KalturaAssetType), AssetTypeEqual) + "\"" + "</assetTypeEqual>";
            }
            if(DataTypeEqual.HasValue)
            {
                ret += "<dataTypeEqual>" + "\"" + Enum.GetName(typeof(KalturaMetaDataType), DataTypeEqual) + "\"" + "</dataTypeEqual>";
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && FeaturesIn != null)
            {
                ret += "<featuresIn>" + EscapeXml(FeaturesIn) + "</featuresIn>";
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && FieldNameEqual.HasValue)
            {
                ret += "<fieldNameEqual>" + "\"" + Enum.GetName(typeof(KalturaMetaFieldName), FieldNameEqual) + "\"" + "</fieldNameEqual>";
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && FieldNameNotEqual.HasValue)
            {
                ret += "<fieldNameNotEqual>" + "\"" + Enum.GetName(typeof(KalturaMetaFieldName), FieldNameNotEqual) + "\"" + "</fieldNameNotEqual>";
            }
            if(IdIn != null)
            {
                ret += "<idIn>" + EscapeXml(IdIn) + "</idIn>";
            }
            if(MultipleValueEqual.HasValue)
            {
                ret += "<multipleValueEqual>" + MultipleValueEqual.ToString().ToLower() + "</multipleValueEqual>";
            }
            if(!DeprecatedAttribute.IsDeprecated("5.0.0.0", currentVersion) && TypeEqual.HasValue)
            {
                ret += "<typeEqual>" + "\"" + Enum.GetName(typeof(KalturaMetaType), TypeEqual) + "\"" + "</typeEqual>";
            }
            return ret;
        }
    }
    public partial class KalturaMetaListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Metas != null)
            {
                propertyValue = "[" + String.Join(", ", Metas.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Metas != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Metas.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaOSSAdapterBaseProfile
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            return ret;
        }
    }
    public partial class KalturaOSSAdapterProfile
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AdapterUrl != null)
            {
                ret.Add("\"adapterUrl\": " + "\"" + EscapeJson(AdapterUrl) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"adapter_url\": " + "\"" + EscapeJson(AdapterUrl) + "\"");
                }
            }
            if(ExternalIdentifier != null)
            {
                ret.Add("\"externalIdentifier\": " + "\"" + EscapeJson(ExternalIdentifier) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"external_identifier\": " + "\"" + EscapeJson(ExternalIdentifier) + "\"");
                }
            }
            if(IsActive.HasValue)
            {
                ret.Add("\"isActive\": " + IsActive.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"is_active\": " + IsActive.ToString().ToLower());
                }
            }
            if(Settings != null)
            {
                propertyValue = "{" + String.Join(", ", Settings.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"ossAdapterSettings\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"oss_adapter_settings\": " + propertyValue);
                }
            }
            if(SharedSecret != null)
            {
                ret.Add("\"sharedSecret\": " + "\"" + EscapeJson(SharedSecret) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"shared_secret\": " + "\"" + EscapeJson(SharedSecret) + "\"");
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AdapterUrl != null)
            {
                ret += "<adapterUrl>" + EscapeXml(AdapterUrl) + "</adapterUrl>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<adapter_url>" + EscapeXml(AdapterUrl) + "</adapter_url>";
                }
            }
            if(ExternalIdentifier != null)
            {
                ret += "<externalIdentifier>" + EscapeXml(ExternalIdentifier) + "</externalIdentifier>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<external_identifier>" + EscapeXml(ExternalIdentifier) + "</external_identifier>";
                }
            }
            if(IsActive.HasValue)
            {
                ret += "<isActive>" + IsActive.ToString().ToLower() + "</isActive>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<is_active>" + IsActive.ToString().ToLower() + "</is_active>";
                }
            }
            if(Settings != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Settings.Select(pair => "<itemKey>" + pair.Key + "</itemKey>" + pair.Value.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<ossAdapterSettings>" + propertyValue + "</ossAdapterSettings>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<oss_adapter_settings>" + propertyValue + "</oss_adapter_settings>";
                }
            }
            if(SharedSecret != null)
            {
                ret += "<sharedSecret>" + EscapeXml(SharedSecret) + "</sharedSecret>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<shared_secret>" + EscapeXml(SharedSecret) + "</shared_secret>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaOSSAdapterProfileListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(OSSAdapterProfiles != null)
            {
                propertyValue = "[" + String.Join(", ", OSSAdapterProfiles.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(OSSAdapterProfiles != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", OSSAdapterProfiles.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaParentalRule
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(blockAnonymousAccess.HasValue)
            {
                ret.Add("\"blockAnonymousAccess\": " + blockAnonymousAccess.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"block_anonymous_access\": " + blockAnonymousAccess.ToString().ToLower());
                }
            }
            ret.Add("\"createDate\": " + CreateDate);
            if(description != null)
            {
                ret.Add("\"description\": " + "\"" + EscapeJson(description) + "\"");
            }
            if(epgTagTypeId.HasValue)
            {
                ret.Add("\"epgTag\": " + epgTagTypeId);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"epg_tag\": " + epgTagTypeId);
                }
            }
            if(epgTagValues != null)
            {
                propertyValue = "[" + String.Join(", ", epgTagValues.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"epgTagValues\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"epg_tag_values\": " + propertyValue);
                }
            }
            ret.Add("\"id\": " + id);
            if(IsActive.HasValue)
            {
                ret.Add("\"isActive\": " + IsActive.ToString().ToLower());
            }
            ret.Add("\"isDefault\": " + isDefault.ToString().ToLower());
            if (currentVersion == null || isOldVersion)
            {
                ret.Add("\"is_default\": " + isDefault.ToString().ToLower());
            }
            if(mediaTagTypeId.HasValue)
            {
                ret.Add("\"mediaTag\": " + mediaTagTypeId);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"media_tag\": " + mediaTagTypeId);
                }
            }
            if(mediaTagValues != null)
            {
                propertyValue = "[" + String.Join(", ", mediaTagValues.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"mediaTagValues\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"media_tag_values\": " + propertyValue);
                }
            }
            if(name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(name) + "\"");
            }
            if(order.HasValue)
            {
                ret.Add("\"order\": " + order);
            }
            ret.Add("\"origin\": " + "\"" + Enum.GetName(typeof(KalturaRuleLevel), Origin) + "\"");
            if(ruleType.HasValue)
            {
                ret.Add("\"ruleType\": " + "\"" + Enum.GetName(typeof(KalturaParentalRuleType), ruleType) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"rule_type\": " + "\"" + Enum.GetName(typeof(KalturaParentalRuleType), ruleType) + "\"");
                }
            }
            ret.Add("\"updateDate\": " + UpdateDate);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(blockAnonymousAccess.HasValue)
            {
                ret += "<blockAnonymousAccess>" + blockAnonymousAccess.ToString().ToLower() + "</blockAnonymousAccess>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<block_anonymous_access>" + blockAnonymousAccess.ToString().ToLower() + "</block_anonymous_access>";
                }
            }
            ret += "<createDate>" + CreateDate + "</createDate>";
            if(description != null)
            {
                ret += "<description>" + EscapeXml(description) + "</description>";
            }
            if(epgTagTypeId.HasValue)
            {
                ret += "<epgTag>" + epgTagTypeId + "</epgTag>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<epg_tag>" + epgTagTypeId + "</epg_tag>";
                }
            }
            if(epgTagValues != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", epgTagValues.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<epgTagValues>" + propertyValue + "</epgTagValues>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<epg_tag_values>" + propertyValue + "</epg_tag_values>";
                }
            }
            ret += "<id>" + id + "</id>";
            if(IsActive.HasValue)
            {
                ret += "<isActive>" + IsActive.ToString().ToLower() + "</isActive>";
            }
            ret += "<isDefault>" + isDefault.ToString().ToLower() + "</isDefault>";
            if (currentVersion == null || isOldVersion)
            {
            ret += "<is_default>" + isDefault.ToString().ToLower() + "</is_default>";
            }
            if(mediaTagTypeId.HasValue)
            {
                ret += "<mediaTag>" + mediaTagTypeId + "</mediaTag>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<media_tag>" + mediaTagTypeId + "</media_tag>";
                }
            }
            if(mediaTagValues != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", mediaTagValues.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<mediaTagValues>" + propertyValue + "</mediaTagValues>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<media_tag_values>" + propertyValue + "</media_tag_values>";
                }
            }
            if(name != null)
            {
                ret += "<name>" + EscapeXml(name) + "</name>";
            }
            if(order.HasValue)
            {
                ret += "<order>" + order + "</order>";
            }
            ret += "<origin>" + "\"" + Enum.GetName(typeof(KalturaRuleLevel), Origin) + "\"" + "</origin>";
            if(ruleType.HasValue)
            {
                ret += "<ruleType>" + "\"" + Enum.GetName(typeof(KalturaParentalRuleType), ruleType) + "\"" + "</ruleType>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<rule_type>" + "\"" + Enum.GetName(typeof(KalturaParentalRuleType), ruleType) + "\"" + "</rule_type>";
                }
            }
            ret += "<updateDate>" + UpdateDate + "</updateDate>";
            return ret;
        }
    }
    public partial class KalturaParentalRuleFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(EntityReferenceEqual.HasValue)
            {
                ret.Add("\"entityReferenceEqual\": " + "\"" + Enum.GetName(typeof(KalturaEntityReferenceBy), EntityReferenceEqual) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(EntityReferenceEqual.HasValue)
            {
                ret += "<entityReferenceEqual>" + "\"" + Enum.GetName(typeof(KalturaEntityReferenceBy), EntityReferenceEqual) + "\"" + "</entityReferenceEqual>";
            }
            return ret;
        }
    }
    public partial class KalturaParentalRuleListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ParentalRule != null)
            {
                propertyValue = "[" + String.Join(", ", ParentalRule.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ParentalRule != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", ParentalRule.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaPermission
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(PermissionItems != null)
            {
                propertyValue = "[" + String.Join(", ", PermissionItems.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"permissionItems\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(PermissionItems != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", PermissionItems.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<permissionItems>" + propertyValue + "</permissionItems>";
            }
            return ret;
        }
    }
    public partial class KalturaPermissionItem
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            ret.Add("\"isExcluded\": " + IsExcluded.ToString().ToLower());
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            ret += "<isExcluded>" + IsExcluded.ToString().ToLower() + "</isExcluded>";
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            return ret;
        }
    }
    public partial class KalturaPermissionsFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Ids != null)
            {
                propertyValue = "[" + String.Join(", ", Ids.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"ids\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Ids != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Ids.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<ids>" + propertyValue + "</ids>";
            }
            return ret;
        }
    }
    public partial class KalturaPin
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"origin\": " + "\"" + Enum.GetName(typeof(KalturaRuleLevel), Origin) + "\"");
            if(PIN != null)
            {
                ret.Add("\"pin\": " + "\"" + EscapeJson(PIN) + "\"");
            }
            ret.Add("\"type\": " + "\"" + Enum.GetName(typeof(KalturaPinType), Type) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<origin>" + "\"" + Enum.GetName(typeof(KalturaRuleLevel), Origin) + "\"" + "</origin>";
            if(PIN != null)
            {
                ret += "<pin>" + EscapeXml(PIN) + "</pin>";
            }
            ret += "<type>" + "\"" + Enum.GetName(typeof(KalturaPinType), Type) + "\"" + "</type>";
            return ret;
        }
    }
    public partial class KalturaPinResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"origin\": " + "\"" + Enum.GetName(typeof(KalturaRuleLevel), Origin) + "\"");
            if(PIN != null)
            {
                ret.Add("\"pin\": " + "\"" + EscapeJson(PIN) + "\"");
            }
            ret.Add("\"type\": " + "\"" + Enum.GetName(typeof(KalturaPinType), Type) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<origin>" + "\"" + Enum.GetName(typeof(KalturaRuleLevel), Origin) + "\"" + "</origin>";
            if(PIN != null)
            {
                ret += "<pin>" + EscapeXml(PIN) + "</pin>";
            }
            ret += "<type>" + "\"" + Enum.GetName(typeof(KalturaPinType), Type) + "\"" + "</type>";
            return ret;
        }
    }
    public partial class KalturaPurchaseSettings
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Permission.HasValue)
            {
                ret.Add("\"permission\": " + "\"" + Enum.GetName(typeof(KalturaPurchaseSettingsType), Permission) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Permission.HasValue)
            {
                ret += "<permission>" + "\"" + Enum.GetName(typeof(KalturaPurchaseSettingsType), Permission) + "\"" + "</permission>";
            }
            return ret;
        }
    }
    public partial class KalturaPurchaseSettingsResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(PurchaseSettingsType.HasValue)
            {
                ret.Add("\"purchaseSettingsType\": " + "\"" + Enum.GetName(typeof(KalturaPurchaseSettingsType), PurchaseSettingsType) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"purchase_settings_type\": " + "\"" + Enum.GetName(typeof(KalturaPurchaseSettingsType), PurchaseSettingsType) + "\"");
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(PurchaseSettingsType.HasValue)
            {
                ret += "<purchaseSettingsType>" + "\"" + Enum.GetName(typeof(KalturaPurchaseSettingsType), PurchaseSettingsType) + "\"" + "</purchaseSettingsType>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<purchase_settings_type>" + "\"" + Enum.GetName(typeof(KalturaPurchaseSettingsType), PurchaseSettingsType) + "\"" + "</purchase_settings_type>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaRecommendationProfile
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AdapterUrl != null)
            {
                ret.Add("\"adapterUrl\": " + "\"" + EscapeJson(AdapterUrl) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"adapter_url\": " + "\"" + EscapeJson(AdapterUrl) + "\"");
                }
            }
            if(ExternalIdentifier != null)
            {
                ret.Add("\"externalIdentifier\": " + "\"" + EscapeJson(ExternalIdentifier) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"external_identifier\": " + "\"" + EscapeJson(ExternalIdentifier) + "\"");
                }
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(IsActive.HasValue)
            {
                ret.Add("\"isActive\": " + IsActive.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"is_active\": " + IsActive.ToString().ToLower());
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(Settings != null)
            {
                propertyValue = "{" + String.Join(", ", Settings.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"recommendationEngineSettings\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"recommendation_engine_settings\": " + propertyValue);
                }
            }
            if(SharedSecret != null)
            {
                ret.Add("\"sharedSecret\": " + "\"" + EscapeJson(SharedSecret) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"shared_secret\": " + "\"" + EscapeJson(SharedSecret) + "\"");
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AdapterUrl != null)
            {
                ret += "<adapterUrl>" + EscapeXml(AdapterUrl) + "</adapterUrl>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<adapter_url>" + EscapeXml(AdapterUrl) + "</adapter_url>";
                }
            }
            if(ExternalIdentifier != null)
            {
                ret += "<externalIdentifier>" + EscapeXml(ExternalIdentifier) + "</externalIdentifier>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<external_identifier>" + EscapeXml(ExternalIdentifier) + "</external_identifier>";
                }
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(IsActive.HasValue)
            {
                ret += "<isActive>" + IsActive.ToString().ToLower() + "</isActive>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<is_active>" + IsActive.ToString().ToLower() + "</is_active>";
                }
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(Settings != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Settings.Select(pair => "<itemKey>" + pair.Key + "</itemKey>" + pair.Value.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<recommendationEngineSettings>" + propertyValue + "</recommendationEngineSettings>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<recommendation_engine_settings>" + propertyValue + "</recommendation_engine_settings>";
                }
            }
            if(SharedSecret != null)
            {
                ret += "<sharedSecret>" + EscapeXml(SharedSecret) + "</sharedSecret>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<shared_secret>" + EscapeXml(SharedSecret) + "</shared_secret>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaRecommendationProfileListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(RecommendationProfiles != null)
            {
                propertyValue = "[" + String.Join(", ", RecommendationProfiles.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(RecommendationProfiles != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", RecommendationProfiles.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaRegion
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ExternalId != null)
            {
                ret.Add("\"externalId\": " + "\"" + EscapeJson(ExternalId) + "\"");
            }
            ret.Add("\"id\": " + Id);
            ret.Add("\"isDefault\": " + IsDefault.ToString().ToLower());
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(RegionalChannels != null)
            {
                propertyValue = "[" + String.Join(", ", RegionalChannels.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"linearChannels\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ExternalId != null)
            {
                ret += "<externalId>" + EscapeXml(ExternalId) + "</externalId>";
            }
            ret += "<id>" + Id + "</id>";
            ret += "<isDefault>" + IsDefault.ToString().ToLower() + "</isDefault>";
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(RegionalChannels != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", RegionalChannels.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<linearChannels>" + propertyValue + "</linearChannels>";
            }
            return ret;
        }
    }
    public partial class KalturaRegionalChannel
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"channelNumber\": " + ChannelNumber);
            ret.Add("\"linearChannelId\": " + LinearChannelId);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<channelNumber>" + ChannelNumber + "</channelNumber>";
            ret += "<linearChannelId>" + LinearChannelId + "</linearChannelId>";
            return ret;
        }
    }
    public partial class KalturaRegionFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ExternalIdIn != null)
            {
                ret.Add("\"externalIdIn\": " + "\"" + EscapeJson(ExternalIdIn) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ExternalIdIn != null)
            {
                ret += "<externalIdIn>" + EscapeXml(ExternalIdIn) + "</externalIdIn>";
            }
            return ret;
        }
    }
    public partial class KalturaRegionListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Regions != null)
            {
                propertyValue = "[" + String.Join(", ", Regions.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Regions != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Regions.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaRegistrySettings
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Key != null)
            {
                ret.Add("\"key\": " + "\"" + EscapeJson(Key) + "\"");
            }
            if(Value != null)
            {
                ret.Add("\"value\": " + "\"" + EscapeJson(Value) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Key != null)
            {
                ret += "<key>" + EscapeXml(Key) + "</key>";
            }
            if(Value != null)
            {
                ret += "<value>" + EscapeXml(Value) + "</value>";
            }
            return ret;
        }
    }
    public partial class KalturaRegistrySettingsListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(RegistrySettings != null)
            {
                propertyValue = "[" + String.Join(", ", RegistrySettings.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(RegistrySettings != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", RegistrySettings.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaRuleFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"by\": " + "\"" + Enum.GetName(typeof(KalturaEntityReferenceBy), By) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<by>" + "\"" + Enum.GetName(typeof(KalturaEntityReferenceBy), By) + "\"" + "</by>";
            return ret;
        }
    }
    public partial class KalturaSearchHistory
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Action != null)
            {
                ret.Add("\"action\": " + "\"" + EscapeJson(Action) + "\"");
            }
            ret.Add("\"createdAt\": " + CreatedAt);
            if(DeviceId != null)
            {
                ret.Add("\"deviceId\": " + "\"" + EscapeJson(DeviceId) + "\"");
            }
            if(Filter != null)
            {
                ret.Add("\"filter\": " + "\"" + EscapeJson(Filter) + "\"");
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + EscapeJson(Id) + "\"");
            }
            if(Language != null)
            {
                ret.Add("\"language\": " + "\"" + EscapeJson(Language) + "\"");
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(Service != null)
            {
                ret.Add("\"service\": " + "\"" + EscapeJson(Service) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Action != null)
            {
                ret += "<action>" + EscapeXml(Action) + "</action>";
            }
            ret += "<createdAt>" + CreatedAt + "</createdAt>";
            if(DeviceId != null)
            {
                ret += "<deviceId>" + EscapeXml(DeviceId) + "</deviceId>";
            }
            if(Filter != null)
            {
                ret += "<filter>" + EscapeXml(Filter) + "</filter>";
            }
            if(Id != null)
            {
                ret += "<id>" + EscapeXml(Id) + "</id>";
            }
            if(Language != null)
            {
                ret += "<language>" + EscapeXml(Language) + "</language>";
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(Service != null)
            {
                ret += "<service>" + EscapeXml(Service) + "</service>";
            }
            return ret;
        }
    }
    public partial class KalturaSearchHistoryFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaSearchHistoryListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaTimeShiftedTvPartnerSettings
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(CatchUpBufferLength.HasValue)
            {
                ret.Add("\"catchUpBufferLength\": " + CatchUpBufferLength);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"catch_up_buffer_length\": " + CatchUpBufferLength);
                }
            }
            if(CatchUpEnabled.HasValue)
            {
                ret.Add("\"catchUpEnabled\": " + CatchUpEnabled.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"catch_up_enabled\": " + CatchUpEnabled.ToString().ToLower());
                }
            }
            if(CdvrEnabled.HasValue)
            {
                ret.Add("\"cdvrEnabled\": " + CdvrEnabled.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"cdvr_enabled\": " + CdvrEnabled.ToString().ToLower());
                }
            }
            if(CleanupNoticePeriod.HasValue)
            {
                ret.Add("\"cleanupNoticePeriod\": " + CleanupNoticePeriod);
            }
            if(NonEntitledChannelPlaybackEnabled.HasValue)
            {
                ret.Add("\"nonEntitledChannelPlaybackEnabled\": " + NonEntitledChannelPlaybackEnabled.ToString().ToLower());
            }
            if(NonExistingChannelPlaybackEnabled.HasValue)
            {
                ret.Add("\"nonExistingChannelPlaybackEnabled\": " + NonExistingChannelPlaybackEnabled.ToString().ToLower());
            }
            if(PaddingAfterProgramEnds.HasValue)
            {
                ret.Add("\"paddingAfterProgramEnds\": " + PaddingAfterProgramEnds);
            }
            if(PaddingBeforeProgramStarts.HasValue)
            {
                ret.Add("\"paddingBeforeProgramStarts\": " + PaddingBeforeProgramStarts);
            }
            if(ProtectionEnabled.HasValue)
            {
                ret.Add("\"protectionEnabled\": " + ProtectionEnabled.ToString().ToLower());
            }
            if(ProtectionPeriod.HasValue)
            {
                ret.Add("\"protectionPeriod\": " + ProtectionPeriod);
            }
            if(ProtectionPolicy.HasValue)
            {
                ret.Add("\"protectionPolicy\": " + "\"" + Enum.GetName(typeof(KalturaProtectionPolicy), ProtectionPolicy) + "\"");
            }
            if(ProtectionQuotaPercentage.HasValue)
            {
                ret.Add("\"protectionQuotaPercentage\": " + ProtectionQuotaPercentage);
            }
            if(QuotaOveragePolicy.HasValue)
            {
                ret.Add("\"quotaOveragePolicy\": " + "\"" + Enum.GetName(typeof(KalturaQuotaOveragePolicy), QuotaOveragePolicy) + "\"");
            }
            if(RecordingLifetimePeriod.HasValue)
            {
                ret.Add("\"recordingLifetimePeriod\": " + RecordingLifetimePeriod);
            }
            if(RecordingScheduleWindow.HasValue)
            {
                ret.Add("\"recordingScheduleWindow\": " + RecordingScheduleWindow);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"recording_schedule_window\": " + RecordingScheduleWindow);
                }
            }
            if(RecordingScheduleWindowEnabled.HasValue)
            {
                ret.Add("\"recordingScheduleWindowEnabled\": " + RecordingScheduleWindowEnabled.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"recording_schedule_window_enabled\": " + RecordingScheduleWindowEnabled.ToString().ToLower());
                }
            }
            if(RecoveryGracePeriod.HasValue)
            {
                ret.Add("\"recoveryGracePeriod\": " + RecoveryGracePeriod);
            }
            if(SeriesRecordingEnabled.HasValue)
            {
                ret.Add("\"seriesRecordingEnabled\": " + SeriesRecordingEnabled.ToString().ToLower());
            }
            if(StartOverEnabled.HasValue)
            {
                ret.Add("\"startOverEnabled\": " + StartOverEnabled.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"start_over_enabled\": " + StartOverEnabled.ToString().ToLower());
                }
            }
            if(TrickPlayBufferLength.HasValue)
            {
                ret.Add("\"trickPlayBufferLength\": " + TrickPlayBufferLength);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"trick_play_buffer_length\": " + TrickPlayBufferLength);
                }
            }
            if(TrickPlayEnabled.HasValue)
            {
                ret.Add("\"trickPlayEnabled\": " + TrickPlayEnabled.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"trick_play_enabled\": " + TrickPlayEnabled.ToString().ToLower());
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(CatchUpBufferLength.HasValue)
            {
                ret += "<catchUpBufferLength>" + CatchUpBufferLength + "</catchUpBufferLength>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<catch_up_buffer_length>" + CatchUpBufferLength + "</catch_up_buffer_length>";
                }
            }
            if(CatchUpEnabled.HasValue)
            {
                ret += "<catchUpEnabled>" + CatchUpEnabled.ToString().ToLower() + "</catchUpEnabled>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<catch_up_enabled>" + CatchUpEnabled.ToString().ToLower() + "</catch_up_enabled>";
                }
            }
            if(CdvrEnabled.HasValue)
            {
                ret += "<cdvrEnabled>" + CdvrEnabled.ToString().ToLower() + "</cdvrEnabled>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<cdvr_enabled>" + CdvrEnabled.ToString().ToLower() + "</cdvr_enabled>";
                }
            }
            if(CleanupNoticePeriod.HasValue)
            {
                ret += "<cleanupNoticePeriod>" + CleanupNoticePeriod + "</cleanupNoticePeriod>";
            }
            if(NonEntitledChannelPlaybackEnabled.HasValue)
            {
                ret += "<nonEntitledChannelPlaybackEnabled>" + NonEntitledChannelPlaybackEnabled.ToString().ToLower() + "</nonEntitledChannelPlaybackEnabled>";
            }
            if(NonExistingChannelPlaybackEnabled.HasValue)
            {
                ret += "<nonExistingChannelPlaybackEnabled>" + NonExistingChannelPlaybackEnabled.ToString().ToLower() + "</nonExistingChannelPlaybackEnabled>";
            }
            if(PaddingAfterProgramEnds.HasValue)
            {
                ret += "<paddingAfterProgramEnds>" + PaddingAfterProgramEnds + "</paddingAfterProgramEnds>";
            }
            if(PaddingBeforeProgramStarts.HasValue)
            {
                ret += "<paddingBeforeProgramStarts>" + PaddingBeforeProgramStarts + "</paddingBeforeProgramStarts>";
            }
            if(ProtectionEnabled.HasValue)
            {
                ret += "<protectionEnabled>" + ProtectionEnabled.ToString().ToLower() + "</protectionEnabled>";
            }
            if(ProtectionPeriod.HasValue)
            {
                ret += "<protectionPeriod>" + ProtectionPeriod + "</protectionPeriod>";
            }
            if(ProtectionPolicy.HasValue)
            {
                ret += "<protectionPolicy>" + "\"" + Enum.GetName(typeof(KalturaProtectionPolicy), ProtectionPolicy) + "\"" + "</protectionPolicy>";
            }
            if(ProtectionQuotaPercentage.HasValue)
            {
                ret += "<protectionQuotaPercentage>" + ProtectionQuotaPercentage + "</protectionQuotaPercentage>";
            }
            if(QuotaOveragePolicy.HasValue)
            {
                ret += "<quotaOveragePolicy>" + "\"" + Enum.GetName(typeof(KalturaQuotaOveragePolicy), QuotaOveragePolicy) + "\"" + "</quotaOveragePolicy>";
            }
            if(RecordingLifetimePeriod.HasValue)
            {
                ret += "<recordingLifetimePeriod>" + RecordingLifetimePeriod + "</recordingLifetimePeriod>";
            }
            if(RecordingScheduleWindow.HasValue)
            {
                ret += "<recordingScheduleWindow>" + RecordingScheduleWindow + "</recordingScheduleWindow>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<recording_schedule_window>" + RecordingScheduleWindow + "</recording_schedule_window>";
                }
            }
            if(RecordingScheduleWindowEnabled.HasValue)
            {
                ret += "<recordingScheduleWindowEnabled>" + RecordingScheduleWindowEnabled.ToString().ToLower() + "</recordingScheduleWindowEnabled>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<recording_schedule_window_enabled>" + RecordingScheduleWindowEnabled.ToString().ToLower() + "</recording_schedule_window_enabled>";
                }
            }
            if(RecoveryGracePeriod.HasValue)
            {
                ret += "<recoveryGracePeriod>" + RecoveryGracePeriod + "</recoveryGracePeriod>";
            }
            if(SeriesRecordingEnabled.HasValue)
            {
                ret += "<seriesRecordingEnabled>" + SeriesRecordingEnabled.ToString().ToLower() + "</seriesRecordingEnabled>";
            }
            if(StartOverEnabled.HasValue)
            {
                ret += "<startOverEnabled>" + StartOverEnabled.ToString().ToLower() + "</startOverEnabled>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<start_over_enabled>" + StartOverEnabled.ToString().ToLower() + "</start_over_enabled>";
                }
            }
            if(TrickPlayBufferLength.HasValue)
            {
                ret += "<trickPlayBufferLength>" + TrickPlayBufferLength + "</trickPlayBufferLength>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<trick_play_buffer_length>" + TrickPlayBufferLength + "</trick_play_buffer_length>";
                }
            }
            if(TrickPlayEnabled.HasValue)
            {
                ret += "<trickPlayEnabled>" + TrickPlayEnabled.ToString().ToLower() + "</trickPlayEnabled>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<trick_play_enabled>" + TrickPlayEnabled.ToString().ToLower() + "</trick_play_enabled>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaUserAssetRule
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Description != null)
            {
                ret.Add("\"description\": " + "\"" + EscapeJson(Description) + "\"");
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            ret.Add("\"ruleType\": " + "\"" + Enum.GetName(typeof(KalturaRuleType), RuleType) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Description != null)
            {
                ret += "<description>" + EscapeXml(Description) + "</description>";
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            ret += "<ruleType>" + "\"" + Enum.GetName(typeof(KalturaRuleType), RuleType) + "\"" + "</ruleType>";
            return ret;
        }
    }
    public partial class KalturaUserAssetRuleFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetIdEqual.HasValue)
            {
                ret.Add("\"assetIdEqual\": " + AssetIdEqual);
            }
            if(AssetTypeEqual.HasValue)
            {
                ret.Add("\"assetTypeEqual\": " + AssetTypeEqual);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetIdEqual.HasValue)
            {
                ret += "<assetIdEqual>" + AssetIdEqual + "</assetIdEqual>";
            }
            if(AssetTypeEqual.HasValue)
            {
                ret += "<assetTypeEqual>" + AssetTypeEqual + "</assetTypeEqual>";
            }
            return ret;
        }
    }
    public partial class KalturaUserAssetRuleListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Rules != null)
            {
                propertyValue = "[" + String.Join(", ", Rules.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Rules != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Rules.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaUserRole
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ExcludedPermissionNames != null)
            {
                ret.Add("\"excludedPermissionNames\": " + "\"" + EscapeJson(ExcludedPermissionNames) + "\"");
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(PermissionNames != null)
            {
                ret.Add("\"permissionNames\": " + "\"" + EscapeJson(PermissionNames) + "\"");
            }
            if(!DeprecatedAttribute.IsDeprecated("4.6.0.0", currentVersion) && Permissions != null)
            {
                propertyValue = "[" + String.Join(", ", Permissions.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"permissions\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ExcludedPermissionNames != null)
            {
                ret += "<excludedPermissionNames>" + EscapeXml(ExcludedPermissionNames) + "</excludedPermissionNames>";
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(PermissionNames != null)
            {
                ret += "<permissionNames>" + EscapeXml(PermissionNames) + "</permissionNames>";
            }
            if(!DeprecatedAttribute.IsDeprecated("4.6.0.0", currentVersion) && Permissions != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Permissions.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<permissions>" + propertyValue + "</permissions>";
            }
            return ret;
        }
    }
    public partial class KalturaUserRoleFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(CurrentUserRoleIdsContains.HasValue)
            {
                ret.Add("\"currentUserRoleIdsContains\": " + CurrentUserRoleIdsContains.ToString().ToLower());
            }
            if(IdIn != null)
            {
                ret.Add("\"idIn\": " + "\"" + EscapeJson(IdIn) + "\"");
            }
            if(!omitObsolete && Ids != null)
            {
                propertyValue = "[" + String.Join(", ", Ids.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"ids\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(CurrentUserRoleIdsContains.HasValue)
            {
                ret += "<currentUserRoleIdsContains>" + CurrentUserRoleIdsContains.ToString().ToLower() + "</currentUserRoleIdsContains>";
            }
            if(IdIn != null)
            {
                ret += "<idIn>" + EscapeXml(IdIn) + "</idIn>";
            }
            if(!omitObsolete && Ids != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Ids.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<ids>" + propertyValue + "</ids>";
            }
            return ret;
        }
    }
    public partial class KalturaUserRoleListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(UserRoles != null)
            {
                propertyValue = "[" + String.Join(", ", UserRoles.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(UserRoles != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", UserRoles.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
}

namespace WebAPI.Models.Pricing
{
    public partial class KalturaAssetPrice
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetId != null)
            {
                ret.Add("\"asset_id\": " + "\"" + EscapeJson(AssetId) + "\"");
            }
            ret.Add("\"asset_type\": " + "\"" + Enum.GetName(typeof(KalturaAssetType), AssetType) + "\"");
            if(FilePrices != null)
            {
                propertyValue = "[" + String.Join(", ", FilePrices.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"file_prices\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AssetId != null)
            {
                ret += "<asset_id>" + EscapeXml(AssetId) + "</asset_id>";
            }
            ret += "<asset_type>" + "\"" + Enum.GetName(typeof(KalturaAssetType), AssetType) + "\"" + "</asset_type>";
            if(FilePrices != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", FilePrices.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<file_prices>" + propertyValue + "</file_prices>";
            }
            return ret;
        }
    }
    public partial class KalturaCollection
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Channels != null)
            {
                propertyValue = "[" + String.Join(", ", Channels.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"channels\": " + propertyValue);
            }
            if(CouponGroups != null)
            {
                propertyValue = "[" + String.Join(", ", CouponGroups.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"couponsGroups\": " + propertyValue);
            }
            ret.Add(Description.ToCustomJson(currentVersion, omitObsolete, "description"));
            if(DiscountModule != null)
            {
                propertyValue = DiscountModule.ToJson(currentVersion, omitObsolete);
                ret.Add("\"discountModule\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"discount_module\": " + propertyValue);
                }
            }
            if(EndDate.HasValue)
            {
                ret.Add("\"endDate\": " + EndDate);
            }
            if(ExternalId != null)
            {
                ret.Add("\"externalId\": " + "\"" + EscapeJson(ExternalId) + "\"");
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + EscapeJson(Id) + "\"");
            }
            ret.Add(Name.ToCustomJson(currentVersion, omitObsolete, "name"));
            if(PriceDetailsId.HasValue)
            {
                ret.Add("\"priceDetailsId\": " + PriceDetailsId);
            }
            if(ProductCodes != null)
            {
                propertyValue = "[" + String.Join(", ", ProductCodes.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"productCodes\": " + propertyValue);
            }
            if(StartDate.HasValue)
            {
                ret.Add("\"startDate\": " + StartDate);
            }
            if(UsageModule != null)
            {
                propertyValue = UsageModule.ToJson(currentVersion, omitObsolete);
                ret.Add("\"usageModule\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Channels != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Channels.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<channels>" + propertyValue + "</channels>";
            }
            if(CouponGroups != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", CouponGroups.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<couponsGroups>" + propertyValue + "</couponsGroups>";
            }
            ret += Description.ToCustomXml(currentVersion, omitObsolete, "description");
            if(DiscountModule != null)
            {
                propertyValue = DiscountModule.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<discountModule>" + propertyValue + "</discountModule>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<discount_module>" + propertyValue + "</discount_module>";
                }
            }
            if(EndDate.HasValue)
            {
                ret += "<endDate>" + EndDate + "</endDate>";
            }
            if(ExternalId != null)
            {
                ret += "<externalId>" + EscapeXml(ExternalId) + "</externalId>";
            }
            if(Id != null)
            {
                ret += "<id>" + EscapeXml(Id) + "</id>";
            }
            ret += Name.ToCustomXml(currentVersion, omitObsolete, "name");
            if(PriceDetailsId.HasValue)
            {
                ret += "<priceDetailsId>" + PriceDetailsId + "</priceDetailsId>";
            }
            if(ProductCodes != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", ProductCodes.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<productCodes>" + propertyValue + "</productCodes>";
            }
            if(StartDate.HasValue)
            {
                ret += "<startDate>" + StartDate + "</startDate>";
            }
            if(UsageModule != null)
            {
                propertyValue = UsageModule.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<usageModule>" + propertyValue + "</usageModule>";
            }
            return ret;
        }
    }
    public partial class KalturaCollectionFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(CollectionIdIn != null)
            {
                ret.Add("\"collectionIdIn\": " + "\"" + EscapeJson(CollectionIdIn) + "\"");
            }
            if(MediaFileIdEqual.HasValue)
            {
                ret.Add("\"mediaFileIdEqual\": " + MediaFileIdEqual);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(CollectionIdIn != null)
            {
                ret += "<collectionIdIn>" + EscapeXml(CollectionIdIn) + "</collectionIdIn>";
            }
            if(MediaFileIdEqual.HasValue)
            {
                ret += "<mediaFileIdEqual>" + MediaFileIdEqual + "</mediaFileIdEqual>";
            }
            return ret;
        }
    }
    public partial class KalturaCollectionListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Collections != null)
            {
                propertyValue = "[" + String.Join(", ", Collections.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Collections != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Collections.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaCollectionPrice
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaCoupon
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(CouponsGroup != null)
            {
                propertyValue = CouponsGroup.ToJson(currentVersion, omitObsolete);
                ret.Add("\"couponsGroup\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"coupons_group\": " + propertyValue);
                }
            }
            if(LeftUses.HasValue)
            {
                ret.Add("\"leftUses\": " + LeftUses);
            }
            ret.Add("\"status\": " + "\"" + Enum.GetName(typeof(KalturaCouponStatus), Status) + "\"");
            if(TotalUses.HasValue)
            {
                ret.Add("\"totalUses\": " + TotalUses);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(CouponsGroup != null)
            {
                propertyValue = CouponsGroup.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<couponsGroup>" + propertyValue + "</couponsGroup>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<coupons_group>" + propertyValue + "</coupons_group>";
                }
            }
            if(LeftUses.HasValue)
            {
                ret += "<leftUses>" + LeftUses + "</leftUses>";
            }
            ret += "<status>" + "\"" + Enum.GetName(typeof(KalturaCouponStatus), Status) + "\"" + "</status>";
            if(TotalUses.HasValue)
            {
                ret += "<totalUses>" + TotalUses + "</totalUses>";
            }
            return ret;
        }
    }
    public partial class KalturaCouponGenerationOptions
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaCouponsGroup
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(CouponGroupType.HasValue)
            {
                ret.Add("\"couponGroupType\": " + "\"" + Enum.GetName(typeof(KalturaCouponGroupType), CouponGroupType) + "\"");
            }
            if(!DeprecatedAttribute.IsDeprecated("4.8.0.0", currentVersion) && Descriptions != null)
            {
                propertyValue = "[" + String.Join(", ", Descriptions.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"descriptions\": " + propertyValue);
            }
            if(!DeprecatedAttribute.IsDeprecated("4.8.2.0", currentVersion) && DiscountCode.HasValue)
            {
                ret.Add("\"discountCode\": " + DiscountCode);
            }
            if(DiscountId.HasValue)
            {
                ret.Add("\"discountId\": " + DiscountId);
            }
            if(EndDate.HasValue)
            {
                ret.Add("\"endDate\": " + EndDate);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"end_date\": " + EndDate);
                }
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + EscapeJson(Id) + "\"");
            }
            if(MaxHouseholdUses.HasValue)
            {
                ret.Add("\"maxHouseholdUses\": " + MaxHouseholdUses);
            }
            if(MaxUsesNumber.HasValue)
            {
                ret.Add("\"maxUsesNumber\": " + MaxUsesNumber);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"max_uses_number\": " + MaxUsesNumber);
                }
            }
            if(MaxUsesNumberOnRenewableSub.HasValue)
            {
                ret.Add("\"maxUsesNumberOnRenewableSub\": " + MaxUsesNumberOnRenewableSub);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"max_uses_number_on_renewable_sub\": " + MaxUsesNumberOnRenewableSub);
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(StartDate.HasValue)
            {
                ret.Add("\"startDate\": " + StartDate);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"start_date\": " + StartDate);
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(CouponGroupType.HasValue)
            {
                ret += "<couponGroupType>" + "\"" + Enum.GetName(typeof(KalturaCouponGroupType), CouponGroupType) + "\"" + "</couponGroupType>";
            }
            if(!DeprecatedAttribute.IsDeprecated("4.8.0.0", currentVersion) && Descriptions != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Descriptions.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<descriptions>" + propertyValue + "</descriptions>";
            }
            if(!DeprecatedAttribute.IsDeprecated("4.8.2.0", currentVersion) && DiscountCode.HasValue)
            {
                ret += "<discountCode>" + DiscountCode + "</discountCode>";
            }
            if(DiscountId.HasValue)
            {
                ret += "<discountId>" + DiscountId + "</discountId>";
            }
            if(EndDate.HasValue)
            {
                ret += "<endDate>" + EndDate + "</endDate>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<end_date>" + EndDate + "</end_date>";
                }
            }
            if(Id != null)
            {
                ret += "<id>" + EscapeXml(Id) + "</id>";
            }
            if(MaxHouseholdUses.HasValue)
            {
                ret += "<maxHouseholdUses>" + MaxHouseholdUses + "</maxHouseholdUses>";
            }
            if(MaxUsesNumber.HasValue)
            {
                ret += "<maxUsesNumber>" + MaxUsesNumber + "</maxUsesNumber>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<max_uses_number>" + MaxUsesNumber + "</max_uses_number>";
                }
            }
            if(MaxUsesNumberOnRenewableSub.HasValue)
            {
                ret += "<maxUsesNumberOnRenewableSub>" + MaxUsesNumberOnRenewableSub + "</maxUsesNumberOnRenewableSub>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<max_uses_number_on_renewable_sub>" + MaxUsesNumberOnRenewableSub + "</max_uses_number_on_renewable_sub>";
                }
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(StartDate.HasValue)
            {
                ret += "<startDate>" + StartDate + "</startDate>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<start_date>" + StartDate + "</start_date>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaCouponsGroupListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(couponsGroups != null)
            {
                propertyValue = "[" + String.Join(", ", couponsGroups.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(couponsGroups != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", couponsGroups.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaDiscount
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"percentage\": " + Percentage);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<percentage>" + Percentage + "</percentage>";
            return ret;
        }
    }
    public partial class KalturaDiscountDetails
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"endDate\": " + EndtDate);
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(MultiCurrencyDiscount != null)
            {
                propertyValue = "[" + String.Join(", ", MultiCurrencyDiscount.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"multiCurrencyDiscount\": " + propertyValue);
            }
            if(name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(name) + "\"");
            }
            ret.Add("\"startDate\": " + StartDate);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<endDate>" + EndtDate + "</endDate>";
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(MultiCurrencyDiscount != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", MultiCurrencyDiscount.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<multiCurrencyDiscount>" + propertyValue + "</multiCurrencyDiscount>";
            }
            if(name != null)
            {
                ret += "<name>" + EscapeXml(name) + "</name>";
            }
            ret += "<startDate>" + StartDate + "</startDate>";
            return ret;
        }
    }
    public partial class KalturaDiscountDetailsFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(IdIn != null)
            {
                ret.Add("\"idIn\": " + "\"" + EscapeJson(IdIn) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(IdIn != null)
            {
                ret += "<idIn>" + EscapeXml(IdIn) + "</idIn>";
            }
            return ret;
        }
    }
    public partial class KalturaDiscountDetailsListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Discounts != null)
            {
                propertyValue = "[" + String.Join(", ", Discounts.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Discounts != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Discounts.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaDiscountModule
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(EndDate.HasValue)
            {
                ret.Add("\"endDate\": " + EndDate);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"end_date\": " + EndDate);
                }
            }
            if(Percent.HasValue)
            {
                ret.Add("\"percent\": " + Percent);
            }
            if(StartDate.HasValue)
            {
                ret.Add("\"startDate\": " + StartDate);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"start_date\": " + StartDate);
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(EndDate.HasValue)
            {
                ret += "<endDate>" + EndDate + "</endDate>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<end_date>" + EndDate + "</end_date>";
                }
            }
            if(Percent.HasValue)
            {
                ret += "<percent>" + Percent + "</percent>";
            }
            if(StartDate.HasValue)
            {
                ret += "<startDate>" + StartDate + "</startDate>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<start_date>" + StartDate + "</start_date>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaItemPrice
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(FileId.HasValue)
            {
                ret.Add("\"fileId\": " + FileId);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"file_id\": " + FileId);
                }
            }
            if(PPVPriceDetails != null)
            {
                propertyValue = "[" + String.Join(", ", PPVPriceDetails.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"ppvPriceDetails\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"ppv_price_details\": " + propertyValue);
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(FileId.HasValue)
            {
                ret += "<fileId>" + FileId + "</fileId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<file_id>" + FileId + "</file_id>";
                }
            }
            if(PPVPriceDetails != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", PPVPriceDetails.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<ppvPriceDetails>" + propertyValue + "</ppvPriceDetails>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<ppv_price_details>" + propertyValue + "</ppv_price_details>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaItemPriceListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ItemPrice != null)
            {
                propertyValue = "[" + String.Join(", ", ItemPrice.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ItemPrice != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", ItemPrice.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaPpv
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(CouponsGroup != null)
            {
                propertyValue = CouponsGroup.ToJson(currentVersion, omitObsolete);
                ret.Add("\"couponsGroup\": " + propertyValue);
            }
            if(Descriptions != null)
            {
                propertyValue = "[" + String.Join(", ", Descriptions.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"descriptions\": " + propertyValue);
            }
            if(DiscountModule != null)
            {
                propertyValue = DiscountModule.ToJson(currentVersion, omitObsolete);
                ret.Add("\"discountModule\": " + propertyValue);
            }
            if(FileTypes != null)
            {
                propertyValue = "[" + String.Join(", ", FileTypes.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"fileTypes\": " + propertyValue);
            }
            if(FirstDeviceLimitation.HasValue)
            {
                ret.Add("\"firstDeviceLimitation\": " + FirstDeviceLimitation.ToString().ToLower());
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + EscapeJson(Id) + "\"");
            }
            if(IsSubscriptionOnly.HasValue)
            {
                ret.Add("\"isSubscriptionOnly\": " + IsSubscriptionOnly.ToString().ToLower());
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(Price != null)
            {
                propertyValue = Price.ToJson(currentVersion, omitObsolete);
                ret.Add("\"price\": " + propertyValue);
            }
            if(ProductCode != null)
            {
                ret.Add("\"productCode\": " + "\"" + EscapeJson(ProductCode) + "\"");
            }
            if(UsageModule != null)
            {
                propertyValue = UsageModule.ToJson(currentVersion, omitObsolete);
                ret.Add("\"usageModule\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(CouponsGroup != null)
            {
                propertyValue = CouponsGroup.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<couponsGroup>" + propertyValue + "</couponsGroup>";
            }
            if(Descriptions != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Descriptions.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<descriptions>" + propertyValue + "</descriptions>";
            }
            if(DiscountModule != null)
            {
                propertyValue = DiscountModule.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<discountModule>" + propertyValue + "</discountModule>";
            }
            if(FileTypes != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", FileTypes.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<fileTypes>" + propertyValue + "</fileTypes>";
            }
            if(FirstDeviceLimitation.HasValue)
            {
                ret += "<firstDeviceLimitation>" + FirstDeviceLimitation.ToString().ToLower() + "</firstDeviceLimitation>";
            }
            if(Id != null)
            {
                ret += "<id>" + EscapeXml(Id) + "</id>";
            }
            if(IsSubscriptionOnly.HasValue)
            {
                ret += "<isSubscriptionOnly>" + IsSubscriptionOnly.ToString().ToLower() + "</isSubscriptionOnly>";
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(Price != null)
            {
                propertyValue = Price.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<price>" + propertyValue + "</price>";
            }
            if(ProductCode != null)
            {
                ret += "<productCode>" + EscapeXml(ProductCode) + "</productCode>";
            }
            if(UsageModule != null)
            {
                propertyValue = UsageModule.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<usageModule>" + propertyValue + "</usageModule>";
            }
            return ret;
        }
    }
    public partial class KalturaPPVItemPriceDetails
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(CollectionId != null)
            {
                ret.Add("\"collectionId\": " + "\"" + EscapeJson(CollectionId) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"collection_id\": " + "\"" + EscapeJson(CollectionId) + "\"");
                }
            }
            if(DiscountEndDate.HasValue)
            {
                ret.Add("\"discountEndDate\": " + DiscountEndDate);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"discount_end_date\": " + DiscountEndDate);
                }
            }
            if(EndDate.HasValue)
            {
                ret.Add("\"endDate\": " + EndDate);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"end_date\": " + EndDate);
                }
            }
            if(FirstDeviceName != null)
            {
                ret.Add("\"firstDeviceName\": " + "\"" + EscapeJson(FirstDeviceName) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"first_device_name\": " + "\"" + EscapeJson(FirstDeviceName) + "\"");
                }
            }
            if(FullPrice != null)
            {
                propertyValue = FullPrice.ToJson(currentVersion, omitObsolete);
                ret.Add("\"fullPrice\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"full_price\": " + propertyValue);
                }
            }
            if(IsInCancelationPeriod.HasValue)
            {
                ret.Add("\"isInCancelationPeriod\": " + IsInCancelationPeriod.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"is_in_cancelation_period\": " + IsInCancelationPeriod.ToString().ToLower());
                }
            }
            if(IsSubscriptionOnly.HasValue)
            {
                ret.Add("\"isSubscriptionOnly\": " + IsSubscriptionOnly.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"is_subscription_only\": " + IsSubscriptionOnly.ToString().ToLower());
                }
            }
            if(PPVDescriptions != null)
            {
                propertyValue = "[" + String.Join(", ", PPVDescriptions.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"ppvDescriptions\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"ppv_descriptions\": " + propertyValue);
                }
            }
            if(PPVModuleId != null)
            {
                ret.Add("\"ppvModuleId\": " + "\"" + EscapeJson(PPVModuleId) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"ppv_module_id\": " + "\"" + EscapeJson(PPVModuleId) + "\"");
                }
            }
            if(PrePaidId != null)
            {
                ret.Add("\"prePaidId\": " + "\"" + EscapeJson(PrePaidId) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"pre_paid_id\": " + "\"" + EscapeJson(PrePaidId) + "\"");
                }
            }
            if(Price != null)
            {
                propertyValue = Price.ToJson(currentVersion, omitObsolete);
                ret.Add("\"price\": " + propertyValue);
            }
            if(ProductCode != null)
            {
                ret.Add("\"ppvProductCode\": " + "\"" + EscapeJson(ProductCode) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"ppv_product_code\": " + "\"" + EscapeJson(ProductCode) + "\"");
                }
            }
            if(PurchasedMediaFileId.HasValue)
            {
                ret.Add("\"purchasedMediaFileId\": " + PurchasedMediaFileId);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"purchased_media_file_id\": " + PurchasedMediaFileId);
                }
            }
            ret.Add("\"purchaseStatus\": " + "\"" + Enum.GetName(typeof(KalturaPurchaseStatus), PurchaseStatus) + "\"");
            if (currentVersion == null || isOldVersion)
            {
                ret.Add("\"purchase_status\": " + "\"" + Enum.GetName(typeof(KalturaPurchaseStatus), PurchaseStatus) + "\"");
            }
            if(PurchaseUserId != null)
            {
                ret.Add("\"purchaseUserId\": " + "\"" + EscapeJson(PurchaseUserId) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"purchase_user_id\": " + "\"" + EscapeJson(PurchaseUserId) + "\"");
                }
            }
            if(RelatedMediaFileIds != null)
            {
                propertyValue = "[" + String.Join(", ", RelatedMediaFileIds.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"relatedMediaFileIds\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"related_media_file_ids\": " + propertyValue);
                }
            }
            if(StartDate.HasValue)
            {
                ret.Add("\"startDate\": " + StartDate);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"start_date\": " + StartDate);
                }
            }
            if(SubscriptionId != null)
            {
                ret.Add("\"subscriptionId\": " + "\"" + EscapeJson(SubscriptionId) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"subscription_id\": " + "\"" + EscapeJson(SubscriptionId) + "\"");
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(CollectionId != null)
            {
                ret += "<collectionId>" + EscapeXml(CollectionId) + "</collectionId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<collection_id>" + EscapeXml(CollectionId) + "</collection_id>";
                }
            }
            if(DiscountEndDate.HasValue)
            {
                ret += "<discountEndDate>" + DiscountEndDate + "</discountEndDate>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<discount_end_date>" + DiscountEndDate + "</discount_end_date>";
                }
            }
            if(EndDate.HasValue)
            {
                ret += "<endDate>" + EndDate + "</endDate>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<end_date>" + EndDate + "</end_date>";
                }
            }
            if(FirstDeviceName != null)
            {
                ret += "<firstDeviceName>" + EscapeXml(FirstDeviceName) + "</firstDeviceName>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<first_device_name>" + EscapeXml(FirstDeviceName) + "</first_device_name>";
                }
            }
            if(FullPrice != null)
            {
                propertyValue = FullPrice.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<fullPrice>" + propertyValue + "</fullPrice>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<full_price>" + propertyValue + "</full_price>";
                }
            }
            if(IsInCancelationPeriod.HasValue)
            {
                ret += "<isInCancelationPeriod>" + IsInCancelationPeriod.ToString().ToLower() + "</isInCancelationPeriod>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<is_in_cancelation_period>" + IsInCancelationPeriod.ToString().ToLower() + "</is_in_cancelation_period>";
                }
            }
            if(IsSubscriptionOnly.HasValue)
            {
                ret += "<isSubscriptionOnly>" + IsSubscriptionOnly.ToString().ToLower() + "</isSubscriptionOnly>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<is_subscription_only>" + IsSubscriptionOnly.ToString().ToLower() + "</is_subscription_only>";
                }
            }
            if(PPVDescriptions != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", PPVDescriptions.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<ppvDescriptions>" + propertyValue + "</ppvDescriptions>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<ppv_descriptions>" + propertyValue + "</ppv_descriptions>";
                }
            }
            if(PPVModuleId != null)
            {
                ret += "<ppvModuleId>" + EscapeXml(PPVModuleId) + "</ppvModuleId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<ppv_module_id>" + EscapeXml(PPVModuleId) + "</ppv_module_id>";
                }
            }
            if(PrePaidId != null)
            {
                ret += "<prePaidId>" + EscapeXml(PrePaidId) + "</prePaidId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<pre_paid_id>" + EscapeXml(PrePaidId) + "</pre_paid_id>";
                }
            }
            if(Price != null)
            {
                propertyValue = Price.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<price>" + propertyValue + "</price>";
            }
            if(ProductCode != null)
            {
                ret += "<ppvProductCode>" + EscapeXml(ProductCode) + "</ppvProductCode>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<ppv_product_code>" + EscapeXml(ProductCode) + "</ppv_product_code>";
                }
            }
            if(PurchasedMediaFileId.HasValue)
            {
                ret += "<purchasedMediaFileId>" + PurchasedMediaFileId + "</purchasedMediaFileId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<purchased_media_file_id>" + PurchasedMediaFileId + "</purchased_media_file_id>";
                }
            }
            ret += "<purchaseStatus>" + "\"" + Enum.GetName(typeof(KalturaPurchaseStatus), PurchaseStatus) + "\"" + "</purchaseStatus>";
            if (currentVersion == null || isOldVersion)
            {
            ret += "<purchase_status>" + "\"" + Enum.GetName(typeof(KalturaPurchaseStatus), PurchaseStatus) + "\"" + "</purchase_status>";
            }
            if(PurchaseUserId != null)
            {
                ret += "<purchaseUserId>" + EscapeXml(PurchaseUserId) + "</purchaseUserId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<purchase_user_id>" + EscapeXml(PurchaseUserId) + "</purchase_user_id>";
                }
            }
            if(RelatedMediaFileIds != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", RelatedMediaFileIds.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<relatedMediaFileIds>" + propertyValue + "</relatedMediaFileIds>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<related_media_file_ids>" + propertyValue + "</related_media_file_ids>";
                }
            }
            if(StartDate.HasValue)
            {
                ret += "<startDate>" + StartDate + "</startDate>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<start_date>" + StartDate + "</start_date>";
                }
            }
            if(SubscriptionId != null)
            {
                ret += "<subscriptionId>" + EscapeXml(SubscriptionId) + "</subscriptionId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<subscription_id>" + EscapeXml(SubscriptionId) + "</subscription_id>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaPpvPrice
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(CollectionId != null)
            {
                ret.Add("\"collectionId\": " + "\"" + EscapeJson(CollectionId) + "\"");
            }
            if(DiscountEndDate.HasValue)
            {
                ret.Add("\"discountEndDate\": " + DiscountEndDate);
            }
            if(EndDate.HasValue)
            {
                ret.Add("\"endDate\": " + EndDate);
            }
            if(FileId.HasValue)
            {
                ret.Add("\"fileId\": " + FileId);
            }
            if(FirstDeviceName != null)
            {
                ret.Add("\"firstDeviceName\": " + "\"" + EscapeJson(FirstDeviceName) + "\"");
            }
            if(FullPrice != null)
            {
                propertyValue = FullPrice.ToJson(currentVersion, omitObsolete);
                ret.Add("\"fullPrice\": " + propertyValue);
            }
            if(IsInCancelationPeriod.HasValue)
            {
                ret.Add("\"isInCancelationPeriod\": " + IsInCancelationPeriod.ToString().ToLower());
            }
            if(IsSubscriptionOnly.HasValue)
            {
                ret.Add("\"isSubscriptionOnly\": " + IsSubscriptionOnly.ToString().ToLower());
            }
            if(PPVDescriptions != null)
            {
                propertyValue = "[" + String.Join(", ", PPVDescriptions.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"ppvDescriptions\": " + propertyValue);
            }
            if(PPVModuleId != null)
            {
                ret.Add("\"ppvModuleId\": " + "\"" + EscapeJson(PPVModuleId) + "\"");
            }
            if(PrePaidId != null)
            {
                ret.Add("\"prePaidId\": " + "\"" + EscapeJson(PrePaidId) + "\"");
            }
            if(ProductCode != null)
            {
                ret.Add("\"ppvProductCode\": " + "\"" + EscapeJson(ProductCode) + "\"");
            }
            if(PurchasedMediaFileId.HasValue)
            {
                ret.Add("\"purchasedMediaFileId\": " + PurchasedMediaFileId);
            }
            if(PurchaseUserId != null)
            {
                ret.Add("\"purchaseUserId\": " + "\"" + EscapeJson(PurchaseUserId) + "\"");
            }
            if(RelatedMediaFileIds != null)
            {
                propertyValue = "[" + String.Join(", ", RelatedMediaFileIds.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"relatedMediaFileIds\": " + propertyValue);
            }
            if(StartDate.HasValue)
            {
                ret.Add("\"startDate\": " + StartDate);
            }
            if(SubscriptionId != null)
            {
                ret.Add("\"subscriptionId\": " + "\"" + EscapeJson(SubscriptionId) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(CollectionId != null)
            {
                ret += "<collectionId>" + EscapeXml(CollectionId) + "</collectionId>";
            }
            if(DiscountEndDate.HasValue)
            {
                ret += "<discountEndDate>" + DiscountEndDate + "</discountEndDate>";
            }
            if(EndDate.HasValue)
            {
                ret += "<endDate>" + EndDate + "</endDate>";
            }
            if(FileId.HasValue)
            {
                ret += "<fileId>" + FileId + "</fileId>";
            }
            if(FirstDeviceName != null)
            {
                ret += "<firstDeviceName>" + EscapeXml(FirstDeviceName) + "</firstDeviceName>";
            }
            if(FullPrice != null)
            {
                propertyValue = FullPrice.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<fullPrice>" + propertyValue + "</fullPrice>";
            }
            if(IsInCancelationPeriod.HasValue)
            {
                ret += "<isInCancelationPeriod>" + IsInCancelationPeriod.ToString().ToLower() + "</isInCancelationPeriod>";
            }
            if(IsSubscriptionOnly.HasValue)
            {
                ret += "<isSubscriptionOnly>" + IsSubscriptionOnly.ToString().ToLower() + "</isSubscriptionOnly>";
            }
            if(PPVDescriptions != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", PPVDescriptions.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<ppvDescriptions>" + propertyValue + "</ppvDescriptions>";
            }
            if(PPVModuleId != null)
            {
                ret += "<ppvModuleId>" + EscapeXml(PPVModuleId) + "</ppvModuleId>";
            }
            if(PrePaidId != null)
            {
                ret += "<prePaidId>" + EscapeXml(PrePaidId) + "</prePaidId>";
            }
            if(ProductCode != null)
            {
                ret += "<ppvProductCode>" + EscapeXml(ProductCode) + "</ppvProductCode>";
            }
            if(PurchasedMediaFileId.HasValue)
            {
                ret += "<purchasedMediaFileId>" + PurchasedMediaFileId + "</purchasedMediaFileId>";
            }
            if(PurchaseUserId != null)
            {
                ret += "<purchaseUserId>" + EscapeXml(PurchaseUserId) + "</purchaseUserId>";
            }
            if(RelatedMediaFileIds != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", RelatedMediaFileIds.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<relatedMediaFileIds>" + propertyValue + "</relatedMediaFileIds>";
            }
            if(StartDate.HasValue)
            {
                ret += "<startDate>" + StartDate + "</startDate>";
            }
            if(SubscriptionId != null)
            {
                ret += "<subscriptionId>" + EscapeXml(SubscriptionId) + "</subscriptionId>";
            }
            return ret;
        }
    }
    public partial class KalturaPreviewModule
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(LifeCycle.HasValue)
            {
                ret.Add("\"lifeCycle\": " + LifeCycle);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"life_cycle\": " + LifeCycle);
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(NonRenewablePeriod.HasValue)
            {
                ret.Add("\"nonRenewablePeriod\": " + NonRenewablePeriod);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"non_renewable_period\": " + NonRenewablePeriod);
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(LifeCycle.HasValue)
            {
                ret += "<lifeCycle>" + LifeCycle + "</lifeCycle>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<life_cycle>" + LifeCycle + "</life_cycle>";
                }
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(NonRenewablePeriod.HasValue)
            {
                ret += "<nonRenewablePeriod>" + NonRenewablePeriod + "</nonRenewablePeriod>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<non_renewable_period>" + NonRenewablePeriod + "</non_renewable_period>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaPrice
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Amount.HasValue)
            {
                ret.Add("\"amount\": " + Amount);
            }
            if(CountryId.HasValue)
            {
                ret.Add("\"countryId\": " + CountryId);
            }
            if(Currency != null)
            {
                ret.Add("\"currency\": " + "\"" + EscapeJson(Currency) + "\"");
            }
            if(CurrencySign != null)
            {
                ret.Add("\"currencySign\": " + "\"" + EscapeJson(CurrencySign) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"currency_sign\": " + "\"" + EscapeJson(CurrencySign) + "\"");
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Amount.HasValue)
            {
                ret += "<amount>" + Amount + "</amount>";
            }
            if(CountryId.HasValue)
            {
                ret += "<countryId>" + CountryId + "</countryId>";
            }
            if(Currency != null)
            {
                ret += "<currency>" + EscapeXml(Currency) + "</currency>";
            }
            if(CurrencySign != null)
            {
                ret += "<currencySign>" + EscapeXml(CurrencySign) + "</currencySign>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<currency_sign>" + EscapeXml(CurrencySign) + "</currency_sign>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaPriceDetails
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Descriptions != null)
            {
                propertyValue = "[" + String.Join(", ", Descriptions.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"descriptions\": " + propertyValue);
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(MultiCurrencyPrice != null)
            {
                propertyValue = "[" + String.Join(", ", MultiCurrencyPrice.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"multiCurrencyPrice\": " + propertyValue);
            }
            if(name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(name) + "\"");
            }
            if(Price != null)
            {
                propertyValue = Price.ToJson(currentVersion, omitObsolete);
                ret.Add("\"price\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Descriptions != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Descriptions.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<descriptions>" + propertyValue + "</descriptions>";
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(MultiCurrencyPrice != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", MultiCurrencyPrice.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<multiCurrencyPrice>" + propertyValue + "</multiCurrencyPrice>";
            }
            if(name != null)
            {
                ret += "<name>" + EscapeXml(name) + "</name>";
            }
            if(Price != null)
            {
                propertyValue = Price.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<price>" + propertyValue + "</price>";
            }
            return ret;
        }
    }
    public partial class KalturaPriceDetailsFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(IdIn != null)
            {
                ret.Add("\"idIn\": " + "\"" + EscapeJson(IdIn) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(IdIn != null)
            {
                ret += "<idIn>" + EscapeXml(IdIn) + "</idIn>";
            }
            return ret;
        }
    }
    public partial class KalturaPriceDetailsListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Prices != null)
            {
                propertyValue = "[" + String.Join(", ", Prices.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Prices != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Prices.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaPricePlan
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(DiscountId.HasValue)
            {
                ret.Add("\"discountId\": " + DiscountId);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"discount_id\": " + DiscountId);
                }
            }
            if(IsRenewable.HasValue)
            {
                ret.Add("\"isRenewable\": " + IsRenewable.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"is_renewable\": " + IsRenewable.ToString().ToLower());
                }
            }
            if(PriceDetailsId.HasValue)
            {
                ret.Add("\"priceDetailsId\": " + PriceDetailsId);
            }
            if(!DeprecatedAttribute.IsDeprecated("4.5.0.0", currentVersion) && PriceId.HasValue)
            {
                ret.Add("\"priceId\": " + PriceId);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"price_id\": " + PriceId);
                }
            }
            if(RenewalsNumber.HasValue)
            {
                ret.Add("\"renewalsNumber\": " + RenewalsNumber);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"renewals_number\": " + RenewalsNumber);
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(DiscountId.HasValue)
            {
                ret += "<discountId>" + DiscountId + "</discountId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<discount_id>" + DiscountId + "</discount_id>";
                }
            }
            if(IsRenewable.HasValue)
            {
                ret += "<isRenewable>" + IsRenewable.ToString().ToLower() + "</isRenewable>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<is_renewable>" + IsRenewable.ToString().ToLower() + "</is_renewable>";
                }
            }
            if(PriceDetailsId.HasValue)
            {
                ret += "<priceDetailsId>" + PriceDetailsId + "</priceDetailsId>";
            }
            if(!DeprecatedAttribute.IsDeprecated("4.5.0.0", currentVersion) && PriceId.HasValue)
            {
                ret += "<priceId>" + PriceId + "</priceId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<price_id>" + PriceId + "</price_id>";
                }
            }
            if(RenewalsNumber.HasValue)
            {
                ret += "<renewalsNumber>" + RenewalsNumber + "</renewalsNumber>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<renewals_number>" + RenewalsNumber + "</renewals_number>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaPricePlanFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(IdIn != null)
            {
                ret.Add("\"idIn\": " + "\"" + EscapeJson(IdIn) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(IdIn != null)
            {
                ret += "<idIn>" + EscapeXml(IdIn) + "</idIn>";
            }
            return ret;
        }
    }
    public partial class KalturaPricePlanListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(PricePlans != null)
            {
                propertyValue = "[" + String.Join(", ", PricePlans.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(PricePlans != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", PricePlans.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaProductCode
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Code != null)
            {
                ret.Add("\"code\": " + "\"" + EscapeJson(Code) + "\"");
            }
            if(InappProvider != null)
            {
                ret.Add("\"inappProvider\": " + "\"" + EscapeJson(InappProvider) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Code != null)
            {
                ret += "<code>" + EscapeXml(Code) + "</code>";
            }
            if(InappProvider != null)
            {
                ret += "<inappProvider>" + EscapeXml(InappProvider) + "</inappProvider>";
            }
            return ret;
        }
    }
    public partial class KalturaProductPrice
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(!isOldVersion && Price != null)
            {
                propertyValue = Price.ToJson(currentVersion, omitObsolete);
                ret.Add("\"price\": " + propertyValue);
            }
            if(ProductId != null)
            {
                ret.Add("\"productId\": " + "\"" + EscapeJson(ProductId) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"product_id\": " + "\"" + EscapeJson(ProductId) + "\"");
                }
            }
            ret.Add("\"productType\": " + "\"" + Enum.GetName(typeof(KalturaTransactionType), ProductType) + "\"");
            if (currentVersion == null || isOldVersion)
            {
                ret.Add("\"product_type\": " + "\"" + Enum.GetName(typeof(KalturaTransactionType), ProductType) + "\"");
            }
            if(!isOldVersion)
            {
                ret.Add("\"purchaseStatus\": " + "\"" + Enum.GetName(typeof(KalturaPurchaseStatus), PurchaseStatus) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(!isOldVersion && Price != null)
            {
                propertyValue = Price.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<price>" + propertyValue + "</price>";
            }
            if(ProductId != null)
            {
                ret += "<productId>" + EscapeXml(ProductId) + "</productId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<product_id>" + EscapeXml(ProductId) + "</product_id>";
                }
            }
            ret += "<productType>" + "\"" + Enum.GetName(typeof(KalturaTransactionType), ProductType) + "\"" + "</productType>";
            if (currentVersion == null || isOldVersion)
            {
            ret += "<product_type>" + "\"" + Enum.GetName(typeof(KalturaTransactionType), ProductType) + "\"" + "</product_type>";
            }
            if(!isOldVersion)
            {
                ret += "<purchaseStatus>" + "\"" + Enum.GetName(typeof(KalturaPurchaseStatus), PurchaseStatus) + "\"" + "</purchaseStatus>";
            }
            return ret;
        }
    }
    public partial class KalturaProductPriceListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ProductsPrices != null)
            {
                propertyValue = "[" + String.Join(", ", ProductsPrices.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ProductsPrices != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", ProductsPrices.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaProductsPriceListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ProductsPrices != null)
            {
                propertyValue = "[" + String.Join(", ", ProductsPrices.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ProductsPrices != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", ProductsPrices.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaPublicCouponGenerationOptions
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Code != null)
            {
                ret.Add("\"code\": " + "\"" + EscapeJson(Code) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Code != null)
            {
                ret += "<code>" + EscapeXml(Code) + "</code>";
            }
            return ret;
        }
    }
    public partial class KalturaRandomCouponGenerationOptions
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"numberOfCoupons\": " + NumberOfCoupons);
            if(UseLetters.HasValue)
            {
                ret.Add("\"useLetters\": " + UseLetters.ToString().ToLower());
            }
            if(UseNumbers.HasValue)
            {
                ret.Add("\"useNumbers\": " + UseNumbers.ToString().ToLower());
            }
            if(UseSpecialCharacters.HasValue)
            {
                ret.Add("\"useSpecialCharacters\": " + UseSpecialCharacters.ToString().ToLower());
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<numberOfCoupons>" + NumberOfCoupons + "</numberOfCoupons>";
            if(UseLetters.HasValue)
            {
                ret += "<useLetters>" + UseLetters.ToString().ToLower() + "</useLetters>";
            }
            if(UseNumbers.HasValue)
            {
                ret += "<useNumbers>" + UseNumbers.ToString().ToLower() + "</useNumbers>";
            }
            if(UseSpecialCharacters.HasValue)
            {
                ret += "<useSpecialCharacters>" + UseSpecialCharacters.ToString().ToLower() + "</useSpecialCharacters>";
            }
            return ret;
        }
    }
    public partial class KalturaSubscription
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Channels != null)
            {
                propertyValue = "[" + String.Join(", ", Channels.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"channels\": " + propertyValue);
            }
            if(CouponGroups != null)
            {
                propertyValue = "[" + String.Join(", ", CouponGroups.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"couponsGroups\": " + propertyValue);
            }
            if(!DeprecatedAttribute.IsDeprecated("4.3.0.0", currentVersion) && CouponsGroup != null)
            {
                propertyValue = CouponsGroup.ToJson(currentVersion, omitObsolete);
                ret.Add("\"couponsGroup\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"coupons_group\": " + propertyValue);
                }
            }
            ret.Add("\"dependencyType\": " + "\"" + Enum.GetName(typeof(KalturaSubscriptionDependencyType), DependencyType) + "\"");
            ret.Add(Description.ToCustomJson(currentVersion, omitObsolete, "description"));
            if(!DeprecatedAttribute.IsDeprecated("3.6.287.27312", currentVersion) && Descriptions != null)
            {
                propertyValue = "[" + String.Join(", ", Descriptions.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"descriptions\": " + propertyValue);
            }
            if(DiscountModule != null)
            {
                propertyValue = DiscountModule.ToJson(currentVersion, omitObsolete);
                ret.Add("\"discountModule\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"discount_module\": " + propertyValue);
                }
            }
            if(EndDate.HasValue)
            {
                ret.Add("\"endDate\": " + EndDate);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"end_date\": " + EndDate);
                }
            }
            if(ExternalId != null)
            {
                ret.Add("\"externalId\": " + "\"" + EscapeJson(ExternalId) + "\"");
            }
            if(FileTypes != null)
            {
                propertyValue = "[" + String.Join(", ", FileTypes.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"fileTypes\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"file_types\": " + propertyValue);
                }
            }
            if(GracePeriodMinutes.HasValue)
            {
                ret.Add("\"gracePeriodMinutes\": " + GracePeriodMinutes);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"grace_period_minutes\": " + GracePeriodMinutes);
                }
            }
            if(HouseholdLimitationsId.HasValue)
            {
                ret.Add("\"householdLimitationsId\": " + HouseholdLimitationsId);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"household_limitations_id\": " + HouseholdLimitationsId);
                }
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + EscapeJson(Id) + "\"");
            }
            ret.Add("\"isCancellationBlocked\": " + IsCancellationBlocked.ToString().ToLower());
            if(IsInfiniteRenewal.HasValue)
            {
                ret.Add("\"isInfiniteRenewal\": " + IsInfiniteRenewal.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"is_infinite_renewal\": " + IsInfiniteRenewal.ToString().ToLower());
                }
            }
            if(IsRenewable.HasValue)
            {
                ret.Add("\"isRenewable\": " + IsRenewable.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"is_renewable\": " + IsRenewable.ToString().ToLower());
                }
            }
            if(IsWaiverEnabled.HasValue)
            {
                ret.Add("\"isWaiverEnabled\": " + IsWaiverEnabled.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"is_waiver_enabled\": " + IsWaiverEnabled.ToString().ToLower());
                }
            }
            if(MaxViewsNumber.HasValue)
            {
                ret.Add("\"maxViewsNumber\": " + MaxViewsNumber);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"max_views_number\": " + MaxViewsNumber);
                }
            }
            if(MediaId.HasValue)
            {
                ret.Add("\"mediaId\": " + MediaId);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"media_id\": " + MediaId);
                }
            }
            ret.Add(Name.ToCustomJson(currentVersion, omitObsolete, "name"));
            if(!DeprecatedAttribute.IsDeprecated("3.6.287.27312", currentVersion) && Names != null)
            {
                propertyValue = "[" + String.Join(", ", Names.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"names\": " + propertyValue);
            }
            if(PremiumServices != null)
            {
                propertyValue = "[" + String.Join(", ", PremiumServices.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"premiumServices\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"premium_services\": " + propertyValue);
                }
            }
            if(PreviewModule != null)
            {
                propertyValue = PreviewModule.ToJson(currentVersion, omitObsolete);
                ret.Add("\"previewModule\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"preview_module\": " + propertyValue);
                }
            }
            if(Price != null)
            {
                propertyValue = Price.ToJson(currentVersion, omitObsolete);
                ret.Add("\"price\": " + propertyValue);
            }
            if(PricePlanIds != null)
            {
                ret.Add("\"pricePlanIds\": " + "\"" + EscapeJson(PricePlanIds) + "\"");
            }
            if(!DeprecatedAttribute.IsDeprecated("4.5.0.0", currentVersion) && PricePlans != null)
            {
                propertyValue = "[" + String.Join(", ", PricePlans.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"pricePlans\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"price_plans\": " + propertyValue);
                }
            }
            if(!DeprecatedAttribute.IsDeprecated("4.3.0.0", currentVersion) && ProductCode != null)
            {
                ret.Add("\"productCode\": " + "\"" + EscapeJson(ProductCode) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"product_code\": " + "\"" + EscapeJson(ProductCode) + "\"");
                }
            }
            if(ProductCodes != null)
            {
                propertyValue = "[" + String.Join(", ", ProductCodes.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"productCodes\": " + propertyValue);
            }
            if(ProrityInOrder.HasValue)
            {
                ret.Add("\"prorityInOrder\": " + ProrityInOrder);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"prority_in_order\": " + ProrityInOrder);
                }
            }
            if(RenewalsNumber.HasValue)
            {
                ret.Add("\"renewalsNumber\": " + RenewalsNumber);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"renewals_number\": " + RenewalsNumber);
                }
            }
            if(StartDate.HasValue)
            {
                ret.Add("\"startDate\": " + StartDate);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"start_date\": " + StartDate);
                }
            }
            if(UserTypes != null)
            {
                propertyValue = "[" + String.Join(", ", UserTypes.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"userTypes\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"user_types\": " + propertyValue);
                }
            }
            if(ViewLifeCycle.HasValue)
            {
                ret.Add("\"viewLifeCycle\": " + ViewLifeCycle);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"view_life_cycle\": " + ViewLifeCycle);
                }
            }
            if(WaiverPeriod.HasValue)
            {
                ret.Add("\"waiverPeriod\": " + WaiverPeriod);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"waiver_period\": " + WaiverPeriod);
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Channels != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Channels.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<channels>" + propertyValue + "</channels>";
            }
            if(CouponGroups != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", CouponGroups.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<couponsGroups>" + propertyValue + "</couponsGroups>";
            }
            if(!DeprecatedAttribute.IsDeprecated("4.3.0.0", currentVersion) && CouponsGroup != null)
            {
                propertyValue = CouponsGroup.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<couponsGroup>" + propertyValue + "</couponsGroup>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<coupons_group>" + propertyValue + "</coupons_group>";
                }
            }
            ret += "<dependencyType>" + "\"" + Enum.GetName(typeof(KalturaSubscriptionDependencyType), DependencyType) + "\"" + "</dependencyType>";
            ret += Description.ToCustomXml(currentVersion, omitObsolete, "description");
            if(!DeprecatedAttribute.IsDeprecated("3.6.287.27312", currentVersion) && Descriptions != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Descriptions.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<descriptions>" + propertyValue + "</descriptions>";
            }
            if(DiscountModule != null)
            {
                propertyValue = DiscountModule.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<discountModule>" + propertyValue + "</discountModule>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<discount_module>" + propertyValue + "</discount_module>";
                }
            }
            if(EndDate.HasValue)
            {
                ret += "<endDate>" + EndDate + "</endDate>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<end_date>" + EndDate + "</end_date>";
                }
            }
            if(ExternalId != null)
            {
                ret += "<externalId>" + EscapeXml(ExternalId) + "</externalId>";
            }
            if(FileTypes != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", FileTypes.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<fileTypes>" + propertyValue + "</fileTypes>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<file_types>" + propertyValue + "</file_types>";
                }
            }
            if(GracePeriodMinutes.HasValue)
            {
                ret += "<gracePeriodMinutes>" + GracePeriodMinutes + "</gracePeriodMinutes>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<grace_period_minutes>" + GracePeriodMinutes + "</grace_period_minutes>";
                }
            }
            if(HouseholdLimitationsId.HasValue)
            {
                ret += "<householdLimitationsId>" + HouseholdLimitationsId + "</householdLimitationsId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<household_limitations_id>" + HouseholdLimitationsId + "</household_limitations_id>";
                }
            }
            if(Id != null)
            {
                ret += "<id>" + EscapeXml(Id) + "</id>";
            }
            ret += "<isCancellationBlocked>" + IsCancellationBlocked.ToString().ToLower() + "</isCancellationBlocked>";
            if(IsInfiniteRenewal.HasValue)
            {
                ret += "<isInfiniteRenewal>" + IsInfiniteRenewal.ToString().ToLower() + "</isInfiniteRenewal>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<is_infinite_renewal>" + IsInfiniteRenewal.ToString().ToLower() + "</is_infinite_renewal>";
                }
            }
            if(IsRenewable.HasValue)
            {
                ret += "<isRenewable>" + IsRenewable.ToString().ToLower() + "</isRenewable>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<is_renewable>" + IsRenewable.ToString().ToLower() + "</is_renewable>";
                }
            }
            if(IsWaiverEnabled.HasValue)
            {
                ret += "<isWaiverEnabled>" + IsWaiverEnabled.ToString().ToLower() + "</isWaiverEnabled>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<is_waiver_enabled>" + IsWaiverEnabled.ToString().ToLower() + "</is_waiver_enabled>";
                }
            }
            if(MaxViewsNumber.HasValue)
            {
                ret += "<maxViewsNumber>" + MaxViewsNumber + "</maxViewsNumber>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<max_views_number>" + MaxViewsNumber + "</max_views_number>";
                }
            }
            if(MediaId.HasValue)
            {
                ret += "<mediaId>" + MediaId + "</mediaId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<media_id>" + MediaId + "</media_id>";
                }
            }
            ret += Name.ToCustomXml(currentVersion, omitObsolete, "name");
            if(!DeprecatedAttribute.IsDeprecated("3.6.287.27312", currentVersion) && Names != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Names.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<names>" + propertyValue + "</names>";
            }
            if(PremiumServices != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", PremiumServices.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<premiumServices>" + propertyValue + "</premiumServices>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<premium_services>" + propertyValue + "</premium_services>";
                }
            }
            if(PreviewModule != null)
            {
                propertyValue = PreviewModule.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<previewModule>" + propertyValue + "</previewModule>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<preview_module>" + propertyValue + "</preview_module>";
                }
            }
            if(Price != null)
            {
                propertyValue = Price.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<price>" + propertyValue + "</price>";
            }
            if(PricePlanIds != null)
            {
                ret += "<pricePlanIds>" + EscapeXml(PricePlanIds) + "</pricePlanIds>";
            }
            if(!DeprecatedAttribute.IsDeprecated("4.5.0.0", currentVersion) && PricePlans != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", PricePlans.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<pricePlans>" + propertyValue + "</pricePlans>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<price_plans>" + propertyValue + "</price_plans>";
                }
            }
            if(!DeprecatedAttribute.IsDeprecated("4.3.0.0", currentVersion) && ProductCode != null)
            {
                ret += "<productCode>" + EscapeXml(ProductCode) + "</productCode>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<product_code>" + EscapeXml(ProductCode) + "</product_code>";
                }
            }
            if(ProductCodes != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", ProductCodes.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<productCodes>" + propertyValue + "</productCodes>";
            }
            if(ProrityInOrder.HasValue)
            {
                ret += "<prorityInOrder>" + ProrityInOrder + "</prorityInOrder>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<prority_in_order>" + ProrityInOrder + "</prority_in_order>";
                }
            }
            if(RenewalsNumber.HasValue)
            {
                ret += "<renewalsNumber>" + RenewalsNumber + "</renewalsNumber>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<renewals_number>" + RenewalsNumber + "</renewals_number>";
                }
            }
            if(StartDate.HasValue)
            {
                ret += "<startDate>" + StartDate + "</startDate>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<start_date>" + StartDate + "</start_date>";
                }
            }
            if(UserTypes != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", UserTypes.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<userTypes>" + propertyValue + "</userTypes>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<user_types>" + propertyValue + "</user_types>";
                }
            }
            if(ViewLifeCycle.HasValue)
            {
                ret += "<viewLifeCycle>" + ViewLifeCycle + "</viewLifeCycle>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<view_life_cycle>" + ViewLifeCycle + "</view_life_cycle>";
                }
            }
            if(WaiverPeriod.HasValue)
            {
                ret += "<waiverPeriod>" + WaiverPeriod + "</waiverPeriod>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<waiver_period>" + WaiverPeriod + "</waiver_period>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaSubscriptionDependencySet
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(BaseSubscriptionId.HasValue)
            {
                ret.Add("\"baseSubscriptionId\": " + BaseSubscriptionId);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(BaseSubscriptionId.HasValue)
            {
                ret += "<baseSubscriptionId>" + BaseSubscriptionId + "</baseSubscriptionId>";
            }
            return ret;
        }
    }
    public partial class KalturaSubscriptionDependencySetFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(BaseSubscriptionIdIn != null)
            {
                ret.Add("\"baseSubscriptionIdIn\": " + "\"" + EscapeJson(BaseSubscriptionIdIn) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(BaseSubscriptionIdIn != null)
            {
                ret += "<baseSubscriptionIdIn>" + EscapeXml(BaseSubscriptionIdIn) + "</baseSubscriptionIdIn>";
            }
            return ret;
        }
    }
    public partial class KalturaSubscriptionFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ExternalIdIn != null)
            {
                ret.Add("\"externalIdIn\": " + "\"" + EscapeJson(ExternalIdIn) + "\"");
            }
            if(MediaFileIdEqual.HasValue)
            {
                ret.Add("\"mediaFileIdEqual\": " + MediaFileIdEqual);
            }
            if(SubscriptionIdIn != null)
            {
                ret.Add("\"subscriptionIdIn\": " + "\"" + EscapeJson(SubscriptionIdIn) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ExternalIdIn != null)
            {
                ret += "<externalIdIn>" + EscapeXml(ExternalIdIn) + "</externalIdIn>";
            }
            if(MediaFileIdEqual.HasValue)
            {
                ret += "<mediaFileIdEqual>" + MediaFileIdEqual + "</mediaFileIdEqual>";
            }
            if(SubscriptionIdIn != null)
            {
                ret += "<subscriptionIdIn>" + EscapeXml(SubscriptionIdIn) + "</subscriptionIdIn>";
            }
            return ret;
        }
    }
    public partial class KalturaSubscriptionListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Subscriptions != null)
            {
                propertyValue = "[" + String.Join(", ", Subscriptions.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Subscriptions != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Subscriptions.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaSubscriptionPrice
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(EndDate.HasValue)
            {
                ret.Add("\"endDate\": " + EndDate);
            }
            if(!omitObsolete && Price != null)
            {
                propertyValue = Price.ToJson(currentVersion, omitObsolete);
                ret.Add("\"price\": " + propertyValue);
            }
            if(!omitObsolete)
            {
                ret.Add("\"purchaseStatus\": " + "\"" + Enum.GetName(typeof(KalturaPurchaseStatus), PurchaseStatus) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"purchase_status\": " + "\"" + Enum.GetName(typeof(KalturaPurchaseStatus), PurchaseStatus) + "\"");
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(EndDate.HasValue)
            {
                ret += "<endDate>" + EndDate + "</endDate>";
            }
            if(!omitObsolete && Price != null)
            {
                propertyValue = Price.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<price>" + propertyValue + "</price>";
            }
            if(!omitObsolete)
            {
                ret += "<purchaseStatus>" + "\"" + Enum.GetName(typeof(KalturaPurchaseStatus), PurchaseStatus) + "\"" + "</purchaseStatus>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<purchase_status>" + "\"" + Enum.GetName(typeof(KalturaPurchaseStatus), PurchaseStatus) + "\"" + "</purchase_status>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaSubscriptionSet
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"id\": " + Id);
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(SubscriptionIds != null)
            {
                ret.Add("\"subscriptionIds\": " + "\"" + EscapeJson(SubscriptionIds) + "\"");
            }
            if(Type.HasValue)
            {
                ret.Add("\"type\": " + "\"" + Enum.GetName(typeof(KalturaSubscriptionSetType), Type) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<id>" + Id + "</id>";
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(SubscriptionIds != null)
            {
                ret += "<subscriptionIds>" + EscapeXml(SubscriptionIds) + "</subscriptionIds>";
            }
            if(Type.HasValue)
            {
                ret += "<type>" + "\"" + Enum.GetName(typeof(KalturaSubscriptionSetType), Type) + "\"" + "</type>";
            }
            return ret;
        }
    }
    public partial class KalturaSubscriptionSetFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(IdIn != null)
            {
                ret.Add("\"idIn\": " + "\"" + EscapeJson(IdIn) + "\"");
            }
            if(SubscriptionIdContains != null)
            {
                ret.Add("\"subscriptionIdContains\": " + "\"" + EscapeJson(SubscriptionIdContains) + "\"");
            }
            if(TypeEqual.HasValue)
            {
                ret.Add("\"typeEqual\": " + "\"" + Enum.GetName(typeof(KalturaSubscriptionSetType), TypeEqual) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(IdIn != null)
            {
                ret += "<idIn>" + EscapeXml(IdIn) + "</idIn>";
            }
            if(SubscriptionIdContains != null)
            {
                ret += "<subscriptionIdContains>" + EscapeXml(SubscriptionIdContains) + "</subscriptionIdContains>";
            }
            if(TypeEqual.HasValue)
            {
                ret += "<typeEqual>" + "\"" + Enum.GetName(typeof(KalturaSubscriptionSetType), TypeEqual) + "\"" + "</typeEqual>";
            }
            return ret;
        }
    }
    public partial class KalturaSubscriptionSetListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(SubscriptionSets != null)
            {
                propertyValue = "[" + String.Join(", ", SubscriptionSets.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(SubscriptionSets != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", SubscriptionSets.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaSubscriptionsFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"by\": " + "\"" + Enum.GetName(typeof(KalturaSubscriptionsFilterBy), By) + "\"");
            if(Ids != null)
            {
                propertyValue = "[" + String.Join(", ", Ids.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"ids\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<by>" + "\"" + Enum.GetName(typeof(KalturaSubscriptionsFilterBy), By) + "\"" + "</by>";
            if(Ids != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Ids.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<ids>" + propertyValue + "</ids>";
            }
            return ret;
        }
    }
    public partial class KalturaSubscriptionSwitchSet
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaUsageModule
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(CouponId.HasValue)
            {
                ret.Add("\"couponId\": " + CouponId);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"coupon_id\": " + CouponId);
                }
            }
            if(FullLifeCycle.HasValue)
            {
                ret.Add("\"fullLifeCycle\": " + FullLifeCycle);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"full_life_cycle\": " + FullLifeCycle);
                }
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(IsOfflinePlayback.HasValue)
            {
                ret.Add("\"isOfflinePlayback\": " + IsOfflinePlayback.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"is_offline_playback\": " + IsOfflinePlayback.ToString().ToLower());
                }
            }
            if(IsWaiverEnabled.HasValue)
            {
                ret.Add("\"isWaiverEnabled\": " + IsWaiverEnabled.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"is_waiver_enabled\": " + IsWaiverEnabled.ToString().ToLower());
                }
            }
            if(MaxViewsNumber.HasValue)
            {
                ret.Add("\"maxViewsNumber\": " + MaxViewsNumber);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"max_views_number\": " + MaxViewsNumber);
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(ViewLifeCycle.HasValue)
            {
                ret.Add("\"viewLifeCycle\": " + ViewLifeCycle);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"view_life_cycle\": " + ViewLifeCycle);
                }
            }
            if(WaiverPeriod.HasValue)
            {
                ret.Add("\"waiverPeriod\": " + WaiverPeriod);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"waiver_period\": " + WaiverPeriod);
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(CouponId.HasValue)
            {
                ret += "<couponId>" + CouponId + "</couponId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<coupon_id>" + CouponId + "</coupon_id>";
                }
            }
            if(FullLifeCycle.HasValue)
            {
                ret += "<fullLifeCycle>" + FullLifeCycle + "</fullLifeCycle>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<full_life_cycle>" + FullLifeCycle + "</full_life_cycle>";
                }
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(IsOfflinePlayback.HasValue)
            {
                ret += "<isOfflinePlayback>" + IsOfflinePlayback.ToString().ToLower() + "</isOfflinePlayback>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<is_offline_playback>" + IsOfflinePlayback.ToString().ToLower() + "</is_offline_playback>";
                }
            }
            if(IsWaiverEnabled.HasValue)
            {
                ret += "<isWaiverEnabled>" + IsWaiverEnabled.ToString().ToLower() + "</isWaiverEnabled>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<is_waiver_enabled>" + IsWaiverEnabled.ToString().ToLower() + "</is_waiver_enabled>";
                }
            }
            if(MaxViewsNumber.HasValue)
            {
                ret += "<maxViewsNumber>" + MaxViewsNumber + "</maxViewsNumber>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<max_views_number>" + MaxViewsNumber + "</max_views_number>";
                }
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(ViewLifeCycle.HasValue)
            {
                ret += "<viewLifeCycle>" + ViewLifeCycle + "</viewLifeCycle>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<view_life_cycle>" + ViewLifeCycle + "</view_life_cycle>";
                }
            }
            if(WaiverPeriod.HasValue)
            {
                ret += "<waiverPeriod>" + WaiverPeriod + "</waiverPeriod>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<waiver_period>" + WaiverPeriod + "</waiver_period>";
                }
            }
            return ret;
        }
    }
}

namespace WebAPI.Models.Users
{
    public partial class KalturaBaseOTTUser
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(FirstName != null)
            {
                ret.Add("\"firstName\": " + "\"" + EscapeJson(FirstName) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"first_name\": " + "\"" + EscapeJson(FirstName) + "\"");
                }
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + EscapeJson(Id) + "\"");
            }
            if(LastName != null)
            {
                ret.Add("\"lastName\": " + "\"" + EscapeJson(LastName) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"last_name\": " + "\"" + EscapeJson(LastName) + "\"");
                }
            }
            if(Username != null)
            {
                ret.Add("\"username\": " + "\"" + EscapeJson(Username) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(FirstName != null)
            {
                ret += "<firstName>" + EscapeXml(FirstName) + "</firstName>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<first_name>" + EscapeXml(FirstName) + "</first_name>";
                }
            }
            if(Id != null)
            {
                ret += "<id>" + EscapeXml(Id) + "</id>";
            }
            if(LastName != null)
            {
                ret += "<lastName>" + EscapeXml(LastName) + "</lastName>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<last_name>" + EscapeXml(LastName) + "</last_name>";
                }
            }
            if(Username != null)
            {
                ret += "<username>" + EscapeXml(Username) + "</username>";
            }
            return ret;
        }
    }
    public partial class KalturaCountry
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Code != null)
            {
                ret.Add("\"code\": " + "\"" + EscapeJson(Code) + "\"");
            }
            if(CurrencyCode != null)
            {
                ret.Add("\"currency\": " + "\"" + EscapeJson(CurrencyCode) + "\"");
            }
            if(CurrencySign != null)
            {
                ret.Add("\"currencySign\": " + "\"" + EscapeJson(CurrencySign) + "\"");
            }
            ret.Add("\"id\": " + Id);
            if(LanguagesCode != null)
            {
                ret.Add("\"languagesCode\": " + "\"" + EscapeJson(LanguagesCode) + "\"");
            }
            if(MainLanguageCode != null)
            {
                ret.Add("\"mainLanguageCode\": " + "\"" + EscapeJson(MainLanguageCode) + "\"");
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(TimeZoneId != null)
            {
                ret.Add("\"timeZoneId\": " + "\"" + EscapeJson(TimeZoneId) + "\"");
            }
            if(VatPercent.HasValue)
            {
                ret.Add("\"vatPercent\": " + VatPercent);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Code != null)
            {
                ret += "<code>" + EscapeXml(Code) + "</code>";
            }
            if(CurrencyCode != null)
            {
                ret += "<currency>" + EscapeXml(CurrencyCode) + "</currency>";
            }
            if(CurrencySign != null)
            {
                ret += "<currencySign>" + EscapeXml(CurrencySign) + "</currencySign>";
            }
            ret += "<id>" + Id + "</id>";
            if(LanguagesCode != null)
            {
                ret += "<languagesCode>" + EscapeXml(LanguagesCode) + "</languagesCode>";
            }
            if(MainLanguageCode != null)
            {
                ret += "<mainLanguageCode>" + EscapeXml(MainLanguageCode) + "</mainLanguageCode>";
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(TimeZoneId != null)
            {
                ret += "<timeZoneId>" + EscapeXml(TimeZoneId) + "</timeZoneId>";
            }
            if(VatPercent.HasValue)
            {
                ret += "<vatPercent>" + VatPercent + "</vatPercent>";
            }
            return ret;
        }
    }
    public partial class KalturaFavorite
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(!omitObsolete && Asset != null)
            {
                propertyValue = Asset.ToJson(currentVersion, omitObsolete);
                ret.Add("\"asset\": " + propertyValue);
            }
            ret.Add("\"assetId\": " + AssetId);
            ret.Add("\"createDate\": " + CreateDate);
            if(ExtraData != null)
            {
                ret.Add("\"extraData\": " + "\"" + EscapeJson(ExtraData) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"extra_data\": " + "\"" + EscapeJson(ExtraData) + "\"");
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(!omitObsolete && Asset != null)
            {
                propertyValue = Asset.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<asset>" + propertyValue + "</asset>";
            }
            ret += "<assetId>" + AssetId + "</assetId>";
            ret += "<createDate>" + CreateDate + "</createDate>";
            if(ExtraData != null)
            {
                ret += "<extraData>" + EscapeXml(ExtraData) + "</extraData>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<extra_data>" + EscapeXml(ExtraData) + "</extra_data>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaFavoriteFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(MediaIdIn != null)
            {
                ret.Add("\"mediaIdIn\": " + "\"" + EscapeJson(MediaIdIn) + "\"");
            }
            if(!omitObsolete && MediaIds != null)
            {
                propertyValue = "[" + String.Join(", ", MediaIds.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"media_ids\": " + propertyValue);
            }
            if(MediaTypeEqual.HasValue)
            {
                ret.Add("\"mediaTypeEqual\": " + MediaTypeEqual);
            }
            if(!omitObsolete && MediaTypeIn.HasValue)
            {
                ret.Add("\"mediaTypeIn\": " + MediaTypeIn);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"media_type\": " + MediaTypeIn);
                }
            }
            if(!omitObsolete && UDID != null)
            {
                ret.Add("\"udid\": " + "\"" + EscapeJson(UDID) + "\"");
            }
            if(UdidEqualCurrent.HasValue)
            {
                ret.Add("\"udidEqualCurrent\": " + UdidEqualCurrent.ToString().ToLower());
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(MediaIdIn != null)
            {
                ret += "<mediaIdIn>" + EscapeXml(MediaIdIn) + "</mediaIdIn>";
            }
            if(!omitObsolete && MediaIds != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", MediaIds.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<media_ids>" + propertyValue + "</media_ids>";
            }
            if(MediaTypeEqual.HasValue)
            {
                ret += "<mediaTypeEqual>" + MediaTypeEqual + "</mediaTypeEqual>";
            }
            if(!omitObsolete && MediaTypeIn.HasValue)
            {
                ret += "<mediaTypeIn>" + MediaTypeIn + "</mediaTypeIn>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<media_type>" + MediaTypeIn + "</media_type>";
                }
            }
            if(!omitObsolete && UDID != null)
            {
                ret += "<udid>" + EscapeXml(UDID) + "</udid>";
            }
            if(UdidEqualCurrent.HasValue)
            {
                ret += "<udidEqualCurrent>" + UdidEqualCurrent.ToString().ToLower() + "</udidEqualCurrent>";
            }
            return ret;
        }
    }
    public partial class KalturaFavoriteListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Favorites != null)
            {
                propertyValue = "[" + String.Join(", ", Favorites.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Favorites != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Favorites.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaLoginResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(LoginSession != null)
            {
                propertyValue = LoginSession.ToJson(currentVersion, omitObsolete);
                ret.Add("\"loginSession\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"login_session\": " + propertyValue);
                }
            }
            if(User != null)
            {
                propertyValue = User.ToJson(currentVersion, omitObsolete);
                ret.Add("\"user\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(LoginSession != null)
            {
                propertyValue = LoginSession.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<loginSession>" + propertyValue + "</loginSession>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<login_session>" + propertyValue + "</login_session>";
                }
            }
            if(User != null)
            {
                propertyValue = User.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<user>" + propertyValue + "</user>";
            }
            return ret;
        }
    }
    public partial class KalturaLoginSession
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(KS != null)
            {
                ret.Add("\"ks\": " + "\"" + EscapeJson(KS) + "\"");
            }
            if(!omitObsolete && RefreshToken != null)
            {
                ret.Add("\"refreshToken\": " + "\"" + EscapeJson(RefreshToken) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"refresh_token\": " + "\"" + EscapeJson(RefreshToken) + "\"");
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(KS != null)
            {
                ret += "<ks>" + EscapeXml(KS) + "</ks>";
            }
            if(!omitObsolete && RefreshToken != null)
            {
                ret += "<refreshToken>" + EscapeXml(RefreshToken) + "</refreshToken>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<refresh_token>" + EscapeXml(RefreshToken) + "</refresh_token>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaOTTUser
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Address != null)
            {
                ret.Add("\"address\": " + "\"" + EscapeJson(Address) + "\"");
            }
            if(AffiliateCode != null)
            {
                ret.Add("\"affiliateCode\": " + "\"" + EscapeJson(AffiliateCode) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"affiliate_code\": " + "\"" + EscapeJson(AffiliateCode) + "\"");
                }
            }
            if(City != null)
            {
                ret.Add("\"city\": " + "\"" + EscapeJson(City) + "\"");
            }
            if(!omitObsolete && Country != null)
            {
                propertyValue = Country.ToJson(currentVersion, omitObsolete);
                ret.Add("\"country\": " + propertyValue);
            }
            if(CountryId.HasValue)
            {
                ret.Add("\"countryId\": " + CountryId);
            }
            if(DynamicData != null)
            {
                propertyValue = "{" + String.Join(", ", DynamicData.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"dynamicData\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"dynamic_data\": " + propertyValue);
                }
            }
            if(Email != null)
            {
                ret.Add("\"email\": " + "\"" + EscapeJson(Email) + "\"");
            }
            if(ExternalId != null)
            {
                ret.Add("\"externalId\": " + "\"" + EscapeJson(ExternalId) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"external_id\": " + "\"" + EscapeJson(ExternalId) + "\"");
                }
            }
            if(!omitObsolete && FacebookId != null)
            {
                ret.Add("\"facebookId\": " + "\"" + EscapeJson(FacebookId) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"facebook_id\": " + "\"" + EscapeJson(FacebookId) + "\"");
                }
            }
            if(!omitObsolete && FacebookImage != null)
            {
                ret.Add("\"facebookImage\": " + "\"" + EscapeJson(FacebookImage) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"facebook_image\": " + "\"" + EscapeJson(FacebookImage) + "\"");
                }
            }
            if(!omitObsolete && FacebookToken != null)
            {
                ret.Add("\"facebookToken\": " + "\"" + EscapeJson(FacebookToken) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"facebook_token\": " + "\"" + EscapeJson(FacebookToken) + "\"");
                }
            }
            if(HouseholdID.HasValue)
            {
                ret.Add("\"householdId\": " + HouseholdID);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"household_id\": " + HouseholdID);
                }
            }
            if(IsHouseholdMaster.HasValue)
            {
                ret.Add("\"isHouseholdMaster\": " + IsHouseholdMaster.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"is_household_master\": " + IsHouseholdMaster.ToString().ToLower());
                }
            }
            if(Phone != null)
            {
                ret.Add("\"phone\": " + "\"" + EscapeJson(Phone) + "\"");
            }
            ret.Add("\"suspensionState\": " + "\"" + Enum.GetName(typeof(KalturaHouseholdSuspensionState), SuspensionState) + "\"");
            if(!omitObsolete)
            {
                ret.Add("\"suspentionState\": " + "\"" + Enum.GetName(typeof(KalturaHouseholdSuspentionState), SuspentionState) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"suspention_state\": " + "\"" + Enum.GetName(typeof(KalturaHouseholdSuspentionState), SuspentionState) + "\"");
                }
            }
            ret.Add("\"userState\": " + "\"" + Enum.GetName(typeof(KalturaUserState), UserState) + "\"");
            if (currentVersion == null || isOldVersion)
            {
                ret.Add("\"user_state\": " + "\"" + Enum.GetName(typeof(KalturaUserState), UserState) + "\"");
            }
            if(UserType != null)
            {
                propertyValue = UserType.ToJson(currentVersion, omitObsolete);
                ret.Add("\"userType\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"user_type\": " + propertyValue);
                }
            }
            if(Zip != null)
            {
                ret.Add("\"zip\": " + "\"" + EscapeJson(Zip) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Address != null)
            {
                ret += "<address>" + EscapeXml(Address) + "</address>";
            }
            if(AffiliateCode != null)
            {
                ret += "<affiliateCode>" + EscapeXml(AffiliateCode) + "</affiliateCode>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<affiliate_code>" + EscapeXml(AffiliateCode) + "</affiliate_code>";
                }
            }
            if(City != null)
            {
                ret += "<city>" + EscapeXml(City) + "</city>";
            }
            if(!omitObsolete && Country != null)
            {
                propertyValue = Country.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<country>" + propertyValue + "</country>";
            }
            if(CountryId.HasValue)
            {
                ret += "<countryId>" + CountryId + "</countryId>";
            }
            if(DynamicData != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", DynamicData.Select(pair => "<itemKey>" + pair.Key + "</itemKey>" + pair.Value.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<dynamicData>" + propertyValue + "</dynamicData>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<dynamic_data>" + propertyValue + "</dynamic_data>";
                }
            }
            if(Email != null)
            {
                ret += "<email>" + EscapeXml(Email) + "</email>";
            }
            if(ExternalId != null)
            {
                ret += "<externalId>" + EscapeXml(ExternalId) + "</externalId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<external_id>" + EscapeXml(ExternalId) + "</external_id>";
                }
            }
            if(!omitObsolete && FacebookId != null)
            {
                ret += "<facebookId>" + EscapeXml(FacebookId) + "</facebookId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<facebook_id>" + EscapeXml(FacebookId) + "</facebook_id>";
                }
            }
            if(!omitObsolete && FacebookImage != null)
            {
                ret += "<facebookImage>" + EscapeXml(FacebookImage) + "</facebookImage>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<facebook_image>" + EscapeXml(FacebookImage) + "</facebook_image>";
                }
            }
            if(!omitObsolete && FacebookToken != null)
            {
                ret += "<facebookToken>" + EscapeXml(FacebookToken) + "</facebookToken>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<facebook_token>" + EscapeXml(FacebookToken) + "</facebook_token>";
                }
            }
            if(HouseholdID.HasValue)
            {
                ret += "<householdId>" + HouseholdID + "</householdId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<household_id>" + HouseholdID + "</household_id>";
                }
            }
            if(IsHouseholdMaster.HasValue)
            {
                ret += "<isHouseholdMaster>" + IsHouseholdMaster.ToString().ToLower() + "</isHouseholdMaster>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<is_household_master>" + IsHouseholdMaster.ToString().ToLower() + "</is_household_master>";
                }
            }
            if(Phone != null)
            {
                ret += "<phone>" + EscapeXml(Phone) + "</phone>";
            }
            ret += "<suspensionState>" + "\"" + Enum.GetName(typeof(KalturaHouseholdSuspensionState), SuspensionState) + "\"" + "</suspensionState>";
            if(!omitObsolete)
            {
                ret += "<suspentionState>" + "\"" + Enum.GetName(typeof(KalturaHouseholdSuspentionState), SuspentionState) + "\"" + "</suspentionState>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<suspention_state>" + "\"" + Enum.GetName(typeof(KalturaHouseholdSuspentionState), SuspentionState) + "\"" + "</suspention_state>";
                }
            }
            ret += "<userState>" + "\"" + Enum.GetName(typeof(KalturaUserState), UserState) + "\"" + "</userState>";
            if (currentVersion == null || isOldVersion)
            {
            ret += "<user_state>" + "\"" + Enum.GetName(typeof(KalturaUserState), UserState) + "\"" + "</user_state>";
            }
            if(UserType != null)
            {
                propertyValue = UserType.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<userType>" + propertyValue + "</userType>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<user_type>" + propertyValue + "</user_type>";
                }
            }
            if(Zip != null)
            {
                ret += "<zip>" + EscapeXml(Zip) + "</zip>";
            }
            return ret;
        }
    }
    public partial class KalturaOTTUserDynamicData
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Key != null)
            {
                ret.Add("\"key\": " + "\"" + EscapeJson(Key) + "\"");
            }
            if(UserId != null)
            {
                ret.Add("\"userId\": " + "\"" + EscapeJson(UserId) + "\"");
            }
            if(Value != null)
            {
                propertyValue = Value.ToJson(currentVersion, omitObsolete);
                ret.Add("\"value\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Key != null)
            {
                ret += "<key>" + EscapeXml(Key) + "</key>";
            }
            if(UserId != null)
            {
                ret += "<userId>" + EscapeXml(UserId) + "</userId>";
            }
            if(Value != null)
            {
                propertyValue = Value.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<value>" + propertyValue + "</value>";
            }
            return ret;
        }
    }
    public partial class KalturaOTTUserDynamicDataList
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"dynamicData\": " + DynamicData);
            if(UserId != null)
            {
                ret.Add("\"userId\": " + "\"" + EscapeJson(UserId) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<dynamicData>" + DynamicData + "</dynamicData>";
            if(UserId != null)
            {
                ret += "<userId>" + EscapeXml(UserId) + "</userId>";
            }
            return ret;
        }
    }
    public partial class KalturaOTTUserFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ExternalIdEqual != null)
            {
                ret.Add("\"externalIdEqual\": " + "\"" + EscapeJson(ExternalIdEqual) + "\"");
            }
            if(IdIn != null)
            {
                ret.Add("\"idIn\": " + "\"" + EscapeJson(IdIn) + "\"");
            }
            if(UsernameEqual != null)
            {
                ret.Add("\"usernameEqual\": " + "\"" + EscapeJson(UsernameEqual) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ExternalIdEqual != null)
            {
                ret += "<externalIdEqual>" + EscapeXml(ExternalIdEqual) + "</externalIdEqual>";
            }
            if(IdIn != null)
            {
                ret += "<idIn>" + EscapeXml(IdIn) + "</idIn>";
            }
            if(UsernameEqual != null)
            {
                ret += "<usernameEqual>" + EscapeXml(UsernameEqual) + "</usernameEqual>";
            }
            return ret;
        }
    }
    public partial class KalturaOTTUserListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Users != null)
            {
                propertyValue = "[" + String.Join(", ", Users.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Users != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Users.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaOTTUserType
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Description != null)
            {
                ret.Add("\"description\": " + "\"" + EscapeJson(Description) + "\"");
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Description != null)
            {
                ret += "<description>" + EscapeXml(Description) + "</description>";
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            return ret;
        }
    }
    public partial class KalturaSession
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"createDate\": " + createDate);
            if(expiry.HasValue)
            {
                ret.Add("\"expiry\": " + expiry);
            }
            if(ks != null)
            {
                ret.Add("\"ks\": " + "\"" + EscapeJson(ks) + "\"");
            }
            if(partnerId.HasValue)
            {
                ret.Add("\"partnerId\": " + partnerId);
            }
            if(privileges != null)
            {
                ret.Add("\"privileges\": " + "\"" + EscapeJson(privileges) + "\"");
            }
            if(!DeprecatedAttribute.IsDeprecated("4.5.0.0", currentVersion))
            {
                ret.Add("\"sessionType\": " + "\"" + Enum.GetName(typeof(KalturaSessionType), sessionType) + "\"");
            }
            if(udid != null)
            {
                ret.Add("\"udid\": " + "\"" + EscapeJson(udid) + "\"");
            }
            if(userId != null)
            {
                ret.Add("\"userId\": " + "\"" + EscapeJson(userId) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<createDate>" + createDate + "</createDate>";
            if(expiry.HasValue)
            {
                ret += "<expiry>" + expiry + "</expiry>";
            }
            if(ks != null)
            {
                ret += "<ks>" + EscapeXml(ks) + "</ks>";
            }
            if(partnerId.HasValue)
            {
                ret += "<partnerId>" + partnerId + "</partnerId>";
            }
            if(privileges != null)
            {
                ret += "<privileges>" + EscapeXml(privileges) + "</privileges>";
            }
            if(!DeprecatedAttribute.IsDeprecated("4.5.0.0", currentVersion))
            {
                ret += "<sessionType>" + "\"" + Enum.GetName(typeof(KalturaSessionType), sessionType) + "\"" + "</sessionType>";
            }
            if(udid != null)
            {
                ret += "<udid>" + EscapeXml(udid) + "</udid>";
            }
            if(userId != null)
            {
                ret += "<userId>" + EscapeXml(userId) + "</userId>";
            }
            return ret;
        }
    }
    public partial class KalturaSessionInfo
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaSSOAdapterProfile
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AdapterUrl != null)
            {
                ret.Add("\"adapterUrl\": " + "\"" + EscapeJson(AdapterUrl) + "\"");
            }
            if(ExternalIdentifier != null)
            {
                ret.Add("\"externalIdentifier\": " + "\"" + EscapeJson(ExternalIdentifier) + "\"");
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(IsActive.HasValue)
            {
                ret.Add("\"isActive\": " + IsActive);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(Settings != null)
            {
                propertyValue = "{" + String.Join(", ", Settings.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"settings\": " + propertyValue);
            }
            if(SharedSecret != null)
            {
                ret.Add("\"sharedSecret\": " + "\"" + EscapeJson(SharedSecret) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AdapterUrl != null)
            {
                ret += "<adapterUrl>" + EscapeXml(AdapterUrl) + "</adapterUrl>";
            }
            if(ExternalIdentifier != null)
            {
                ret += "<externalIdentifier>" + EscapeXml(ExternalIdentifier) + "</externalIdentifier>";
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(IsActive.HasValue)
            {
                ret += "<isActive>" + IsActive + "</isActive>";
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(Settings != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Settings.Select(pair => "<itemKey>" + pair.Key + "</itemKey>" + pair.Value.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<settings>" + propertyValue + "</settings>";
            }
            if(SharedSecret != null)
            {
                ret += "<sharedSecret>" + EscapeXml(SharedSecret) + "</sharedSecret>";
            }
            return ret;
        }
    }
    public partial class KalturaSSOAdapterProfileListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(SSOAdapters != null)
            {
                propertyValue = "[" + String.Join(", ", SSOAdapters.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(SSOAdapters != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", SSOAdapters.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaUserAssetsList
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(List != null)
            {
                propertyValue = "[" + String.Join(", ", List.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"list\": " + propertyValue);
            }
            ret.Add("\"listType\": " + "\"" + Enum.GetName(typeof(KalturaUserAssetsListType), ListType) + "\"");
            if (currentVersion == null || isOldVersion)
            {
                ret.Add("\"list_type\": " + "\"" + Enum.GetName(typeof(KalturaUserAssetsListType), ListType) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(List != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", List.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<list>" + propertyValue + "</list>";
            }
            ret += "<listType>" + "\"" + Enum.GetName(typeof(KalturaUserAssetsListType), ListType) + "\"" + "</listType>";
            if (currentVersion == null || isOldVersion)
            {
            ret += "<list_type>" + "\"" + Enum.GetName(typeof(KalturaUserAssetsListType), ListType) + "\"" + "</list_type>";
            }
            return ret;
        }
    }
    public partial class KalturaUserAssetsListFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"assetTypeEqual\": " + "\"" + Enum.GetName(typeof(KalturaUserAssetsListItemType), AssetTypeEqual) + "\"");
            if (currentVersion == null || isOldVersion)
            {
                ret.Add("\"asset_type\": " + "\"" + Enum.GetName(typeof(KalturaUserAssetsListItemType), AssetTypeEqual) + "\"");
            }
            ret.Add("\"by\": " + "\"" + Enum.GetName(typeof(KalturaEntityReferenceBy), By) + "\"");
            ret.Add("\"listTypeEqual\": " + "\"" + Enum.GetName(typeof(KalturaUserAssetsListType), ListTypeEqual) + "\"");
            if (currentVersion == null || isOldVersion)
            {
                ret.Add("\"list_type\": " + "\"" + Enum.GetName(typeof(KalturaUserAssetsListType), ListTypeEqual) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<assetTypeEqual>" + "\"" + Enum.GetName(typeof(KalturaUserAssetsListItemType), AssetTypeEqual) + "\"" + "</assetTypeEqual>";
            if (currentVersion == null || isOldVersion)
            {
            ret += "<asset_type>" + "\"" + Enum.GetName(typeof(KalturaUserAssetsListItemType), AssetTypeEqual) + "\"" + "</asset_type>";
            }
            ret += "<by>" + "\"" + Enum.GetName(typeof(KalturaEntityReferenceBy), By) + "\"" + "</by>";
            ret += "<listTypeEqual>" + "\"" + Enum.GetName(typeof(KalturaUserAssetsListType), ListTypeEqual) + "\"" + "</listTypeEqual>";
            if (currentVersion == null || isOldVersion)
            {
            ret += "<list_type>" + "\"" + Enum.GetName(typeof(KalturaUserAssetsListType), ListTypeEqual) + "\"" + "</list_type>";
            }
            return ret;
        }
    }
    public partial class KalturaUserAssetsListItem
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + EscapeJson(Id) + "\"");
            }
            ret.Add("\"listType\": " + "\"" + Enum.GetName(typeof(KalturaUserAssetsListType), ListType) + "\"");
            if (currentVersion == null || isOldVersion)
            {
                ret.Add("\"list_type\": " + "\"" + Enum.GetName(typeof(KalturaUserAssetsListType), ListType) + "\"");
            }
            if(OrderIndex.HasValue)
            {
                ret.Add("\"orderIndex\": " + OrderIndex);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"order_index\": " + OrderIndex);
                }
            }
            ret.Add("\"type\": " + "\"" + Enum.GetName(typeof(KalturaUserAssetsListItemType), Type) + "\"");
            if(UserId != null)
            {
                ret.Add("\"userId\": " + "\"" + EscapeJson(UserId) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"user_id\": " + "\"" + EscapeJson(UserId) + "\"");
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Id != null)
            {
                ret += "<id>" + EscapeXml(Id) + "</id>";
            }
            ret += "<listType>" + "\"" + Enum.GetName(typeof(KalturaUserAssetsListType), ListType) + "\"" + "</listType>";
            if (currentVersion == null || isOldVersion)
            {
            ret += "<list_type>" + "\"" + Enum.GetName(typeof(KalturaUserAssetsListType), ListType) + "\"" + "</list_type>";
            }
            if(OrderIndex.HasValue)
            {
                ret += "<orderIndex>" + OrderIndex + "</orderIndex>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<order_index>" + OrderIndex + "</order_index>";
                }
            }
            ret += "<type>" + "\"" + Enum.GetName(typeof(KalturaUserAssetsListItemType), Type) + "\"" + "</type>";
            if(UserId != null)
            {
                ret += "<userId>" + EscapeXml(UserId) + "</userId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<user_id>" + EscapeXml(UserId) + "</user_id>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaUserInterest
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + EscapeJson(Id) + "\"");
            }
            if(Topic != null)
            {
                propertyValue = Topic.ToJson(currentVersion, omitObsolete);
                ret.Add("\"topic\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Id != null)
            {
                ret += "<id>" + EscapeXml(Id) + "</id>";
            }
            if(Topic != null)
            {
                propertyValue = Topic.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<topic>" + propertyValue + "</topic>";
            }
            return ret;
        }
    }
    public partial class KalturaUserInterestListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(UserInterests != null)
            {
                propertyValue = "[" + String.Join(", ", UserInterests.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(UserInterests != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", UserInterests.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaUserInterestTopic
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(MetaId != null)
            {
                ret.Add("\"metaId\": " + "\"" + EscapeJson(MetaId) + "\"");
            }
            if(ParentTopic != null)
            {
                propertyValue = ParentTopic.ToJson(currentVersion, omitObsolete);
                ret.Add("\"parentTopic\": " + propertyValue);
            }
            if(Value != null)
            {
                ret.Add("\"value\": " + "\"" + EscapeJson(Value) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(MetaId != null)
            {
                ret += "<metaId>" + EscapeXml(MetaId) + "</metaId>";
            }
            if(ParentTopic != null)
            {
                propertyValue = ParentTopic.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<parentTopic>" + propertyValue + "</parentTopic>";
            }
            if(Value != null)
            {
                ret += "<value>" + EscapeXml(Value) + "</value>";
            }
            return ret;
        }
    }
    public partial class KalturaUserLoginPin
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ExpirationTime.HasValue)
            {
                ret.Add("\"expirationTime\": " + ExpirationTime);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"expiration_time\": " + ExpirationTime);
                }
            }
            if(PinCode != null)
            {
                ret.Add("\"pinCode\": " + "\"" + EscapeJson(PinCode) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"pin_code\": " + "\"" + EscapeJson(PinCode) + "\"");
                }
            }
            if(UserId != null)
            {
                ret.Add("\"userId\": " + "\"" + EscapeJson(UserId) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"user_id\": " + "\"" + EscapeJson(UserId) + "\"");
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ExpirationTime.HasValue)
            {
                ret += "<expirationTime>" + ExpirationTime + "</expirationTime>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<expiration_time>" + ExpirationTime + "</expiration_time>";
                }
            }
            if(PinCode != null)
            {
                ret += "<pinCode>" + EscapeXml(PinCode) + "</pinCode>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<pin_code>" + EscapeXml(PinCode) + "</pin_code>";
                }
            }
            if(UserId != null)
            {
                ret += "<userId>" + EscapeXml(UserId) + "</userId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<user_id>" + EscapeXml(UserId) + "</user_id>";
                }
            }
            return ret;
        }
    }
}

namespace WebAPI.Models.Partner
{
    public partial class KalturaBillingPartnerConfig
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(!omitObsolete && PartnerConfigurationType != null)
            {
                propertyValue = PartnerConfigurationType.ToJson(currentVersion, omitObsolete);
                ret.Add("\"partnerConfigurationType\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"partner_configuration_type\": " + propertyValue);
                }
            }
            ret.Add("\"type\": " + "\"" + Enum.GetName(typeof(KalturaPartnerConfigurationType), Type) + "\"");
            if(Value != null)
            {
                ret.Add("\"value\": " + "\"" + EscapeJson(Value) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(!omitObsolete && PartnerConfigurationType != null)
            {
                propertyValue = PartnerConfigurationType.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<partnerConfigurationType>" + propertyValue + "</partnerConfigurationType>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<partner_configuration_type>" + propertyValue + "</partner_configuration_type>";
                }
            }
            ret += "<type>" + "\"" + Enum.GetName(typeof(KalturaPartnerConfigurationType), Type) + "\"" + "</type>";
            if(Value != null)
            {
                ret += "<value>" + EscapeXml(Value) + "</value>";
            }
            return ret;
        }
    }
    public partial class KalturaConcurrencyPartnerConfig
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(DeviceFamilyIds != null)
            {
                ret.Add("\"deviceFamilyIds\": " + "\"" + EscapeJson(DeviceFamilyIds) + "\"");
            }
            ret.Add("\"evictionPolicy\": " + "\"" + Enum.GetName(typeof(KalturaEvictionPolicyType), EvictionPolicy) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(DeviceFamilyIds != null)
            {
                ret += "<deviceFamilyIds>" + EscapeXml(DeviceFamilyIds) + "</deviceFamilyIds>";
            }
            ret += "<evictionPolicy>" + "\"" + Enum.GetName(typeof(KalturaEvictionPolicyType), EvictionPolicy) + "\"" + "</evictionPolicy>";
            return ret;
        }
    }
    public partial class KalturaPartnerConfiguration
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaPartnerConfigurationFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"partnerConfigurationTypeEqual\": " + "\"" + Enum.GetName(typeof(KalturaPartnerConfigurationType), PartnerConfigurationTypeEqual) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<partnerConfigurationTypeEqual>" + "\"" + Enum.GetName(typeof(KalturaPartnerConfigurationType), PartnerConfigurationTypeEqual) + "\"" + "</partnerConfigurationTypeEqual>";
            return ret;
        }
    }
    public partial class KalturaPartnerConfigurationHolder
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"type\": " + "\"" + Enum.GetName(typeof(KalturaPartnerConfigurationType), type) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<type>" + "\"" + Enum.GetName(typeof(KalturaPartnerConfigurationType), type) + "\"" + "</type>";
            return ret;
        }
    }
    public partial class KalturaPartnerConfigurationListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
}

namespace WebAPI.Models.Upload
{
    public partial class KalturaBulk
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(CreateDate.HasValue)
            {
                ret.Add("\"createDate\": " + CreateDate);
            }
            ret.Add("\"id\": " + Id);
            if(Status.HasValue)
            {
                ret.Add("\"status\": " + "\"" + Enum.GetName(typeof(KalturaBatchJobStatus), Status) + "\"");
            }
            if(UpdateDate.HasValue)
            {
                ret.Add("\"updateDate\": " + UpdateDate);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(CreateDate.HasValue)
            {
                ret += "<createDate>" + CreateDate + "</createDate>";
            }
            ret += "<id>" + Id + "</id>";
            if(Status.HasValue)
            {
                ret += "<status>" + "\"" + Enum.GetName(typeof(KalturaBatchJobStatus), Status) + "\"" + "</status>";
            }
            if(UpdateDate.HasValue)
            {
                ret += "<updateDate>" + UpdateDate + "</updateDate>";
            }
            return ret;
        }
    }
    public partial class KalturaBulkFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(StatusEqual.HasValue)
            {
                ret.Add("\"statusEqual\": " + "\"" + Enum.GetName(typeof(KalturaBatchJobStatus), StatusEqual) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(StatusEqual.HasValue)
            {
                ret += "<statusEqual>" + "\"" + Enum.GetName(typeof(KalturaBatchJobStatus), StatusEqual) + "\"" + "</statusEqual>";
            }
            return ret;
        }
    }
    public partial class KalturaBulkListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaUploadToken
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(CreateDate.HasValue)
            {
                ret.Add("\"createDate\": " + CreateDate);
            }
            if(FileSize.HasValue)
            {
                ret.Add("\"fileSize\": " + FileSize);
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + EscapeJson(Id) + "\"");
            }
            if(Status.HasValue)
            {
                ret.Add("\"status\": " + "\"" + Enum.GetName(typeof(KalturaUploadTokenStatus), Status) + "\"");
            }
            if(UpdateDate.HasValue)
            {
                ret.Add("\"updateDate\": " + UpdateDate);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(CreateDate.HasValue)
            {
                ret += "<createDate>" + CreateDate + "</createDate>";
            }
            if(FileSize.HasValue)
            {
                ret += "<fileSize>" + FileSize + "</fileSize>";
            }
            if(Id != null)
            {
                ret += "<id>" + EscapeXml(Id) + "</id>";
            }
            if(Status.HasValue)
            {
                ret += "<status>" + "\"" + Enum.GetName(typeof(KalturaUploadTokenStatus), Status) + "\"" + "</status>";
            }
            if(UpdateDate.HasValue)
            {
                ret += "<updateDate>" + UpdateDate + "</updateDate>";
            }
            return ret;
        }
    }
}

namespace WebAPI.Models.DMS
{
    public partial class KalturaConfigurationGroup
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ConfigurationIdentifiers != null)
            {
                propertyValue = "[" + String.Join(", ", ConfigurationIdentifiers.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"configurationIdentifiers\": " + propertyValue);
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + EscapeJson(Id) + "\"");
            }
            ret.Add("\"isDefault\": " + IsDefault.ToString().ToLower());
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            ret.Add("\"numberOfDevices\": " + NumberOfDevices);
            ret.Add("\"partnerId\": " + PartnerId);
            if(Tags != null)
            {
                propertyValue = "[" + String.Join(", ", Tags.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"tags\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ConfigurationIdentifiers != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", ConfigurationIdentifiers.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<configurationIdentifiers>" + propertyValue + "</configurationIdentifiers>";
            }
            if(Id != null)
            {
                ret += "<id>" + EscapeXml(Id) + "</id>";
            }
            ret += "<isDefault>" + IsDefault.ToString().ToLower() + "</isDefault>";
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            ret += "<numberOfDevices>" + NumberOfDevices + "</numberOfDevices>";
            ret += "<partnerId>" + PartnerId + "</partnerId>";
            if(Tags != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Tags.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<tags>" + propertyValue + "</tags>";
            }
            return ret;
        }
    }
    public partial class KalturaConfigurationGroupDevice
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ConfigurationGroupId != null)
            {
                ret.Add("\"configurationGroupId\": " + "\"" + EscapeJson(ConfigurationGroupId) + "\"");
            }
            ret.Add("\"partnerId\": " + PartnerId);
            if(Udid != null)
            {
                ret.Add("\"udid\": " + "\"" + EscapeJson(Udid) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ConfigurationGroupId != null)
            {
                ret += "<configurationGroupId>" + EscapeXml(ConfigurationGroupId) + "</configurationGroupId>";
            }
            ret += "<partnerId>" + PartnerId + "</partnerId>";
            if(Udid != null)
            {
                ret += "<udid>" + EscapeXml(Udid) + "</udid>";
            }
            return ret;
        }
    }
    public partial class KalturaConfigurationGroupDeviceFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ConfigurationGroupIdEqual != null)
            {
                ret.Add("\"configurationGroupIdEqual\": " + "\"" + EscapeJson(ConfigurationGroupIdEqual) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ConfigurationGroupIdEqual != null)
            {
                ret += "<configurationGroupIdEqual>" + EscapeXml(ConfigurationGroupIdEqual) + "</configurationGroupIdEqual>";
            }
            return ret;
        }
    }
    public partial class KalturaConfigurationGroupDeviceListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaConfigurationGroupListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaConfigurationGroupTag
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ConfigurationGroupId != null)
            {
                ret.Add("\"configurationGroupId\": " + "\"" + EscapeJson(ConfigurationGroupId) + "\"");
            }
            ret.Add("\"partnerId\": " + PartnerId);
            if(Tag != null)
            {
                ret.Add("\"tag\": " + "\"" + EscapeJson(Tag) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ConfigurationGroupId != null)
            {
                ret += "<configurationGroupId>" + EscapeXml(ConfigurationGroupId) + "</configurationGroupId>";
            }
            ret += "<partnerId>" + PartnerId + "</partnerId>";
            if(Tag != null)
            {
                ret += "<tag>" + EscapeXml(Tag) + "</tag>";
            }
            return ret;
        }
    }
    public partial class KalturaConfigurationGroupTagFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ConfigurationGroupIdEqual != null)
            {
                ret.Add("\"configurationGroupIdEqual\": " + "\"" + EscapeJson(ConfigurationGroupIdEqual) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ConfigurationGroupIdEqual != null)
            {
                ret += "<configurationGroupIdEqual>" + EscapeXml(ConfigurationGroupIdEqual) + "</configurationGroupIdEqual>";
            }
            return ret;
        }
    }
    public partial class KalturaConfigurationGroupTagListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaConfigurationIdentifier
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + EscapeJson(Id) + "\"");
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Id != null)
            {
                ret += "<id>" + EscapeXml(Id) + "</id>";
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            return ret;
        }
    }
    public partial class KalturaConfigurations
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AppName != null)
            {
                ret.Add("\"appName\": " + "\"" + EscapeJson(AppName) + "\"");
            }
            if(ClientVersion != null)
            {
                ret.Add("\"clientVersion\": " + "\"" + EscapeJson(ClientVersion) + "\"");
            }
            if(ConfigurationGroupId != null)
            {
                ret.Add("\"configurationGroupId\": " + "\"" + EscapeJson(ConfigurationGroupId) + "\"");
            }
            if(Content != null)
            {
                ret.Add("\"content\": " + "\"" + EscapeJson(Content) + "\"");
            }
            if(ExternalPushId != null)
            {
                ret.Add("\"externalPushId\": " + "\"" + EscapeJson(ExternalPushId) + "\"");
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + EscapeJson(Id) + "\"");
            }
            ret.Add("\"isForceUpdate\": " + IsForceUpdate.ToString().ToLower());
            ret.Add("\"partnerId\": " + PartnerId);
            ret.Add("\"platform\": " + "\"" + Enum.GetName(typeof(KalturaPlatform), Platform) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AppName != null)
            {
                ret += "<appName>" + EscapeXml(AppName) + "</appName>";
            }
            if(ClientVersion != null)
            {
                ret += "<clientVersion>" + EscapeXml(ClientVersion) + "</clientVersion>";
            }
            if(ConfigurationGroupId != null)
            {
                ret += "<configurationGroupId>" + EscapeXml(ConfigurationGroupId) + "</configurationGroupId>";
            }
            if(Content != null)
            {
                ret += "<content>" + EscapeXml(Content) + "</content>";
            }
            if(ExternalPushId != null)
            {
                ret += "<externalPushId>" + EscapeXml(ExternalPushId) + "</externalPushId>";
            }
            if(Id != null)
            {
                ret += "<id>" + EscapeXml(Id) + "</id>";
            }
            ret += "<isForceUpdate>" + IsForceUpdate.ToString().ToLower() + "</isForceUpdate>";
            ret += "<partnerId>" + PartnerId + "</partnerId>";
            ret += "<platform>" + "\"" + Enum.GetName(typeof(KalturaPlatform), Platform) + "\"" + "</platform>";
            return ret;
        }
    }
    public partial class KalturaConfigurationsFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ConfigurationGroupIdEqual != null)
            {
                ret.Add("\"configurationGroupIdEqual\": " + "\"" + EscapeJson(ConfigurationGroupIdEqual) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ConfigurationGroupIdEqual != null)
            {
                ret += "<configurationGroupIdEqual>" + EscapeXml(ConfigurationGroupIdEqual) + "</configurationGroupIdEqual>";
            }
            return ret;
        }
    }
    public partial class KalturaConfigurationsListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaDeviceReport
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ConfigurationGroupId != null)
            {
                ret.Add("\"configurationGroupId\": " + "\"" + EscapeJson(ConfigurationGroupId) + "\"");
            }
            ret.Add("\"lastAccessDate\": " + LastAccessDate);
            if(LastAccessIP != null)
            {
                ret.Add("\"lastAccessIP\": " + "\"" + EscapeJson(LastAccessIP) + "\"");
            }
            if(OperationSystem != null)
            {
                ret.Add("\"operationSystem\": " + "\"" + EscapeJson(OperationSystem) + "\"");
            }
            ret.Add("\"partnerId\": " + PartnerId);
            if(PushParameters != null)
            {
                propertyValue = PushParameters.ToJson(currentVersion, omitObsolete);
                ret.Add("\"pushParameters\": " + propertyValue);
            }
            if(Udid != null)
            {
                ret.Add("\"udid\": " + "\"" + EscapeJson(Udid) + "\"");
            }
            if(UserAgent != null)
            {
                ret.Add("\"userAgent\": " + "\"" + EscapeJson(UserAgent) + "\"");
            }
            if(VersionAppName != null)
            {
                ret.Add("\"versionAppName\": " + "\"" + EscapeJson(VersionAppName) + "\"");
            }
            if(VersionNumber != null)
            {
                ret.Add("\"versionNumber\": " + "\"" + EscapeJson(VersionNumber) + "\"");
            }
            ret.Add("\"versionPlatform\": " + "\"" + Enum.GetName(typeof(KalturaPlatform), VersionPlatform) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ConfigurationGroupId != null)
            {
                ret += "<configurationGroupId>" + EscapeXml(ConfigurationGroupId) + "</configurationGroupId>";
            }
            ret += "<lastAccessDate>" + LastAccessDate + "</lastAccessDate>";
            if(LastAccessIP != null)
            {
                ret += "<lastAccessIP>" + EscapeXml(LastAccessIP) + "</lastAccessIP>";
            }
            if(OperationSystem != null)
            {
                ret += "<operationSystem>" + EscapeXml(OperationSystem) + "</operationSystem>";
            }
            ret += "<partnerId>" + PartnerId + "</partnerId>";
            if(PushParameters != null)
            {
                propertyValue = PushParameters.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<pushParameters>" + propertyValue + "</pushParameters>";
            }
            if(Udid != null)
            {
                ret += "<udid>" + EscapeXml(Udid) + "</udid>";
            }
            if(UserAgent != null)
            {
                ret += "<userAgent>" + EscapeXml(UserAgent) + "</userAgent>";
            }
            if(VersionAppName != null)
            {
                ret += "<versionAppName>" + EscapeXml(VersionAppName) + "</versionAppName>";
            }
            if(VersionNumber != null)
            {
                ret += "<versionNumber>" + EscapeXml(VersionNumber) + "</versionNumber>";
            }
            ret += "<versionPlatform>" + "\"" + Enum.GetName(typeof(KalturaPlatform), VersionPlatform) + "\"" + "</versionPlatform>";
            return ret;
        }
    }
    public partial class KalturaDeviceReportFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"lastAccessDateGreaterThanOrEqual\": " + LastAccessDateGreaterThanOrEqual);
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<lastAccessDateGreaterThanOrEqual>" + LastAccessDateGreaterThanOrEqual + "</lastAccessDateGreaterThanOrEqual>";
            return ret;
        }
    }
    public partial class KalturaPushParams
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ExternalToken != null)
            {
                ret.Add("\"externalToken\": " + "\"" + EscapeJson(ExternalToken) + "\"");
            }
            if(Token != null)
            {
                ret.Add("\"token\": " + "\"" + EscapeJson(Token) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ExternalToken != null)
            {
                ret += "<externalToken>" + EscapeXml(ExternalToken) + "</externalToken>";
            }
            if(Token != null)
            {
                ret += "<token>" + EscapeXml(Token) + "</token>";
            }
            return ret;
        }
    }
}

namespace WebAPI.Models.Domains
{
    public partial class KalturaDevice
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
    public partial class KalturaDeviceBrand
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(DeviceFamilyId.HasValue)
            {
                ret.Add("\"deviceFamilyid\": " + DeviceFamilyId);
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(DeviceFamilyId.HasValue)
            {
                ret += "<deviceFamilyid>" + DeviceFamilyId + "</deviceFamilyid>";
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            return ret;
        }
    }
    public partial class KalturaDeviceFamily
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(!omitObsolete && Devices != null)
            {
                propertyValue = "[" + String.Join(", ", Devices.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"devices\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(!omitObsolete && Devices != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Devices.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<devices>" + propertyValue + "</devices>";
            }
            return ret;
        }
    }
    public partial class KalturaDeviceFamilyBase
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(!omitObsolete && ConcurrentLimit.HasValue)
            {
                ret.Add("\"concurrentLimit\": " + ConcurrentLimit);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"concurrent_limit\": " + ConcurrentLimit);
                }
            }
            if(!omitObsolete && DeviceLimit.HasValue)
            {
                ret.Add("\"deviceLimit\": " + DeviceLimit);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"device_limit\": " + DeviceLimit);
                }
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(!omitObsolete && ConcurrentLimit.HasValue)
            {
                ret += "<concurrentLimit>" + ConcurrentLimit + "</concurrentLimit>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<concurrent_limit>" + ConcurrentLimit + "</concurrent_limit>";
                }
            }
            if(!omitObsolete && DeviceLimit.HasValue)
            {
                ret += "<deviceLimit>" + DeviceLimit + "</deviceLimit>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<device_limit>" + DeviceLimit + "</device_limit>";
                }
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            return ret;
        }
    }
    public partial class KalturaDevicePin
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Pin != null)
            {
                ret.Add("\"pin\": " + "\"" + EscapeJson(Pin) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Pin != null)
            {
                ret += "<pin>" + EscapeXml(Pin) + "</pin>";
            }
            return ret;
        }
    }
    public partial class KalturaDeviceRegistrationStatusHolder
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"status\": " + "\"" + Enum.GetName(typeof(KalturaDeviceRegistrationStatus), Status) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<status>" + "\"" + Enum.GetName(typeof(KalturaDeviceRegistrationStatus), Status) + "\"" + "</status>";
            return ret;
        }
    }
    public partial class KalturaHomeNetwork
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Description != null)
            {
                ret.Add("\"description\": " + "\"" + EscapeJson(Description) + "\"");
            }
            if(ExternalId != null)
            {
                ret.Add("\"externalId\": " + "\"" + EscapeJson(ExternalId) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"external_id\": " + "\"" + EscapeJson(ExternalId) + "\"");
                }
            }
            if(IsActive.HasValue)
            {
                ret.Add("\"isActive\": " + IsActive.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"is_active\": " + IsActive.ToString().ToLower());
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Description != null)
            {
                ret += "<description>" + EscapeXml(Description) + "</description>";
            }
            if(ExternalId != null)
            {
                ret += "<externalId>" + EscapeXml(ExternalId) + "</externalId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<external_id>" + EscapeXml(ExternalId) + "</external_id>";
                }
            }
            if(IsActive.HasValue)
            {
                ret += "<isActive>" + IsActive.ToString().ToLower() + "</isActive>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<is_active>" + IsActive.ToString().ToLower() + "</is_active>";
                }
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            return ret;
        }
    }
    public partial class KalturaHomeNetworkListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaHousehold
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ConcurrentLimit.HasValue)
            {
                ret.Add("\"concurrentLimit\": " + ConcurrentLimit);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"concurrent_limit\": " + ConcurrentLimit);
                }
            }
            if(!omitObsolete && DefaultUsers != null)
            {
                propertyValue = "[" + String.Join(", ", DefaultUsers.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"defaultUsers\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"default_users\": " + propertyValue);
                }
            }
            if(Description != null)
            {
                ret.Add("\"description\": " + "\"" + EscapeJson(Description) + "\"");
            }
            if(!omitObsolete && DeviceFamilies != null)
            {
                propertyValue = "[" + String.Join(", ", DeviceFamilies.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"deviceFamilies\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"device_families\": " + propertyValue);
                }
            }
            if(DevicesLimit.HasValue)
            {
                ret.Add("\"devicesLimit\": " + DevicesLimit);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"devices_limit\": " + DevicesLimit);
                }
            }
            if(ExternalId != null)
            {
                ret.Add("\"externalId\": " + "\"" + EscapeJson(ExternalId) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"external_id\": " + "\"" + EscapeJson(ExternalId) + "\"");
                }
            }
            if(FrequencyNextDeviceAction.HasValue)
            {
                ret.Add("\"frequencyNextDeviceAction\": " + FrequencyNextDeviceAction);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"frequency_next_device_action\": " + FrequencyNextDeviceAction);
                }
            }
            if(FrequencyNextUserAction.HasValue)
            {
                ret.Add("\"frequencyNextUserAction\": " + FrequencyNextUserAction);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"frequency_next_user_action\": " + FrequencyNextUserAction);
                }
            }
            if(HouseholdLimitationsId.HasValue)
            {
                ret.Add("\"householdLimitationsId\": " + HouseholdLimitationsId);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"household_limitations_id\": " + HouseholdLimitationsId);
                }
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(IsFrequencyEnabled.HasValue)
            {
                ret.Add("\"isFrequencyEnabled\": " + IsFrequencyEnabled.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"is_frequency_enabled\": " + IsFrequencyEnabled.ToString().ToLower());
                }
            }
            if(!omitObsolete && MasterUsers != null)
            {
                propertyValue = "[" + String.Join(", ", MasterUsers.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"masterUsers\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"master_users\": " + propertyValue);
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(!omitObsolete && PendingUsers != null)
            {
                propertyValue = "[" + String.Join(", ", PendingUsers.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"pendingUsers\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"pending_users\": " + propertyValue);
                }
            }
            if(RegionId.HasValue)
            {
                ret.Add("\"regionId\": " + RegionId);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"region_id\": " + RegionId);
                }
            }
            if(Restriction.HasValue)
            {
                ret.Add("\"restriction\": " + "\"" + Enum.GetName(typeof(KalturaHouseholdRestriction), Restriction) + "\"");
            }
            if(RoleId.HasValue)
            {
                ret.Add("\"roleId\": " + RoleId);
            }
            if(State.HasValue)
            {
                ret.Add("\"state\": " + "\"" + Enum.GetName(typeof(KalturaHouseholdState), State) + "\"");
            }
            if(!omitObsolete && Users != null)
            {
                propertyValue = "[" + String.Join(", ", Users.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"users\": " + propertyValue);
            }
            if(UsersLimit.HasValue)
            {
                ret.Add("\"usersLimit\": " + UsersLimit);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"users_limit\": " + UsersLimit);
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ConcurrentLimit.HasValue)
            {
                ret += "<concurrentLimit>" + ConcurrentLimit + "</concurrentLimit>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<concurrent_limit>" + ConcurrentLimit + "</concurrent_limit>";
                }
            }
            if(!omitObsolete && DefaultUsers != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", DefaultUsers.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<defaultUsers>" + propertyValue + "</defaultUsers>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<default_users>" + propertyValue + "</default_users>";
                }
            }
            if(Description != null)
            {
                ret += "<description>" + EscapeXml(Description) + "</description>";
            }
            if(!omitObsolete && DeviceFamilies != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", DeviceFamilies.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<deviceFamilies>" + propertyValue + "</deviceFamilies>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<device_families>" + propertyValue + "</device_families>";
                }
            }
            if(DevicesLimit.HasValue)
            {
                ret += "<devicesLimit>" + DevicesLimit + "</devicesLimit>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<devices_limit>" + DevicesLimit + "</devices_limit>";
                }
            }
            if(ExternalId != null)
            {
                ret += "<externalId>" + EscapeXml(ExternalId) + "</externalId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<external_id>" + EscapeXml(ExternalId) + "</external_id>";
                }
            }
            if(FrequencyNextDeviceAction.HasValue)
            {
                ret += "<frequencyNextDeviceAction>" + FrequencyNextDeviceAction + "</frequencyNextDeviceAction>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<frequency_next_device_action>" + FrequencyNextDeviceAction + "</frequency_next_device_action>";
                }
            }
            if(FrequencyNextUserAction.HasValue)
            {
                ret += "<frequencyNextUserAction>" + FrequencyNextUserAction + "</frequencyNextUserAction>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<frequency_next_user_action>" + FrequencyNextUserAction + "</frequency_next_user_action>";
                }
            }
            if(HouseholdLimitationsId.HasValue)
            {
                ret += "<householdLimitationsId>" + HouseholdLimitationsId + "</householdLimitationsId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<household_limitations_id>" + HouseholdLimitationsId + "</household_limitations_id>";
                }
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(IsFrequencyEnabled.HasValue)
            {
                ret += "<isFrequencyEnabled>" + IsFrequencyEnabled.ToString().ToLower() + "</isFrequencyEnabled>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<is_frequency_enabled>" + IsFrequencyEnabled.ToString().ToLower() + "</is_frequency_enabled>";
                }
            }
            if(!omitObsolete && MasterUsers != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", MasterUsers.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<masterUsers>" + propertyValue + "</masterUsers>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<master_users>" + propertyValue + "</master_users>";
                }
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(!omitObsolete && PendingUsers != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", PendingUsers.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<pendingUsers>" + propertyValue + "</pendingUsers>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<pending_users>" + propertyValue + "</pending_users>";
                }
            }
            if(RegionId.HasValue)
            {
                ret += "<regionId>" + RegionId + "</regionId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<region_id>" + RegionId + "</region_id>";
                }
            }
            if(Restriction.HasValue)
            {
                ret += "<restriction>" + "\"" + Enum.GetName(typeof(KalturaHouseholdRestriction), Restriction) + "\"" + "</restriction>";
            }
            if(RoleId.HasValue)
            {
                ret += "<roleId>" + RoleId + "</roleId>";
            }
            if(State.HasValue)
            {
                ret += "<state>" + "\"" + Enum.GetName(typeof(KalturaHouseholdState), State) + "\"" + "</state>";
            }
            if(!omitObsolete && Users != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Users.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<users>" + propertyValue + "</users>";
            }
            if(UsersLimit.HasValue)
            {
                ret += "<usersLimit>" + UsersLimit + "</usersLimit>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<users_limit>" + UsersLimit + "</users_limit>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaHouseholdDevice
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ActivatedOn.HasValue)
            {
                ret.Add("\"activatedOn\": " + ActivatedOn);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"activated_on\": " + ActivatedOn);
                }
            }
            if(!omitObsolete && Brand != null)
            {
                ret.Add("\"brand\": " + "\"" + EscapeJson(Brand) + "\"");
            }
            if(BrandId.HasValue)
            {
                ret.Add("\"brandId\": " + BrandId);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"brand_id\": " + BrandId);
                }
            }
            if(DeviceFamilyId.HasValue)
            {
                ret.Add("\"deviceFamilyId\": " + DeviceFamilyId);
            }
            if(Drm != null)
            {
                propertyValue = Drm.ToJson(currentVersion, omitObsolete);
                ret.Add("\"drm\": " + propertyValue);
            }
            ret.Add("\"householdId\": " + HouseholdId);
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(!omitObsolete && State.HasValue)
            {
                ret.Add("\"state\": " + "\"" + Enum.GetName(typeof(KalturaDeviceState), State) + "\"");
            }
            if(Status.HasValue)
            {
                ret.Add("\"status\": " + "\"" + Enum.GetName(typeof(KalturaDeviceStatus), Status) + "\"");
            }
            if(Udid != null)
            {
                ret.Add("\"udid\": " + "\"" + EscapeJson(Udid) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ActivatedOn.HasValue)
            {
                ret += "<activatedOn>" + ActivatedOn + "</activatedOn>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<activated_on>" + ActivatedOn + "</activated_on>";
                }
            }
            if(!omitObsolete && Brand != null)
            {
                ret += "<brand>" + EscapeXml(Brand) + "</brand>";
            }
            if(BrandId.HasValue)
            {
                ret += "<brandId>" + BrandId + "</brandId>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<brand_id>" + BrandId + "</brand_id>";
                }
            }
            if(DeviceFamilyId.HasValue)
            {
                ret += "<deviceFamilyId>" + DeviceFamilyId + "</deviceFamilyId>";
            }
            if(Drm != null)
            {
                propertyValue = Drm.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<drm>" + propertyValue + "</drm>";
            }
            ret += "<householdId>" + HouseholdId + "</householdId>";
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(!omitObsolete && State.HasValue)
            {
                ret += "<state>" + "\"" + Enum.GetName(typeof(KalturaDeviceState), State) + "\"" + "</state>";
            }
            if(Status.HasValue)
            {
                ret += "<status>" + "\"" + Enum.GetName(typeof(KalturaDeviceStatus), Status) + "\"" + "</status>";
            }
            if(Udid != null)
            {
                ret += "<udid>" + EscapeXml(Udid) + "</udid>";
            }
            return ret;
        }
    }
    public partial class KalturaHouseholdDeviceFamilyLimitations
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ConcurrentLimit.HasValue)
            {
                ret.Add("\"concurrentLimit\": " + ConcurrentLimit);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"concurrent_limit\": " + ConcurrentLimit);
                }
            }
            if(DeviceLimit.HasValue)
            {
                ret.Add("\"deviceLimit\": " + DeviceLimit);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"device_limit\": " + DeviceLimit);
                }
            }
            if(Frequency.HasValue)
            {
                ret.Add("\"frequency\": " + Frequency);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ConcurrentLimit.HasValue)
            {
                ret += "<concurrentLimit>" + ConcurrentLimit + "</concurrentLimit>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<concurrent_limit>" + ConcurrentLimit + "</concurrent_limit>";
                }
            }
            if(DeviceLimit.HasValue)
            {
                ret += "<deviceLimit>" + DeviceLimit + "</deviceLimit>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<device_limit>" + DeviceLimit + "</device_limit>";
                }
            }
            if(Frequency.HasValue)
            {
                ret += "<frequency>" + Frequency + "</frequency>";
            }
            return ret;
        }
    }
    public partial class KalturaHouseholdDeviceFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(DeviceFamilyIdIn != null)
            {
                ret.Add("\"deviceFamilyIdIn\": " + "\"" + EscapeJson(DeviceFamilyIdIn) + "\"");
            }
            if(HouseholdIdEqual.HasValue)
            {
                ret.Add("\"householdIdEqual\": " + HouseholdIdEqual);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(DeviceFamilyIdIn != null)
            {
                ret += "<deviceFamilyIdIn>" + EscapeXml(DeviceFamilyIdIn) + "</deviceFamilyIdIn>";
            }
            if(HouseholdIdEqual.HasValue)
            {
                ret += "<householdIdEqual>" + HouseholdIdEqual + "</householdIdEqual>";
            }
            return ret;
        }
    }
    public partial class KalturaHouseholdDeviceListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaHouseholdLimitations
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(ConcurrentLimit.HasValue)
            {
                ret.Add("\"concurrentLimit\": " + ConcurrentLimit);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"concurrent_limit\": " + ConcurrentLimit);
                }
            }
            if(DeviceFamiliesLimitations != null)
            {
                propertyValue = "[" + String.Join(", ", DeviceFamiliesLimitations.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"deviceFamiliesLimitations\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"device_families_limitations\": " + propertyValue);
                }
            }
            if(DeviceFrequency.HasValue)
            {
                ret.Add("\"deviceFrequency\": " + DeviceFrequency);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"device_frequency\": " + DeviceFrequency);
                }
            }
            if(DeviceFrequencyDescription != null)
            {
                ret.Add("\"deviceFrequencyDescription\": " + "\"" + EscapeJson(DeviceFrequencyDescription) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"device_frequency_description\": " + "\"" + EscapeJson(DeviceFrequencyDescription) + "\"");
                }
            }
            if(DeviceLimit.HasValue)
            {
                ret.Add("\"deviceLimit\": " + DeviceLimit);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"device_limit\": " + DeviceLimit);
                }
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(NpvrQuotaInSeconds.HasValue)
            {
                ret.Add("\"npvrQuotaInSeconds\": " + NpvrQuotaInSeconds);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"npvr_quota_in_seconds\": " + NpvrQuotaInSeconds);
                }
            }
            if(UserFrequency.HasValue)
            {
                ret.Add("\"userFrequency\": " + UserFrequency);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"user_frequency\": " + UserFrequency);
                }
            }
            if(UserFrequencyDescription != null)
            {
                ret.Add("\"userFrequencyDescription\": " + "\"" + EscapeJson(UserFrequencyDescription) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"user_frequency_description\": " + "\"" + EscapeJson(UserFrequencyDescription) + "\"");
                }
            }
            if(UsersLimit.HasValue)
            {
                ret.Add("\"usersLimit\": " + UsersLimit);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"users_limit\": " + UsersLimit);
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(ConcurrentLimit.HasValue)
            {
                ret += "<concurrentLimit>" + ConcurrentLimit + "</concurrentLimit>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<concurrent_limit>" + ConcurrentLimit + "</concurrent_limit>";
                }
            }
            if(DeviceFamiliesLimitations != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", DeviceFamiliesLimitations.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<deviceFamiliesLimitations>" + propertyValue + "</deviceFamiliesLimitations>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<device_families_limitations>" + propertyValue + "</device_families_limitations>";
                }
            }
            if(DeviceFrequency.HasValue)
            {
                ret += "<deviceFrequency>" + DeviceFrequency + "</deviceFrequency>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<device_frequency>" + DeviceFrequency + "</device_frequency>";
                }
            }
            if(DeviceFrequencyDescription != null)
            {
                ret += "<deviceFrequencyDescription>" + EscapeXml(DeviceFrequencyDescription) + "</deviceFrequencyDescription>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<device_frequency_description>" + EscapeXml(DeviceFrequencyDescription) + "</device_frequency_description>";
                }
            }
            if(DeviceLimit.HasValue)
            {
                ret += "<deviceLimit>" + DeviceLimit + "</deviceLimit>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<device_limit>" + DeviceLimit + "</device_limit>";
                }
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(NpvrQuotaInSeconds.HasValue)
            {
                ret += "<npvrQuotaInSeconds>" + NpvrQuotaInSeconds + "</npvrQuotaInSeconds>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<npvr_quota_in_seconds>" + NpvrQuotaInSeconds + "</npvr_quota_in_seconds>";
                }
            }
            if(UserFrequency.HasValue)
            {
                ret += "<userFrequency>" + UserFrequency + "</userFrequency>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<user_frequency>" + UserFrequency + "</user_frequency>";
                }
            }
            if(UserFrequencyDescription != null)
            {
                ret += "<userFrequencyDescription>" + EscapeXml(UserFrequencyDescription) + "</userFrequencyDescription>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<user_frequency_description>" + EscapeXml(UserFrequencyDescription) + "</user_frequency_description>";
                }
            }
            if(UsersLimit.HasValue)
            {
                ret += "<usersLimit>" + UsersLimit + "</usersLimit>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<users_limit>" + UsersLimit + "</users_limit>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaHouseholdUser
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(HouseholdId.HasValue)
            {
                ret.Add("\"householdId\": " + HouseholdId);
            }
            if(HouseholdMasterUsername != null)
            {
                ret.Add("\"householdMasterUsername\": " + "\"" + EscapeJson(HouseholdMasterUsername) + "\"");
            }
            if(IsDefault.HasValue)
            {
                ret.Add("\"isDefault\": " + IsDefault.ToString().ToLower());
            }
            if(IsMaster.HasValue)
            {
                ret.Add("\"isMaster\": " + IsMaster.ToString().ToLower());
            }
            ret.Add("\"status\": " + "\"" + Enum.GetName(typeof(KalturaHouseholdUserStatus), Status) + "\"");
            if(UserId != null)
            {
                ret.Add("\"userId\": " + "\"" + EscapeJson(UserId) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(HouseholdId.HasValue)
            {
                ret += "<householdId>" + HouseholdId + "</householdId>";
            }
            if(HouseholdMasterUsername != null)
            {
                ret += "<householdMasterUsername>" + EscapeXml(HouseholdMasterUsername) + "</householdMasterUsername>";
            }
            if(IsDefault.HasValue)
            {
                ret += "<isDefault>" + IsDefault.ToString().ToLower() + "</isDefault>";
            }
            if(IsMaster.HasValue)
            {
                ret += "<isMaster>" + IsMaster.ToString().ToLower() + "</isMaster>";
            }
            ret += "<status>" + "\"" + Enum.GetName(typeof(KalturaHouseholdUserStatus), Status) + "\"" + "</status>";
            if(UserId != null)
            {
                ret += "<userId>" + EscapeXml(UserId) + "</userId>";
            }
            return ret;
        }
    }
    public partial class KalturaHouseholdUserFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(HouseholdIdEqual.HasValue)
            {
                ret.Add("\"householdIdEqual\": " + HouseholdIdEqual);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(HouseholdIdEqual.HasValue)
            {
                ret += "<householdIdEqual>" + HouseholdIdEqual + "</householdIdEqual>";
            }
            return ret;
        }
    }
    public partial class KalturaHouseholdUserListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaHouseholdWithHolder
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"type\": " + "\"" + Enum.GetName(typeof(KalturaHouseholdWith), type) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<type>" + "\"" + Enum.GetName(typeof(KalturaHouseholdWith), type) + "\"" + "</type>";
            return ret;
        }
    }
}

namespace WebAPI.Models.Billing
{
    public partial class KalturaHouseholdPaymentGateway
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(IsDefault.HasValue)
            {
                ret.Add("\"isDefault\": " + IsDefault.ToString().ToLower());
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            ret.Add("\"selectedBy\": " + "\"" + Enum.GetName(typeof(KalturaHouseholdPaymentGatewaySelectedBy), selectedBy) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(IsDefault.HasValue)
            {
                ret += "<isDefault>" + IsDefault.ToString().ToLower() + "</isDefault>";
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            ret += "<selectedBy>" + "\"" + Enum.GetName(typeof(KalturaHouseholdPaymentGatewaySelectedBy), selectedBy) + "\"" + "</selectedBy>";
            return ret;
        }
    }
    public partial class KalturaHouseholdPaymentGatewayListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaHouseholdPaymentMethod
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(!omitObsolete && AllowMultiInstance.HasValue)
            {
                ret.Add("\"allowMultiInstance\": " + AllowMultiInstance.ToString().ToLower());
            }
            if(Details != null)
            {
                ret.Add("\"details\": " + "\"" + EscapeJson(Details) + "\"");
            }
            if(ExternalId != null)
            {
                ret.Add("\"externalId\": " + "\"" + EscapeJson(ExternalId) + "\"");
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(IsDefault.HasValue)
            {
                ret.Add("\"isDefault\": " + IsDefault.ToString().ToLower());
            }
            if(!omitObsolete && Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(PaymentGatewayId.HasValue)
            {
                ret.Add("\"paymentGatewayId\": " + PaymentGatewayId);
            }
            ret.Add("\"paymentMethodProfileId\": " + PaymentMethodProfileId);
            if(!omitObsolete && Selected.HasValue)
            {
                ret.Add("\"selected\": " + Selected.ToString().ToLower());
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(!omitObsolete && AllowMultiInstance.HasValue)
            {
                ret += "<allowMultiInstance>" + AllowMultiInstance.ToString().ToLower() + "</allowMultiInstance>";
            }
            if(Details != null)
            {
                ret += "<details>" + EscapeXml(Details) + "</details>";
            }
            if(ExternalId != null)
            {
                ret += "<externalId>" + EscapeXml(ExternalId) + "</externalId>";
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(IsDefault.HasValue)
            {
                ret += "<isDefault>" + IsDefault.ToString().ToLower() + "</isDefault>";
            }
            if(!omitObsolete && Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(PaymentGatewayId.HasValue)
            {
                ret += "<paymentGatewayId>" + PaymentGatewayId + "</paymentGatewayId>";
            }
            ret += "<paymentMethodProfileId>" + PaymentMethodProfileId + "</paymentMethodProfileId>";
            if(!omitObsolete && Selected.HasValue)
            {
                ret += "<selected>" + Selected.ToString().ToLower() + "</selected>";
            }
            return ret;
        }
    }
    public partial class KalturaHouseholdPaymentMethodListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Objects != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Objects.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaPaymentGateway
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(paymentGateway != null)
            {
                propertyValue = paymentGateway.ToJson(currentVersion, omitObsolete);
                ret.Add("\"payment_gateway\": " + propertyValue);
            }
            ret.Add("\"selected_by\": " + "\"" + Enum.GetName(typeof(KalturaHouseholdPaymentGatewaySelectedBy), selectedBy) + "\"");
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(paymentGateway != null)
            {
                propertyValue = paymentGateway.PropertiesToXml(currentVersion, omitObsolete);
                ret += "<payment_gateway>" + propertyValue + "</payment_gateway>";
            }
            ret += "<selected_by>" + "\"" + Enum.GetName(typeof(KalturaHouseholdPaymentGatewaySelectedBy), selectedBy) + "\"" + "</selected_by>";
            return ret;
        }
    }
    public partial class KalturaPaymentGatewayBaseProfile
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(IsDefault.HasValue)
            {
                ret.Add("\"isDefault\": " + IsDefault.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"is_default\": " + IsDefault.ToString().ToLower());
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(!omitObsolete && PaymentMethods != null)
            {
                propertyValue = "[" + String.Join(", ", PaymentMethods.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"paymentMethods\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"payment_methods\": " + propertyValue);
                }
            }
            if(selectedBy.HasValue)
            {
                ret.Add("\"selectedBy\": " + "\"" + Enum.GetName(typeof(KalturaHouseholdPaymentGatewaySelectedBy), selectedBy) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"selected_by\": " + "\"" + Enum.GetName(typeof(KalturaHouseholdPaymentGatewaySelectedBy), selectedBy) + "\"");
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(IsDefault.HasValue)
            {
                ret += "<isDefault>" + IsDefault.ToString().ToLower() + "</isDefault>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<is_default>" + IsDefault.ToString().ToLower() + "</is_default>";
                }
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(!omitObsolete && PaymentMethods != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", PaymentMethods.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<paymentMethods>" + propertyValue + "</paymentMethods>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<payment_methods>" + propertyValue + "</payment_methods>";
                }
            }
            if(selectedBy.HasValue)
            {
                ret += "<selectedBy>" + "\"" + Enum.GetName(typeof(KalturaHouseholdPaymentGatewaySelectedBy), selectedBy) + "\"" + "</selectedBy>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<selected_by>" + "\"" + Enum.GetName(typeof(KalturaHouseholdPaymentGatewaySelectedBy), selectedBy) + "\"" + "</selected_by>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaPaymentGatewayConfiguration
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(Configuration != null)
            {
                propertyValue = "[" + String.Join(", ", Configuration.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"paymentGatewayConfiguration\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"payment_gatewaye_configuration\": " + propertyValue);
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(Configuration != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Configuration.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<paymentGatewayConfiguration>" + propertyValue + "</paymentGatewayConfiguration>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<payment_gatewaye_configuration>" + propertyValue + "</payment_gatewaye_configuration>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaPaymentGatewayProfile
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AdapterUrl != null)
            {
                ret.Add("\"adapterUrl\": " + "\"" + EscapeJson(AdapterUrl) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"adapter_url\": " + "\"" + EscapeJson(AdapterUrl) + "\"");
                }
            }
            if(ExternalIdentifier != null)
            {
                ret.Add("\"externalIdentifier\": " + "\"" + EscapeJson(ExternalIdentifier) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"external_identifier\": " + "\"" + EscapeJson(ExternalIdentifier) + "\"");
                }
            }
            ret.Add("\"externalVerification\": " + ExternalVerification.ToString().ToLower());
            if(IsActive.HasValue)
            {
                ret.Add("\"isActive\": " + IsActive);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"is_active\": " + IsActive);
                }
            }
            if(PendingInterval.HasValue)
            {
                ret.Add("\"pendingInterval\": " + PendingInterval);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"pending_interval\": " + PendingInterval);
                }
            }
            if(PendingRetries.HasValue)
            {
                ret.Add("\"pendingRetries\": " + PendingRetries);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"pending_retries\": " + PendingRetries);
                }
            }
            if(RenewIntervalMinutes.HasValue)
            {
                ret.Add("\"renewIntervalMinutes\": " + RenewIntervalMinutes);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"renew_interval_minutes\": " + RenewIntervalMinutes);
                }
            }
            if(RenewStartMinutes.HasValue)
            {
                ret.Add("\"renewStartMinutes\": " + RenewStartMinutes);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"renew_start_minutes\": " + RenewStartMinutes);
                }
            }
            if(RenewUrl != null)
            {
                ret.Add("\"renewUrl\": " + "\"" + EscapeJson(RenewUrl) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"renew_url\": " + "\"" + EscapeJson(RenewUrl) + "\"");
                }
            }
            if(Settings != null)
            {
                propertyValue = "{" + String.Join(", ", Settings.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"paymentGatewaySettings\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"payment_gateway_settings\": " + propertyValue);
                }
            }
            if(SharedSecret != null)
            {
                ret.Add("\"sharedSecret\": " + "\"" + EscapeJson(SharedSecret) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"shared_secret\": " + "\"" + EscapeJson(SharedSecret) + "\"");
                }
            }
            if(StatusUrl != null)
            {
                ret.Add("\"statusUrl\": " + "\"" + EscapeJson(StatusUrl) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"status_url\": " + "\"" + EscapeJson(StatusUrl) + "\"");
                }
            }
            if(TransactUrl != null)
            {
                ret.Add("\"transactUrl\": " + "\"" + EscapeJson(TransactUrl) + "\"");
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"transact_url\": " + "\"" + EscapeJson(TransactUrl) + "\"");
                }
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AdapterUrl != null)
            {
                ret += "<adapterUrl>" + EscapeXml(AdapterUrl) + "</adapterUrl>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<adapter_url>" + EscapeXml(AdapterUrl) + "</adapter_url>";
                }
            }
            if(ExternalIdentifier != null)
            {
                ret += "<externalIdentifier>" + EscapeXml(ExternalIdentifier) + "</externalIdentifier>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<external_identifier>" + EscapeXml(ExternalIdentifier) + "</external_identifier>";
                }
            }
            ret += "<externalVerification>" + ExternalVerification.ToString().ToLower() + "</externalVerification>";
            if(IsActive.HasValue)
            {
                ret += "<isActive>" + IsActive + "</isActive>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<is_active>" + IsActive + "</is_active>";
                }
            }
            if(PendingInterval.HasValue)
            {
                ret += "<pendingInterval>" + PendingInterval + "</pendingInterval>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<pending_interval>" + PendingInterval + "</pending_interval>";
                }
            }
            if(PendingRetries.HasValue)
            {
                ret += "<pendingRetries>" + PendingRetries + "</pendingRetries>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<pending_retries>" + PendingRetries + "</pending_retries>";
                }
            }
            if(RenewIntervalMinutes.HasValue)
            {
                ret += "<renewIntervalMinutes>" + RenewIntervalMinutes + "</renewIntervalMinutes>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<renew_interval_minutes>" + RenewIntervalMinutes + "</renew_interval_minutes>";
                }
            }
            if(RenewStartMinutes.HasValue)
            {
                ret += "<renewStartMinutes>" + RenewStartMinutes + "</renewStartMinutes>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<renew_start_minutes>" + RenewStartMinutes + "</renew_start_minutes>";
                }
            }
            if(RenewUrl != null)
            {
                ret += "<renewUrl>" + EscapeXml(RenewUrl) + "</renewUrl>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<renew_url>" + EscapeXml(RenewUrl) + "</renew_url>";
                }
            }
            if(Settings != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", Settings.Select(pair => "<itemKey>" + pair.Key + "</itemKey>" + pair.Value.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<paymentGatewaySettings>" + propertyValue + "</paymentGatewaySettings>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<payment_gateway_settings>" + propertyValue + "</payment_gateway_settings>";
                }
            }
            if(SharedSecret != null)
            {
                ret += "<sharedSecret>" + EscapeXml(SharedSecret) + "</sharedSecret>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<shared_secret>" + EscapeXml(SharedSecret) + "</shared_secret>";
                }
            }
            if(StatusUrl != null)
            {
                ret += "<statusUrl>" + EscapeXml(StatusUrl) + "</statusUrl>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<status_url>" + EscapeXml(StatusUrl) + "</status_url>";
                }
            }
            if(TransactUrl != null)
            {
                ret += "<transactUrl>" + EscapeXml(TransactUrl) + "</transactUrl>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<transact_url>" + EscapeXml(TransactUrl) + "</transact_url>";
                }
            }
            return ret;
        }
    }
    public partial class KalturaPaymentGatewayProfileListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(PaymentGatewayProfiles != null)
            {
                propertyValue = "[" + String.Join(", ", PaymentGatewayProfiles.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(PaymentGatewayProfiles != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", PaymentGatewayProfiles.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
    public partial class KalturaPaymentMethod
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AllowMultiInstance.HasValue)
            {
                ret.Add("\"allowMultiInstance\": " + AllowMultiInstance.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"allow_multi_instance\": " + AllowMultiInstance.ToString().ToLower());
                }
            }
            if(HouseholdPaymentMethods != null)
            {
                propertyValue = "[" + String.Join(", ", HouseholdPaymentMethods.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"householdPaymentMethods\": " + propertyValue);
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"household_payment_methods\": " + propertyValue);
                }
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AllowMultiInstance.HasValue)
            {
                ret += "<allowMultiInstance>" + AllowMultiInstance.ToString().ToLower() + "</allowMultiInstance>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<allow_multi_instance>" + AllowMultiInstance.ToString().ToLower() + "</allow_multi_instance>";
                }
            }
            if(HouseholdPaymentMethods != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", HouseholdPaymentMethods.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<householdPaymentMethods>" + propertyValue + "</householdPaymentMethods>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<household_payment_methods>" + propertyValue + "</household_payment_methods>";
                }
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            return ret;
        }
    }
    public partial class KalturaPaymentMethodProfile
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(AllowMultiInstance.HasValue)
            {
                ret.Add("\"allowMultiInstance\": " + AllowMultiInstance.ToString().ToLower());
                if (currentVersion == null || isOldVersion)
                {
                    ret.Add("\"allow_multi_instance\": " + AllowMultiInstance.ToString().ToLower());
                }
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + EscapeJson(Name) + "\"");
            }
            if(PaymentGatewayId.HasValue)
            {
                ret.Add("\"paymentGatewayId\": " + PaymentGatewayId);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(AllowMultiInstance.HasValue)
            {
                ret += "<allowMultiInstance>" + AllowMultiInstance.ToString().ToLower() + "</allowMultiInstance>";
                if (currentVersion == null || isOldVersion)
                {
                ret += "<allow_multi_instance>" + AllowMultiInstance.ToString().ToLower() + "</allow_multi_instance>";
                }
            }
            if(Id.HasValue)
            {
                ret += "<id>" + Id + "</id>";
            }
            if(Name != null)
            {
                ret += "<name>" + EscapeXml(Name) + "</name>";
            }
            if(PaymentGatewayId.HasValue)
            {
                ret += "<paymentGatewayId>" + PaymentGatewayId + "</paymentGatewayId>";
            }
            return ret;
        }
    }
    public partial class KalturaPaymentMethodProfileFilter
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(PaymentGatewayIdEqual.HasValue)
            {
                ret.Add("\"paymentGatewayIdEqual\": " + PaymentGatewayIdEqual);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(PaymentGatewayIdEqual.HasValue)
            {
                ret += "<paymentGatewayIdEqual>" + PaymentGatewayIdEqual + "</paymentGatewayIdEqual>";
            }
            return ret;
        }
    }
    public partial class KalturaPaymentMethodProfileListResponse
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            if(PaymentMethodProfiles != null)
            {
                propertyValue = "[" + String.Join(", ", PaymentMethodProfiles.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            if(PaymentMethodProfiles != null)
            {
                propertyValue = "<item>" + String.Join("</item><item>", PaymentMethodProfiles.Select(item => item.PropertiesToXml(currentVersion, omitObsolete))) + "</item>";
                ret += "<objects>" + propertyValue + "</objects>";
            }
            return ret;
        }
    }
}

namespace WebAPI.EventNotifications
{
    public partial class KalturaHttpNotification
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            return ret;
        }
    }
}

namespace WebAPI.Managers.Models
{
    public partial class StatusWrapper
    {
        protected override List<string> PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            List<string> ret = base.PropertiesToJson(currentVersion, omitObsolete);
            string propertyValue;
            ret.Add("\"executionTime\": " + ExecutionTime);
            if(Result != null)
            {
                propertyValue = (Result is IKalturaSerializable ? (Result as IKalturaSerializable).ToJson(currentVersion, omitObsolete) : JsonManager.GetInstance().Serialize(Result));
                ret.Add("\"result\": " + propertyValue);
            }
            return ret;
        }
        
        public override string PropertiesToXml(Version currentVersion, bool omitObsolete)
        {
            bool isOldVersion = OldStandardAttribute.isCurrentRequestOldVersion(currentVersion);
            string ret = base.PropertiesToXml(currentVersion, omitObsolete);
            string propertyValue;
            ret += "<executionTime>" + ExecutionTime + "</executionTime>";
            if(Result != null)
            {
                propertyValue = (Result is IKalturaSerializable ? (Result as IKalturaSerializable).PropertiesToXml(currentVersion, omitObsolete) : Result.ToString());
                ret += "<result>" + propertyValue + "</result>";
            }
            return ret;
        }
    }
}
