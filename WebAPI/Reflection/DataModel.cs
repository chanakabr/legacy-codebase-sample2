// NOTICE: This is a generated file, to modify it, edit Program.cs in Reflector project
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Web;
using WebAPI.Managers;
using WebAPI.Managers.Scheme;

namespace WebAPI.Reflection
{
    public class DataModel
    {
        public static bool IsDeprecated(Type type, string propertyName)
        {
            switch (type.Name)
            {
                case "KalturaSubscription":
                    switch (propertyName)
                    {
                        case "Descriptions":
                            return DeprecatedAttribute.IsDeprecated("3.6.287.27312");
                        case "Names":
                            return DeprecatedAttribute.IsDeprecated("3.6.287.27312");
                    };
                    break;
                    
            }
            
            return false;
        }
        
        public static bool IsObsolete(Type type, string propertyName)
        {
            switch (type.Name)
            {
                case "KalturaAsset":
                    switch (propertyName)
                    {
                        case "Statistics":
                            return true;
                    };
                    break;
                    
                case "KalturaAssetHistoryFilter":
                    switch (propertyName)
                    {
                        case "filterTypes":
                        case "with":
                            return true;
                    };
                    break;
                    
                case "KalturaAssetInfo":
                    switch (propertyName)
                    {
                        case "Statistics":
                            return true;
                    };
                    break;
                    
                case "KalturaBaseAssetInfo":
                    switch (propertyName)
                    {
                        case "Statistics":
                            return true;
                    };
                    break;
                    
                case "KalturaBillingPartnerConfig":
                    switch (propertyName)
                    {
                        case "PartnerConfigurationType":
                            return true;
                    };
                    break;
                    
                case "KalturaBookmark":
                    switch (propertyName)
                    {
                        case "User":
                            return true;
                    };
                    break;
                    
                case "KalturaBookmarkFilter":
                    switch (propertyName)
                    {
                        case "AssetIn":
                            return true;
                    };
                    break;
                    
                case "KalturaChannel":
                    switch (propertyName)
                    {
                        case "MediaTypes":
                            return true;
                    };
                    break;
                    
                case "KalturaCollectionEntitlement":
                    switch (propertyName)
                    {
                        case "IsInGracePeriod":
                        case "IsRenewable":
                        case "IsRenewableForPurchase":
                        case "MediaFileId":
                        case "MediaId":
                        case "NextRenewalDate":
                        case "PurchaseId":
                        case "Type":
                            return true;
                    };
                    break;
                    
                case "KalturaDevice":
                    switch (propertyName)
                    {
                        case "Brand":
                        case "State":
                            return true;
                    };
                    break;
                    
                case "KalturaDeviceFamily":
                    switch (propertyName)
                    {
                        case "ConcurrentLimit":
                        case "DeviceLimit":
                        case "Devices":
                            return true;
                    };
                    break;
                    
                case "KalturaDeviceFamilyBase":
                    switch (propertyName)
                    {
                        case "ConcurrentLimit":
                        case "DeviceLimit":
                            return true;
                    };
                    break;
                    
                case "KalturaEntitlement":
                    switch (propertyName)
                    {
                        case "IsInGracePeriod":
                        case "IsRenewable":
                        case "IsRenewableForPurchase":
                        case "MediaFileId":
                        case "MediaId":
                        case "NextRenewalDate":
                        case "PurchaseId":
                        case "Type":
                            return true;
                    };
                    break;
                    
                case "KalturaEntitlementCancellation":
                    switch (propertyName)
                    {
                        case "Type":
                            return true;
                    };
                    break;
                    
                case "KalturaFavorite":
                    switch (propertyName)
                    {
                        case "Asset":
                            return true;
                    };
                    break;
                    
                case "KalturaFavoriteFilter":
                    switch (propertyName)
                    {
                        case "MediaIds":
                        case "MediaTypeIn":
                        case "UDID":
                            return true;
                    };
                    break;
                    
                case "KalturaHousehold":
                    switch (propertyName)
                    {
                        case "DefaultUsers":
                        case "DeviceFamilies":
                        case "MasterUsers":
                        case "PendingUsers":
                        case "Users":
                            return true;
                    };
                    break;
                    
                case "KalturaHouseholdDevice":
                    switch (propertyName)
                    {
                        case "Brand":
                        case "State":
                            return true;
                    };
                    break;
                    
                case "KalturaHouseholdPaymentMethod":
                    switch (propertyName)
                    {
                        case "AllowMultiInstance":
                        case "Name":
                        case "Selected":
                            return true;
                    };
                    break;
                    
                case "KalturaMediaAsset":
                    switch (propertyName)
                    {
                        case "Statistics":
                            return true;
                    };
                    break;
                    
                case "KalturaOTTUser":
                    switch (propertyName)
                    {
                        case "Country":
                        case "FacebookId":
                        case "FacebookImage":
                        case "FacebookToken":
                        case "SuspentionState":
                            return true;
                    };
                    break;
                    
                case "KalturaPaymentGatewayBaseProfile":
                    switch (propertyName)
                    {
                        case "PaymentMethods":
                            return true;
                    };
                    break;
                    
                case "KalturaPaymentGatewayProfile":
                    switch (propertyName)
                    {
                        case "PaymentMethods":
                            return true;
                    };
                    break;
                    
                case "KalturaPpvEntitlement":
                    switch (propertyName)
                    {
                        case "IsInGracePeriod":
                        case "IsRenewable":
                        case "IsRenewableForPurchase":
                        case "NextRenewalDate":
                        case "PurchaseId":
                        case "Type":
                            return true;
                    };
                    break;
                    
                case "KalturaProgramAsset":
                    switch (propertyName)
                    {
                        case "Statistics":
                            return true;
                    };
                    break;
                    
                case "KalturaRecordingAsset":
                    switch (propertyName)
                    {
                        case "Statistics":
                            return true;
                    };
                    break;
                    
                case "KalturaSubscriptionEntitlement":
                    switch (propertyName)
                    {
                        case "MediaFileId":
                        case "MediaId":
                        case "PurchaseId":
                        case "Type":
                            return true;
                    };
                    break;
                    
                case "KalturaSubscriptionPrice":
                    switch (propertyName)
                    {
                        case "Price":
                        case "PurchaseStatus":
                            return true;
                    };
                    break;
                    
                case "KalturaUserRoleFilter":
                    switch (propertyName)
                    {
                        case "Ids":
                            return true;
                    };
                    break;
                    
            }
            
            return IsDeprecated(type, propertyName);
        }
        
        public static Dictionary<string, string> getOldMembers(MethodInfo action, Version currentVersion)
        {
            Dictionary<string, string> ret = null;
            switch (action.DeclaringType.Name)
            {
                case "CdnAdapterProfileController":
                    switch(action.Name)
                    {
                        case "Delete":
                            ret = new Dictionary<string, string>() { 
                                 {"adapterId", "adapter_id"},
                            };
                            break;
                    }
                    break;
                    
                case "CDVRAdapterProfileController":
                    switch(action.Name)
                    {
                        case "Delete":
                            ret = new Dictionary<string, string>() { 
                                 {"adapterId", "adapter_id"},
                            };
                            break;
                        case "GenerateSharedSecret":
                            ret = new Dictionary<string, string>() { 
                                 {"adapterId", "adapter_id"},
                            };
                            break;
                    }
                    break;
                    
                case "ChannelController":
                    switch(action.Name)
                    {
                        case "Delete":
                            ret = new Dictionary<string, string>() { 
                                 {"channelId", "channel_id"},
                            };
                            break;
                    }
                    break;
                    
                case "EntitlementController":
                    switch(action.Name)
                    {
                        case "Buy":
                            ret = new Dictionary<string, string>() { 
                                 {"couponCode", "coupon_code"},
                                 {"encryptedCvv", "encrypted_cvv"},
                                 {"extraParams", "extra_params"},
                                 {"fileId", "file_id"},
                                 {"isSubscription", "is_subscription"},
                                 {"itemId", "item_id"},
                            };
                            break;
                        case "Cancel":
                            ret = new Dictionary<string, string>() { 
                                 {"assetId", "asset_id"},
                                 {"transactionType", "transaction_type"},
                            };
                            break;
                        case "CancelRenewal":
                            ret = new Dictionary<string, string>() { 
                                 {"subscriptionId", "subscription_id"},
                            };
                            break;
                        case "ForceCancel":
                            ret = new Dictionary<string, string>() { 
                                 {"assetId", "asset_id"},
                                 {"transactionType", "transaction_type"},
                            };
                            break;
                        case "Grant":
                            ret = new Dictionary<string, string>() { 
                                 {"contentId", "content_id"},
                                 {"productId", "product_id"},
                                 {"productType", "product_type"},
                            };
                            break;
                    }
                    break;
                    
                case "ExternalChannelProfileController":
                    switch(action.Name)
                    {
                        case "Add":
                            ret = new Dictionary<string, string>() { 
                                 {"externalChannel", "external_channel"},
                            };
                            break;
                        case "Delete":
                            ret = new Dictionary<string, string>() { 
                                 {"externalChannelId", "external_channel_id"},
                            };
                            break;
                    }
                    break;
                    
                case "FollowTvSeriesController":
                    switch(action.Name)
                    {
                        case "Delete":
                            ret = new Dictionary<string, string>() { 
                                 {"assetId", "asset_id"},
                            };
                            break;
                    }
                    break;
                    
                case "HomeNetworkController":
                    switch(action.Name)
                    {
                        case "Add":
                            ret = new Dictionary<string, string>() { 
                                 {"homeNetwork", "home_network"},
                            };
                            break;
                        case "Delete":
                            ret = new Dictionary<string, string>() { 
                                 {"externalId", "external_id"},
                            };
                            break;
                    }
                    break;
                    
                case "HouseholdController":
                    switch(action.Name)
                    {
                        case "ResetFrequency":
                            ret = new Dictionary<string, string>() { 
                                 {"frequencyType", "household_frequency_type"},
                            };
                            break;
                    }
                    break;
                    
                case "HouseholdDeviceController":
                    switch(action.Name)
                    {
                        case "AddByPin":
                            ret = new Dictionary<string, string>() { 
                                 {"deviceName", "device_name"},
                            };
                            break;
                        case "GeneratePin":
                            ret = new Dictionary<string, string>() { 
                                 {"brandId", "brand_id"},
                            };
                            break;
                    }
                    break;
                    
                case "HouseholdPaymentGatewayController":
                    switch(action.Name)
                    {
                        case "Invoke":
                            ret = new Dictionary<string, string>() { 
                                 {"extraParameters", "extra_parameters"},
                            };
                            break;
                    }
                    break;
                    
                case "HouseholdUserController":
                    switch(action.Name)
                    {
                        case "Delete":
                            ret = new Dictionary<string, string>() { 
                                 {"id", "user_id_to_delete"},
                            };
                            break;
                    }
                    break;
                    
                case "LicensedUrlController":
                    switch(action.Name)
                    {
                        case "GetOldStandard":
                            ret = new Dictionary<string, string>() { 
                                 {"assetId", "asset_id"},
                                 {"assetType", "asset_type"},
                                 {"baseUrl", "base_url"},
                                 {"contentId", "content_id"},
                                 {"startDate", "start_date"},
                                 {"streamType", "stream_type"},
                            };
                            break;
                    }
                    break;
                    
                case "MessageTemplateController":
                    switch(action.Name)
                    {
                        case "Get":
                            ret = new Dictionary<string, string>() { 
                                 {"messageType", "asset_Type"},
                            };
                            if (currentVersion != null && currentVersion.CompareTo(new Version("3.6.2094.15157")) < 0 && currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) > 0)
                            {
                                if (ret.ContainsKey("messageType"))
                                {
                                    ret.Remove("messageType");
                                }
                                ret.Add("messageType", "assetType");
                            }
                            break;
                        case "Update":
                            ret = new Dictionary<string, string>() { 
                                 {"messageType", "asset_Type"},
                            };
                            if (currentVersion != null && currentVersion.CompareTo(new Version("3.6.2094.15157")) < 0 && currentVersion.CompareTo(new Version(OldStandardAttribute.Version)) > 0)
                            {
                                if (ret.ContainsKey("messageType"))
                                {
                                    ret.Remove("messageType");
                                }
                                ret.Add("messageType", "assetType");
                            }
                            break;
                    }
                    break;
                    
                case "OssAdapterProfileController":
                    switch(action.Name)
                    {
                        case "Add":
                            ret = new Dictionary<string, string>() { 
                                 {"ossAdapter", "oss_adapter"},
                            };
                            break;
                        case "Delete":
                            ret = new Dictionary<string, string>() { 
                                 {"ossAdapterId", "oss_adapter_id"},
                            };
                            break;
                        case "GenerateSharedSecret":
                            ret = new Dictionary<string, string>() { 
                                 {"ossAdapterId", "oss_adapter_id"},
                            };
                            break;
                    }
                    break;
                    
                case "OttUserController":
                    switch(action.Name)
                    {
                        case "Activate":
                            ret = new Dictionary<string, string>() { 
                                 {"activationToken", "activation_token"},
                            };
                            break;
                        case "AddRole":
                            ret = new Dictionary<string, string>() { 
                                 {"roleId", "role_id"},
                            };
                            break;
                        case "Login":
                            ret = new Dictionary<string, string>() { 
                                 {"extraParams", "extra_params"},
                            };
                            break;
                        case "RefreshSession":
                            ret = new Dictionary<string, string>() { 
                                 {"refreshToken", "refresh_token"},
                            };
                            break;
                        case "UpdateLoginData":
                            ret = new Dictionary<string, string>() { 
                                 {"newPassword", "new_password"},
                                 {"oldPassword", "old_password"},
                            };
                            break;
                    }
                    break;
                    
                case "ParentalRuleController":
                    switch(action.Name)
                    {
                        case "Disable":
                            ret = new Dictionary<string, string>() { 
                                 {"entityReference", "by"},
                                 {"ruleId", "rule_id"},
                            };
                            break;
                        case "DisableDefault":
                            ret = new Dictionary<string, string>() { 
                                 {"entityReference", "by"},
                            };
                            break;
                        case "Enable":
                            ret = new Dictionary<string, string>() { 
                                 {"entityReference", "by"},
                                 {"ruleId", "rule_id"},
                            };
                            break;
                    }
                    break;
                    
                case "PaymentGatewayController":
                    switch(action.Name)
                    {
                        case "Delete":
                            ret = new Dictionary<string, string>() { 
                                 {"paymentGatewayId", "payment_gateway_id"},
                            };
                            break;
                    }
                    break;
                    
                case "PaymentGatewayProfileController":
                    switch(action.Name)
                    {
                        case "AddOldStandard":
                            ret = new Dictionary<string, string>() { 
                                 {"paymentGateway", "payment_gateway"},
                            };
                            break;
                        case "Delete":
                            ret = new Dictionary<string, string>() { 
                                 {"paymentGatewayId", "payment_gateway_id"},
                            };
                            break;
                        case "GenerateSharedSecret":
                            ret = new Dictionary<string, string>() { 
                                 {"paymentGatewayId", "payment_gateway_id"},
                            };
                            break;
                        case "GetConfiguration":
                            ret = new Dictionary<string, string>() { 
                                 {"extraParameters", "extra_parameters"},
                            };
                            break;
                        case "UpdateOldStandard":
                            ret = new Dictionary<string, string>() { 
                                 {"paymentGateway", "payment_gateway"},
                                 {"paymentGatewayId", "payment_gateway_id"},
                            };
                            break;
                    }
                    break;
                    
                case "PaymentMethodProfileController":
                    switch(action.Name)
                    {
                        case "UpdateOldStandard":
                            ret = new Dictionary<string, string>() { 
                                 {"paymentGatewayId", "payment_gateway_id"},
                                 {"paymentMethod", "payment_method"},
                            };
                            break;
                    }
                    break;
                    
                case "RecommendationProfileController":
                    switch(action.Name)
                    {
                        case "Add":
                            ret = new Dictionary<string, string>() { 
                                 {"recommendationEngine", "recommendation_engine"},
                            };
                            break;
                        case "GenerateSharedSecret":
                            ret = new Dictionary<string, string>() { 
                                 {"recommendationEngineId", "recommendation_engine_id"},
                            };
                            break;
                    }
                    break;
                    
                case "SessionController":
                    switch(action.Name)
                    {
                        case "GetOldStandard":
                            ret = new Dictionary<string, string>() { 
                                 {"session", "ks_to_parse"},
                            };
                            break;
                    }
                    break;
                    
                case "TransactionController":
                    switch(action.Name)
                    {
                        case "SetWaiver":
                            ret = new Dictionary<string, string>() { 
                                 {"assetId", "asset_id"},
                                 {"transactionType", "transaction_type"},
                            };
                            break;
                    }
                    break;
                    
                case "UserLoginPinController":
                    switch(action.Name)
                    {
                        case "Delete":
                            ret = new Dictionary<string, string>() { 
                                 {"pinCode", "pin_code"},
                            };
                            break;
                        case "Update":
                            ret = new Dictionary<string, string>() { 
                                 {"pinCode", "pin_code"},
                            };
                            break;
                    }
                    break;
                    
            }
            
            return ret;
        }
        
        public static Dictionary<string, string> getOldMembers(Type type)
        {
            switch (type.Name)
            {
                case "KalturaAnnouncement":
                    return new Dictionary<string, string>() { 
                        {"startTime", "start_time"},
                    };
                    
                case "KalturaAssetBookmark":
                    return new Dictionary<string, string>() { 
                        {"finishedWatching", "finished_watching"},
                        {"positionOwner", "position_owner"},
                    };
                    
                case "KalturaAssetHistoryFilter":
                    return new Dictionary<string, string>() { 
                        {"daysLessThanOrEqual", "days"},
                        {"filterTypes", "filter_types"},
                        {"statusEqual", "filter_status"},
                    };
                    
                case "KalturaAssetInfo":
                    return new Dictionary<string, string>() { 
                        {"endDate", "end_date"},
                        {"extraParams", "extra_params"},
                        {"mediaFiles", "media_files"},
                        {"startDate", "start_date"},
                    };
                    
                case "KalturaAssetInfoFilter":
                    return new Dictionary<string, string>() { 
                        {"referenceType", "reference_type"},
                    };
                    
                case "KalturaAssetInfoListResponse":
                    return new Dictionary<string, string>() { 
                        {"requestId", "request_id"},
                    };
                    
                case "KalturaAssetsFilter":
                    return new Dictionary<string, string>() { 
                        {"assets", "Assets"},
                    };
                    
                case "KalturaAssetStatistics":
                    return new Dictionary<string, string>() { 
                        {"assetId", "asset_id"},
                        {"buzzScore", "buzz_score"},
                        {"ratingCount", "rating_count"},
                    };
                    
                case "KalturaBaseAssetInfo":
                    return new Dictionary<string, string>() { 
                        {"mediaFiles", "media_files"},
                    };
                    
                case "KalturaBaseOTTUser":
                    return new Dictionary<string, string>() { 
                        {"firstName", "first_name"},
                        {"lastName", "last_name"},
                    };
                    
                case "KalturaBillingPartnerConfig":
                    return new Dictionary<string, string>() { 
                        {"partnerConfigurationType", "partner_configuration_type"},
                    };
                    
                case "KalturaBillingResponse":
                    return new Dictionary<string, string>() { 
                        {"externalReceiptCode", "external_receipt_code"},
                        {"receiptCode", "receipt_code"},
                    };
                    
                case "KalturaBillingTransaction":
                    return new Dictionary<string, string>() { 
                        {"actionDate", "action_date"},
                        {"billingAction", "billing_action"},
                        {"billingProviderRef", "billing_provider_ref"},
                        {"endDate", "end_date"},
                        {"isRecurring", "is_recurring"},
                        {"itemType", "item_type"},
                        {"paymentMethod", "payment_method"},
                        {"paymentMethodExtraDetails", "payment_method_extra_details"},
                        {"purchasedItemCode", "purchased_item_code"},
                        {"purchasedItemName", "purchased_item_name"},
                        {"purchaseId", "purchase_id"},
                        {"recieptCode", "reciept_code"},
                        {"startDate", "start_date"},
                    };
                    
                case "KalturaBuzzScore":
                    return new Dictionary<string, string>() { 
                        {"avgScore", "avg_score"},
                        {"normalizedAvgScore", "normalized_avg_score"},
                        {"updateDate", "update_date"},
                    };
                    
                case "KalturaCDVRAdapterProfile":
                    return new Dictionary<string, string>() { 
                        {"adapterUrl", "adapter_url"},
                        {"dynamicLinksSupport", "dynamic_links_support"},
                        {"externalIdentifier", "external_identifier"},
                        {"isActive", "is_active"},
                        {"sharedSecret", "shared_secret"},
                    };
                    
                case "KalturaChannel":
                    return new Dictionary<string, string>() { 
                        {"assetTypes", "asset_types"},
                        {"filterExpression", "filter_expression"},
                    };
                    
                case "KalturaChannelProfile":
                    return new Dictionary<string, string>() { 
                        {"assetTypes", "asset_types"},
                        {"filterExpression", "filter_expression"},
                        {"isActive", "is_active"},
                    };
                    
                case "KalturaCollectionEntitlement":
                    return new Dictionary<string, string>() { 
                        {"currentDate", "current_date"},
                        {"currentUses", "current_uses"},
                        {"deviceName", "device_name"},
                        {"deviceUdid", "device_udid"},
                        {"endDate", "end_date"},
                        {"entitlementId", "entitlement_id"},
                        {"isCancelationWindowEnabled", "is_cancelation_window_enabled"},
                        {"isInGracePeriod", "is_in_grace_period"},
                        {"isRenewable", "is_renewable"},
                        {"isRenewableForPurchase", "is_renewable_for_purchase"},
                        {"lastViewDate", "last_view_date"},
                        {"maxUses", "max_uses"},
                        {"mediaFileId", "media_file_id"},
                        {"mediaId", "media_id"},
                        {"nextRenewalDate", "next_renewal_date"},
                        {"paymentMethod", "payment_method"},
                        {"purchaseDate", "purchase_date"},
                        {"purchaseId", "purchase_id"},
                    };
                    
                case "KalturaCoupon":
                    return new Dictionary<string, string>() { 
                        {"couponsGroup", "coupons_group"},
                    };
                    
                case "KalturaCouponsGroup":
                    return new Dictionary<string, string>() { 
                        {"endDate", "end_date"},
                        {"maxUsesNumber", "max_uses_number"},
                        {"maxUsesNumberOnRenewableSub", "max_uses_number_on_renewable_sub"},
                        {"startDate", "start_date"},
                    };
                    
                case "KalturaDevice":
                    return new Dictionary<string, string>() { 
                        {"activatedOn", "activated_on"},
                        {"brandId", "brand_id"},
                    };
                    
                case "KalturaDeviceFamily":
                    return new Dictionary<string, string>() { 
                        {"concurrentLimit", "concurrent_limit"},
                        {"deviceLimit", "device_limit"},
                    };
                    
                case "KalturaDeviceFamilyBase":
                    return new Dictionary<string, string>() { 
                        {"concurrentLimit", "concurrent_limit"},
                        {"deviceLimit", "device_limit"},
                    };
                    
                case "KalturaDiscountModule":
                    return new Dictionary<string, string>() { 
                        {"endDate", "end_date"},
                        {"startDate", "start_date"},
                    };
                    
                case "KalturaEntitlement":
                    return new Dictionary<string, string>() { 
                        {"currentDate", "current_date"},
                        {"currentUses", "current_uses"},
                        {"deviceName", "device_name"},
                        {"deviceUdid", "device_udid"},
                        {"endDate", "end_date"},
                        {"entitlementId", "entitlement_id"},
                        {"isCancelationWindowEnabled", "is_cancelation_window_enabled"},
                        {"isInGracePeriod", "is_in_grace_period"},
                        {"isRenewable", "is_renewable"},
                        {"isRenewableForPurchase", "is_renewable_for_purchase"},
                        {"lastViewDate", "last_view_date"},
                        {"maxUses", "max_uses"},
                        {"mediaFileId", "media_file_id"},
                        {"mediaId", "media_id"},
                        {"nextRenewalDate", "next_renewal_date"},
                        {"paymentMethod", "payment_method"},
                        {"purchaseDate", "purchase_date"},
                        {"purchaseId", "purchase_id"},
                    };
                    
                case "KalturaEntitlementFilter":
                    return new Dictionary<string, string>() { 
                        {"entitlementTypeEqual", "entitlement_type"},
                    };
                    
                case "KalturaEntitlementsFilter":
                    return new Dictionary<string, string>() { 
                        {"entitlementType", "entitlement_type"},
                    };
                    
                case "KalturaEPGChannelAssets":
                    return new Dictionary<string, string>() { 
                        {"channelId", "channel_id"},
                    };
                    
                case "KalturaEPGChannelAssetsListResponse":
                    return new Dictionary<string, string>() { 
                        {"objects", "assets"},
                    };
                    
                case "KalturaEpgChannelFilter":
                    return new Dictionary<string, string>() { 
                        {"endTime", "end_time"},
                        {"startTime", "start_time"},
                    };
                    
                case "KalturaExportTask":
                    return new Dictionary<string, string>() { 
                        {"dataType", "data_type"},
                        {"exportType", "export_type"},
                        {"isActive", "is_active"},
                        {"notificationUrl", "notification_url"},
                        {"vodTypes", "vod_types"},
                    };
                    
                case "KalturaExternalChannelProfile":
                    return new Dictionary<string, string>() { 
                        {"externalIdentifier", "external_identifier"},
                        {"filterExpression", "filter_expression"},
                        {"isActive", "is_active"},
                        {"recommendationEngineId", "recommendation_engine_id"},
                    };
                    
                case "KalturaFavorite":
                    return new Dictionary<string, string>() { 
                        {"extraData", "extra_data"},
                    };
                    
                case "KalturaFavoriteFilter":
                    return new Dictionary<string, string>() { 
                        {"mediaTypeIn", "media_type"},
                    };
                    
                case "KalturaFeed":
                    return new Dictionary<string, string>() { 
                        {"assetId", "asset_id"},
                    };
                    
                case "KalturaFollowDataBase":
                    return new Dictionary<string, string>() { 
                        {"announcementId", "announcement_id"},
                        {"followPhrase", "follow_phrase"},
                    };
                    
                case "KalturaFollowDataTvSeries":
                    return new Dictionary<string, string>() { 
                        {"announcementId", "announcement_id"},
                        {"assetId", "asset_id"},
                        {"followPhrase", "follow_phrase"},
                    };
                    
                case "KalturaFollowTvSeries":
                    return new Dictionary<string, string>() { 
                        {"announcementId", "announcement_id"},
                        {"followPhrase", "follow_phrase"},
                    };
                    
                case "KalturaGenericRule":
                    return new Dictionary<string, string>() { 
                        {"ruleType", "rule_type"},
                    };
                    
                case "KalturaGenericRuleFilter":
                    return new Dictionary<string, string>() { 
                        {"assetId", "asset_id"},
                        {"assetType", "asset_type"},
                    };
                    
                case "KalturaHomeNetwork":
                    return new Dictionary<string, string>() { 
                        {"externalId", "external_id"},
                        {"isActive", "is_active"},
                    };
                    
                case "KalturaHousehold":
                    return new Dictionary<string, string>() { 
                        {"concurrentLimit", "concurrent_limit"},
                        {"defaultUsers", "default_users"},
                        {"deviceFamilies", "device_families"},
                        {"devicesLimit", "devices_limit"},
                        {"externalId", "external_id"},
                        {"frequencyNextDeviceAction", "frequency_next_device_action"},
                        {"frequencyNextUserAction", "frequency_next_user_action"},
                        {"householdLimitationsId", "household_limitations_id"},
                        {"isFrequencyEnabled", "is_frequency_enabled"},
                        {"masterUsers", "master_users"},
                        {"pendingUsers", "pending_users"},
                        {"regionId", "region_id"},
                        {"usersLimit", "users_limit"},
                    };
                    
                case "KalturaHouseholdDevice":
                    return new Dictionary<string, string>() { 
                        {"activatedOn", "activated_on"},
                        {"brandId", "brand_id"},
                    };
                    
                case "KalturaHouseholdDeviceFamilyLimitations":
                    return new Dictionary<string, string>() { 
                        {"concurrentLimit", "concurrent_limit"},
                        {"deviceLimit", "device_limit"},
                    };
                    
                case "KalturaHouseholdLimitations":
                    return new Dictionary<string, string>() { 
                        {"concurrentLimit", "concurrent_limit"},
                        {"deviceFamiliesLimitations", "device_families_limitations"},
                        {"deviceFrequency", "device_frequency"},
                        {"deviceFrequencyDescription", "device_frequency_description"},
                        {"deviceLimit", "device_limit"},
                        {"npvrQuotaInSeconds", "npvr_quota_in_seconds"},
                        {"userFrequency", "user_frequency"},
                        {"userFrequencyDescription", "user_frequency_description"},
                        {"usersLimit", "users_limit"},
                    };
                    
                case "KalturaItemPrice":
                    return new Dictionary<string, string>() { 
                        {"fileId", "file_id"},
                        {"ppvPriceDetails", "ppv_price_details"},
                        {"productId", "product_id"},
                        {"productType", "product_type"},
                    };
                    
                case "KalturaLicensedUrl":
                    return new Dictionary<string, string>() { 
                        {"altUrl", "alt_url"},
                        {"mainUrl", "main_url"},
                    };
                    
                case "KalturaLoginResponse":
                    return new Dictionary<string, string>() { 
                        {"loginSession", "login_session"},
                    };
                    
                case "KalturaLoginSession":
                    return new Dictionary<string, string>() { 
                        {"refreshToken", "refresh_token"},
                    };
                    
                case "KalturaMediaFile":
                    return new Dictionary<string, string>() { 
                        {"assetId", "asset_id"},
                        {"externalId", "external_id"},
                    };
                    
                case "KalturaMediaImage":
                    return new Dictionary<string, string>() { 
                        {"isDefault", "is_default"},
                    };
                    
                case "KalturaMessageTemplate":
                    return new Dictionary<string, string>() { 
                        {"assetType", "asset_type"},
                        {"dateFormat", "date_format"},
                    };
                    
                case "KalturaNotificationSettings":
                    return new Dictionary<string, string>() { 
                        {"pushFollowEnabled", "push_follow_enabled"},
                        {"pushNotificationEnabled", "push_notification_enabled"},
                    };
                    
                case "KalturaNotificationsPartnerSettings":
                    return new Dictionary<string, string>() { 
                        {"pushEndHour", "push_end_hour"},
                        {"pushNotificationEnabled", "push_notification_enabled"},
                        {"pushStartHour", "push_start_hour"},
                        {"pushSystemAnnouncementsEnabled", "push_system_announcements_enabled"},
                    };
                    
                case "KalturaNotificationsSettings":
                    return new Dictionary<string, string>() { 
                        {"pushFollowEnabled", "push_follow_enabled"},
                        {"pushNotificationEnabled", "push_notification_enabled"},
                    };
                    
                case "KalturaOSSAdapterProfile":
                    return new Dictionary<string, string>() { 
                        {"adapterUrl", "adapter_url"},
                        {"externalIdentifier", "external_identifier"},
                        {"isActive", "is_active"},
                        {"ossAdapterSettings", "oss_adapter_settings"},
                        {"sharedSecret", "shared_secret"},
                    };
                    
                case "KalturaOTTCategory":
                    return new Dictionary<string, string>() { 
                        {"childCategories", "child_categories"},
                        {"parentCategoryId", "parent_category_id"},
                    };
                    
                case "KalturaOTTUser":
                    return new Dictionary<string, string>() { 
                        {"affiliateCode", "affiliate_code"},
                        {"dynamicData", "dynamic_data"},
                        {"externalId", "external_id"},
                        {"facebookId", "facebook_id"},
                        {"facebookImage", "facebook_image"},
                        {"facebookToken", "facebook_token"},
                        {"firstName", "first_name"},
                        {"householdId", "household_id"},
                        {"isHouseholdMaster", "is_household_master"},
                        {"lastName", "last_name"},
                        {"suspentionState", "suspention_state"},
                        {"userState", "user_state"},
                        {"userType", "user_type"},
                    };
                    
                case "KalturaParentalRule":
                    return new Dictionary<string, string>() { 
                        {"blockAnonymousAccess", "block_anonymous_access"},
                        {"epgTag", "epg_tag"},
                        {"epgTagValues", "epg_tag_values"},
                        {"isDefault", "is_default"},
                        {"mediaTag", "media_tag"},
                        {"mediaTagValues", "media_tag_values"},
                        {"ruleType", "rule_type"},
                    };
                    
                case "KalturaPartnerNotificationSettings":
                    return new Dictionary<string, string>() { 
                        {"pushEndHour", "push_end_hour"},
                        {"pushNotificationEnabled", "push_notification_enabled"},
                        {"pushStartHour", "push_start_hour"},
                        {"pushSystemAnnouncementsEnabled", "push_system_announcements_enabled"},
                    };
                    
                case "KalturaPaymentGatewayBaseProfile":
                    return new Dictionary<string, string>() { 
                        {"isDefault", "is_default"},
                        {"paymentMethods", "payment_methods"},
                        {"selectedBy", "selected_by"},
                    };
                    
                case "KalturaPaymentGatewayConfiguration":
                    return new Dictionary<string, string>() { 
                        {"paymentGatewayConfiguration", "payment_gatewaye_configuration"},
                    };
                    
                case "KalturaPaymentGatewayProfile":
                    return new Dictionary<string, string>() { 
                        {"adapterUrl", "adapter_url"},
                        {"externalIdentifier", "external_identifier"},
                        {"isActive", "is_active"},
                        {"isDefault", "is_default"},
                        {"paymentMethods", "payment_methods"},
                        {"pendingInterval", "pending_interval"},
                        {"pendingRetries", "pending_retries"},
                        {"renewIntervalMinutes", "renew_interval_minutes"},
                        {"renewStartMinutes", "renew_start_minutes"},
                        {"renewUrl", "renew_url"},
                        {"selectedBy", "selected_by"},
                        {"paymentGatewaySettings", "payment_gateway_settings"},
                        {"sharedSecret", "shared_secret"},
                        {"statusUrl", "status_url"},
                        {"transactUrl", "transact_url"},
                    };
                    
                case "KalturaPaymentMethod":
                    return new Dictionary<string, string>() { 
                        {"allowMultiInstance", "allow_multi_instance"},
                        {"householdPaymentMethods", "household_payment_methods"},
                    };
                    
                case "KalturaPaymentMethodProfile":
                    return new Dictionary<string, string>() { 
                        {"allowMultiInstance", "allow_multi_instance"},
                    };
                    
                case "KalturaPersonalAssetRequest":
                    return new Dictionary<string, string>() { 
                        {"fileIds", "file_ids"},
                    };
                    
                case "KalturaPersonalFeed":
                    return new Dictionary<string, string>() { 
                        {"assetId", "asset_id"},
                    };
                    
                case "KalturaPersonalFollowFeed":
                    return new Dictionary<string, string>() { 
                        {"assetId", "asset_id"},
                    };
                    
                case "KalturaPlaybackSource":
                    return new Dictionary<string, string>() { 
                        {"assetId", "asset_id"},
                        {"externalId", "external_id"},
                    };
                    
                case "KalturaPlayerAssetData":
                    return new Dictionary<string, string>() { 
                        {"averageBitrate", "average_bitrate"},
                        {"currentBitrate", "current_bitrate"},
                        {"totalBitrate", "total_bitrate"},
                    };
                    
                case "KalturaPpvEntitlement":
                    return new Dictionary<string, string>() { 
                        {"currentDate", "current_date"},
                        {"currentUses", "current_uses"},
                        {"deviceName", "device_name"},
                        {"deviceUdid", "device_udid"},
                        {"endDate", "end_date"},
                        {"entitlementId", "entitlement_id"},
                        {"isCancelationWindowEnabled", "is_cancelation_window_enabled"},
                        {"isInGracePeriod", "is_in_grace_period"},
                        {"isRenewable", "is_renewable"},
                        {"isRenewableForPurchase", "is_renewable_for_purchase"},
                        {"lastViewDate", "last_view_date"},
                        {"maxUses", "max_uses"},
                        {"mediaFileId", "media_file_id"},
                        {"mediaId", "media_id"},
                        {"nextRenewalDate", "next_renewal_date"},
                        {"paymentMethod", "payment_method"},
                        {"purchaseDate", "purchase_date"},
                        {"purchaseId", "purchase_id"},
                    };
                    
                case "KalturaPPVItemPriceDetails":
                    return new Dictionary<string, string>() { 
                        {"collectionId", "collection_id"},
                        {"discountEndDate", "discount_end_date"},
                        {"endDate", "end_date"},
                        {"firstDeviceName", "first_device_name"},
                        {"fullPrice", "full_price"},
                        {"isInCancelationPeriod", "is_in_cancelation_period"},
                        {"isSubscriptionOnly", "is_subscription_only"},
                        {"ppvDescriptions", "ppv_descriptions"},
                        {"ppvModuleId", "ppv_module_id"},
                        {"prePaidId", "pre_paid_id"},
                        {"ppvProductCode", "ppv_product_code"},
                        {"purchasedMediaFileId", "purchased_media_file_id"},
                        {"purchaseStatus", "purchase_status"},
                        {"purchaseUserId", "purchase_user_id"},
                        {"relatedMediaFileIds", "related_media_file_ids"},
                        {"startDate", "start_date"},
                        {"subscriptionId", "subscription_id"},
                    };
                    
                case "KalturaPpvPrice":
                    return new Dictionary<string, string>() { 
                        {"productId", "product_id"},
                        {"productType", "product_type"},
                    };
                    
                case "KalturaPreviewModule":
                    return new Dictionary<string, string>() { 
                        {"lifeCycle", "life_cycle"},
                        {"nonRenewablePeriod", "non_renewable_period"},
                    };
                    
                case "KalturaPrice":
                    return new Dictionary<string, string>() { 
                        {"currencySign", "currency_sign"},
                    };
                    
                case "KalturaPricePlan":
                    return new Dictionary<string, string>() { 
                        {"couponId", "coupon_id"},
                        {"discountId", "discount_id"},
                        {"fullLifeCycle", "full_life_cycle"},
                        {"isOfflinePlayback", "is_offline_playback"},
                        {"isRenewable", "is_renewable"},
                        {"isWaiverEnabled", "is_waiver_enabled"},
                        {"maxViewsNumber", "max_views_number"},
                        {"priceId", "price_id"},
                        {"renewalsNumber", "renewals_number"},
                        {"viewLifeCycle", "view_life_cycle"},
                        {"waiverPeriod", "waiver_period"},
                    };
                    
                case "KalturaPricesFilter":
                    return new Dictionary<string, string>() { 
                        {"filesIds", "files_ids"},
                        {"shouldGetOnlyLowest", "should_get_only_lowest"},
                        {"subscriptionsIds", "subscriptions_ids"},
                    };
                    
                case "KalturaProductPrice":
                    return new Dictionary<string, string>() { 
                        {"productId", "product_id"},
                        {"productType", "product_type"},
                    };
                    
                case "KalturaPurchaseSettingsResponse":
                    return new Dictionary<string, string>() { 
                        {"purchaseSettingsType", "purchase_settings_type"},
                    };
                    
                case "KalturaRecommendationProfile":
                    return new Dictionary<string, string>() { 
                        {"adapterUrl", "adapter_url"},
                        {"externalIdentifier", "external_identifier"},
                        {"isActive", "is_active"},
                        {"recommendationEngineSettings", "recommendation_engine_settings"},
                        {"sharedSecret", "shared_secret"},
                    };
                    
                case "KalturaSocialFacebookConfig":
                    return new Dictionary<string, string>() { 
                        {"appId", "app_id"},
                    };
                    
                case "KalturaSocialResponse":
                    return new Dictionary<string, string>() { 
                        {"kalturaUsername", "kaltura_username"},
                        {"minFriendsLimitation", "min_friends_limitation"},
                        {"socialUsername", "social_username"},
                        {"socialUser", "social_user"},
                        {"userId", "user_id"},
                    };
                    
                case "KalturaSocialUser":
                    return new Dictionary<string, string>() { 
                        {"firstName", "first_name"},
                        {"lastName", "last_name"},
                        {"userId", "user_id"},
                    };
                    
                case "KalturaSubscription":
                    return new Dictionary<string, string>() { 
                        {"couponsGroup", "coupons_group"},
                        {"discountModule", "discount_module"},
                        {"endDate", "end_date"},
                        {"fileTypes", "file_types"},
                        {"gracePeriodMinutes", "grace_period_minutes"},
                        {"householdLimitationsId", "household_limitations_id"},
                        {"isInfiniteRenewal", "is_infinite_renewal"},
                        {"isRenewable", "is_renewable"},
                        {"isWaiverEnabled", "is_waiver_enabled"},
                        {"maxViewsNumber", "max_views_number"},
                        {"mediaId", "media_id"},
                        {"premiumServices", "premium_services"},
                        {"previewModule", "preview_module"},
                        {"pricePlans", "price_plans"},
                        {"productCode", "product_code"},
                        {"prorityInOrder", "prority_in_order"},
                        {"renewalsNumber", "renewals_number"},
                        {"startDate", "start_date"},
                        {"userTypes", "user_types"},
                        {"viewLifeCycle", "view_life_cycle"},
                        {"waiverPeriod", "waiver_period"},
                    };
                    
                case "KalturaSubscriptionEntitlement":
                    return new Dictionary<string, string>() { 
                        {"currentDate", "current_date"},
                        {"currentUses", "current_uses"},
                        {"deviceName", "device_name"},
                        {"deviceUdid", "device_udid"},
                        {"endDate", "end_date"},
                        {"entitlementId", "entitlement_id"},
                        {"isCancelationWindowEnabled", "is_cancelation_window_enabled"},
                        {"isInGracePeriod", "is_in_grace_period"},
                        {"isRenewable", "is_renewable"},
                        {"isRenewableForPurchase", "is_renewable_for_purchase"},
                        {"lastViewDate", "last_view_date"},
                        {"maxUses", "max_uses"},
                        {"mediaFileId", "media_file_id"},
                        {"mediaId", "media_id"},
                        {"nextRenewalDate", "next_renewal_date"},
                        {"paymentMethod", "payment_method"},
                        {"purchaseDate", "purchase_date"},
                        {"purchaseId", "purchase_id"},
                    };
                    
                case "KalturaSubscriptionPrice":
                    return new Dictionary<string, string>() { 
                        {"productId", "product_id"},
                        {"productType", "product_type"},
                        {"purchaseStatus", "purchase_status"},
                    };
                    
                case "KalturaTimeShiftedTvPartnerSettings":
                    return new Dictionary<string, string>() { 
                        {"catchUpBufferLength", "catch_up_buffer_length"},
                        {"catchUpEnabled", "catch_up_enabled"},
                        {"cdvrEnabled", "cdvr_enabled"},
                        {"recordingScheduleWindow", "recording_schedule_window"},
                        {"recordingScheduleWindowEnabled", "recording_schedule_window_enabled"},
                        {"startOverEnabled", "start_over_enabled"},
                        {"trickPlayBufferLength", "trick_play_buffer_length"},
                        {"trickPlayEnabled", "trick_play_enabled"},
                    };
                    
                case "KalturaTransaction":
                    return new Dictionary<string, string>() { 
                        {"createdAt", "created_at"},
                        {"failReasonCode", "fail_reason_code"},
                        {"paymentGatewayReferenceId", "payment_gateway_reference_id"},
                        {"paymentGatewayResponseId", "payment_gateway_response_id"},
                    };
                    
                case "KalturaTransactionsFilter":
                    return new Dictionary<string, string>() { 
                        {"endDate", "end_date"},
                        {"startDate", "start_date"},
                    };
                    
                case "KalturaUsageModule":
                    return new Dictionary<string, string>() { 
                        {"couponId", "coupon_id"},
                        {"fullLifeCycle", "full_life_cycle"},
                        {"isOfflinePlayback", "is_offline_playback"},
                        {"isWaiverEnabled", "is_waiver_enabled"},
                        {"maxViewsNumber", "max_views_number"},
                        {"viewLifeCycle", "view_life_cycle"},
                        {"waiverPeriod", "waiver_period"},
                    };
                    
                case "KalturaUserAssetsList":
                    return new Dictionary<string, string>() { 
                        {"listType", "list_type"},
                    };
                    
                case "KalturaUserAssetsListFilter":
                    return new Dictionary<string, string>() { 
                        {"assetTypeEqual", "asset_type"},
                        {"listTypeEqual", "list_type"},
                    };
                    
                case "KalturaUserAssetsListItem":
                    return new Dictionary<string, string>() { 
                        {"listType", "list_type"},
                        {"orderIndex", "order_index"},
                        {"userId", "user_id"},
                    };
                    
                case "KalturaUserBillingTransaction":
                    return new Dictionary<string, string>() { 
                        {"actionDate", "action_date"},
                        {"billingAction", "billing_action"},
                        {"billingProviderRef", "billing_provider_ref"},
                        {"endDate", "end_date"},
                        {"isRecurring", "is_recurring"},
                        {"itemType", "item_type"},
                        {"paymentMethod", "payment_method"},
                        {"paymentMethodExtraDetails", "payment_method_extra_details"},
                        {"purchasedItemCode", "purchased_item_code"},
                        {"purchasedItemName", "purchased_item_name"},
                        {"purchaseId", "purchase_id"},
                        {"recieptCode", "reciept_code"},
                        {"startDate", "start_date"},
                        {"userFullName", "user_full_name"},
                        {"userId", "user_id"},
                    };
                    
                case "KalturaUserLoginPin":
                    return new Dictionary<string, string>() { 
                        {"expirationTime", "expiration_time"},
                        {"pinCode", "pin_code"},
                        {"userId", "user_id"},
                    };
                    
                case "KalturaWatchHistoryAsset":
                    return new Dictionary<string, string>() { 
                        {"finishedWatching", "finished_watching"},
                        {"watchedDate", "watched_date"},
                    };
                    
                case "AnnouncementController":
                    return new Dictionary<string, string>() { 
                        {"addOldStandard", "add"},
                        {"enableSystemAnnouncements", "createAnnouncement"},
                        {"listOldStandard", "list"},
                        {"updateOldStandard", "update"},
                    };
                    
                case "AssetController":
                    return new Dictionary<string, string>() { 
                        {"getOldStandard", "get"},
                        {"listOldStandard", "list"},
                    };
                    
                case "AssetHistoryController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                    };
                    
                case "BookmarkController":
                    return new Dictionary<string, string>() { 
                        {"addOldStandard", "add"},
                        {"listOldStandard", "list"},
                    };
                    
                case "CDVRAdapterProfileController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                        {"updateOldStandard", "update"},
                    };
                    
                case "ChannelController":
                    return new Dictionary<string, string>() { 
                        {"addOldStandard", "add"},
                        {"updateOldStandard", "update"},
                    };
                    
                case "EntitlementController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                    };
                    
                case "ExportTaskController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                        {"updateOldStandard", "update"},
                    };
                    
                case "ExternalChannelProfileController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                        {"updateOldStandard", "update"},
                    };
                    
                case "FavoriteController":
                    return new Dictionary<string, string>() { 
                        {"addOldStandard", "add"},
                        {"deleteOldStandard", "delete"},
                        {"listOldStandard", "list"},
                    };
                    
                case "FollowTvSeriesController":
                    return new Dictionary<string, string>() { 
                        {"addOldStandard", "add"},
                        {"listOldStandard", "list"},
                    };
                    
                case "HomeNetworkController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                        {"updateOldStandard", "update"},
                    };
                    
                case "HouseholdController":
                    return new Dictionary<string, string>() { 
                        {"addOldStandard", "add"},
                        {"getOldStandard", "get"},
                        {"updateOldStandard", "update"},
                    };
                    
                case "HouseholdDeviceController":
                    return new Dictionary<string, string>() { 
                        {"addOldStandard", "add"},
                        {"updateOldStandard", "update"},
                    };
                    
                case "HouseholdPaymentGatewayController":
                    return new Dictionary<string, string>() { 
                        {"disable", "delete"},
                        {"enable", "set"},
                    };
                    
                case "HouseholdPremiumServiceController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                    };
                    
                case "HouseholdUserController":
                    return new Dictionary<string, string>() { 
                        {"addOldStandard", "add"},
                    };
                    
                case "LicensedUrlController":
                    return new Dictionary<string, string>() { 
                        {"getOldStandard", "get"},
                    };
                    
                case "NotificationsPartnerSettingsController":
                    return new Dictionary<string, string>() { 
                        {"getOldStandard", "get"},
                        {"updateOldStandard", "update"},
                    };
                    
                case "NotificationsSettingsController":
                    return new Dictionary<string, string>() { 
                        {"getOldStandard", "get"},
                        {"updateOldStandard", "update"},
                    };
                    
                case "OssAdapterProfileController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                        {"updateOldStandard", "update"},
                    };
                    
                case "OttUserController":
                    return new Dictionary<string, string>() { 
                        {"getOldStandard", "get"},
                        {"register", "add"},
                        {"resetPassword", "sendPassword"},
                        {"setPassword", "resetPassword"},
                        {"updateLoginData", "changePassword"},
                    };
                    
                case "ParentalRuleController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                    };
                    
                case "PaymentGatewayProfileController":
                    return new Dictionary<string, string>() { 
                        {"addOldStandard", "add"},
                        {"listOldStandard", "list"},
                        {"updateOldStandard", "update"},
                    };
                    
                case "PaymentGatewayProfileSettingsController":
                    return new Dictionary<string, string>() { 
                        {"addOldStandard", "add"},
                    };
                    
                case "PaymentMethodProfileController":
                    return new Dictionary<string, string>() { 
                        {"addOldStandard", "add"},
                        {"deleteOldStandard", "delete"},
                        {"listOldStandard", "list"},
                        {"updateOldStandard", "update"},
                    };
                    
                case "PersonalFeedController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                    };
                    
                case "PinController":
                    return new Dictionary<string, string>() { 
                        {"getOldStandard", "get"},
                        {"updateOldStandard", "update"},
                    };
                    
                case "PurchaseSettingsController":
                    return new Dictionary<string, string>() { 
                        {"getOldStandard", "get"},
                        {"updateOldStandard", "update"},
                    };
                    
                case "RecommendationProfileController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                        {"updateOldStandard", "update"},
                    };
                    
                case "RegistrySettingsController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                    };
                    
                case "SessionController":
                    return new Dictionary<string, string>() { 
                        {"getOldStandard", "get"},
                    };
                    
                case "SocialController":
                    return new Dictionary<string, string>() { 
                        {"getByTokenOldStandard", "getByToken"},
                        {"getConfiguration", "config"},
                        {"mergeOldStandard", "merge"},
                        {"registerOldStandard", "register"},
                        {"unmergeOldStandard", "unmerge"},
                    };
                    
                case "SubscriptionController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                    };
                    
                case "TopicController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                    };
                    
                case "TransactionController":
                    return new Dictionary<string, string>() { 
                        {"purchaseOldStandard", "purchase"},
                        {"purchaseSessionIdOldStandard", "purchaseSessionId"},
                        {"setWaiver", "waiver"},
                    };
                    
                case "TransactionHistoryController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                    };
                    
                case "UserAssetRuleController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                    };
                    
                case "UserAssetsListItemController":
                    return new Dictionary<string, string>() { 
                        {"deleteOldStandard", "delete"},
                        {"getOldStandard", "get"},
                    };
                    
                case "UserRoleController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                    };
                    
            }
            
            return null;
        }
        
        public static string getApiName(PropertyInfo property)
        {
            switch (property.DeclaringType.Name)
            {
                case "KalturaAccessControlMessage":
                    switch(property.Name)
                    {
                        case "Code":
                            return "code";
                        case "Message":
                            return "message";
                    }
                    break;
                    
                case "KalturaActionPermissionItem":
                    switch(property.Name)
                    {
                        case "Action":
                            return "action";
                        case "ActionPrivacy":
                            return "actionPrivacy";
                        case "Network":
                            return "network";
                        case "Privacy":
                            return "privacy";
                    }
                    break;
                    
                case "KalturaAnnouncement":
                    switch(property.Name)
                    {
                        case "Enabled":
                            return "enabled";
                        case "Id":
                            return "id";
                        case "Message":
                            return "message";
                        case "Name":
                            return "name";
                        case "Recipients":
                            return "recipients";
                        case "StartTime":
                            return "startTime";
                        case "Status":
                            return "status";
                        case "Timezone":
                            return "timezone";
                    }
                    break;
                    
                case "KalturaAnnouncementListResponse":
                    switch(property.Name)
                    {
                        case "Announcements":
                            return "objects";
                    }
                    break;
                    
                case "KalturaApiActionPermissionItem":
                    switch(property.Name)
                    {
                        case "Action":
                            return "action";
                        case "Service":
                            return "service";
                    }
                    break;
                    
                case "KalturaApiArgumentPermissionItem":
                    switch(property.Name)
                    {
                        case "Action":
                            return "action";
                        case "Parameter":
                            return "parameter";
                        case "Service":
                            return "service";
                    }
                    break;
                    
                case "KalturaApiParameterPermissionItem":
                    switch(property.Name)
                    {
                        case "Action":
                            return "action";
                        case "Object":
                            return "object";
                        case "Parameter":
                            return "parameter";
                    }
                    break;
                    
                case "KalturaAppToken":
                    switch(property.Name)
                    {
                        case "Expiry":
                            return "expiry";
                        case "HashType":
                            return "hashType";
                        case "Id":
                            return "id";
                        case "PartnerId":
                            return "partnerId";
                        case "SessionDuration":
                            return "sessionDuration";
                        case "SessionPrivileges":
                            return "sessionPrivileges";
                        case "SessionType":
                            return "sessionType";
                        case "SessionUserId":
                            return "sessionUserId";
                        case "Status":
                            return "status";
                        case "Token":
                            return "token";
                    }
                    break;
                    
                case "KalturaAsset":
                    switch(property.Name)
                    {
                        case "Description":
                            return "description";
                        case "EnableCatchUp":
                            return "enableCatchUp";
                        case "EnableCdvr":
                            return "enableCdvr";
                        case "EnableStartOver":
                            return "enableStartOver";
                        case "EnableTrickPlay":
                            return "enableTrickPlay";
                        case "EndDate":
                            return "endDate";
                        case "ExternalId":
                            return "externalId";
                        case "Id":
                            return "id";
                        case "Images":
                            return "images";
                        case "MediaFiles":
                            return "mediaFiles";
                        case "Metas":
                            return "metas";
                        case "Name":
                            return "name";
                        case "StartDate":
                            return "startDate";
                        case "Statistics":
                            return "stats";
                        case "Tags":
                            return "tags";
                        case "Type":
                            return "type";
                    }
                    break;
                    
                case "KalturaAssetBookmark":
                    switch(property.Name)
                    {
                        case "IsFinishedWatching":
                            return "finishedWatching";
                        case "Position":
                            return "position";
                        case "PositionOwner":
                            return "positionOwner";
                        case "User":
                            return "user";
                    }
                    break;
                    
                case "KalturaAssetBookmarks":
                    switch(property.Name)
                    {
                        case "Bookmarks":
                            return "objects";
                    }
                    break;
                    
                case "KalturaAssetComment":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "assetId";
                        case "AssetType":
                            return "assetType";
                        case "Id":
                            return "id";
                        case "SubHeader":
                            return "subHeader";
                    }
                    break;
                    
                case "KalturaAssetCommentFilter":
                    switch(property.Name)
                    {
                        case "AssetIdEqual":
                            return "assetIdEqual";
                        case "AssetTypeEqual":
                            return "assetTypeEqual";
                    }
                    break;
                    
                case "KalturaAssetCommentListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaAssetCount":
                    switch(property.Name)
                    {
                        case "Count":
                            return "count";
                        case "SubCounts":
                            return "subs";
                        case "Value":
                            return "value";
                    }
                    break;
                    
                case "KalturaAssetCountListResponse":
                    switch(property.Name)
                    {
                        case "AssetsCount":
                            return "assetsCount";
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaAssetFieldGroupBy":
                    switch(property.Name)
                    {
                        case "Value":
                            return "value";
                    }
                    break;
                    
                case "KalturaAssetFileContext":
                    switch(property.Name)
                    {
                        case "FullLifeCycle":
                            return "fullLifeCycle";
                        case "IsOfflinePlayBack":
                            return "isOfflinePlayBack";
                        case "ViewLifeCycle":
                            return "viewLifeCycle";
                    }
                    break;
                    
                case "KalturaAssetFilter":
                    switch(property.Name)
                    {
                        case "DynamicOrderBy":
                            return "dynamicOrderBy";
                    }
                    break;
                    
                case "KalturaAssetHistory":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "assetId";
                        case "AssetType":
                            return "assetType";
                        case "Duration":
                            return "duration";
                        case "IsFinishedWatching":
                            return "finishedWatching";
                        case "LastWatched":
                            return "watchedDate";
                        case "Position":
                            return "position";
                    }
                    break;
                    
                case "KalturaAssetHistoryFilter":
                    switch(property.Name)
                    {
                        case "AssetIdIn":
                            return "assetIdIn";
                        case "DaysLessThanOrEqual":
                            return "daysLessThanOrEqual";
                        case "StatusEqual":
                            return "statusEqual";
                        case "TypeIn":
                            return "typeIn";
                    }
                    break;
                    
                case "KalturaAssetHistoryListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaAssetInfo":
                    switch(property.Name)
                    {
                        case "EndDate":
                            return "endDate";
                        case "ExtraParams":
                            return "extraParams";
                        case "Metas":
                            return "metas";
                        case "StartDate":
                            return "startDate";
                        case "Tags":
                            return "tags";
                    }
                    break;
                    
                case "KalturaAssetInfoFilter":
                    switch(property.Name)
                    {
                        case "cutWith":
                            return "cut_with";
                        case "FilterTags":
                            return "filter_tags";
                        case "IDs":
                            return "ids";
                        case "ReferenceType":
                            return "referenceType";
                    }
                    break;
                    
                case "KalturaAssetInfoListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                        case "RequestId":
                            return "requestId";
                    }
                    break;
                    
                case "KalturaAssetListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaAssetMetaOrTagGroupBy":
                    switch(property.Name)
                    {
                        case "Value":
                            return "value";
                    }
                    break;
                    
                case "KalturaAssetPrice":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "asset_id";
                        case "AssetType":
                            return "asset_type";
                        case "FilePrices":
                            return "file_prices";
                    }
                    break;
                    
                case "KalturaAssetReminder":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "assetId";
                    }
                    break;
                    
                case "KalturaAssetsBookmarksResponse":
                    switch(property.Name)
                    {
                        case "AssetsBookmarks":
                            return "objects";
                    }
                    break;
                    
                case "KalturaAssetsCount":
                    switch(property.Name)
                    {
                        case "Field":
                            return "field";
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaAssetsFilter":
                    switch(property.Name)
                    {
                        case "Assets":
                            return "assets";
                    }
                    break;
                    
                case "KalturaAssetStatistics":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "assetId";
                        case "BuzzAvgScore":
                            return "buzzScore";
                        case "Likes":
                            return "likes";
                        case "Rating":
                            return "rating";
                        case "RatingCount":
                            return "ratingCount";
                        case "Views":
                            return "views";
                    }
                    break;
                    
                case "KalturaAssetStatisticsListResponse":
                    switch(property.Name)
                    {
                        case "AssetsStatistics":
                            return "objects";
                    }
                    break;
                    
                case "KalturaAssetStatisticsQuery":
                    switch(property.Name)
                    {
                        case "AssetIdIn":
                            return "assetIdIn";
                        case "AssetTypeEqual":
                            return "assetTypeEqual";
                        case "EndDateGreaterThanOrEqual":
                            return "endDateGreaterThanOrEqual";
                        case "StartDateGreaterThanOrEqual":
                            return "startDateGreaterThanOrEqual";
                    }
                    break;
                    
                case "KalturaBaseAssetInfo":
                    switch(property.Name)
                    {
                        case "Description":
                            return "description";
                        case "Id":
                            return "id";
                        case "Images":
                            return "images";
                        case "MediaFiles":
                            return "mediaFiles";
                        case "Name":
                            return "name";
                        case "Statistics":
                            return "stats";
                        case "Type":
                            return "type";
                    }
                    break;
                    
                case "KalturaBaseChannel":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                    }
                    break;
                    
                case "KalturaBaseOTTUser":
                    switch(property.Name)
                    {
                        case "FirstName":
                            return "firstName";
                        case "Id":
                            return "id";
                        case "LastName":
                            return "lastName";
                        case "Username":
                            return "username";
                    }
                    break;
                    
                case "KalturaBillingPartnerConfig":
                    switch(property.Name)
                    {
                        case "PartnerConfigurationType":
                            return "partnerConfigurationType";
                        case "Type":
                            return "type";
                        case "Value":
                            return "value";
                    }
                    break;
                    
                case "KalturaBillingResponse":
                    switch(property.Name)
                    {
                        case "ExternalReceiptCode":
                            return "externalReceiptCode";
                        case "ReceiptCode":
                            return "receiptCode";
                    }
                    break;
                    
                case "KalturaBillingTransaction":
                    switch(property.Name)
                    {
                        case "purchaseID":
                            return "purchaseId";
                    }
                    break;
                    
                case "KalturaBillingTransactionListResponse":
                    switch(property.Name)
                    {
                        case "transactions":
                            return "objects";
                    }
                    break;
                    
                case "KalturaBookmark":
                    switch(property.Name)
                    {
                        case "IsFinishedWatching":
                            return "finishedWatching";
                        case "PlayerData":
                            return "playerData";
                        case "Position":
                            return "position";
                        case "PositionOwner":
                            return "positionOwner";
                        case "User":
                            return "user";
                        case "UserId":
                            return "userId";
                    }
                    break;
                    
                case "KalturaBookmarkFilter":
                    switch(property.Name)
                    {
                        case "AssetIdIn":
                            return "assetIdIn";
                        case "AssetIn":
                            return "assetIn";
                        case "AssetTypeEqual":
                            return "assetTypeEqual";
                    }
                    break;
                    
                case "KalturaBookmarkListResponse":
                    switch(property.Name)
                    {
                        case "AssetsBookmarks":
                            return "objects";
                    }
                    break;
                    
                case "KalturaBookmarkPlayerData":
                    switch(property.Name)
                    {
                        case "averageBitRate":
                            return "averageBitrate";
                        case "currentBitRate":
                            return "currentBitrate";
                        case "FileId":
                            return "fileId";
                        case "totalBitRate":
                            return "totalBitrate";
                    }
                    break;
                    
                case "KalturaBundleFilter":
                    switch(property.Name)
                    {
                        case "BundleTypeEqual":
                            return "bundleTypeEqual";
                        case "IdEqual":
                            return "idEqual";
                        case "TypeIn":
                            return "typeIn";
                    }
                    break;
                    
                case "KalturaBuzzScore":
                    switch(property.Name)
                    {
                        case "AvgScore":
                            return "avgScore";
                        case "NormalizedAvgScore":
                            return "normalizedAvgScore";
                        case "UpdateDate":
                            return "updateDate";
                    }
                    break;
                    
                case "KalturaCDNAdapterProfile":
                    switch(property.Name)
                    {
                        case "AdapterUrl":
                            return "adapterUrl";
                        case "BaseUrl":
                            return "baseUrl";
                        case "Id":
                            return "id";
                        case "IsActive":
                            return "isActive";
                        case "Name":
                            return "name";
                        case "Settings":
                            return "settings";
                        case "SharedSecret":
                            return "sharedSecret";
                        case "SystemName":
                            return "systemName";
                    }
                    break;
                    
                case "KalturaCDNAdapterProfileListResponse":
                    switch(property.Name)
                    {
                        case "Adapters":
                            return "objects";
                    }
                    break;
                    
                case "KalturaCDNPartnerSettings":
                    switch(property.Name)
                    {
                        case "DefaultAdapterId":
                            return "defaultAdapterId";
                        case "DefaultRecordingAdapterId":
                            return "defaultRecordingAdapterId";
                    }
                    break;
                    
                case "KalturaCDVRAdapterProfile":
                    switch(property.Name)
                    {
                        case "AdapterUrl":
                            return "adapterUrl";
                        case "DynamicLinksSupport":
                            return "dynamicLinksSupport";
                        case "ExternalIdentifier":
                            return "externalIdentifier";
                        case "Id":
                            return "id";
                        case "IsActive":
                            return "isActive";
                        case "Name":
                            return "name";
                        case "Settings":
                            return "settings";
                        case "SharedSecret":
                            return "sharedSecret";
                    }
                    break;
                    
                case "KalturaCDVRAdapterProfileListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaChannel":
                    switch(property.Name)
                    {
                        case "AssetTypes":
                            return "assetTypes";
                        case "Description":
                            return "description";
                        case "FilterExpression":
                            return "filterExpression";
                        case "Images":
                            return "images";
                        case "IsActive":
                            return "isActive";
                        case "MediaTypes":
                            return "media_types";
                        case "Order":
                            return "order";
                    }
                    break;
                    
                case "KalturaChannelExternalFilter":
                    switch(property.Name)
                    {
                        case "FreeText":
                            return "freeText";
                        case "IdEqual":
                            return "idEqual";
                        case "UtcOffsetEqual":
                            return "utcOffsetEqual";
                    }
                    break;
                    
                case "KalturaChannelFilter":
                    switch(property.Name)
                    {
                        case "IdEqual":
                            return "idEqual";
                        case "KSql":
                            return "kSql";
                        case "OrderBy":
                            return "orderBy";
                    }
                    break;
                    
                case "KalturaChannelProfile":
                    switch(property.Name)
                    {
                        case "AssetTypes":
                            return "assetTypes";
                        case "Description":
                            return "description";
                        case "FilterExpression":
                            return "filterExpression";
                        case "Id":
                            return "id";
                        case "IsActive":
                            return "isActive";
                        case "Name":
                            return "name";
                        case "Order":
                            return "order";
                    }
                    break;
                    
                case "KalturaClientConfiguration":
                    switch(property.Name)
                    {
                        case "ApiVersion":
                            return "apiVersion";
                        case "ClientTag":
                            return "clientTag";
                    }
                    break;
                    
                case "KalturaCompensation":
                    switch(property.Name)
                    {
                        case "Amount":
                            return "amount";
                        case "AppliedRenewalIterations":
                            return "appliedRenewalIterations";
                        case "CompensationType":
                            return "compensationType";
                        case "Id":
                            return "id";
                        case "PurchaseId":
                            return "purchaseId";
                        case "SubscriptionId":
                            return "subscriptionId";
                        case "TotalRenewalIterations":
                            return "totalRenewalIterations";
                    }
                    break;
                    
                case "KalturaConfigurationGroup":
                    switch(property.Name)
                    {
                        case "ConfigurationIdentifiers":
                            return "configurationIdentifiers";
                        case "Id":
                            return "id";
                        case "IsDefault":
                            return "isDefault";
                        case "Name":
                            return "name";
                        case "NumberOfDevices":
                            return "numberOfDevices";
                        case "PartnerId":
                            return "partnerId";
                        case "Tags":
                            return "tags";
                    }
                    break;
                    
                case "KalturaConfigurationGroupDevice":
                    switch(property.Name)
                    {
                        case "ConfigurationGroupId":
                            return "configurationGroupId";
                        case "PartnerId":
                            return "partnerId";
                        case "Udid":
                            return "udid";
                    }
                    break;
                    
                case "KalturaConfigurationGroupDeviceFilter":
                    switch(property.Name)
                    {
                        case "ConfigurationGroupIdEqual":
                            return "configurationGroupIdEqual";
                    }
                    break;
                    
                case "KalturaConfigurationGroupDeviceListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaConfigurationGroupListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaConfigurationGroupTag":
                    switch(property.Name)
                    {
                        case "ConfigurationGroupId":
                            return "configurationGroupId";
                        case "PartnerId":
                            return "partnerId";
                        case "Tag":
                            return "tag";
                    }
                    break;
                    
                case "KalturaConfigurationGroupTagFilter":
                    switch(property.Name)
                    {
                        case "ConfigurationGroupIdEqual":
                            return "configurationGroupIdEqual";
                    }
                    break;
                    
                case "KalturaConfigurationGroupTagListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaConfigurationIdentifier":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                    }
                    break;
                    
                case "KalturaConfigurations":
                    switch(property.Name)
                    {
                        case "AppName":
                            return "appName";
                        case "ClientVersion":
                            return "clientVersion";
                        case "ConfigurationGroupId":
                            return "configurationGroupId";
                        case "Content":
                            return "content";
                        case "ExternalPushId":
                            return "externalPushId";
                        case "Id":
                            return "id";
                        case "IsForceUpdate":
                            return "isForceUpdate";
                        case "PartnerId":
                            return "partnerId";
                        case "Platform":
                            return "platform";
                    }
                    break;
                    
                case "KalturaConfigurationsFilter":
                    switch(property.Name)
                    {
                        case "ConfigurationGroupIdEqual":
                            return "configurationGroupIdEqual";
                    }
                    break;
                    
                case "KalturaConfigurationsListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaCountry":
                    switch(property.Name)
                    {
                        case "Code":
                            return "code";
                        case "CurrencyCode":
                            return "currency";
                        case "CurrencySign":
                            return "currencySign";
                        case "Id":
                            return "id";
                        case "LanguagesCode":
                            return "languagesCode";
                        case "MainLanguageCode":
                            return "mainLanguageCode";
                        case "Name":
                            return "name";
                        case "VatPercent":
                            return "vatPercent";
                    }
                    break;
                    
                case "KalturaCountryFilter":
                    switch(property.Name)
                    {
                        case "IdIn":
                            return "idIn";
                        case "IpEqual":
                            return "ipEqual";
                        case "IpEqualCurrent":
                            return "ipEqualCurrent";
                    }
                    break;
                    
                case "KalturaCountryListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaCoupon":
                    switch(property.Name)
                    {
                        case "CouponsGroup":
                            return "couponsGroup";
                        case "Status":
                            return "status";
                    }
                    break;
                    
                case "KalturaCouponsGroup":
                    switch(property.Name)
                    {
                        case "CouponGroupType":
                            return "couponGroupType";
                        case "Descriptions":
                            return "descriptions";
                        case "EndDate":
                            return "endDate";
                        case "Id":
                            return "id";
                        case "MaxUsesNumber":
                            return "maxUsesNumber";
                        case "MaxUsesNumberOnRenewableSub":
                            return "maxUsesNumberOnRenewableSub";
                        case "Name":
                            return "name";
                        case "StartDate":
                            return "startDate";
                    }
                    break;
                    
                case "KalturaCurrency":
                    switch(property.Name)
                    {
                        case "Code":
                            return "code";
                        case "IsDefault":
                            return "isDefault";
                        case "Name":
                            return "name";
                        case "Sign":
                            return "sign";
                    }
                    break;
                    
                case "KalturaCurrencyFilter":
                    switch(property.Name)
                    {
                        case "CodeIn":
                            return "codeIn";
                    }
                    break;
                    
                case "KalturaCurrencyListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaDeviceBrand":
                    switch(property.Name)
                    {
                        case "DeviceFamilyId":
                            return "deviceFamilyid";
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                    }
                    break;
                    
                case "KalturaDeviceBrandListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaDeviceFamily":
                    switch(property.Name)
                    {
                        case "Devices":
                            return "devices";
                    }
                    break;
                    
                case "KalturaDeviceFamilyBase":
                    switch(property.Name)
                    {
                        case "ConcurrentLimit":
                            return "concurrentLimit";
                        case "DeviceLimit":
                            return "deviceLimit";
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                    }
                    break;
                    
                case "KalturaDeviceFamilyListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaDevicePin":
                    switch(property.Name)
                    {
                        case "Pin":
                            return "pin";
                    }
                    break;
                    
                case "KalturaDeviceRegistrationStatusHolder":
                    switch(property.Name)
                    {
                        case "Status":
                            return "status";
                    }
                    break;
                    
                case "KalturaDeviceReport":
                    switch(property.Name)
                    {
                        case "ConfigurationGroupId":
                            return "configurationGroupId";
                        case "LastAccessDate":
                            return "lastAccessDate";
                        case "LastAccessIP":
                            return "lastAccessIP";
                        case "OperationSystem":
                            return "operationSystem";
                        case "PartnerId":
                            return "partnerId";
                        case "PushParameters":
                            return "pushParameters";
                        case "Udid":
                            return "udid";
                        case "UserAgent":
                            return "userAgent";
                        case "VersionAppName":
                            return "versionAppName";
                        case "VersionNumber":
                            return "versionNumber";
                        case "VersionPlatform":
                            return "versionPlatform";
                    }
                    break;
                    
                case "KalturaDeviceReportFilter":
                    switch(property.Name)
                    {
                        case "LastAccessDateGreaterThanOrEqual":
                            return "lastAccessDateGreaterThanOrEqual";
                    }
                    break;
                    
                case "KalturaDiscountModule":
                    switch(property.Name)
                    {
                        case "EndDate":
                            return "endDate";
                        case "Percent":
                            return "percent";
                        case "StartDate":
                            return "startDate";
                    }
                    break;
                    
                case "KalturaDrmPlaybackPluginData":
                    switch(property.Name)
                    {
                        case "LicenseURL":
                            return "licenseURL";
                        case "Scheme":
                            return "scheme";
                    }
                    break;
                    
                case "KalturaDynamicOrderBy":
                    switch(property.Name)
                    {
                        case "Name":
                            return "name";
                        case "OrderBy":
                            return "orderBy";
                    }
                    break;
                    
                case "KalturaEngagement":
                    switch(property.Name)
                    {
                        case "AdapterDynamicData":
                            return "adapterDynamicData";
                        case "AdapterId":
                            return "adapterId";
                        case "Id":
                            return "id";
                        case "IntervalSeconds":
                            return "intervalSeconds";
                        case "IsActive":
                            return "isActive";
                        case "SendTimeInSeconds":
                            return "sendTimeInSeconds";
                        case "TotalNumberOfRecipients":
                            return "totalNumberOfRecipients";
                        case "Type":
                            return "type";
                        case "UserList":
                            return "userList";
                    }
                    break;
                    
                case "KalturaEngagementAdapter":
                    switch(property.Name)
                    {
                        case "AdapterUrl":
                            return "adapterUrl";
                        case "IsActive":
                            return "isActive";
                        case "ProviderUrl":
                            return "providerUrl";
                        case "Settings":
                            return "engagementAdapterSettings";
                        case "SharedSecret":
                            return "sharedSecret";
                    }
                    break;
                    
                case "KalturaEngagementAdapterBase":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                    }
                    break;
                    
                case "KalturaEngagementAdapterListResponse":
                    switch(property.Name)
                    {
                        case "EngagementAdapters":
                            return "objects";
                    }
                    break;
                    
                case "KalturaEngagementListResponse":
                    switch(property.Name)
                    {
                        case "Engagements":
                            return "objects";
                    }
                    break;
                    
                case "KalturaEntitlement":
                    switch(property.Name)
                    {
                        case "CurrentDate":
                            return "currentDate";
                        case "CurrentUses":
                            return "currentUses";
                        case "DeviceName":
                            return "deviceName";
                        case "DeviceUDID":
                            return "deviceUdid";
                        case "EndDate":
                            return "endDate";
                        case "EntitlementId":
                            return "entitlementId";
                        case "HouseholdId":
                            return "householdId";
                        case "Id":
                            return "id";
                        case "IsCancelationWindowEnabled":
                            return "isCancelationWindowEnabled";
                        case "IsInGracePeriod":
                            return "isInGracePeriod";
                        case "IsRenewable":
                            return "isRenewable";
                        case "IsRenewableForPurchase":
                            return "isRenewableForPurchase";
                        case "LastViewDate":
                            return "lastViewDate";
                        case "MaxUses":
                            return "maxUses";
                        case "MediaFileId":
                            return "mediaFileId";
                        case "MediaId":
                            return "mediaId";
                        case "NextRenewalDate":
                            return "nextRenewalDate";
                        case "PaymentMethod":
                            return "paymentMethod";
                        case "PurchaseDate":
                            return "purchaseDate";
                        case "PurchaseId":
                            return "purchaseId";
                        case "Type":
                            return "type";
                        case "UserId":
                            return "userId";
                    }
                    break;
                    
                case "KalturaEntitlementCancellation":
                    switch(property.Name)
                    {
                        case "HouseholdId":
                            return "householdId";
                        case "Id":
                            return "id";
                        case "ProductId":
                            return "productId";
                        case "Type":
                            return "type";
                        case "UserId":
                            return "userId";
                    }
                    break;
                    
                case "KalturaEntitlementFilter":
                    switch(property.Name)
                    {
                        case "EntitlementTypeEqual":
                            return "entitlementTypeEqual";
                        case "EntityReferenceEqual":
                            return "entityReferenceEqual";
                        case "IsExpiredEqual":
                            return "isExpiredEqual";
                    }
                    break;
                    
                case "KalturaEntitlementListResponse":
                    switch(property.Name)
                    {
                        case "Entitlements":
                            return "objects";
                    }
                    break;
                    
                case "KalturaEntitlementsFilter":
                    switch(property.Name)
                    {
                        case "By":
                            return "by";
                        case "EntitlementType":
                            return "entitlementType";
                    }
                    break;
                    
                case "KalturaEPGChannelAssets":
                    switch(property.Name)
                    {
                        case "Assets":
                            return "objects";
                        case "ChannelID":
                            return "channelId";
                    }
                    break;
                    
                case "KalturaEPGChannelAssetsListResponse":
                    switch(property.Name)
                    {
                        case "Channels":
                            return "objects";
                    }
                    break;
                    
                case "KalturaEpgChannelFilter":
                    switch(property.Name)
                    {
                        case "EndTime":
                            return "endTime";
                        case "IDs":
                            return "ids";
                        case "StartTime":
                            return "startTime";
                    }
                    break;
                    
                case "KalturaExportTask":
                    switch(property.Name)
                    {
                        case "Alias":
                            return "alias";
                        case "DataType":
                            return "dataType";
                        case "ExportType":
                            return "exportType";
                        case "Filter":
                            return "filter";
                        case "Frequency":
                            return "frequency";
                        case "Id":
                            return "id";
                        case "IsActive":
                            return "isActive";
                        case "Name":
                            return "name";
                        case "NotificationUrl":
                            return "notificationUrl";
                        case "VodTypes":
                            return "vodTypes";
                    }
                    break;
                    
                case "KalturaExportTaskFilter":
                    switch(property.Name)
                    {
                        case "IdIn":
                            return "idIn";
                    }
                    break;
                    
                case "KalturaExportTaskListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaExternalChannelProfile":
                    switch(property.Name)
                    {
                        case "Enrichments":
                            return "enrichments";
                        case "ExternalIdentifier":
                            return "externalIdentifier";
                        case "FilterExpression":
                            return "filterExpression";
                        case "Id":
                            return "id";
                        case "IsActive":
                            return "isActive";
                        case "Name":
                            return "name";
                        case "RecommendationEngineId":
                            return "recommendationEngineId";
                    }
                    break;
                    
                case "KalturaExternalChannelProfileListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaExternalReceipt":
                    switch(property.Name)
                    {
                        case "PaymentGatewayName":
                            return "paymentGatewayName";
                        case "ReceiptId":
                            return "receiptId";
                    }
                    break;
                    
                case "KalturaFacebookPost":
                    switch(property.Name)
                    {
                        case "Comments":
                            return "comments";
                        case "Link":
                            return "link";
                    }
                    break;
                    
                case "KalturaFairPlayPlaybackPluginData":
                    switch(property.Name)
                    {
                        case "Certificate":
                            return "certificate";
                    }
                    break;
                    
                case "KalturaFavorite":
                    switch(property.Name)
                    {
                        case "Asset":
                            return "asset";
                        case "AssetId":
                            return "assetId";
                        case "CreateDate":
                            return "createDate";
                        case "ExtraData":
                            return "extraData";
                    }
                    break;
                    
                case "KalturaFavoriteFilter":
                    switch(property.Name)
                    {
                        case "MediaIdIn":
                            return "mediaIdIn";
                        case "MediaIds":
                            return "media_ids";
                        case "MediaTypeEqual":
                            return "mediaTypeEqual";
                        case "MediaTypeIn":
                            return "mediaTypeIn";
                        case "UDID":
                            return "udid";
                    }
                    break;
                    
                case "KalturaFavoriteListResponse":
                    switch(property.Name)
                    {
                        case "Favorites":
                            return "objects";
                    }
                    break;
                    
                case "KalturaFeed":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "assetId";
                    }
                    break;
                    
                case "KalturaFilter`1":
                    switch(property.Name)
                    {
                        case "OrderBy":
                            return "orderBy";
                    }
                    break;
                    
                case "KalturaFilterPager":
                    switch(property.Name)
                    {
                        case "PageIndex":
                            return "pageIndex";
                        case "PageSize":
                            return "pageSize";
                    }
                    break;
                    
                case "KalturaFollowDataBase":
                    switch(property.Name)
                    {
                        case "AnnouncementId":
                            return "announcementId";
                        case "FollowPhrase":
                            return "followPhrase";
                        case "Status":
                            return "status";
                        case "Timestamp":
                            return "timestamp";
                        case "Title":
                            return "title";
                    }
                    break;
                    
                case "KalturaFollowDataTvSeries":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "assetId";
                    }
                    break;
                    
                case "KalturaFollowTvSeries":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "assetId";
                    }
                    break;
                    
                case "KalturaFollowTvSeriesListResponse":
                    switch(property.Name)
                    {
                        case "FollowDataList":
                            return "objects";
                    }
                    break;
                    
                case "KalturaGenericRule":
                    switch(property.Name)
                    {
                        case "Description":
                            return "description";
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "RuleType":
                            return "ruleType";
                    }
                    break;
                    
                case "KalturaGenericRuleFilter":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "assetId";
                        case "AssetType":
                            return "assetType";
                    }
                    break;
                    
                case "KalturaGenericRuleListResponse":
                    switch(property.Name)
                    {
                        case "GenericRules":
                            return "objects";
                    }
                    break;
                    
                case "KalturaGroupPermission":
                    switch(property.Name)
                    {
                        case "Group":
                            return "group";
                    }
                    break;
                    
                case "KalturaHomeNetwork":
                    switch(property.Name)
                    {
                        case "Description":
                            return "description";
                        case "ExternalId":
                            return "externalId";
                        case "IsActive":
                            return "isActive";
                        case "Name":
                            return "name";
                    }
                    break;
                    
                case "KalturaHomeNetworkListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaHousehold":
                    switch(property.Name)
                    {
                        case "ConcurrentLimit":
                            return "concurrentLimit";
                        case "DefaultUsers":
                            return "defaultUsers";
                        case "Description":
                            return "description";
                        case "DeviceFamilies":
                            return "deviceFamilies";
                        case "DevicesLimit":
                            return "devicesLimit";
                        case "ExternalId":
                            return "externalId";
                        case "FrequencyNextDeviceAction":
                            return "frequencyNextDeviceAction";
                        case "FrequencyNextUserAction":
                            return "frequencyNextUserAction";
                        case "HouseholdLimitationsId":
                            return "householdLimitationsId";
                        case "Id":
                            return "id";
                        case "IsFrequencyEnabled":
                            return "isFrequencyEnabled";
                        case "MasterUsers":
                            return "masterUsers";
                        case "Name":
                            return "name";
                        case "PendingUsers":
                            return "pendingUsers";
                        case "RegionId":
                            return "regionId";
                        case "Restriction":
                            return "restriction";
                        case "State":
                            return "state";
                        case "Users":
                            return "users";
                        case "UsersLimit":
                            return "usersLimit";
                    }
                    break;
                    
                case "KalturaHouseholdDevice":
                    switch(property.Name)
                    {
                        case "ActivatedOn":
                            return "activatedOn";
                        case "Brand":
                            return "brand";
                        case "BrandId":
                            return "brandId";
                        case "DeviceFamilyId":
                            return "deviceFamilyId";
                        case "HouseholdId":
                            return "householdId";
                        case "Name":
                            return "name";
                        case "State":
                            return "state";
                        case "Status":
                            return "status";
                        case "Udid":
                            return "udid";
                    }
                    break;
                    
                case "KalturaHouseholdDeviceFamilyLimitations":
                    switch(property.Name)
                    {
                        case "ConcurrentLimit":
                            return "concurrentLimit";
                        case "DeviceLimit":
                            return "deviceLimit";
                        case "Frequency":
                            return "frequency";
                    }
                    break;
                    
                case "KalturaHouseholdDeviceFilter":
                    switch(property.Name)
                    {
                        case "DeviceFamilyIdIn":
                            return "deviceFamilyIdIn";
                        case "HouseholdIdEqual":
                            return "householdIdEqual";
                    }
                    break;
                    
                case "KalturaHouseholdDeviceListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaHouseholdLimitations":
                    switch(property.Name)
                    {
                        case "ConcurrentLimit":
                            return "concurrentLimit";
                        case "DeviceFamiliesLimitations":
                            return "deviceFamiliesLimitations";
                        case "DeviceFrequency":
                            return "deviceFrequency";
                        case "DeviceFrequencyDescription":
                            return "deviceFrequencyDescription";
                        case "DeviceLimit":
                            return "deviceLimit";
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "NpvrQuotaInSeconds":
                            return "npvrQuotaInSeconds";
                        case "UserFrequency":
                            return "userFrequency";
                        case "UserFrequencyDescription":
                            return "userFrequencyDescription";
                        case "UsersLimit":
                            return "usersLimit";
                    }
                    break;
                    
                case "KalturaHouseholdPaymentGateway":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "IsDefault":
                            return "isDefault";
                        case "Name":
                            return "name";
                    }
                    break;
                    
                case "KalturaHouseholdPaymentGatewayListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaHouseholdPaymentMethod":
                    switch(property.Name)
                    {
                        case "AllowMultiInstance":
                            return "allowMultiInstance";
                        case "Details":
                            return "details";
                        case "ExternalId":
                            return "externalId";
                        case "Id":
                            return "id";
                        case "IsDefault":
                            return "isDefault";
                        case "Name":
                            return "name";
                        case "PaymentGatewayId":
                            return "paymentGatewayId";
                        case "PaymentMethodProfileId":
                            return "paymentMethodProfileId";
                        case "Selected":
                            return "selected";
                    }
                    break;
                    
                case "KalturaHouseholdPaymentMethodListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaHouseholdPremiumServiceListResponse":
                    switch(property.Name)
                    {
                        case "PremiumServices":
                            return "objects";
                    }
                    break;
                    
                case "KalturaHouseholdQuota":
                    switch(property.Name)
                    {
                        case "AvailableQuota":
                            return "availableQuota";
                        case "HouseholdId":
                            return "householdId";
                        case "TotalQuota":
                            return "totalQuota";
                    }
                    break;
                    
                case "KalturaHouseholdUser":
                    switch(property.Name)
                    {
                        case "HouseholdId":
                            return "householdId";
                        case "HouseholdMasterUsername":
                            return "householdMasterUsername";
                        case "IsDefault":
                            return "isDefault";
                        case "IsMaster":
                            return "isMaster";
                        case "Status":
                            return "status";
                        case "UserId":
                            return "userId";
                    }
                    break;
                    
                case "KalturaHouseholdUserFilter":
                    switch(property.Name)
                    {
                        case "HouseholdIdEqual":
                            return "householdIdEqual";
                    }
                    break;
                    
                case "KalturaHouseholdUserListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaIdentifierTypeFilter":
                    switch(property.Name)
                    {
                        case "By":
                            return "by";
                        case "Identifier":
                            return "identifier";
                    }
                    break;
                    
                case "KalturaInboxMessage":
                    switch(property.Name)
                    {
                        case "CreatedAt":
                            return "createdAt";
                        case "Id":
                            return "id";
                        case "Message":
                            return "message";
                        case "Status":
                            return "status";
                        case "Type":
                            return "type";
                        case "Url":
                            return "url";
                    }
                    break;
                    
                case "KalturaInboxMessageFilter":
                    switch(property.Name)
                    {
                        case "CreatedAtGreaterThanOrEqual":
                            return "createdAtGreaterThanOrEqual";
                        case "CreatedAtLessThanOrEqual":
                            return "createdAtLessThanOrEqual";
                        case "TypeIn":
                            return "typeIn";
                    }
                    break;
                    
                case "KalturaInboxMessageListResponse":
                    switch(property.Name)
                    {
                        case "InboxMessages":
                            return "objects";
                    }
                    break;
                    
                case "KalturaInboxMessageResponse":
                    switch(property.Name)
                    {
                        case "InboxMessages":
                            return "objects";
                    }
                    break;
                    
                case "KalturaItemPrice":
                    switch(property.Name)
                    {
                        case "FileId":
                            return "fileId";
                        case "PPVPriceDetails":
                            return "ppvPriceDetails";
                    }
                    break;
                    
                case "KalturaItemPriceListResponse":
                    switch(property.Name)
                    {
                        case "ItemPrice":
                            return "objects";
                    }
                    break;
                    
                case "KalturaLanguage":
                    switch(property.Name)
                    {
                        case "Code":
                            return "code";
                        case "Direction":
                            return "direction";
                        case "IsDefault":
                            return "isDefault";
                        case "Name":
                            return "name";
                        case "SystemName":
                            return "systemName";
                    }
                    break;
                    
                case "KalturaLanguageFilter":
                    switch(property.Name)
                    {
                        case "CodeIn":
                            return "codeIn";
                    }
                    break;
                    
                case "KalturaLanguageListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaLastPosition":
                    switch(property.Name)
                    {
                        case "Position":
                            return "position";
                        case "PositionOwner":
                            return "position_owner";
                        case "UserId":
                            return "user_id";
                    }
                    break;
                    
                case "KalturaLastPositionFilter":
                    switch(property.Name)
                    {
                        case "By":
                            return "by";
                        case "Ids":
                            return "ids";
                        case "Type":
                            return "type";
                    }
                    break;
                    
                case "KalturaLastPositionListResponse":
                    switch(property.Name)
                    {
                        case "LastPositions":
                            return "objects";
                    }
                    break;
                    
                case "KalturaLicensedUrl":
                    switch(property.Name)
                    {
                        case "AltUrl":
                            return "altUrl";
                        case "MainUrl":
                            return "mainUrl";
                    }
                    break;
                    
                case "KalturaLicensedUrlBaseRequest":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "assetId";
                    }
                    break;
                    
                case "KalturaLicensedUrlEpgRequest":
                    switch(property.Name)
                    {
                        case "StartDate":
                            return "startDate";
                        case "StreamType":
                            return "streamType";
                    }
                    break;
                    
                case "KalturaLicensedUrlMediaRequest":
                    switch(property.Name)
                    {
                        case "BaseUrl":
                            return "baseUrl";
                        case "ContentId":
                            return "contentId";
                    }
                    break;
                    
                case "KalturaLicensedUrlRecordingRequest":
                    switch(property.Name)
                    {
                        case "FileType":
                            return "fileType";
                    }
                    break;
                    
                case "KalturaListFollowDataTvSeriesResponse":
                    switch(property.Name)
                    {
                        case "FollowDataList":
                            return "objects";
                    }
                    break;
                    
                case "KalturaListResponse":
                    switch(property.Name)
                    {
                        case "TotalCount":
                            return "totalCount";
                    }
                    break;
                    
                case "KalturaLoginResponse":
                    switch(property.Name)
                    {
                        case "LoginSession":
                            return "loginSession";
                        case "User":
                            return "user";
                    }
                    break;
                    
                case "KalturaLoginSession":
                    switch(property.Name)
                    {
                        case "KS":
                            return "ks";
                        case "RefreshToken":
                            return "refreshToken";
                    }
                    break;
                    
                case "KalturaMediaAsset":
                    switch(property.Name)
                    {
                        case "CatchUpBuffer":
                            return "catchUpBuffer";
                        case "DeviceRule":
                            return "deviceRule";
                        case "EnableRecordingPlaybackNonEntitledChannel":
                            return "enableRecordingPlaybackNonEntitledChannel";
                        case "EntryId":
                            return "entryId";
                        case "ExternalIds":
                            return "externalIds";
                        case "GeoBlockRule":
                            return "geoBlockRule";
                        case "TrickPlayBuffer":
                            return "trickPlayBuffer";
                        case "TypeDescription":
                            return "typeDescription";
                        case "WatchPermissionRule":
                            return "watchPermissionRule";
                    }
                    break;
                    
                case "KalturaMediaFile":
                    switch(property.Name)
                    {
                        case "AltCdnCode":
                            return "altCdnCode";
                        case "AssetId":
                            return "assetId";
                        case "BillingType":
                            return "billingType";
                        case "CdnCode":
                            return "cdnCode";
                        case "CdnName":
                            return "cdnName";
                        case "Duration":
                            return "duration";
                        case "ExternalId":
                            return "externalId";
                        case "HandlingType":
                            return "handlingType";
                        case "Id":
                            return "id";
                        case "PPVModules":
                            return "ppvModules";
                        case "ProductCode":
                            return "productCode";
                        case "Quality":
                            return "quality";
                        case "Type":
                            return "type";
                        case "Url":
                            return "url";
                    }
                    break;
                    
                case "KalturaMediaImage":
                    switch(property.Name)
                    {
                        case "Height":
                            return "height";
                        case "Id":
                            return "id";
                        case "IsDefault":
                            return "isDefault";
                        case "Ratio":
                            return "ratio";
                        case "Url":
                            return "url";
                        case "Version":
                            return "version";
                        case "Width":
                            return "width";
                    }
                    break;
                    
                case "KalturaMessageAnnouncementListResponse":
                    switch(property.Name)
                    {
                        case "Announcements":
                            return "objects";
                    }
                    break;
                    
                case "KalturaMessageTemplate":
                    switch(property.Name)
                    {
                        case "Action":
                            return "action";
                        case "AssetType":
                            return "assetType";
                        case "DateFormat":
                            return "dateFormat";
                        case "Message":
                            return "message";
                        case "Sound":
                            return "sound";
                        case "URL":
                            return "url";
                    }
                    break;
                    
                case "KalturaMeta":
                    switch(property.Name)
                    {
                        case "AssetType":
                            return "assetType";
                        case "FieldName":
                            return "fieldName";
                        case "Name":
                            return "name";
                        case "Type":
                            return "type";
                    }
                    break;
                    
                case "KalturaMetaFilter":
                    switch(property.Name)
                    {
                        case "AssetTypeEqual":
                            return "assetTypeEqual";
                        case "FieldNameEqual":
                            return "fieldNameEqual";
                        case "FieldNameNotEqual":
                            return "fieldNameNotEqual";
                        case "TypeEqual":
                            return "typeEqual";
                    }
                    break;
                    
                case "KalturaMetaListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaMultilingualString":
                    switch(property.Name)
                    {
                        case "Values":
                            return "values";
                    }
                    break;
                    
                case "KalturaMultilingualStringValueArray":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaNetworkActionStatus":
                    switch(property.Name)
                    {
                        case "Network":
                            return "network";
                        case "Status":
                            return "status";
                    }
                    break;
                    
                case "KalturaNotification":
                    switch(property.Name)
                    {
                        case "eventObject":
                            return "object";
                    }
                    break;
                    
                case "KalturaNotificationsPartnerSettings":
                    switch(property.Name)
                    {
                        case "AutomaticIssueFollowNotification":
                            return "automaticIssueFollowNotification";
                        case "ChurnMailSubject":
                            return "churnMailSubject";
                        case "ChurnMailTemplateName":
                            return "churnMailTemplateName";
                        case "InboxEnabled":
                            return "inboxEnabled";
                        case "MailSenderName":
                            return "mailSenderName";
                        case "MessageTTLDays":
                            return "messageTTLDays";
                        case "PushAdapterUrl":
                            return "pushAdapterUrl";
                        case "PushEndHour":
                            return "pushEndHour";
                        case "PushNotificationEnabled":
                            return "pushNotificationEnabled";
                        case "PushStartHour":
                            return "pushStartHour";
                        case "PushSystemAnnouncementsEnabled":
                            return "pushSystemAnnouncementsEnabled";
                        case "ReminderEnabled":
                            return "reminderEnabled";
                        case "ReminderOffset":
                            return "reminderOffsetSec";
                        case "SenderEmail":
                            return "senderEmail";
                        case "TopicExpirationDurationDays":
                            return "topicExpirationDurationDays";
                    }
                    break;
                    
                case "KalturaNotificationsSettings":
                    switch(property.Name)
                    {
                        case "PushFollowEnabled":
                            return "pushFollowEnabled";
                        case "PushNotificationEnabled":
                            return "pushNotificationEnabled";
                    }
                    break;
                    
                case "KalturaNpvrPremiumService":
                    switch(property.Name)
                    {
                        case "QuotaInMinutes":
                            return "quotaInMinutes";
                    }
                    break;
                    
                case "KalturaOSSAdapterBaseProfile":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                    }
                    break;
                    
                case "KalturaOSSAdapterProfile":
                    switch(property.Name)
                    {
                        case "AdapterUrl":
                            return "adapterUrl";
                        case "ExternalIdentifier":
                            return "externalIdentifier";
                        case "IsActive":
                            return "isActive";
                        case "Settings":
                            return "ossAdapterSettings";
                        case "SharedSecret":
                            return "sharedSecret";
                    }
                    break;
                    
                case "KalturaOSSAdapterProfileListResponse":
                    switch(property.Name)
                    {
                        case "OSSAdapterProfiles":
                            return "objects";
                    }
                    break;
                    
                case "KalturaOTTCategory":
                    switch(property.Name)
                    {
                        case "Channels":
                            return "channels";
                        case "ChildCategories":
                            return "childCategories";
                        case "Id":
                            return "id";
                        case "Images":
                            return "images";
                        case "Name":
                            return "name";
                        case "ParentCategoryId":
                            return "parentCategoryId";
                    }
                    break;
                    
                case "KalturaOTTUser":
                    switch(property.Name)
                    {
                        case "Address":
                            return "address";
                        case "AffiliateCode":
                            return "affiliateCode";
                        case "City":
                            return "city";
                        case "Country":
                            return "country";
                        case "CountryId":
                            return "countryId";
                        case "DynamicData":
                            return "dynamicData";
                        case "Email":
                            return "email";
                        case "ExternalId":
                            return "externalId";
                        case "FacebookId":
                            return "facebookId";
                        case "FacebookImage":
                            return "facebookImage";
                        case "FacebookToken":
                            return "facebookToken";
                        case "HouseholdID":
                            return "householdId";
                        case "IsHouseholdMaster":
                            return "isHouseholdMaster";
                        case "Phone":
                            return "phone";
                        case "SuspensionState":
                            return "suspensionState";
                        case "SuspentionState":
                            return "suspentionState";
                        case "UserState":
                            return "userState";
                        case "UserType":
                            return "userType";
                        case "Zip":
                            return "zip";
                    }
                    break;
                    
                case "KalturaOTTUserFilter":
                    switch(property.Name)
                    {
                        case "ExternalIdEqual":
                            return "externalIdEqual";
                        case "IdIn":
                            return "idIn";
                        case "UsernameEqual":
                            return "usernameEqual";
                    }
                    break;
                    
                case "KalturaOTTUserListResponse":
                    switch(property.Name)
                    {
                        case "Users":
                            return "objects";
                    }
                    break;
                    
                case "KalturaOTTUserType":
                    switch(property.Name)
                    {
                        case "Description":
                            return "description";
                        case "Id":
                            return "id";
                    }
                    break;
                    
                case "KalturaParentalRule":
                    switch(property.Name)
                    {
                        case "epgTagTypeId":
                            return "epgTag";
                        case "mediaTagTypeId":
                            return "mediaTag";
                        case "Origin":
                            return "origin";
                    }
                    break;
                    
                case "KalturaParentalRuleFilter":
                    switch(property.Name)
                    {
                        case "EntityReferenceEqual":
                            return "entityReferenceEqual";
                    }
                    break;
                    
                case "KalturaParentalRuleListResponse":
                    switch(property.Name)
                    {
                        case "ParentalRule":
                            return "objects";
                    }
                    break;
                    
                case "KalturaPaymentGateway":
                    switch(property.Name)
                    {
                        case "paymentGateway":
                            return "payment_gateway";
                        case "selectedBy":
                            return "selected_by";
                    }
                    break;
                    
                case "KalturaPaymentGatewayBaseProfile":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "IsDefault":
                            return "isDefault";
                        case "Name":
                            return "name";
                        case "PaymentMethods":
                            return "paymentMethods";
                    }
                    break;
                    
                case "KalturaPaymentGatewayConfiguration":
                    switch(property.Name)
                    {
                        case "Configuration":
                            return "paymentGatewayConfiguration";
                    }
                    break;
                    
                case "KalturaPaymentGatewayProfile":
                    switch(property.Name)
                    {
                        case "AdapterUrl":
                            return "adapterUrl";
                        case "ExternalIdentifier":
                            return "externalIdentifier";
                        case "IsActive":
                            return "isActive";
                        case "PendingInterval":
                            return "pendingInterval";
                        case "PendingRetries":
                            return "pendingRetries";
                        case "RenewIntervalMinutes":
                            return "renewIntervalMinutes";
                        case "RenewStartMinutes":
                            return "renewStartMinutes";
                        case "RenewUrl":
                            return "renewUrl";
                        case "Settings":
                            return "paymentGatewaySettings";
                        case "SharedSecret":
                            return "sharedSecret";
                        case "StatusUrl":
                            return "statusUrl";
                        case "TransactUrl":
                            return "transactUrl";
                    }
                    break;
                    
                case "KalturaPaymentGatewayProfileListResponse":
                    switch(property.Name)
                    {
                        case "PaymentGatewayProfiles":
                            return "objects";
                    }
                    break;
                    
                case "KalturaPaymentMethod":
                    switch(property.Name)
                    {
                        case "AllowMultiInstance":
                            return "allowMultiInstance";
                        case "HouseholdPaymentMethods":
                            return "householdPaymentMethods";
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                    }
                    break;
                    
                case "KalturaPaymentMethodProfile":
                    switch(property.Name)
                    {
                        case "AllowMultiInstance":
                            return "allowMultiInstance";
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "PaymentGatewayId":
                            return "paymentGatewayId";
                    }
                    break;
                    
                case "KalturaPaymentMethodProfileFilter":
                    switch(property.Name)
                    {
                        case "PaymentGatewayIdEqual":
                            return "paymentGatewayIdEqual";
                    }
                    break;
                    
                case "KalturaPaymentMethodProfileListResponse":
                    switch(property.Name)
                    {
                        case "PaymentMethodProfiles":
                            return "objects";
                    }
                    break;
                    
                case "KalturaPermission":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "PermissionItems":
                            return "permissionItems";
                    }
                    break;
                    
                case "KalturaPermissionItem":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                    }
                    break;
                    
                case "KalturaPermissionsFilter":
                    switch(property.Name)
                    {
                        case "Ids":
                            return "ids";
                    }
                    break;
                    
                case "KalturaPersistedFilter`1":
                    switch(property.Name)
                    {
                        case "Name":
                            return "name";
                    }
                    break;
                    
                case "KalturaPersonalAsset":
                    switch(property.Name)
                    {
                        case "Bookmarks":
                            return "bookmarks";
                        case "Files":
                            return "files";
                        case "Following":
                            return "following";
                        case "Id":
                            return "id";
                        case "Type":
                            return "type";
                    }
                    break;
                    
                case "KalturaPersonalAssetListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaPersonalAssetRequest":
                    switch(property.Name)
                    {
                        case "FileIds":
                            return "fileIds";
                        case "Id":
                            return "id";
                        case "Type":
                            return "type";
                    }
                    break;
                    
                case "KalturaPersonalFeedListResponse":
                    switch(property.Name)
                    {
                        case "PersonalFollowFeed":
                            return "objects";
                    }
                    break;
                    
                case "KalturaPersonalFile":
                    switch(property.Name)
                    {
                        case "Discounted":
                            return "discounted";
                        case "Entitled":
                            return "entitled";
                        case "Id":
                            return "id";
                        case "Offer":
                            return "offer";
                    }
                    break;
                    
                case "KalturaPersonalFollowFeedResponse":
                    switch(property.Name)
                    {
                        case "PersonalFollowFeed":
                            return "objects";
                    }
                    break;
                    
                case "KalturaPin":
                    switch(property.Name)
                    {
                        case "Origin":
                            return "origin";
                        case "PIN":
                            return "pin";
                        case "Type":
                            return "type";
                    }
                    break;
                    
                case "KalturaPinResponse":
                    switch(property.Name)
                    {
                        case "Origin":
                            return "origin";
                        case "PIN":
                            return "pin";
                        case "Type":
                            return "type";
                    }
                    break;
                    
                case "KalturaPlaybackContext":
                    switch(property.Name)
                    {
                        case "Actions":
                            return "actions";
                        case "Messages":
                            return "messages";
                        case "Sources":
                            return "sources";
                    }
                    break;
                    
                case "KalturaPlaybackContextOptions":
                    switch(property.Name)
                    {
                        case "AssetFileIds":
                            return "assetFileIds";
                        case "Context":
                            return "context";
                        case "MediaProtocol":
                            return "mediaProtocol";
                        case "StreamerType":
                            return "streamerType";
                    }
                    break;
                    
                case "KalturaPlaybackSource":
                    switch(property.Name)
                    {
                        case "AdsParams":
                            return "adsParam";
                        case "AdsPolicy":
                            return "adsPolicy";
                        case "Drm":
                            return "drm";
                        case "Format":
                            return "format";
                        case "Protocols":
                            return "protocols";
                    }
                    break;
                    
                case "KalturaPlayerAssetData":
                    switch(property.Name)
                    {
                        case "averageBitRate":
                            return "averageBitrate";
                        case "currentBitRate":
                            return "currentBitrate";
                        case "totalBitRate":
                            return "totalBitrate";
                    }
                    break;
                    
                case "KalturaPpv":
                    switch(property.Name)
                    {
                        case "CouponsGroup":
                            return "couponsGroup";
                        case "Descriptions":
                            return "descriptions";
                        case "DiscountModule":
                            return "discountModule";
                        case "FileTypes":
                            return "fileTypes";
                        case "FirstDeviceLimitation":
                            return "firstDeviceLimitation";
                        case "Id":
                            return "id";
                        case "IsSubscriptionOnly":
                            return "isSubscriptionOnly";
                        case "Name":
                            return "name";
                        case "Price":
                            return "price";
                        case "ProductCode":
                            return "productCode";
                        case "UsageModule":
                            return "usageModule";
                    }
                    break;
                    
                case "KalturaPpvEntitlement":
                    switch(property.Name)
                    {
                        case "MediaFileId":
                            return "mediaFileId";
                        case "MediaId":
                            return "mediaId";
                    }
                    break;
                    
                case "KalturaPPVItemPriceDetails":
                    switch(property.Name)
                    {
                        case "CollectionId":
                            return "collectionId";
                        case "DiscountEndDate":
                            return "discountEndDate";
                        case "EndDate":
                            return "endDate";
                        case "FirstDeviceName":
                            return "firstDeviceName";
                        case "FullPrice":
                            return "fullPrice";
                        case "IsInCancelationPeriod":
                            return "isInCancelationPeriod";
                        case "IsSubscriptionOnly":
                            return "isSubscriptionOnly";
                        case "PPVDescriptions":
                            return "ppvDescriptions";
                        case "PPVModuleId":
                            return "ppvModuleId";
                        case "PrePaidId":
                            return "prePaidId";
                        case "Price":
                            return "price";
                        case "ProductCode":
                            return "ppvProductCode";
                        case "PurchasedMediaFileId":
                            return "purchasedMediaFileId";
                        case "PurchaseStatus":
                            return "purchaseStatus";
                        case "PurchaseUserId":
                            return "purchaseUserId";
                        case "RelatedMediaFileIds":
                            return "relatedMediaFileIds";
                        case "StartDate":
                            return "startDate";
                        case "SubscriptionId":
                            return "subscriptionId";
                    }
                    break;
                    
                case "KalturaPpvPrice":
                    switch(property.Name)
                    {
                        case "CollectionId":
                            return "collectionId";
                        case "DiscountEndDate":
                            return "discountEndDate";
                        case "EndDate":
                            return "endDate";
                        case "FileId":
                            return "fileId";
                        case "FirstDeviceName":
                            return "firstDeviceName";
                        case "FullPrice":
                            return "fullPrice";
                        case "IsInCancelationPeriod":
                            return "isInCancelationPeriod";
                        case "IsSubscriptionOnly":
                            return "isSubscriptionOnly";
                        case "PPVDescriptions":
                            return "ppvDescriptions";
                        case "PPVModuleId":
                            return "ppvModuleId";
                        case "PrePaidId":
                            return "prePaidId";
                        case "ProductCode":
                            return "ppvProductCode";
                        case "PurchasedMediaFileId":
                            return "purchasedMediaFileId";
                        case "PurchaseUserId":
                            return "purchaseUserId";
                        case "RelatedMediaFileIds":
                            return "relatedMediaFileIds";
                        case "StartDate":
                            return "startDate";
                        case "SubscriptionId":
                            return "subscriptionId";
                    }
                    break;
                    
                case "KalturaPremiumService":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                    }
                    break;
                    
                case "KalturaPreviewModule":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "LifeCycle":
                            return "lifeCycle";
                        case "Name":
                            return "name";
                        case "NonRenewablePeriod":
                            return "nonRenewablePeriod";
                    }
                    break;
                    
                case "KalturaPrice":
                    switch(property.Name)
                    {
                        case "Amount":
                            return "amount";
                        case "Currency":
                            return "currency";
                        case "CurrencySign":
                            return "currencySign";
                    }
                    break;
                    
                case "KalturaPriceDetails":
                    switch(property.Name)
                    {
                        case "Descriptions":
                            return "descriptions";
                        case "Id":
                            return "id";
                        case "Price":
                            return "price";
                    }
                    break;
                    
                case "KalturaPricePlan":
                    switch(property.Name)
                    {
                        case "DiscountId":
                            return "discountId";
                        case "IsRenewable":
                            return "isRenewable";
                        case "PriceId":
                            return "priceId";
                        case "RenewalsNumber":
                            return "renewalsNumber";
                    }
                    break;
                    
                case "KalturaPricesFilter":
                    switch(property.Name)
                    {
                        case "FilesIds":
                            return "filesIds";
                        case "ShouldGetOnlyLowest":
                            return "shouldGetOnlyLowest";
                        case "SubscriptionsIds":
                            return "subscriptionsIds";
                    }
                    break;
                    
                case "KalturaProductPrice":
                    switch(property.Name)
                    {
                        case "Price":
                            return "price";
                        case "ProductId":
                            return "productId";
                        case "ProductType":
                            return "productType";
                        case "PurchaseStatus":
                            return "purchaseStatus";
                    }
                    break;
                    
                case "KalturaProductPriceFilter":
                    switch(property.Name)
                    {
                        case "CouponCodeEqual":
                            return "couponCodeEqual";
                        case "FileIdIn":
                            return "fileIdIn";
                        case "SubscriptionIdIn":
                            return "subscriptionIdIn";
                    }
                    break;
                    
                case "KalturaProductPriceListResponse":
                    switch(property.Name)
                    {
                        case "ProductsPrices":
                            return "objects";
                    }
                    break;
                    
                case "KalturaProductsPriceListResponse":
                    switch(property.Name)
                    {
                        case "ProductsPrices":
                            return "objects";
                    }
                    break;
                    
                case "KalturaProgramAsset":
                    switch(property.Name)
                    {
                        case "Crid":
                            return "crid";
                        case "EpgChannelId":
                            return "epgChannelId";
                        case "EpgId":
                            return "epgId";
                        case "LinearAssetId":
                            return "linearAssetId";
                        case "RelatedMediaId":
                            return "relatedMediaId";
                    }
                    break;
                    
                case "KalturaPurchase":
                    switch(property.Name)
                    {
                        case "AdapterData":
                            return "adapterData";
                        case "Coupon":
                            return "coupon";
                        case "Currency":
                            return "currency";
                        case "PaymentGatewayId":
                            return "paymentGatewayId";
                        case "PaymentMethodId":
                            return "paymentMethodId";
                        case "Price":
                            return "price";
                    }
                    break;
                    
                case "KalturaPurchaseBase":
                    switch(property.Name)
                    {
                        case "ContentId":
                            return "contentId";
                        case "ProductId":
                            return "productId";
                        case "ProductType":
                            return "productType";
                    }
                    break;
                    
                case "KalturaPurchaseSession":
                    switch(property.Name)
                    {
                        case "PreviewModuleId":
                            return "previewModuleId";
                    }
                    break;
                    
                case "KalturaPurchaseSettings":
                    switch(property.Name)
                    {
                        case "Permission":
                            return "permission";
                    }
                    break;
                    
                case "KalturaPurchaseSettingsResponse":
                    switch(property.Name)
                    {
                        case "PurchaseSettingsType":
                            return "purchaseSettingsType";
                    }
                    break;
                    
                case "KalturaPushParams":
                    switch(property.Name)
                    {
                        case "ExternalToken":
                            return "externalToken";
                        case "Token":
                            return "token";
                    }
                    break;
                    
                case "KalturaRecommendationProfile":
                    switch(property.Name)
                    {
                        case "AdapterUrl":
                            return "adapterUrl";
                        case "ExternalIdentifier":
                            return "externalIdentifier";
                        case "Id":
                            return "id";
                        case "IsActive":
                            return "isActive";
                        case "Name":
                            return "name";
                        case "Settings":
                            return "recommendationEngineSettings";
                        case "SharedSecret":
                            return "sharedSecret";
                    }
                    break;
                    
                case "KalturaRecommendationProfileListResponse":
                    switch(property.Name)
                    {
                        case "RecommendationProfiles":
                            return "objects";
                    }
                    break;
                    
                case "KalturaRecording":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "assetId";
                        case "CreateDate":
                            return "createDate";
                        case "Id":
                            return "id";
                        case "IsProtected":
                            return "isProtected";
                        case "Status":
                            return "status";
                        case "Type":
                            return "type";
                        case "UpdateDate":
                            return "updateDate";
                        case "ViewableUntilDate":
                            return "viewableUntilDate";
                    }
                    break;
                    
                case "KalturaRecordingAsset":
                    switch(property.Name)
                    {
                        case "RecordingId":
                            return "recordingId";
                    }
                    break;
                    
                case "KalturaRecordingContext":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "assetId";
                        case "Code":
                            return "code";
                        case "Message":
                            return "message";
                        case "Recording":
                            return "recording";
                    }
                    break;
                    
                case "KalturaRecordingContextFilter":
                    switch(property.Name)
                    {
                        case "AssetIdIn":
                            return "assetIdIn";
                    }
                    break;
                    
                case "KalturaRecordingContextListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaRecordingFilter":
                    switch(property.Name)
                    {
                        case "FilterExpression":
                            return "filterExpression";
                        case "StatusIn":
                            return "statusIn";
                    }
                    break;
                    
                case "KalturaRecordingListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaRegion":
                    switch(property.Name)
                    {
                        case "ExternalId":
                            return "externalId";
                        case "Id":
                            return "id";
                        case "IsDefault":
                            return "isDefault";
                        case "Name":
                            return "name";
                        case "RegionalChannels":
                            return "linearChannels";
                    }
                    break;
                    
                case "KalturaRegionalChannel":
                    switch(property.Name)
                    {
                        case "ChannelNumber":
                            return "channelNumber";
                        case "LinearChannelId":
                            return "linearChannelId";
                    }
                    break;
                    
                case "KalturaRegionFilter":
                    switch(property.Name)
                    {
                        case "ExternalIdIn":
                            return "externalIdIn";
                    }
                    break;
                    
                case "KalturaRegionListResponse":
                    switch(property.Name)
                    {
                        case "Regions":
                            return "objects";
                    }
                    break;
                    
                case "KalturaRegistryResponse":
                    switch(property.Name)
                    {
                        case "AnnouncementId":
                            return "announcementId";
                        case "Key":
                            return "key";
                        case "Url":
                            return "url";
                    }
                    break;
                    
                case "KalturaRegistrySettings":
                    switch(property.Name)
                    {
                        case "Key":
                            return "key";
                        case "Value":
                            return "value";
                    }
                    break;
                    
                case "KalturaRegistrySettingsListResponse":
                    switch(property.Name)
                    {
                        case "RegistrySettings":
                            return "objects";
                    }
                    break;
                    
                case "KalturaRelatedExternalFilter":
                    switch(property.Name)
                    {
                        case "FreeText":
                            return "freeText";
                        case "IdEqual":
                            return "idEqual";
                        case "TypeIn":
                            return "typeIn";
                        case "UtcOffsetEqual":
                            return "utcOffsetEqual";
                    }
                    break;
                    
                case "KalturaRelatedFilter":
                    switch(property.Name)
                    {
                        case "IdEqual":
                            return "idEqual";
                        case "KSql":
                            return "kSql";
                        case "TypeIn":
                            return "typeIn";
                    }
                    break;
                    
                case "KalturaReminder":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                    }
                    break;
                    
                case "KalturaReminderFilter":
                    switch(property.Name)
                    {
                        case "KSql":
                            return "kSql";
                    }
                    break;
                    
                case "KalturaReminderListResponse":
                    switch(property.Name)
                    {
                        case "Reminders":
                            return "objects";
                    }
                    break;
                    
                case "KalturaReportListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaRequestConfiguration":
                    switch(property.Name)
                    {
                        case "KS":
                            return "ks";
                        case "Language":
                            return "language";
                        case "PartnerID":
                            return "partnerId";
                        case "UserID":
                            return "userId";
                    }
                    break;
                    
                case "KalturaRuleAction":
                    switch(property.Name)
                    {
                        case "Type":
                            return "type";
                    }
                    break;
                    
                case "KalturaRuleFilter":
                    switch(property.Name)
                    {
                        case "By":
                            return "by";
                    }
                    break;
                    
                case "KalturaScheduledRecordingProgramFilter":
                    switch(property.Name)
                    {
                        case "ChannelsIn":
                            return "channelsIn";
                        case "EndDateLessThanOrNull":
                            return "endDateLessThanOrNull";
                        case "RecordingTypeEqual":
                            return "recordingTypeEqual";
                        case "StartDateGreaterThanOrNull":
                            return "startDateGreaterThanOrNull";
                    }
                    break;
                    
                case "KalturaSearchAssetFilter":
                    switch(property.Name)
                    {
                        case "IdIn":
                            return "idIn";
                        case "KSql":
                            return "kSql";
                        case "TypeIn":
                            return "typeIn";
                    }
                    break;
                    
                case "KalturaSearchExternalFilter":
                    switch(property.Name)
                    {
                        case "Query":
                            return "query";
                        case "TypeIn":
                            return "typeIn";
                        case "UtcOffsetEqual":
                            return "utcOffsetEqual";
                    }
                    break;
                    
                case "KalturaSearchHistory":
                    switch(property.Name)
                    {
                        case "Action":
                            return "action";
                        case "CreatedAt":
                            return "createdAt";
                        case "DeviceId":
                            return "deviceId";
                        case "Filter":
                            return "filter";
                        case "Id":
                            return "id";
                        case "Language":
                            return "language";
                        case "Name":
                            return "name";
                        case "Service":
                            return "service";
                    }
                    break;
                    
                case "KalturaSearchHistoryListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaSeriesRecording":
                    switch(property.Name)
                    {
                        case "ChannelId":
                            return "channelId";
                        case "CreateDate":
                            return "createDate";
                        case "EpgId":
                            return "epgId";
                        case "ExcludedSeasons":
                            return "excludedSeasons";
                        case "Id":
                            return "id";
                        case "SeasonNumber":
                            return "seasonNumber";
                        case "SeriesId":
                            return "seriesId";
                        case "Type":
                            return "type";
                        case "UpdateDate":
                            return "updateDate";
                    }
                    break;
                    
                case "KalturaSeriesRecordingListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaSlimAsset":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Type":
                            return "type";
                    }
                    break;
                    
                case "KalturaSlimAssetInfoWrapper":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaSocial":
                    switch(property.Name)
                    {
                        case "Birthday":
                            return "birthday";
                        case "Email":
                            return "email";
                        case "FirstName":
                            return "firstName";
                        case "Gender":
                            return "gender";
                        case "ID":
                            return "id";
                        case "LastName":
                            return "lastName";
                        case "Name":
                            return "name";
                        case "PictureUrl":
                            return "pictureUrl";
                        case "Status":
                            return "status";
                        case "UserId":
                            return "userId";
                    }
                    break;
                    
                case "KalturaSocialAction":
                    switch(property.Name)
                    {
                        case "ActionType":
                            return "actionType";
                        case "AssetId":
                            return "assetId";
                        case "AssetType":
                            return "assetType";
                        case "Id":
                            return "id";
                        case "Time":
                            return "time";
                        case "Url":
                            return "url";
                    }
                    break;
                    
                case "KalturaSocialActionFilter":
                    switch(property.Name)
                    {
                        case "ActionTypeIn":
                            return "actionTypeIn";
                        case "AssetIdIn":
                            return "assetIdIn";
                        case "AssetTypeEqual":
                            return "assetTypeEqual";
                    }
                    break;
                    
                case "KalturaSocialActionListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaSocialActionRate":
                    switch(property.Name)
                    {
                        case "Rate":
                            return "rate";
                    }
                    break;
                    
                case "KalturaSocialComment":
                    switch(property.Name)
                    {
                        case "CreateDate":
                            return "createDate";
                        case "Header":
                            return "header";
                        case "Text":
                            return "text";
                        case "Writer":
                            return "writer";
                    }
                    break;
                    
                case "KalturaSocialCommentFilter":
                    switch(property.Name)
                    {
                        case "AssetIdEqual":
                            return "assetIdEqual";
                        case "AssetTypeEqual":
                            return "assetTypeEqual";
                        case "CreateDateGreaterThan":
                            return "createDateGreaterThan";
                        case "SocialPlatformEqual":
                            return "socialPlatformEqual";
                    }
                    break;
                    
                case "KalturaSocialCommentListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaSocialFacebookConfig":
                    switch(property.Name)
                    {
                        case "AppId":
                            return "appId";
                        case "Permissions":
                            return "permissions";
                    }
                    break;
                    
                case "KalturaSocialFriendActivity":
                    switch(property.Name)
                    {
                        case "SocialAction":
                            return "socialAction";
                        case "UserFullName":
                            return "userFullName";
                        case "UserPictureUrl":
                            return "userPictureUrl";
                    }
                    break;
                    
                case "KalturaSocialFriendActivityFilter":
                    switch(property.Name)
                    {
                        case "ActionTypeIn":
                            return "actionTypeIn";
                        case "AssetIdEqual":
                            return "assetIdEqual";
                        case "AssetTypeEqual":
                            return "assetTypeEqual";
                    }
                    break;
                    
                case "KalturaSocialFriendActivityListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaSocialNetworkComment":
                    switch(property.Name)
                    {
                        case "AuthorImageUrl":
                            return "authorImageUrl";
                        case "LikeCounter":
                            return "likeCounter";
                    }
                    break;
                    
                case "KalturaSocialResponse":
                    switch(property.Name)
                    {
                        case "Data":
                            return "data";
                        case "KalturaName":
                            return "kalturaUsername";
                        case "MinFriends":
                            return "minFriendsLimitation";
                        case "Pic":
                            return "pic";
                        case "SocialNetworkUsername":
                            return "socialUsername";
                        case "SocialUser":
                            return "socialUser";
                        case "Status":
                            return "status";
                        case "Token":
                            return "token";
                        case "UserId":
                            return "userId";
                    }
                    break;
                    
                case "KalturaSocialUser":
                    switch(property.Name)
                    {
                        case "Birthday":
                            return "birthday";
                        case "Email":
                            return "email";
                        case "FirstName":
                            return "firstName";
                        case "Gender":
                            return "gender";
                        case "ID":
                            return "id";
                        case "LastName":
                            return "lastName";
                        case "Name":
                            return "name";
                        case "UserId":
                            return "userId";
                    }
                    break;
                    
                case "KalturaSocialUserConfig":
                    switch(property.Name)
                    {
                        case "PermissionItems":
                            return "actionPermissionItems";
                    }
                    break;
                    
                case "KalturaStringValueArray":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaSubscription":
                    switch(property.Name)
                    {
                        case "Channels":
                            return "channels";
                        case "CouponsGroup":
                            return "couponsGroup";
                        case "Description":
                            return "description";
                        case "Descriptions":
                            return "descriptions";
                        case "DiscountModule":
                            return "discountModule";
                        case "EndDate":
                            return "endDate";
                        case "FileTypes":
                            return "fileTypes";
                        case "GracePeriodMinutes":
                            return "gracePeriodMinutes";
                        case "HouseholdLimitationsId":
                            return "householdLimitationsId";
                        case "Id":
                            return "id";
                        case "IsInfiniteRenewal":
                            return "isInfiniteRenewal";
                        case "IsRenewable":
                            return "isRenewable";
                        case "IsWaiverEnabled":
                            return "isWaiverEnabled";
                        case "MaxViewsNumber":
                            return "maxViewsNumber";
                        case "MediaId":
                            return "mediaId";
                        case "Name":
                            return "name";
                        case "Names":
                            return "names";
                        case "PremiumServices":
                            return "premiumServices";
                        case "PreviewModule":
                            return "previewModule";
                        case "Price":
                            return "price";
                        case "PricePlans":
                            return "pricePlans";
                        case "ProductCode":
                            return "productCode";
                        case "ProrityInOrder":
                            return "prorityInOrder";
                        case "RenewalsNumber":
                            return "renewalsNumber";
                        case "StartDate":
                            return "startDate";
                        case "UserTypes":
                            return "userTypes";
                        case "ViewLifeCycle":
                            return "viewLifeCycle";
                        case "WaiverPeriod":
                            return "waiverPeriod";
                    }
                    break;
                    
                case "KalturaSubscriptionEntitlement":
                    switch(property.Name)
                    {
                        case "IsInGracePeriod":
                            return "isInGracePeriod";
                        case "IsRenewable":
                            return "isRenewable";
                        case "IsRenewableForPurchase":
                            return "isRenewableForPurchase";
                        case "NextRenewalDate":
                            return "nextRenewalDate";
                        case "PaymentGatewayId":
                            return "paymentGatewayId";
                        case "PaymentMethodId":
                            return "paymentMethodId";
                    }
                    break;
                    
                case "KalturaSubscriptionFilter":
                    switch(property.Name)
                    {
                        case "MediaFileIdEqual":
                            return "mediaFileIdEqual";
                        case "SubscriptionIdIn":
                            return "subscriptionIdIn";
                    }
                    break;
                    
                case "KalturaSubscriptionListResponse":
                    switch(property.Name)
                    {
                        case "Subscriptions":
                            return "objects";
                    }
                    break;
                    
                case "KalturaSubscriptionPrice":
                    switch(property.Name)
                    {
                        case "Price":
                            return "price";
                        case "PurchaseStatus":
                            return "purchaseStatus";
                    }
                    break;
                    
                case "KalturaSubscriptionsFilter":
                    switch(property.Name)
                    {
                        case "By":
                            return "by";
                        case "Ids":
                            return "ids";
                    }
                    break;
                    
                case "KalturaTimeShiftedTvPartnerSettings":
                    switch(property.Name)
                    {
                        case "CatchUpBufferLength":
                            return "catchUpBufferLength";
                        case "CatchUpEnabled":
                            return "catchUpEnabled";
                        case "CdvrEnabled":
                            return "cdvrEnabled";
                        case "CleanupNoticePeriod":
                            return "cleanupNoticePeriod";
                        case "NonEntitledChannelPlaybackEnabled":
                            return "nonEntitledChannelPlaybackEnabled";
                        case "NonExistingChannelPlaybackEnabled":
                            return "nonExistingChannelPlaybackEnabled";
                        case "PaddingAfterProgramEnds":
                            return "paddingAfterProgramEnds";
                        case "PaddingBeforeProgramStarts":
                            return "paddingBeforeProgramStarts";
                        case "ProtectionEnabled":
                            return "protectionEnabled";
                        case "ProtectionPeriod":
                            return "protectionPeriod";
                        case "ProtectionPolicy":
                            return "protectionPolicy";
                        case "ProtectionQuotaPercentage":
                            return "protectionQuotaPercentage";
                        case "QuotaOveragePolicy":
                            return "quotaOveragePolicy";
                        case "RecordingLifetimePeriod":
                            return "recordingLifetimePeriod";
                        case "RecordingScheduleWindow":
                            return "recordingScheduleWindow";
                        case "RecordingScheduleWindowEnabled":
                            return "recordingScheduleWindowEnabled";
                        case "RecoveryGracePeriod":
                            return "recoveryGracePeriod";
                        case "SeriesRecordingEnabled":
                            return "seriesRecordingEnabled";
                        case "StartOverEnabled":
                            return "startOverEnabled";
                        case "TrickPlayBufferLength":
                            return "trickPlayBufferLength";
                        case "TrickPlayEnabled":
                            return "trickPlayEnabled";
                    }
                    break;
                    
                case "KalturaTopic":
                    switch(property.Name)
                    {
                        case "AutomaticIssueNotification":
                            return "automaticIssueNotification";
                        case "Id":
                            return "id";
                        case "LastMessageSentDateSec":
                            return "lastMessageSentDateSec";
                        case "Name":
                            return "name";
                        case "SubscribersAmount":
                            return "subscribersAmount";
                    }
                    break;
                    
                case "KalturaTopicListResponse":
                    switch(property.Name)
                    {
                        case "Topics":
                            return "objects";
                    }
                    break;
                    
                case "KalturaTopicResponse":
                    switch(property.Name)
                    {
                        case "Topics":
                            return "objects";
                    }
                    break;
                    
                case "KalturaTransaction":
                    switch(property.Name)
                    {
                        case "CreatedAt":
                            return "createdAt";
                        case "FailReasonCode":
                            return "failReasonCode";
                        case "Id":
                            return "id";
                        case "PGReferenceID":
                            return "paymentGatewayReferenceId";
                        case "PGResponseID":
                            return "paymentGatewayResponseId";
                        case "State":
                            return "state";
                    }
                    break;
                    
                case "KalturaTransactionHistoryFilter":
                    switch(property.Name)
                    {
                        case "EndDateLessThanOrEqual":
                            return "endDateLessThanOrEqual";
                        case "EntityReferenceEqual":
                            return "entityReferenceEqual";
                        case "StartDateGreaterThanOrEqual":
                            return "startDateGreaterThanOrEqual";
                    }
                    break;
                    
                case "KalturaTransactionsFilter":
                    switch(property.Name)
                    {
                        case "By":
                            return "by";
                        case "EndDate":
                            return "endDate";
                        case "StartDate":
                            return "startDate";
                    }
                    break;
                    
                case "KalturaTransactionStatus":
                    switch(property.Name)
                    {
                        case "AdapterStatus":
                            return "adapterTransactionStatus";
                        case "ExternalId":
                            return "externalId";
                        case "ExternalMessage":
                            return "externalMessage";
                        case "ExternalStatus":
                            return "externalStatus";
                        case "FailReason":
                            return "failReason";
                    }
                    break;
                    
                case "KalturaTranslationToken":
                    switch(property.Name)
                    {
                        case "Language":
                            return "language";
                        case "Value":
                            return "value";
                    }
                    break;
                    
                case "KalturaUsageModule":
                    switch(property.Name)
                    {
                        case "CouponId":
                            return "couponId";
                        case "FullLifeCycle":
                            return "fullLifeCycle";
                        case "Id":
                            return "id";
                        case "IsOfflinePlayback":
                            return "isOfflinePlayback";
                        case "IsWaiverEnabled":
                            return "isWaiverEnabled";
                        case "MaxViewsNumber":
                            return "maxViewsNumber";
                        case "Name":
                            return "name";
                        case "ViewLifeCycle":
                            return "viewLifeCycle";
                        case "WaiverPeriod":
                            return "waiverPeriod";
                    }
                    break;
                    
                case "KalturaUserAssetRule":
                    switch(property.Name)
                    {
                        case "Description":
                            return "description";
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "RuleType":
                            return "ruleType";
                    }
                    break;
                    
                case "KalturaUserAssetRuleFilter":
                    switch(property.Name)
                    {
                        case "AssetIdEqual":
                            return "assetIdEqual";
                        case "AssetTypeEqual":
                            return "assetTypeEqual";
                    }
                    break;
                    
                case "KalturaUserAssetRuleListResponse":
                    switch(property.Name)
                    {
                        case "Rules":
                            return "objects";
                    }
                    break;
                    
                case "KalturaUserAssetsList":
                    switch(property.Name)
                    {
                        case "List":
                            return "list";
                        case "ListType":
                            return "listType";
                    }
                    break;
                    
                case "KalturaUserAssetsListFilter":
                    switch(property.Name)
                    {
                        case "AssetTypeEqual":
                            return "assetTypeEqual";
                        case "By":
                            return "by";
                        case "ListTypeEqual":
                            return "listTypeEqual";
                    }
                    break;
                    
                case "KalturaUserAssetsListItem":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "ListType":
                            return "listType";
                        case "OrderIndex":
                            return "orderIndex";
                        case "Type":
                            return "type";
                        case "UserId":
                            return "userId";
                    }
                    break;
                    
                case "KalturaUserBillingTransaction":
                    switch(property.Name)
                    {
                        case "UserFullName":
                            return "userFullName";
                        case "UserID":
                            return "userId";
                    }
                    break;
                    
                case "KalturaUserLoginPin":
                    switch(property.Name)
                    {
                        case "ExpirationTime":
                            return "expirationTime";
                        case "PinCode":
                            return "pinCode";
                        case "UserId":
                            return "userId";
                    }
                    break;
                    
                case "KalturaUserRole":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "Permissions":
                            return "permissions";
                    }
                    break;
                    
                case "KalturaUserRoleFilter":
                    switch(property.Name)
                    {
                        case "IdIn":
                            return "idIn";
                        case "Ids":
                            return "ids";
                    }
                    break;
                    
                case "KalturaUserRoleListResponse":
                    switch(property.Name)
                    {
                        case "UserRoles":
                            return "objects";
                    }
                    break;
                    
                case "KalturaUserSocialActionResponse":
                    switch(property.Name)
                    {
                        case "NetworkStatus":
                            return "failStatus";
                        case "SocialAction":
                            return "socialAction";
                    }
                    break;
                    
                case "KalturaWatchHistoryAsset":
                    switch(property.Name)
                    {
                        case "Asset":
                            return "asset";
                        case "Duration":
                            return "duration";
                        case "IsFinishedWatching":
                            return "finishedWatching";
                        case "LastWatched":
                            return "watchedDate";
                        case "Position":
                            return "position";
                    }
                    break;
                    
                case "KalturaWatchHistoryAssetWrapper":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
            }
            
            return property.Name;
        }
        
        public static void validateAuthorization(MethodInfo action, string serviceName, string actionName)
        {
            bool silent = false;
            switch (action.DeclaringType.Name)
            {
                case "AssetFileController":
                    switch(action.Name)
                    {
                        case "PlayManifest":
                            return;
                            
                    }
                    break;
                    
                case "BaseCategoryController":
                    return;
                    
                case "BookmarkController":
                    switch(action.Name)
                    {
                        case "Add":
                            silent = true;
                            break;
                            
                        case "AddOldStandard":
                            silent = true;
                            break;
                            
                    }
                    break;
                    
                case "ConfigurationsController":
                    switch(action.Name)
                    {
                        case "ServeByDevice":
                            return;
                            
                    }
                    break;
                    
                case "MultiRequestController":
                    return;
                    
                case "OttUserController":
                    switch(action.Name)
                    {
                        case "Activate":
                            return;
                            
                        case "AnonymousLogin":
                            return;
                            
                        case "FacebookLogin":
                            return;
                            
                        case "Login":
                            return;
                            
                        case "LoginWithPin":
                            return;
                            
                        case "RefreshSession":
                            silent = true;
                            break;
                            
                        case "Register":
                            return;
                            
                        case "ResendActivationToken":
                            return;
                            
                        case "resetPassword":
                            return;
                            
                        case "setInitialPassword":
                            return;
                            
                        case "setPassword":
                            return;
                            
                        case "updatePassword":
                            silent = true;
                            break;
                            
                        case "validateToken":
                            return;
                            
                    }
                    break;
                    
                case "ServiceController":
                    return;
                    
                case "SocialController":
                    switch(action.Name)
                    {
                        case "Login":
                            return;
                            
                    }
                    break;
                    
                case "SystemController":
                    switch(action.Name)
                    {
                        case "GetTime":
                            return;
                            
                        case "GetVersion":
                            return;
                            
                        case "Ping":
                            return;
                            
                    }
                    break;
                    
                case "VersionController":
                    return;
                    
            }
            
            RolesManager.ValidateActionPermitted(serviceName, actionName, silent);
        }
        
        public static string getServeActionContentType(MethodInfo action)
        {
            switch (action.DeclaringType.Name)
            {
                case "ConfigurationsController":
                    switch(action.Name)
                    {
                        case "ServeByDevice":
                            return "application/json";
                            
                    }
                    break;
                    
            }
            
            return null;
        }
        
        public static bool isNewStandardOnly(PropertyInfo property)
        {
            switch (property.DeclaringType.Name)
            {
                case "KalturaProductPrice":
                    switch(property.Name)
                    {
                        case "Price":
                            return true;
                        case "PurchaseStatus":
                            return true;
                    }
                    break;
                    
            }
            
            return false;
        }
        
    }
}
