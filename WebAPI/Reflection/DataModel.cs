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
                        case "Names":
                            return DeprecatedAttribute.IsDeprecated("3.6.287.27312");
                        case "Descriptions":
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
                case "KalturaEntitlement":
                    switch (propertyName)
                    {
                        case "Type":
                        case "PurchaseId":
                        case "NextRenewalDate":
                        case "IsRenewableForPurchase":
                        case "IsRenewable":
                        case "MediaFileId":
                        case "MediaId":
                        case "IsInGracePeriod":
                            return true;
                    };
                    break;
                    
                case "KalturaCollectionEntitlement":
                    switch (propertyName)
                    {
                        case "Type":
                        case "PurchaseId":
                        case "NextRenewalDate":
                        case "IsRenewableForPurchase":
                        case "IsRenewable":
                        case "MediaFileId":
                        case "MediaId":
                        case "IsInGracePeriod":
                            return true;
                    };
                    break;
                    
                case "KalturaPpvEntitlement":
                    switch (propertyName)
                    {
                        case "Type":
                        case "PurchaseId":
                        case "NextRenewalDate":
                        case "IsRenewableForPurchase":
                        case "IsRenewable":
                        case "IsInGracePeriod":
                            return true;
                    };
                    break;
                    
                case "KalturaSubscriptionEntitlement":
                    switch (propertyName)
                    {
                        case "Type":
                        case "PurchaseId":
                        case "MediaFileId":
                        case "MediaId":
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
                    
                case "KalturaBookmark":
                    switch (propertyName)
                    {
                        case "User":
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
                    
                case "KalturaAsset":
                    switch (propertyName)
                    {
                        case "Statistics":
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
                    
                case "KalturaMediaAsset":
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
                    
                case "KalturaUserRoleFilter":
                    switch (propertyName)
                    {
                        case "Ids":
                            return true;
                    };
                    break;
                    
                case "KalturaHouseholdPaymentMethod":
                    switch (propertyName)
                    {
                        case "Name":
                        case "AllowMultiInstance":
                        case "Selected":
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
                    
                case "KalturaBillingPartnerConfig":
                    switch (propertyName)
                    {
                        case "PartnerConfigurationType":
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
                    
                case "KalturaHouseholdDevice":
                    switch (propertyName)
                    {
                        case "Brand":
                        case "State":
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
                    
                case "KalturaDeviceFamilyBase":
                    switch (propertyName)
                    {
                        case "DeviceLimit":
                        case "ConcurrentLimit":
                            return true;
                    };
                    break;
                    
                case "KalturaDeviceFamily":
                    switch (propertyName)
                    {
                        case "Devices":
                        case "DeviceLimit":
                        case "ConcurrentLimit":
                            return true;
                    };
                    break;
                    
                case "KalturaHousehold":
                    switch (propertyName)
                    {
                        case "Users":
                        case "MasterUsers":
                        case "DefaultUsers":
                        case "PendingUsers":
                        case "DeviceFamilies":
                            return true;
                    };
                    break;
                    
                case "KalturaFavoriteFilter":
                    switch (propertyName)
                    {
                        case "MediaTypeIn":
                        case "UDID":
                        case "MediaIds":
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
                    
                case "KalturaChannel":
                    switch (propertyName)
                    {
                        case "MediaTypes":
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
                    
                case "KalturaAssetInfo":
                    switch (propertyName)
                    {
                        case "Statistics":
                            return true;
                    };
                    break;
                    
            }
            
            return IsDeprecated(type, propertyName);
        }
        
        public static Dictionary<string, string> getOldMembers(MethodInfo action)
        {
            switch (action.DeclaringType.Name)
            {
                case "CdnAdapterProfileController":
                    switch(action.Name)
                    {
                       case "Delete":
                            return new Dictionary<string, string>() { 
                                {"adapterId", "adapter_id"},
                           };
                    }
                    break;
                    
                case "CDVRAdapterProfileController":
                    switch(action.Name)
                    {
                       case "Delete":
                            return new Dictionary<string, string>() { 
                                {"adapterId", "adapter_id"},
                           };
                       case "GenerateSharedSecret":
                            return new Dictionary<string, string>() { 
                                {"adapterId", "adapter_id"},
                           };
                    }
                    break;
                    
                case "HouseholdPaymentGatewayController":
                    switch(action.Name)
                    {
                       case "Invoke":
                            return new Dictionary<string, string>() { 
                                {"extraParameters", "extra_parameters"},
                           };
                    }
                    break;
                    
                case "MessageTemplateController":
                    switch(action.Name)
                    {
                       case "Get":
                            return new Dictionary<string, string>() { 
                                {"assetType", "asset_Type"},
                           };
                    }
                    break;
                    
                case "FollowTvSeriesController":
                    switch(action.Name)
                    {
                       case "Delete":
                            return new Dictionary<string, string>() { 
                                {"assetId", "asset_id"},
                           };
                    }
                    break;
                    
                case "LicensedUrlController":
                    switch(action.Name)
                    {
                       case "GetOldStandard":
                            return new Dictionary<string, string>() { 
                                {"assetType", "asset_type"},
                                {"contentId", "content_id"},
                                {"baseUrl", "base_url"},
                                {"startDate", "start_date"},
                                {"streamType", "stream_type"},
                                {"assetId", "asset_id"},
                           };
                    }
                    break;
                    
                case "HomeNetworkController":
                    switch(action.Name)
                    {
                       case "Add":
                            return new Dictionary<string, string>() { 
                                {"homeNetwork", "home_network"},
                           };
                       case "Delete":
                            return new Dictionary<string, string>() { 
                                {"externalId", "external_id"},
                           };
                    }
                    break;
                    
                case "PaymentMethodProfileController":
                    switch(action.Name)
                    {
                       case "UpdateOldStandard":
                            return new Dictionary<string, string>() { 
                                {"paymentMethod", "payment_method"},
                                {"paymentGatewayId", "payment_gateway_id"},
                           };
                    }
                    break;
                    
                case "ExternalChannelProfileController":
                    switch(action.Name)
                    {
                       case "Delete":
                            return new Dictionary<string, string>() { 
                                {"externalChannelId", "external_channel_id"},
                           };
                       case "Add":
                            return new Dictionary<string, string>() { 
                                {"externalChannel", "external_channel"},
                           };
                    }
                    break;
                    
                case "OssAdapterProfileController":
                    switch(action.Name)
                    {
                       case "Delete":
                            return new Dictionary<string, string>() { 
                                {"ossAdapterId", "oss_adapter_id"},
                           };
                       case "Add":
                            return new Dictionary<string, string>() { 
                                {"ossAdapter", "oss_adapter"},
                           };
                       case "GenerateSharedSecret":
                            return new Dictionary<string, string>() { 
                                {"ossAdapterId", "oss_adapter_id"},
                           };
                    }
                    break;
                    
                case "RecommendationProfileController":
                    switch(action.Name)
                    {
                       case "Add":
                            return new Dictionary<string, string>() { 
                                {"recommendationEngine", "recommendation_engine"},
                           };
                       case "GenerateSharedSecret":
                            return new Dictionary<string, string>() { 
                                {"recommendationEngineId", "recommendation_engine_id"},
                           };
                    }
                    break;
                    
                case "SessionController":
                    switch(action.Name)
                    {
                       case "GetOldStandard":
                            return new Dictionary<string, string>() { 
                                {"session", "ks_to_parse"},
                           };
                    }
                    break;
                    
                case "HouseholdDeviceController":
                    switch(action.Name)
                    {
                       case "AddByPin":
                            return new Dictionary<string, string>() { 
                                {"deviceName", "device_name"},
                           };
                       case "GeneratePin":
                            return new Dictionary<string, string>() { 
                                {"brandId", "brand_id"},
                           };
                    }
                    break;
                    
                case "HouseholdUserController":
                    switch(action.Name)
                    {
                       case "Delete":
                            return new Dictionary<string, string>() { 
                                {"id", "user_id_to_delete"},
                           };
                    }
                    break;
                    
                case "ChannelController":
                    switch(action.Name)
                    {
                       case "Delete":
                            return new Dictionary<string, string>() { 
                                {"channelId", "channel_id"},
                           };
                    }
                    break;
                    
                case "EntitlementController":
                    switch(action.Name)
                    {
                       case "Cancel":
                            return new Dictionary<string, string>() { 
                                {"assetId", "asset_id"},
                                {"transactionType", "transaction_type"},
                           };
                       case "ForceCancel":
                            return new Dictionary<string, string>() { 
                                {"transactionType", "transaction_type"},
                                {"assetId", "asset_id"},
                           };
                       case "CancelRenewal":
                            return new Dictionary<string, string>() { 
                                {"subscriptionId", "subscription_id"},
                           };
                       case "Grant":
                            return new Dictionary<string, string>() { 
                                {"contentId", "content_id"},
                                {"productType", "product_type"},
                                {"productId", "product_id"},
                           };
                       case "Buy":
                            return new Dictionary<string, string>() { 
                                {"encryptedCvv", "encrypted_cvv"},
                                {"extraParams", "extra_params"},
                                {"fileId", "file_id"},
                                {"itemId", "item_id"},
                                {"isSubscription", "is_subscription"},
                                {"couponCode", "coupon_code"},
                           };
                    }
                    break;
                    
                case "PaymentGatewayController":
                    switch(action.Name)
                    {
                       case "Delete":
                            return new Dictionary<string, string>() { 
                                {"paymentGatewayId", "payment_gateway_id"},
                           };
                    }
                    break;
                    
                case "PaymentGatewayProfileController":
                    switch(action.Name)
                    {
                       case "Delete":
                            return new Dictionary<string, string>() { 
                                {"paymentGatewayId", "payment_gateway_id"},
                           };
                       case "AddOldStandard":
                            return new Dictionary<string, string>() { 
                                {"paymentGateway", "payment_gateway"},
                           };
                       case "UpdateOldStandard":
                            return new Dictionary<string, string>() { 
                                {"paymentGateway", "payment_gateway"},
                                {"paymentGatewayId", "payment_gateway_id"},
                           };
                       case "GenerateSharedSecret":
                            return new Dictionary<string, string>() { 
                                {"paymentGatewayId", "payment_gateway_id"},
                           };
                       case "GetConfiguration":
                            return new Dictionary<string, string>() { 
                                {"extraParameters", "extra_parameters"},
                           };
                    }
                    break;
                    
                case "TransactionController":
                    switch(action.Name)
                    {
                       case "SetWaiver":
                            return new Dictionary<string, string>() { 
                                {"assetId", "asset_id"},
                                {"transactionType", "transaction_type"},
                           };
                    }
                    break;
                    
                case "UserLoginPinController":
                    switch(action.Name)
                    {
                       case "Update":
                            return new Dictionary<string, string>() { 
                                {"pinCode", "pin_code"},
                           };
                       case "Delete":
                            return new Dictionary<string, string>() { 
                                {"pinCode", "pin_code"},
                           };
                    }
                    break;
                    
                case "ParentalRuleController":
                    switch(action.Name)
                    {
                       case "Enable":
                            return new Dictionary<string, string>() { 
                                {"entityReference", "by"},
                                {"ruleId", "rule_id"},
                           };
                       case "Disable":
                            return new Dictionary<string, string>() { 
                                {"ruleId", "rule_id"},
                                {"entityReference", "by"},
                           };
                       case "DisableDefault":
                            return new Dictionary<string, string>() { 
                                {"entityReference", "by"},
                           };
                    }
                    break;
                    
                case "HouseholdController":
                    switch(action.Name)
                    {
                       case "ResetFrequency":
                            return new Dictionary<string, string>() { 
                                {"frequencyType", "household_frequency_type"},
                           };
                    }
                    break;
                    
                case "OttUserController":
                    switch(action.Name)
                    {
                       case "Login":
                            return new Dictionary<string, string>() { 
                                {"extraParams", "extra_params"},
                           };
                       case "RefreshSession":
                            return new Dictionary<string, string>() { 
                                {"refreshToken", "refresh_token"},
                           };
                       case "UpdateLoginData":
                            return new Dictionary<string, string>() { 
                                {"oldPassword", "old_password"},
                                {"newPassword", "new_password"},
                           };
                       case "AddRole":
                            return new Dictionary<string, string>() { 
                                {"roleId", "role_id"},
                           };
                       case "Activate":
                            return new Dictionary<string, string>() { 
                                {"activationToken", "activation_token"},
                           };
                    }
                    break;
                    
            }
            
            return null;
        }
        
        public static Dictionary<string, string> getOldMembers(Type type)
        {
            switch (type.Name)
            {
                case "KalturaEntitlement":
                    return new Dictionary<string, string>() { 
                        {"currentDate", "current_date"},
                        {"isInGracePeriod", "is_in_grace_period"},
                        {"purchaseId", "purchase_id"},
                        {"paymentMethod", "payment_method"},
                        {"entitlementId", "entitlement_id"},
                        {"currentUses", "current_uses"},
                        {"endDate", "end_date"},
                        {"deviceName", "device_name"},
                        {"lastViewDate", "last_view_date"},
                        {"purchaseDate", "purchase_date"},
                        {"maxUses", "max_uses"},
                        {"mediaFileId", "media_file_id"},
                        {"deviceUdid", "device_udid"},
                        {"mediaId", "media_id"},
                        {"isCancelationWindowEnabled", "is_cancelation_window_enabled"},
                        {"nextRenewalDate", "next_renewal_date"},
                        {"isRenewableForPurchase", "is_renewable_for_purchase"},
                        {"isRenewable", "is_renewable"},
                    };
                    
                case "KalturaCollectionEntitlement":
                    return new Dictionary<string, string>() { 
                        {"currentDate", "current_date"},
                        {"isInGracePeriod", "is_in_grace_period"},
                        {"purchaseId", "purchase_id"},
                        {"paymentMethod", "payment_method"},
                        {"entitlementId", "entitlement_id"},
                        {"currentUses", "current_uses"},
                        {"endDate", "end_date"},
                        {"deviceName", "device_name"},
                        {"lastViewDate", "last_view_date"},
                        {"purchaseDate", "purchase_date"},
                        {"maxUses", "max_uses"},
                        {"mediaFileId", "media_file_id"},
                        {"deviceUdid", "device_udid"},
                        {"mediaId", "media_id"},
                        {"isCancelationWindowEnabled", "is_cancelation_window_enabled"},
                        {"nextRenewalDate", "next_renewal_date"},
                        {"isRenewableForPurchase", "is_renewable_for_purchase"},
                        {"isRenewable", "is_renewable"},
                    };
                    
                case "KalturaMediaFile":
                    return new Dictionary<string, string>() { 
                        {"assetId", "asset_id"},
                        {"externalId", "external_id"},
                    };
                    
                case "KalturaPlaybackSource":
                    return new Dictionary<string, string>() { 
                        {"assetId", "asset_id"},
                        {"externalId", "external_id"},
                    };
                    
                case "KalturaPpvEntitlement":
                    return new Dictionary<string, string>() { 
                        {"currentDate", "current_date"},
                        {"isInGracePeriod", "is_in_grace_period"},
                        {"purchaseId", "purchase_id"},
                        {"paymentMethod", "payment_method"},
                        {"entitlementId", "entitlement_id"},
                        {"currentUses", "current_uses"},
                        {"endDate", "end_date"},
                        {"deviceName", "device_name"},
                        {"lastViewDate", "last_view_date"},
                        {"purchaseDate", "purchase_date"},
                        {"maxUses", "max_uses"},
                        {"mediaFileId", "media_file_id"},
                        {"deviceUdid", "device_udid"},
                        {"mediaId", "media_id"},
                        {"isCancelationWindowEnabled", "is_cancelation_window_enabled"},
                        {"nextRenewalDate", "next_renewal_date"},
                        {"isRenewableForPurchase", "is_renewable_for_purchase"},
                        {"isRenewable", "is_renewable"},
                    };
                    
                case "KalturaSubscriptionEntitlement":
                    return new Dictionary<string, string>() { 
                        {"currentDate", "current_date"},
                        {"isInGracePeriod", "is_in_grace_period"},
                        {"purchaseId", "purchase_id"},
                        {"paymentMethod", "payment_method"},
                        {"entitlementId", "entitlement_id"},
                        {"currentUses", "current_uses"},
                        {"endDate", "end_date"},
                        {"deviceName", "device_name"},
                        {"lastViewDate", "last_view_date"},
                        {"purchaseDate", "purchase_date"},
                        {"maxUses", "max_uses"},
                        {"mediaFileId", "media_file_id"},
                        {"deviceUdid", "device_udid"},
                        {"mediaId", "media_id"},
                        {"isCancelationWindowEnabled", "is_cancelation_window_enabled"},
                        {"nextRenewalDate", "next_renewal_date"},
                        {"isRenewableForPurchase", "is_renewable_for_purchase"},
                        {"isRenewable", "is_renewable"},
                    };
                    
                case "KalturaDeviceBrand":
                    return new Dictionary<string, string>() { 
                        {"concurrentLimit", "concurrent_limit"},
                        {"deviceLimit", "device_limit"},
                    };
                    
                case "KalturaGenericRule":
                    return new Dictionary<string, string>() { 
                        {"ruleType", "rule_type"},
                    };
                    
                case "KalturaBaseAssetInfo":
                    return new Dictionary<string, string>() { 
                        {"mediaFiles", "media_files"},
                    };
                    
                case "KalturaAsset":
                    return new Dictionary<string, string>() { 
                        {"mediaFiles", "media_files"},
                    };
                    
                case "KalturaProgramAsset":
                    return new Dictionary<string, string>() { 
                        {"mediaFiles", "media_files"},
                    };
                    
                case "KalturaMediaAsset":
                    return new Dictionary<string, string>() { 
                        {"mediaFiles", "media_files"},
                    };
                    
                case "KalturaRecordingAsset":
                    return new Dictionary<string, string>() { 
                        {"mediaFiles", "media_files"},
                    };
                    
                case "KalturaEntitlementFilter":
                    return new Dictionary<string, string>() { 
                        {"entitlementType", "entitlement_type"},
                    };
                    
                case "KalturaCDVRAdapterProfile":
                    return new Dictionary<string, string>() { 
                        {"adapterUrl", "adapter_url"},
                        {"externalIdentifier", "external_identifier"},
                        {"sharedSecret", "shared_secret"},
                        {"isActive", "is_active"},
                        {"dynamicLinksSupport", "dynamic_links_support"},
                    };
                    
                case "KalturaExportTask":
                    return new Dictionary<string, string>() { 
                        {"isActive", "is_active"},
                        {"dataType", "data_type"},
                        {"exportType", "export_type"},
                        {"notificationUrl", "notification_url"},
                        {"vodTypes", "vod_types"},
                    };
                    
                case "KalturaExternalChannelProfile":
                    return new Dictionary<string, string>() { 
                        {"recommendationEngineId", "recommendation_engine_id"},
                        {"isActive", "is_active"},
                        {"externalIdentifier", "external_identifier"},
                        {"filterExpression", "filter_expression"},
                    };
                    
                case "KalturaGenericRuleFilter":
                    return new Dictionary<string, string>() { 
                        {"assetId", "asset_id"},
                        {"assetType", "asset_type"},
                    };
                    
                case "KalturaOSSAdapterProfile":
                    return new Dictionary<string, string>() { 
                        {"sharedSecret", "shared_secret"},
                        {"isActive", "is_active"},
                        {"adapterUrl", "adapter_url"},
                        {"ossAdapterSettings", "oss_adapter_settings"},
                        {"externalIdentifier", "external_identifier"},
                    };
                    
                case "KalturaRecommendationProfile":
                    return new Dictionary<string, string>() { 
                        {"adapterUrl", "adapter_url"},
                        {"recommendationEngineSettings", "recommendation_engine_settings"},
                        {"externalIdentifier", "external_identifier"},
                        {"sharedSecret", "shared_secret"},
                        {"isActive", "is_active"},
                    };
                    
                case "KalturaChannelProfile":
                    return new Dictionary<string, string>() { 
                        {"filterExpression", "filter_expression"},
                        {"assetTypes", "asset_types"},
                        {"isActive", "is_active"},
                    };
                    
                case "KalturaPaymentMethod":
                    return new Dictionary<string, string>() { 
                        {"allowMultiInstance", "allow_multi_instance"},
                        {"householdPaymentMethods", "household_payment_methods"},
                    };
                    
                case "KalturaPaymentGatewayConfiguration":
                    return new Dictionary<string, string>() { 
                        {"paymentGatewayConfiguration", "payment_gatewaye_configuration"},
                    };
                    
                case "KalturaPaymentMethodProfile":
                    return new Dictionary<string, string>() { 
                        {"allowMultiInstance", "allow_multi_instance"},
                    };
                    
                case "KalturaAssetBookmark":
                    return new Dictionary<string, string>() { 
                        {"finishedWatching", "finished_watching"},
                        {"positionOwner", "position_owner"},
                    };
                    
                case "KalturaAssetsFilter":
                    return new Dictionary<string, string>() { 
                        {"assets", "Assets"},
                    };
                    
                case "KalturaEPGChannelAssetsListResponse":
                    return new Dictionary<string, string>() { 
                        {"objects", "assets"},
                    };
                    
                case "KalturaEPGChannelAssets":
                    return new Dictionary<string, string>() { 
                        {"channelId", "channel_id"},
                    };
                    
                case "KalturaEpgChannelFilter":
                    return new Dictionary<string, string>() { 
                        {"startTime", "start_time"},
                        {"endTime", "end_time"},
                    };
                    
                case "KalturaAssetHistoryFilter":
                    return new Dictionary<string, string>() { 
                        {"daysLessThanOrEqual", "days"},
                        {"filterTypes", "filter_types"},
                        {"statusEqual", "filter_status"},
                    };
                    
                case "KalturaAssetInfoFilter":
                    return new Dictionary<string, string>() { 
                        {"referenceType", "reference_type"},
                    };
                    
                case "KalturaPaymentGatewayBaseProfile":
                    return new Dictionary<string, string>() { 
                        {"selectedBy", "selected_by"},
                        {"paymentMethods", "payment_methods"},
                        {"isDefault", "is_default"},
                    };
                    
                case "KalturaPaymentGatewayProfile":
                    return new Dictionary<string, string>() { 
                        {"transactUrl", "transact_url"},
                        {"paymentGatewayeSettings", "payment_gateway_settings"},
                        {"renewIntervalMinutes", "renew_interval_minutes"},
                        {"isActive", "is_active"},
                        {"adapterUrl", "adapter_url"},
                        {"statusUrl", "status_url"},
                        {"renewUrl", "renew_url"},
                        {"renewStartMinutes", "renew_start_minutes"},
                        {"externalIdentifier", "external_identifier"},
                        {"pendingInterval", "pending_interval"},
                        {"pendingRetries", "pending_retries"},
                        {"sharedSecret", "shared_secret"},
                        {"selectedBy", "selected_by"},
                        {"paymentMethods", "payment_methods"},
                        {"isDefault", "is_default"},
                    };
                    
                case "KalturaPersonalAssetRequest":
                    return new Dictionary<string, string>() { 
                        {"fileIds", "file_ids"},
                    };
                    
                case "KalturaLicensedUrl":
                    return new Dictionary<string, string>() { 
                        {"altUrl", "alt_url"},
                        {"mainUrl", "main_url"},
                    };
                    
                case "KalturaBillingResponse":
                    return new Dictionary<string, string>() { 
                        {"receiptCode", "receipt_code"},
                        {"externalReceiptCode", "external_receipt_code"},
                    };
                    
                case "KalturaBillingTransaction":
                    return new Dictionary<string, string>() { 
                        {"purchasedItemCode", "purchased_item_code"},
                        {"actionDate", "action_date"},
                        {"startDate", "start_date"},
                        {"endDate", "end_date"},
                        {"recieptCode", "reciept_code"},
                        {"purchasedItemName", "purchased_item_name"},
                        {"itemType", "item_type"},
                        {"billingAction", "billing_action"},
                        {"paymentMethod", "payment_method"},
                        {"paymentMethodExtraDetails", "payment_method_extra_details"},
                        {"isRecurring", "is_recurring"},
                        {"billingProviderRef", "billing_provider_ref"},
                        {"purchaseId", "purchase_id"},
                    };
                    
                case "KalturaUserBillingTransaction":
                    return new Dictionary<string, string>() { 
                        {"userId", "user_id"},
                        {"userFullName", "user_full_name"},
                        {"purchasedItemCode", "purchased_item_code"},
                        {"actionDate", "action_date"},
                        {"startDate", "start_date"},
                        {"endDate", "end_date"},
                        {"recieptCode", "reciept_code"},
                        {"purchasedItemName", "purchased_item_name"},
                        {"itemType", "item_type"},
                        {"billingAction", "billing_action"},
                        {"paymentMethod", "payment_method"},
                        {"paymentMethodExtraDetails", "payment_method_extra_details"},
                        {"isRecurring", "is_recurring"},
                        {"billingProviderRef", "billing_provider_ref"},
                        {"purchaseId", "purchase_id"},
                    };
                    
                case "KalturaEntitlementsFilter":
                    return new Dictionary<string, string>() { 
                        {"entitlementType", "entitlement_type"},
                    };
                    
                case "KalturaPricesFilter":
                    return new Dictionary<string, string>() { 
                        {"filesIds", "files_ids"},
                        {"subscriptionsIds", "subscriptions_ids"},
                        {"shouldGetOnlyLowest", "should_get_only_lowest"},
                    };
                    
                case "KalturaHouseholdLimitations":
                    return new Dictionary<string, string>() { 
                        {"usersLimit", "users_limit"},
                        {"deviceFamiliesLimitations", "device_families_limitations"},
                        {"concurrentLimit", "concurrent_limit"},
                        {"deviceLimit", "device_limit"},
                        {"deviceFrequency", "device_frequency"},
                        {"deviceFrequencyDescription", "device_frequency_description"},
                        {"userFrequency", "user_frequency"},
                        {"userFrequencyDescription", "user_frequency_description"},
                        {"npvrQuotaInSeconds", "npvr_quota_in_seconds"},
                    };
                    
                case "KalturaTransaction":
                    return new Dictionary<string, string>() { 
                        {"failReasonCode", "fail_reason_code"},
                        {"paymentGatewayResponseId", "payment_gateway_response_id"},
                        {"createdAt", "created_at"},
                        {"paymentGatewayReferenceId", "payment_gateway_reference_id"},
                    };
                    
                case "KalturaTransactionsFilter":
                    return new Dictionary<string, string>() { 
                        {"startDate", "start_date"},
                        {"endDate", "end_date"},
                    };
                    
                case "KalturaPlayerAssetData":
                    return new Dictionary<string, string>() { 
                        {"totalBitrate", "total_bitrate"},
                        {"currentBitrate", "current_bitrate"},
                        {"averageBitrate", "average_bitrate"},
                    };
                    
                case "KalturaAnnouncement":
                    return new Dictionary<string, string>() { 
                        {"startTime", "start_time"},
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
                        {"assetId", "asset_id"},
                        {"announcementId", "announcement_id"},
                        {"followPhrase", "follow_phrase"},
                    };
                    
                case "KalturaFollowTvSeries":
                    return new Dictionary<string, string>() { 
                        {"announcementId", "announcement_id"},
                        {"followPhrase", "follow_phrase"},
                    };
                    
                case "KalturaMessageTemplate":
                    return new Dictionary<string, string>() { 
                        {"assetType", "asset_type"},
                        {"dateFormat", "date_format"},
                    };
                    
                case "KalturaNotificationsSettings":
                    return new Dictionary<string, string>() { 
                        {"pushNotificationEnabled", "push_notification_enabled"},
                        {"pushFollowEnabled", "push_follow_enabled"},
                    };
                    
                case "KalturaNotificationSettings":
                    return new Dictionary<string, string>() { 
                        {"pushNotificationEnabled", "push_notification_enabled"},
                        {"pushFollowEnabled", "push_follow_enabled"},
                    };
                    
                case "KalturaNotificationsPartnerSettings":
                    return new Dictionary<string, string>() { 
                        {"pushEndHour", "push_end_hour"},
                        {"pushNotificationEnabled", "push_notification_enabled"},
                        {"pushSystemAnnouncementsEnabled", "push_system_announcements_enabled"},
                        {"pushStartHour", "push_start_hour"},
                    };
                    
                case "KalturaPartnerNotificationSettings":
                    return new Dictionary<string, string>() { 
                        {"pushEndHour", "push_end_hour"},
                        {"pushNotificationEnabled", "push_notification_enabled"},
                        {"pushSystemAnnouncementsEnabled", "push_system_announcements_enabled"},
                        {"pushStartHour", "push_start_hour"},
                    };
                    
                case "KalturaPersonalFollowFeed":
                    return new Dictionary<string, string>() { 
                        {"assetId", "asset_id"},
                    };
                    
                case "KalturaPersonalFeed":
                    return new Dictionary<string, string>() { 
                        {"assetId", "asset_id"},
                    };
                    
                case "KalturaBillingPartnerConfig":
                    return new Dictionary<string, string>() { 
                        {"partnerConfigurationType", "partner_configuration_type"},
                    };
                    
                case "KalturaProductPrice":
                    return new Dictionary<string, string>() { 
                        {"productId", "product_id"},
                        {"productType", "product_type"},
                    };
                    
                case "KalturaPpvPrice":
                    return new Dictionary<string, string>() { 
                        {"productId", "product_id"},
                        {"productType", "product_type"},
                    };
                    
                case "KalturaCoupon":
                    return new Dictionary<string, string>() { 
                        {"couponsGroup", "coupons_group"},
                    };
                    
                case "KalturaPPVItemPriceDetails":
                    return new Dictionary<string, string>() { 
                        {"isSubscriptionOnly", "is_subscription_only"},
                        {"ppvProductCode", "ppv_product_code"},
                        {"prePaidId", "pre_paid_id"},
                        {"ppvDescriptions", "ppv_descriptions"},
                        {"isInCancelationPeriod", "is_in_cancelation_period"},
                        {"ppvModuleId", "ppv_module_id"},
                        {"fullPrice", "full_price"},
                        {"purchaseStatus", "purchase_status"},
                        {"subscriptionId", "subscription_id"},
                        {"collectionId", "collection_id"},
                        {"purchaseUserId", "purchase_user_id"},
                        {"purchasedMediaFileId", "purchased_media_file_id"},
                        {"relatedMediaFileIds", "related_media_file_ids"},
                        {"startDate", "start_date"},
                        {"endDate", "end_date"},
                        {"discountEndDate", "discount_end_date"},
                        {"firstDeviceName", "first_device_name"},
                    };
                    
                case "KalturaCouponsGroup":
                    return new Dictionary<string, string>() { 
                        {"maxUsesNumberOnRenewableSub", "max_uses_number_on_renewable_sub"},
                        {"maxUsesNumber", "max_uses_number"},
                        {"startDate", "start_date"},
                        {"endDate", "end_date"},
                    };
                    
                case "KalturaDiscountModule":
                    return new Dictionary<string, string>() { 
                        {"endDate", "end_date"},
                        {"startDate", "start_date"},
                    };
                    
                case "KalturaItemPrice":
                    return new Dictionary<string, string>() { 
                        {"ppvPriceDetails", "ppv_price_details"},
                        {"fileId", "file_id"},
                        {"productId", "product_id"},
                        {"productType", "product_type"},
                    };
                    
                case "KalturaPreviewModule":
                    return new Dictionary<string, string>() { 
                        {"lifeCycle", "life_cycle"},
                        {"nonRenewablePeriod", "non_renewable_period"},
                    };
                    
                case "KalturaUsageModule":
                    return new Dictionary<string, string>() { 
                        {"viewLifeCycle", "view_life_cycle"},
                        {"fullLifeCycle", "full_life_cycle"},
                        {"isOfflinePlayback", "is_offline_playback"},
                        {"maxViewsNumber", "max_views_number"},
                        {"couponId", "coupon_id"},
                        {"waiverPeriod", "waiver_period"},
                        {"isWaiverEnabled", "is_waiver_enabled"},
                    };
                    
                case "KalturaPricePlan":
                    return new Dictionary<string, string>() { 
                        {"discountId", "discount_id"},
                        {"isRenewable", "is_renewable"},
                        {"renewalsNumber", "renewals_number"},
                        {"priceId", "price_id"},
                        {"viewLifeCycle", "view_life_cycle"},
                        {"fullLifeCycle", "full_life_cycle"},
                        {"isOfflinePlayback", "is_offline_playback"},
                        {"maxViewsNumber", "max_views_number"},
                        {"couponId", "coupon_id"},
                        {"waiverPeriod", "waiver_period"},
                        {"isWaiverEnabled", "is_waiver_enabled"},
                    };
                    
                case "KalturaSubscriptionPrice":
                    return new Dictionary<string, string>() { 
                        {"price", "price"},
                        {"purchaseStatus", "purchase_status"},
                        {"productId", "product_id"},
                        {"productType", "product_type"},
                    };
                    
                case "KalturaHouseholdDevice":
                    return new Dictionary<string, string>() { 
                        {"activatedOn", "activated_on"},
                        {"brandId", "brand_id"},
                    };
                    
                case "KalturaDevice":
                    return new Dictionary<string, string>() { 
                        {"activatedOn", "activated_on"},
                        {"brandId", "brand_id"},
                    };
                    
                case "KalturaDeviceFamilyBase":
                    return new Dictionary<string, string>() { 
                        {"deviceLimit", "device_limit"},
                        {"concurrentLimit", "concurrent_limit"},
                    };
                    
                case "KalturaDeviceFamily":
                    return new Dictionary<string, string>() { 
                        {"deviceLimit", "device_limit"},
                        {"concurrentLimit", "concurrent_limit"},
                    };
                    
                case "KalturaHouseholdDeviceFamilyLimitations":
                    return new Dictionary<string, string>() { 
                        {"deviceLimit", "device_limit"},
                        {"concurrentLimit", "concurrent_limit"},
                    };
                    
                case "KalturaHousehold":
                    return new Dictionary<string, string>() { 
                        {"deviceFamilies", "device_families"},
                        {"frequencyNextUserAction", "frequency_next_user_action"},
                        {"externalId", "external_id"},
                        {"householdLimitationsId", "household_limitations_id"},
                        {"devicesLimit", "devices_limit"},
                        {"usersLimit", "users_limit"},
                        {"concurrentLimit", "concurrent_limit"},
                        {"masterUsers", "master_users"},
                        {"defaultUsers", "default_users"},
                        {"pendingUsers", "pending_users"},
                        {"regionId", "region_id"},
                        {"isFrequencyEnabled", "is_frequency_enabled"},
                        {"frequencyNextDeviceAction", "frequency_next_device_action"},
                    };
                    
                case "KalturaHomeNetwork":
                    return new Dictionary<string, string>() { 
                        {"isActive", "is_active"},
                        {"externalId", "external_id"},
                    };
                    
                case "KalturaPrice":
                    return new Dictionary<string, string>() { 
                        {"currencySign", "currency_sign"},
                    };
                    
                case "KalturaSubscription":
                    return new Dictionary<string, string>() { 
                        {"waiverPeriod", "waiver_period"},
                        {"userTypes", "user_types"},
                        {"isWaiverEnabled", "is_waiver_enabled"},
                        {"startDate", "start_date"},
                        {"endDate", "end_date"},
                        {"fileTypes", "file_types"},
                        {"isRenewable", "is_renewable"},
                        {"renewalsNumber", "renewals_number"},
                        {"isInfiniteRenewal", "is_infinite_renewal"},
                        {"discountModule", "discount_module"},
                        {"couponsGroup", "coupons_group"},
                        {"mediaId", "media_id"},
                        {"prorityInOrder", "prority_in_order"},
                        {"productCode", "product_code"},
                        {"pricePlans", "price_plans"},
                        {"previewModule", "preview_module"},
                        {"householdLimitationsId", "household_limitations_id"},
                        {"gracePeriodMinutes", "grace_period_minutes"},
                        {"premiumServices", "premium_services"},
                        {"maxViewsNumber", "max_views_number"},
                        {"viewLifeCycle", "view_life_cycle"},
                    };
                    
                case "KalturaSocialFacebookConfig":
                    return new Dictionary<string, string>() { 
                        {"appId", "app_id"},
                    };
                    
                case "KalturaSocialResponse":
                    return new Dictionary<string, string>() { 
                        {"socialUser", "social_user"},
                        {"userId", "user_id"},
                        {"kalturaUsername", "kaltura_username"},
                        {"socialUsername", "social_username"},
                        {"minFriendsLimitation", "min_friends_limitation"},
                    };
                    
                case "KalturaSocialUser":
                    return new Dictionary<string, string>() { 
                        {"userId", "user_id"},
                        {"firstName", "first_name"},
                        {"lastName", "last_name"},
                    };
                    
                case "KalturaTimeShiftedTvPartnerSettings":
                    return new Dictionary<string, string>() { 
                        {"trickPlayBufferLength", "trick_play_buffer_length"},
                        {"recordingScheduleWindow", "recording_schedule_window"},
                        {"catchUpEnabled", "catch_up_enabled"},
                        {"cdvrEnabled", "cdvr_enabled"},
                        {"startOverEnabled", "start_over_enabled"},
                        {"trickPlayEnabled", "trick_play_enabled"},
                        {"recordingScheduleWindowEnabled", "recording_schedule_window_enabled"},
                        {"catchUpBufferLength", "catch_up_buffer_length"},
                    };
                    
                case "KalturaFavoriteFilter":
                    return new Dictionary<string, string>() { 
                        {"mediaTypeIn", "media_type"},
                    };
                    
                case "KalturaLoginResponse":
                    return new Dictionary<string, string>() { 
                        {"loginSession", "login_session"},
                    };
                    
                case "KalturaUserAssetsListFilter":
                    return new Dictionary<string, string>() { 
                        {"assetTypeEqual", "asset_type"},
                        {"listTypeEqual", "list_type"},
                    };
                    
                case "KalturaFavorite":
                    return new Dictionary<string, string>() { 
                        {"extraData", "extra_data"},
                    };
                    
                case "KalturaLoginSession":
                    return new Dictionary<string, string>() { 
                        {"refreshToken", "refresh_token"},
                    };
                    
                case "KalturaBaseOTTUser":
                    return new Dictionary<string, string>() { 
                        {"firstName", "first_name"},
                        {"lastName", "last_name"},
                    };
                    
                case "KalturaPurchaseSettingsResponse":
                    return new Dictionary<string, string>() { 
                        {"purchaseSettingsType", "purchase_settings_type"},
                    };
                    
                case "KalturaOTTCategory":
                    return new Dictionary<string, string>() { 
                        {"childCategories", "child_categories"},
                        {"parentCategoryId", "parent_category_id"},
                    };
                    
                case "KalturaChannel":
                    return new Dictionary<string, string>() { 
                        {"filterExpression", "filter_expression"},
                        {"assetTypes", "asset_types"},
                    };
                    
                case "KalturaParentalRule":
                    return new Dictionary<string, string>() { 
                        {"mediaTagValues", "media_tag_values"},
                        {"isDefault", "is_default"},
                        {"epgTagValues", "epg_tag_values"},
                        {"mediaTag", "media_tag"},
                        {"epgTag", "epg_tag"},
                        {"blockAnonymousAccess", "block_anonymous_access"},
                        {"ruleType", "rule_type"},
                    };
                    
                case "KalturaUserLoginPin":
                    return new Dictionary<string, string>() { 
                        {"userId", "user_id"},
                        {"expirationTime", "expiration_time"},
                        {"pinCode", "pin_code"},
                    };
                    
                case "KalturaOTTUser":
                    return new Dictionary<string, string>() { 
                        {"affiliateCode", "affiliate_code"},
                        {"externalId", "external_id"},
                        {"userType", "user_type"},
                        {"facebookImage", "facebook_image"},
                        {"householdId", "household_id"},
                        {"facebookToken", "facebook_token"},
                        {"dynamicData", "dynamic_data"},
                        {"isHouseholdMaster", "is_household_master"},
                        {"suspentionState", "suspention_state"},
                        {"userState", "user_state"},
                        {"facebookId", "facebook_id"},
                        {"firstName", "first_name"},
                        {"lastName", "last_name"},
                    };
                    
                case "KalturaUserAssetsList":
                    return new Dictionary<string, string>() { 
                        {"listType", "list_type"},
                    };
                    
                case "KalturaUserAssetsListItem":
                    return new Dictionary<string, string>() { 
                        {"listType", "list_type"},
                        {"orderIndex", "order_index"},
                        {"userId", "user_id"},
                    };
                    
                case "KalturaAssetInfoListResponse":
                    return new Dictionary<string, string>() { 
                        {"requestId", "request_id"},
                    };
                    
                case "KalturaAssetInfo":
                    return new Dictionary<string, string>() { 
                        {"startDate", "start_date"},
                        {"endDate", "end_date"},
                        {"extraParams", "extra_params"},
                        {"mediaFiles", "media_files"},
                    };
                    
                case "KalturaAssetStatistics":
                    return new Dictionary<string, string>() { 
                        {"buzzScore", "buzz_score"},
                        {"assetId", "asset_id"},
                        {"ratingCount", "rating_count"},
                    };
                    
                case "KalturaBuzzScore":
                    return new Dictionary<string, string>() { 
                        {"avgScore", "avg_score"},
                        {"normalizedAvgScore", "normalized_avg_score"},
                        {"updateDate", "update_date"},
                    };
                    
                case "KalturaMediaImage":
                    return new Dictionary<string, string>() { 
                        {"isDefault", "is_default"},
                    };
                    
                case "KalturaWatchHistoryAsset":
                    return new Dictionary<string, string>() { 
                        {"finishedWatching", "finished_watching"},
                        {"watchedDate", "watched_date"},
                    };
                    
                case "AnnouncementController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                        {"enableSystemAnnouncements", "createannouncement"},
                        {"addOldStandard", "add"},
                        {"updateOldStandard", "update"},
                    };
                    
                case "AssetController":
                    return new Dictionary<string, string>() { 
                        {"getOldStandard", "get"},
                        {"listOldStandard", "list"},
                    };
                    
                case "CDVRAdapterProfileController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                        {"updateOldStandard", "update"},
                    };
                    
                case "HouseholdPaymentGatewayController":
                    return new Dictionary<string, string>() { 
                        {"enable", "set"},
                        {"disable", "delete"},
                    };
                    
                case "FollowTvSeriesController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                        {"addOldStandard", "add"},
                    };
                    
                case "PersonalFeedController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                    };
                    
                case "LicensedUrlController":
                    return new Dictionary<string, string>() { 
                        {"getOldStandard", "get"},
                    };
                    
                case "HomeNetworkController":
                    return new Dictionary<string, string>() { 
                        {"updateOldStandard", "update"},
                        {"listOldStandard", "list"},
                    };
                    
                case "NotificationsPartnerSettingsController":
                    return new Dictionary<string, string>() { 
                        {"updateOldStandard", "update"},
                        {"getOldStandard", "get"},
                    };
                    
                case "NotificationsSettingsController":
                    return new Dictionary<string, string>() { 
                        {"getOldStandard", "get"},
                        {"updateOldStandard", "update"},
                    };
                    
                case "PaymentMethodProfileController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                        {"deleteOldStandard", "delete"},
                        {"addOldStandard", "add"},
                        {"updateOldStandard", "update"},
                    };
                    
                case "RegistrySettingsController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                    };
                    
                case "TopicController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                    };
                    
                case "UserAssetsListItemController":
                    return new Dictionary<string, string>() { 
                        {"getOldStandard", "get"},
                        {"deleteOldStandard", "delete"},
                    };
                    
                case "HouseholdPremiumServiceController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                    };
                    
                case "ExportTaskController":
                    return new Dictionary<string, string>() { 
                        {"updateOldStandard", "update"},
                        {"listOldStandard", "list"},
                    };
                    
                case "ExternalChannelProfileController":
                    return new Dictionary<string, string>() { 
                        {"updateOldStandard", "update"},
                        {"listOldStandard", "list"},
                    };
                    
                case "OssAdapterProfileController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                        {"updateOldStandard", "update"},
                    };
                    
                case "RecommendationProfileController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                        {"updateOldStandard", "update"},
                    };
                    
                case "SessionController":
                    return new Dictionary<string, string>() { 
                        {"getOldStandard", "get"},
                    };
                    
                case "UserAssetRuleController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                    };
                    
                case "AssetHistoryController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                    };
                    
                case "HouseholdDeviceController":
                    return new Dictionary<string, string>() { 
                        {"updateOldStandard", "update"},
                        {"addOldStandard", "add"},
                    };
                    
                case "BookmarkController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                        {"addOldStandard", "add"},
                    };
                    
                case "PurchaseSettingsController":
                    return new Dictionary<string, string>() { 
                        {"updateOldStandard", "update"},
                        {"getOldStandard", "get"},
                    };
                    
                case "HouseholdUserController":
                    return new Dictionary<string, string>() { 
                        {"addOldStandard", "add"},
                    };
                    
                case "SubscriptionController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                    };
                    
                case "ChannelController":
                    return new Dictionary<string, string>() { 
                        {"updateOldStandard", "update"},
                        {"addOldStandard", "add"},
                    };
                    
                case "PaymentGatewayProfileSettingsController":
                    return new Dictionary<string, string>() { 
                        {"addOldStandard", "add"},
                    };
                    
                case "EntitlementController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                    };
                    
                case "FavoriteController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                        {"addOldStandard", "add"},
                        {"deleteOldStandard", "delete"},
                    };
                    
                case "PaymentGatewayProfileController":
                    return new Dictionary<string, string>() { 
                        {"updateOldStandard", "update"},
                        {"listOldStandard", "list"},
                        {"addOldStandard", "add"},
                    };
                    
                case "SocialController":
                    return new Dictionary<string, string>() { 
                        {"getConfiguration", "config"},
                        {"getByTokenOldStandard", "getbytoken"},
                        {"registerOldStandard", "register"},
                        {"mergeOldStandard", "merge"},
                        {"unmergeOldStandard", "unmerge"},
                    };
                    
                case "TransactionController":
                    return new Dictionary<string, string>() { 
                        {"setWaiver", "waiver"},
                        {"purchaseSessionIdOldStandard", "purchasesessionid"},
                        {"purchaseOldStandard", "purchase"},
                    };
                    
                case "PinController":
                    return new Dictionary<string, string>() { 
                        {"getOldStandard", "get"},
                        {"updateOldStandard", "update"},
                    };
                    
                case "ParentalRuleController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                    };
                    
                case "TransactionHistoryController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                    };
                    
                case "UserRoleController":
                    return new Dictionary<string, string>() { 
                        {"listOldStandard", "list"},
                    };
                    
                case "HouseholdController":
                    return new Dictionary<string, string>() { 
                        {"addOldStandard", "add"},
                        {"updateOldStandard", "update"},
                        {"getOldStandard", "get"},
                    };
                    
                case "OttUserController":
                    return new Dictionary<string, string>() { 
                        {"getOldStandard", "get"},
                        {"register", "add"},
                        {"updateLoginData", "changepassword"},
                        {"setPassword", "resetpassword"},
                        {"resetPassword", "sendpassword"},
                    };
                    
            }
            
            return null;
        }
        
        public static string getApiName(PropertyInfo property)
        {
            switch (property.DeclaringType.Name)
            {
                case "KalturaNotification":
                    switch(property.Name)
                    {
                        case "eventObject":
                            return "object";
                    }
                    break;
                    
                case "KalturaFilter`1":
                    switch(property.Name)
                    {
                        case "OrderBy":
                            return "orderBy";
                    }
                    break;
                    
                case "KalturaMetaFilter":
                    switch(property.Name)
                    {
                        case "FieldNameEqual":
                            return "fieldNameEqual";
                        case "FieldNameNotEqual":
                            return "fieldNameNotEqual";
                        case "TypeEqual":
                            return "typeEqual";
                        case "AssetTypeEqual":
                            return "assetTypeEqual";
                    }
                    break;
                    
                case "KalturaListResponse":
                    switch(property.Name)
                    {
                        case "TotalCount":
                            return "totalCount";
                    }
                    break;
                    
                case "KalturaMetaListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaMeta":
                    switch(property.Name)
                    {
                        case "Name":
                            return "name";
                        case "FieldName":
                            return "fieldName";
                        case "Type":
                            return "type";
                        case "AssetType":
                            return "assetType";
                    }
                    break;
                    
                case "KalturaDeviceBrandListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaCountryFilter":
                    switch(property.Name)
                    {
                        case "IdIn":
                            return "idIn";
                    }
                    break;
                    
                case "KalturaCountryListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaOSSAdapterProfileListResponse":
                    switch(property.Name)
                    {
                        case "OSSAdapterProfiles":
                            return "objects";
                    }
                    break;
                    
                case "KalturaAssetCountListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaAssetCount":
                    switch(property.Name)
                    {
                        case "Value":
                            return "value";
                        case "Count":
                            return "count";
                        case "SubCounts":
                            return "subs";
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
                    
                case "KalturaAssetGroupBy":
                    switch(property.Name)
                    {
                        case "Value":
                            return "value";
                    }
                    break;
                    
                case "KalturaLastPositionListResponse":
                    switch(property.Name)
                    {
                        case "LastPositions":
                            return "objects";
                    }
                    break;
                    
                case "KalturaLastPosition":
                    switch(property.Name)
                    {
                        case "UserId":
                            return "user_id";
                        case "Position":
                            return "position";
                        case "PositionOwner":
                            return "position_owner";
                    }
                    break;
                    
                case "KalturaLastPositionFilter":
                    switch(property.Name)
                    {
                        case "Ids":
                            return "ids";
                        case "Type":
                            return "type";
                        case "By":
                            return "by";
                    }
                    break;
                    
                case "KalturaAccessControlMessage":
                    switch(property.Name)
                    {
                        case "Message":
                            return "message";
                        case "Code":
                            return "code";
                    }
                    break;
                    
                case "KalturaEntitlement":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Type":
                            return "type";
                        case "EntitlementId":
                            return "entitlementId";
                        case "CurrentUses":
                            return "currentUses";
                        case "EndDate":
                            return "endDate";
                        case "CurrentDate":
                            return "currentDate";
                        case "LastViewDate":
                            return "lastViewDate";
                        case "PurchaseDate":
                            return "purchaseDate";
                        case "PurchaseId":
                            return "purchaseId";
                        case "PaymentMethod":
                            return "paymentMethod";
                        case "DeviceUDID":
                            return "deviceUdid";
                        case "DeviceName":
                            return "deviceName";
                        case "IsCancelationWindowEnabled":
                            return "isCancelationWindowEnabled";
                        case "MaxUses":
                            return "maxUses";
                        case "NextRenewalDate":
                            return "nextRenewalDate";
                        case "IsRenewableForPurchase":
                            return "isRenewableForPurchase";
                        case "IsRenewable":
                            return "isRenewable";
                        case "MediaFileId":
                            return "mediaFileId";
                        case "MediaId":
                            return "mediaId";
                        case "IsInGracePeriod":
                            return "isInGracePeriod";
                    }
                    break;
                    
                case "KalturaCompensation":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "SubscriptionId":
                            return "subscriptionId";
                        case "CompensationType":
                            return "compensationType";
                        case "Amount":
                            return "amount";
                        case "TotalRenewalIterations":
                            return "totalRenewalIterations";
                        case "AppliedRenewalIterations":
                            return "appliedRenewalIterations";
                        case "PurchaseId":
                            return "purchaseId";
                    }
                    break;
                    
                case "KalturaDrmPlaybackPluginData":
                    switch(property.Name)
                    {
                        case "Scheme":
                            return "scheme";
                        case "LicenseURL":
                            return "licenseURL";
                    }
                    break;
                    
                case "KalturaFairPlayPlaybackPluginData":
                    switch(property.Name)
                    {
                        case "Certificate":
                            return "certificate";
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
                    
                case "KalturaNpvrPremiumService":
                    switch(property.Name)
                    {
                        case "QuotaInMinutes":
                            return "quotaInMinutes";
                    }
                    break;
                    
                case "KalturaPlaybackContext":
                    switch(property.Name)
                    {
                        case "Sources":
                            return "sources";
                        case "Actions":
                            return "actions";
                        case "Messages":
                            return "messages";
                    }
                    break;
                    
                case "KalturaPlaybackContextOptions":
                    switch(property.Name)
                    {
                        case "MediaProtocol":
                            return "mediaProtocol";
                        case "StreamerType":
                            return "streamerType";
                        case "AssetFileIds":
                            return "assetFileIds";
                        case "Context":
                            return "context";
                    }
                    break;
                    
                case "KalturaMediaFile":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "assetId";
                        case "Id":
                            return "id";
                        case "Type":
                            return "type";
                        case "Url":
                            return "url";
                        case "Duration":
                            return "duration";
                        case "ExternalId":
                            return "externalId";
                        case "BillingType":
                            return "billingType";
                        case "Quality":
                            return "quality";
                        case "HandlingType":
                            return "handlingType";
                        case "CdnName":
                            return "cdnName";
                        case "CdnCode":
                            return "cdnCode";
                        case "AltCdnCode":
                            return "altCdnCode";
                        case "PPVModules":
                            return "ppvModules";
                        case "ProductCode":
                            return "productCode";
                    }
                    break;
                    
                case "KalturaPlaybackSource":
                    switch(property.Name)
                    {
                        case "Format":
                            return "format";
                        case "Protocols":
                            return "protocols";
                        case "Drm":
                            return "drm";
                        case "AdsPolicy":
                            return "adsPolicy";
                        case "AdsParams":
                            return "adsParam";
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
                    
                case "KalturaRuleAction":
                    switch(property.Name)
                    {
                        case "Type":
                            return "type";
                    }
                    break;
                    
                case "KalturaSubscriptionEntitlement":
                    switch(property.Name)
                    {
                        case "NextRenewalDate":
                            return "nextRenewalDate";
                        case "IsRenewableForPurchase":
                            return "isRenewableForPurchase";
                        case "IsRenewable":
                            return "isRenewable";
                        case "IsInGracePeriod":
                            return "isInGracePeriod";
                        case "PaymentGatewayId":
                            return "paymentGatewayId";
                        case "PaymentMethodId":
                            return "paymentMethodId";
                    }
                    break;
                    
                case "KalturaConfigurations":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "PartnerId":
                            return "partnerId";
                        case "ConfigurationGroupId":
                            return "configurationGroupId";
                        case "AppName":
                            return "appName";
                        case "ClientVersion":
                            return "clientVersion";
                        case "Platform":
                            return "platform";
                        case "ExternalPushId":
                            return "externalPushId";
                        case "IsForceUpdate":
                            return "isForceUpdate";
                        case "Content":
                            return "content";
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
                    
                case "KalturaDeviceReportFilter":
                    switch(property.Name)
                    {
                        case "LastAccessDateGreaterThanOrEqual":
                            return "lastAccessDateGreaterThanOrEqual";
                    }
                    break;
                    
                case "KalturaDeviceReport":
                    switch(property.Name)
                    {
                        case "PartnerId":
                            return "partnerId";
                        case "ConfigurationGroupId":
                            return "configurationGroupId";
                        case "Udid":
                            return "udid";
                        case "PushParameters":
                            return "pushParameters";
                        case "VersionNumber":
                            return "versionNumber";
                        case "VersionPlatform":
                            return "versionPlatform";
                        case "VersionAppName":
                            return "versionAppName";
                        case "LastAccessIP":
                            return "lastAccessIP";
                        case "LastAccessDate":
                            return "lastAccessDate";
                        case "UserAgent":
                            return "userAgent";
                        case "OperationSystem":
                            return "operationSystem";
                    }
                    break;
                    
                case "KalturaConfigurationGroupTagFilter":
                    switch(property.Name)
                    {
                        case "ConfigurationGroupIdEqual":
                            return "configurationGroupIdEqual";
                    }
                    break;
                    
                case "KalturaConfigurationGroupDeviceFilter":
                    switch(property.Name)
                    {
                        case "ConfigurationGroupIdEqual":
                            return "configurationGroupIdEqual";
                    }
                    break;
                    
                case "KalturaPushParams":
                    switch(property.Name)
                    {
                        case "Token":
                            return "token";
                        case "ExternalToken":
                            return "externalToken";
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
                    
                case "KalturaConfigurationGroupDeviceListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaConfigurationGroupTagListResponse":
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
                    
                case "KalturaConfigurationGroup":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "PartnerId":
                            return "partnerId";
                        case "IsDefault":
                            return "isDefault";
                        case "Tags":
                            return "tags";
                        case "NumberOfDevices":
                            return "numberOfDevices";
                        case "ConfigurationIdentifiers":
                            return "configurationIdentifiers";
                    }
                    break;
                    
                case "KalturaConfigurationGroupListResponse":
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
                    
                case "KalturaScheduledRecordingProgramFilter":
                    switch(property.Name)
                    {
                        case "RecordingTypeEqual":
                            return "recordingTypeEqual";
                        case "ChannelsIn":
                            return "channelsIn";
                        case "StartDateGreaterThanOrNull":
                            return "startDateGreaterThanOrNull";
                        case "EndDateLessThanOrNull":
                            return "endDateLessThanOrNull";
                    }
                    break;
                    
                case "KalturaDeviceBrand":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "DeviceFamilyId":
                            return "deviceFamilyid";
                    }
                    break;
                    
                case "KalturaHouseholdDeviceFilter":
                    switch(property.Name)
                    {
                        case "HouseholdIdEqual":
                            return "householdIdEqual";
                        case "DeviceFamilyIdIn":
                            return "deviceFamilyIdIn";
                    }
                    break;
                    
                case "KalturaDeviceFamilyListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
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
                    
                case "KalturaRegionalChannel":
                    switch(property.Name)
                    {
                        case "LinearChannelId":
                            return "linearChannelId";
                        case "ChannelNumber":
                            return "channelNumber";
                    }
                    break;
                    
                case "KalturaPin":
                    switch(property.Name)
                    {
                        case "PIN":
                            return "pin";
                        case "Origin":
                            return "origin";
                        case "Type":
                            return "type";
                    }
                    break;
                    
                case "KalturaPurchaseSettings":
                    switch(property.Name)
                    {
                        case "Permission":
                            return "permission";
                    }
                    break;
                    
                case "KalturaRegion":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "ExternalId":
                            return "externalId";
                        case "IsDefault":
                            return "isDefault";
                        case "RegionalChannels":
                            return "linearChannels";
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
                    
                case "KalturaGenericRule":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "RuleType":
                            return "ruleType";
                        case "Name":
                            return "name";
                        case "Description":
                            return "description";
                    }
                    break;
                    
                case "KalturaParentalRuleFilter":
                    switch(property.Name)
                    {
                        case "EntityReferenceEqual":
                            return "entityReferenceEqual";
                    }
                    break;
                    
                case "KalturaExportTaskFilter":
                    switch(property.Name)
                    {
                        case "IdIn":
                            return "idIn";
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
                    
                case "KalturaCDNAdapterProfile":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "IsActive":
                            return "isActive";
                        case "AdapterUrl":
                            return "adapterUrl";
                        case "BaseUrl":
                            return "baseUrl";
                        case "Settings":
                            return "settings";
                        case "SystemName":
                            return "systemName";
                        case "SharedSecret":
                            return "sharedSecret";
                    }
                    break;
                    
                case "KalturaCDNAdapterProfileListResponse":
                    switch(property.Name)
                    {
                        case "Adapters":
                            return "objects";
                    }
                    break;
                    
                case "KalturaSocialComment":
                    switch(property.Name)
                    {
                        case "Header":
                            return "header";
                        case "Text":
                            return "text";
                        case "CreateDate":
                            return "createDate";
                        case "Writer":
                            return "writer";
                    }
                    break;
                    
                case "KalturaAssetComment":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "AssetId":
                            return "assetId";
                        case "AssetType":
                            return "assetType";
                        case "SubHeader":
                            return "subHeader";
                    }
                    break;
                    
                case "KalturaAssetStatisticsQuery":
                    switch(property.Name)
                    {
                        case "AssetIdIn":
                            return "assetIdIn";
                        case "AssetTypeEqual":
                            return "assetTypeEqual";
                        case "StartDateGreaterThanOrEqual":
                            return "startDateGreaterThanOrEqual";
                        case "EndDateGreaterThanOrEqual":
                            return "endDateGreaterThanOrEqual";
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
                    
                case "KalturaBookmarkFilter":
                    switch(property.Name)
                    {
                        case "AssetIn":
                            return "assetIn";
                        case "AssetIdIn":
                            return "assetIdIn";
                        case "AssetTypeEqual":
                            return "assetTypeEqual";
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
                    
                case "KalturaBookmark":
                    switch(property.Name)
                    {
                        case "User":
                            return "user";
                        case "UserId":
                            return "userId";
                        case "Position":
                            return "position";
                        case "PositionOwner":
                            return "positionOwner";
                        case "IsFinishedWatching":
                            return "finishedWatching";
                        case "PlayerData":
                            return "playerData";
                    }
                    break;
                    
                case "KalturaBookmarkPlayerData":
                    switch(property.Name)
                    {
                        case "averageBitRate":
                            return "averageBitrate";
                        case "totalBitRate":
                            return "totalBitrate";
                        case "currentBitRate":
                            return "currentBitrate";
                        case "FileId":
                            return "fileId";
                    }
                    break;
                    
                case "KalturaBookmarkListResponse":
                    switch(property.Name)
                    {
                        case "AssetsBookmarks":
                            return "objects";
                    }
                    break;
                    
                case "KalturaAssetListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaBaseAssetInfo":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Type":
                            return "type";
                        case "Name":
                            return "name";
                        case "Description":
                            return "description";
                        case "Images":
                            return "images";
                        case "MediaFiles":
                            return "mediaFiles";
                        case "Statistics":
                            return "stats";
                    }
                    break;
                    
                case "KalturaAsset":
                    switch(property.Name)
                    {
                        case "Metas":
                            return "metas";
                        case "Tags":
                            return "tags";
                        case "StartDate":
                            return "startDate";
                        case "EndDate":
                            return "endDate";
                        case "EnableCdvr":
                            return "enableCdvr";
                        case "EnableCatchUp":
                            return "enableCatchUp";
                        case "EnableStartOver":
                            return "enableStartOver";
                        case "EnableTrickPlay":
                            return "enableTrickPlay";
                        case "ExternalId":
                            return "externalId";
                    }
                    break;
                    
                case "KalturaProgramAsset":
                    switch(property.Name)
                    {
                        case "EpgChannelId":
                            return "epgChannelId";
                        case "EpgId":
                            return "epgId";
                        case "RelatedMediaId":
                            return "relatedMediaId";
                        case "Crid":
                            return "crid";
                        case "LinearAssetId":
                            return "linearAssetId";
                    }
                    break;
                    
                case "KalturaMediaAsset":
                    switch(property.Name)
                    {
                        case "ExternalIds":
                            return "externalIds";
                        case "CatchUpBuffer":
                            return "catchUpBuffer";
                        case "TrickPlayBuffer":
                            return "trickPlayBuffer";
                        case "EnableRecordingPlaybackNonEntitledChannel":
                            return "enableRecordingPlaybackNonEntitledChannel";
                        case "TypeDescription":
                            return "typeDescription";
                        case "EntryId":
                            return "entryId";
                        case "DeviceRule":
                            return "deviceRule";
                        case "GeoBlockRule":
                            return "geoBlockRule";
                        case "WatchPermissionRule":
                            return "watchPermissionRule";
                    }
                    break;
                    
                case "KalturaRecordingAsset":
                    switch(property.Name)
                    {
                        case "RecordingId":
                            return "recordingId";
                    }
                    break;
                    
                case "KalturaBundleFilter":
                    switch(property.Name)
                    {
                        case "IdEqual":
                            return "idEqual";
                        case "TypeIn":
                            return "typeIn";
                        case "BundleTypeEqual":
                            return "bundleTypeEqual";
                    }
                    break;
                    
                case "KalturaChannelExternalFilter":
                    switch(property.Name)
                    {
                        case "IdEqual":
                            return "idEqual";
                        case "UtcOffsetEqual":
                            return "utcOffsetEqual";
                        case "FreeText":
                            return "freeText";
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
                    
                case "KalturaAssetCommentListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaRelatedFilter":
                    switch(property.Name)
                    {
                        case "KSql":
                            return "kSql";
                        case "IdEqual":
                            return "idEqual";
                        case "TypeIn":
                            return "typeIn";
                    }
                    break;
                    
                case "KalturaRelatedExternalFilter":
                    switch(property.Name)
                    {
                        case "IdEqual":
                            return "idEqual";
                        case "TypeIn":
                            return "typeIn";
                        case "UtcOffsetEqual":
                            return "utcOffsetEqual";
                        case "FreeText":
                            return "freeText";
                    }
                    break;
                    
                case "KalturaSearchAssetFilter":
                    switch(property.Name)
                    {
                        case "KSql":
                            return "kSql";
                        case "TypeIn":
                            return "typeIn";
                        case "IdIn":
                            return "idIn";
                    }
                    break;
                    
                case "KalturaSearchExternalFilter":
                    switch(property.Name)
                    {
                        case "Query":
                            return "query";
                        case "UtcOffsetEqual":
                            return "utcOffsetEqual";
                        case "TypeIn":
                            return "typeIn";
                    }
                    break;
                    
                case "KalturaLicensedUrlBaseRequest":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "assetId";
                    }
                    break;
                    
                case "KalturaLicensedUrlMediaRequest":
                    switch(property.Name)
                    {
                        case "ContentId":
                            return "contentId";
                        case "BaseUrl":
                            return "baseUrl";
                    }
                    break;
                    
                case "KalturaLicensedUrlEpgRequest":
                    switch(property.Name)
                    {
                        case "StreamType":
                            return "streamType";
                        case "StartDate":
                            return "startDate";
                    }
                    break;
                    
                case "KalturaLicensedUrlRecordingRequest":
                    switch(property.Name)
                    {
                        case "FileType":
                            return "fileType";
                    }
                    break;
                    
                case "KalturaAssetFileContext":
                    switch(property.Name)
                    {
                        case "ViewLifeCycle":
                            return "viewLifeCycle";
                        case "FullLifeCycle":
                            return "fullLifeCycle";
                        case "IsOfflinePlayBack":
                            return "isOfflinePlayBack";
                    }
                    break;
                    
                case "KalturaSeriesRecording":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "EpgId":
                            return "epgId";
                        case "ChannelId":
                            return "channelId";
                        case "SeriesId":
                            return "seriesId";
                        case "SeasonNumber":
                            return "seasonNumber";
                        case "Type":
                            return "type";
                        case "CreateDate":
                            return "createDate";
                        case "UpdateDate":
                            return "updateDate";
                        case "ExcludedSeasons":
                            return "excludedSeasons";
                    }
                    break;
                    
                case "KalturaSeriesRecordingListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaTransactionStatus":
                    switch(property.Name)
                    {
                        case "AdapterStatus":
                            return "adapterTransactionStatus";
                        case "ExternalId":
                            return "externalId";
                        case "ExternalStatus":
                            return "externalStatus";
                        case "ExternalMessage":
                            return "externalMessage";
                        case "FailReason":
                            return "failReason";
                    }
                    break;
                    
                case "KalturaPurchaseBase":
                    switch(property.Name)
                    {
                        case "ProductId":
                            return "productId";
                        case "ContentId":
                            return "contentId";
                        case "ProductType":
                            return "productType";
                    }
                    break;
                    
                case "KalturaPurchase":
                    switch(property.Name)
                    {
                        case "Currency":
                            return "currency";
                        case "Price":
                            return "price";
                        case "PaymentMethodId":
                            return "paymentMethodId";
                        case "PaymentGatewayId":
                            return "paymentGatewayId";
                        case "Coupon":
                            return "coupon";
                        case "AdapterData":
                            return "adapterData";
                    }
                    break;
                    
                case "KalturaPurchaseSession":
                    switch(property.Name)
                    {
                        case "PreviewModuleId":
                            return "previewModuleId";
                    }
                    break;
                    
                case "KalturaExternalReceipt":
                    switch(property.Name)
                    {
                        case "ReceiptId":
                            return "receiptId";
                        case "PaymentGatewayName":
                            return "paymentGatewayName";
                    }
                    break;
                    
                case "KalturaHouseholdPremiumServiceListResponse":
                    switch(property.Name)
                    {
                        case "PremiumServices":
                            return "objects";
                    }
                    break;
                    
                case "KalturaProductPriceFilter":
                    switch(property.Name)
                    {
                        case "SubscriptionIdIn":
                            return "subscriptionIdIn";
                        case "FileIdIn":
                            return "fileIdIn";
                        case "CouponCodeEqual":
                            return "couponCodeEqual";
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
                    
                case "KalturaTransactionHistoryFilter":
                    switch(property.Name)
                    {
                        case "EntityReferenceEqual":
                            return "entityReferenceEqual";
                        case "StartDateGreaterThanOrEqual":
                            return "startDateGreaterThanOrEqual";
                        case "EndDateLessThanOrEqual":
                            return "endDateLessThanOrEqual";
                    }
                    break;
                    
                case "KalturaCDVRAdapterProfileListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaCDVRAdapterProfile":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "IsActive":
                            return "isActive";
                        case "AdapterUrl":
                            return "adapterUrl";
                        case "Settings":
                            return "settings";
                        case "ExternalIdentifier":
                            return "externalIdentifier";
                        case "SharedSecret":
                            return "sharedSecret";
                        case "DynamicLinksSupport":
                            return "dynamicLinksSupport";
                    }
                    break;
                    
                case "KalturaPermissionsFilter":
                    switch(property.Name)
                    {
                        case "Ids":
                            return "ids";
                    }
                    break;
                    
                case "KalturaExportTask":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Alias":
                            return "alias";
                        case "Name":
                            return "name";
                        case "DataType":
                            return "dataType";
                        case "Filter":
                            return "filter";
                        case "ExportType":
                            return "exportType";
                        case "Frequency":
                            return "frequency";
                        case "NotificationUrl":
                            return "notificationUrl";
                        case "VodTypes":
                            return "vodTypes";
                        case "IsActive":
                            return "isActive";
                    }
                    break;
                    
                case "KalturaExportTaskListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaExternalChannelProfileListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaExternalChannelProfile":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "IsActive":
                            return "isActive";
                        case "ExternalIdentifier":
                            return "externalIdentifier";
                        case "FilterExpression":
                            return "filterExpression";
                        case "RecommendationEngineId":
                            return "recommendationEngineId";
                        case "Enrichments":
                            return "enrichments";
                    }
                    break;
                    
                case "KalturaUserAssetRule":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "RuleType":
                            return "ruleType";
                        case "Name":
                            return "name";
                        case "Description":
                            return "description";
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
                        case "IsActive":
                            return "isActive";
                        case "AdapterUrl":
                            return "adapterUrl";
                        case "Settings":
                            return "ossAdapterSettings";
                        case "ExternalIdentifier":
                            return "externalIdentifier";
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
                    
                case "KalturaRecommendationProfile":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "IsActive":
                            return "isActive";
                        case "AdapterUrl":
                            return "adapterUrl";
                        case "Settings":
                            return "recommendationEngineSettings";
                        case "ExternalIdentifier":
                            return "externalIdentifier";
                        case "SharedSecret":
                            return "sharedSecret";
                    }
                    break;
                    
                case "KalturaRegistrySettingsListResponse":
                    switch(property.Name)
                    {
                        case "RegistrySettings":
                            return "objects";
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
                    
                case "KalturaUserRoleFilter":
                    switch(property.Name)
                    {
                        case "Ids":
                            return "ids";
                        case "IdIn":
                            return "idIn";
                    }
                    break;
                    
                case "KalturaChannelProfile":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "Description":
                            return "description";
                        case "IsActive":
                            return "isActive";
                        case "FilterExpression":
                            return "filterExpression";
                        case "AssetTypes":
                            return "assetTypes";
                        case "Order":
                            return "order";
                    }
                    break;
                    
                case "KalturaHouseholdPaymentMethod":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "ExternalId":
                            return "externalId";
                        case "PaymentGatewayId":
                            return "paymentGatewayId";
                        case "Details":
                            return "details";
                        case "IsDefault":
                            return "isDefault";
                        case "PaymentMethodProfileId":
                            return "paymentMethodProfileId";
                        case "Name":
                            return "name";
                        case "AllowMultiInstance":
                            return "allowMultiInstance";
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
                    
                case "KalturaPaymentMethod":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "AllowMultiInstance":
                            return "allowMultiInstance";
                        case "HouseholdPaymentMethods":
                            return "householdPaymentMethods";
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
                    
                case "KalturaPaymentGatewayConfiguration":
                    switch(property.Name)
                    {
                        case "Configuration":
                            return "paymentGatewayConfiguration";
                    }
                    break;
                    
                case "KalturaPaymentMethodProfileListResponse":
                    switch(property.Name)
                    {
                        case "PaymentMethodProfiles":
                            return "objects";
                    }
                    break;
                    
                case "KalturaPaymentMethodProfileFilter":
                    switch(property.Name)
                    {
                        case "PaymentGatewayIdEqual":
                            return "paymentGatewayIdEqual";
                    }
                    break;
                    
                case "KalturaPaymentMethodProfile":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "PaymentGatewayId":
                            return "paymentGatewayId";
                        case "Name":
                            return "name";
                        case "AllowMultiInstance":
                            return "allowMultiInstance";
                    }
                    break;
                    
                case "KalturaAssetBookmark":
                    switch(property.Name)
                    {
                        case "User":
                            return "user";
                        case "Position":
                            return "position";
                        case "PositionOwner":
                            return "positionOwner";
                        case "IsFinishedWatching":
                            return "finishedWatching";
                    }
                    break;
                    
                case "KalturaAssetsFilter":
                    switch(property.Name)
                    {
                        case "Assets":
                            return "assets";
                    }
                    break;
                    
                case "KalturaEPGChannelAssetsListResponse":
                    switch(property.Name)
                    {
                        case "Channels":
                            return "objects";
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
                    
                case "KalturaEpgChannelFilter":
                    switch(property.Name)
                    {
                        case "IDs":
                            return "ids";
                        case "StartTime":
                            return "startTime";
                        case "EndTime":
                            return "endTime";
                    }
                    break;
                    
                case "KalturaAssetHistoryFilter":
                    switch(property.Name)
                    {
                        case "TypeIn":
                            return "typeIn";
                        case "AssetIdIn":
                            return "assetIdIn";
                        case "StatusEqual":
                            return "statusEqual";
                        case "DaysLessThanOrEqual":
                            return "daysLessThanOrEqual";
                    }
                    break;
                    
                case "KalturaAssetInfoFilter":
                    switch(property.Name)
                    {
                        case "IDs":
                            return "ids";
                        case "ReferenceType":
                            return "referenceType";
                        case "FilterTags":
                            return "filter_tags";
                        case "cutWith":
                            return "cut_with";
                    }
                    break;
                    
                case "KalturaPersonalFile":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Discounted":
                            return "discounted";
                        case "Offer":
                            return "offer";
                        case "Entitled":
                            return "entitled";
                    }
                    break;
                    
                case "KalturaPersonalAssetListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaPersonalAsset":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Type":
                            return "type";
                        case "Bookmarks":
                            return "bookmarks";
                        case "Files":
                            return "files";
                        case "Following":
                            return "following";
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
                    
                case "KalturaPaymentGatewayBaseProfile":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "IsDefault":
                            return "isDefault";
                        case "PaymentMethods":
                            return "paymentMethods";
                    }
                    break;
                    
                case "KalturaHouseholdPaymentGateway":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "IsDefault":
                            return "isDefault";
                    }
                    break;
                    
                case "KalturaHouseholdPaymentGatewayListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaPaymentGatewayProfile":
                    switch(property.Name)
                    {
                        case "IsActive":
                            return "isActive";
                        case "AdapterUrl":
                            return "adapterUrl";
                        case "TransactUrl":
                            return "transactUrl";
                        case "StatusUrl":
                            return "statusUrl";
                        case "RenewUrl":
                            return "renewUrl";
                        case "Settings":
                            return "paymentGatewaySettings";
                        case "ExternalIdentifier":
                            return "externalIdentifier";
                        case "PendingInterval":
                            return "pendingInterval";
                        case "PendingRetries":
                            return "pendingRetries";
                        case "SharedSecret":
                            return "sharedSecret";
                        case "RenewIntervalMinutes":
                            return "renewIntervalMinutes";
                        case "RenewStartMinutes":
                            return "renewStartMinutes";
                    }
                    break;
                    
                case "KalturaPaymentGatewayProfileListResponse":
                    switch(property.Name)
                    {
                        case "PaymentGatewayProfiles":
                            return "objects";
                    }
                    break;
                    
                case "KalturaParentalRuleListResponse":
                    switch(property.Name)
                    {
                        case "ParentalRule":
                            return "objects";
                    }
                    break;
                    
                case "KalturaPersonalAssetRequest":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Type":
                            return "type";
                        case "FileIds":
                            return "fileIds";
                    }
                    break;
                    
                case "KalturaAssetsBookmarksResponse":
                    switch(property.Name)
                    {
                        case "AssetsBookmarks":
                            return "objects";
                    }
                    break;
                    
                case "KalturaAssetBookmarks":
                    switch(property.Name)
                    {
                        case "Bookmarks":
                            return "objects";
                    }
                    break;
                    
                case "KalturaHouseholdQuota":
                    switch(property.Name)
                    {
                        case "HouseholdId":
                            return "householdId";
                        case "TotalQuota":
                            return "totalQuota";
                        case "AvailableQuota":
                            return "availableQuota";
                    }
                    break;
                    
                case "KalturaRecording":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Status":
                            return "status";
                        case "AssetId":
                            return "assetId";
                        case "Type":
                            return "type";
                        case "ViewableUntilDate":
                            return "viewableUntilDate";
                        case "IsProtected":
                            return "isProtected";
                        case "CreateDate":
                            return "createDate";
                        case "UpdateDate":
                            return "updateDate";
                    }
                    break;
                    
                case "KalturaRecordingContext":
                    switch(property.Name)
                    {
                        case "Code":
                            return "code";
                        case "Message":
                            return "message";
                        case "AssetId":
                            return "assetId";
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
                        case "StatusIn":
                            return "statusIn";
                        case "FilterExpression":
                            return "filterExpression";
                    }
                    break;
                    
                case "KalturaRecordingListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaLicensedUrl":
                    switch(property.Name)
                    {
                        case "MainUrl":
                            return "mainUrl";
                        case "AltUrl":
                            return "altUrl";
                    }
                    break;
                    
                case "KalturaBillingResponse":
                    switch(property.Name)
                    {
                        case "ReceiptCode":
                            return "receiptCode";
                        case "ExternalReceiptCode":
                            return "externalReceiptCode";
                    }
                    break;
                    
                case "KalturaBillingTransactionListResponse":
                    switch(property.Name)
                    {
                        case "transactions":
                            return "objects";
                    }
                    break;
                    
                case "KalturaBillingTransaction":
                    switch(property.Name)
                    {
                        case "purchaseID":
                            return "purchaseId";
                    }
                    break;
                    
                case "KalturaUserBillingTransaction":
                    switch(property.Name)
                    {
                        case "UserID":
                            return "userId";
                        case "UserFullName":
                            return "userFullNName";
                    }
                    break;
                    
                case "KalturaEntitlementsFilter":
                    switch(property.Name)
                    {
                        case "EntitlementType":
                            return "entitlementType";
                        case "By":
                            return "by";
                    }
                    break;
                    
                case "KalturaPricesFilter":
                    switch(property.Name)
                    {
                        case "SubscriptionsIds":
                            return "subscriptionsIds";
                        case "FilesIds":
                            return "filesIds";
                        case "ShouldGetOnlyLowest":
                            return "shouldGetOnlyLowest";
                    }
                    break;
                    
                case "KalturaHouseholdDeviceListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
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
                    
                case "KalturaHouseholdUser":
                    switch(property.Name)
                    {
                        case "HouseholdId":
                            return "householdId";
                        case "UserId":
                            return "userId";
                        case "IsMaster":
                            return "isMaster";
                        case "HouseholdMasterUsername":
                            return "householdMasterUsername";
                        case "Status":
                            return "status";
                        case "IsDefault":
                            return "isDefault";
                    }
                    break;
                    
                case "KalturaHouseholdLimitations":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "ConcurrentLimit":
                            return "concurrentLimit";
                        case "DeviceLimit":
                            return "deviceLimit";
                        case "DeviceFrequency":
                            return "deviceFrequency";
                        case "DeviceFrequencyDescription":
                            return "deviceFrequencyDescription";
                        case "UserFrequency":
                            return "userFrequency";
                        case "UserFrequencyDescription":
                            return "userFrequencyDescription";
                        case "NpvrQuotaInSeconds":
                            return "npvrQuotaInSeconds";
                        case "UsersLimit":
                            return "usersLimit";
                        case "DeviceFamiliesLimitations":
                            return "deviceFamiliesLimitations";
                    }
                    break;
                    
                case "KalturaDevicePin":
                    switch(property.Name)
                    {
                        case "Pin":
                            return "pin";
                    }
                    break;
                    
                case "KalturaMultilingualStringValueArray":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
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
                    
                case "KalturaApiArgumentPermissionItem":
                    switch(property.Name)
                    {
                        case "Service":
                            return "service";
                        case "Action":
                            return "action";
                        case "Parameter":
                            return "parameter";
                    }
                    break;
                    
                case "KalturaApiParameterPermissionItem":
                    switch(property.Name)
                    {
                        case "Object":
                            return "object";
                        case "Parameter":
                            return "parameter";
                        case "Action":
                            return "action";
                    }
                    break;
                    
                case "KalturaMultilingualString":
                    switch(property.Name)
                    {
                        case "Values":
                            return "values";
                    }
                    break;
                    
                case "KalturaApiActionPermissionItem":
                    switch(property.Name)
                    {
                        case "Service":
                            return "service";
                        case "Action":
                            return "action";
                    }
                    break;
                    
                case "KalturaAppToken":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Expiry":
                            return "expiry";
                        case "PartnerId":
                            return "partnerId";
                        case "SessionDuration":
                            return "sessionDuration";
                        case "HashType":
                            return "hashType";
                        case "SessionPrivileges":
                            return "sessionPrivileges";
                        case "SessionType":
                            return "sessionType";
                        case "Status":
                            return "status";
                        case "Token":
                            return "token";
                        case "SessionUserId":
                            return "sessionUserId";
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
                    
                case "KalturaGroupPermission":
                    switch(property.Name)
                    {
                        case "Group":
                            return "group";
                    }
                    break;
                    
                case "KalturaIdentifierTypeFilter":
                    switch(property.Name)
                    {
                        case "Identifier":
                            return "identifier";
                        case "By":
                            return "by";
                    }
                    break;
                    
                case "KalturaTransaction":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "PGReferenceID":
                            return "paymentGatewayReferenceId";
                        case "PGResponseID":
                            return "paymentGatewayResponseId";
                        case "State":
                            return "state";
                        case "FailReasonCode":
                            return "failReasonCode";
                        case "CreatedAt":
                            return "createdAt";
                    }
                    break;
                    
                case "KalturaFilterPager":
                    switch(property.Name)
                    {
                        case "PageSize":
                            return "pageSize";
                        case "PageIndex":
                            return "pageIndex";
                    }
                    break;
                    
                case "KalturaTransactionsFilter":
                    switch(property.Name)
                    {
                        case "By":
                            return "by";
                        case "StartDate":
                            return "startDate";
                        case "EndDate":
                            return "endDate";
                    }
                    break;
                    
                case "KalturaDeviceRegistrationStatusHolder":
                    switch(property.Name)
                    {
                        case "Status":
                            return "status";
                    }
                    break;
                    
                case "KalturaRuleFilter":
                    switch(property.Name)
                    {
                        case "By":
                            return "by";
                    }
                    break;
                    
                case "KalturaClientConfiguration":
                    switch(property.Name)
                    {
                        case "ClientTag":
                            return "clientTag";
                        case "ApiVersion":
                            return "apiVersion";
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
                        case "PartnerID":
                            return "partnerId";
                        case "UserID":
                            return "userId";
                        case "Language":
                            return "language";
                        case "KS":
                            return "ks";
                    }
                    break;
                    
                case "KalturaStringValueArray":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaUserRoleListResponse":
                    switch(property.Name)
                    {
                        case "UserRoles":
                            return "objects";
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
                    
                case "KalturaPlayerAssetData":
                    switch(property.Name)
                    {
                        case "averageBitRate":
                            return "averageBitrate";
                        case "totalBitRate":
                            return "totalBitrate";
                        case "currentBitRate":
                            return "currentBitrate";
                    }
                    break;
                    
                case "KalturaReminder":
                    switch(property.Name)
                    {
                        case "Name":
                            return "name";
                        case "Id":
                            return "id";
                    }
                    break;
                    
                case "KalturaAssetReminder":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "assetId";
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
                    
                case "KalturaInboxMessageResponse":
                    switch(property.Name)
                    {
                        case "InboxMessages":
                            return "objects";
                    }
                    break;
                    
                case "KalturaInboxMessageListResponse":
                    switch(property.Name)
                    {
                        case "InboxMessages":
                            return "objects";
                    }
                    break;
                    
                case "KalturaAnnouncement":
                    switch(property.Name)
                    {
                        case "Name":
                            return "name";
                        case "Message":
                            return "message";
                        case "Enabled":
                            return "enabled";
                        case "StartTime":
                            return "startTime";
                        case "Timezone":
                            return "timezone";
                        case "Status":
                            return "status";
                        case "Recipients":
                            return "recipients";
                        case "Id":
                            return "id";
                    }
                    break;
                    
                case "KalturaFeed":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "assetId";
                    }
                    break;
                    
                case "KalturaFollowDataBase":
                    switch(property.Name)
                    {
                        case "AnnouncementId":
                            return "announcementId";
                        case "Status":
                            return "status";
                        case "Title":
                            return "title";
                        case "Timestamp":
                            return "timestamp";
                        case "FollowPhrase":
                            return "followPhrase";
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
                    
                case "KalturaInboxMessage":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Message":
                            return "message";
                        case "Status":
                            return "status";
                        case "Type":
                            return "type";
                        case "CreatedAt":
                            return "createdAt";
                        case "Url":
                            return "url";
                    }
                    break;
                    
                case "KalturaInboxMessageFilter":
                    switch(property.Name)
                    {
                        case "TypeIn":
                            return "typeIn";
                        case "CreatedAtGreaterThanOrEqual":
                            return "createdAtGreaterThanOrEqual";
                        case "CreatedAtLessThanOrEqual":
                            return "createdAtLessThanOrEqual";
                    }
                    break;
                    
                case "KalturaMessageTemplate":
                    switch(property.Name)
                    {
                        case "Message":
                            return "message";
                        case "DateFormat":
                            return "dateFormat";
                        case "AssetType":
                            return "assetType";
                        case "Sound":
                            return "sound";
                        case "Action":
                            return "action";
                        case "URL":
                            return "url";
                    }
                    break;
                    
                case "KalturaListFollowDataTvSeriesResponse":
                    switch(property.Name)
                    {
                        case "FollowDataList":
                            return "objects";
                    }
                    break;
                    
                case "KalturaFollowTvSeriesListResponse":
                    switch(property.Name)
                    {
                        case "FollowDataList":
                            return "objects";
                    }
                    break;
                    
                case "KalturaAnnouncementListResponse":
                    switch(property.Name)
                    {
                        case "Announcements":
                            return "objects";
                    }
                    break;
                    
                case "KalturaMessageAnnouncementListResponse":
                    switch(property.Name)
                    {
                        case "Announcements":
                            return "objects";
                    }
                    break;
                    
                case "KalturaNotificationsSettings":
                    switch(property.Name)
                    {
                        case "PushNotificationEnabled":
                            return "pushNotificationEnabled";
                        case "PushFollowEnabled":
                            return "pushFollowEnabled";
                    }
                    break;
                    
                case "KalturaNotificationsPartnerSettings":
                    switch(property.Name)
                    {
                        case "PushNotificationEnabled":
                            return "pushNotificationEnabled";
                        case "PushSystemAnnouncementsEnabled":
                            return "pushSystemAnnouncementsEnabled";
                        case "PushStartHour":
                            return "pushStartHour";
                        case "PushEndHour":
                            return "pushEndHour";
                        case "InboxEnabled":
                            return "inboxEnabled";
                        case "MessageTTLDays":
                            return "messageTTLDays";
                        case "AutomaticIssueFollowNotification":
                            return "automaticIssueFollowNotification";
                        case "TopicExpirationDurationDays":
                            return "topicExpirationDurationDays";
                        case "ReminderEnabled":
                            return "reminderEnabled";
                        case "ReminderOffset":
                            return "reminderOffsetSec";
                        case "PushAdapterUrl":
                            return "pushAdapterUrl";
                    }
                    break;
                    
                case "KalturaPersonalFollowFeedResponse":
                    switch(property.Name)
                    {
                        case "PersonalFollowFeed":
                            return "objects";
                    }
                    break;
                    
                case "KalturaPersonalFeedListResponse":
                    switch(property.Name)
                    {
                        case "PersonalFollowFeed":
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
                    
                case "KalturaTopic":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "SubscribersAmount":
                            return "subscribersAmount";
                        case "AutomaticIssueNotification":
                            return "automaticIssueNotification";
                        case "LastMessageSentDateSec":
                            return "lastMessageSentDateSec";
                    }
                    break;
                    
                case "KalturaTopicResponse":
                    switch(property.Name)
                    {
                        case "Topics":
                            return "objects";
                    }
                    break;
                    
                case "KalturaTopicListResponse":
                    switch(property.Name)
                    {
                        case "Topics":
                            return "objects";
                    }
                    break;
                    
                case "KalturaBillingPartnerConfig":
                    switch(property.Name)
                    {
                        case "Value":
                            return "value";
                        case "PartnerConfigurationType":
                            return "partnerConfigurationType";
                        case "Type":
                            return "type";
                    }
                    break;
                    
                case "KalturaProductPrice":
                    switch(property.Name)
                    {
                        case "ProductId":
                            return "productId";
                        case "ProductType":
                            return "productType";
                        case "Price":
                            return "price";
                        case "PurchaseStatus":
                            return "purchaseStatus";
                    }
                    break;
                    
                case "KalturaPpvPrice":
                    switch(property.Name)
                    {
                        case "FileId":
                            return "fileId";
                        case "PPVModuleId":
                            return "ppvModuleId";
                        case "IsSubscriptionOnly":
                            return "isSubscriptionOnly";
                        case "FullPrice":
                            return "fullPrice";
                        case "SubscriptionId":
                            return "subscriptionId";
                        case "CollectionId":
                            return "collectionId";
                        case "PrePaidId":
                            return "prePaidId";
                        case "PPVDescriptions":
                            return "ppvDescriptions";
                        case "PurchaseUserId":
                            return "purchaseUserId";
                        case "PurchasedMediaFileId":
                            return "purchasedMediaFileId";
                        case "RelatedMediaFileIds":
                            return "relatedMediaFileIds";
                        case "StartDate":
                            return "startDate";
                        case "EndDate":
                            return "endDate";
                        case "DiscountEndDate":
                            return "discountEndDate";
                        case "FirstDeviceName":
                            return "firstDeviceName";
                        case "IsInCancelationPeriod":
                            return "isInCancelationPeriod";
                        case "ProductCode":
                            return "ppvProductCode";
                    }
                    break;
                    
                case "KalturaProductPriceListResponse":
                    switch(property.Name)
                    {
                        case "ProductsPrices":
                            return "objects";
                    }
                    break;
                    
                case "KalturaSubscriptionFilter":
                    switch(property.Name)
                    {
                        case "SubscriptionIdIn":
                            return "subscriptionIdIn";
                        case "MediaFileIdEqual":
                            return "mediaFileIdEqual";
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
                    
                case "KalturaCoupon":
                    switch(property.Name)
                    {
                        case "CouponsGroup":
                            return "couponsGroup";
                        case "Status":
                            return "status";
                    }
                    break;
                    
                case "KalturaItemPriceListResponse":
                    switch(property.Name)
                    {
                        case "ItemPrice":
                            return "objects";
                    }
                    break;
                    
                case "KalturaPpv":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "Price":
                            return "price";
                        case "FileTypes":
                            return "fileTypes";
                        case "DiscountModule":
                            return "discountModules";
                        case "CouponsGroup":
                            return "couponsGroup";
                        case "Descriptions":
                            return "descriptions";
                        case "ProductCode":
                            return "productCode";
                        case "IsSubscriptionOnly":
                            return "isSubscriptionOnly";
                        case "FirstDeviceLimitation":
                            return "firstDeviceLimitation";
                        case "UsageModule":
                            return "usageModule";
                    }
                    break;
                    
                case "KalturaPPVItemPriceDetails":
                    switch(property.Name)
                    {
                        case "PPVModuleId":
                            return "ppvModuleId";
                        case "IsSubscriptionOnly":
                            return "isSubscriptionOnly";
                        case "Price":
                            return "price";
                        case "FullPrice":
                            return "fullPrice";
                        case "PurchaseStatus":
                            return "purchaseStatus";
                        case "SubscriptionId":
                            return "subscriptionId";
                        case "CollectionId":
                            return "collectionId";
                        case "PrePaidId":
                            return "prePaidId";
                        case "PPVDescriptions":
                            return "ppvDescriptions";
                        case "PurchaseUserId":
                            return "purchaseUserId";
                        case "PurchasedMediaFileId":
                            return "purchasedMediaFileId";
                        case "RelatedMediaFileIds":
                            return "relatedMediaFileIds";
                        case "StartDate":
                            return "startDate";
                        case "EndDate":
                            return "endDate";
                        case "DiscountEndDate":
                            return "discountEndDate";
                        case "FirstDeviceName":
                            return "firstDeviceName";
                        case "IsInCancelationPeriod":
                            return "isInCancelationPeriod";
                        case "ProductCode":
                            return "ppvProductCode";
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
                    
                case "KalturaCouponsGroup":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "Descriptions":
                            return "descriptions";
                        case "StartDate":
                            return "startDate";
                        case "EndDate":
                            return "endDate";
                        case "MaxUsesNumber":
                            return "maxUsesNumber";
                        case "MaxUsesNumberOnRenewableSub":
                            return "maxUsesNumberOnRenewableSub";
                        case "CouponGroupType":
                            return "couponGroupType";
                    }
                    break;
                    
                case "KalturaDiscountModule":
                    switch(property.Name)
                    {
                        case "Percent":
                            return "percent";
                        case "StartDate":
                            return "startDate";
                        case "EndDate":
                            return "endDate";
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
                    
                case "KalturaPreviewModule":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "LifeCycle":
                            return "lifeCycle";
                        case "NonRenewablePeriod":
                            return "nonRenewablePeriod";
                    }
                    break;
                    
                case "KalturaPriceDetails":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Price":
                            return "price";
                        case "Descriptions":
                            return "descriptions";
                    }
                    break;
                    
                case "KalturaUsageModule":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "MaxViewsNumber":
                            return "maxViewsNumber";
                        case "ViewLifeCycle":
                            return "viewLifeCycle";
                        case "FullLifeCycle":
                            return "fullLifeCycle";
                        case "CouponId":
                            return "couponId";
                        case "WaiverPeriod":
                            return "waiverPeriod";
                        case "IsWaiverEnabled":
                            return "isWaiverEnabled";
                        case "IsOfflinePlayback":
                            return "isOfflinePlayback";
                    }
                    break;
                    
                case "KalturaPricePlan":
                    switch(property.Name)
                    {
                        case "IsRenewable":
                            return "isRenewable";
                        case "RenewalsNumber":
                            return "renewalsNumber";
                        case "PriceId":
                            return "priceId";
                        case "DiscountId":
                            return "discountId";
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
                        case "Ids":
                            return "ids";
                        case "By":
                            return "by";
                    }
                    break;
                    
                case "KalturaProductsPriceListResponse":
                    switch(property.Name)
                    {
                        case "ProductsPrices":
                            return "objects";
                    }
                    break;
                    
                case "KalturaEntitlementListResponse":
                    switch(property.Name)
                    {
                        case "Entitlements":
                            return "objects";
                    }
                    break;
                    
                case "KalturaHouseholdDevice":
                    switch(property.Name)
                    {
                        case "HouseholdId":
                            return "householdId";
                        case "Udid":
                            return "udid";
                        case "Name":
                            return "name";
                        case "Brand":
                            return "brand";
                        case "BrandId":
                            return "brandId";
                        case "ActivatedOn":
                            return "activatedOn";
                        case "State":
                            return "state";
                        case "Status":
                            return "status";
                        case "DeviceFamilyId":
                            return "deviceFamilyId";
                    }
                    break;
                    
                case "KalturaDeviceFamilyBase":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "DeviceLimit":
                            return "deviceLimit";
                        case "ConcurrentLimit":
                            return "concurrentLimit";
                    }
                    break;
                    
                case "KalturaDeviceFamily":
                    switch(property.Name)
                    {
                        case "Devices":
                            return "devices";
                    }
                    break;
                    
                case "KalturaHouseholdDeviceFamilyLimitations":
                    switch(property.Name)
                    {
                        case "Frequency":
                            return "frequency";
                        case "DeviceLimit":
                            return "deviceLimit";
                        case "ConcurrentLimit":
                            return "concurrentLimit";
                    }
                    break;
                    
                case "KalturaHousehold":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "Description":
                            return "description";
                        case "ExternalId":
                            return "externalId";
                        case "HouseholdLimitationsId":
                            return "householdLimitationsId";
                        case "DevicesLimit":
                            return "devicesLimit";
                        case "UsersLimit":
                            return "usersLimit";
                        case "ConcurrentLimit":
                            return "concurrentLimit";
                        case "Users":
                            return "users";
                        case "MasterUsers":
                            return "masterUsers";
                        case "DefaultUsers":
                            return "defaultUsers";
                        case "PendingUsers":
                            return "pendingUsers";
                        case "RegionId":
                            return "regionId";
                        case "State":
                            return "state";
                        case "IsFrequencyEnabled":
                            return "isFrequencyEnabled";
                        case "FrequencyNextDeviceAction":
                            return "frequencyNextDeviceAction";
                        case "FrequencyNextUserAction":
                            return "frequencyNextUserAction";
                        case "Restriction":
                            return "restriction";
                        case "DeviceFamilies":
                            return "deviceFamilies";
                    }
                    break;
                    
                case "KalturaHomeNetworkListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaHomeNetwork":
                    switch(property.Name)
                    {
                        case "ExternalId":
                            return "externalId";
                        case "Name":
                            return "name";
                        case "Description":
                            return "description";
                        case "IsActive":
                            return "isActive";
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
                    
                case "KalturaSubscription":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Channels":
                            return "channels";
                        case "StartDate":
                            return "startDate";
                        case "EndDate":
                            return "endDate";
                        case "FileTypes":
                            return "fileTypes";
                        case "IsRenewable":
                            return "isRenewable";
                        case "RenewalsNumber":
                            return "renewalsNumber";
                        case "IsInfiniteRenewal":
                            return "isInfiniteRenewal";
                        case "Price":
                            return "price";
                        case "DiscountModule":
                            return "discountModule";
                        case "CouponsGroup":
                            return "couponsGroup";
                        case "Name":
                            return "name";
                        case "Names":
                            return "names";
                        case "Description":
                            return "description";
                        case "Descriptions":
                            return "descriptions";
                        case "MediaId":
                            return "mediaId";
                        case "ProrityInOrder":
                            return "prorityInOrder";
                        case "ProductCode":
                            return "productCode";
                        case "PricePlans":
                            return "pricePlans";
                        case "PreviewModule":
                            return "previewModule";
                        case "HouseholdLimitationsId":
                            return "householdLimitationsId";
                        case "GracePeriodMinutes":
                            return "gracePeriodMinutes";
                        case "PremiumServices":
                            return "premiumServices";
                        case "MaxViewsNumber":
                            return "maxViewsNumber";
                        case "ViewLifeCycle":
                            return "viewLifeCycle";
                        case "WaiverPeriod":
                            return "waiverPeriod";
                        case "IsWaiverEnabled":
                            return "isWaiverEnabled";
                        case "UserTypes":
                            return "userTypes";
                    }
                    break;
                    
                case "KalturaSocialCommentFilter":
                    switch(property.Name)
                    {
                        case "AssetIdEqual":
                            return "assetIdEqual";
                        case "AssetTypeEqual":
                            return "assetTypeEqual";
                        case "SocialPlatformEqual":
                            return "socialPlatformEqual";
                        case "CreateDateGreaterThan":
                            return "createDateGreaterThan";
                    }
                    break;
                    
                case "KalturaSocialNetworkComment":
                    switch(property.Name)
                    {
                        case "LikeCounter":
                            return "likeCounter";
                        case "AuthorImageUrl":
                            return "authorImageUrl";
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
                    
                case "KalturaSocialCommentListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaActionPermissionItem":
                    switch(property.Name)
                    {
                        case "Network":
                            return "network";
                        case "ActionPrivacy":
                            return "actionPrivacy";
                        case "Privacy":
                            return "privacy";
                    }
                    break;
                    
                case "KalturaSocial":
                    switch(property.Name)
                    {
                        case "ID":
                            return "id";
                        case "Name":
                            return "name";
                        case "FirstName":
                            return "firstName";
                        case "LastName":
                            return "lastName";
                        case "Email":
                            return "email";
                        case "Gender":
                            return "gender";
                        case "UserId":
                            return "userId";
                        case "Birthday":
                            return "birthday";
                        case "Status":
                            return "status";
                        case "PictureUrl":
                            return "pictureUrl";
                    }
                    break;
                    
                case "KalturaSocialAction":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "ActionType":
                            return "actionType";
                        case "Time":
                            return "time";
                        case "AssetId":
                            return "assetId";
                        case "AssetType":
                            return "assetType";
                        case "Url":
                            return "url";
                    }
                    break;
                    
                case "KalturaSocialActionRate":
                    switch(property.Name)
                    {
                        case "Rate":
                            return "rate";
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
                        case "UserFullName":
                            return "userFullName";
                        case "UserPictureUrl":
                            return "userPictureUrl";
                        case "SocialAction":
                            return "socialAction";
                    }
                    break;
                    
                case "KalturaSocialFriendActivityListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaSocialFriendActivityFilter":
                    switch(property.Name)
                    {
                        case "AssetIdEqual":
                            return "assetIdEqual";
                        case "AssetTypeEqual":
                            return "assetTypeEqual";
                        case "ActionTypeIn":
                            return "actionTypeIn";
                    }
                    break;
                    
                case "KalturaSocialResponse":
                    switch(property.Name)
                    {
                        case "Status":
                            return "status";
                        case "UserId":
                            return "userId";
                        case "KalturaName":
                            return "kalturaUsername";
                        case "SocialNetworkUsername":
                            return "socialUsername";
                        case "Pic":
                            return "pic";
                        case "Data":
                            return "data";
                        case "MinFriends":
                            return "minFriendsLimitation";
                        case "Token":
                            return "token";
                        case "SocialUser":
                            return "socialUser";
                    }
                    break;
                    
                case "KalturaSocialUser":
                    switch(property.Name)
                    {
                        case "ID":
                            return "id";
                        case "Name":
                            return "name";
                        case "FirstName":
                            return "firstName";
                        case "LastName":
                            return "lastName";
                        case "Email":
                            return "email";
                        case "Gender":
                            return "gender";
                        case "UserId":
                            return "userId";
                        case "Birthday":
                            return "birthday";
                    }
                    break;
                    
                case "KalturaTimeShiftedTvPartnerSettings":
                    switch(property.Name)
                    {
                        case "CatchUpEnabled":
                            return "catchUpEnabled";
                        case "CdvrEnabled":
                            return "cdvrEnabled";
                        case "StartOverEnabled":
                            return "startOverEnabled";
                        case "TrickPlayEnabled":
                            return "trickPlayEnabled";
                        case "RecordingScheduleWindowEnabled":
                            return "recordingScheduleWindowEnabled";
                        case "ProtectionEnabled":
                            return "protectionEnabled";
                        case "CatchUpBufferLength":
                            return "catchUpBufferLength";
                        case "TrickPlayBufferLength":
                            return "trickPlayBufferLength";
                        case "RecordingScheduleWindow":
                            return "recordingScheduleWindow";
                        case "PaddingBeforeProgramStarts":
                            return "paddingBeforeProgramStarts";
                        case "PaddingAfterProgramEnds":
                            return "paddingAfterProgramEnds";
                        case "ProtectionPeriod":
                            return "protectionPeriod";
                        case "ProtectionQuotaPercentage":
                            return "protectionQuotaPercentage";
                        case "RecordingLifetimePeriod":
                            return "recordingLifetimePeriod";
                        case "CleanupNoticePeriod":
                            return "cleanupNoticePeriod";
                        case "SeriesRecordingEnabled":
                            return "seriesRecordingEnabled";
                        case "NonEntitledChannelPlaybackEnabled":
                            return "nonEntitledChannelPlaybackEnabled";
                        case "NonExistingChannelPlaybackEnabled":
                            return "nonExistingChannelPlaybackEnabled";
                        case "QuotaOveragePolicy":
                            return "quotaOveragePolicy";
                        case "ProtectionPolicy":
                            return "protectionPolicy";
                    }
                    break;
                    
                case "KalturaSocialActionFilter":
                    switch(property.Name)
                    {
                        case "AssetIdIn":
                            return "assetIdIn";
                        case "AssetTypeEqual":
                            return "assetTypeEqual";
                        case "ActionTypeIn":
                            return "actionTypeIn";
                    }
                    break;
                    
                case "KalturaSocialUserConfig":
                    switch(property.Name)
                    {
                        case "PermissionItems":
                            return "actionPermissionItems";
                    }
                    break;
                    
                case "KalturaSocialActionListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaUserSocialActionResponse":
                    switch(property.Name)
                    {
                        case "SocialAction":
                            return "socialAction";
                        case "NetworkStatus":
                            return "failStatus";
                    }
                    break;
                    
                case "KalturaNetworkActionStatus":
                    switch(property.Name)
                    {
                        case "Status":
                            return "status";
                        case "Network":
                            return "network";
                    }
                    break;
                    
                case "KalturaFavoriteFilter":
                    switch(property.Name)
                    {
                        case "MediaTypeIn":
                            return "mediaTypeIn";
                        case "MediaTypeEqual":
                            return "mediaTypeEqual";
                        case "UDID":
                            return "udid";
                        case "MediaIds":
                            return "media_ids";
                        case "MediaIdIn":
                            return "mediaIdIn";
                    }
                    break;
                    
                case "KalturaLoginResponse":
                    switch(property.Name)
                    {
                        case "User":
                            return "user";
                        case "LoginSession":
                            return "loginSession";
                    }
                    break;
                    
                case "KalturaOTTUserFilter":
                    switch(property.Name)
                    {
                        case "UsernameEqual":
                            return "usernameEqual";
                        case "ExternalIdEqual":
                            return "externalIdEqual";
                        case "IdIn":
                            return "idIn";
                    }
                    break;
                    
                case "KalturaUserAssetsListFilter":
                    switch(property.Name)
                    {
                        case "By":
                            return "by";
                        case "ListTypeEqual":
                            return "listTypeEqual";
                        case "AssetTypeEqual":
                            return "assetTypeEqual";
                    }
                    break;
                    
                case "KalturaFavorite":
                    switch(property.Name)
                    {
                        case "Asset":
                            return "asset";
                        case "AssetId":
                            return "assetId";
                        case "ExtraData":
                            return "extraData";
                        case "CreateDate":
                            return "createDate";
                    }
                    break;
                    
                case "KalturaFavoriteListResponse":
                    switch(property.Name)
                    {
                        case "Favorites":
                            return "objects";
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
                    
                case "KalturaBaseOTTUser":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Username":
                            return "username";
                        case "FirstName":
                            return "firstName";
                        case "LastName":
                            return "lastName";
                    }
                    break;
                    
                case "KalturaPinResponse":
                    switch(property.Name)
                    {
                        case "PIN":
                            return "pin";
                        case "Origin":
                            return "origin";
                        case "Type":
                            return "type";
                    }
                    break;
                    
                case "KalturaPurchaseSettingsResponse":
                    switch(property.Name)
                    {
                        case "PurchaseSettingsType":
                            return "purchaseSettingsType";
                    }
                    break;
                    
                case "KalturaOTTCategory":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "ParentCategoryId":
                            return "parentCategoryId";
                        case "ChildCategories":
                            return "childCategories";
                        case "Channels":
                            return "channels";
                        case "Images":
                            return "images";
                    }
                    break;
                    
                case "KalturaChannel":
                    switch(property.Name)
                    {
                        case "Description":
                            return "description";
                        case "Images":
                            return "images";
                        case "AssetTypes":
                            return "assetTypes";
                        case "MediaTypes":
                            return "media_types";
                        case "FilterExpression":
                            return "filterExpression";
                        case "IsActive":
                            return "isActive";
                        case "Order":
                            return "order";
                    }
                    break;
                    
                case "KalturaParentalRule":
                    switch(property.Name)
                    {
                        case "mediaTagTypeId":
                            return "mediaTag";
                        case "epgTagTypeId":
                            return "epgTag";
                        case "Origin":
                            return "origin";
                    }
                    break;
                    
                case "KalturaCountry":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "Name":
                            return "name";
                        case "Code":
                            return "code";
                    }
                    break;
                    
                case "KalturaUserLoginPin":
                    switch(property.Name)
                    {
                        case "PinCode":
                            return "pinCode";
                        case "ExpirationTime":
                            return "expirationTime";
                        case "UserId":
                            return "userId";
                    }
                    break;
                    
                case "KalturaOTTUser":
                    switch(property.Name)
                    {
                        case "HouseholdID":
                            return "householdId";
                        case "Email":
                            return "email";
                        case "Address":
                            return "address";
                        case "City":
                            return "city";
                        case "Country":
                            return "country";
                        case "CountryId":
                            return "countryId";
                        case "Zip":
                            return "zip";
                        case "Phone":
                            return "phone";
                        case "FacebookId":
                            return "facebookId";
                        case "FacebookImage":
                            return "facebookImage";
                        case "AffiliateCode":
                            return "affiliateCode";
                        case "FacebookToken":
                            return "facebookToken";
                        case "ExternalId":
                            return "externalId";
                        case "UserType":
                            return "userType";
                        case "DynamicData":
                            return "dynamicData";
                        case "IsHouseholdMaster":
                            return "isHouseholdMaster";
                        case "SuspentionState":
                            return "suspentionState";
                        case "SuspensionState":
                            return "suspensionState";
                        case "UserState":
                            return "userState";
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
                    
                case "KalturaUserAssetsListItem":
                    switch(property.Name)
                    {
                        case "Id":
                            return "id";
                        case "OrderIndex":
                            return "orderIndex";
                        case "Type":
                            return "type";
                        case "UserId":
                            return "userId";
                        case "ListType":
                            return "listType";
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
                        case "Id":
                            return "id";
                        case "Description":
                            return "description";
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
                    
                case "KalturaAssetInfo":
                    switch(property.Name)
                    {
                        case "Metas":
                            return "metas";
                        case "MetasNew":
                            return "metasNew";
                        case "Tags":
                            return "tags";
                        case "StartDate":
                            return "startDate";
                        case "EndDate":
                            return "endDate";
                        case "ExtraParams":
                            return "extraParams";
                    }
                    break;
                    
                case "KalturaAssetStatistics":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "assetId";
                        case "Likes":
                            return "likes";
                        case "Views":
                            return "views";
                        case "RatingCount":
                            return "ratingCount";
                        case "Rating":
                            return "rating";
                        case "BuzzAvgScore":
                            return "buzzScore";
                    }
                    break;
                    
                case "KalturaAssetStatisticsListResponse":
                    switch(property.Name)
                    {
                        case "AssetsStatistics":
                            return "objects";
                    }
                    break;
                    
                case "KalturaBuzzScore":
                    switch(property.Name)
                    {
                        case "NormalizedAvgScore":
                            return "normalizedAvgScore";
                        case "UpdateDate":
                            return "updateDate";
                        case "AvgScore":
                            return "avgScore";
                    }
                    break;
                    
                case "KalturaMediaImage":
                    switch(property.Name)
                    {
                        case "Ratio":
                            return "ratio";
                        case "Width":
                            return "width";
                        case "Height":
                            return "height";
                        case "Url":
                            return "url";
                        case "Version":
                            return "version";
                        case "Id":
                            return "id";
                        case "IsDefault":
                            return "isDefault";
                    }
                    break;
                    
                case "KalturaSlimAssetInfoWrapper":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaAssetHistoryListResponse":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaAssetHistory":
                    switch(property.Name)
                    {
                        case "AssetId":
                            return "assetId";
                        case "AssetType":
                            return "assetType";
                        case "Position":
                            return "position";
                        case "Duration":
                            return "duration";
                        case "LastWatched":
                            return "watchedDate";
                        case "IsFinishedWatching":
                            return "finishedWatching";
                    }
                    break;
                    
                case "KalturaWatchHistoryAssetWrapper":
                    switch(property.Name)
                    {
                        case "Objects":
                            return "objects";
                    }
                    break;
                    
                case "KalturaWatchHistoryAsset":
                    switch(property.Name)
                    {
                        case "Asset":
                            return "asset";
                        case "Position":
                            return "position";
                        case "Duration":
                            return "duration";
                        case "LastWatched":
                            return "watchedDate";
                        case "IsFinishedWatching":
                            return "finishedWatching";
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
                    
                case "ConfigurationsController":
                    switch(action.Name)
                    {
                        case "ServeByDevice":
                            return;
                            
                    }
                    break;
                    
                case "SystemController":
                    switch(action.Name)
                    {
                        case "Ping":
                            return;
                            
                        case "GetTime":
                            return;
                            
                        case "GetVersion":
                            return;
                            
                    }
                    break;
                    
                case "VersionController":
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
                    
                case "BaseCategoryController":
                    return;
                    
                case "MultiRequestController":
                    return;
                    
                case "ServiceController":
                    return;
                    
                case "SocialController":
                    switch(action.Name)
                    {
                        case "Login":
                            return;
                            
                    }
                    break;
                    
                case "OttUserController":
                    switch(action.Name)
                    {
                        case "AnonymousLogin":
                            return;
                            
                        case "LoginWithPin":
                            return;
                            
                        case "Login":
                            return;
                            
                        case "RefreshSession":
                            silent = true;
                            break;
                            
                        case "FacebookLogin":
                            return;
                            
                        case "Register":
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
                            
                        case "Activate":
                            return;
                            
                        case "ResendActivationToken":
                            return;
                            
                    }
                    break;
                    
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
