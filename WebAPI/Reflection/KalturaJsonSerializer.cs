// NOTICE: This is a generated file, to modify it, edit Program.cs in Reflector project
using System;
using System.Linq;
using System.Web;
using WebAPI.Managers.Scheme;
using System.Collections.Generic;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.Social;
using WebAPI.Models.General;
using WebAPI.Models.Notifications;
using WebAPI.Models.Notification;
using WebAPI.App_Start;
using WebAPI.Models.Catalog;
using WebAPI.Models.Pricing;
using WebAPI.Models.Users;
using WebAPI.Models.Partner;
using WebAPI.Models.API;
using WebAPI.Models.DMS;
using WebAPI.Models.Domains;
using WebAPI.Models.Billing;
using WebAPI.EventNotifications;
using WebAPI.Managers.Models;

namespace WebAPI.Models.ConditionalAccess
{
    public partial class KalturaAccessControlBlockAction
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"type\": " + Type.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAccessControlMessage
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Code != null)
            {
                ret.Add("\"code\": " + "\"" + Code + "\"");
            }
            if(Message != null)
            {
                ret.Add("\"message\": " + "\"" + Message + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAdsContext
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Sources != null && Sources.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Sources.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"sources\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAdsSource
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(AdsParams != null)
            {
                ret.Add("\"adsParam\": " + "\"" + AdsParams + "\"");
            }
            if(AdsPolicy.HasValue)
            {
                ret.Add("\"adsPolicy\": " + AdsPolicy.GetHashCode());
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Type != null)
            {
                ret.Add("\"type\": " + "\"" + Type + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAssetFileContext
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(FullLifeCycle != null)
            {
                ret.Add("\"fullLifeCycle\": " + "\"" + FullLifeCycle + "\"");
            }
            ret.Add("\"isOfflinePlayBack\": " + IsOfflinePlayBack.ToString().ToLower());
            if(ViewLifeCycle != null)
            {
                ret.Add("\"viewLifeCycle\": " + "\"" + ViewLifeCycle + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaBillingResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(ExternalReceiptCode != null)
            {
                ret.Add("\"externalReceiptCode\": " + "\"" + ExternalReceiptCode + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"external_receipt_code\": " + "\"" + ExternalReceiptCode + "\"");
                }
            }
            if(ReceiptCode != null)
            {
                ret.Add("\"receiptCode\": " + "\"" + ReceiptCode + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"receipt_code\": " + "\"" + ReceiptCode + "\"");
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaBillingTransaction
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(actionDate.HasValue)
            {
                ret.Add("\"actionDate\": " + actionDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"action_date\": " + actionDate);
                }
            }
            ret.Add("\"billingAction\": " + billingAction.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"billing_action\": " + billingAction.GetHashCode());
            }
            ret.Add("\"billingPriceType\": " + billingPriceType.GetHashCode());
            if(billingProviderRef.HasValue)
            {
                ret.Add("\"billingProviderRef\": " + billingProviderRef);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"billing_provider_ref\": " + billingProviderRef);
                }
            }
            if(endDate.HasValue)
            {
                ret.Add("\"endDate\": " + endDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"end_date\": " + endDate);
                }
            }
            if(isRecurring.HasValue)
            {
                ret.Add("\"isRecurring\": " + isRecurring.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_recurring\": " + isRecurring.ToString().ToLower());
                }
            }
            ret.Add("\"itemType\": " + itemType.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"item_type\": " + itemType.GetHashCode());
            }
            ret.Add("\"paymentMethod\": " + paymentMethod.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"payment_method\": " + paymentMethod.GetHashCode());
            }
            if(paymentMethodExtraDetails != null)
            {
                ret.Add("\"paymentMethodExtraDetails\": " + "\"" + paymentMethodExtraDetails + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"payment_method_extra_details\": " + "\"" + paymentMethodExtraDetails + "\"");
                }
            }
            if(price != null)
            {
                propertyValue = price.ToJson(currentVersion, omitObsolete);
                ret.Add("\"price\": " + propertyValue);
            }
            if(purchasedItemCode != null)
            {
                ret.Add("\"purchasedItemCode\": " + "\"" + purchasedItemCode + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"purchased_item_code\": " + "\"" + purchasedItemCode + "\"");
                }
            }
            if(purchasedItemName != null)
            {
                ret.Add("\"purchasedItemName\": " + "\"" + purchasedItemName + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"purchased_item_name\": " + "\"" + purchasedItemName + "\"");
                }
            }
            if(purchaseID.HasValue)
            {
                ret.Add("\"purchaseId\": " + purchaseID);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"purchase_id\": " + purchaseID);
                }
            }
            if(recieptCode != null)
            {
                ret.Add("\"recieptCode\": " + "\"" + recieptCode + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"reciept_code\": " + "\"" + recieptCode + "\"");
                }
            }
            if(remarks != null)
            {
                ret.Add("\"remarks\": " + "\"" + remarks + "\"");
            }
            if(startDate.HasValue)
            {
                ret.Add("\"startDate\": " + startDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"start_date\": " + startDate);
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaBillingTransactionListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(transactions != null && transactions.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", transactions.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaCDVRAdapterProfile
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(AdapterUrl != null)
            {
                ret.Add("\"adapterUrl\": " + "\"" + AdapterUrl + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"adapter_url\": " + "\"" + AdapterUrl + "\"");
                }
            }
            if(DynamicLinksSupport.HasValue)
            {
                ret.Add("\"dynamicLinksSupport\": " + DynamicLinksSupport.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"dynamic_links_support\": " + DynamicLinksSupport.ToString().ToLower());
                }
            }
            if(ExternalIdentifier != null)
            {
                ret.Add("\"externalIdentifier\": " + "\"" + ExternalIdentifier + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"external_identifier\": " + "\"" + ExternalIdentifier + "\"");
                }
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(IsActive.HasValue)
            {
                ret.Add("\"isActive\": " + IsActive.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_active\": " + IsActive.ToString().ToLower());
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(Settings != null && Settings.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", Settings.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"settings\": " + propertyValue);
            }
            if(SharedSecret != null)
            {
                ret.Add("\"sharedSecret\": " + "\"" + SharedSecret + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"shared_secret\": " + "\"" + SharedSecret + "\"");
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaCDVRAdapterProfileListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaCollectionEntitlement
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(CurrentDate.HasValue)
            {
                ret.Add("\"currentDate\": " + CurrentDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"current_date\": " + CurrentDate);
                }
            }
            if(CurrentUses.HasValue)
            {
                ret.Add("\"currentUses\": " + CurrentUses);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"current_uses\": " + CurrentUses);
                }
            }
            if(DeviceName != null)
            {
                ret.Add("\"deviceName\": " + "\"" + DeviceName + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"device_name\": " + "\"" + DeviceName + "\"");
                }
            }
            if(DeviceUDID != null)
            {
                ret.Add("\"deviceUdid\": " + "\"" + DeviceUDID + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"device_udid\": " + "\"" + DeviceUDID + "\"");
                }
            }
            if(EndDate.HasValue)
            {
                ret.Add("\"endDate\": " + EndDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"end_date\": " + EndDate);
                }
            }
            if(!DeprecatedAttribute.IsDeprecated("4.8.0.0", currentVersion) && EntitlementId != null)
            {
                ret.Add("\"entitlementId\": " + "\"" + EntitlementId + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"entitlement_id\": " + "\"" + EntitlementId + "\"");
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
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_cancelation_window_enabled\": " + IsCancelationWindowEnabled.ToString().ToLower());
                }
            }
            if(!omitObsolete && IsInGracePeriod.HasValue)
            {
                ret.Add("\"isInGracePeriod\": " + IsInGracePeriod.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_in_grace_period\": " + IsInGracePeriod.ToString().ToLower());
                }
            }
            if(!omitObsolete && IsRenewable.HasValue)
            {
                ret.Add("\"isRenewable\": " + IsRenewable.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_renewable\": " + IsRenewable.ToString().ToLower());
                }
            }
            if(!omitObsolete && IsRenewableForPurchase.HasValue)
            {
                ret.Add("\"isRenewableForPurchase\": " + IsRenewableForPurchase.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_renewable_for_purchase\": " + IsRenewableForPurchase.ToString().ToLower());
                }
            }
            if(LastViewDate.HasValue)
            {
                ret.Add("\"lastViewDate\": " + LastViewDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"last_view_date\": " + LastViewDate);
                }
            }
            if(MaxUses.HasValue)
            {
                ret.Add("\"maxUses\": " + MaxUses);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"max_uses\": " + MaxUses);
                }
            }
            if(!omitObsolete && MediaFileId.HasValue)
            {
                ret.Add("\"mediaFileId\": " + MediaFileId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"media_file_id\": " + MediaFileId);
                }
            }
            if(!omitObsolete && MediaId.HasValue)
            {
                ret.Add("\"mediaId\": " + MediaId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"media_id\": " + MediaId);
                }
            }
            if(!omitObsolete && NextRenewalDate.HasValue)
            {
                ret.Add("\"nextRenewalDate\": " + NextRenewalDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"next_renewal_date\": " + NextRenewalDate);
                }
            }
            ret.Add("\"paymentMethod\": " + PaymentMethod.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"payment_method\": " + PaymentMethod.GetHashCode());
            }
            if(ProductId != null)
            {
                ret.Add("\"productId\": " + "\"" + ProductId + "\"");
            }
            if(PurchaseDate.HasValue)
            {
                ret.Add("\"purchaseDate\": " + PurchaseDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"purchase_date\": " + PurchaseDate);
                }
            }
            if(!omitObsolete && PurchaseId.HasValue)
            {
                ret.Add("\"purchaseId\": " + PurchaseId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"purchase_id\": " + PurchaseId);
                }
            }
            if(!omitObsolete)
            {
                ret.Add("\"type\": " + Type.GetHashCode());
            }
            if(UserId != null)
            {
                ret.Add("\"userId\": " + "\"" + UserId + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaCompensation
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"amount\": " + Amount);
            ret.Add("\"appliedRenewalIterations\": " + AppliedRenewalIterations);
            ret.Add("\"compensationType\": " + CompensationType.GetHashCode());
            ret.Add("\"id\": " + Id);
            ret.Add("\"purchaseId\": " + PurchaseId);
            ret.Add("\"subscriptionId\": " + SubscriptionId);
            ret.Add("\"totalRenewalIterations\": " + TotalRenewalIterations);
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaCustomDrmPlaybackPluginData
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(LicenseURL != null)
            {
                ret.Add("\"licenseURL\": " + "\"" + LicenseURL + "\"");
            }
            ret.Add("\"scheme\": " + Scheme.GetHashCode());
            if(Data != null)
            {
                ret.Add("\"data\": " + "\"" + Data + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaDrmPlaybackPluginData
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(LicenseURL != null)
            {
                ret.Add("\"licenseURL\": " + "\"" + LicenseURL + "\"");
            }
            ret.Add("\"scheme\": " + Scheme.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaEntitlement
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(CurrentDate.HasValue)
            {
                ret.Add("\"currentDate\": " + CurrentDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"current_date\": " + CurrentDate);
                }
            }
            if(CurrentUses.HasValue)
            {
                ret.Add("\"currentUses\": " + CurrentUses);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"current_uses\": " + CurrentUses);
                }
            }
            if(DeviceName != null)
            {
                ret.Add("\"deviceName\": " + "\"" + DeviceName + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"device_name\": " + "\"" + DeviceName + "\"");
                }
            }
            if(DeviceUDID != null)
            {
                ret.Add("\"deviceUdid\": " + "\"" + DeviceUDID + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"device_udid\": " + "\"" + DeviceUDID + "\"");
                }
            }
            if(EndDate.HasValue)
            {
                ret.Add("\"endDate\": " + EndDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"end_date\": " + EndDate);
                }
            }
            if(!DeprecatedAttribute.IsDeprecated("4.8.0.0", currentVersion) && EntitlementId != null)
            {
                ret.Add("\"entitlementId\": " + "\"" + EntitlementId + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"entitlement_id\": " + "\"" + EntitlementId + "\"");
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
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_cancelation_window_enabled\": " + IsCancelationWindowEnabled.ToString().ToLower());
                }
            }
            if(!omitObsolete && IsInGracePeriod.HasValue)
            {
                ret.Add("\"isInGracePeriod\": " + IsInGracePeriod.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_in_grace_period\": " + IsInGracePeriod.ToString().ToLower());
                }
            }
            if(!omitObsolete && IsRenewable.HasValue)
            {
                ret.Add("\"isRenewable\": " + IsRenewable.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_renewable\": " + IsRenewable.ToString().ToLower());
                }
            }
            if(!omitObsolete && IsRenewableForPurchase.HasValue)
            {
                ret.Add("\"isRenewableForPurchase\": " + IsRenewableForPurchase.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_renewable_for_purchase\": " + IsRenewableForPurchase.ToString().ToLower());
                }
            }
            if(LastViewDate.HasValue)
            {
                ret.Add("\"lastViewDate\": " + LastViewDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"last_view_date\": " + LastViewDate);
                }
            }
            if(MaxUses.HasValue)
            {
                ret.Add("\"maxUses\": " + MaxUses);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"max_uses\": " + MaxUses);
                }
            }
            if(!omitObsolete && MediaFileId.HasValue)
            {
                ret.Add("\"mediaFileId\": " + MediaFileId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"media_file_id\": " + MediaFileId);
                }
            }
            if(!omitObsolete && MediaId.HasValue)
            {
                ret.Add("\"mediaId\": " + MediaId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"media_id\": " + MediaId);
                }
            }
            if(!omitObsolete && NextRenewalDate.HasValue)
            {
                ret.Add("\"nextRenewalDate\": " + NextRenewalDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"next_renewal_date\": " + NextRenewalDate);
                }
            }
            ret.Add("\"paymentMethod\": " + PaymentMethod.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"payment_method\": " + PaymentMethod.GetHashCode());
            }
            if(ProductId != null)
            {
                ret.Add("\"productId\": " + "\"" + ProductId + "\"");
            }
            if(PurchaseDate.HasValue)
            {
                ret.Add("\"purchaseDate\": " + PurchaseDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"purchase_date\": " + PurchaseDate);
                }
            }
            if(!omitObsolete && PurchaseId.HasValue)
            {
                ret.Add("\"purchaseId\": " + PurchaseId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"purchase_id\": " + PurchaseId);
                }
            }
            if(!omitObsolete)
            {
                ret.Add("\"type\": " + Type.GetHashCode());
            }
            if(UserId != null)
            {
                ret.Add("\"userId\": " + "\"" + UserId + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaEntitlementCancellation
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"householdId\": " + HouseholdId);
            ret.Add("\"id\": " + Id);
            if(ProductId != null)
            {
                ret.Add("\"productId\": " + "\"" + ProductId + "\"");
            }
            if(!omitObsolete)
            {
                ret.Add("\"type\": " + Type.GetHashCode());
            }
            if(UserId != null)
            {
                ret.Add("\"userId\": " + "\"" + UserId + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaEntitlementFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(!DeprecatedAttribute.IsDeprecated("4.8.0.0", currentVersion) && EntitlementTypeEqual.HasValue)
            {
                ret.Add("\"entitlementTypeEqual\": " + EntitlementTypeEqual.GetHashCode());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"entitlement_type\": " + EntitlementTypeEqual.GetHashCode());
                }
            }
            ret.Add("\"entityReferenceEqual\": " + EntityReferenceEqual.GetHashCode());
            if(IsExpiredEqual.HasValue)
            {
                ret.Add("\"isExpiredEqual\": " + IsExpiredEqual.ToString().ToLower());
            }
            if(ProductTypeEqual.HasValue)
            {
                ret.Add("\"productTypeEqual\": " + ProductTypeEqual.GetHashCode());
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaEntitlementListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Entitlements != null && Entitlements.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Entitlements.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaEntitlementRenewal
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"date\": " + Date);
            if(Price != null)
            {
                propertyValue = Price.ToJson(currentVersion, omitObsolete);
                ret.Add("\"price\": " + propertyValue);
            }
            ret.Add("\"purchaseId\": " + PurchaseId);
            ret.Add("\"subscriptionId\": " + SubscriptionId);
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaEntitlementRenewalBase
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"price\": " + Price);
            ret.Add("\"purchaseId\": " + PurchaseId);
            ret.Add("\"subscriptionId\": " + SubscriptionId);
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaEntitlementsFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"by\": " + By.GetHashCode());
            ret.Add("\"entitlementType\": " + EntitlementType.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"entitlement_type\": " + EntitlementType.GetHashCode());
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaExternalReceipt
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(ContentId.HasValue)
            {
                ret.Add("\"contentId\": " + ContentId);
            }
            ret.Add("\"productId\": " + ProductId);
            ret.Add("\"productType\": " + ProductType.GetHashCode());
            if(PaymentGatewayName != null)
            {
                ret.Add("\"paymentGatewayName\": " + "\"" + PaymentGatewayName + "\"");
            }
            if(ReceiptId != null)
            {
                ret.Add("\"receiptId\": " + "\"" + ReceiptId + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaFairPlayPlaybackPluginData
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(LicenseURL != null)
            {
                ret.Add("\"licenseURL\": " + "\"" + LicenseURL + "\"");
            }
            ret.Add("\"scheme\": " + Scheme.GetHashCode());
            if(Certificate != null)
            {
                ret.Add("\"certificate\": " + "\"" + Certificate + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaHouseholdPremiumService
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaHouseholdPremiumServiceListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(PremiumServices != null && PremiumServices.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", PremiumServices.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaHouseholdQuota
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"availableQuota\": " + AvailableQuota);
            ret.Add("\"householdId\": " + HouseholdId);
            ret.Add("\"totalQuota\": " + TotalQuota);
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaLicensedUrl
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(AltUrl != null)
            {
                ret.Add("\"altUrl\": " + "\"" + AltUrl + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"alt_url\": " + "\"" + AltUrl + "\"");
                }
            }
            if(MainUrl != null)
            {
                ret.Add("\"mainUrl\": " + "\"" + MainUrl + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"main_url\": " + "\"" + MainUrl + "\"");
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaLicensedUrlBaseRequest
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(AssetId != null)
            {
                ret.Add("\"assetId\": " + "\"" + AssetId + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaLicensedUrlEpgRequest
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(AssetId != null)
            {
                ret.Add("\"assetId\": " + "\"" + AssetId + "\"");
            }
            if(BaseUrl != null)
            {
                ret.Add("\"baseUrl\": " + "\"" + BaseUrl + "\"");
            }
            ret.Add("\"contentId\": " + ContentId);
            ret.Add("\"startDate\": " + StartDate);
            ret.Add("\"streamType\": " + StreamType.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaLicensedUrlMediaRequest
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(AssetId != null)
            {
                ret.Add("\"assetId\": " + "\"" + AssetId + "\"");
            }
            if(BaseUrl != null)
            {
                ret.Add("\"baseUrl\": " + "\"" + BaseUrl + "\"");
            }
            ret.Add("\"contentId\": " + ContentId);
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaLicensedUrlRecordingRequest
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(AssetId != null)
            {
                ret.Add("\"assetId\": " + "\"" + AssetId + "\"");
            }
            if(FileType != null)
            {
                ret.Add("\"fileType\": " + "\"" + FileType + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaNpvrPremiumService
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(QuotaInMinutes.HasValue)
            {
                ret.Add("\"quotaInMinutes\": " + QuotaInMinutes);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPlaybackContext
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Actions != null && Actions.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Actions.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"actions\": " + propertyValue);
            }
            if(Messages != null && Messages.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Messages.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"messages\": " + propertyValue);
            }
            if(Sources != null && Sources.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Sources.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"sources\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPlaybackContextOptions
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(AssetFileIds != null)
            {
                ret.Add("\"assetFileIds\": " + "\"" + AssetFileIds + "\"");
            }
            if(Context.HasValue)
            {
                ret.Add("\"context\": " + Context.GetHashCode());
            }
            if(MediaProtocol != null)
            {
                ret.Add("\"mediaProtocol\": " + "\"" + MediaProtocol + "\"");
            }
            if(StreamerType != null)
            {
                ret.Add("\"streamerType\": " + "\"" + StreamerType + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPlaybackSource
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(AltCdnCode != null)
            {
                ret.Add("\"altCdnCode\": " + "\"" + AltCdnCode + "\"");
            }
            if(AssetId.HasValue)
            {
                ret.Add("\"assetId\": " + AssetId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"asset_id\": " + AssetId);
                }
            }
            if(BillingType != null)
            {
                ret.Add("\"billingType\": " + "\"" + BillingType + "\"");
            }
            if(CdnCode != null)
            {
                ret.Add("\"cdnCode\": " + "\"" + CdnCode + "\"");
            }
            if(CdnName != null)
            {
                ret.Add("\"cdnName\": " + "\"" + CdnName + "\"");
            }
            if(Duration.HasValue)
            {
                ret.Add("\"duration\": " + Duration);
            }
            if(ExternalId != null)
            {
                ret.Add("\"externalId\": " + "\"" + ExternalId + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"external_id\": " + "\"" + ExternalId + "\"");
                }
            }
            ret.Add("\"fileSize\": " + FileSize);
            if(HandlingType != null)
            {
                ret.Add("\"handlingType\": " + "\"" + HandlingType + "\"");
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(PPVModules != null)
            {
                propertyValue = PPVModules.ToJson(currentVersion, omitObsolete);
                ret.Add("\"ppvModules\": " + propertyValue);
            }
            if(ProductCode != null)
            {
                ret.Add("\"productCode\": " + "\"" + ProductCode + "\"");
            }
            if(Quality != null)
            {
                ret.Add("\"quality\": " + "\"" + Quality + "\"");
            }
            if(Type != null)
            {
                ret.Add("\"type\": " + "\"" + Type + "\"");
            }
            if(Url != null)
            {
                ret.Add("\"url\": " + "\"" + Url + "\"");
            }
            if(!DeprecatedAttribute.IsDeprecated("4.6.0.0", currentVersion) && AdsParams != null)
            {
                ret.Add("\"adsParam\": " + "\"" + AdsParams + "\"");
            }
            if(!DeprecatedAttribute.IsDeprecated("4.6.0.0", currentVersion) && AdsPolicy.HasValue)
            {
                ret.Add("\"adsPolicy\": " + AdsPolicy.GetHashCode());
            }
            if(Drm != null && Drm.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Drm.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"drm\": " + propertyValue);
            }
            if(Format != null)
            {
                ret.Add("\"format\": " + "\"" + Format + "\"");
            }
            if(Protocols != null)
            {
                ret.Add("\"protocols\": " + "\"" + Protocols + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPluginData
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPpvEntitlement
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(CurrentDate.HasValue)
            {
                ret.Add("\"currentDate\": " + CurrentDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"current_date\": " + CurrentDate);
                }
            }
            if(CurrentUses.HasValue)
            {
                ret.Add("\"currentUses\": " + CurrentUses);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"current_uses\": " + CurrentUses);
                }
            }
            if(DeviceName != null)
            {
                ret.Add("\"deviceName\": " + "\"" + DeviceName + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"device_name\": " + "\"" + DeviceName + "\"");
                }
            }
            if(DeviceUDID != null)
            {
                ret.Add("\"deviceUdid\": " + "\"" + DeviceUDID + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"device_udid\": " + "\"" + DeviceUDID + "\"");
                }
            }
            if(EndDate.HasValue)
            {
                ret.Add("\"endDate\": " + EndDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"end_date\": " + EndDate);
                }
            }
            if(!DeprecatedAttribute.IsDeprecated("4.8.0.0", currentVersion) && EntitlementId != null)
            {
                ret.Add("\"entitlementId\": " + "\"" + EntitlementId + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"entitlement_id\": " + "\"" + EntitlementId + "\"");
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
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_cancelation_window_enabled\": " + IsCancelationWindowEnabled.ToString().ToLower());
                }
            }
            if(!omitObsolete && IsInGracePeriod.HasValue)
            {
                ret.Add("\"isInGracePeriod\": " + IsInGracePeriod.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_in_grace_period\": " + IsInGracePeriod.ToString().ToLower());
                }
            }
            if(!omitObsolete && IsRenewable.HasValue)
            {
                ret.Add("\"isRenewable\": " + IsRenewable.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_renewable\": " + IsRenewable.ToString().ToLower());
                }
            }
            if(!omitObsolete && IsRenewableForPurchase.HasValue)
            {
                ret.Add("\"isRenewableForPurchase\": " + IsRenewableForPurchase.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_renewable_for_purchase\": " + IsRenewableForPurchase.ToString().ToLower());
                }
            }
            if(LastViewDate.HasValue)
            {
                ret.Add("\"lastViewDate\": " + LastViewDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"last_view_date\": " + LastViewDate);
                }
            }
            if(MaxUses.HasValue)
            {
                ret.Add("\"maxUses\": " + MaxUses);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"max_uses\": " + MaxUses);
                }
            }
            if(!omitObsolete && MediaFileId.HasValue)
            {
                ret.Add("\"mediaFileId\": " + MediaFileId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"media_file_id\": " + MediaFileId);
                }
            }
            if(!omitObsolete && MediaId.HasValue)
            {
                ret.Add("\"mediaId\": " + MediaId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"media_id\": " + MediaId);
                }
            }
            if(!omitObsolete && NextRenewalDate.HasValue)
            {
                ret.Add("\"nextRenewalDate\": " + NextRenewalDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"next_renewal_date\": " + NextRenewalDate);
                }
            }
            ret.Add("\"paymentMethod\": " + PaymentMethod.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"payment_method\": " + PaymentMethod.GetHashCode());
            }
            if(ProductId != null)
            {
                ret.Add("\"productId\": " + "\"" + ProductId + "\"");
            }
            if(PurchaseDate.HasValue)
            {
                ret.Add("\"purchaseDate\": " + PurchaseDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"purchase_date\": " + PurchaseDate);
                }
            }
            if(!omitObsolete && PurchaseId.HasValue)
            {
                ret.Add("\"purchaseId\": " + PurchaseId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"purchase_id\": " + PurchaseId);
                }
            }
            if(!omitObsolete)
            {
                ret.Add("\"type\": " + Type.GetHashCode());
            }
            if(UserId != null)
            {
                ret.Add("\"userId\": " + "\"" + UserId + "\"");
            }
            if(MediaFileId.HasValue)
            {
                ret.Add("\"mediaFileId\": " + MediaFileId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"media_file_id\": " + MediaFileId);
                }
            }
            if(MediaId.HasValue)
            {
                ret.Add("\"mediaId\": " + MediaId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"media_id\": " + MediaId);
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPremiumService
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPricesFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(FilesIds != null && FilesIds.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", FilesIds.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"filesIds\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"files_ids\": " + propertyValue);
                }
            }
            if(ShouldGetOnlyLowest.HasValue)
            {
                ret.Add("\"shouldGetOnlyLowest\": " + ShouldGetOnlyLowest.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"should_get_only_lowest\": " + ShouldGetOnlyLowest.ToString().ToLower());
                }
            }
            if(SubscriptionsIds != null && SubscriptionsIds.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", SubscriptionsIds.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"subscriptionsIds\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"subscriptions_ids\": " + propertyValue);
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaProductPriceFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(CollectionIdIn != null)
            {
                ret.Add("\"collectionIdIn\": " + "\"" + CollectionIdIn + "\"");
            }
            if(CouponCodeEqual != null)
            {
                ret.Add("\"couponCodeEqual\": " + "\"" + CouponCodeEqual + "\"");
            }
            if(FileIdIn != null)
            {
                ret.Add("\"fileIdIn\": " + "\"" + FileIdIn + "\"");
            }
            if(isLowest.HasValue)
            {
                ret.Add("\"isLowest\": " + isLowest.ToString().ToLower());
            }
            if(SubscriptionIdIn != null)
            {
                ret.Add("\"subscriptionIdIn\": " + "\"" + SubscriptionIdIn + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPurchase
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(ContentId.HasValue)
            {
                ret.Add("\"contentId\": " + ContentId);
            }
            ret.Add("\"productId\": " + ProductId);
            ret.Add("\"productType\": " + ProductType.GetHashCode());
            if(AdapterData != null)
            {
                ret.Add("\"adapterData\": " + "\"" + AdapterData + "\"");
            }
            if(Coupon != null)
            {
                ret.Add("\"coupon\": " + "\"" + Coupon + "\"");
            }
            if(Currency != null)
            {
                ret.Add("\"currency\": " + "\"" + Currency + "\"");
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
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPurchaseBase
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(ContentId.HasValue)
            {
                ret.Add("\"contentId\": " + ContentId);
            }
            ret.Add("\"productId\": " + ProductId);
            ret.Add("\"productType\": " + ProductType.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPurchaseSession
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(ContentId.HasValue)
            {
                ret.Add("\"contentId\": " + ContentId);
            }
            ret.Add("\"productId\": " + ProductId);
            ret.Add("\"productType\": " + ProductType.GetHashCode());
            if(AdapterData != null)
            {
                ret.Add("\"adapterData\": " + "\"" + AdapterData + "\"");
            }
            if(Coupon != null)
            {
                ret.Add("\"coupon\": " + "\"" + Coupon + "\"");
            }
            if(Currency != null)
            {
                ret.Add("\"currency\": " + "\"" + Currency + "\"");
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
            if(PreviewModuleId.HasValue)
            {
                ret.Add("\"previewModuleId\": " + PreviewModuleId);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaRecording
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"assetId\": " + AssetId);
            ret.Add("\"createDate\": " + CreateDate);
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            ret.Add("\"isProtected\": " + IsProtected.ToString().ToLower());
            ret.Add("\"status\": " + Status.GetHashCode());
            ret.Add("\"type\": " + Type.GetHashCode());
            ret.Add("\"updateDate\": " + UpdateDate);
            if(ViewableUntilDate.HasValue)
            {
                ret.Add("\"viewableUntilDate\": " + ViewableUntilDate);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaRecordingContext
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"assetId\": " + AssetId);
            ret.Add("\"code\": " + Code);
            if(Message != null)
            {
                ret.Add("\"message\": " + "\"" + Message + "\"");
            }
            if(Recording != null)
            {
                propertyValue = Recording.ToJson(currentVersion, omitObsolete);
                ret.Add("\"recording\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaRecordingContextFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(AssetIdIn != null)
            {
                ret.Add("\"assetIdIn\": " + "\"" + AssetIdIn + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaRecordingContextListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaRecordingFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(FilterExpression != null)
            {
                ret.Add("\"filterExpression\": " + "\"" + FilterExpression + "\"");
            }
            if(StatusIn != null)
            {
                ret.Add("\"statusIn\": " + "\"" + StatusIn + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaRecordingListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaRuleAction
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"type\": " + Type.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSeriesRecording
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"channelId\": " + ChannelId);
            ret.Add("\"createDate\": " + CreateDate);
            ret.Add("\"epgId\": " + EpgId);
            if(ExcludedSeasons != null && ExcludedSeasons.Count > 0)
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
                ret.Add("\"seriesId\": " + "\"" + SeriesId + "\"");
            }
            ret.Add("\"type\": " + Type.GetHashCode());
            ret.Add("\"updateDate\": " + UpdateDate);
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSeriesRecordingFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSeriesRecordingListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSubscriptionEntitlement
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(CurrentDate.HasValue)
            {
                ret.Add("\"currentDate\": " + CurrentDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"current_date\": " + CurrentDate);
                }
            }
            if(CurrentUses.HasValue)
            {
                ret.Add("\"currentUses\": " + CurrentUses);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"current_uses\": " + CurrentUses);
                }
            }
            if(DeviceName != null)
            {
                ret.Add("\"deviceName\": " + "\"" + DeviceName + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"device_name\": " + "\"" + DeviceName + "\"");
                }
            }
            if(DeviceUDID != null)
            {
                ret.Add("\"deviceUdid\": " + "\"" + DeviceUDID + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"device_udid\": " + "\"" + DeviceUDID + "\"");
                }
            }
            if(EndDate.HasValue)
            {
                ret.Add("\"endDate\": " + EndDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"end_date\": " + EndDate);
                }
            }
            if(!DeprecatedAttribute.IsDeprecated("4.8.0.0", currentVersion) && EntitlementId != null)
            {
                ret.Add("\"entitlementId\": " + "\"" + EntitlementId + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"entitlement_id\": " + "\"" + EntitlementId + "\"");
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
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_cancelation_window_enabled\": " + IsCancelationWindowEnabled.ToString().ToLower());
                }
            }
            if(!omitObsolete && IsInGracePeriod.HasValue)
            {
                ret.Add("\"isInGracePeriod\": " + IsInGracePeriod.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_in_grace_period\": " + IsInGracePeriod.ToString().ToLower());
                }
            }
            if(!omitObsolete && IsRenewable.HasValue)
            {
                ret.Add("\"isRenewable\": " + IsRenewable.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_renewable\": " + IsRenewable.ToString().ToLower());
                }
            }
            if(!omitObsolete && IsRenewableForPurchase.HasValue)
            {
                ret.Add("\"isRenewableForPurchase\": " + IsRenewableForPurchase.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_renewable_for_purchase\": " + IsRenewableForPurchase.ToString().ToLower());
                }
            }
            if(LastViewDate.HasValue)
            {
                ret.Add("\"lastViewDate\": " + LastViewDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"last_view_date\": " + LastViewDate);
                }
            }
            if(MaxUses.HasValue)
            {
                ret.Add("\"maxUses\": " + MaxUses);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"max_uses\": " + MaxUses);
                }
            }
            if(!omitObsolete && MediaFileId.HasValue)
            {
                ret.Add("\"mediaFileId\": " + MediaFileId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"media_file_id\": " + MediaFileId);
                }
            }
            if(!omitObsolete && MediaId.HasValue)
            {
                ret.Add("\"mediaId\": " + MediaId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"media_id\": " + MediaId);
                }
            }
            if(!omitObsolete && NextRenewalDate.HasValue)
            {
                ret.Add("\"nextRenewalDate\": " + NextRenewalDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"next_renewal_date\": " + NextRenewalDate);
                }
            }
            ret.Add("\"paymentMethod\": " + PaymentMethod.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"payment_method\": " + PaymentMethod.GetHashCode());
            }
            if(ProductId != null)
            {
                ret.Add("\"productId\": " + "\"" + ProductId + "\"");
            }
            if(PurchaseDate.HasValue)
            {
                ret.Add("\"purchaseDate\": " + PurchaseDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"purchase_date\": " + PurchaseDate);
                }
            }
            if(!omitObsolete && PurchaseId.HasValue)
            {
                ret.Add("\"purchaseId\": " + PurchaseId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"purchase_id\": " + PurchaseId);
                }
            }
            if(!omitObsolete)
            {
                ret.Add("\"type\": " + Type.GetHashCode());
            }
            if(UserId != null)
            {
                ret.Add("\"userId\": " + "\"" + UserId + "\"");
            }
            if(IsInGracePeriod.HasValue)
            {
                ret.Add("\"isInGracePeriod\": " + IsInGracePeriod.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_in_grace_period\": " + IsInGracePeriod.ToString().ToLower());
                }
            }
            if(IsRenewable.HasValue)
            {
                ret.Add("\"isRenewable\": " + IsRenewable.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_renewable\": " + IsRenewable.ToString().ToLower());
                }
            }
            if(IsRenewableForPurchase.HasValue)
            {
                ret.Add("\"isRenewableForPurchase\": " + IsRenewableForPurchase.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_renewable_for_purchase\": " + IsRenewableForPurchase.ToString().ToLower());
                }
            }
            ret.Add("\"isSuspended\": " + IsSuspended.ToString().ToLower());
            if(NextRenewalDate.HasValue)
            {
                ret.Add("\"nextRenewalDate\": " + NextRenewalDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
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
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaTransaction
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(CreatedAt.HasValue)
            {
                ret.Add("\"createdAt\": " + CreatedAt);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"created_at\": " + CreatedAt);
                }
            }
            if(FailReasonCode.HasValue)
            {
                ret.Add("\"failReasonCode\": " + FailReasonCode);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"fail_reason_code\": " + FailReasonCode);
                }
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + Id + "\"");
            }
            if(PGReferenceID != null)
            {
                ret.Add("\"paymentGatewayReferenceId\": " + "\"" + PGReferenceID + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"payment_gateway_reference_id\": " + "\"" + PGReferenceID + "\"");
                }
            }
            if(PGResponseID != null)
            {
                ret.Add("\"paymentGatewayResponseId\": " + "\"" + PGResponseID + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"payment_gateway_response_id\": " + "\"" + PGResponseID + "\"");
                }
            }
            if(State != null)
            {
                ret.Add("\"state\": " + "\"" + State + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaTransactionHistoryFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(EndDateLessThanOrEqual.HasValue)
            {
                ret.Add("\"endDateLessThanOrEqual\": " + EndDateLessThanOrEqual);
            }
            ret.Add("\"entityReferenceEqual\": " + EntityReferenceEqual.GetHashCode());
            if(StartDateGreaterThanOrEqual.HasValue)
            {
                ret.Add("\"startDateGreaterThanOrEqual\": " + StartDateGreaterThanOrEqual);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaTransactionsFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(PageIndex.HasValue)
            {
                ret.Add("\"pageIndex\": " + PageIndex);
            }
            if(PageSize.HasValue)
            {
                ret.Add("\"pageSize\": " + PageSize);
            }
            ret.Add("\"by\": " + By.GetHashCode());
            if(EndDate.HasValue)
            {
                ret.Add("\"endDate\": " + EndDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"end_date\": " + EndDate);
                }
            }
            if(StartDate.HasValue)
            {
                ret.Add("\"startDate\": " + StartDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"start_date\": " + StartDate);
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaTransactionStatus
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"adapterTransactionStatus\": " + AdapterStatus.GetHashCode());
            if(ExternalId != null)
            {
                ret.Add("\"externalId\": " + "\"" + ExternalId + "\"");
            }
            if(ExternalMessage != null)
            {
                ret.Add("\"externalMessage\": " + "\"" + ExternalMessage + "\"");
            }
            if(ExternalStatus != null)
            {
                ret.Add("\"externalStatus\": " + "\"" + ExternalStatus + "\"");
            }
            ret.Add("\"failReason\": " + FailReason);
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaUnifiedPaymentRenewal
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"date\": " + Date);
            if(Entitlements != null && Entitlements.Count > 0)
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
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaUserBillingTransaction
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(actionDate.HasValue)
            {
                ret.Add("\"actionDate\": " + actionDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"action_date\": " + actionDate);
                }
            }
            ret.Add("\"billingAction\": " + billingAction.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"billing_action\": " + billingAction.GetHashCode());
            }
            ret.Add("\"billingPriceType\": " + billingPriceType.GetHashCode());
            if(billingProviderRef.HasValue)
            {
                ret.Add("\"billingProviderRef\": " + billingProviderRef);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"billing_provider_ref\": " + billingProviderRef);
                }
            }
            if(endDate.HasValue)
            {
                ret.Add("\"endDate\": " + endDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"end_date\": " + endDate);
                }
            }
            if(isRecurring.HasValue)
            {
                ret.Add("\"isRecurring\": " + isRecurring.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_recurring\": " + isRecurring.ToString().ToLower());
                }
            }
            ret.Add("\"itemType\": " + itemType.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"item_type\": " + itemType.GetHashCode());
            }
            ret.Add("\"paymentMethod\": " + paymentMethod.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"payment_method\": " + paymentMethod.GetHashCode());
            }
            if(paymentMethodExtraDetails != null)
            {
                ret.Add("\"paymentMethodExtraDetails\": " + "\"" + paymentMethodExtraDetails + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"payment_method_extra_details\": " + "\"" + paymentMethodExtraDetails + "\"");
                }
            }
            if(price != null)
            {
                propertyValue = price.ToJson(currentVersion, omitObsolete);
                ret.Add("\"price\": " + propertyValue);
            }
            if(purchasedItemCode != null)
            {
                ret.Add("\"purchasedItemCode\": " + "\"" + purchasedItemCode + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"purchased_item_code\": " + "\"" + purchasedItemCode + "\"");
                }
            }
            if(purchasedItemName != null)
            {
                ret.Add("\"purchasedItemName\": " + "\"" + purchasedItemName + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"purchased_item_name\": " + "\"" + purchasedItemName + "\"");
                }
            }
            if(purchaseID.HasValue)
            {
                ret.Add("\"purchaseId\": " + purchaseID);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"purchase_id\": " + purchaseID);
                }
            }
            if(recieptCode != null)
            {
                ret.Add("\"recieptCode\": " + "\"" + recieptCode + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"reciept_code\": " + "\"" + recieptCode + "\"");
                }
            }
            if(remarks != null)
            {
                ret.Add("\"remarks\": " + "\"" + remarks + "\"");
            }
            if(startDate.HasValue)
            {
                ret.Add("\"startDate\": " + startDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"start_date\": " + startDate);
                }
            }
            if(UserFullName != null)
            {
                ret.Add("\"userFullName\": " + "\"" + UserFullName + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"user_full_name\": " + "\"" + UserFullName + "\"");
                }
            }
            if(UserID != null)
            {
                ret.Add("\"userId\": " + "\"" + UserID + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"user_id\": " + "\"" + UserID + "\"");
                }
            }
            return String.Join(", ", ret);
        }
    }
}

namespace WebAPI.Models.Social
{
    public partial class KalturaActionPermissionItem
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Action != null)
            {
                ret.Add("\"action\": " + "\"" + Action + "\"");
            }
            ret.Add("\"actionPrivacy\": " + ActionPrivacy.GetHashCode());
            if(Network.HasValue)
            {
                ret.Add("\"network\": " + Network.GetHashCode());
            }
            ret.Add("\"privacy\": " + Privacy.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaFacebookPost
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"createDate\": " + CreateDate);
            if(Header != null)
            {
                ret.Add("\"header\": " + "\"" + Header + "\"");
            }
            if(Text != null)
            {
                ret.Add("\"text\": " + "\"" + Text + "\"");
            }
            if(Writer != null)
            {
                ret.Add("\"writer\": " + "\"" + Writer + "\"");
            }
            if(AuthorImageUrl != null)
            {
                ret.Add("\"authorImageUrl\": " + "\"" + AuthorImageUrl + "\"");
            }
            if(LikeCounter != null)
            {
                ret.Add("\"likeCounter\": " + "\"" + LikeCounter + "\"");
            }
            if(Comments != null && Comments.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Comments.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"comments\": " + propertyValue);
            }
            if(Link != null)
            {
                ret.Add("\"link\": " + "\"" + Link + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaFacebookSocial
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Birthday != null)
            {
                ret.Add("\"birthday\": " + "\"" + Birthday + "\"");
            }
            if(Email != null)
            {
                ret.Add("\"email\": " + "\"" + Email + "\"");
            }
            if(FirstName != null)
            {
                ret.Add("\"firstName\": " + "\"" + FirstName + "\"");
            }
            if(Gender != null)
            {
                ret.Add("\"gender\": " + "\"" + Gender + "\"");
            }
            if(ID != null)
            {
                ret.Add("\"id\": " + "\"" + ID + "\"");
            }
            if(LastName != null)
            {
                ret.Add("\"lastName\": " + "\"" + LastName + "\"");
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(PictureUrl != null)
            {
                ret.Add("\"pictureUrl\": " + "\"" + PictureUrl + "\"");
            }
            if(Status != null)
            {
                ret.Add("\"status\": " + "\"" + Status + "\"");
            }
            if(UserId != null)
            {
                ret.Add("\"userId\": " + "\"" + UserId + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaNetworkActionStatus
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Network.HasValue)
            {
                ret.Add("\"network\": " + Network.GetHashCode());
            }
            ret.Add("\"status\": " + Status.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSocial
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Birthday != null)
            {
                ret.Add("\"birthday\": " + "\"" + Birthday + "\"");
            }
            if(Email != null)
            {
                ret.Add("\"email\": " + "\"" + Email + "\"");
            }
            if(FirstName != null)
            {
                ret.Add("\"firstName\": " + "\"" + FirstName + "\"");
            }
            if(Gender != null)
            {
                ret.Add("\"gender\": " + "\"" + Gender + "\"");
            }
            if(ID != null)
            {
                ret.Add("\"id\": " + "\"" + ID + "\"");
            }
            if(LastName != null)
            {
                ret.Add("\"lastName\": " + "\"" + LastName + "\"");
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(PictureUrl != null)
            {
                ret.Add("\"pictureUrl\": " + "\"" + PictureUrl + "\"");
            }
            if(Status != null)
            {
                ret.Add("\"status\": " + "\"" + Status + "\"");
            }
            if(UserId != null)
            {
                ret.Add("\"userId\": " + "\"" + UserId + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSocialAction
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"actionType\": " + ActionType.GetHashCode());
            if(AssetId.HasValue)
            {
                ret.Add("\"assetId\": " + AssetId);
            }
            ret.Add("\"assetType\": " + AssetType.GetHashCode());
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + Id + "\"");
            }
            if(Time.HasValue)
            {
                ret.Add("\"time\": " + Time);
            }
            if(Url != null)
            {
                ret.Add("\"url\": " + "\"" + Url + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSocialActionFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(ActionTypeIn != null)
            {
                ret.Add("\"actionTypeIn\": " + "\"" + ActionTypeIn + "\"");
            }
            if(AssetIdIn != null)
            {
                ret.Add("\"assetIdIn\": " + "\"" + AssetIdIn + "\"");
            }
            ret.Add("\"assetTypeEqual\": " + AssetTypeEqual.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSocialActionListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSocialActionRate
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"actionType\": " + ActionType.GetHashCode());
            if(AssetId.HasValue)
            {
                ret.Add("\"assetId\": " + AssetId);
            }
            ret.Add("\"assetType\": " + AssetType.GetHashCode());
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + Id + "\"");
            }
            if(Time.HasValue)
            {
                ret.Add("\"time\": " + Time);
            }
            if(Url != null)
            {
                ret.Add("\"url\": " + "\"" + Url + "\"");
            }
            ret.Add("\"rate\": " + Rate);
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSocialComment
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"createDate\": " + CreateDate);
            if(Header != null)
            {
                ret.Add("\"header\": " + "\"" + Header + "\"");
            }
            if(Text != null)
            {
                ret.Add("\"text\": " + "\"" + Text + "\"");
            }
            if(Writer != null)
            {
                ret.Add("\"writer\": " + "\"" + Writer + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSocialCommentFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            ret.Add("\"assetIdEqual\": " + AssetIdEqual);
            ret.Add("\"assetTypeEqual\": " + AssetTypeEqual.GetHashCode());
            ret.Add("\"createDateGreaterThan\": " + CreateDateGreaterThan);
            ret.Add("\"socialPlatformEqual\": " + SocialPlatformEqual.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSocialCommentListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSocialConfig
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSocialFacebookConfig
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(AppId != null)
            {
                ret.Add("\"appId\": " + "\"" + AppId + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"app_id\": " + "\"" + AppId + "\"");
                }
            }
            if(Permissions != null)
            {
                ret.Add("\"permissions\": " + "\"" + Permissions + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSocialFriendActivity
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(SocialAction != null)
            {
                propertyValue = SocialAction.ToJson(currentVersion, omitObsolete);
                ret.Add("\"socialAction\": " + propertyValue);
            }
            if(UserFullName != null)
            {
                ret.Add("\"userFullName\": " + "\"" + UserFullName + "\"");
            }
            if(UserPictureUrl != null)
            {
                ret.Add("\"userPictureUrl\": " + "\"" + UserPictureUrl + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSocialFriendActivityFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(ActionTypeIn != null)
            {
                ret.Add("\"actionTypeIn\": " + "\"" + ActionTypeIn + "\"");
            }
            if(AssetIdEqual.HasValue)
            {
                ret.Add("\"assetIdEqual\": " + AssetIdEqual);
            }
            if(AssetTypeEqual.HasValue)
            {
                ret.Add("\"assetTypeEqual\": " + AssetTypeEqual.GetHashCode());
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSocialFriendActivityListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSocialNetworkComment
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"createDate\": " + CreateDate);
            if(Header != null)
            {
                ret.Add("\"header\": " + "\"" + Header + "\"");
            }
            if(Text != null)
            {
                ret.Add("\"text\": " + "\"" + Text + "\"");
            }
            if(Writer != null)
            {
                ret.Add("\"writer\": " + "\"" + Writer + "\"");
            }
            if(AuthorImageUrl != null)
            {
                ret.Add("\"authorImageUrl\": " + "\"" + AuthorImageUrl + "\"");
            }
            if(LikeCounter != null)
            {
                ret.Add("\"likeCounter\": " + "\"" + LikeCounter + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSocialResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Data != null)
            {
                ret.Add("\"data\": " + "\"" + Data + "\"");
            }
            if(KalturaName != null)
            {
                ret.Add("\"kalturaUsername\": " + "\"" + KalturaName + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"kaltura_username\": " + "\"" + KalturaName + "\"");
                }
            }
            if(MinFriends != null)
            {
                ret.Add("\"minFriendsLimitation\": " + "\"" + MinFriends + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"min_friends_limitation\": " + "\"" + MinFriends + "\"");
                }
            }
            if(Pic != null)
            {
                ret.Add("\"pic\": " + "\"" + Pic + "\"");
            }
            if(SocialNetworkUsername != null)
            {
                ret.Add("\"socialUsername\": " + "\"" + SocialNetworkUsername + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"social_username\": " + "\"" + SocialNetworkUsername + "\"");
                }
            }
            if(SocialUser != null)
            {
                propertyValue = SocialUser.ToJson(currentVersion, omitObsolete);
                ret.Add("\"socialUser\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"social_user\": " + propertyValue);
                }
            }
            if(Status != null)
            {
                ret.Add("\"status\": " + "\"" + Status + "\"");
            }
            if(Token != null)
            {
                ret.Add("\"token\": " + "\"" + Token + "\"");
            }
            if(UserId != null)
            {
                ret.Add("\"userId\": " + "\"" + UserId + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"user_id\": " + "\"" + UserId + "\"");
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSocialUser
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Birthday != null)
            {
                ret.Add("\"birthday\": " + "\"" + Birthday + "\"");
            }
            if(Email != null)
            {
                ret.Add("\"email\": " + "\"" + Email + "\"");
            }
            if(FirstName != null)
            {
                ret.Add("\"firstName\": " + "\"" + FirstName + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"first_name\": " + "\"" + FirstName + "\"");
                }
            }
            if(Gender != null)
            {
                ret.Add("\"gender\": " + "\"" + Gender + "\"");
            }
            if(ID != null)
            {
                ret.Add("\"id\": " + "\"" + ID + "\"");
            }
            if(LastName != null)
            {
                ret.Add("\"lastName\": " + "\"" + LastName + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"last_name\": " + "\"" + LastName + "\"");
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(UserId != null)
            {
                ret.Add("\"userId\": " + "\"" + UserId + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"user_id\": " + "\"" + UserId + "\"");
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSocialUserConfig
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(PermissionItems != null && PermissionItems.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", PermissionItems.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"actionPermissionItems\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaTwitterTwit
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"createDate\": " + CreateDate);
            if(Header != null)
            {
                ret.Add("\"header\": " + "\"" + Header + "\"");
            }
            if(Text != null)
            {
                ret.Add("\"text\": " + "\"" + Text + "\"");
            }
            if(Writer != null)
            {
                ret.Add("\"writer\": " + "\"" + Writer + "\"");
            }
            if(AuthorImageUrl != null)
            {
                ret.Add("\"authorImageUrl\": " + "\"" + AuthorImageUrl + "\"");
            }
            if(LikeCounter != null)
            {
                ret.Add("\"likeCounter\": " + "\"" + LikeCounter + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaUserSocialActionResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(NetworkStatus != null && NetworkStatus.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", NetworkStatus.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"failStatus\": " + propertyValue);
            }
            if(SocialAction != null)
            {
                propertyValue = SocialAction.ToJson(currentVersion, omitObsolete);
                ret.Add("\"socialAction\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
}

namespace WebAPI.Models.General
{
    public partial class KalturaAggregationCountFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaApiActionPermissionItem
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            ret.Add("\"isExcluded\": " + IsExcluded.ToString().ToLower());
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(Action != null)
            {
                ret.Add("\"action\": " + "\"" + Action + "\"");
            }
            if(Service != null)
            {
                ret.Add("\"service\": " + "\"" + Service + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaApiArgumentPermissionItem
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            ret.Add("\"isExcluded\": " + IsExcluded.ToString().ToLower());
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(Action != null)
            {
                ret.Add("\"action\": " + "\"" + Action + "\"");
            }
            if(Parameter != null)
            {
                ret.Add("\"parameter\": " + "\"" + Parameter + "\"");
            }
            if(Service != null)
            {
                ret.Add("\"service\": " + "\"" + Service + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaApiParameterPermissionItem
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            ret.Add("\"isExcluded\": " + IsExcluded.ToString().ToLower());
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            ret.Add("\"action\": " + Action.GetHashCode());
            if(Object != null)
            {
                ret.Add("\"object\": " + "\"" + Object + "\"");
            }
            if(Parameter != null)
            {
                ret.Add("\"parameter\": " + "\"" + Parameter + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAppToken
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Expiry.HasValue)
            {
                ret.Add("\"expiry\": " + Expiry);
            }
            if(HashType.HasValue)
            {
                ret.Add("\"hashType\": " + HashType.GetHashCode());
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + Id + "\"");
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
                ret.Add("\"sessionPrivileges\": " + "\"" + SessionPrivileges + "\"");
            }
            if(!DeprecatedAttribute.IsDeprecated("4.5.0.0", currentVersion) && SessionType.HasValue)
            {
                ret.Add("\"sessionType\": " + SessionType.GetHashCode());
            }
            if(SessionUserId != null)
            {
                ret.Add("\"sessionUserId\": " + "\"" + SessionUserId + "\"");
            }
            if(!DeprecatedAttribute.IsDeprecated("4.5.0.0", currentVersion))
            {
                ret.Add("\"status\": " + Status.GetHashCode());
            }
            if(Token != null)
            {
                ret.Add("\"token\": " + "\"" + Token + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaBaseResponseProfile
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaBooleanValue
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(description != null)
            {
                ret.Add("\"description\": " + "\"" + description + "\"");
            }
            ret.Add("\"value\": " + value.ToString().ToLower());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaClientConfiguration
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(ApiVersion != null)
            {
                ret.Add("\"apiVersion\": " + "\"" + ApiVersion + "\"");
            }
            if(ClientTag != null)
            {
                ret.Add("\"clientTag\": " + "\"" + ClientTag + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaDetachedResponseProfile
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"filter\": " + Filter);
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(RelatedProfiles != null && RelatedProfiles.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", RelatedProfiles.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"relatedProfiles\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaDoubleValue
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(description != null)
            {
                ret.Add("\"description\": " + "\"" + description + "\"");
            }
            ret.Add("\"value\": " + value);
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaFilter<T>
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy);
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaFilterPager
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(PageIndex.HasValue)
            {
                ret.Add("\"pageIndex\": " + PageIndex);
            }
            if(PageSize.HasValue)
            {
                ret.Add("\"pageSize\": " + PageSize);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaGroupPermission
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(PermissionItems != null && PermissionItems.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", PermissionItems.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"permissionItems\": " + propertyValue);
            }
            if(Group != null)
            {
                ret.Add("\"group\": " + "\"" + Group + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaIdentifierTypeFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"by\": " + By.GetHashCode());
            if(Identifier != null)
            {
                ret.Add("\"identifier\": " + "\"" + Identifier + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaIntegerValue
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(description != null)
            {
                ret.Add("\"description\": " + "\"" + description + "\"");
            }
            ret.Add("\"value\": " + value);
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaIntegerValueListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Values != null && Values.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Values.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaKeyValue
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(key != null)
            {
                ret.Add("\"key\": " + "\"" + key + "\"");
            }
            if(value != null)
            {
                ret.Add("\"value\": " + "\"" + value + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaLongValue
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(description != null)
            {
                ret.Add("\"description\": " + "\"" + description + "\"");
            }
            ret.Add("\"value\": " + value);
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaMultilingualStringValue
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(description != null)
            {
                ret.Add("\"description\": " + "\"" + description + "\"");
            }
            ret.Add(value.ToCustomJson(currentVersion, omitObsolete, "value"));
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaMultilingualStringValueArray
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaNotification
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(eventObject != null)
            {
                propertyValue = eventObject.ToJson(currentVersion, omitObsolete);
                ret.Add("\"object\": " + propertyValue);
            }
            if(eventObjectType != null)
            {
                ret.Add("\"eventObjectType\": " + "\"" + eventObjectType + "\"");
            }
            if(eventType.HasValue)
            {
                ret.Add("\"eventType\": " + eventType.GetHashCode());
            }
            if(systemName != null)
            {
                ret.Add("\"systemName\": " + "\"" + systemName + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaOTTObject
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPersistedFilter<T>
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy);
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaReport
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaReportFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaReportListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaRequestConfiguration
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Currency != null)
            {
                ret.Add("\"currency\": " + "\"" + Currency + "\"");
            }
            if(KS != null)
            {
                ret.Add("\"ks\": " + "\"" + KS + "\"");
            }
            if(Language != null)
            {
                ret.Add("\"language\": " + "\"" + Language + "\"");
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
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaStringValue
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(description != null)
            {
                ret.Add("\"description\": " + "\"" + description + "\"");
            }
            if(value != null)
            {
                ret.Add("\"value\": " + "\"" + value + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaStringValueArray
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaTranslationToken
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Language != null)
            {
                ret.Add("\"language\": " + "\"" + Language + "\"");
            }
            if(Value != null)
            {
                ret.Add("\"value\": " + "\"" + Value + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaValue
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(description != null)
            {
                ret.Add("\"description\": " + "\"" + description + "\"");
            }
            return String.Join(", ", ret);
        }
    }
}

namespace WebAPI.Models.Notifications
{
    public partial class KalturaAnnouncement
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
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
                ret.Add("\"imageUrl\": " + "\"" + ImageUrl + "\"");
            }
            if(Message != null)
            {
                ret.Add("\"message\": " + "\"" + Message + "\"");
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            ret.Add("\"recipients\": " + Recipients.GetHashCode());
            if(StartTime.HasValue)
            {
                ret.Add("\"startTime\": " + StartTime);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"start_time\": " + StartTime);
                }
            }
            ret.Add("\"status\": " + Status.GetHashCode());
            if(Timezone != null)
            {
                ret.Add("\"timezone\": " + "\"" + Timezone + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAssetReminder
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            ret.Add("\"type\": " + Type.GetHashCode());
            ret.Add("\"assetId\": " + AssetId);
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaReminder
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            ret.Add("\"type\": " + Type.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSeriesReminder
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            ret.Add("\"type\": " + Type.GetHashCode());
            ret.Add("\"epgChannelId\": " + EpgChannelId);
            if(SeasonNumber.HasValue)
            {
                ret.Add("\"seasonNumber\": " + SeasonNumber);
            }
            if(SeriesId != null)
            {
                ret.Add("\"seriesId\": " + "\"" + SeriesId + "\"");
            }
            return String.Join(", ", ret);
        }
    }
}

namespace WebAPI.Models.Notification
{
    public partial class KalturaAnnouncementFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAnnouncementListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Announcements != null && Announcements.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Announcements.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAssetReminderFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(!omitObsolete && KSql != null)
            {
                ret.Add("\"kSql\": " + "\"" + KSql + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaEmailMessage
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(BccAddress != null)
            {
                ret.Add("\"bccAddress\": " + "\"" + BccAddress + "\"");
            }
            if(ExtraParameters != null && ExtraParameters.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", ExtraParameters.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"extraParameters\": " + propertyValue);
            }
            if(FirstName != null)
            {
                ret.Add("\"firstName\": " + "\"" + FirstName + "\"");
            }
            if(LastName != null)
            {
                ret.Add("\"lastName\": " + "\"" + LastName + "\"");
            }
            if(SenderFrom != null)
            {
                ret.Add("\"senderFrom\": " + "\"" + SenderFrom + "\"");
            }
            if(SenderName != null)
            {
                ret.Add("\"senderName\": " + "\"" + SenderName + "\"");
            }
            if(SenderTo != null)
            {
                ret.Add("\"senderTo\": " + "\"" + SenderTo + "\"");
            }
            if(Subject != null)
            {
                ret.Add("\"subject\": " + "\"" + Subject + "\"");
            }
            if(TemplateName != null)
            {
                ret.Add("\"templateName\": " + "\"" + TemplateName + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaEngagement
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(AdapterDynamicData != null)
            {
                ret.Add("\"adapterDynamicData\": " + "\"" + AdapterDynamicData + "\"");
            }
            ret.Add("\"adapterId\": " + AdapterId);
            ret.Add("\"couponGroupId\": " + CouponGroupId);
            ret.Add("\"id\": " + Id);
            ret.Add("\"intervalSeconds\": " + IntervalSeconds);
            ret.Add("\"sendTimeInSeconds\": " + SendTimeInSeconds);
            ret.Add("\"totalNumberOfRecipients\": " + TotalNumberOfRecipients);
            ret.Add("\"type\": " + Type.GetHashCode());
            if(UserList != null)
            {
                ret.Add("\"userList\": " + "\"" + UserList + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaEngagementAdapter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(AdapterUrl != null)
            {
                ret.Add("\"adapterUrl\": " + "\"" + AdapterUrl + "\"");
            }
            if(IsActive.HasValue)
            {
                ret.Add("\"isActive\": " + IsActive.ToString().ToLower());
            }
            if(ProviderUrl != null)
            {
                ret.Add("\"providerUrl\": " + "\"" + ProviderUrl + "\"");
            }
            if(Settings != null && Settings.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", Settings.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"engagementAdapterSettings\": " + propertyValue);
            }
            if(SharedSecret != null)
            {
                ret.Add("\"sharedSecret\": " + "\"" + SharedSecret + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaEngagementAdapterBase
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaEngagementAdapterListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(EngagementAdapters != null && EngagementAdapters.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", EngagementAdapters.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaEngagementFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(SendTimeGreaterThanOrEqual.HasValue)
            {
                ret.Add("\"sendTimeGreaterThanOrEqual\": " + SendTimeGreaterThanOrEqual);
            }
            if(TypeIn != null)
            {
                ret.Add("\"typeIn\": " + "\"" + TypeIn + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaEngagementListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Engagements != null && Engagements.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Engagements.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaFeed
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"assetId\": " + AssetId);
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"asset_id\": " + AssetId);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaFollowDataBase
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"announcementId\": " + AnnouncementId);
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"announcement_id\": " + AnnouncementId);
            }
            if(FollowPhrase != null)
            {
                ret.Add("\"followPhrase\": " + "\"" + FollowPhrase + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"follow_phrase\": " + "\"" + FollowPhrase + "\"");
                }
            }
            ret.Add("\"status\": " + Status);
            ret.Add("\"timestamp\": " + Timestamp);
            if(Title != null)
            {
                ret.Add("\"title\": " + "\"" + Title + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaFollowDataTvSeries
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"announcementId\": " + AnnouncementId);
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"announcement_id\": " + AnnouncementId);
            }
            if(FollowPhrase != null)
            {
                ret.Add("\"followPhrase\": " + "\"" + FollowPhrase + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"follow_phrase\": " + "\"" + FollowPhrase + "\"");
                }
            }
            ret.Add("\"status\": " + Status);
            ret.Add("\"timestamp\": " + Timestamp);
            if(Title != null)
            {
                ret.Add("\"title\": " + "\"" + Title + "\"");
            }
            ret.Add("\"assetId\": " + AssetId);
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"asset_id\": " + AssetId);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaFollowTvSeries
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"announcementId\": " + AnnouncementId);
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"announcement_id\": " + AnnouncementId);
            }
            if(FollowPhrase != null)
            {
                ret.Add("\"followPhrase\": " + "\"" + FollowPhrase + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"follow_phrase\": " + "\"" + FollowPhrase + "\"");
                }
            }
            ret.Add("\"status\": " + Status);
            ret.Add("\"timestamp\": " + Timestamp);
            if(Title != null)
            {
                ret.Add("\"title\": " + "\"" + Title + "\"");
            }
            ret.Add("\"assetId\": " + AssetId);
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaFollowTvSeriesFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaFollowTvSeriesListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(FollowDataList != null && FollowDataList.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", FollowDataList.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaInboxMessage
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"createdAt\": " + CreatedAt);
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + Id + "\"");
            }
            if(Message != null)
            {
                ret.Add("\"message\": " + "\"" + Message + "\"");
            }
            ret.Add("\"status\": " + Status.GetHashCode());
            ret.Add("\"type\": " + Type.GetHashCode());
            if(Url != null)
            {
                ret.Add("\"url\": " + "\"" + Url + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaInboxMessageFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
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
                ret.Add("\"typeIn\": " + "\"" + TypeIn + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaInboxMessageListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(InboxMessages != null && InboxMessages.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", InboxMessages.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaInboxMessageResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(InboxMessages != null && InboxMessages.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", InboxMessages.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaInboxMessageTypeHolder
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"type\": " + type.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaListFollowDataTvSeriesResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(FollowDataList != null && FollowDataList.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", FollowDataList.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaMessageAnnouncementListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Announcements != null && Announcements.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Announcements.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaMessageTemplate
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Action != null)
            {
                ret.Add("\"action\": " + "\"" + Action + "\"");
            }
            if(DateFormat != null)
            {
                ret.Add("\"dateFormat\": " + "\"" + DateFormat + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"date_format\": " + "\"" + DateFormat + "\"");
                }
            }
            if(Message != null)
            {
                ret.Add("\"message\": " + "\"" + Message + "\"");
            }
            ret.Add("\"messageType\": " + MessageType.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"asset_type\": " + MessageType.GetHashCode());
            }
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0 || currentVersion.CompareTo(new Version("3.6.2094.15157")) > 0)
            {
                ret.Add("\"assetType\": " + MessageType.GetHashCode());
            }
            if(Sound != null)
            {
                ret.Add("\"sound\": " + "\"" + Sound + "\"");
            }
            if(URL != null)
            {
                ret.Add("\"url\": " + "\"" + URL + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaNotificationSettings
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(PushFollowEnabled.HasValue)
            {
                ret.Add("\"pushFollowEnabled\": " + PushFollowEnabled.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"push_follow_enabled\": " + PushFollowEnabled.ToString().ToLower());
                }
            }
            if(PushNotificationEnabled.HasValue)
            {
                ret.Add("\"pushNotificationEnabled\": " + PushNotificationEnabled.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"push_notification_enabled\": " + PushNotificationEnabled.ToString().ToLower());
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaNotificationsPartnerSettings
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(AutomaticIssueFollowNotification.HasValue)
            {
                ret.Add("\"automaticIssueFollowNotification\": " + AutomaticIssueFollowNotification.ToString().ToLower());
            }
            if(ChurnMailSubject != null)
            {
                ret.Add("\"churnMailSubject\": " + "\"" + ChurnMailSubject + "\"");
            }
            if(ChurnMailTemplateName != null)
            {
                ret.Add("\"churnMailTemplateName\": " + "\"" + ChurnMailTemplateName + "\"");
            }
            if(InboxEnabled.HasValue)
            {
                ret.Add("\"inboxEnabled\": " + InboxEnabled.ToString().ToLower());
            }
            if(MailSenderName != null)
            {
                ret.Add("\"mailSenderName\": " + "\"" + MailSenderName + "\"");
            }
            if(MessageTTLDays.HasValue)
            {
                ret.Add("\"messageTTLDays\": " + MessageTTLDays);
            }
            if(PushAdapterUrl != null)
            {
                ret.Add("\"pushAdapterUrl\": " + "\"" + PushAdapterUrl + "\"");
            }
            if(PushEndHour.HasValue)
            {
                ret.Add("\"pushEndHour\": " + PushEndHour);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"push_end_hour\": " + PushEndHour);
                }
            }
            if(PushNotificationEnabled.HasValue)
            {
                ret.Add("\"pushNotificationEnabled\": " + PushNotificationEnabled.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"push_notification_enabled\": " + PushNotificationEnabled.ToString().ToLower());
                }
            }
            if(PushStartHour.HasValue)
            {
                ret.Add("\"pushStartHour\": " + PushStartHour);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"push_start_hour\": " + PushStartHour);
                }
            }
            if(PushSystemAnnouncementsEnabled.HasValue)
            {
                ret.Add("\"pushSystemAnnouncementsEnabled\": " + PushSystemAnnouncementsEnabled.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
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
                ret.Add("\"senderEmail\": " + "\"" + SenderEmail + "\"");
            }
            if(TopicExpirationDurationDays.HasValue)
            {
                ret.Add("\"topicExpirationDurationDays\": " + TopicExpirationDurationDays);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaNotificationsSettings
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(PushFollowEnabled.HasValue)
            {
                ret.Add("\"pushFollowEnabled\": " + PushFollowEnabled.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"push_follow_enabled\": " + PushFollowEnabled.ToString().ToLower());
                }
            }
            if(PushNotificationEnabled.HasValue)
            {
                ret.Add("\"pushNotificationEnabled\": " + PushNotificationEnabled.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"push_notification_enabled\": " + PushNotificationEnabled.ToString().ToLower());
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPartnerNotificationSettings
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(AutomaticIssueFollowNotification.HasValue)
            {
                ret.Add("\"automaticIssueFollowNotification\": " + AutomaticIssueFollowNotification.ToString().ToLower());
            }
            if(ChurnMailSubject != null)
            {
                ret.Add("\"churnMailSubject\": " + "\"" + ChurnMailSubject + "\"");
            }
            if(ChurnMailTemplateName != null)
            {
                ret.Add("\"churnMailTemplateName\": " + "\"" + ChurnMailTemplateName + "\"");
            }
            if(InboxEnabled.HasValue)
            {
                ret.Add("\"inboxEnabled\": " + InboxEnabled.ToString().ToLower());
            }
            if(MailSenderName != null)
            {
                ret.Add("\"mailSenderName\": " + "\"" + MailSenderName + "\"");
            }
            if(MessageTTLDays.HasValue)
            {
                ret.Add("\"messageTTLDays\": " + MessageTTLDays);
            }
            if(PushAdapterUrl != null)
            {
                ret.Add("\"pushAdapterUrl\": " + "\"" + PushAdapterUrl + "\"");
            }
            if(PushEndHour.HasValue)
            {
                ret.Add("\"pushEndHour\": " + PushEndHour);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"push_end_hour\": " + PushEndHour);
                }
            }
            if(PushNotificationEnabled.HasValue)
            {
                ret.Add("\"pushNotificationEnabled\": " + PushNotificationEnabled.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"push_notification_enabled\": " + PushNotificationEnabled.ToString().ToLower());
                }
            }
            if(PushStartHour.HasValue)
            {
                ret.Add("\"pushStartHour\": " + PushStartHour);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"push_start_hour\": " + PushStartHour);
                }
            }
            if(PushSystemAnnouncementsEnabled.HasValue)
            {
                ret.Add("\"pushSystemAnnouncementsEnabled\": " + PushSystemAnnouncementsEnabled.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
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
                ret.Add("\"senderEmail\": " + "\"" + SenderEmail + "\"");
            }
            if(TopicExpirationDurationDays.HasValue)
            {
                ret.Add("\"topicExpirationDurationDays\": " + TopicExpirationDurationDays);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPersonalFeed
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"assetId\": " + AssetId);
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"asset_id\": " + AssetId);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPersonalFeedFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPersonalFeedListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(PersonalFollowFeed != null && PersonalFollowFeed.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", PersonalFollowFeed.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPersonalFollowFeed
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"assetId\": " + AssetId);
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"asset_id\": " + AssetId);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPersonalFollowFeedResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(PersonalFollowFeed != null && PersonalFollowFeed.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", PersonalFollowFeed.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPushMessage
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Action != null)
            {
                ret.Add("\"action\": " + "\"" + Action + "\"");
            }
            if(Message != null)
            {
                ret.Add("\"message\": " + "\"" + Message + "\"");
            }
            if(Sound != null)
            {
                ret.Add("\"sound\": " + "\"" + Sound + "\"");
            }
            if(Url != null)
            {
                ret.Add("\"url\": " + "\"" + Url + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaRegistryResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"announcementId\": " + AnnouncementId);
            if(Key != null)
            {
                ret.Add("\"key\": " + "\"" + Key + "\"");
            }
            if(Url != null)
            {
                ret.Add("\"url\": " + "\"" + Url + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaReminderFilter<T>
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy);
            if(!omitObsolete && KSql != null)
            {
                ret.Add("\"kSql\": " + "\"" + KSql + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaReminderListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Reminders != null && Reminders.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Reminders.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSeasonsReminderFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(!omitObsolete && KSql != null)
            {
                ret.Add("\"kSql\": " + "\"" + KSql + "\"");
            }
            if(EpgChannelIdEqual.HasValue)
            {
                ret.Add("\"epgChannelIdEqual\": " + EpgChannelIdEqual);
            }
            if(SeasonNumberIn != null)
            {
                ret.Add("\"seasonNumberIn\": " + "\"" + SeasonNumberIn + "\"");
            }
            if(SeriesIdEqual != null)
            {
                ret.Add("\"seriesIdEqual\": " + "\"" + SeriesIdEqual + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSeriesReminderFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(!omitObsolete && KSql != null)
            {
                ret.Add("\"kSql\": " + "\"" + KSql + "\"");
            }
            if(EpgChannelIdEqual.HasValue)
            {
                ret.Add("\"epgChannelIdEqual\": " + EpgChannelIdEqual);
            }
            if(SeriesIdIn != null)
            {
                ret.Add("\"seriesIdIn\": " + "\"" + SeriesIdIn + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaTopic
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"automaticIssueNotification\": " + AutomaticIssueNotification.GetHashCode());
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + Id + "\"");
            }
            ret.Add("\"lastMessageSentDateSec\": " + LastMessageSentDateSec);
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(SubscribersAmount != null)
            {
                ret.Add("\"subscribersAmount\": " + "\"" + SubscribersAmount + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaTopicFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaTopicListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Topics != null && Topics.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Topics.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaTopicResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Topics != null && Topics.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Topics.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
}

namespace WebAPI.App_Start
{
    public partial class KalturaAPIException
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(args != null && args.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", args.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"args\": " + propertyValue);
            }
            if(code != null)
            {
                ret.Add("\"code\": " + "\"" + code + "\"");
            }
            if(message != null)
            {
                ret.Add("\"message\": " + "\"" + message + "\"");
            }
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaApiExceptionArg
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(name != null)
            {
                ret.Add("\"name\": " + "\"" + name + "\"");
            }
            if(value != null)
            {
                ret.Add("\"value\": " + "\"" + value + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAPIExceptionWrapper
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(error != null)
            {
                propertyValue = error.ToJson(currentVersion, omitObsolete);
                ret.Add("\"error\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
}

namespace WebAPI.Models.Catalog
{
    public partial class KalturaAsset
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add(Description.ToCustomJson(currentVersion, omitObsolete, "description"));
            if(EnableCatchUp.HasValue)
            {
                ret.Add("\"enableCatchUp\": " + EnableCatchUp.ToString().ToLower());
            }
            if(EnableCdvr.HasValue)
            {
                ret.Add("\"enableCdvr\": " + EnableCdvr.ToString().ToLower());
            }
            if(EnableStartOver.HasValue)
            {
                ret.Add("\"enableStartOver\": " + EnableStartOver.ToString().ToLower());
            }
            if(EnableTrickPlay.HasValue)
            {
                ret.Add("\"enableTrickPlay\": " + EnableTrickPlay.ToString().ToLower());
            }
            if(EndDate.HasValue)
            {
                ret.Add("\"endDate\": " + EndDate);
            }
            if(ExternalId != null)
            {
                ret.Add("\"externalId\": " + "\"" + ExternalId + "\"");
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Images != null && Images.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Images.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"images\": " + propertyValue);
            }
            if(MediaFiles != null && MediaFiles.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", MediaFiles.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"mediaFiles\": " + propertyValue);
            }
            if(Metas != null && Metas.Count > 0)
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
            if(Tags != null && Tags.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", Tags.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"tags\": " + propertyValue);
            }
            if(Type.HasValue)
            {
                ret.Add("\"type\": " + Type);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAssetBookmark
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(IsFinishedWatching.HasValue)
            {
                ret.Add("\"finishedWatching\": " + IsFinishedWatching.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"finished_watching\": " + IsFinishedWatching.ToString().ToLower());
                }
            }
            if(Position.HasValue)
            {
                ret.Add("\"position\": " + Position);
            }
            ret.Add("\"positionOwner\": " + PositionOwner.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"position_owner\": " + PositionOwner.GetHashCode());
            }
            if(User != null)
            {
                propertyValue = User.ToJson(currentVersion, omitObsolete);
                ret.Add("\"user\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAssetBookmarks
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + Id + "\"");
            }
            ret.Add("\"type\": " + Type.GetHashCode());
            if(Bookmarks != null && Bookmarks.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Bookmarks.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAssetComment
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"createDate\": " + CreateDate);
            if(Header != null)
            {
                ret.Add("\"header\": " + "\"" + Header + "\"");
            }
            if(Text != null)
            {
                ret.Add("\"text\": " + "\"" + Text + "\"");
            }
            if(Writer != null)
            {
                ret.Add("\"writer\": " + "\"" + Writer + "\"");
            }
            ret.Add("\"assetId\": " + AssetId);
            ret.Add("\"assetType\": " + AssetType.GetHashCode());
            ret.Add("\"id\": " + Id);
            if(SubHeader != null)
            {
                ret.Add("\"subHeader\": " + "\"" + SubHeader + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAssetCommentFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            ret.Add("\"assetIdEqual\": " + AssetIdEqual);
            ret.Add("\"assetTypeEqual\": " + AssetTypeEqual.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAssetCommentListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAssetCount
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"count\": " + Count);
            if(SubCounts != null && SubCounts.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", SubCounts.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"subs\": " + propertyValue);
            }
            if(Value != null)
            {
                ret.Add("\"value\": " + "\"" + Value + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAssetCountListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            ret.Add("\"assetsCount\": " + AssetsCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAssetFieldGroupBy
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"value\": " + Value.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAssetFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(DynamicOrderBy != null)
            {
                propertyValue = DynamicOrderBy.ToJson(currentVersion, omitObsolete);
                ret.Add("\"dynamicOrderBy\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAssetGroupBy
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAssetHistory
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"assetId\": " + AssetId);
            ret.Add("\"assetType\": " + AssetType.GetHashCode());
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
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAssetHistoryFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(AssetIdIn != null)
            {
                ret.Add("\"assetIdIn\": " + "\"" + AssetIdIn + "\"");
            }
            if(DaysLessThanOrEqual.HasValue)
            {
                ret.Add("\"daysLessThanOrEqual\": " + DaysLessThanOrEqual);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"days\": " + DaysLessThanOrEqual);
                }
            }
            if(!omitObsolete && filterTypes != null && filterTypes.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", filterTypes.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"filterTypes\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"filter_types\": " + propertyValue);
                }
            }
            if(StatusEqual.HasValue)
            {
                ret.Add("\"statusEqual\": " + StatusEqual.GetHashCode());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"filter_status\": " + StatusEqual.GetHashCode());
                }
            }
            if(TypeIn != null)
            {
                ret.Add("\"typeIn\": " + "\"" + TypeIn + "\"");
            }
            if(!omitObsolete && with != null && with.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", with.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"with\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAssetHistoryListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAssetInfo
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Description != null)
            {
                ret.Add("\"description\": " + "\"" + Description + "\"");
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Images != null && Images.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Images.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"images\": " + propertyValue);
            }
            if(MediaFiles != null && MediaFiles.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", MediaFiles.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"mediaFiles\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"media_files\": " + propertyValue);
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
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
            if(EndDate.HasValue)
            {
                ret.Add("\"endDate\": " + EndDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"end_date\": " + EndDate);
                }
            }
            if(ExtraParams != null && ExtraParams.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", ExtraParams.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"extraParams\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"extra_params\": " + propertyValue);
                }
            }
            if(Metas != null && Metas.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", Metas.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"metas\": " + propertyValue);
            }
            if(StartDate.HasValue)
            {
                ret.Add("\"startDate\": " + StartDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"start_date\": " + StartDate);
                }
            }
            if(Tags != null && Tags.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", Tags.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"tags\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAssetInfoFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"cut_with\": " + cutWith.GetHashCode());
            if(FilterTags != null && FilterTags.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", FilterTags.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"filter_tags\": " + propertyValue);
            }
            if(IDs != null && IDs.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", IDs.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"ids\": " + propertyValue);
            }
            ret.Add("\"referenceType\": " + ReferenceType.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"reference_type\": " + ReferenceType.GetHashCode());
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAssetInfoListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            if(RequestId != null)
            {
                ret.Add("\"requestId\": " + "\"" + RequestId + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"request_id\": " + "\"" + RequestId + "\"");
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAssetListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAssetMetaOrTagGroupBy
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Value != null)
            {
                ret.Add("\"value\": " + "\"" + Value + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAssetsBookmarksResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(AssetsBookmarks != null && AssetsBookmarks.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", AssetsBookmarks.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAssetsCount
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Field != null)
            {
                ret.Add("\"field\": " + "\"" + Field + "\"");
            }
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAssetsFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Assets != null && Assets.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Assets.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"assets\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"Assets\": " + propertyValue);
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAssetStatistics
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"assetId\": " + AssetId);
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"asset_id\": " + AssetId);
            }
            if(BuzzAvgScore != null)
            {
                propertyValue = BuzzAvgScore.ToJson(currentVersion, omitObsolete);
                ret.Add("\"buzzScore\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"buzz_score\": " + propertyValue);
                }
            }
            ret.Add("\"likes\": " + Likes);
            ret.Add("\"rating\": " + Rating);
            ret.Add("\"ratingCount\": " + RatingCount);
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"rating_count\": " + RatingCount);
            }
            ret.Add("\"views\": " + Views);
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAssetStatisticsListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(AssetsStatistics != null && AssetsStatistics.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", AssetsStatistics.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaAssetStatisticsQuery
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(AssetIdIn != null)
            {
                ret.Add("\"assetIdIn\": " + "\"" + AssetIdIn + "\"");
            }
            ret.Add("\"assetTypeEqual\": " + AssetTypeEqual.GetHashCode());
            ret.Add("\"endDateGreaterThanOrEqual\": " + EndDateGreaterThanOrEqual);
            ret.Add("\"startDateGreaterThanOrEqual\": " + StartDateGreaterThanOrEqual);
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaBaseAssetInfo
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Description != null)
            {
                ret.Add("\"description\": " + "\"" + Description + "\"");
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Images != null && Images.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Images.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"images\": " + propertyValue);
            }
            if(MediaFiles != null && MediaFiles.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", MediaFiles.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"mediaFiles\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"media_files\": " + propertyValue);
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
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
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaBaseChannel
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaBaseSearchAssetFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(DynamicOrderBy != null)
            {
                propertyValue = DynamicOrderBy.ToJson(currentVersion, omitObsolete);
                ret.Add("\"dynamicOrderBy\": " + propertyValue);
            }
            if(GroupBy != null && GroupBy.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", GroupBy.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"groupBy\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaBookmark
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + Id + "\"");
            }
            ret.Add("\"type\": " + Type.GetHashCode());
            if(IsFinishedWatching.HasValue)
            {
                ret.Add("\"finishedWatching\": " + IsFinishedWatching.ToString().ToLower());
            }
            if(PlayerData != null)
            {
                propertyValue = PlayerData.ToJson(currentVersion, omitObsolete);
                ret.Add("\"playerData\": " + propertyValue);
            }
            if(Position.HasValue)
            {
                ret.Add("\"position\": " + Position);
            }
            ret.Add("\"positionOwner\": " + PositionOwner.GetHashCode());
            if(!omitObsolete && User != null)
            {
                propertyValue = User.ToJson(currentVersion, omitObsolete);
                ret.Add("\"user\": " + propertyValue);
            }
            if(UserId != null)
            {
                ret.Add("\"userId\": " + "\"" + UserId + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaBookmarkFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(AssetIdIn != null)
            {
                ret.Add("\"assetIdIn\": " + "\"" + AssetIdIn + "\"");
            }
            if(!omitObsolete && AssetIn != null && AssetIn.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", AssetIn.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"assetIn\": " + propertyValue);
            }
            if(AssetTypeEqual.HasValue)
            {
                ret.Add("\"assetTypeEqual\": " + AssetTypeEqual.GetHashCode());
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaBookmarkListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(AssetsBookmarks != null && AssetsBookmarks.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", AssetsBookmarks.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaBookmarkPlayerData
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"action\": " + action.GetHashCode());
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
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaBundleFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(DynamicOrderBy != null)
            {
                propertyValue = DynamicOrderBy.ToJson(currentVersion, omitObsolete);
                ret.Add("\"dynamicOrderBy\": " + propertyValue);
            }
            ret.Add("\"bundleTypeEqual\": " + BundleTypeEqual.GetHashCode());
            ret.Add("\"idEqual\": " + IdEqual);
            if(TypeIn != null)
            {
                ret.Add("\"typeIn\": " + "\"" + TypeIn + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaBuzzScore
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(AvgScore.HasValue)
            {
                ret.Add("\"avgScore\": " + AvgScore);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"avg_score\": " + AvgScore);
                }
            }
            if(NormalizedAvgScore.HasValue)
            {
                ret.Add("\"normalizedAvgScore\": " + NormalizedAvgScore);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"normalized_avg_score\": " + NormalizedAvgScore);
                }
            }
            if(UpdateDate.HasValue)
            {
                ret.Add("\"updateDate\": " + UpdateDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"update_date\": " + UpdateDate);
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaCatalogWithHolder
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"type\": " + type.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaChannel
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(AssetTypes != null && AssetTypes.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", AssetTypes.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"assetTypes\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"asset_types\": " + propertyValue);
                }
            }
            if(Description != null)
            {
                ret.Add("\"description\": " + "\"" + Description + "\"");
            }
            if(FilterExpression != null)
            {
                ret.Add("\"filterExpression\": " + "\"" + FilterExpression + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"filter_expression\": " + "\"" + FilterExpression + "\"");
                }
            }
            if(GroupBy != null)
            {
                propertyValue = GroupBy.ToJson(currentVersion, omitObsolete);
                ret.Add("\"groupBy\": " + propertyValue);
            }
            if(Images != null && Images.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Images.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"images\": " + propertyValue);
            }
            if(IsActive.HasValue)
            {
                ret.Add("\"isActive\": " + IsActive.ToString().ToLower());
            }
            if(!omitObsolete && MediaTypes != null && MediaTypes.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", MediaTypes.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"media_types\": " + propertyValue);
            }
            ret.Add("\"order\": " + Order.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaChannelExternalFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(DynamicOrderBy != null)
            {
                propertyValue = DynamicOrderBy.ToJson(currentVersion, omitObsolete);
                ret.Add("\"dynamicOrderBy\": " + propertyValue);
            }
            if(FreeText != null)
            {
                ret.Add("\"freeText\": " + "\"" + FreeText + "\"");
            }
            ret.Add("\"idEqual\": " + IdEqual);
            ret.Add("\"utcOffsetEqual\": " + UtcOffsetEqual);
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaChannelFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(DynamicOrderBy != null)
            {
                propertyValue = DynamicOrderBy.ToJson(currentVersion, omitObsolete);
                ret.Add("\"dynamicOrderBy\": " + propertyValue);
            }
            ret.Add("\"idEqual\": " + IdEqual);
            if(KSql != null)
            {
                ret.Add("\"kSql\": " + "\"" + KSql + "\"");
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaDynamicOrderBy
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(OrderBy.HasValue)
            {
                ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaEPGChannelAssets
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Assets != null && Assets.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Assets.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            if(ChannelID.HasValue)
            {
                ret.Add("\"channelId\": " + ChannelID);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"channel_id\": " + ChannelID);
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaEPGChannelAssetsListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Channels != null && Channels.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Channels.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"assets\": " + propertyValue);
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaEpgChannelFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(EndTime.HasValue)
            {
                ret.Add("\"endTime\": " + EndTime);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"end_time\": " + EndTime);
                }
            }
            if(IDs != null && IDs.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", IDs.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"ids\": " + propertyValue);
            }
            if(StartTime.HasValue)
            {
                ret.Add("\"startTime\": " + StartTime);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"start_time\": " + StartTime);
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaLastPosition
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"position\": " + Position);
            ret.Add("\"position_owner\": " + PositionOwner.GetHashCode());
            if(UserId != null)
            {
                ret.Add("\"user_id\": " + "\"" + UserId + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaLastPositionFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"by\": " + By.GetHashCode());
            if(Ids != null && Ids.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Ids.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"ids\": " + propertyValue);
            }
            ret.Add("\"type\": " + Type.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaLastPositionListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(LastPositions != null && LastPositions.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", LastPositions.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaMediaAsset
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add(Description.ToCustomJson(currentVersion, omitObsolete, "description"));
            if(EnableCatchUp.HasValue)
            {
                ret.Add("\"enableCatchUp\": " + EnableCatchUp.ToString().ToLower());
            }
            if(EnableCdvr.HasValue)
            {
                ret.Add("\"enableCdvr\": " + EnableCdvr.ToString().ToLower());
            }
            if(EnableStartOver.HasValue)
            {
                ret.Add("\"enableStartOver\": " + EnableStartOver.ToString().ToLower());
            }
            if(EnableTrickPlay.HasValue)
            {
                ret.Add("\"enableTrickPlay\": " + EnableTrickPlay.ToString().ToLower());
            }
            if(EndDate.HasValue)
            {
                ret.Add("\"endDate\": " + EndDate);
            }
            if(ExternalId != null)
            {
                ret.Add("\"externalId\": " + "\"" + ExternalId + "\"");
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Images != null && Images.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Images.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"images\": " + propertyValue);
            }
            if(MediaFiles != null && MediaFiles.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", MediaFiles.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"mediaFiles\": " + propertyValue);
            }
            if(Metas != null && Metas.Count > 0)
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
            if(Tags != null && Tags.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", Tags.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"tags\": " + propertyValue);
            }
            if(Type.HasValue)
            {
                ret.Add("\"type\": " + Type);
            }
            if(CatchUpBuffer.HasValue)
            {
                ret.Add("\"catchUpBuffer\": " + CatchUpBuffer);
            }
            if(DeviceRule != null)
            {
                ret.Add("\"deviceRule\": " + "\"" + DeviceRule + "\"");
            }
            if(EnableRecordingPlaybackNonEntitledChannel.HasValue)
            {
                ret.Add("\"enableRecordingPlaybackNonEntitledChannel\": " + EnableRecordingPlaybackNonEntitledChannel.ToString().ToLower());
            }
            if(EntryId != null)
            {
                ret.Add("\"entryId\": " + "\"" + EntryId + "\"");
            }
            if(ExternalIds != null)
            {
                ret.Add("\"externalIds\": " + "\"" + ExternalIds + "\"");
            }
            if(GeoBlockRule != null)
            {
                ret.Add("\"geoBlockRule\": " + "\"" + GeoBlockRule + "\"");
            }
            if(TrickPlayBuffer.HasValue)
            {
                ret.Add("\"trickPlayBuffer\": " + TrickPlayBuffer);
            }
            if(TypeDescription != null)
            {
                ret.Add("\"typeDescription\": " + "\"" + TypeDescription + "\"");
            }
            if(WatchPermissionRule != null)
            {
                ret.Add("\"watchPermissionRule\": " + "\"" + WatchPermissionRule + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaMediaFile
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(AltCdnCode != null)
            {
                ret.Add("\"altCdnCode\": " + "\"" + AltCdnCode + "\"");
            }
            if(AssetId.HasValue)
            {
                ret.Add("\"assetId\": " + AssetId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"asset_id\": " + AssetId);
                }
            }
            if(BillingType != null)
            {
                ret.Add("\"billingType\": " + "\"" + BillingType + "\"");
            }
            if(CdnCode != null)
            {
                ret.Add("\"cdnCode\": " + "\"" + CdnCode + "\"");
            }
            if(CdnName != null)
            {
                ret.Add("\"cdnName\": " + "\"" + CdnName + "\"");
            }
            if(Duration.HasValue)
            {
                ret.Add("\"duration\": " + Duration);
            }
            if(ExternalId != null)
            {
                ret.Add("\"externalId\": " + "\"" + ExternalId + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"external_id\": " + "\"" + ExternalId + "\"");
                }
            }
            ret.Add("\"fileSize\": " + FileSize);
            if(HandlingType != null)
            {
                ret.Add("\"handlingType\": " + "\"" + HandlingType + "\"");
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(PPVModules != null)
            {
                propertyValue = PPVModules.ToJson(currentVersion, omitObsolete);
                ret.Add("\"ppvModules\": " + propertyValue);
            }
            if(ProductCode != null)
            {
                ret.Add("\"productCode\": " + "\"" + ProductCode + "\"");
            }
            if(Quality != null)
            {
                ret.Add("\"quality\": " + "\"" + Quality + "\"");
            }
            if(Type != null)
            {
                ret.Add("\"type\": " + "\"" + Type + "\"");
            }
            if(Url != null)
            {
                ret.Add("\"url\": " + "\"" + Url + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaMediaImage
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Height.HasValue)
            {
                ret.Add("\"height\": " + Height);
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + Id + "\"");
            }
            if(IsDefault.HasValue)
            {
                ret.Add("\"isDefault\": " + IsDefault.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_default\": " + IsDefault.ToString().ToLower());
                }
            }
            if(Ratio != null)
            {
                ret.Add("\"ratio\": " + "\"" + Ratio + "\"");
            }
            if(Url != null)
            {
                ret.Add("\"url\": " + "\"" + Url + "\"");
            }
            if(Version.HasValue)
            {
                ret.Add("\"version\": " + Version);
            }
            if(Width.HasValue)
            {
                ret.Add("\"width\": " + Width);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaOTTCategory
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Channels != null && Channels.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Channels.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"channels\": " + propertyValue);
            }
            if(ChildCategories != null && ChildCategories.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", ChildCategories.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"childCategories\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"child_categories\": " + propertyValue);
                }
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Images != null && Images.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Images.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"images\": " + propertyValue);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(ParentCategoryId.HasValue)
            {
                ret.Add("\"parentCategoryId\": " + ParentCategoryId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"parent_category_id\": " + ParentCategoryId);
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPersonalAsset
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Bookmarks != null && Bookmarks.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Bookmarks.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"bookmarks\": " + propertyValue);
            }
            if(Files != null && Files.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Files.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"files\": " + propertyValue);
            }
            ret.Add("\"following\": " + Following.ToString().ToLower());
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            ret.Add("\"type\": " + Type.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPersonalAssetListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPersonalAssetRequest
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(FileIds != null && FileIds.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", FileIds.Select(item => item.ToString())) + "]";
                ret.Add("\"fileIds\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"file_ids\": " + propertyValue);
                }
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            ret.Add("\"type\": " + Type.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPersonalAssetWithHolder
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"type\": " + type.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPersonalFile
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
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
                ret.Add("\"offer\": " + "\"" + Offer + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPlayerAssetData
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(action != null)
            {
                ret.Add("\"action\": " + "\"" + action + "\"");
            }
            if(averageBitRate.HasValue)
            {
                ret.Add("\"averageBitrate\": " + averageBitRate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"average_bitrate\": " + averageBitRate);
                }
            }
            if(currentBitRate.HasValue)
            {
                ret.Add("\"currentBitrate\": " + currentBitRate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
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
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"total_bitrate\": " + totalBitRate);
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaProgramAsset
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add(Description.ToCustomJson(currentVersion, omitObsolete, "description"));
            if(EnableCatchUp.HasValue)
            {
                ret.Add("\"enableCatchUp\": " + EnableCatchUp.ToString().ToLower());
            }
            if(EnableCdvr.HasValue)
            {
                ret.Add("\"enableCdvr\": " + EnableCdvr.ToString().ToLower());
            }
            if(EnableStartOver.HasValue)
            {
                ret.Add("\"enableStartOver\": " + EnableStartOver.ToString().ToLower());
            }
            if(EnableTrickPlay.HasValue)
            {
                ret.Add("\"enableTrickPlay\": " + EnableTrickPlay.ToString().ToLower());
            }
            if(EndDate.HasValue)
            {
                ret.Add("\"endDate\": " + EndDate);
            }
            if(ExternalId != null)
            {
                ret.Add("\"externalId\": " + "\"" + ExternalId + "\"");
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Images != null && Images.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Images.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"images\": " + propertyValue);
            }
            if(MediaFiles != null && MediaFiles.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", MediaFiles.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"mediaFiles\": " + propertyValue);
            }
            if(Metas != null && Metas.Count > 0)
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
            if(Tags != null && Tags.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", Tags.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"tags\": " + propertyValue);
            }
            if(Type.HasValue)
            {
                ret.Add("\"type\": " + Type);
            }
            if(Crid != null)
            {
                ret.Add("\"crid\": " + "\"" + Crid + "\"");
            }
            if(EpgChannelId.HasValue)
            {
                ret.Add("\"epgChannelId\": " + EpgChannelId);
            }
            if(EpgId != null)
            {
                ret.Add("\"epgId\": " + "\"" + EpgId + "\"");
            }
            if(LinearAssetId.HasValue)
            {
                ret.Add("\"linearAssetId\": " + LinearAssetId);
            }
            if(RelatedMediaId.HasValue)
            {
                ret.Add("\"relatedMediaId\": " + RelatedMediaId);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaRecordingAsset
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add(Description.ToCustomJson(currentVersion, omitObsolete, "description"));
            if(EnableCatchUp.HasValue)
            {
                ret.Add("\"enableCatchUp\": " + EnableCatchUp.ToString().ToLower());
            }
            if(EnableCdvr.HasValue)
            {
                ret.Add("\"enableCdvr\": " + EnableCdvr.ToString().ToLower());
            }
            if(EnableStartOver.HasValue)
            {
                ret.Add("\"enableStartOver\": " + EnableStartOver.ToString().ToLower());
            }
            if(EnableTrickPlay.HasValue)
            {
                ret.Add("\"enableTrickPlay\": " + EnableTrickPlay.ToString().ToLower());
            }
            if(EndDate.HasValue)
            {
                ret.Add("\"endDate\": " + EndDate);
            }
            if(ExternalId != null)
            {
                ret.Add("\"externalId\": " + "\"" + ExternalId + "\"");
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Images != null && Images.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Images.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"images\": " + propertyValue);
            }
            if(MediaFiles != null && MediaFiles.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", MediaFiles.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"mediaFiles\": " + propertyValue);
            }
            if(Metas != null && Metas.Count > 0)
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
            if(Tags != null && Tags.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", Tags.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"tags\": " + propertyValue);
            }
            if(Type.HasValue)
            {
                ret.Add("\"type\": " + Type);
            }
            if(Crid != null)
            {
                ret.Add("\"crid\": " + "\"" + Crid + "\"");
            }
            if(EpgChannelId.HasValue)
            {
                ret.Add("\"epgChannelId\": " + EpgChannelId);
            }
            if(EpgId != null)
            {
                ret.Add("\"epgId\": " + "\"" + EpgId + "\"");
            }
            if(LinearAssetId.HasValue)
            {
                ret.Add("\"linearAssetId\": " + LinearAssetId);
            }
            if(RelatedMediaId.HasValue)
            {
                ret.Add("\"relatedMediaId\": " + RelatedMediaId);
            }
            if(RecordingId != null)
            {
                ret.Add("\"recordingId\": " + "\"" + RecordingId + "\"");
            }
            if(RecordingType.HasValue)
            {
                ret.Add("\"recordingType\": " + RecordingType.GetHashCode());
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaRelatedExternalFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(DynamicOrderBy != null)
            {
                propertyValue = DynamicOrderBy.ToJson(currentVersion, omitObsolete);
                ret.Add("\"dynamicOrderBy\": " + propertyValue);
            }
            if(FreeText != null)
            {
                ret.Add("\"freeText\": " + "\"" + FreeText + "\"");
            }
            ret.Add("\"idEqual\": " + IdEqual);
            if(TypeIn != null)
            {
                ret.Add("\"typeIn\": " + "\"" + TypeIn + "\"");
            }
            ret.Add("\"utcOffsetEqual\": " + UtcOffsetEqual);
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaRelatedFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(DynamicOrderBy != null)
            {
                propertyValue = DynamicOrderBy.ToJson(currentVersion, omitObsolete);
                ret.Add("\"dynamicOrderBy\": " + propertyValue);
            }
            if(GroupBy != null && GroupBy.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", GroupBy.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"groupBy\": " + propertyValue);
            }
            if(IdEqual.HasValue)
            {
                ret.Add("\"idEqual\": " + IdEqual);
            }
            if(KSql != null)
            {
                ret.Add("\"kSql\": " + "\"" + KSql + "\"");
            }
            if(TypeIn != null)
            {
                ret.Add("\"typeIn\": " + "\"" + TypeIn + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaScheduledRecordingProgramFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(DynamicOrderBy != null)
            {
                propertyValue = DynamicOrderBy.ToJson(currentVersion, omitObsolete);
                ret.Add("\"dynamicOrderBy\": " + propertyValue);
            }
            if(ChannelsIn != null)
            {
                ret.Add("\"channelsIn\": " + "\"" + ChannelsIn + "\"");
            }
            if(EndDateLessThanOrNull.HasValue)
            {
                ret.Add("\"endDateLessThanOrNull\": " + EndDateLessThanOrNull);
            }
            ret.Add("\"recordingTypeEqual\": " + RecordingTypeEqual.GetHashCode());
            if(StartDateGreaterThanOrNull.HasValue)
            {
                ret.Add("\"startDateGreaterThanOrNull\": " + StartDateGreaterThanOrNull);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSearchAssetFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(DynamicOrderBy != null)
            {
                propertyValue = DynamicOrderBy.ToJson(currentVersion, omitObsolete);
                ret.Add("\"dynamicOrderBy\": " + propertyValue);
            }
            if(GroupBy != null && GroupBy.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", GroupBy.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"groupBy\": " + propertyValue);
            }
            if(IdIn != null)
            {
                ret.Add("\"idIn\": " + "\"" + IdIn + "\"");
            }
            if(KSql != null)
            {
                ret.Add("\"kSql\": " + "\"" + KSql + "\"");
            }
            if(TypeIn != null)
            {
                ret.Add("\"typeIn\": " + "\"" + TypeIn + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSearchExternalFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(DynamicOrderBy != null)
            {
                propertyValue = DynamicOrderBy.ToJson(currentVersion, omitObsolete);
                ret.Add("\"dynamicOrderBy\": " + propertyValue);
            }
            if(Query != null)
            {
                ret.Add("\"query\": " + "\"" + Query + "\"");
            }
            if(TypeIn != null)
            {
                ret.Add("\"typeIn\": " + "\"" + TypeIn + "\"");
            }
            ret.Add("\"utcOffsetEqual\": " + UtcOffsetEqual);
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSlimAsset
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + Id + "\"");
            }
            ret.Add("\"type\": " + Type.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSlimAssetInfoWrapper
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaWatchHistoryAsset
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
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
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"finished_watching\": " + IsFinishedWatching.ToString().ToLower());
                }
            }
            if(LastWatched.HasValue)
            {
                ret.Add("\"watchedDate\": " + LastWatched);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"watched_date\": " + LastWatched);
                }
            }
            if(Position.HasValue)
            {
                ret.Add("\"position\": " + Position);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaWatchHistoryAssetWrapper
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
}

namespace WebAPI.Models.Pricing
{
    public partial class KalturaAssetPrice
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(AssetId != null)
            {
                ret.Add("\"asset_id\": " + "\"" + AssetId + "\"");
            }
            ret.Add("\"asset_type\": " + AssetType.GetHashCode());
            if(FilePrices != null && FilePrices.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", FilePrices.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"file_prices\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaCollection
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Channels != null && Channels.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Channels.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"channels\": " + propertyValue);
            }
            if(CouponGroups != null && CouponGroups.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", CouponGroups.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"couponsGroups\": " + propertyValue);
            }
            ret.Add(Description.ToCustomJson(currentVersion, omitObsolete, "description"));
            if(DiscountModule != null)
            {
                propertyValue = DiscountModule.ToJson(currentVersion, omitObsolete);
                ret.Add("\"discountModule\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
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
                ret.Add("\"externalId\": " + "\"" + ExternalId + "\"");
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + Id + "\"");
            }
            ret.Add(Name.ToCustomJson(currentVersion, omitObsolete, "name"));
            if(PriceDetailsId.HasValue)
            {
                ret.Add("\"priceDetailsId\": " + PriceDetailsId);
            }
            if(ProductCodes != null && ProductCodes.Count > 0)
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
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaCollectionFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(CollectionIdIn != null)
            {
                ret.Add("\"collectionIdIn\": " + "\"" + CollectionIdIn + "\"");
            }
            if(MediaFileIdEqual.HasValue)
            {
                ret.Add("\"mediaFileIdEqual\": " + MediaFileIdEqual);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaCollectionListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Collections != null && Collections.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Collections.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaCollectionPrice
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Price != null)
            {
                propertyValue = Price.ToJson(currentVersion, omitObsolete);
                ret.Add("\"price\": " + propertyValue);
            }
            if(ProductId != null)
            {
                ret.Add("\"productId\": " + "\"" + ProductId + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"product_id\": " + "\"" + ProductId + "\"");
                }
            }
            ret.Add("\"productType\": " + ProductType.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"product_type\": " + ProductType.GetHashCode());
            }
            ret.Add("\"purchaseStatus\": " + PurchaseStatus.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaCoupon
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(CouponsGroup != null)
            {
                propertyValue = CouponsGroup.ToJson(currentVersion, omitObsolete);
                ret.Add("\"couponsGroup\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"coupons_group\": " + propertyValue);
                }
            }
            ret.Add("\"status\": " + Status.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaCouponsGroup
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(CouponGroupType.HasValue)
            {
                ret.Add("\"couponGroupType\": " + CouponGroupType.GetHashCode());
            }
            if(Descriptions != null && Descriptions.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Descriptions.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"descriptions\": " + propertyValue);
            }
            if(EndDate.HasValue)
            {
                ret.Add("\"endDate\": " + EndDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"end_date\": " + EndDate);
                }
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + Id + "\"");
            }
            if(MaxUsesNumber.HasValue)
            {
                ret.Add("\"maxUsesNumber\": " + MaxUsesNumber);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"max_uses_number\": " + MaxUsesNumber);
                }
            }
            if(MaxUsesNumberOnRenewableSub.HasValue)
            {
                ret.Add("\"maxUsesNumberOnRenewableSub\": " + MaxUsesNumberOnRenewableSub);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"max_uses_number_on_renewable_sub\": " + MaxUsesNumberOnRenewableSub);
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(StartDate.HasValue)
            {
                ret.Add("\"startDate\": " + StartDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"start_date\": " + StartDate);
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaDiscountModule
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(EndDate.HasValue)
            {
                ret.Add("\"endDate\": " + EndDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
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
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"start_date\": " + StartDate);
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaItemPrice
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Price != null)
            {
                propertyValue = Price.ToJson(currentVersion, omitObsolete);
                ret.Add("\"price\": " + propertyValue);
            }
            if(ProductId != null)
            {
                ret.Add("\"productId\": " + "\"" + ProductId + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"product_id\": " + "\"" + ProductId + "\"");
                }
            }
            ret.Add("\"productType\": " + ProductType.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"product_type\": " + ProductType.GetHashCode());
            }
            ret.Add("\"purchaseStatus\": " + PurchaseStatus.GetHashCode());
            if(FileId.HasValue)
            {
                ret.Add("\"fileId\": " + FileId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"file_id\": " + FileId);
                }
            }
            if(PPVPriceDetails != null && PPVPriceDetails.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", PPVPriceDetails.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"ppvPriceDetails\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"ppv_price_details\": " + propertyValue);
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaItemPriceListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(ItemPrice != null && ItemPrice.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", ItemPrice.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPpv
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(CouponsGroup != null)
            {
                propertyValue = CouponsGroup.ToJson(currentVersion, omitObsolete);
                ret.Add("\"couponsGroup\": " + propertyValue);
            }
            if(Descriptions != null && Descriptions.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Descriptions.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"descriptions\": " + propertyValue);
            }
            if(DiscountModule != null)
            {
                propertyValue = DiscountModule.ToJson(currentVersion, omitObsolete);
                ret.Add("\"discountModule\": " + propertyValue);
            }
            if(FileTypes != null && FileTypes.Count > 0)
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
                ret.Add("\"id\": " + "\"" + Id + "\"");
            }
            if(IsSubscriptionOnly.HasValue)
            {
                ret.Add("\"isSubscriptionOnly\": " + IsSubscriptionOnly.ToString().ToLower());
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(Price != null)
            {
                propertyValue = Price.ToJson(currentVersion, omitObsolete);
                ret.Add("\"price\": " + propertyValue);
            }
            if(ProductCode != null)
            {
                ret.Add("\"productCode\": " + "\"" + ProductCode + "\"");
            }
            if(UsageModule != null)
            {
                propertyValue = UsageModule.ToJson(currentVersion, omitObsolete);
                ret.Add("\"usageModule\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPPVItemPriceDetails
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(CollectionId != null)
            {
                ret.Add("\"collectionId\": " + "\"" + CollectionId + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"collection_id\": " + "\"" + CollectionId + "\"");
                }
            }
            if(DiscountEndDate.HasValue)
            {
                ret.Add("\"discountEndDate\": " + DiscountEndDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"discount_end_date\": " + DiscountEndDate);
                }
            }
            if(EndDate.HasValue)
            {
                ret.Add("\"endDate\": " + EndDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"end_date\": " + EndDate);
                }
            }
            if(FirstDeviceName != null)
            {
                ret.Add("\"firstDeviceName\": " + "\"" + FirstDeviceName + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"first_device_name\": " + "\"" + FirstDeviceName + "\"");
                }
            }
            if(FullPrice != null)
            {
                propertyValue = FullPrice.ToJson(currentVersion, omitObsolete);
                ret.Add("\"fullPrice\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"full_price\": " + propertyValue);
                }
            }
            if(IsInCancelationPeriod.HasValue)
            {
                ret.Add("\"isInCancelationPeriod\": " + IsInCancelationPeriod.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_in_cancelation_period\": " + IsInCancelationPeriod.ToString().ToLower());
                }
            }
            if(IsSubscriptionOnly.HasValue)
            {
                ret.Add("\"isSubscriptionOnly\": " + IsSubscriptionOnly.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_subscription_only\": " + IsSubscriptionOnly.ToString().ToLower());
                }
            }
            if(PPVDescriptions != null && PPVDescriptions.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", PPVDescriptions.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"ppvDescriptions\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"ppv_descriptions\": " + propertyValue);
                }
            }
            if(PPVModuleId != null)
            {
                ret.Add("\"ppvModuleId\": " + "\"" + PPVModuleId + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"ppv_module_id\": " + "\"" + PPVModuleId + "\"");
                }
            }
            if(PrePaidId != null)
            {
                ret.Add("\"prePaidId\": " + "\"" + PrePaidId + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"pre_paid_id\": " + "\"" + PrePaidId + "\"");
                }
            }
            if(Price != null)
            {
                propertyValue = Price.ToJson(currentVersion, omitObsolete);
                ret.Add("\"price\": " + propertyValue);
            }
            if(ProductCode != null)
            {
                ret.Add("\"ppvProductCode\": " + "\"" + ProductCode + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"ppv_product_code\": " + "\"" + ProductCode + "\"");
                }
            }
            if(PurchasedMediaFileId.HasValue)
            {
                ret.Add("\"purchasedMediaFileId\": " + PurchasedMediaFileId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"purchased_media_file_id\": " + PurchasedMediaFileId);
                }
            }
            ret.Add("\"purchaseStatus\": " + PurchaseStatus.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"purchase_status\": " + PurchaseStatus.GetHashCode());
            }
            if(PurchaseUserId != null)
            {
                ret.Add("\"purchaseUserId\": " + "\"" + PurchaseUserId + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"purchase_user_id\": " + "\"" + PurchaseUserId + "\"");
                }
            }
            if(RelatedMediaFileIds != null && RelatedMediaFileIds.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", RelatedMediaFileIds.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"relatedMediaFileIds\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"related_media_file_ids\": " + propertyValue);
                }
            }
            if(StartDate.HasValue)
            {
                ret.Add("\"startDate\": " + StartDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"start_date\": " + StartDate);
                }
            }
            if(SubscriptionId != null)
            {
                ret.Add("\"subscriptionId\": " + "\"" + SubscriptionId + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"subscription_id\": " + "\"" + SubscriptionId + "\"");
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPpvPrice
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Price != null)
            {
                propertyValue = Price.ToJson(currentVersion, omitObsolete);
                ret.Add("\"price\": " + propertyValue);
            }
            if(ProductId != null)
            {
                ret.Add("\"productId\": " + "\"" + ProductId + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"product_id\": " + "\"" + ProductId + "\"");
                }
            }
            ret.Add("\"productType\": " + ProductType.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"product_type\": " + ProductType.GetHashCode());
            }
            ret.Add("\"purchaseStatus\": " + PurchaseStatus.GetHashCode());
            if(CollectionId != null)
            {
                ret.Add("\"collectionId\": " + "\"" + CollectionId + "\"");
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
                ret.Add("\"firstDeviceName\": " + "\"" + FirstDeviceName + "\"");
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
            if(PPVDescriptions != null && PPVDescriptions.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", PPVDescriptions.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"ppvDescriptions\": " + propertyValue);
            }
            if(PPVModuleId != null)
            {
                ret.Add("\"ppvModuleId\": " + "\"" + PPVModuleId + "\"");
            }
            if(PrePaidId != null)
            {
                ret.Add("\"prePaidId\": " + "\"" + PrePaidId + "\"");
            }
            if(ProductCode != null)
            {
                ret.Add("\"ppvProductCode\": " + "\"" + ProductCode + "\"");
            }
            if(PurchasedMediaFileId.HasValue)
            {
                ret.Add("\"purchasedMediaFileId\": " + PurchasedMediaFileId);
            }
            if(PurchaseUserId != null)
            {
                ret.Add("\"purchaseUserId\": " + "\"" + PurchaseUserId + "\"");
            }
            if(RelatedMediaFileIds != null && RelatedMediaFileIds.Count > 0)
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
                ret.Add("\"subscriptionId\": " + "\"" + SubscriptionId + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPreviewModule
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(LifeCycle.HasValue)
            {
                ret.Add("\"lifeCycle\": " + LifeCycle);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"life_cycle\": " + LifeCycle);
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(NonRenewablePeriod.HasValue)
            {
                ret.Add("\"nonRenewablePeriod\": " + NonRenewablePeriod);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"non_renewable_period\": " + NonRenewablePeriod);
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPrice
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
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
                ret.Add("\"currency\": " + "\"" + Currency + "\"");
            }
            if(CurrencySign != null)
            {
                ret.Add("\"currencySign\": " + "\"" + CurrencySign + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"currency_sign\": " + "\"" + CurrencySign + "\"");
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPriceDetails
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Descriptions != null && Descriptions.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Descriptions.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"descriptions\": " + propertyValue);
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(MultiCurrencyPrice != null && MultiCurrencyPrice.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", MultiCurrencyPrice.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"multiCurrencyPrice\": " + propertyValue);
            }
            if(name != null)
            {
                ret.Add("\"name\": " + "\"" + name + "\"");
            }
            if(Price != null)
            {
                propertyValue = Price.ToJson(currentVersion, omitObsolete);
                ret.Add("\"price\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPriceDetailsFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(IdIn != null)
            {
                ret.Add("\"idIn\": " + "\"" + IdIn + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPriceDetailsListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Prices != null && Prices.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Prices.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPricePlan
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(CouponId.HasValue)
            {
                ret.Add("\"couponId\": " + CouponId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"coupon_id\": " + CouponId);
                }
            }
            if(FullLifeCycle.HasValue)
            {
                ret.Add("\"fullLifeCycle\": " + FullLifeCycle);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
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
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_offline_playback\": " + IsOfflinePlayback.ToString().ToLower());
                }
            }
            if(IsWaiverEnabled.HasValue)
            {
                ret.Add("\"isWaiverEnabled\": " + IsWaiverEnabled.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_waiver_enabled\": " + IsWaiverEnabled.ToString().ToLower());
                }
            }
            if(MaxViewsNumber.HasValue)
            {
                ret.Add("\"maxViewsNumber\": " + MaxViewsNumber);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"max_views_number\": " + MaxViewsNumber);
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(ViewLifeCycle.HasValue)
            {
                ret.Add("\"viewLifeCycle\": " + ViewLifeCycle);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"view_life_cycle\": " + ViewLifeCycle);
                }
            }
            if(WaiverPeriod.HasValue)
            {
                ret.Add("\"waiverPeriod\": " + WaiverPeriod);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"waiver_period\": " + WaiverPeriod);
                }
            }
            if(DiscountId.HasValue)
            {
                ret.Add("\"discountId\": " + DiscountId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"discount_id\": " + DiscountId);
                }
            }
            if(IsRenewable.HasValue)
            {
                ret.Add("\"isRenewable\": " + IsRenewable.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
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
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"price_id\": " + PriceId);
                }
            }
            if(RenewalsNumber.HasValue)
            {
                ret.Add("\"renewalsNumber\": " + RenewalsNumber);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"renewals_number\": " + RenewalsNumber);
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPricePlanFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(IdIn != null)
            {
                ret.Add("\"idIn\": " + "\"" + IdIn + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPricePlanListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(PricePlans != null && PricePlans.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", PricePlans.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaProductCode
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Code != null)
            {
                ret.Add("\"code\": " + "\"" + Code + "\"");
            }
            if(InappProvider != null)
            {
                ret.Add("\"inappProvider\": " + "\"" + InappProvider + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaProductPrice
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Price != null)
            {
                propertyValue = Price.ToJson(currentVersion, omitObsolete);
                ret.Add("\"price\": " + propertyValue);
            }
            if(ProductId != null)
            {
                ret.Add("\"productId\": " + "\"" + ProductId + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"product_id\": " + "\"" + ProductId + "\"");
                }
            }
            ret.Add("\"productType\": " + ProductType.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"product_type\": " + ProductType.GetHashCode());
            }
            ret.Add("\"purchaseStatus\": " + PurchaseStatus.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaProductPriceListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(ProductsPrices != null && ProductsPrices.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", ProductsPrices.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaProductsPriceListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(ProductsPrices != null && ProductsPrices.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", ProductsPrices.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSubscription
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Channels != null && Channels.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Channels.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"channels\": " + propertyValue);
            }
            if(CouponGroups != null && CouponGroups.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", CouponGroups.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"couponsGroups\": " + propertyValue);
            }
            if(!DeprecatedAttribute.IsDeprecated("4.3.0.0", currentVersion) && CouponsGroup != null)
            {
                propertyValue = CouponsGroup.ToJson(currentVersion, omitObsolete);
                ret.Add("\"couponsGroup\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"coupons_group\": " + propertyValue);
                }
            }
            ret.Add("\"dependencyType\": " + DependencyType.GetHashCode());
            ret.Add(Description.ToCustomJson(currentVersion, omitObsolete, "description"));
            if(!DeprecatedAttribute.IsDeprecated("3.6.287.27312", currentVersion) && Descriptions != null && Descriptions.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Descriptions.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"descriptions\": " + propertyValue);
            }
            if(DiscountModule != null)
            {
                propertyValue = DiscountModule.ToJson(currentVersion, omitObsolete);
                ret.Add("\"discountModule\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"discount_module\": " + propertyValue);
                }
            }
            if(EndDate.HasValue)
            {
                ret.Add("\"endDate\": " + EndDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"end_date\": " + EndDate);
                }
            }
            if(ExternalId != null)
            {
                ret.Add("\"externalId\": " + "\"" + ExternalId + "\"");
            }
            if(FileTypes != null && FileTypes.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", FileTypes.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"fileTypes\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"file_types\": " + propertyValue);
                }
            }
            if(GracePeriodMinutes.HasValue)
            {
                ret.Add("\"gracePeriodMinutes\": " + GracePeriodMinutes);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"grace_period_minutes\": " + GracePeriodMinutes);
                }
            }
            if(HouseholdLimitationsId.HasValue)
            {
                ret.Add("\"householdLimitationsId\": " + HouseholdLimitationsId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"household_limitations_id\": " + HouseholdLimitationsId);
                }
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + Id + "\"");
            }
            ret.Add("\"isCancellationBlocked\": " + IsCancellationBlocked.ToString().ToLower());
            if(IsInfiniteRenewal.HasValue)
            {
                ret.Add("\"isInfiniteRenewal\": " + IsInfiniteRenewal.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_infinite_renewal\": " + IsInfiniteRenewal.ToString().ToLower());
                }
            }
            if(IsRenewable.HasValue)
            {
                ret.Add("\"isRenewable\": " + IsRenewable.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_renewable\": " + IsRenewable.ToString().ToLower());
                }
            }
            if(IsWaiverEnabled.HasValue)
            {
                ret.Add("\"isWaiverEnabled\": " + IsWaiverEnabled.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_waiver_enabled\": " + IsWaiverEnabled.ToString().ToLower());
                }
            }
            if(MaxViewsNumber.HasValue)
            {
                ret.Add("\"maxViewsNumber\": " + MaxViewsNumber);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"max_views_number\": " + MaxViewsNumber);
                }
            }
            if(MediaId.HasValue)
            {
                ret.Add("\"mediaId\": " + MediaId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"media_id\": " + MediaId);
                }
            }
            ret.Add(Name.ToCustomJson(currentVersion, omitObsolete, "name"));
            if(!DeprecatedAttribute.IsDeprecated("3.6.287.27312", currentVersion) && Names != null && Names.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Names.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"names\": " + propertyValue);
            }
            if(PremiumServices != null && PremiumServices.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", PremiumServices.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"premiumServices\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"premium_services\": " + propertyValue);
                }
            }
            if(PreviewModule != null)
            {
                propertyValue = PreviewModule.ToJson(currentVersion, omitObsolete);
                ret.Add("\"previewModule\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
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
                ret.Add("\"pricePlanIds\": " + "\"" + PricePlanIds + "\"");
            }
            if(!DeprecatedAttribute.IsDeprecated("4.5.0.0", currentVersion) && PricePlans != null && PricePlans.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", PricePlans.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"pricePlans\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"price_plans\": " + propertyValue);
                }
            }
            if(!DeprecatedAttribute.IsDeprecated("4.3.0.0", currentVersion) && ProductCode != null)
            {
                ret.Add("\"productCode\": " + "\"" + ProductCode + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"product_code\": " + "\"" + ProductCode + "\"");
                }
            }
            if(ProductCodes != null && ProductCodes.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", ProductCodes.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"productCodes\": " + propertyValue);
            }
            if(ProrityInOrder.HasValue)
            {
                ret.Add("\"prorityInOrder\": " + ProrityInOrder);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"prority_in_order\": " + ProrityInOrder);
                }
            }
            if(RenewalsNumber.HasValue)
            {
                ret.Add("\"renewalsNumber\": " + RenewalsNumber);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"renewals_number\": " + RenewalsNumber);
                }
            }
            if(StartDate.HasValue)
            {
                ret.Add("\"startDate\": " + StartDate);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"start_date\": " + StartDate);
                }
            }
            if(UserTypes != null && UserTypes.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", UserTypes.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"userTypes\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"user_types\": " + propertyValue);
                }
            }
            if(ViewLifeCycle.HasValue)
            {
                ret.Add("\"viewLifeCycle\": " + ViewLifeCycle);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"view_life_cycle\": " + ViewLifeCycle);
                }
            }
            if(WaiverPeriod.HasValue)
            {
                ret.Add("\"waiverPeriod\": " + WaiverPeriod);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"waiver_period\": " + WaiverPeriod);
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSubscriptionDependencySet
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"id\": " + Id);
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(SubscriptionIds != null)
            {
                ret.Add("\"subscriptionIds\": " + "\"" + SubscriptionIds + "\"");
            }
            if(Type.HasValue)
            {
                ret.Add("\"type\": " + Type.GetHashCode());
            }
            if(BaseSubscriptionId.HasValue)
            {
                ret.Add("\"baseSubscriptionId\": " + BaseSubscriptionId);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSubscriptionDependencySetFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(IdIn != null)
            {
                ret.Add("\"idIn\": " + "\"" + IdIn + "\"");
            }
            if(SubscriptionIdContains != null)
            {
                ret.Add("\"subscriptionIdContains\": " + "\"" + SubscriptionIdContains + "\"");
            }
            if(TypeEqual.HasValue)
            {
                ret.Add("\"typeEqual\": " + TypeEqual.GetHashCode());
            }
            if(BaseSubscriptionIdIn != null)
            {
                ret.Add("\"baseSubscriptionIdIn\": " + "\"" + BaseSubscriptionIdIn + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSubscriptionFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(ExternalIdIn != null)
            {
                ret.Add("\"externalIdIn\": " + "\"" + ExternalIdIn + "\"");
            }
            if(MediaFileIdEqual.HasValue)
            {
                ret.Add("\"mediaFileIdEqual\": " + MediaFileIdEqual);
            }
            if(SubscriptionIdIn != null)
            {
                ret.Add("\"subscriptionIdIn\": " + "\"" + SubscriptionIdIn + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSubscriptionListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Subscriptions != null && Subscriptions.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Subscriptions.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSubscriptionPrice
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Price != null)
            {
                propertyValue = Price.ToJson(currentVersion, omitObsolete);
                ret.Add("\"price\": " + propertyValue);
            }
            if(ProductId != null)
            {
                ret.Add("\"productId\": " + "\"" + ProductId + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"product_id\": " + "\"" + ProductId + "\"");
                }
            }
            ret.Add("\"productType\": " + ProductType.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"product_type\": " + ProductType.GetHashCode());
            }
            ret.Add("\"purchaseStatus\": " + PurchaseStatus.GetHashCode());
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
                ret.Add("\"purchaseStatus\": " + PurchaseStatus.GetHashCode());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"purchase_status\": " + PurchaseStatus.GetHashCode());
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSubscriptionSet
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"id\": " + Id);
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(SubscriptionIds != null)
            {
                ret.Add("\"subscriptionIds\": " + "\"" + SubscriptionIds + "\"");
            }
            if(Type.HasValue)
            {
                ret.Add("\"type\": " + Type.GetHashCode());
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSubscriptionSetFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(IdIn != null)
            {
                ret.Add("\"idIn\": " + "\"" + IdIn + "\"");
            }
            if(SubscriptionIdContains != null)
            {
                ret.Add("\"subscriptionIdContains\": " + "\"" + SubscriptionIdContains + "\"");
            }
            if(TypeEqual.HasValue)
            {
                ret.Add("\"typeEqual\": " + TypeEqual.GetHashCode());
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSubscriptionSetListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(SubscriptionSets != null && SubscriptionSets.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", SubscriptionSets.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSubscriptionsFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"by\": " + By.GetHashCode());
            if(Ids != null && Ids.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Ids.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"ids\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSubscriptionSwitchSet
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"id\": " + Id);
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(SubscriptionIds != null)
            {
                ret.Add("\"subscriptionIds\": " + "\"" + SubscriptionIds + "\"");
            }
            if(Type.HasValue)
            {
                ret.Add("\"type\": " + Type.GetHashCode());
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaUsageModule
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(CouponId.HasValue)
            {
                ret.Add("\"couponId\": " + CouponId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"coupon_id\": " + CouponId);
                }
            }
            if(FullLifeCycle.HasValue)
            {
                ret.Add("\"fullLifeCycle\": " + FullLifeCycle);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
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
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_offline_playback\": " + IsOfflinePlayback.ToString().ToLower());
                }
            }
            if(IsWaiverEnabled.HasValue)
            {
                ret.Add("\"isWaiverEnabled\": " + IsWaiverEnabled.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_waiver_enabled\": " + IsWaiverEnabled.ToString().ToLower());
                }
            }
            if(MaxViewsNumber.HasValue)
            {
                ret.Add("\"maxViewsNumber\": " + MaxViewsNumber);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"max_views_number\": " + MaxViewsNumber);
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(ViewLifeCycle.HasValue)
            {
                ret.Add("\"viewLifeCycle\": " + ViewLifeCycle);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"view_life_cycle\": " + ViewLifeCycle);
                }
            }
            if(WaiverPeriod.HasValue)
            {
                ret.Add("\"waiverPeriod\": " + WaiverPeriod);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"waiver_period\": " + WaiverPeriod);
                }
            }
            return String.Join(", ", ret);
        }
    }
}

namespace WebAPI.Models.Users
{
    public partial class KalturaBaseOTTUser
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(FirstName != null)
            {
                ret.Add("\"firstName\": " + "\"" + FirstName + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"first_name\": " + "\"" + FirstName + "\"");
                }
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + Id + "\"");
            }
            if(LastName != null)
            {
                ret.Add("\"lastName\": " + "\"" + LastName + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"last_name\": " + "\"" + LastName + "\"");
                }
            }
            if(Username != null)
            {
                ret.Add("\"username\": " + "\"" + Username + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaCountry
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Code != null)
            {
                ret.Add("\"code\": " + "\"" + Code + "\"");
            }
            if(CurrencyCode != null)
            {
                ret.Add("\"currency\": " + "\"" + CurrencyCode + "\"");
            }
            if(CurrencySign != null)
            {
                ret.Add("\"currencySign\": " + "\"" + CurrencySign + "\"");
            }
            ret.Add("\"id\": " + Id);
            if(LanguagesCode != null)
            {
                ret.Add("\"languagesCode\": " + "\"" + LanguagesCode + "\"");
            }
            if(MainLanguageCode != null)
            {
                ret.Add("\"mainLanguageCode\": " + "\"" + MainLanguageCode + "\"");
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(VatPercent.HasValue)
            {
                ret.Add("\"vatPercent\": " + VatPercent);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaFavorite
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(!omitObsolete && Asset != null)
            {
                propertyValue = Asset.ToJson(currentVersion, omitObsolete);
                ret.Add("\"asset\": " + propertyValue);
            }
            ret.Add("\"assetId\": " + AssetId);
            ret.Add("\"createDate\": " + CreateDate);
            if(ExtraData != null)
            {
                ret.Add("\"extraData\": " + "\"" + ExtraData + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"extra_data\": " + "\"" + ExtraData + "\"");
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaFavoriteFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(MediaIdIn != null)
            {
                ret.Add("\"mediaIdIn\": " + "\"" + MediaIdIn + "\"");
            }
            if(!omitObsolete && MediaIds != null && MediaIds.Count > 0)
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
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"media_type\": " + MediaTypeIn);
                }
            }
            if(!omitObsolete && UDID != null)
            {
                ret.Add("\"udid\": " + "\"" + UDID + "\"");
            }
            if(UdidEqualCurrent.HasValue)
            {
                ret.Add("\"udidEqualCurrent\": " + UdidEqualCurrent.ToString().ToLower());
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaFavoriteListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Favorites != null && Favorites.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Favorites.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaLoginResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(LoginSession != null)
            {
                propertyValue = LoginSession.ToJson(currentVersion, omitObsolete);
                ret.Add("\"loginSession\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"login_session\": " + propertyValue);
                }
            }
            if(User != null)
            {
                propertyValue = User.ToJson(currentVersion, omitObsolete);
                ret.Add("\"user\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaLoginSession
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(KS != null)
            {
                ret.Add("\"ks\": " + "\"" + KS + "\"");
            }
            if(!omitObsolete && RefreshToken != null)
            {
                ret.Add("\"refreshToken\": " + "\"" + RefreshToken + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"refresh_token\": " + "\"" + RefreshToken + "\"");
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaOTTUser
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(FirstName != null)
            {
                ret.Add("\"firstName\": " + "\"" + FirstName + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"first_name\": " + "\"" + FirstName + "\"");
                }
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + Id + "\"");
            }
            if(LastName != null)
            {
                ret.Add("\"lastName\": " + "\"" + LastName + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"last_name\": " + "\"" + LastName + "\"");
                }
            }
            if(Username != null)
            {
                ret.Add("\"username\": " + "\"" + Username + "\"");
            }
            if(Address != null)
            {
                ret.Add("\"address\": " + "\"" + Address + "\"");
            }
            if(AffiliateCode != null)
            {
                ret.Add("\"affiliateCode\": " + "\"" + AffiliateCode + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"affiliate_code\": " + "\"" + AffiliateCode + "\"");
                }
            }
            if(City != null)
            {
                ret.Add("\"city\": " + "\"" + City + "\"");
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
            if(DynamicData != null && DynamicData.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", DynamicData.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"dynamicData\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"dynamic_data\": " + propertyValue);
                }
            }
            if(Email != null)
            {
                ret.Add("\"email\": " + "\"" + Email + "\"");
            }
            if(ExternalId != null)
            {
                ret.Add("\"externalId\": " + "\"" + ExternalId + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"external_id\": " + "\"" + ExternalId + "\"");
                }
            }
            if(!omitObsolete && FacebookId != null)
            {
                ret.Add("\"facebookId\": " + "\"" + FacebookId + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"facebook_id\": " + "\"" + FacebookId + "\"");
                }
            }
            if(!omitObsolete && FacebookImage != null)
            {
                ret.Add("\"facebookImage\": " + "\"" + FacebookImage + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"facebook_image\": " + "\"" + FacebookImage + "\"");
                }
            }
            if(!omitObsolete && FacebookToken != null)
            {
                ret.Add("\"facebookToken\": " + "\"" + FacebookToken + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"facebook_token\": " + "\"" + FacebookToken + "\"");
                }
            }
            if(HouseholdID.HasValue)
            {
                ret.Add("\"householdId\": " + HouseholdID);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"household_id\": " + HouseholdID);
                }
            }
            if(IsHouseholdMaster.HasValue)
            {
                ret.Add("\"isHouseholdMaster\": " + IsHouseholdMaster.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_household_master\": " + IsHouseholdMaster.ToString().ToLower());
                }
            }
            if(Phone != null)
            {
                ret.Add("\"phone\": " + "\"" + Phone + "\"");
            }
            ret.Add("\"suspensionState\": " + SuspensionState.GetHashCode());
            if(!omitObsolete)
            {
                ret.Add("\"suspentionState\": " + SuspentionState.GetHashCode());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"suspention_state\": " + SuspentionState.GetHashCode());
                }
            }
            ret.Add("\"userState\": " + UserState.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"user_state\": " + UserState.GetHashCode());
            }
            if(UserType != null)
            {
                propertyValue = UserType.ToJson(currentVersion, omitObsolete);
                ret.Add("\"userType\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"user_type\": " + propertyValue);
                }
            }
            if(Zip != null)
            {
                ret.Add("\"zip\": " + "\"" + Zip + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaOTTUserDynamicData
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Key != null)
            {
                ret.Add("\"key\": " + "\"" + Key + "\"");
            }
            if(UserId != null)
            {
                ret.Add("\"userId\": " + "\"" + UserId + "\"");
            }
            if(Value != null)
            {
                propertyValue = Value.ToJson(currentVersion, omitObsolete);
                ret.Add("\"value\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaOTTUserFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(ExternalIdEqual != null)
            {
                ret.Add("\"externalIdEqual\": " + "\"" + ExternalIdEqual + "\"");
            }
            if(IdIn != null)
            {
                ret.Add("\"idIn\": " + "\"" + IdIn + "\"");
            }
            if(UsernameEqual != null)
            {
                ret.Add("\"usernameEqual\": " + "\"" + UsernameEqual + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaOTTUserListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Users != null && Users.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Users.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaOTTUserType
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Description != null)
            {
                ret.Add("\"description\": " + "\"" + Description + "\"");
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSession
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"createDate\": " + createDate);
            if(expiry.HasValue)
            {
                ret.Add("\"expiry\": " + expiry);
            }
            if(ks != null)
            {
                ret.Add("\"ks\": " + "\"" + ks + "\"");
            }
            if(partnerId.HasValue)
            {
                ret.Add("\"partnerId\": " + partnerId);
            }
            if(privileges != null)
            {
                ret.Add("\"privileges\": " + "\"" + privileges + "\"");
            }
            if(!DeprecatedAttribute.IsDeprecated("4.5.0.0", currentVersion))
            {
                ret.Add("\"sessionType\": " + sessionType.GetHashCode());
            }
            if(udid != null)
            {
                ret.Add("\"udid\": " + "\"" + udid + "\"");
            }
            if(userId != null)
            {
                ret.Add("\"userId\": " + "\"" + userId + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSessionInfo
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"createDate\": " + createDate);
            if(expiry.HasValue)
            {
                ret.Add("\"expiry\": " + expiry);
            }
            if(ks != null)
            {
                ret.Add("\"ks\": " + "\"" + ks + "\"");
            }
            if(partnerId.HasValue)
            {
                ret.Add("\"partnerId\": " + partnerId);
            }
            if(privileges != null)
            {
                ret.Add("\"privileges\": " + "\"" + privileges + "\"");
            }
            if(!DeprecatedAttribute.IsDeprecated("4.5.0.0", currentVersion))
            {
                ret.Add("\"sessionType\": " + sessionType.GetHashCode());
            }
            if(udid != null)
            {
                ret.Add("\"udid\": " + "\"" + udid + "\"");
            }
            if(userId != null)
            {
                ret.Add("\"userId\": " + "\"" + userId + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaUserAssetsList
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(List != null && List.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", List.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"list\": " + propertyValue);
            }
            ret.Add("\"listType\": " + ListType.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"list_type\": " + ListType.GetHashCode());
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaUserAssetsListFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"assetTypeEqual\": " + AssetTypeEqual.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"asset_type\": " + AssetTypeEqual.GetHashCode());
            }
            ret.Add("\"by\": " + By.GetHashCode());
            ret.Add("\"listTypeEqual\": " + ListTypeEqual.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"list_type\": " + ListTypeEqual.GetHashCode());
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaUserAssetsListItem
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + Id + "\"");
            }
            ret.Add("\"listType\": " + ListType.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"list_type\": " + ListType.GetHashCode());
            }
            if(OrderIndex.HasValue)
            {
                ret.Add("\"orderIndex\": " + OrderIndex);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"order_index\": " + OrderIndex);
                }
            }
            ret.Add("\"type\": " + Type.GetHashCode());
            if(UserId != null)
            {
                ret.Add("\"userId\": " + "\"" + UserId + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"user_id\": " + "\"" + UserId + "\"");
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaUserInterest
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + Id + "\"");
            }
            if(Topic != null)
            {
                propertyValue = Topic.ToJson(currentVersion, omitObsolete);
                ret.Add("\"topic\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaUserInterestListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(UserInterests != null && UserInterests.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", UserInterests.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaUserInterestTopic
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(MetaId != null)
            {
                ret.Add("\"metaId\": " + "\"" + MetaId + "\"");
            }
            if(ParentTopic != null)
            {
                propertyValue = ParentTopic.ToJson(currentVersion, omitObsolete);
                ret.Add("\"parentTopic\": " + propertyValue);
            }
            if(Value != null)
            {
                ret.Add("\"value\": " + "\"" + Value + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaUserLoginPin
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(ExpirationTime.HasValue)
            {
                ret.Add("\"expirationTime\": " + ExpirationTime);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"expiration_time\": " + ExpirationTime);
                }
            }
            if(PinCode != null)
            {
                ret.Add("\"pinCode\": " + "\"" + PinCode + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"pin_code\": " + "\"" + PinCode + "\"");
                }
            }
            if(UserId != null)
            {
                ret.Add("\"userId\": " + "\"" + UserId + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"user_id\": " + "\"" + UserId + "\"");
                }
            }
            return String.Join(", ", ret);
        }
    }
}

namespace WebAPI.Models.Partner
{
    public partial class KalturaBillingPartnerConfig
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(!omitObsolete && PartnerConfigurationType != null)
            {
                propertyValue = PartnerConfigurationType.ToJson(currentVersion, omitObsolete);
                ret.Add("\"partnerConfigurationType\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"partner_configuration_type\": " + propertyValue);
                }
            }
            ret.Add("\"type\": " + Type.GetHashCode());
            if(Value != null)
            {
                ret.Add("\"value\": " + "\"" + Value + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPartnerConfiguration
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPartnerConfigurationHolder
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"type\": " + type.GetHashCode());
            return String.Join(", ", ret);
        }
    }
}

namespace WebAPI.Models.API
{
    public partial class KalturaCDNAdapterProfile
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(AdapterUrl != null)
            {
                ret.Add("\"adapterUrl\": " + "\"" + AdapterUrl + "\"");
            }
            if(BaseUrl != null)
            {
                ret.Add("\"baseUrl\": " + "\"" + BaseUrl + "\"");
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
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(Settings != null && Settings.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", Settings.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"settings\": " + propertyValue);
            }
            if(SharedSecret != null)
            {
                ret.Add("\"sharedSecret\": " + "\"" + SharedSecret + "\"");
            }
            if(SystemName != null)
            {
                ret.Add("\"systemName\": " + "\"" + SystemName + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaCDNAdapterProfileListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Adapters != null && Adapters.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Adapters.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaCDNPartnerSettings
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(DefaultAdapterId.HasValue)
            {
                ret.Add("\"defaultAdapterId\": " + DefaultAdapterId);
            }
            if(DefaultRecordingAdapterId.HasValue)
            {
                ret.Add("\"defaultRecordingAdapterId\": " + DefaultRecordingAdapterId);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaChannelEnrichmentHolder
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"type\": " + type.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaChannelProfile
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(AssetTypes != null && AssetTypes.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", AssetTypes.Select(item => item.ToString())) + "]";
                ret.Add("\"assetTypes\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"asset_types\": " + propertyValue);
                }
            }
            if(Description != null)
            {
                ret.Add("\"description\": " + "\"" + Description + "\"");
            }
            if(FilterExpression != null)
            {
                ret.Add("\"filterExpression\": " + "\"" + FilterExpression + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"filter_expression\": " + "\"" + FilterExpression + "\"");
                }
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(IsActive.HasValue)
            {
                ret.Add("\"isActive\": " + IsActive.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_active\": " + IsActive.ToString().ToLower());
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            ret.Add("\"order\": " + Order.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaCountryFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(IdIn != null)
            {
                ret.Add("\"idIn\": " + "\"" + IdIn + "\"");
            }
            if(IpEqual != null)
            {
                ret.Add("\"ipEqual\": " + "\"" + IpEqual + "\"");
            }
            if(IpEqualCurrent.HasValue)
            {
                ret.Add("\"ipEqualCurrent\": " + IpEqualCurrent.ToString().ToLower());
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaCountryListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaCurrency
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Code != null)
            {
                ret.Add("\"code\": " + "\"" + Code + "\"");
            }
            ret.Add("\"isDefault\": " + IsDefault.ToString().ToLower());
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(Sign != null)
            {
                ret.Add("\"sign\": " + "\"" + Sign + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaCurrencyFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(CodeIn != null)
            {
                ret.Add("\"codeIn\": " + "\"" + CodeIn + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaCurrencyListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaDeviceBrandListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaDeviceFamilyListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaExportFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(ids != null && ids.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", ids.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"ids\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaExportTask
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Alias != null)
            {
                ret.Add("\"alias\": " + "\"" + Alias + "\"");
            }
            ret.Add("\"dataType\": " + DataType.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"data_type\": " + DataType.GetHashCode());
            }
            ret.Add("\"exportType\": " + ExportType.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"export_type\": " + ExportType.GetHashCode());
            }
            if(Filter != null)
            {
                ret.Add("\"filter\": " + "\"" + Filter + "\"");
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
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_active\": " + IsActive.ToString().ToLower());
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(NotificationUrl != null)
            {
                ret.Add("\"notificationUrl\": " + "\"" + NotificationUrl + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"notification_url\": " + "\"" + NotificationUrl + "\"");
                }
            }
            if(VodTypes != null && VodTypes.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", VodTypes.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"vodTypes\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"vod_types\": " + propertyValue);
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaExportTaskFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(IdIn != null)
            {
                ret.Add("\"idIn\": " + "\"" + IdIn + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaExportTaskListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaExternalChannelProfile
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Enrichments != null && Enrichments.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Enrichments.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"enrichments\": " + propertyValue);
            }
            if(ExternalIdentifier != null)
            {
                ret.Add("\"externalIdentifier\": " + "\"" + ExternalIdentifier + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"external_identifier\": " + "\"" + ExternalIdentifier + "\"");
                }
            }
            if(FilterExpression != null)
            {
                ret.Add("\"filterExpression\": " + "\"" + FilterExpression + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"filter_expression\": " + "\"" + FilterExpression + "\"");
                }
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(IsActive.HasValue)
            {
                ret.Add("\"isActive\": " + IsActive.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_active\": " + IsActive.ToString().ToLower());
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(RecommendationEngineId.HasValue)
            {
                ret.Add("\"recommendationEngineId\": " + RecommendationEngineId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"recommendation_engine_id\": " + RecommendationEngineId);
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaExternalChannelProfileListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaGenericRule
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Description != null)
            {
                ret.Add("\"description\": " + "\"" + Description + "\"");
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            ret.Add("\"ruleType\": " + RuleType.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"rule_type\": " + RuleType.GetHashCode());
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaGenericRuleFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(AssetId.HasValue)
            {
                ret.Add("\"assetId\": " + AssetId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"asset_id\": " + AssetId);
                }
            }
            if(AssetType.HasValue)
            {
                ret.Add("\"assetType\": " + AssetType);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"asset_type\": " + AssetType);
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaGenericRuleListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(GenericRules != null && GenericRules.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", GenericRules.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaLanguage
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Code != null)
            {
                ret.Add("\"code\": " + "\"" + Code + "\"");
            }
            if(Direction != null)
            {
                ret.Add("\"direction\": " + "\"" + Direction + "\"");
            }
            ret.Add("\"isDefault\": " + IsDefault.ToString().ToLower());
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(SystemName != null)
            {
                ret.Add("\"systemName\": " + "\"" + SystemName + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaLanguageFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(CodeIn != null)
            {
                ret.Add("\"codeIn\": " + "\"" + CodeIn + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaLanguageListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaMeta
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"assetType\": " + AssetType.GetHashCode());
            if(Features != null)
            {
                ret.Add("\"features\": " + "\"" + Features + "\"");
            }
            ret.Add("\"fieldName\": " + FieldName.GetHashCode());
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + Id + "\"");
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(ParentId != null)
            {
                ret.Add("\"parentId\": " + "\"" + ParentId + "\"");
            }
            ret.Add("\"partnerId\": " + PartnerId);
            ret.Add("\"type\": " + Type.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaMetaFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(AssetTypeEqual.HasValue)
            {
                ret.Add("\"assetTypeEqual\": " + AssetTypeEqual.GetHashCode());
            }
            if(FeaturesIn != null)
            {
                ret.Add("\"featuresIn\": " + "\"" + FeaturesIn + "\"");
            }
            if(FieldNameEqual.HasValue)
            {
                ret.Add("\"fieldNameEqual\": " + FieldNameEqual.GetHashCode());
            }
            if(FieldNameNotEqual.HasValue)
            {
                ret.Add("\"fieldNameNotEqual\": " + FieldNameNotEqual.GetHashCode());
            }
            if(TypeEqual.HasValue)
            {
                ret.Add("\"typeEqual\": " + TypeEqual.GetHashCode());
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaMetaListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaOSSAdapterBaseProfile
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaOSSAdapterProfile
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(AdapterUrl != null)
            {
                ret.Add("\"adapterUrl\": " + "\"" + AdapterUrl + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"adapter_url\": " + "\"" + AdapterUrl + "\"");
                }
            }
            if(ExternalIdentifier != null)
            {
                ret.Add("\"externalIdentifier\": " + "\"" + ExternalIdentifier + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"external_identifier\": " + "\"" + ExternalIdentifier + "\"");
                }
            }
            if(IsActive.HasValue)
            {
                ret.Add("\"isActive\": " + IsActive.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_active\": " + IsActive.ToString().ToLower());
                }
            }
            if(Settings != null && Settings.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", Settings.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"ossAdapterSettings\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"oss_adapter_settings\": " + propertyValue);
                }
            }
            if(SharedSecret != null)
            {
                ret.Add("\"sharedSecret\": " + "\"" + SharedSecret + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"shared_secret\": " + "\"" + SharedSecret + "\"");
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaOSSAdapterProfileListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(OSSAdapterProfiles != null && OSSAdapterProfiles.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", OSSAdapterProfiles.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaParentalRule
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(blockAnonymousAccess.HasValue)
            {
                ret.Add("\"blockAnonymousAccess\": " + blockAnonymousAccess.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"block_anonymous_access\": " + blockAnonymousAccess.ToString().ToLower());
                }
            }
            if(description != null)
            {
                ret.Add("\"description\": " + "\"" + description + "\"");
            }
            if(epgTagTypeId.HasValue)
            {
                ret.Add("\"epgTag\": " + epgTagTypeId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"epg_tag\": " + epgTagTypeId);
                }
            }
            if(epgTagValues != null && epgTagValues.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", epgTagValues.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"epgTagValues\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"epg_tag_values\": " + propertyValue);
                }
            }
            if(id.HasValue)
            {
                ret.Add("\"id\": " + id);
            }
            if(isDefault.HasValue)
            {
                ret.Add("\"isDefault\": " + isDefault.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_default\": " + isDefault.ToString().ToLower());
                }
            }
            if(mediaTagTypeId.HasValue)
            {
                ret.Add("\"mediaTag\": " + mediaTagTypeId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"media_tag\": " + mediaTagTypeId);
                }
            }
            if(mediaTagValues != null && mediaTagValues.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", mediaTagValues.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"mediaTagValues\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"media_tag_values\": " + propertyValue);
                }
            }
            if(name != null)
            {
                ret.Add("\"name\": " + "\"" + name + "\"");
            }
            if(order.HasValue)
            {
                ret.Add("\"order\": " + order);
            }
            ret.Add("\"origin\": " + Origin.GetHashCode());
            ret.Add("\"ruleType\": " + ruleType.GetHashCode());
            if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
            {
                ret.Add("\"rule_type\": " + ruleType.GetHashCode());
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaParentalRuleFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(EntityReferenceEqual.HasValue)
            {
                ret.Add("\"entityReferenceEqual\": " + EntityReferenceEqual.GetHashCode());
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaParentalRuleListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(ParentalRule != null && ParentalRule.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", ParentalRule.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPermission
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(PermissionItems != null && PermissionItems.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", PermissionItems.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"permissionItems\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPermissionItem
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            ret.Add("\"isExcluded\": " + IsExcluded.ToString().ToLower());
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPermissionsFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Ids != null && Ids.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Ids.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"ids\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPin
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"origin\": " + Origin.GetHashCode());
            if(PIN != null)
            {
                ret.Add("\"pin\": " + "\"" + PIN + "\"");
            }
            ret.Add("\"type\": " + Type.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPinResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"origin\": " + Origin.GetHashCode());
            if(PIN != null)
            {
                ret.Add("\"pin\": " + "\"" + PIN + "\"");
            }
            ret.Add("\"type\": " + Type.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPurchaseSettings
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"origin\": " + Origin.GetHashCode());
            if(PIN != null)
            {
                ret.Add("\"pin\": " + "\"" + PIN + "\"");
            }
            ret.Add("\"type\": " + Type.GetHashCode());
            if(Permission.HasValue)
            {
                ret.Add("\"permission\": " + Permission.GetHashCode());
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPurchaseSettingsResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"origin\": " + Origin.GetHashCode());
            if(PIN != null)
            {
                ret.Add("\"pin\": " + "\"" + PIN + "\"");
            }
            ret.Add("\"type\": " + Type.GetHashCode());
            if(PurchaseSettingsType.HasValue)
            {
                ret.Add("\"purchaseSettingsType\": " + PurchaseSettingsType.GetHashCode());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"purchase_settings_type\": " + PurchaseSettingsType.GetHashCode());
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaRecommendationProfile
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(AdapterUrl != null)
            {
                ret.Add("\"adapterUrl\": " + "\"" + AdapterUrl + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"adapter_url\": " + "\"" + AdapterUrl + "\"");
                }
            }
            if(ExternalIdentifier != null)
            {
                ret.Add("\"externalIdentifier\": " + "\"" + ExternalIdentifier + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"external_identifier\": " + "\"" + ExternalIdentifier + "\"");
                }
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(IsActive.HasValue)
            {
                ret.Add("\"isActive\": " + IsActive.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_active\": " + IsActive.ToString().ToLower());
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(Settings != null && Settings.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", Settings.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"recommendationEngineSettings\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"recommendation_engine_settings\": " + propertyValue);
                }
            }
            if(SharedSecret != null)
            {
                ret.Add("\"sharedSecret\": " + "\"" + SharedSecret + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"shared_secret\": " + "\"" + SharedSecret + "\"");
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaRecommendationProfileListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(RecommendationProfiles != null && RecommendationProfiles.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", RecommendationProfiles.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaRegion
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(ExternalId != null)
            {
                ret.Add("\"externalId\": " + "\"" + ExternalId + "\"");
            }
            ret.Add("\"id\": " + Id);
            ret.Add("\"isDefault\": " + IsDefault.ToString().ToLower());
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(RegionalChannels != null && RegionalChannels.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", RegionalChannels.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"linearChannels\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaRegionalChannel
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"channelNumber\": " + ChannelNumber);
            ret.Add("\"linearChannelId\": " + LinearChannelId);
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaRegionFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(ExternalIdIn != null)
            {
                ret.Add("\"externalIdIn\": " + "\"" + ExternalIdIn + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaRegionListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Regions != null && Regions.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Regions.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaRegistrySettings
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Key != null)
            {
                ret.Add("\"key\": " + "\"" + Key + "\"");
            }
            if(Value != null)
            {
                ret.Add("\"value\": " + "\"" + Value + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaRegistrySettingsListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(RegistrySettings != null && RegistrySettings.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", RegistrySettings.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaRuleFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"by\": " + By.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSearchHistory
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Action != null)
            {
                ret.Add("\"action\": " + "\"" + Action + "\"");
            }
            ret.Add("\"createdAt\": " + CreatedAt);
            if(DeviceId != null)
            {
                ret.Add("\"deviceId\": " + "\"" + DeviceId + "\"");
            }
            if(Filter != null)
            {
                ret.Add("\"filter\": " + "\"" + Filter + "\"");
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + Id + "\"");
            }
            if(Language != null)
            {
                ret.Add("\"language\": " + "\"" + Language + "\"");
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(Service != null)
            {
                ret.Add("\"service\": " + "\"" + Service + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSearchHistoryFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaSearchHistoryListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaTimeShiftedTvPartnerSettings
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(CatchUpBufferLength.HasValue)
            {
                ret.Add("\"catchUpBufferLength\": " + CatchUpBufferLength);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"catch_up_buffer_length\": " + CatchUpBufferLength);
                }
            }
            if(CatchUpEnabled.HasValue)
            {
                ret.Add("\"catchUpEnabled\": " + CatchUpEnabled.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"catch_up_enabled\": " + CatchUpEnabled.ToString().ToLower());
                }
            }
            if(CdvrEnabled.HasValue)
            {
                ret.Add("\"cdvrEnabled\": " + CdvrEnabled.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
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
                ret.Add("\"protectionPolicy\": " + ProtectionPolicy.GetHashCode());
            }
            if(ProtectionQuotaPercentage.HasValue)
            {
                ret.Add("\"protectionQuotaPercentage\": " + ProtectionQuotaPercentage);
            }
            if(QuotaOveragePolicy.HasValue)
            {
                ret.Add("\"quotaOveragePolicy\": " + QuotaOveragePolicy.GetHashCode());
            }
            if(RecordingLifetimePeriod.HasValue)
            {
                ret.Add("\"recordingLifetimePeriod\": " + RecordingLifetimePeriod);
            }
            if(RecordingScheduleWindow.HasValue)
            {
                ret.Add("\"recordingScheduleWindow\": " + RecordingScheduleWindow);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"recording_schedule_window\": " + RecordingScheduleWindow);
                }
            }
            if(RecordingScheduleWindowEnabled.HasValue)
            {
                ret.Add("\"recordingScheduleWindowEnabled\": " + RecordingScheduleWindowEnabled.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
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
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"start_over_enabled\": " + StartOverEnabled.ToString().ToLower());
                }
            }
            if(TrickPlayBufferLength.HasValue)
            {
                ret.Add("\"trickPlayBufferLength\": " + TrickPlayBufferLength);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"trick_play_buffer_length\": " + TrickPlayBufferLength);
                }
            }
            if(TrickPlayEnabled.HasValue)
            {
                ret.Add("\"trickPlayEnabled\": " + TrickPlayEnabled.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"trick_play_enabled\": " + TrickPlayEnabled.ToString().ToLower());
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaUserAssetRule
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Description != null)
            {
                ret.Add("\"description\": " + "\"" + Description + "\"");
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            ret.Add("\"ruleType\": " + RuleType.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaUserAssetRuleFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(AssetIdEqual.HasValue)
            {
                ret.Add("\"assetIdEqual\": " + AssetIdEqual);
            }
            if(AssetTypeEqual.HasValue)
            {
                ret.Add("\"assetTypeEqual\": " + AssetTypeEqual);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaUserAssetRuleListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Rules != null && Rules.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Rules.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaUserRole
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(ExcludedPermissionNames != null)
            {
                ret.Add("\"excludedPermissionNames\": " + "\"" + ExcludedPermissionNames + "\"");
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(PermissionNames != null)
            {
                ret.Add("\"permissionNames\": " + "\"" + PermissionNames + "\"");
            }
            if(!DeprecatedAttribute.IsDeprecated("4.6.0.0", currentVersion) && Permissions != null && Permissions.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Permissions.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"permissions\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaUserRoleFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(CurrentUserRoleIdsContains.HasValue)
            {
                ret.Add("\"currentUserRoleIdsContains\": " + CurrentUserRoleIdsContains.ToString().ToLower());
            }
            if(IdIn != null)
            {
                ret.Add("\"idIn\": " + "\"" + IdIn + "\"");
            }
            if(!omitObsolete && Ids != null && Ids.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Ids.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"ids\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaUserRoleListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(UserRoles != null && UserRoles.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", UserRoles.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
}

namespace WebAPI.Models.DMS
{
    public partial class KalturaConfigurationGroup
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(ConfigurationIdentifiers != null && ConfigurationIdentifiers.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", ConfigurationIdentifiers.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"configurationIdentifiers\": " + propertyValue);
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + Id + "\"");
            }
            ret.Add("\"isDefault\": " + IsDefault.ToString().ToLower());
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            ret.Add("\"numberOfDevices\": " + NumberOfDevices);
            ret.Add("\"partnerId\": " + PartnerId);
            if(Tags != null && Tags.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Tags.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"tags\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaConfigurationGroupDevice
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(ConfigurationGroupId != null)
            {
                ret.Add("\"configurationGroupId\": " + "\"" + ConfigurationGroupId + "\"");
            }
            ret.Add("\"partnerId\": " + PartnerId);
            if(Udid != null)
            {
                ret.Add("\"udid\": " + "\"" + Udid + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaConfigurationGroupDeviceFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(ConfigurationGroupIdEqual != null)
            {
                ret.Add("\"configurationGroupIdEqual\": " + "\"" + ConfigurationGroupIdEqual + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaConfigurationGroupDeviceListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaConfigurationGroupListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaConfigurationGroupTag
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(ConfigurationGroupId != null)
            {
                ret.Add("\"configurationGroupId\": " + "\"" + ConfigurationGroupId + "\"");
            }
            ret.Add("\"partnerId\": " + PartnerId);
            if(Tag != null)
            {
                ret.Add("\"tag\": " + "\"" + Tag + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaConfigurationGroupTagFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(ConfigurationGroupIdEqual != null)
            {
                ret.Add("\"configurationGroupIdEqual\": " + "\"" + ConfigurationGroupIdEqual + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaConfigurationGroupTagListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaConfigurationIdentifier
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + Id + "\"");
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaConfigurations
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(AppName != null)
            {
                ret.Add("\"appName\": " + "\"" + AppName + "\"");
            }
            if(ClientVersion != null)
            {
                ret.Add("\"clientVersion\": " + "\"" + ClientVersion + "\"");
            }
            if(ConfigurationGroupId != null)
            {
                ret.Add("\"configurationGroupId\": " + "\"" + ConfigurationGroupId + "\"");
            }
            if(Content != null)
            {
                ret.Add("\"content\": " + "\"" + Content + "\"");
            }
            if(ExternalPushId != null)
            {
                ret.Add("\"externalPushId\": " + "\"" + ExternalPushId + "\"");
            }
            if(Id != null)
            {
                ret.Add("\"id\": " + "\"" + Id + "\"");
            }
            ret.Add("\"isForceUpdate\": " + IsForceUpdate.ToString().ToLower());
            ret.Add("\"partnerId\": " + PartnerId);
            ret.Add("\"platform\": " + Platform.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaConfigurationsFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(ConfigurationGroupIdEqual != null)
            {
                ret.Add("\"configurationGroupIdEqual\": " + "\"" + ConfigurationGroupIdEqual + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaConfigurationsListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaDeviceReport
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(ConfigurationGroupId != null)
            {
                ret.Add("\"configurationGroupId\": " + "\"" + ConfigurationGroupId + "\"");
            }
            ret.Add("\"lastAccessDate\": " + LastAccessDate);
            if(LastAccessIP != null)
            {
                ret.Add("\"lastAccessIP\": " + "\"" + LastAccessIP + "\"");
            }
            if(OperationSystem != null)
            {
                ret.Add("\"operationSystem\": " + "\"" + OperationSystem + "\"");
            }
            ret.Add("\"partnerId\": " + PartnerId);
            if(PushParameters != null)
            {
                propertyValue = PushParameters.ToJson(currentVersion, omitObsolete);
                ret.Add("\"pushParameters\": " + propertyValue);
            }
            if(Udid != null)
            {
                ret.Add("\"udid\": " + "\"" + Udid + "\"");
            }
            if(UserAgent != null)
            {
                ret.Add("\"userAgent\": " + "\"" + UserAgent + "\"");
            }
            if(VersionAppName != null)
            {
                ret.Add("\"versionAppName\": " + "\"" + VersionAppName + "\"");
            }
            if(VersionNumber != null)
            {
                ret.Add("\"versionNumber\": " + "\"" + VersionNumber + "\"");
            }
            ret.Add("\"versionPlatform\": " + VersionPlatform.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaDeviceReportFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            ret.Add("\"lastAccessDateGreaterThanOrEqual\": " + LastAccessDateGreaterThanOrEqual);
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPushParams
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(ExternalToken != null)
            {
                ret.Add("\"externalToken\": " + "\"" + ExternalToken + "\"");
            }
            if(Token != null)
            {
                ret.Add("\"token\": " + "\"" + Token + "\"");
            }
            return String.Join(", ", ret);
        }
    }
}

namespace WebAPI.Models.Domains
{
    public partial class KalturaDevice
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(ActivatedOn.HasValue)
            {
                ret.Add("\"activatedOn\": " + ActivatedOn);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"activated_on\": " + ActivatedOn);
                }
            }
            if(!omitObsolete && Brand != null)
            {
                ret.Add("\"brand\": " + "\"" + Brand + "\"");
            }
            if(BrandId.HasValue)
            {
                ret.Add("\"brandId\": " + BrandId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
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
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(!omitObsolete && State.HasValue)
            {
                ret.Add("\"state\": " + State.GetHashCode());
            }
            if(Status.HasValue)
            {
                ret.Add("\"status\": " + Status.GetHashCode());
            }
            if(Udid != null)
            {
                ret.Add("\"udid\": " + "\"" + Udid + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaDeviceBrand
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
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
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaDeviceFamily
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(!omitObsolete && ConcurrentLimit.HasValue)
            {
                ret.Add("\"concurrentLimit\": " + ConcurrentLimit);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"concurrent_limit\": " + ConcurrentLimit);
                }
            }
            if(!omitObsolete && DeviceLimit.HasValue)
            {
                ret.Add("\"deviceLimit\": " + DeviceLimit);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
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
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(!omitObsolete && Devices != null && Devices.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Devices.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"devices\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaDeviceFamilyBase
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(!omitObsolete && ConcurrentLimit.HasValue)
            {
                ret.Add("\"concurrentLimit\": " + ConcurrentLimit);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"concurrent_limit\": " + ConcurrentLimit);
                }
            }
            if(!omitObsolete && DeviceLimit.HasValue)
            {
                ret.Add("\"deviceLimit\": " + DeviceLimit);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
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
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaDevicePin
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Pin != null)
            {
                ret.Add("\"pin\": " + "\"" + Pin + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaDeviceRegistrationStatusHolder
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"status\": " + Status.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaHomeNetwork
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Description != null)
            {
                ret.Add("\"description\": " + "\"" + Description + "\"");
            }
            if(ExternalId != null)
            {
                ret.Add("\"externalId\": " + "\"" + ExternalId + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"external_id\": " + "\"" + ExternalId + "\"");
                }
            }
            if(IsActive.HasValue)
            {
                ret.Add("\"isActive\": " + IsActive.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_active\": " + IsActive.ToString().ToLower());
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaHomeNetworkListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaHousehold
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(ConcurrentLimit.HasValue)
            {
                ret.Add("\"concurrentLimit\": " + ConcurrentLimit);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"concurrent_limit\": " + ConcurrentLimit);
                }
            }
            if(!omitObsolete && DefaultUsers != null && DefaultUsers.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", DefaultUsers.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"defaultUsers\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"default_users\": " + propertyValue);
                }
            }
            if(Description != null)
            {
                ret.Add("\"description\": " + "\"" + Description + "\"");
            }
            if(!omitObsolete && DeviceFamilies != null && DeviceFamilies.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", DeviceFamilies.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"deviceFamilies\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"device_families\": " + propertyValue);
                }
            }
            if(DevicesLimit.HasValue)
            {
                ret.Add("\"devicesLimit\": " + DevicesLimit);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"devices_limit\": " + DevicesLimit);
                }
            }
            if(ExternalId != null)
            {
                ret.Add("\"externalId\": " + "\"" + ExternalId + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"external_id\": " + "\"" + ExternalId + "\"");
                }
            }
            if(FrequencyNextDeviceAction.HasValue)
            {
                ret.Add("\"frequencyNextDeviceAction\": " + FrequencyNextDeviceAction);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"frequency_next_device_action\": " + FrequencyNextDeviceAction);
                }
            }
            if(FrequencyNextUserAction.HasValue)
            {
                ret.Add("\"frequencyNextUserAction\": " + FrequencyNextUserAction);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"frequency_next_user_action\": " + FrequencyNextUserAction);
                }
            }
            if(HouseholdLimitationsId.HasValue)
            {
                ret.Add("\"householdLimitationsId\": " + HouseholdLimitationsId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
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
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_frequency_enabled\": " + IsFrequencyEnabled.ToString().ToLower());
                }
            }
            if(!omitObsolete && MasterUsers != null && MasterUsers.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", MasterUsers.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"masterUsers\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"master_users\": " + propertyValue);
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(!omitObsolete && PendingUsers != null && PendingUsers.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", PendingUsers.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"pendingUsers\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"pending_users\": " + propertyValue);
                }
            }
            if(RegionId.HasValue)
            {
                ret.Add("\"regionId\": " + RegionId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"region_id\": " + RegionId);
                }
            }
            if(Restriction.HasValue)
            {
                ret.Add("\"restriction\": " + Restriction.GetHashCode());
            }
            if(RoleId.HasValue)
            {
                ret.Add("\"roleId\": " + RoleId);
            }
            if(State.HasValue)
            {
                ret.Add("\"state\": " + State.GetHashCode());
            }
            if(!omitObsolete && Users != null && Users.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Users.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"users\": " + propertyValue);
            }
            if(UsersLimit.HasValue)
            {
                ret.Add("\"usersLimit\": " + UsersLimit);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"users_limit\": " + UsersLimit);
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaHouseholdDevice
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(ActivatedOn.HasValue)
            {
                ret.Add("\"activatedOn\": " + ActivatedOn);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"activated_on\": " + ActivatedOn);
                }
            }
            if(!omitObsolete && Brand != null)
            {
                ret.Add("\"brand\": " + "\"" + Brand + "\"");
            }
            if(BrandId.HasValue)
            {
                ret.Add("\"brandId\": " + BrandId);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
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
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(!omitObsolete && State.HasValue)
            {
                ret.Add("\"state\": " + State.GetHashCode());
            }
            if(Status.HasValue)
            {
                ret.Add("\"status\": " + Status.GetHashCode());
            }
            if(Udid != null)
            {
                ret.Add("\"udid\": " + "\"" + Udid + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaHouseholdDeviceFamilyLimitations
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(!omitObsolete && ConcurrentLimit.HasValue)
            {
                ret.Add("\"concurrentLimit\": " + ConcurrentLimit);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"concurrent_limit\": " + ConcurrentLimit);
                }
            }
            if(!omitObsolete && DeviceLimit.HasValue)
            {
                ret.Add("\"deviceLimit\": " + DeviceLimit);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
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
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(ConcurrentLimit.HasValue)
            {
                ret.Add("\"concurrentLimit\": " + ConcurrentLimit);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"concurrent_limit\": " + ConcurrentLimit);
                }
            }
            if(DeviceLimit.HasValue)
            {
                ret.Add("\"deviceLimit\": " + DeviceLimit);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"device_limit\": " + DeviceLimit);
                }
            }
            if(Frequency.HasValue)
            {
                ret.Add("\"frequency\": " + Frequency);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaHouseholdDeviceFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(DeviceFamilyIdIn != null)
            {
                ret.Add("\"deviceFamilyIdIn\": " + "\"" + DeviceFamilyIdIn + "\"");
            }
            if(HouseholdIdEqual.HasValue)
            {
                ret.Add("\"householdIdEqual\": " + HouseholdIdEqual);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaHouseholdDeviceListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaHouseholdLimitations
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(ConcurrentLimit.HasValue)
            {
                ret.Add("\"concurrentLimit\": " + ConcurrentLimit);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"concurrent_limit\": " + ConcurrentLimit);
                }
            }
            if(DeviceFamiliesLimitations != null && DeviceFamiliesLimitations.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", DeviceFamiliesLimitations.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"deviceFamiliesLimitations\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"device_families_limitations\": " + propertyValue);
                }
            }
            if(DeviceFrequency.HasValue)
            {
                ret.Add("\"deviceFrequency\": " + DeviceFrequency);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"device_frequency\": " + DeviceFrequency);
                }
            }
            if(DeviceFrequencyDescription != null)
            {
                ret.Add("\"deviceFrequencyDescription\": " + "\"" + DeviceFrequencyDescription + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"device_frequency_description\": " + "\"" + DeviceFrequencyDescription + "\"");
                }
            }
            if(DeviceLimit.HasValue)
            {
                ret.Add("\"deviceLimit\": " + DeviceLimit);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
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
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(NpvrQuotaInSeconds.HasValue)
            {
                ret.Add("\"npvrQuotaInSeconds\": " + NpvrQuotaInSeconds);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"npvr_quota_in_seconds\": " + NpvrQuotaInSeconds);
                }
            }
            if(UserFrequency.HasValue)
            {
                ret.Add("\"userFrequency\": " + UserFrequency);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"user_frequency\": " + UserFrequency);
                }
            }
            if(UserFrequencyDescription != null)
            {
                ret.Add("\"userFrequencyDescription\": " + "\"" + UserFrequencyDescription + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"user_frequency_description\": " + "\"" + UserFrequencyDescription + "\"");
                }
            }
            if(UsersLimit.HasValue)
            {
                ret.Add("\"usersLimit\": " + UsersLimit);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"users_limit\": " + UsersLimit);
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaHouseholdUser
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(HouseholdId.HasValue)
            {
                ret.Add("\"householdId\": " + HouseholdId);
            }
            if(HouseholdMasterUsername != null)
            {
                ret.Add("\"householdMasterUsername\": " + "\"" + HouseholdMasterUsername + "\"");
            }
            if(IsDefault.HasValue)
            {
                ret.Add("\"isDefault\": " + IsDefault.ToString().ToLower());
            }
            if(IsMaster.HasValue)
            {
                ret.Add("\"isMaster\": " + IsMaster.ToString().ToLower());
            }
            ret.Add("\"status\": " + Status.GetHashCode());
            if(UserId != null)
            {
                ret.Add("\"userId\": " + "\"" + UserId + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaHouseholdUserFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(HouseholdIdEqual.HasValue)
            {
                ret.Add("\"householdIdEqual\": " + HouseholdIdEqual);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaHouseholdUserListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaHouseholdWithHolder
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"type\": " + type.GetHashCode());
            return String.Join(", ", ret);
        }
    }
}

namespace WebAPI.Models.Billing
{
    public partial class KalturaHouseholdPaymentGateway
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
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
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            ret.Add("\"selectedBy\": " + selectedBy.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaHouseholdPaymentGatewayListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaHouseholdPaymentMethod
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(!omitObsolete && AllowMultiInstance.HasValue)
            {
                ret.Add("\"allowMultiInstance\": " + AllowMultiInstance.ToString().ToLower());
            }
            if(Details != null)
            {
                ret.Add("\"details\": " + "\"" + Details + "\"");
            }
            if(ExternalId != null)
            {
                ret.Add("\"externalId\": " + "\"" + ExternalId + "\"");
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
                ret.Add("\"name\": " + "\"" + Name + "\"");
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
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaHouseholdPaymentMethodListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(Objects != null && Objects.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Objects.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPaymentGateway
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(paymentGateway != null)
            {
                propertyValue = paymentGateway.ToJson(currentVersion, omitObsolete);
                ret.Add("\"payment_gateway\": " + propertyValue);
            }
            ret.Add("\"selected_by\": " + selectedBy.GetHashCode());
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPaymentGatewayBaseProfile
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(IsDefault.HasValue)
            {
                ret.Add("\"isDefault\": " + IsDefault.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_default\": " + IsDefault.ToString().ToLower());
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(!omitObsolete && PaymentMethods != null && PaymentMethods.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", PaymentMethods.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"paymentMethods\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"payment_methods\": " + propertyValue);
                }
            }
            if(selectedBy.HasValue)
            {
                ret.Add("\"selectedBy\": " + selectedBy.GetHashCode());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"selected_by\": " + selectedBy.GetHashCode());
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPaymentGatewayConfiguration
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Configuration != null && Configuration.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", Configuration.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"paymentGatewayConfiguration\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"payment_gatewaye_configuration\": " + propertyValue);
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPaymentGatewayProfile
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(Id.HasValue)
            {
                ret.Add("\"id\": " + Id);
            }
            if(IsDefault.HasValue)
            {
                ret.Add("\"isDefault\": " + IsDefault.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_default\": " + IsDefault.ToString().ToLower());
                }
            }
            if(Name != null)
            {
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(!omitObsolete && PaymentMethods != null && PaymentMethods.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", PaymentMethods.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"paymentMethods\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"payment_methods\": " + propertyValue);
                }
            }
            if(selectedBy.HasValue)
            {
                ret.Add("\"selectedBy\": " + selectedBy.GetHashCode());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"selected_by\": " + selectedBy.GetHashCode());
                }
            }
            if(AdapterUrl != null)
            {
                ret.Add("\"adapterUrl\": " + "\"" + AdapterUrl + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"adapter_url\": " + "\"" + AdapterUrl + "\"");
                }
            }
            if(ExternalIdentifier != null)
            {
                ret.Add("\"externalIdentifier\": " + "\"" + ExternalIdentifier + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"external_identifier\": " + "\"" + ExternalIdentifier + "\"");
                }
            }
            ret.Add("\"externalVerification\": " + ExternalVerification.ToString().ToLower());
            if(IsActive.HasValue)
            {
                ret.Add("\"isActive\": " + IsActive);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"is_active\": " + IsActive);
                }
            }
            if(PendingInterval.HasValue)
            {
                ret.Add("\"pendingInterval\": " + PendingInterval);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"pending_interval\": " + PendingInterval);
                }
            }
            if(PendingRetries.HasValue)
            {
                ret.Add("\"pendingRetries\": " + PendingRetries);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"pending_retries\": " + PendingRetries);
                }
            }
            if(RenewIntervalMinutes.HasValue)
            {
                ret.Add("\"renewIntervalMinutes\": " + RenewIntervalMinutes);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"renew_interval_minutes\": " + RenewIntervalMinutes);
                }
            }
            if(RenewStartMinutes.HasValue)
            {
                ret.Add("\"renewStartMinutes\": " + RenewStartMinutes);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"renew_start_minutes\": " + RenewStartMinutes);
                }
            }
            if(RenewUrl != null)
            {
                ret.Add("\"renewUrl\": " + "\"" + RenewUrl + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"renew_url\": " + "\"" + RenewUrl + "\"");
                }
            }
            if(Settings != null && Settings.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", Settings.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"paymentGatewaySettings\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"payment_gateway_settings\": " + propertyValue);
                }
            }
            if(SharedSecret != null)
            {
                ret.Add("\"sharedSecret\": " + "\"" + SharedSecret + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"shared_secret\": " + "\"" + SharedSecret + "\"");
                }
            }
            if(StatusUrl != null)
            {
                ret.Add("\"statusUrl\": " + "\"" + StatusUrl + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"status_url\": " + "\"" + StatusUrl + "\"");
                }
            }
            if(TransactUrl != null)
            {
                ret.Add("\"transactUrl\": " + "\"" + TransactUrl + "\"");
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"transact_url\": " + "\"" + TransactUrl + "\"");
                }
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPaymentGatewayProfileListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(PaymentGatewayProfiles != null && PaymentGatewayProfiles.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", PaymentGatewayProfiles.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPaymentMethod
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(AllowMultiInstance.HasValue)
            {
                ret.Add("\"allowMultiInstance\": " + AllowMultiInstance.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
                {
                    ret.Add("\"allow_multi_instance\": " + AllowMultiInstance.ToString().ToLower());
                }
            }
            if(HouseholdPaymentMethods != null && HouseholdPaymentMethods.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", HouseholdPaymentMethods.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"householdPaymentMethods\": " + propertyValue);
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
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
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPaymentMethodProfile
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(AllowMultiInstance.HasValue)
            {
                ret.Add("\"allowMultiInstance\": " + AllowMultiInstance.ToString().ToLower());
                if (currentVersion == null || currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) < 0)
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
                ret.Add("\"name\": " + "\"" + Name + "\"");
            }
            if(PaymentGatewayId.HasValue)
            {
                ret.Add("\"paymentGatewayId\": " + PaymentGatewayId);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPaymentMethodProfileFilter
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"orderBy\": " + OrderBy.GetHashCode());
            if(PaymentGatewayIdEqual.HasValue)
            {
                ret.Add("\"paymentGatewayIdEqual\": " + PaymentGatewayIdEqual);
            }
            return String.Join(", ", ret);
        }
    }
    public partial class KalturaPaymentMethodProfileListResponse
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            ret.Add("\"totalCount\": " + TotalCount);
            if(PaymentMethodProfiles != null && PaymentMethodProfiles.Count > 0)
            {
                propertyValue = "[" + String.Join(", ", PaymentMethodProfiles.Select(item => item.ToJson(currentVersion, omitObsolete))) + "]";
                ret.Add("\"objects\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
}

namespace WebAPI.EventNotifications
{
    public partial class KalturaHttpNotification
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            if(objectType != null)
            {
                ret.Add("\"objectType\": " + "\"" + objectType + "\"");
            }
            if(relatedObjects != null && relatedObjects.Count > 0)
            {
                propertyValue = "{" + String.Join(", ", relatedObjects.Select(pair => "\"" + pair.Key + "\": " + pair.Value.ToJson(currentVersion, omitObsolete))) + "}";
                ret.Add("\"relatedObjects\": " + propertyValue);
            }
            if(eventObject != null)
            {
                propertyValue = eventObject.ToJson(currentVersion, omitObsolete);
                ret.Add("\"object\": " + propertyValue);
            }
            if(eventObjectType != null)
            {
                ret.Add("\"eventObjectType\": " + "\"" + eventObjectType + "\"");
            }
            if(eventType.HasValue)
            {
                ret.Add("\"eventType\": " + eventType.GetHashCode());
            }
            if(systemName != null)
            {
                ret.Add("\"systemName\": " + "\"" + systemName + "\"");
            }
            return String.Join(", ", ret);
        }
    }
}

namespace WebAPI.Managers.Models
{
    public partial class StatusWrapper
    {
        protected override string PropertiesToJson(Version currentVersion, bool omitObsolete)
        {
            List<string> ret = new List<string>();
            string propertyValue;
            ret.Add("\"executionTime\": " + ExecutionTime);
            if(Result != null)
            {
                propertyValue = (Result is IKalturaJsonable ? (Result as IKalturaJsonable).ToJson(currentVersion, omitObsolete) : JsonManager.GetInstance().Serialize(Result));
                ret.Add("\"result\": " + propertyValue);
            }
            return String.Join(", ", ret);
        }
    }
}
