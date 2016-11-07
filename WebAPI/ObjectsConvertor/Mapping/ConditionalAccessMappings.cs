using System;
using AutoMapper;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Utils;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Catalog;
using ApiObjects;
using ApiObjects.SearchObjects;
using ApiObjects.TimeShiftedTv;
using ConditionalAccess;
using Pricing;
using WebAPI.Models.ConditionalAccess;
using WebAPI.Models.General;
using Billing;
using ConditionalAccess.Response;
using WebAPI.Models.Pricing;
using WebAPI.Models.Catalog;
using ApiObjects.Billing;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class ConditionalAccessMappings
    {
        public static void RegisterMappings()
        {
            // Entitlements(WS) to  WebAPI.Entitlement(REST)
            #region Entitlement
            Mapper.CreateMap<Entitlement, KalturaEntitlement>()
               .ForMember(dest => dest.EntitlementId, opt => opt.MapFrom(src => src.entitlementId))
               .ForMember(dest => dest.CurrentUses, opt => opt.MapFrom(src => src.currentUses))
               .ForMember(dest => dest.CurrentDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.currentDate)))
               .ForMember(dest => dest.LastViewDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.lastViewDate)))
               .ForMember(dest => dest.PurchaseDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.purchaseDate)))
               .ForMember(dest => dest.PurchaseId, opt => opt.MapFrom(src => src.purchaseID))
               .ForMember(dest => dest.DeviceUDID, opt => opt.MapFrom(src => src.deviceUDID))
               .ForMember(dest => dest.DeviceName, opt => opt.MapFrom(src => src.deviceName))
               .ForMember(dest => dest.IsCancelationWindowEnabled, opt => opt.MapFrom(src => src.cancelWindow))
               .ForMember(dest => dest.MaxUses, opt => opt.MapFrom(src => src.maxUses))
               .ForMember(dest => dest.NextRenewalDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.nextRenewalDate)))
               .ForMember(dest => dest.IsRenewableForPurchase, opt => opt.MapFrom(src => src.recurringStatus))
               .ForMember(dest => dest.IsRenewable, opt => opt.MapFrom(src => src.isRenewable))
               .ForMember(dest => dest.MediaFileId, opt => opt.MapFrom(src => src.mediaFileID))
               .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.type))
               .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.endDate)))
               .ForMember(dest => dest.PaymentMethod, opt => opt.MapFrom(src => src.paymentMethod))
               .ForMember(dest => dest.IsInGracePeriod, opt => opt.MapFrom(src => src.IsInGracePeriod))
               .ForMember(dest => dest.MediaId, opt => opt.MapFrom(src => src.mediaID));
            #endregion

            // BillingTransactions(WS) to  BillingTransactions(REST)
            #region Billing Transaction Container
            Mapper.CreateMap<BillingTransactionContainer, KalturaBillingTransaction>()
               .ForMember(dest => dest.actionDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dtActionDate)))
               .ForMember(dest => dest.startDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dtStartDate)))
               .ForMember(dest => dest.endDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dtEndDate)))

               .ForMember(dest => dest.billingAction, opt => opt.MapFrom(src => src.m_eBillingAction))
               .ForMember(dest => dest.itemType, opt => opt.MapFrom(src => src.m_eItemType))
               .ForMember(dest => dest.paymentMethod, opt => opt.MapFrom(src => src.m_ePaymentMethod))
               .ForMember(dest => dest.isRecurring, opt => opt.MapFrom(src => src.m_bIsRecurring))
               .ForMember(dest => dest.billingProviderRef, opt => opt.MapFrom(src => src.m_nBillingProviderRef))
               .ForMember(dest => dest.purchaseID, opt => opt.MapFrom(src => src.m_nPurchaseID))
               .ForMember(dest => dest.purchasedItemName, opt => opt.MapFrom(src => src.m_sPurchasedItemName))
               .ForMember(dest => dest.purchasedItemCode, opt => opt.MapFrom(src => src.m_sPurchasedItemCode))
               .ForMember(dest => dest.recieptCode, opt => opt.MapFrom(src => src.m_sRecieptCode))
               .ForMember(dest => dest.remarks, opt => opt.MapFrom(src => src.m_sRemarks))
               .ForMember(dest => dest.paymentMethodExtraDetails, opt => opt.MapFrom(src => src.m_sPaymentMethodExtraDetails))
               .ForMember(dest => dest.price, opt => opt.MapFrom(src => src.m_Price))
               ;

            #endregion

            // BillingTransactions(WS) to  BillingTransactions(REST)
            #region Billing Transaction List
            Mapper.CreateMap<BillingTransactionsResponse, KalturaBillingTransactionListResponse>()
               .ForMember(dest => dest.TotalCount, opt => opt.MapFrom(src => src.m_nTransactionsCount))
               .ForMember(dest => dest.transactions, opt => opt.MapFrom(src => src.m_Transactions));
            #endregion

            #region Price
            Mapper.CreateMap<Price, KalturaPrice>()
              .ForMember(dest => dest.Amount, opt => opt.MapFrom(src => src.m_dPrice))
              .ForMember(dest => dest.Currency, opt => opt.MapFrom(src => src.m_oCurrency.m_sCurrencyCD3))
              .ForMember(dest => dest.CurrencySign, opt => opt.MapFrom(src => src.m_oCurrency.m_sCurrencySign));
            #endregion

            // BillingResponse
            #region Billing
            Mapper.CreateMap<BillingResponse, KalturaBillingResponse>()
               .ForMember(dest => dest.ReceiptCode, opt => opt.MapFrom(src => src.m_sRecieptCode))
               .ForMember(dest => dest.ExternalReceiptCode, opt => opt.MapFrom(src => src.m_sExternalReceiptCode));
            #endregion

            // TransactionResponse to KalturaTransactionResponse
            #region Transaction
            Mapper.CreateMap<TransactionResponse, KalturaTransaction>()
               .ForMember(dest => dest.PGReferenceID, opt => opt.MapFrom(src => src.PGReferenceID))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.TransactionID))
               .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State.ToString()))
               .ForMember(dest => dest.PGResponseID, opt => opt.MapFrom(src => src.PGResponseCode))
               .ForMember(dest => dest.FailReasonCode, opt => opt.MapFrom(src => src.FailReasonCode))
               .ForMember(dest => dest.CreatedAt, opt => opt.MapFrom(src => src.CreatedAt));
            #endregion

            #region Billing Transaction Container to User Billing Transaciton
            Mapper.CreateMap<BillingTransactionContainer, KalturaUserBillingTransaction>()
               .ForMember(dest => dest.actionDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dtActionDate)))
               .ForMember(dest => dest.startDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dtStartDate)))
               .ForMember(dest => dest.endDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dtEndDate)))

               .ForMember(dest => dest.billingAction, opt => opt.MapFrom(src => src.m_eBillingAction))
               .ForMember(dest => dest.itemType, opt => opt.MapFrom(src => src.m_eItemType))
               .ForMember(dest => dest.paymentMethod, opt => opt.MapFrom(src => src.m_ePaymentMethod))
               .ForMember(dest => dest.isRecurring, opt => opt.MapFrom(src => src.m_bIsRecurring))
               .ForMember(dest => dest.billingProviderRef, opt => opt.MapFrom(src => src.m_nBillingProviderRef))
               .ForMember(dest => dest.purchaseID, opt => opt.MapFrom(src => src.m_nPurchaseID))
               .ForMember(dest => dest.purchasedItemName, opt => opt.MapFrom(src => src.m_sPurchasedItemName))
               .ForMember(dest => dest.purchasedItemCode, opt => opt.MapFrom(src => src.m_sPurchasedItemCode))
               .ForMember(dest => dest.recieptCode, opt => opt.MapFrom(src => src.m_sRecieptCode))
               .ForMember(dest => dest.remarks, opt => opt.MapFrom(src => src.m_sRemarks))
               .ForMember(dest => dest.paymentMethodExtraDetails, opt => opt.MapFrom(src => src.m_sPaymentMethodExtraDetails))
               .ForMember(dest => dest.price, opt => opt.MapFrom(src => src.m_Price))
               ;

            #endregion

            #region TransactionHistoryContainer to KalturaUserBillingTransaction
            Mapper.CreateMap<TransactionHistoryContainer, KalturaUserBillingTransaction>()
               .ForMember(dest => dest.actionDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dtActionDate)))
               .ForMember(dest => dest.startDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dtStartDate)))
               .ForMember(dest => dest.endDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dtEndDate)))
               .ForMember(dest => dest.billingAction, opt => opt.MapFrom(src => src.m_eBillingAction))
               .ForMember(dest => dest.itemType, opt => opt.MapFrom(src => src.m_eItemType))
               .ForMember(dest => dest.paymentMethod, opt => opt.MapFrom(src => src.m_ePaymentMethod))
               .ForMember(dest => dest.isRecurring, opt => opt.MapFrom(src => src.m_bIsRecurring))
               .ForMember(dest => dest.billingProviderRef, opt => opt.MapFrom(src => src.m_nBillingProviderRef))
               .ForMember(dest => dest.purchaseID, opt => opt.MapFrom(src => src.m_nPurchaseID))
               .ForMember(dest => dest.purchasedItemName, opt => opt.MapFrom(src => src.m_sPurchasedItemName))
               .ForMember(dest => dest.purchasedItemCode, opt => opt.MapFrom(src => src.m_sPurchasedItemCode))
               .ForMember(dest => dest.recieptCode, opt => opt.MapFrom(src => src.m_sRecieptCode))
               .ForMember(dest => dest.remarks, opt => opt.MapFrom(src => src.m_sRemarks))
               .ForMember(dest => dest.paymentMethodExtraDetails, opt => opt.MapFrom(src => src.m_sPaymentMethodExtraDetails))
               .ForMember(dest => dest.price, opt => opt.MapFrom(src => src.m_Price))
               .ForMember(dest => dest.UserID, opt => opt.MapFrom(src => src.SiteGuid))
               .ForMember(dest => dest.UserFullName, opt => opt.MapFrom(src => src.UserFullName));

            #endregion

            //#region Domains Billing Transactions
            //Mapper.CreateMap<DomainsBillingTransactionsResponse, KalturaHouseholdsBillingTransactions>()
            //    .ForMember(dest => dest.DomainsBillingTransactions, opt => opt.MapFrom(src => src.billingTransactions));

            //#endregion

            //#region Domain Billing Transactions
            //Mapper.CreateMap<DomainBillingTransactionsResponse, >()
            //    .ForMember(dest => dest.UsersBillingTransactions, opt => opt.MapFrom(src => src.m_BillingTransactionResponses))
            //    .ForMember(dest => dest.HouseholdId, opt => opt.MapFrom(src => src.m_nDomainID));
            //#endregion

            //#region User Billing Transactions
            //Mapper.CreateMap<UserBillingTransactionsResponse, KalturaUserBillingTransactions>()
            //    .ForMember(dest => dest.SiteGuid, opt => opt.MapFrom(src => src.m_sSiteGUID))
            //    .ForMember(dest => dest.BillingTransactions, opt => opt.MapFrom(src => src.m_BillingTransactionResponse));
            //#endregion

            #region Asset Item Prices
            Mapper.CreateMap<AssetItemPrices, KalturaAssetPrice>()
              .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.AssetId))
              .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => src.AssetType))
              .ForMember(dest => dest.FilePrices, opt => opt.MapFrom(src => src.PriceContainers))
              ;
            #endregion

            #region Asset Files
            Mapper.CreateMap<KalturaPersonalAssetRequest, AssetFiles>()
              .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => src.Type))
              .ForMember(dest => dest.FileIds, opt => opt.MapFrom(src => 
                  //{
                      //if (src.FileIds != null)
                      //{
                      //    return null;
                      //}
                      //else
                      //{
                          src.FileIds
                          //.Select(i => (long)i).ToList();
                      //}
                  //}
                ))
              ;
            #endregion

            // ServiceObject to KalturaPremiumService
            Mapper.CreateMap<ServiceObject, KalturaPremiumService>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            Mapper.CreateMap<KalturaCDVRAdapterProfile, CDVRAdapter>()
               .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
               .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => ConvertCDVRAdapterSettings(src.Settings)))
               .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.ExternalIdentifier))
               .ForMember(dest => dest.DynamicLinksSupport, opt => opt.MapFrom(src => src.DynamicLinksSupport))
               .ForMember(dest => dest.SharedSecret, opt => opt.MapFrom(src => src.SharedSecret));

            Mapper.CreateMap<CDVRAdapter, KalturaCDVRAdapterProfile>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.AdapterUrl, opt => opt.MapFrom(src => src.AdapterUrl))
              .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
              .ForMember(dest => dest.Settings, opt => opt.MapFrom(src => ConvertCDVRAdapterSettings(src.Settings)))
              .ForMember(dest => dest.ExternalIdentifier, opt => opt.MapFrom(src => src.ExternalIdentifier))
              .ForMember(dest => dest.DynamicLinksSupport, opt => opt.MapFrom(src => src.DynamicLinksSupport))
              .ForMember(dest => dest.SharedSecret, opt => opt.MapFrom(src => src.SharedSecret));

  
            // LicensedLinkResponse to KalturaLicensedUrls
            Mapper.CreateMap<LicensedLinkResponse, KalturaLicensedUrl>()
               .ForMember(dest => dest.MainUrl, opt => opt.MapFrom(src => src.mainUrl))
               .ForMember(dest => dest.AltUrl, opt => opt.MapFrom(src => src.altUrl));

            #region Recordings

            // KalturaRecording to Recording
            Mapper.CreateMap<KalturaRecording, Recording>()
               .ForMember(dest => dest.EpgId, opt => opt.MapFrom(src => src.AssetId))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.RecordingStatus, opt => opt.MapFrom(src => ConvertKalturaRecordingStatus(src.Status)))
               .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertKalturaRecordingType(src.Type)))               
               .ForMember(dest => dest.ViewableUntilDate, opt => opt.MapFrom(src => src.ViewableUntilDate))
               .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => SerializationUtils.ConvertFromUnixTimestamp(src.CreateDate)))
               .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => SerializationUtils.ConvertFromUnixTimestamp(src.UpdateDate)));

            // Recording to KalturaRecording
            Mapper.CreateMap<Recording, KalturaRecording>()
               .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.EpgId))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Status, opt => opt.MapFrom(src => ConvertTstvRecordingStatus(src.RecordingStatus)))
               .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertRecordingType(src.Type)))
               .ForMember(dest => dest.IsProtected, opt => opt.MapFrom(src => IsRecordingProtected(src.ProtectedUntilDate)))
               .ForMember(dest => dest.ViewableUntilDate, opt => opt.MapFrom(src => src.ViewableUntilDate))
               .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.CreateDate)))
               .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.UpdateDate)));
            
            // KalturaSeriesRecording to SeriesRecording
            Mapper.CreateMap<KalturaSeriesRecording, SeriesRecording>()
               .ForMember(dest => dest.EpgId, opt => opt.MapFrom(src => src.EpgId))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.EpgChannelId, opt => opt.MapFrom(src => src.ChannelId))
               .ForMember(dest => dest.SeasonNumber, opt => opt.MapFrom(src => src.SeasonNumber))
               .ForMember(dest => dest.SeriesId, opt => opt.MapFrom(src => src.SeriesId))
               .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertKalturaRecordingType(src.Type)))
               .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => SerializationUtils.ConvertFromUnixTimestamp(src.CreateDate)));

            // SeriesRecording to KalturaSeriesRecording
            Mapper.CreateMap<SeriesRecording, KalturaSeriesRecording>()
               .ForMember(dest => dest.EpgId, opt => opt.MapFrom(src => src.EpgId))
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.ChannelId, opt => opt.MapFrom(src => src.EpgChannelId))
               .ForMember(dest => dest.SeasonNumber, opt => opt.MapFrom(src => src.SeasonNumber > 0 ? (int?)src.SeasonNumber : null))
               .ForMember(dest => dest.SeriesId, opt => opt.MapFrom(src => src.SeriesId))
               .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertRecordingType(src.Type)))
               .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.CreateDate)))
               .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.UpdateDate)))
               .ForMember(dest => dest.ExcludedSeasons, opt => opt.MapFrom(src => src.ExcludedSeasons));
            #endregion

            #region Household Quota
            Mapper.CreateMap<DomainQuotaResponse, KalturaHouseholdQuota>()
               .ForMember(dest => dest.AvailableQuota, opt => opt.MapFrom(src => src.AvailableQuota))
               .ForMember(dest => dest.TotalQuota, opt => opt.MapFrom(src => src.TotalQuota));
               
            #endregion

            //KalturaHouseholdPremiumService
            Mapper.CreateMap<ServiceObject, KalturaHouseholdPremiumService>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            // KalturaAssetFileContext to EntitlementResponse
            Mapper.CreateMap<KalturaAssetFileContext, EntitlementResponse>()
              .ForMember(dest => dest.ViewLifeCycle, opt => opt.MapFrom(src => src.ViewLifeCycle))
              .ForMember(dest => dest.FullLifeCycle, opt => opt.MapFrom(src => src.FullLifeCycle))
            .ForMember(dest => dest.IsOfflinePlayBack, opt => opt.MapFrom(src => src.IsOfflinePlayBack));

            // EntitlementResponse to KalturaAssetFileContext
            Mapper.CreateMap<EntitlementResponse, KalturaAssetFileContext>()
              .ForMember(dest => dest.ViewLifeCycle, opt => opt.MapFrom(src => src.ViewLifeCycle))
              .ForMember(dest => dest.FullLifeCycle, opt => opt.MapFrom(src => src.FullLifeCycle))
            .ForMember(dest => dest.IsOfflinePlayBack, opt => opt.MapFrom(src => src.IsOfflinePlayBack));

        }

        #region Recording Help Methods

        public static bool IsRecordingProtected(long? protectedUntilEpoch)
        {
            if (!protectedUntilEpoch.HasValue)
            {
                return false;
            }
            
            long currentUtcTime = SerializationUtils.GetCurrentUtcTimeInUnixTimestamp();
            return protectedUntilEpoch.Value > currentUtcTime;
        }

        public static TstvRecordingStatus ConvertKalturaRecordingStatus(KalturaRecordingStatus recordingStatus)
        {
            TstvRecordingStatus result;
            switch (recordingStatus)
            {
                case KalturaRecordingStatus.CANCELED:
                    result = TstvRecordingStatus.Canceled;
                    break;
                case KalturaRecordingStatus.DELETED:
                    result = TstvRecordingStatus.Deleted;
                    break;
                case KalturaRecordingStatus.FAILED:
                    result = TstvRecordingStatus.Failed;
                    break;
                case KalturaRecordingStatus.RECORDED:
                    result = TstvRecordingStatus.Recorded;
                    break;
                case KalturaRecordingStatus.RECORDING:
                    result = TstvRecordingStatus.Recording;
                    break;
                case KalturaRecordingStatus.SCHEDULED:
                    result = TstvRecordingStatus.Scheduled;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown recordingStatus type");
            }
            return result;
        }

        public static KalturaRecordingStatus ConvertTstvRecordingStatus(TstvRecordingStatus recordingStatus)
        {
            KalturaRecordingStatus result;
            switch (recordingStatus)
            {
                case TstvRecordingStatus.SeriesCancel:
                case TstvRecordingStatus.Canceled:
                    result = KalturaRecordingStatus.CANCELED;
                    break;
                case TstvRecordingStatus.SeriesDelete:
                case TstvRecordingStatus.Deleted:
                    result = KalturaRecordingStatus.DELETED;
                    break;
                case TstvRecordingStatus.Failed:
                    result = KalturaRecordingStatus.FAILED;
                    break;
                case TstvRecordingStatus.Recorded:
                    result = KalturaRecordingStatus.RECORDED;
                    break;
                case TstvRecordingStatus.Recording:
                    result = KalturaRecordingStatus.RECORDING;
                    break;
                case TstvRecordingStatus.Scheduled:
                    result = KalturaRecordingStatus.SCHEDULED;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown recordingStatus type");
            }
            return result;
        }

        public static RecordingType ConvertKalturaRecordingType(KalturaRecordingType recordingType)
        {
            RecordingType result;
            switch (recordingType)
            {
                case KalturaRecordingType.SINGLE:
                    result = RecordingType.Single;
                    break;
                case KalturaRecordingType.SEASON:
                    result = RecordingType.Season;
                    break;
                case KalturaRecordingType.SERIES:
                    result = RecordingType.Series;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown recordingType type");
            }
            return result;
        }

        public static KalturaRecordingType ConvertRecordingType(RecordingType recordingType)
        {
            KalturaRecordingType result;
            switch (recordingType)
            {
                case RecordingType.Single:
                    result = KalturaRecordingType.SINGLE;
                    break;
                case RecordingType.Season:
                    result = KalturaRecordingType.SEASON;
                    break;
                case RecordingType.Series:
                    result = KalturaRecordingType.SERIES;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown recordingType type");
            }
            return result;
        }

        public static ApiObjects.SearchObjects.OrderObj ConvertOrderToOrderObj(KalturaRecordingOrderBy order)
        {
            ApiObjects.SearchObjects.OrderObj result = new ApiObjects.SearchObjects.OrderObj();

            switch (order)
            {
                case KalturaRecordingOrderBy.TITLE_ASC:
                    result.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.NAME;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                    break;
                case KalturaRecordingOrderBy.TITLE_DESC:
                    result.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.NAME;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaRecordingOrderBy.START_DATE_ASC:
                    result.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.START_DATE;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                    break;
                case KalturaRecordingOrderBy.START_DATE_DESC:
                    result.m_eOrderBy = ApiObjects.SearchObjects.OrderBy.START_DATE;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
            }
            return result;
        }
        public static SeriesRecordingOrderObj ConvertOrderToSeriesOrderObj(KalturaSeriesRecordingOrderBy order)
        {
            SeriesRecordingOrderObj result = new SeriesRecordingOrderObj();

            switch (order)
            {
                case KalturaSeriesRecordingOrderBy.START_DATE_ASC:
                    result.OrderBy = SeriesOrderBy.START_DATE;
                    result.OrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                    break;
                case KalturaSeriesRecordingOrderBy.START_DATE_DESC:
                    result.OrderBy = SeriesOrderBy.START_DATE;
                    result.OrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaSeriesRecordingOrderBy.ID_ASC:
                    result.OrderBy = SeriesOrderBy.ID;
                    result.OrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                    break;
                case KalturaSeriesRecordingOrderBy.ID_DESC:
                    result.OrderBy = SeriesOrderBy.ID;
                    result.OrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaSeriesRecordingOrderBy.SERIES_ID_ASC:
                    result.OrderBy = SeriesOrderBy.SERIES_ID;
                    result.OrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                    break;
                case KalturaSeriesRecordingOrderBy.SERIES_ID_DESC:
                    result.OrderBy = SeriesOrderBy.SERIES_ID;
                    result.OrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
            }
            return result;
        }

        #endregion

        // TransactionType to eTransactionType
        public static eTransactionType ConvertTransactionType(KalturaTransactionType clientTransactionType)
        {
            eTransactionType result;
            switch (clientTransactionType)
            {
                case KalturaTransactionType.ppv:
                    result = eTransactionType.PPV;
                    break;
                case KalturaTransactionType.subscription:
                    result = eTransactionType.Subscription;
                    break;
                case KalturaTransactionType.collection:
                    result = eTransactionType.Collection;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown transaction type");
            }
            return result;
        }

        internal static List<CDVRAdapterSettings> ConvertCDVRAdapterSettings(SerializableDictionary<string, KalturaStringValue> settings)
        {
            List<CDVRAdapterSettings> result = null;

            if (settings != null && settings.Count > 0)
            {
                result = new List<CDVRAdapterSettings>();
                CDVRAdapterSettings pc;
                foreach (KeyValuePair<string, KalturaStringValue> data in settings)
                {
                    if (!string.IsNullOrEmpty(data.Key))
                    {
                        pc = new CDVRAdapterSettings();
                        pc.key = data.Key;
                        pc.value = data.Value.value;
                        result.Add(pc);
                    }
                }
            }
            if (result != null && result.Count > 0)
            {
                return result;
            }
            else
            {
                return null;
            }
        }

        public static Dictionary<string, KalturaStringValue> ConvertCDVRAdapterSettings(List<CDVRAdapterSettings> settings)
        {
            Dictionary<string, KalturaStringValue> result = null;

            if (settings != null && settings.Count() > 0)
            {
                result = new Dictionary<string, KalturaStringValue>();
                foreach (var data in settings)
                {
                    if (!string.IsNullOrEmpty(data.key))
                    {
                        result.Add(data.key, new KalturaStringValue() { value = data.value });
                    }
                }
            }
            return result;
        }

        public static TransactionHistoryOrderBy ConvertTransactionHistoryOrderBy(KalturaTransactionHistoryOrderBy orderBy)
        {
            TransactionHistoryOrderBy result;

            switch (orderBy)
            {
                case KalturaTransactionHistoryOrderBy.CREATE_DATE_DESC:
                    result = TransactionHistoryOrderBy.CreateDateDesc;
                    break;
                case KalturaTransactionHistoryOrderBy.CREATE_DATE_ASC:
                    result = TransactionHistoryOrderBy.CreateDateAsc;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown transaction history order by");
            }

            return result;
        }

        public static EntitlementOrderBy ConvertEntitlementOrderBy(KalturaEntitlementOrderBy orderBy)
        {
            EntitlementOrderBy result;

            switch (orderBy)
            {
                case KalturaEntitlementOrderBy.PURCHASE_DATE_ASC:
                    result = EntitlementOrderBy.PurchaseDateAsc;
                    break;
                case KalturaEntitlementOrderBy.PURCHASE_DATE_DESC:
                    result = EntitlementOrderBy.PurchaseDateDesc;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown entitlement order by");
            }

            return result;
        }
    }
}