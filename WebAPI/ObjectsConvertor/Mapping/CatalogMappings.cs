using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Catalog;
using ApiObjects.Epg;
using ApiObjects.SearchObjects;
using ApiObjects.Statistics;
using AutoMapper;
using AutoMapper.Configuration;
using Catalog.Response;
using Core.Catalog;
using Core.Catalog.Response;
using Core.Users;
using GroupsCacheManager;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using TVinciShared;
using WebAPI.App_Start;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.Models.Upload;
using WebAPI.ObjectsConvertor.Mapping.Utils;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class CatalogMappings
    {
        public static void RegisterMappings(MapperConfigurationExpression cfg)
        {
            #region Picture, EpgPicture -> KalturaMediaImage

            // Picture to KalturaMediaImage
            cfg.CreateMap<Picture, KalturaMediaImage>()
                 .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.m_sURL))
                 .ForMember(dest => dest.Height, opt => opt.MapFrom(src => GetPictureHeight(src.m_sSize)))
                 .ForMember(dest => dest.Width, opt => opt.MapFrom(src => GetPictureWidth(src.m_sSize)))
                 .ForMember(dest => dest.Ratio, opt => opt.MapFrom(src => src.ratio))
                 .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.isDefault))
                 .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.version))
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id));

            // EPGPicture to KalturaMediaImage
            cfg.CreateMap<EpgPicture, KalturaMediaImage>()
                 .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
                 .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.PicHeight))
                 .ForMember(dest => dest.Width, opt => opt.MapFrom(src => src.PicWidth))
                 .ForMember(dest => dest.Ratio, opt => opt.MapFrom(src => src.Ratio))
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                 .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.Version));

            cfg.CreateMap<Image, KalturaMediaImage>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ContentId))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault))
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
                .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.Version))
                .ForMember(dest => dest.Ratio, opt => opt.MapFrom(src => src.RatioName))
                .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Height.ToNullable()))
                .ForMember(dest => dest.Width, opt => opt.MapFrom(src => src.Width.ToNullable()));

            #endregion

            #region FileMedia, KalturaMediaFile

            //File 
            cfg.CreateMap<FileMedia, KalturaMediaFile>()
                 .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.m_nMediaID))
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nFileId))
                 .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.m_sFileFormat))
                 .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.m_sUrl))
                 .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.m_nDuration))
                 .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.m_sCoGUID))
                 .ForMember(dest => dest.BillingType, opt => opt.MapFrom(src => src.m_sBillingType))
                 .ForMember(dest => dest.Quality, opt => opt.MapFrom(src => src.Quality))
                 .ForMember(dest => dest.HandlingType, opt => opt.MapFrom(src => src.HandlingType))
                 .ForMember(dest => dest.CdnName, opt => opt.MapFrom(src => src.StreamingCompanyName))
                 .ForMember(dest => dest.CdnCode, opt => opt.MapFrom(src => src.m_nCdnID))
                 .ForMember(dest => dest.AltCdnCode, opt => opt.MapFrom(src => src.m_sAltUrl))
                 .ForMember(dest => dest.PPVModules, opt => opt.MapFrom(src => BuildPPVModulesList(src.PPVModules)))
                 .ForMember(dest => dest.ProductCode, opt => opt.MapFrom(src => src.ProductCode))
                 .ForMember(dest => dest.FileSize, opt => opt.MapFrom(src => src.FileSize))
                 .ForMember(dest => dest.CatalogEndDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.CatalogEndDate)))
                 ;

            #endregion

            #region BuzzWeightedAverScore, KalturaBuzzScore

            //BuzzScore
            cfg.CreateMap<BuzzWeightedAverScore, KalturaBuzzScore>()
                 .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.UpdateDate)))
                 .ForMember(dest => dest.NormalizedAvgScore, opt => opt.MapFrom(src => src.NormalizedWeightedAverageScore))
                 .ForMember(dest => dest.AvgScore, opt => opt.MapFrom(src => src.WeightedAverageScore));

            #endregion

            #region AssetStatsResult, KalturaAssetStatistics

            //AssetStats 
            cfg.CreateMap<AssetStatsResult, KalturaAssetStatistics>()
                 .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.m_nAssetID))
                 .ForMember(dest => dest.Likes, opt => opt.MapFrom(src => src.m_nLikes))
                 .ForMember(dest => dest.Views, opt => opt.MapFrom(src => src.m_nViews))
                 .ForMember(dest => dest.RatingCount, opt => opt.MapFrom(src => src.m_nVotes))
                 .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.m_dRate))
                 .ForMember(dest => dest.BuzzAvgScore, opt => opt.MapFrom(src => src.m_buzzAverScore));

            #endregion

            #region MediaObj, EPGChannelProgrammeObject, ProgramObj, RecordingObj -> KalturaAssetInfo

            //Media to AssetInfo
            cfg.CreateMap<MediaObj, KalturaAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => KalturaMultilingualString.GetCurrent(src.Name, src.m_sName)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => KalturaMultilingualString.GetCurrent(src.Description, src.m_sDescription)))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dStartDate)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dEndDate)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.m_oMediaType.m_nTypeID))
                .ForMember(dest => dest.Metas, opt => opt.MapFrom(src => BuildMetasDictionary(src.m_lMetas)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => BuildTagsDictionary(src.m_lTags)))
                .ForMember(dest => dest.ExtraParams, opt => opt.MapFrom(src => BuildExtraParamsDictionary(src)));

            //EPG to AssetInfo
            cfg.CreateMap<EPGChannelProgrammeObject, KalturaAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.EPG_ID))
                // KalturaMultilingualString.GetCurrent(src.ProgrammeName, src.NAME)
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => GetProgramName(src)))
                // KalturaMultilingualString.GetCurrent(src.ProgrammeDescription, src.DESCRIPTION))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => GetProgramDescription(src)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateUtils.StringExactToUtcUnixTimestampSeconds(src.START_DATE)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.StringExactToUtcUnixTimestampSeconds(src.END_DATE)))
                .ForMember(dest => dest.Metas, opt => opt.MapFrom(src => BuildMetasDictionary(src.EPG_Meta)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => BuildTagsDictionary(src.EPG_TAGS)))
                .ForMember(dest => dest.ExtraParams, opt => opt.MapFrom(src => BuildExtraParamsDictionary(src)));

            //ProgramObj to KalturaAssetInfo
            cfg.CreateMap<ProgramObj, KalturaAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => KalturaMultilingualString.GetCurrent(src.m_oProgram.ProgrammeName, src.m_oProgram.NAME)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => KalturaMultilingualString.GetCurrent(src.m_oProgram.ProgrammeDescription, src.m_oProgram.DESCRIPTION)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateUtils.StringExactToUtcUnixTimestampSeconds(src.m_oProgram.START_DATE)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.StringExactToUtcUnixTimestampSeconds(src.m_oProgram.END_DATE)))
                .ForMember(dest => dest.Metas, opt => opt.MapFrom(src => BuildMetasDictionary(src.m_oProgram.EPG_Meta)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => BuildTagsDictionary(src.m_oProgram.EPG_TAGS)))
                .ForMember(dest => dest.ExtraParams, opt => opt.MapFrom(src => BuildExtraParamsDictionary(src.m_oProgram)))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.m_oProgram.EPG_PICTURES));

            //EPG (recording) to AssetInfo
            cfg.CreateMap<RecordingObj, KalturaAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.RecordingId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => KalturaMultilingualString.GetCurrent(src.Program.m_oProgram.ProgrammeName, src.Program.m_oProgram.NAME)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => KalturaMultilingualString.GetCurrent(src.Program.m_oProgram.ProgrammeDescription, src.Program.m_oProgram.DESCRIPTION)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateUtils.StringExactToUtcUnixTimestampSeconds(src.Program.m_oProgram.START_DATE)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.StringExactToUtcUnixTimestampSeconds(src.Program.m_oProgram.END_DATE)))
                .ForMember(dest => dest.Metas, opt => opt.MapFrom(src => BuildMetasDictionary(src.Program.m_oProgram.EPG_Meta)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => BuildTagsDictionary(src.Program.m_oProgram.EPG_TAGS)));

            #endregion

            #region MediaObj, ProgramObj -> KalturaBaseAssetInfo

            //Media to SlimAssetInfo
            cfg.CreateMap<MediaObj, KalturaBaseAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => new KalturaMultilingualString(src.Name).ToString()))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => new KalturaMultilingualString(src.Description).ToString()))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.m_oMediaType.m_nTypeID));

            //Media to SlimAssetInfo
            cfg.CreateMap<ProgramObj, KalturaBaseAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => new KalturaMultilingualString(src.m_oProgram.ProgrammeName).ToString()))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => new KalturaMultilingualString(src.m_oProgram.ProgrammeDescription).ToString()))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (int)AssetType.epg));

            #endregion

            #region -> KalturaChannel

            //channelObj to Channel
            cfg.CreateMap<channelObj, WebAPI.Models.Catalog.KalturaChannel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nChannelID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => new KalturaMultilingualString(src.m_sTitle)))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.m_lPic))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => new KalturaMultilingualString(src.m_sDescription)));

            //Channel (Catalog) to Channel
            cfg.CreateMap<GroupsCacheManager.Channel, WebAPI.Models.Catalog.KalturaChannel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nChannelID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => new KalturaMultilingualString(src.m_sName)))
                .ForMember(dest => dest.AssetTypes, opt => opt.MapFrom(src => src.m_nMediaType))
                .ForMember(dest => dest.MediaTypes, opt => opt.MapFrom(src => src.m_nMediaType))
                .ForMember(dest => dest.FilterExpression, opt => opt.MapFrom(src => src.filterQuery))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => new KalturaMultilingualString(src.m_sDescription)))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => ConvertToNullableBool(src.m_nIsActive)))
                .ForMember(dest => dest.Order, opt => opt.ResolveUsing(src => ConvertOrderObjToAssetOrder(src.m_OrderObject.m_eOrderBy, src.m_OrderObject.m_eOrderDir)))
                .ForMember(dest => dest.GroupBy, opt => opt.ResolveUsing(src => ConvertToGroupBy(src.searchGroupBy)))
                .ForMember(dest => dest.SupportSegmentBasedOrdering, opt => opt.MapFrom(src => src.SupportSegmentBasedOrdering))
                .ForMember(dest => dest.AssetUserRuleId, opt => opt.MapFrom(src => src.AssetUserRuleId));

            //KSQLChannel to KalturaChannel
            cfg.CreateMap<KSQLChannel, KalturaChannel>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => new KalturaMultilingualString(src.Name)))
               .ForMember(dest => dest.OldName, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.AssetTypes, opt => opt.MapFrom(src => src.AssetTypes))
               .ForMember(dest => dest.MediaTypes, opt => opt.MapFrom(src => src.AssetTypes))
               .ForMember(dest => dest.Description, opt => opt.MapFrom(src => new KalturaMultilingualString(src.Description)))
               .ForMember(dest => dest.OldDescription, opt => opt.MapFrom(src => src.Description))
               .ForMember(dest => dest.FilterExpression, opt => opt.MapFrom(src => src.FilterQuery))
               .ForMember(dest => dest.IsActive, opt => opt.ResolveUsing(src => Convert.ToBoolean(src.IsActive)))
               .ForMember(dest => dest.Order, opt => opt.ResolveUsing(src => ConvertOrderObjToAssetOrder(src.Order)))
               .ForMember(dest => dest.GroupBy, opt => opt.ResolveUsing(src => ConvertGroupByToAssetGroupBy(src.GroupBy)));

            #endregion

            #region -> KSQLChannel

            //KalturaChannelProfile to KSQLChannel
            cfg.CreateMap<WebAPI.Models.API.KalturaChannelProfile, KSQLChannel>()
               .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.AssetTypes, opt => opt.MapFrom(src => src.AssetTypes))
               .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
               .ForMember(dest => dest.FilterQuery, opt => opt.MapFrom(src => src.FilterExpression))
               .ForMember(dest => dest.IsActive, opt => opt.ResolveUsing(src => Convert.ToInt32(src.IsActive)))
               .ForMember(dest => dest.Order, opt => opt.MapFrom(src => ApiMappings.ConvertOrderToOrderObj(src.Order)))
               ;

            //KalturaChannel to KSQLChannel
            cfg.CreateMap<KalturaChannel, KSQLChannel>()
               .ForMember(dest => dest.ID, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name != null ? src.Name.GetDefaultLanugageValue() : src.OldName))
               .ForMember(dest => dest.AssetTypes, opt => opt.MapFrom(src => src.getAssetTypes()))
               .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description != null ? src.Description.GetDefaultLanugageValue() : src.OldDescription))
               .ForMember(dest => dest.FilterQuery, opt => opt.MapFrom(src => src.FilterExpression))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive.HasValue && src.IsActive.Value ? 1 : 0))
               .ForMember(dest => dest.Order, opt => opt.ResolveUsing(src => ConvertAssetOrderToOrderObj(src.Order.Value)))
               .ForMember(dest => dest.GroupBy, opt => opt.ResolveUsing(src => ConvertAssetGroupByToGroupBy(src.GroupBy)));


            #endregion

            //KSQLChannel to KalturaChannelProfile
            cfg.CreateMap<KSQLChannel, WebAPI.Models.API.KalturaChannelProfile>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.AssetTypes, opt => opt.MapFrom(src => src.AssetTypes))
               .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
               .ForMember(dest => dest.FilterExpression, opt => opt.MapFrom(src => src.FilterQuery))
               .ForMember(dest => dest.IsActive, opt => opt.ResolveUsing(src => Convert.ToBoolean(src.IsActive)))
               .ForMember(dest => dest.Order, opt => opt.MapFrom(src => ApiMappings.ConvertOrderObjToOrder(src.Order)));

            //Channel (Catalog) to KalturaDynamicChannel
            cfg.CreateMap<GroupsCacheManager.Channel, WebAPI.Models.Catalog.KalturaDynamicChannel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nChannelID))
                .ForMember(dest => dest.SystemName, opt => opt.MapFrom(src => src.SystemName))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => new KalturaMultilingualString(src.NamesInOtherLanguages, src.m_sName)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => new KalturaMultilingualString(src.DescriptionInOtherLanguages, src.m_sDescription)))
                .ForMember(dest => dest.AssetTypes, opt => opt.MapFrom(src => src.m_nMediaType))
                .ForMember(dest => dest.Ksql, opt => opt.MapFrom(src => src.filterQuery))
                .ForMember(dest => dest.IsActive, opt => opt.ResolveUsing(src => ConvertToNullableBool(src.m_nIsActive)))
                .ForMember(dest => dest.OrderBy, opt => opt.ResolveUsing(src => ConvertToKalturaChannelOrder(src.m_OrderObject)))
                .ForMember(dest => dest.GroupBy, opt => opt.ResolveUsing(src => ConvertToGroupBy(src.searchGroupBy)))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.CreateDate)))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.UpdateDate)))
                .ForMember(dest => dest.SupportSegmentBasedOrdering, opt => opt.MapFrom(src => src.SupportSegmentBasedOrdering))
                .ForMember(dest => dest.AssetUserRuleId, opt => opt.MapFrom(src => src.AssetUserRuleId))
                .ForMember(dest => dest.MetaData, opt => opt.MapFrom(src => ConditionalAccessMappings.ConvertMetaData(src.MetaData)))
                .AfterMap((src, dest) => dest.MetaData = dest.MetaData != null && dest.MetaData.Any() ? dest.MetaData : null);


            //KalturaDynamicChannel to Channel (Catalog)  
            cfg.CreateMap<WebAPI.Models.Catalog.KalturaDynamicChannel, GroupsCacheManager.Channel>()
               .ForMember(dest => dest.m_nChannelID, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.SystemName, opt => opt.MapFrom(src => src.SystemName))
               .ForMember(dest => dest.m_sName, opt => opt.MapFrom(src => src.Name.GetDefaultLanugageValue()))
               .ForMember(dest => dest.NamesInOtherLanguages, opt => opt.MapFrom(src => src.Name.GetNoneDefaultLanugageContainer()))
               .ForMember(dest => dest.m_nMediaType, opt => opt.MapFrom(src => src.getAssetTypes()))
               .ForMember(dest => dest.m_sDescription, opt => opt.MapFrom(src => src.Description.GetDefaultLanugageValue()))
               .ForMember(dest => dest.DescriptionInOtherLanguages, opt => opt.MapFrom(src => src.Description.GetNoneDefaultLanugageContainer()))
               .ForMember(dest => dest.filterQuery, opt => opt.MapFrom(src => src.Ksql))
               .ForMember(dest => dest.m_nIsActive, opt => opt.MapFrom(src => ConvertToNullableInt(src.IsActive)))
               .ForMember(dest => dest.m_OrderObject, opt => opt.ResolveUsing(src => ConvertAssetOrderToOrderObj(src.OrderBy)))
               .ForMember(dest => dest.searchGroupBy, opt => opt.ResolveUsing(src => ConvertToGroupBy(src.GroupBy)))
               .ForMember(dest => dest.m_nChannelTypeID, opt => opt.MapFrom(src => (int)ChannelType.KSQL))
               .ForMember(dest => dest.m_eOrderBy, opt => opt.Ignore())
               .ForMember(dest => dest.m_eOrderDir, opt => opt.Ignore())
               .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
               .ForMember(dest => dest.UpdateDate, opt => opt.Ignore())
               .ForMember(dest => dest.SupportSegmentBasedOrdering, opt => opt.MapFrom(src => src.SupportSegmentBasedOrdering))
               .ForMember(dest => dest.AssetUserRuleId, opt => opt.MapFrom(src => src.AssetUserRuleId))
               .ForMember(dest => dest.MetaData, opt => opt.MapFrom(src => ConditionalAccessMappings.ConvertMetaData(src.MetaData)))
               .AfterMap((src, dest) => dest.MetaData = src.MetaData != null ? dest.MetaData : null);

            //Channel (Catalog) to KalturaManualChannel
            cfg.CreateMap<GroupsCacheManager.Channel, WebAPI.Models.Catalog.KalturaManualChannel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nChannelID))
                .ForMember(dest => dest.SystemName, opt => opt.MapFrom(src => src.SystemName))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => new KalturaMultilingualString(src.NamesInOtherLanguages, src.m_sName)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => new KalturaMultilingualString(src.DescriptionInOtherLanguages, src.m_sDescription)))
                .ForMember(dest => dest.IsActive, opt => opt.ResolveUsing(src => ConvertToNullableBool(src.m_nIsActive)))
                .ForMember(dest => dest.OrderBy, opt => opt.ResolveUsing(src => ConvertToKalturaChannelOrder(src.m_OrderObject)))
                .ForMember(dest => dest.MediaIds, opt => opt.MapFrom(src => src.m_lManualMedias != null ? string.Join(",", src.m_lManualMedias.OrderBy(x => x.m_nOrderNum).Select(x => x.m_sMediaId)) : string.Empty))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.CreateDate)))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.UpdateDate)))
                .ForMember(dest => dest.SupportSegmentBasedOrdering, opt => opt.MapFrom(src => src.SupportSegmentBasedOrdering))
                .ForMember(dest => dest.AssetUserRuleId, opt => opt.MapFrom(src => src.AssetUserRuleId))
                .ForMember(dest => dest.MetaData, opt => opt.MapFrom(src => ConditionalAccessMappings.ConvertMetaData(src.MetaData)))
                .AfterMap((src, dest) => dest.MetaData = dest.MetaData != null && dest.MetaData.Any() ? dest.MetaData : null);

            //KalturaManualChannel to Channel (Catalog)
            cfg.CreateMap<WebAPI.Models.Catalog.KalturaManualChannel, GroupsCacheManager.Channel>()
               .ForMember(dest => dest.m_nChannelID, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.SystemName, opt => opt.MapFrom(src => src.SystemName))
               .ForMember(dest => dest.m_sName, opt => opt.MapFrom(src => src.Name.GetDefaultLanugageValue()))
               .ForMember(dest => dest.NamesInOtherLanguages, opt => opt.MapFrom(src => src.Name.GetNoneDefaultLanugageContainer()))
               .ForMember(dest => dest.m_nMediaType, opt => opt.MapFrom(src => src.getAssetTypes()))
               .ForMember(dest => dest.m_sDescription, opt => opt.MapFrom(src => src.Description.GetDefaultLanugageValue()))
               .ForMember(dest => dest.DescriptionInOtherLanguages, opt => opt.MapFrom(src => src.Description.GetNoneDefaultLanugageContainer()))
               .ForMember(dest => dest.m_lManualMedias, opt => opt.ResolveUsing(src => ConvertToManualMedias(src.MediaIds)))
               .ForMember(dest => dest.m_nIsActive, opt => opt.MapFrom(src => ConvertToNullableInt(src.IsActive)))
               .ForMember(dest => dest.m_OrderObject, opt => opt.ResolveUsing(src => ConvertAssetOrderToOrderObj(src.OrderBy)))
               .ForMember(dest => dest.searchGroupBy, opt => opt.ResolveUsing(src => ConvertToGroupBy(src.GroupBy)))
               .ForMember(dest => dest.m_nChannelTypeID, opt => opt.MapFrom(src => (int)ChannelType.Manual))
               .ForMember(dest => dest.m_eOrderBy, opt => opt.Ignore())
               .ForMember(dest => dest.m_eOrderDir, opt => opt.Ignore())
               .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
               .ForMember(dest => dest.UpdateDate, opt => opt.Ignore())
               .ForMember(dest => dest.SupportSegmentBasedOrdering, opt => opt.MapFrom(src => src.SupportSegmentBasedOrdering))
               .ForMember(dest => dest.AssetUserRuleId, opt => opt.MapFrom(src => src.AssetUserRuleId))
               .ForMember(dest => dest.MetaData, opt => opt.MapFrom(src => ConditionalAccessMappings.ConvertMetaData(src.MetaData)))
               .AfterMap((src, dest) => dest.MetaData = src.MetaData != null ? dest.MetaData : null);

            //CategoryResponse to Category
            cfg.CreateMap<CategoryResponse, WebAPI.Models.Catalog.KalturaOTTCategory>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sTitle))
                .ForMember(dest => dest.ParentCategoryId, opt => opt.MapFrom(src => src.m_nParentCategoryID))
                .ForMember(dest => dest.ChildCategories, opt => opt.MapFrom(src => src.m_oChildCategories))
                .ForMember(dest => dest.Channels, opt => opt.MapFrom(src => src.m_oChannels))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.m_lPics));

            //AssetBookmarks to KalturaAssetBookmarks
            cfg.CreateMap<AssetBookmarks, WebAPI.Models.Catalog.KalturaAssetBookmarks>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AssetID))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => ConvertAssetType(src.AssetType)))
                .ForMember(dest => dest.Bookmarks, opt => opt.MapFrom(src => src.Bookmarks));

            //Bookmark to KalturaAssetBookmark
            cfg.CreateMap<Bookmark, WebAPI.Models.Catalog.KalturaAssetBookmark>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Location))
                .ForMember(dest => dest.PositionOwner, opt => opt.ResolveUsing(src => ConvertPositionOwner(src.UserType)))
                .ForMember(dest => dest.IsFinishedWatching, opt => opt.MapFrom(src => src.IsFinishedWatching));

            //User to KalturaBaseOTTUser
            cfg.CreateMap<User, WebAPI.Models.Users.KalturaBaseOTTUser>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_sSiteGUID))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.m_oBasicData.m_sUserName))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.m_oBasicData.m_sFirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.m_oBasicData.m_sLastName));

            //UnifiedSearchResuannounclt to KalturaSlimAsset
            cfg.CreateMap<UnifiedSearchResult, WebAPI.Models.Catalog.KalturaSlimAsset>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => ConvertAssetType(src.AssetType)));

            // Country
            cfg.CreateMap<Core.Users.Country, WebAPI.Models.Users.KalturaCountry>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nObjecrtID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sCountryName))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.m_sCountryCode));

            //EPGChannelProgrammeObject to KalturaProgramAsset
            cfg.CreateMap<EPGChannelProgrammeObject, KalturaProgramAsset>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.EPG_ID)) //???
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => new KalturaMultilingualString(src.ProgrammeName)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => new KalturaMultilingualString(src.ProgrammeDescription)))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => DateUtils.StringExactToUtcUnixTimestampSeconds(src.CREATE_DATE)))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => DateUtils.StringExactToUtcUnixTimestampSeconds(src.UPDATE_DATE)))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateUtils.StringExactToUtcUnixTimestampSeconds(src.START_DATE)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.StringExactToUtcUnixTimestampSeconds(src.END_DATE)))
                .ForMember(dest => dest.Metas, opt => opt.MapFrom(src => BuildMetasDictionary(src.EPG_Meta)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => BuildTagsDictionary(src.EPG_TAGS)))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.EPG_PICTURES))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.EPG_IDENTIFIER))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.EpgChannelId, opt => opt.MapFrom(src => StringUtils.TryConvertTo<long>(src.EPG_CHANNEL_ID)))
                .ForMember(dest => dest.EpgId, opt => opt.MapFrom(src => src.EPG_IDENTIFIER))
                .ForMember(dest => dest.RelatedMediaId, opt => opt.MapFrom(src => StringUtils.TryConvertTo<long>(src.media_id)))
                .ForMember(dest => dest.Crid, opt => opt.MapFrom(src => src.CRID))
                .ForMember(dest => dest.LinearAssetId, opt => opt.MapFrom(src => src.LINEAR_MEDIA_ID > 0 ? (long?)src.LINEAR_MEDIA_ID : null))
                .ForMember(dest => dest.CdvrEnabled, opt => opt.MapFrom(src => src.ENABLE_CDVR == 1))
                .ForMember(dest => dest.CatchUpEnabled, opt => opt.MapFrom(src => src.ENABLE_CATCH_UP == 1))
                .ForMember(dest => dest.StartOverEnabled, opt => opt.MapFrom(src => src.ENABLE_START_OVER == 1))
                .ForMember(dest => dest.TrickPlayEnabled, opt => opt.MapFrom(src => src.ENABLE_TRICK_PLAY == 1));

            #region Old Asset (Obj)

            //BaseObject to KalturaAsset
            cfg.CreateMap<BaseObject, KalturaAsset>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dUpdateDate)));

            //MediaObj to KalturaMediaAsset
            cfg.CreateMap<MediaObj, KalturaMediaAsset>()
                .IncludeBase<BaseObject, KalturaAsset>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => new KalturaMultilingualString(src.Name.ToList(), src.m_sName)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => new KalturaMultilingualString(src.Description.ToList(), src.m_sDescription)))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dStartDate)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dEndDate)))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dCreationDate)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.m_oMediaType.m_nTypeID))
                .ForMember(dest => dest.Metas, opt => opt.MapFrom(src => BuildMetasDictionary(src.m_lMetas)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => BuildTagsDictionary(src.m_lTags)))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.m_lPicture))
                .ForMember(dest => dest.MediaFiles, opt => opt.MapFrom(src => src.m_lFiles))
                .ForMember(dest => dest.ExternalIds, opt => opt.MapFrom(src => src.m_ExternalIDs))
                .ForMember(dest => dest.CatchUpBuffer, opt => opt.MapFrom(src => src.CatchUpBuffer))
                .ForMember(dest => dest.TrickPlayBuffer, opt => opt.MapFrom(src => src.TrickPlayBuffer))
                .ForMember(dest => dest.TypeDescription, opt => opt.MapFrom(src => src.m_oMediaType.m_sTypeName))
                .ForMember(dest => dest.DeviceRule, opt => opt.MapFrom(src => src.DeviceRule))
                .ForMember(dest => dest.GeoBlockRule, opt => opt.MapFrom(src => src.GeoblockRule))
                .ForMember(dest => dest.WatchPermissionRule, opt => opt.MapFrom(src => src.WatchPermissionRule))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.CoGuid))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.EntryId, opt => opt.MapFrom(src => src.EntryId));

            //MediaObj to KalturaLiveAsset
            cfg.CreateMap<MediaObj, KalturaLiveAsset>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dUpdateDate)))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => new KalturaMultilingualString(src.Name.ToList(), src.m_sName)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => new KalturaMultilingualString(src.Description.ToList(), src.m_sDescription)))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dStartDate)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dEndDate)))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dCreationDate)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.m_oMediaType.m_nTypeID))
                .ForMember(dest => dest.Metas, opt => opt.MapFrom(src => BuildMetasDictionary(src.m_lMetas)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => BuildTagsDictionary(src.m_lTags)))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.m_lPicture))
                .ForMember(dest => dest.MediaFiles, opt => opt.MapFrom(src => src.m_lFiles))
                .ForMember(dest => dest.TypeDescription, opt => opt.MapFrom(src => src.m_oMediaType.m_sTypeName))
                .ForMember(dest => dest.DeviceRule, opt => opt.MapFrom(src => src.DeviceRule))
                .ForMember(dest => dest.GeoBlockRule, opt => opt.MapFrom(src => src.GeoblockRule))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.WatchPermissionRule, opt => opt.MapFrom(src => src.WatchPermissionRule))
                .ForMember(dest => dest.EntryId, opt => opt.MapFrom(src => src.EntryId))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.CoGuid))
                .ForMember(dest => dest.SummedCatchUpBuffer, opt => opt.MapFrom(src => src.CatchUpBuffer))
                .ForMember(dest => dest.BufferTrickPlay, opt => opt.Ignore())
                .ForMember(dest => dest.BufferCatchUp, opt => opt.Ignore())
                .ForMember(dest => dest.CatchUpEnabled, opt => opt.MapFrom(src => src.EnableCatchUp))
                .ForMember(dest => dest.CdvrEnabled, opt => opt.MapFrom(src => src.EnableCDVR))
                .ForMember(dest => dest.EnableCatchUpState, opt => opt.Ignore())
                .ForMember(dest => dest.EnableCdvrState, opt => opt.Ignore())
                .ForMember(dest => dest.EnableRecordingPlaybackNonEntitledChannelState, opt => opt.Ignore())
                .ForMember(dest => dest.EnableStartOverState, opt => opt.Ignore())
                .ForMember(dest => dest.EnableTrickPlayState, opt => opt.Ignore())
                .ForMember(dest => dest.ExternalCdvrId, opt => opt.MapFrom(src => src.ExternalCdvrId))
                .ForMember(dest => dest.ExternalEpgIngestId, opt => opt.Ignore())
                .ForMember(dest => dest.RecordingPlaybackNonEntitledChannelEnabled, opt => opt.MapFrom(src => src.EnableRecordingPlaybackNonEntitledChannel))
                .ForMember(dest => dest.StartOverEnabled, opt => opt.MapFrom(src => src.EnableStartOver))
                .ForMember(dest => dest.SummedTrickPlayBuffer, opt => opt.MapFrom(src => src.TrickPlayBuffer))
                .ForMember(dest => dest.TrickPlayEnabled, opt => opt.MapFrom(src => src.EnableTrickPlay))
                .ForMember(dest => dest.ChannelType, opt => opt.Ignore())
                .ForMember(dest => dest.ExternalIds, opt => opt.MapFrom(src => src.m_ExternalIDs));

            //ProgramObj to KalturaProgramAsset
            cfg.CreateMap<ProgramObj, KalturaProgramAsset>()
                .IncludeBase<BaseObject, KalturaAsset>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => new KalturaMultilingualString(src.m_oProgram.ProgrammeName)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => new KalturaMultilingualString(src.m_oProgram.ProgrammeDescription)))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => DateUtils.StringExactToUtcUnixTimestampSeconds(src.m_oProgram.CREATE_DATE)))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateUtils.StringExactToUtcUnixTimestampSeconds(src.m_oProgram.START_DATE)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.StringExactToUtcUnixTimestampSeconds(src.m_oProgram.END_DATE)))
                .ForMember(dest => dest.Metas, opt => opt.MapFrom(src => BuildMetasDictionary(src.m_oProgram.EPG_Meta)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => BuildTagsDictionary(src.m_oProgram.EPG_TAGS)))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.m_oProgram.EPG_PICTURES))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.m_oProgram.EPG_IDENTIFIER))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.EpgChannelId, opt => opt.MapFrom(src => StringUtils.TryConvertTo<long>(src.m_oProgram.EPG_CHANNEL_ID)))
                .ForMember(dest => dest.EpgId, opt => opt.MapFrom(src => src.m_oProgram.EPG_IDENTIFIER))
                .ForMember(dest => dest.RelatedMediaId, opt => opt.MapFrom(src => StringUtils.TryConvertTo<long>(src.m_oProgram.media_id)))
                .ForMember(dest => dest.Crid, opt => opt.MapFrom(src => src.m_oProgram.CRID))
                .ForMember(dest => dest.LinearAssetId, opt => opt.MapFrom(src => src.m_oProgram.LINEAR_MEDIA_ID > 0 ? (long?)src.m_oProgram.LINEAR_MEDIA_ID : null))
                .ForMember(dest => dest.CdvrEnabled, opt => opt.MapFrom(src => src.m_oProgram.ENABLE_CDVR == 1))
                .ForMember(dest => dest.CatchUpEnabled, opt => opt.MapFrom(src => src.m_oProgram.ENABLE_CATCH_UP == 1))
                .ForMember(dest => dest.StartOverEnabled, opt => opt.MapFrom(src => src.m_oProgram.ENABLE_START_OVER == 1))
                .ForMember(dest => dest.TrickPlayEnabled, opt => opt.MapFrom(src => src.m_oProgram.ENABLE_TRICK_PLAY == 1));

            //RecordingObj to KalturaRecordingAsset
            cfg.CreateMap<RecordingObj, KalturaRecordingAsset>()
                .IncludeBase<BaseObject, KalturaAsset>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Program.AssetId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => new KalturaMultilingualString(src.Program.m_oProgram.ProgrammeName)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => new KalturaMultilingualString(src.Program.m_oProgram.ProgrammeDescription)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateUtils.StringExactToUtcUnixTimestampSeconds(src.Program.m_oProgram.START_DATE)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.StringExactToUtcUnixTimestampSeconds(src.Program.m_oProgram.END_DATE)))
                .ForMember(dest => dest.Metas, opt => opt.MapFrom(src => BuildMetasDictionary(src.Program.m_oProgram.EPG_Meta)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => BuildTagsDictionary(src.Program.m_oProgram.EPG_TAGS)))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Program.m_oProgram.EPG_PICTURES))
                .ForMember(dest => dest.EpgChannelId, opt => opt.MapFrom(src => src.Program.m_oProgram.EPG_CHANNEL_ID))
                .ForMember(dest => dest.EpgId, opt => opt.MapFrom(src => src.Program.m_oProgram.EPG_IDENTIFIER))
                .ForMember(dest => dest.RelatedMediaId, opt => opt.MapFrom(src => src.Program.m_oProgram.media_id))
                .ForMember(dest => dest.CdvrEnabled, opt => opt.MapFrom(src => src.Program.m_oProgram.ENABLE_CDVR == 1))
                .ForMember(dest => dest.CatchUpEnabled, opt => opt.MapFrom(src => src.Program.m_oProgram.ENABLE_CATCH_UP == 1))
                .ForMember(dest => dest.StartOverEnabled, opt => opt.MapFrom(src => src.Program.m_oProgram.ENABLE_START_OVER == 1))
                .ForMember(dest => dest.TrickPlayEnabled, opt => opt.MapFrom(src => src.Program.m_oProgram.ENABLE_TRICK_PLAY == 1))
                .ForMember(dest => dest.Crid, opt => opt.MapFrom(src => src.Program.m_oProgram.CRID))
                .ForMember(dest => dest.LinearAssetId, opt => opt.MapFrom(src => src.Program.m_oProgram.LINEAR_MEDIA_ID > 0 ? (long?)src.Program.m_oProgram.LINEAR_MEDIA_ID : null))
                .ForMember(dest => dest.RecordingId, opt => opt.MapFrom(src => src.RecordingId))
                .ForMember(dest => dest.RecordingType, opt => opt.MapFrom(src => src.RecordingType));

            #endregion

            #region New Asset (OPC)

            //KalturaAsset to Asset
            cfg.CreateMap<KalturaAsset, Asset>()
                .IncludeBase<IKalturaExcelableObject, IExcelObject>()
                .ForMember(dest => dest.Id, opt => opt.Ignore())
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.GetDefaultLanugageValue()))
                .ForMember(dest => dest.NamesWithLanguages, opt => opt.MapFrom(src => src.Name.GetNoneDefaultLanugageContainer().ToArray()))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description != null ? src.Description.GetDefaultLanugageValue() : null))
                .ForMember(dest => dest.DescriptionsWithLanguages, opt => opt.MapFrom(src => src.Description != null ? src.Description.GetNoneDefaultLanugageContainer().ToArray() : null))
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.Metas, opt => opt.MapFrom(src => src.Metas != null ? GetMetaList(src.Metas) : new List<Metas>()))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => src.Tags != null ? GetTagsList(src.Tags) : new List<Tags>()))
                .ForMember(dest => dest.RelatedEntities, opt => opt.MapFrom(src => src.RelatedEntities != null ? GetRelatedEntitiesList(src.RelatedEntities) : new List<RelatedEntities>()))
                .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdateDate, opt => opt.Ignore())
                .ForMember(dest => dest.StartDate, opt => opt.ResolveUsing(src => ConvertToNullableDatetime(src.StartDate)))
                .ForMember(dest => dest.EndDate, opt => opt.ResolveUsing(src => ConvertToNullableDatetime(src.EndDate)))
                .ForMember(dest => dest.CoGuid, opt => opt.MapFrom(src => src.ExternalId));

            //KalturaMediaAsset to MediaAsset
            cfg.CreateMap<KalturaMediaAsset, MediaAsset>()
                .IncludeBase<KalturaAsset, Asset>()
                .ForMember(dest => dest.MediaType, opt => opt.MapFrom(src => new Core.Catalog.MediaType(string.Empty, src.Type.HasValue ? src.Type.Value : 0)))
                .ForMember(dest => dest.DeviceRuleId, opt => opt.MapFrom(src => src.DeviceRuleId))
                .ForMember(dest => dest.GeoBlockRuleId, opt => opt.MapFrom(src => src.GeoBlockRuleId))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.EntryId, opt => opt.MapFrom(src => src.EntryId))
                .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => eAssetTypes.MEDIA))
                .ForMember(dest => dest.InheritancePolicy, opt => opt.MapFrom(src => ConvertInheritancePolicy(src.InheritancePolicy)))
                .ForMember(dest => dest.Files, opt => opt.MapFrom(src => src.MediaFiles));

            //KalturaLiveAsset to LiveAsset
            cfg.CreateMap<KalturaLiveAsset, LiveAsset>()
                .IncludeBase<KalturaMediaAsset, MediaAsset>()
                .ForMember(dest => dest.SummedCatchUpBuffer, opt => opt.MapFrom(src => src.SummedCatchUpBuffer))
                .ForMember(dest => dest.SummedTrickPlayBuffer, opt => opt.MapFrom(src => src.SummedTrickPlayBuffer))
                .ForMember(dest => dest.BufferCatchUp, opt => opt.MapFrom(src => src.BufferCatchUp))
                .ForMember(dest => dest.CatchUpEnabled, opt => opt.MapFrom(src => src.CatchUpEnabled))
                .ForMember(dest => dest.CdvrEnabled, opt => opt.MapFrom(src => src.CdvrEnabled))
                .ForMember(dest => dest.EnableCatchUpState, opt => opt.ResolveUsing(src => ConvertToTstvState(src.EnableCatchUpState)))
                .ForMember(dest => dest.EnableCdvrState, opt => opt.ResolveUsing(src => ConvertToTstvState(src.EnableCdvrState)))
                .ForMember(dest => dest.EnableRecordingPlaybackNonEntitledChannelState, opt => opt.ResolveUsing(src => ConvertToTstvState(src.EnableRecordingPlaybackNonEntitledChannelState)))
                .ForMember(dest => dest.EnableStartOverState, opt => opt.ResolveUsing(src => ConvertToTstvState(src.EnableStartOverState)))
                .ForMember(dest => dest.EnableTrickPlayState, opt => opt.ResolveUsing(src => ConvertToTstvState(src.EnableTrickPlayState)))
                .ForMember(dest => dest.ExternalCdvrId, opt => opt.MapFrom(src => src.ExternalCdvrId))
                .ForMember(dest => dest.ExternalEpgIngestId, opt => opt.MapFrom(src => src.ExternalEpgIngestId))
                .ForMember(dest => dest.RecordingPlaybackNonEntitledChannelEnabled, opt => opt.MapFrom(src => src.RecordingPlaybackNonEntitledChannelEnabled))
                .ForMember(dest => dest.StartOverEnabled, opt => opt.MapFrom(src => src.StartOverEnabled))
                .ForMember(dest => dest.BufferTrickPlay, opt => opt.MapFrom(src => src.BufferTrickPlay))
                .ForMember(dest => dest.TrickPlayEnabled, opt => opt.MapFrom(src => src.TrickPlayEnabled))
                .ForMember(dest => dest.ChannelType, opt => opt.ResolveUsing(src => ConvertToLinearChannelType(src.ChannelType)));

            cfg.CreateMap<KalturaProgramAsset, EpgAsset>()
                .IncludeBase<KalturaAsset, Asset>()
                .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => eAssetTypes.EPG))
                .ForMember(dest => dest.EpgChannelId, opt => opt.MapFrom(src => src.EpgChannelId))
                .ForMember(dest => dest.EpgIdentifier, opt => opt.MapFrom(src => src.ExternalId))
                .ForMember(dest => dest.RelatedMediaId, opt => opt.MapFrom(src => src.RelatedMediaId))
                .ForMember(dest => dest.Crid, opt => opt.MapFrom(src => src.Crid))
                .ForMember(dest => dest.LinearAssetId, opt => opt.MapFrom(src => src.LinearAssetId))
                .ForMember(dest => dest.CdvrEnabled, opt => opt.MapFrom(src => src.CdvrEnabled))
                .ForMember(dest => dest.CatchUpEnabled, opt => opt.MapFrom(src => src.CatchUpEnabled))
                .ForMember(dest => dest.StartOverEnabled, opt => opt.MapFrom(src => src.StartOverEnabled))
                .ForMember(dest => dest.TrickPlayEnabled, opt => opt.MapFrom(src => src.TrickPlayEnabled));

            cfg.CreateMap<RecordingAsset, KalturaRecordingAsset>()
                .IncludeBase<EpgAsset, KalturaProgramAsset>()
                .ForMember(dest => dest.RecordingType, opt => opt.MapFrom(src => src.RecordingType))
                .ForMember(dest => dest.RecordingId, opt => opt.MapFrom(src => src.RecordingId));

            // Asset to KalturaAsset
            cfg.CreateMap<Asset, KalturaAsset>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => new KalturaMultilingualString(src.NamesWithLanguages, src.Name)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => new KalturaMultilingualString(src.DescriptionsWithLanguages, src.Description)))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.CreateDate)))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.UpdateDate)))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.StartDate)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.EndDate)))
                .ForMember(dest => dest.Metas, opt => opt.MapFrom(src => BuildMetasDictionary(src.Metas)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => BuildTagsDictionary(src.Tags)))
                .ForMember(dest => dest.RelatedEntities, opt => opt.MapFrom(src => BuildRelatedEntitiesDictionary(src.RelatedEntities)))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.CoGuid))
                .ForMember(dest => dest.IndexStatus, opt => opt.MapFrom(src => src.IndexStatus))
                ;

            //MediaAsset to KalturaMediaAsset
            cfg.CreateMap<MediaAsset, KalturaMediaAsset>()
                .IncludeBase<Asset, KalturaAsset>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.MediaType.m_nTypeID))
                .ForMember(dest => dest.MediaFiles, opt => opt.MapFrom(src => src.Files))
                .ForMember(dest => dest.TypeDescription, opt => opt.MapFrom(src => src.MediaType.m_sTypeName))
                .ForMember(dest => dest.DeviceRuleId, opt => opt.MapFrom(src => src.DeviceRuleId))
                .ForMember(dest => dest.GeoBlockRuleId, opt => opt.MapFrom(src => src.GeoBlockRuleId))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.WatchPermissionRule, opt => opt.Ignore())
                .ForMember(dest => dest.EntryId, opt => opt.MapFrom(src => src.EntryId))
                .ForMember(dest => dest.InheritancePolicy, opt => opt.MapFrom(src => ConvertInheritancePolicy(src.InheritancePolicy)))
                .ForMember(dest => dest.ExternalIds, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.FallBackEpgIdentifier) ? src.FallBackEpgIdentifier : null))
            ;

            //LiveAsset to KalturaLiveAsset
            cfg.CreateMap<LiveAsset, KalturaLiveAsset>()
                .IncludeBase<MediaAsset, KalturaMediaAsset>()
                .ForMember(dest => dest.SummedCatchUpBuffer, opt => opt.MapFrom(src => src.SummedCatchUpBuffer))
                .ForMember(dest => dest.BufferTrickPlay, opt => opt.MapFrom(src => src.BufferTrickPlay))
                .ForMember(dest => dest.BufferCatchUp, opt => opt.MapFrom(src => src.BufferCatchUp))
                .ForMember(dest => dest.CatchUpEnabled, opt => opt.MapFrom(src => src.CatchUpEnabled))
                .ForMember(dest => dest.CdvrEnabled, opt => opt.MapFrom(src => src.CdvrEnabled))
                .ForMember(dest => dest.EnableCatchUpState, opt => opt.ResolveUsing(src => ConvertToKalturaTimeShiftedTvState(src.EnableCatchUpState)))
                .ForMember(dest => dest.EnableCdvrState, opt => opt.ResolveUsing(src => ConvertToKalturaTimeShiftedTvState(src.EnableCdvrState)))
                .ForMember(dest => dest.EnableRecordingPlaybackNonEntitledChannelState, opt => opt.ResolveUsing(src => ConvertToKalturaTimeShiftedTvState(src.EnableRecordingPlaybackNonEntitledChannelState)))
                .ForMember(dest => dest.EnableStartOverState, opt => opt.ResolveUsing(src => ConvertToKalturaTimeShiftedTvState(src.EnableStartOverState)))
                .ForMember(dest => dest.EnableTrickPlayState, opt => opt.ResolveUsing(src => ConvertToKalturaTimeShiftedTvState(src.EnableTrickPlayState)))
                .ForMember(dest => dest.ExternalCdvrId, opt => opt.MapFrom(src => src.ExternalCdvrId))
                .ForMember(dest => dest.ExternalEpgIngestId, opt => opt.MapFrom(src => src.ExternalEpgIngestId))
                .ForMember(dest => dest.RecordingPlaybackNonEntitledChannelEnabled, opt => opt.MapFrom(src => src.RecordingPlaybackNonEntitledChannelEnabled))
                .ForMember(dest => dest.StartOverEnabled, opt => opt.MapFrom(src => src.StartOverEnabled))
                .ForMember(dest => dest.SummedTrickPlayBuffer, opt => opt.MapFrom(src => src.SummedTrickPlayBuffer))
                .ForMember(dest => dest.TrickPlayEnabled, opt => opt.MapFrom(src => src.TrickPlayEnabled))
                .ForMember(dest => dest.ChannelType, opt => opt.ResolveUsing(src => ConvertToKalturaLinearChannelType(src.ChannelType)))
                .ForMember(dest => dest.ExternalIds, opt => opt.MapFrom(src => src.EpgChannelId));

            cfg.CreateMap<EpgAsset, KalturaProgramAsset>()
                .IncludeBase<Asset, KalturaAsset>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.EpgChannelId, opt => opt.MapFrom(src => src.EpgChannelId))
                .ForMember(dest => dest.EpgId, opt => opt.MapFrom(src => src.EpgIdentifier))
                .ForMember(dest => dest.RelatedMediaId, opt => opt.MapFrom(src => src.RelatedMediaId))
                .ForMember(dest => dest.Crid, opt => opt.MapFrom(src => src.Crid))
                .ForMember(dest => dest.LinearAssetId, opt => opt.MapFrom(src => src.LinearAssetId))
                .ForMember(dest => dest.EnableCdvr, opt => opt.MapFrom(src => src.CdvrEnabled))
                .ForMember(dest => dest.EnableCatchUp, opt => opt.MapFrom(src => src.CatchUpEnabled))
                .ForMember(dest => dest.EnableStartOver, opt => opt.MapFrom(src => src.StartOverEnabled))
                .ForMember(dest => dest.EnableTrickPlay, opt => opt.MapFrom(src => src.TrickPlayEnabled));
            //TODO ANAT - ASK LIOR ABOUT IMAGES (WHY WE ARE NOT MAPPING THEM HERE INSTED OF OUTSIDE THE MAPPING)
            //.ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Epg.EPG_PICTURES));

            cfg.CreateMap<AssetIndexStatus, KalturaAssetIndexStatus>()
                .ConvertUsing(syncStatus =>
                {
                    switch (syncStatus)
                    {
                        case AssetIndexStatus.Ok:
                            return KalturaAssetIndexStatus.Ok;
                        case AssetIndexStatus.Deleted:
                            return KalturaAssetIndexStatus.Deleted;
                        case AssetIndexStatus.NotUpdated:
                            return KalturaAssetIndexStatus.NotUpdated;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown SyncStatus value : {0}", syncStatus.ToString()));
                    }
                });


            #endregion

            //Comments to KalturaAssetComment
            cfg.CreateMap<Comments, KalturaAssetComment>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.m_nAssetID))
                .ForMember(dest => dest.AssetType, opt => opt.ResolveUsing(src => ConvertToKalturaAssetType(src.AssetType)))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dCreateDate)))
                .ForMember(dest => dest.Header, opt => opt.MapFrom(src => src.m_sHeader))
                .ForMember(dest => dest.SubHeader, opt => opt.MapFrom(src => src.m_sSubHeader))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.m_sContentText))
                .ForMember(dest => dest.Writer, opt => opt.MapFrom(src => src.m_sWriter));

            // Aggregation - asset count
            cfg.CreateMap<AggregationsResult, KalturaAssetsCount>()
                .ForMember(dest => dest.Field, opt => opt.MapFrom(src => src.field))
                .ForMember(dest => dest.Objects, opt => opt.MapFrom(src => src.results));

            cfg.CreateMap<AggregationResult, KalturaAssetCount>()
                .ForMember(dest => dest.Count, opt => opt.MapFrom(src => src.count))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.value))
                .ForMember(dest => dest.SubCounts, opt => opt.MapFrom(src => src.subs));

            #region New Catalog Management

            // AssetStruct to KalturaAssetStruct
            cfg.CreateMap<AssetStruct, KalturaAssetStruct>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => new KalturaMultilingualString(src.NamesInOtherLanguages, src.Name)))
                .ForMember(dest => dest.SystemName, opt => opt.MapFrom(src => src.SystemName))
                .ForMember(dest => dest.IsProtected, opt => opt.MapFrom(src => src.IsPredefined))
                .ForMember(dest => dest.MetaIds, opt => opt.MapFrom(src => src.MetaIds != null ? string.Join(",", src.MetaIds) : string.Empty))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate))
                .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.GetCommaSeparatedFeatures()))
                .ForMember(dest => dest.PluralName, opt => opt.MapFrom(src => src.PluralName))
                .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentId))
                .ForMember(dest => dest.ConnectingMetaId, opt => opt.MapFrom(src => src.ConnectingMetaId))
                .ForMember(dest => dest.ConnectedParentMetaId, opt => opt.MapFrom(src => src.ConnectedParentMetaId));

            // KalturaAssetStruct to AssetStruct
            cfg.CreateMap<KalturaAssetStruct, AssetStruct>()
                .IncludeBase<IKalturaExcelStructureManager, IExcelStructureManager>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.GetDefaultLanugageValue()))
                .ForMember(dest => dest.NamesInOtherLanguages, opt => opt.MapFrom(src => src.Name.GetNoneDefaultLanugageContainer()))
                .ForMember(dest => dest.SystemName, opt => opt.MapFrom(src => src.SystemName))
                .ForMember(dest => dest.IsPredefined, opt => opt.MapFrom(src => src.IsProtected))
                .ForMember(dest => dest.MetaIds, opt => opt.ResolveUsing(src => src.GetItemsIn<List<long>, long>(src.MetaIds, "KalturaAssetStruct.metaIds")))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate))
                .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.GetFeaturesAsHashSet()))
                .ForMember(dest => dest.PluralName, opt => opt.MapFrom(src => src.PluralName))
                .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentId))
                .ForMember(dest => dest.ConnectingMetaId, opt => opt.MapFrom(src => src.ConnectingMetaId))
                .ForMember(dest => dest.ConnectedParentMetaId, opt => opt.MapFrom(src => src.ConnectedParentMetaId));

            // MediaFileType to KalturaMediaFileType
            cfg.CreateMap<MediaFileType, KalturaMediaFileType>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate))
                .ForMember(dest => dest.IsTrailer, opt => opt.MapFrom(src => src.IsTrailer))
                .ForMember(dest => dest.StreamerType, opt => opt.ResolveUsing(src => ConvertStreamerType(src.StreamerType)))
                .ForMember(dest => dest.DrmProfileId, opt => opt.MapFrom(src => src.DrmId))
                .ForMember(dest => dest.Quality, opt => opt.ResolveUsing(src => ConvertToKalturaAssetFileTypeQuality(src.Quality)))
                .ForMember(dest => dest.VideoCodecs, opt => opt.MapFrom(src => src.CreateMappedHashSetForKalturaMediaFileType(src.VideoCodecs)))
                .ForMember(dest => dest.AudioCodecs, opt => opt.MapFrom(src => src.CreateMappedHashSetForKalturaMediaFileType(src.AudioCodecs)));

            // KalturaMediaFileType to MediaFileType
            cfg.CreateMap<KalturaMediaFileType, MediaFileType>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Status.HasValue ? src.Status.Value : true))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate))
                .ForMember(dest => dest.IsTrailer, opt => opt.MapFrom(src => src.IsTrailer))
                .ForMember(dest => dest.StreamerType, opt => opt.ResolveUsing(src => ConvertStreamerType(src.StreamerType)))
                .ForMember(dest => dest.DrmId, opt => opt.MapFrom(src => src.DrmProfileId))
                .ForMember(dest => dest.Quality, opt => opt.ResolveUsing(src => ConvertToAssetFileTypeQuality(src.Quality)))
                .ForMember(dest => dest.VideoCodecs, opt => opt.MapFrom(src => src.CreateMappedHashSetForMediaFileType(src.VideoCodecs)))
                .ForMember(dest => dest.AudioCodecs, opt => opt.MapFrom(src => src.CreateMappedHashSetForMediaFileType(src.AudioCodecs)));

            // Topic to KalturaMeta
            cfg.CreateMap<Topic, Models.API.KalturaMeta>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => new KalturaMultilingualString(src.NamesInOtherLanguages, src.Name)))
              .ForMember(dest => dest.SystemName, opt => opt.MapFrom(src => src.SystemName))
              .ForMember(dest => dest.DataType, opt => opt.ResolveUsing(src => ConvertToKalturaMetaDataType(src.Type)))
              .ForMember(dest => dest.MultipleValue, opt => opt.MapFrom(src => src.Type == ApiObjects.MetaType.Tag))
              .ForMember(dest => dest.IsProtected, opt => opt.MapFrom(src => src.IsPredefined))
              .ForMember(dest => dest.HelpText, opt => opt.MapFrom(src => src.HelpText))
              .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.GetCommaSeparatedFeatures()))
              .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentId.HasValue ? src.ParentId.Value.ToString() : null))
              .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
              .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate))
              .ForMember(dest => dest.Type, opt => opt.Ignore())
              .ForMember(dest => dest.PartnerId, opt => opt.Ignore())
              .ForMember(dest => dest.FieldName, opt => opt.Ignore())
              .ForMember(dest => dest.AssetType, opt => opt.Ignore());

            // KalturaMeta to Topic
            cfg.CreateMap<Models.API.KalturaMeta, Topic>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => !string.IsNullOrEmpty(src.Id) ? long.Parse(src.Id) : 0))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.GetDefaultLanugageValue()))
              .ForMember(dest => dest.NamesInOtherLanguages, opt => opt.MapFrom(src => src.Name.GetNoneDefaultLanugageContainer()))
              .ForMember(dest => dest.SystemName, opt => opt.MapFrom(src => src.SystemName))
              .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => ConvertToMetaType(src.DataType, src.MultipleValue)))
              .ForMember(dest => dest.IsPredefined, opt => opt.MapFrom(src => src.IsProtected))
              .ForMember(dest => dest.HelpText, opt => opt.MapFrom(src => src.HelpText))
              .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.GetFeaturesAsHashSet()))
              .ForMember(dest => dest.ParentId, opt => opt.ResolveUsing(src => StringUtils.TryConvertTo<long>(src.ParentId)))
              .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
              .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate));

            // AssetStructMeta to KalturaAssetStructMeta
            cfg.CreateMap<AssetStructMeta, KalturaAssetStructMeta>()
                .ForMember(dest => dest.AssetStructId, opt => opt.MapFrom(src => src.AssetStructId))
                .ForMember(dest => dest.MetaId, opt => opt.MapFrom(src => src.MetaId))
                .ForMember(dest => dest.IngestReferencePath, opt => opt.MapFrom(src => src.IngestReferencePath))
                .ForMember(dest => dest.ProtectFromIngest, opt => opt.MapFrom(src => src.ProtectFromIngest))
                .ForMember(dest => dest.DefaultIngestValue, opt => opt.MapFrom(src => src.DefaultIngestValue))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate))
                .ForMember(dest => dest.IsInherited, opt => opt.MapFrom(src => src.IsInherited))
                ;

            // KalturaAssetStructMeta to AssetStructMeta
            cfg.CreateMap<KalturaAssetStructMeta, AssetStructMeta>()
               .ForMember(dest => dest.AssetStructId, opt => opt.MapFrom(src => src.AssetStructId))
               .ForMember(dest => dest.MetaId, opt => opt.MapFrom(src => src.MetaId))
               .ForMember(dest => dest.IngestReferencePath, opt => opt.MapFrom(src => src.IngestReferencePath))
               .ForMember(dest => dest.ProtectFromIngest, opt => opt.MapFrom(src => src.ProtectFromIngest))
               .ForMember(dest => dest.DefaultIngestValue, opt => opt.MapFrom(src => src.DefaultIngestValue))
               .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
               .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate))
               .ForMember(dest => dest.IsInherited, opt => opt.MapFrom(src => src.IsInherited))
                ;

            #endregion

            #region Tag

            cfg.CreateMap<TagValue, KalturaTag>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.tagId))
              .ForMember(dest => dest.TagTypeId, opt => opt.MapFrom(src => src.topicId))
              .ForMember(dest => dest.Tag, opt => opt.MapFrom(src => new KalturaMultilingualString(src.TagsInOtherLanguages, src.value)))
              ;

            cfg.CreateMap<KalturaTag, TagValue>()
             .ForMember(dest => dest.topicId, opt => opt.MapFrom(src => src.TagTypeId))
             .ForMember(dest => dest.tagId, opt => opt.MapFrom(src => src.Id))
             .ForMember(dest => dest.value, opt => opt.MapFrom(src => src.Tag.GetDefaultLanugageValue()))
             .ForMember(dest => dest.TagsInOtherLanguages, opt => opt.MapFrom(src => src.Tag.GetNoneDefaultLanugageContainer()))
             ;

            #endregion       

            #region ImageType

            cfg.CreateMap<ImageType, KalturaImageType>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.DefaultImageId, opt => opt.MapFrom(src => src.DefaultImageId))
              .ForMember(dest => dest.HelpText, opt => opt.MapFrom(src => src.HelpText))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.RatioId, opt => opt.MapFrom(src => src.RatioId))
              .ForMember(dest => dest.SystemName, opt => opt.MapFrom(src => src.SystemName))
              ;

            cfg.CreateMap<KalturaImageType, ImageType>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.DefaultImageId, opt => opt.MapFrom(src => src.DefaultImageId))
              .ForMember(dest => dest.HelpText, opt => opt.MapFrom(src => src.HelpText))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.RatioId, opt => opt.MapFrom(src => src.RatioId))
              .ForMember(dest => dest.SystemName, opt => opt.MapFrom(src => src.SystemName))
             ;

            #endregion       

            #region Ratio

            cfg.CreateMap<Core.Catalog.Ratio, KalturaRatio>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Height))
              .ForMember(dest => dest.Width, opt => opt.MapFrom(src => src.Width))
              .ForMember(dest => dest.PrecisionPrecentage, opt => opt.MapFrom(src => src.PrecisionPrecentage));

            cfg.CreateMap<KalturaRatio, Core.Catalog.Ratio>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Height))
              .ForMember(dest => dest.Width, opt => opt.MapFrom(src => src.Width))
              .ForMember(dest => dest.PrecisionPrecentage, opt => opt.MapFrom(src => src.PrecisionPrecentage));

            #endregion

            #region Image

            cfg.CreateMap<Core.Catalog.Image, KalturaImage>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.ImageObjectId, opt => opt.MapFrom(src => src.ImageObjectId))
              .ForMember(dest => dest.ImageTypeId, opt => opt.MapFrom(src => src.ImageTypeId))
              .ForMember(dest => dest.ContentId, opt => opt.MapFrom(src => src.ContentId))
              .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
              .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.Version))
              .ForMember(dest => dest.Status, opt => opt.ResolveUsing(src => ConvertImageStatus(src.Status)))
              .ForMember(dest => dest.ImageObjectType, opt => opt.ResolveUsing(src => ConvertImageObjectType(src.ImageObjectType)))
              .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault));

            cfg.CreateMap<KalturaImage, Core.Catalog.Image>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.ImageObjectId, opt => opt.MapFrom(src => src.ImageObjectId))
              .ForMember(dest => dest.ImageTypeId, opt => opt.MapFrom(src => src.ImageTypeId))
              .ForMember(dest => dest.ContentId, opt => opt.MapFrom(src => src.ContentId))
              .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
              .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.Version))
              .ForMember(dest => dest.Status, opt => opt.ResolveUsing(src => ConvertImageStatus(src.Status)))
              .ForMember(dest => dest.ImageObjectType, opt => opt.ResolveUsing(src => ConvertImageObjectType(src.ImageObjectType.Value)))
              .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault));

            #endregion

            #region AssetFile
            //File 
            cfg.CreateMap<AssetFile, KalturaMediaFile>()
                 .ForMember(dest => dest.AdditionalData, opt => opt.MapFrom(src => src.AdditionalData))
                 .ForMember(dest => dest.AltExternalId, opt => opt.MapFrom(src => src.AltExternalId))
                 .ForMember(dest => dest.AltStreamingCode, opt => opt.MapFrom(src => src.AltStreamingCode))
                 .ForMember(dest => dest.AlternativeCdnAdapaterProfileId, opt => opt.MapFrom(src => src.AlternativeCdnAdapaterProfileId))
                 .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.AssetId))
                 .ForMember(dest => dest.BillingType, opt => opt.MapFrom(src => src.BillingType))
                 .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Duration))
                 .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.EndDate)))
                 .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.ExternalId))
                 .ForMember(dest => dest.ExternalStoreId, opt => opt.MapFrom(src => src.ExternalStoreId))
                 .ForMember(dest => dest.FileSize, opt => opt.MapFrom(src => src.FileSize))
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                 .ForMember(dest => dest.IsDefaultLanguage, opt => opt.MapFrom(src => src.IsDefaultLanguage))
                 .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.Language))
                 .ForMember(dest => dest.OrderNum, opt => opt.MapFrom(src => src.OrderNum))
                 .ForMember(dest => dest.OutputProtecationLevel, opt => opt.MapFrom(src => src.OutputProtecationLevel))
                 .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.StartDate)))
                 .ForMember(dest => dest.CdnAdapaterProfileId, opt => opt.MapFrom(src => src.CdnAdapaterProfileId))
                 .ForMember(dest => dest.TypeId, opt => opt.MapFrom(src => src.TypeId))
                 .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.GetTypeName()))
                 .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
                 .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsActive))
                 .ForMember(dest => dest.CatalogEndDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.CatalogEndDate)))
                 ;

            //File
            cfg.CreateMap<KalturaMediaFile, AssetFile>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.TypeId, opt => opt.MapFrom(src => src.TypeId))
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
                .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Duration))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.ExternalId))
                .ForMember(dest => dest.AltExternalId, opt => opt.MapFrom(src => src.AltExternalId))
                .ForMember(dest => dest.ExternalStoreId, opt => opt.MapFrom(src => src.ExternalStoreId))
                .ForMember(dest => dest.CdnAdapaterProfileId, opt => opt.MapFrom(src => src.CdnAdapaterProfileId))
                .ForMember(dest => dest.AltStreamingCode, opt => opt.MapFrom(src => src.AltStreamingCode))
                .ForMember(dest => dest.AlternativeCdnAdapaterProfileId, opt => opt.MapFrom(src => src.AlternativeCdnAdapaterProfileId))
                .ForMember(dest => dest.AdditionalData, opt => opt.MapFrom(src => src.AdditionalData))
                .ForMember(dest => dest.BillingType, opt => opt.MapFrom(src => src.BillingType))
                .ForMember(dest => dest.OrderNum, opt => opt.MapFrom(src => src.OrderNum))
                .ForMember(dest => dest.Language, opt => opt.MapFrom(src => src.Language))
                .ForMember(dest => dest.IsDefaultLanguage, opt => opt.MapFrom(src => src.IsDefaultLanguage))
                .ForMember(dest => dest.OutputProtecationLevel, opt => opt.MapFrom(src => src.OutputProtecationLevel))
                .ForMember(dest => dest.StartDate, opt => opt.ResolveUsing(src => ConvertToNullableDatetime(src.StartDate)))
                .ForMember(dest => dest.EndDate, opt => opt.ResolveUsing(src => ConvertToNullableDatetime(src.EndDate)))
                .ForMember(dest => dest.FileSize, opt => opt.MapFrom(src => src.FileSize))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Status))
                .ForMember(dest => dest.CatalogEndDate, opt => opt.ResolveUsing(src => ConvertToNullableDatetime(src.CatalogEndDate)))
                .ForMember(dest => dest.PpvModule, opt => opt.MapFrom(src => GetPPVModule(src.PPVModules)));

            #endregion

            #region BulkUpload

            cfg.CreateMap<IKalturaBulkUploadObject, IBulkUploadObject>();

            cfg.CreateMap<IKalturaExcelableObject, IExcelObject>()
                .IncludeBase<IKalturaBulkUploadObject, IBulkUploadObject>();

            cfg.CreateMap<IKalturaBulkUploadStructureManager, IBulkUploadStructureManager>();

            cfg.CreateMap<IKalturaExcelStructureManager, IExcelStructureManager>()
                .IncludeBase<IKalturaBulkUploadStructureManager, IBulkUploadStructureManager>();

            // BulkUpload to KalturaBulkUpload
            cfg.CreateMap<BulkUpload, KalturaBulkUpload>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.FileName, opt => opt.MapFrom(src => src.FileName))
               .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
               .ForMember(dest => dest.Action, opt => opt.MapFrom(src => src.Action))
               .ForMember(dest => dest.NumOfObjects, opt => opt.MapFrom(src => src.NumOfObjects))
               .ForMember(dest => dest.UploadedByUserId, opt => opt.MapFrom(src => src.UpdaterId))
               .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.CreateDate)))
               .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.UpdateDate)))
               .ForMember(dest => dest.Results, opt => opt.MapFrom(src => src.Results))
               .ForMember(dest => dest.Errors, opt => opt.MapFrom(src => src.Errors))
               ;

            cfg.CreateMap<BulkUploadJobStatus, KalturaBulkUploadJobStatus>()
               .ConvertUsing(bulkUploadJobStatus =>
               {
                   switch (bulkUploadJobStatus)
                   {
                       case BulkUploadJobStatus.Pending:
                           return KalturaBulkUploadJobStatus.Pending;
                       case BulkUploadJobStatus.Uploaded:
                           return KalturaBulkUploadJobStatus.Uploaded;
                       case BulkUploadJobStatus.Queued:
                           return KalturaBulkUploadJobStatus.Queued;
                       case BulkUploadJobStatus.Parsing:
                           return KalturaBulkUploadJobStatus.Parsing;
                       case BulkUploadJobStatus.Processing:
                           return KalturaBulkUploadJobStatus.Processing;
                       case BulkUploadJobStatus.Processed:
                           return KalturaBulkUploadJobStatus.Processed;
                       case BulkUploadJobStatus.Success:
                           return KalturaBulkUploadJobStatus.Success;
                       case BulkUploadJobStatus.Partial:
                           return KalturaBulkUploadJobStatus.Partial;
                       case BulkUploadJobStatus.Failed:
                           return KalturaBulkUploadJobStatus.Failed;
                       case BulkUploadJobStatus.Fatal:
                           return KalturaBulkUploadJobStatus.Fatal;
                       default:
                           throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown bulkUploadJobStatus value : {0}", bulkUploadJobStatus.ToString()));
                   }
               });

            cfg.CreateMap<KalturaBulkUploadJobStatus, BulkUploadJobStatus>()
               .ConvertUsing(kalturaBulkUploadJobStatus =>
               {
                   switch (kalturaBulkUploadJobStatus)
                   {
                       case KalturaBulkUploadJobStatus.Pending:
                           return BulkUploadJobStatus.Pending;
                       case KalturaBulkUploadJobStatus.Uploaded:
                           return BulkUploadJobStatus.Uploaded;
                       case KalturaBulkUploadJobStatus.Queued:
                           return BulkUploadJobStatus.Queued;
                       case KalturaBulkUploadJobStatus.Parsing:
                           return BulkUploadJobStatus.Parsing;
                       case KalturaBulkUploadJobStatus.Processing:
                           return BulkUploadJobStatus.Processing;
                       case KalturaBulkUploadJobStatus.Processed:
                           return BulkUploadJobStatus.Processed;
                       case KalturaBulkUploadJobStatus.Success:
                           return BulkUploadJobStatus.Success;
                       case KalturaBulkUploadJobStatus.Partial:
                           return BulkUploadJobStatus.Partial;
                       case KalturaBulkUploadJobStatus.Failed:
                           return BulkUploadJobStatus.Failed;
                       case KalturaBulkUploadJobStatus.Fatal:
                           return BulkUploadJobStatus.Fatal;
                       default:
                           throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown KalturaBulkUploadJobStatus value:{0}", kalturaBulkUploadJobStatus.ToString()));
                   }
               });

            cfg.CreateMap<BulkUploadJobAction, KalturaBulkUploadJobAction>()
               .ConvertUsing(bulkUploadJobAction =>
               {
                   switch (bulkUploadJobAction)
                   {
                       case BulkUploadJobAction.Upsert:
                           return KalturaBulkUploadJobAction.Upsert;
                       case BulkUploadJobAction.Delete:
                           return KalturaBulkUploadJobAction.Delete;
                       default:
                           throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown bulkUploadJobAction value : {0}", bulkUploadJobAction.ToString()));
                   }
               });

            cfg.CreateMap<BulkUploadResult, KalturaBulkUploadResult>()
              .ForMember(dest => dest.ObjectId, opt => opt.MapFrom(src => src.ObjectId))
              .ForMember(dest => dest.Index, opt => opt.MapFrom(src => src.Index))
              .ForMember(dest => dest.BulkUploadId, opt => opt.MapFrom(src => src.BulkUploadId))
              .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.Status))
              .ForMember(dest => dest.Errors, opt => opt.MapFrom(src => src.Errors))
              .ForMember(dest => dest.Warnings, opt => opt.MapFrom(src => src.Warnings));

            cfg.CreateMap<BulkUploadResultStatus, KalturaBulkUploadResultStatus>()
                .ConvertUsing(bulkUploadResultStatus =>
                {
                    switch (bulkUploadResultStatus)
                    {
                        case BulkUploadResultStatus.Error:
                            return KalturaBulkUploadResultStatus.Error;
                        case BulkUploadResultStatus.Ok:
                            return KalturaBulkUploadResultStatus.Ok;
                        case BulkUploadResultStatus.InProgress:
                            return KalturaBulkUploadResultStatus.InProgress;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, string.Format("Unknown bulkUploadResultStatus value : {0}", bulkUploadResultStatus.ToString()));
                    }
                });

            cfg.CreateMap<BulkUploadAssetResult, KalturaBulkUploadAssetResult>()
                .IncludeBase<BulkUploadResult, KalturaBulkUploadResult>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.ExternalId));

            cfg.CreateMap<BulkUploadMediaAssetResult, KalturaBulkUploadMediaAssetResult>()
               .IncludeBase<BulkUploadAssetResult, KalturaBulkUploadAssetResult>();

            cfg.CreateMap<BulkUploadLiveAssetResult, KalturaBulkUploadLiveAssetResult>()
               .IncludeBase<BulkUploadMediaAssetResult, KalturaBulkUploadMediaAssetResult>();

            cfg.CreateMap<BulkUploadProgramAssetResult, KalturaBulkUploadProgramAssetResult>()
              .IncludeBase<BulkUploadResult, KalturaBulkUploadResult>()
              .ForMember(dest => dest.ProgramId, opt => opt.MapFrom(src => src.ProgramId))
              .ForMember(dest => dest.ProgramExternalId, opt => opt.MapFrom(src => src.ProgramExternalId))
              .ForMember(dest => dest.LiveAssetId, opt => opt.MapFrom(src => src.LiveAssetId));

            cfg.CreateMap<KalturaBulkUploadJobData, BulkUploadJobData>();

            cfg.CreateMap<KalturaBulkUploadExcelJobData, BulkUploadExcelJobData>()
               .IncludeBase<KalturaBulkUploadJobData, BulkUploadJobData>();

            cfg.CreateMap<KalturaBulkUploadIngestJobData, BulkUploadIngestJobData>()
                .IncludeBase<KalturaBulkUploadJobData, BulkUploadJobData>();

            cfg.CreateMap<KalturaBulkUploadObjectData, BulkUploadObjectData>();

            cfg.CreateMap<KalturaBulkUploadAssetData, BulkUploadAssetData>()
               .IncludeBase<KalturaBulkUploadObjectData, BulkUploadObjectData>()
               .ForMember(dest => dest.TypeId, opt => opt.MapFrom(src => src.TypeId));

            cfg.CreateMap<KalturaBulkUploadMediaAssetData, BulkUploadMediaAssetData>()
               .IncludeBase<KalturaBulkUploadAssetData, BulkUploadAssetData>();

            cfg.CreateMap<KalturaBulkUploadProgramAssetData, BulkUploadEpgAssetData>()
               .IncludeBase<KalturaBulkUploadObjectData, BulkUploadObjectData>();

            cfg.CreateMap<KalturaBulkUploadLiveAssetData, BulkUploadLiveAssetData>()
               .IncludeBase<KalturaBulkUploadMediaAssetData, BulkUploadMediaAssetData>();

            cfg.CreateMap<ApiObjects.Response.Status, KalturaMessage>()
              .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.Code))
              .ForMember(dest => dest.Message, opt => opt.MapFrom(src => src.Message))
              .ForMember(dest => dest.Args, opt => opt.MapFrom(src => src.Args));

            cfg.CreateMap<ApiObjects.KeyValuePair, KeyValuePair<string, KalturaStringValue>>()
             .ForMember(dest => dest.Key, opt => opt.MapFrom(src => src.key))
             .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.value));

            #endregion

            #region CategoryItem
            cfg.CreateMap<ApiLogic.Catalog.CategoryItem, KalturaCategoryItem>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentId))
               .ForMember(dest => dest.ChildrenIds, opt => opt.MapFrom(src => src.ChildrenIds != null ? string.Join(",", src.ChildrenIds) : null))
               .ForMember(dest => dest.UnifiedChannels, opt => opt.MapFrom(src => src.UnifiedChannels))
               .ForMember(dest => dest.DynamicData, opt => opt.MapFrom(src => src.DynamicData != null ? src.DynamicData.ToDictionary(k => k.Key, v => v.Value) : null));

            cfg.CreateMap<KalturaCategoryItem, ApiLogic.Catalog.CategoryItem>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentId))
              .ForMember(dest => dest.ChildrenIds, opt => opt.ResolveUsing(src => !string.IsNullOrEmpty(src.ChildrenIds) ? src.GetItemsIn<List<long>, long>(src.ChildrenIds, "KalturaCategoryItem.childCategoriesIds", true) : null))
              .ForMember(dest => dest.UnifiedChannels, opt => opt.MapFrom(src => src.UnifiedChannels))
              .ForMember(dest => dest.DynamicData, opt => opt.MapFrom(src => WebAPI.Utils.Utils.ConvertSerializeableDictionary(src.DynamicData)));

            cfg.CreateMap<ApiLogic.Catalog.UnifiedChannelType, KalturaChannelType>()
                .ConvertUsing(type =>
                {
                    switch (type)
                    {
                        case ApiLogic.Catalog.UnifiedChannelType.Internal:
                            return KalturaChannelType.Internal;
                        case ApiLogic.Catalog.UnifiedChannelType.External:
                            return KalturaChannelType.External;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown UnifiedChannelType value : {type.ToString()}");
                    }
                });

            cfg.CreateMap<KalturaChannelType, ApiLogic.Catalog.UnifiedChannelType>()
                .ConvertUsing(type =>
                {
                    switch (type)
                    {
                        case KalturaChannelType.Internal:
                            return ApiLogic.Catalog.UnifiedChannelType.Internal;
                        case KalturaChannelType.External:
                            return ApiLogic.Catalog.UnifiedChannelType.External;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown KalturaChannelType value : {type.ToString()}");
                    }
                });

            cfg.CreateMap<KalturaUnifiedChannel, ApiLogic.Catalog.UnifiedChannel>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type));

            cfg.CreateMap<ApiLogic.Catalog.UnifiedChannel, KalturaUnifiedChannel>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type));

            cfg.CreateMap<KalturaUnifiedChannelInfo, ApiLogic.Catalog.UnifiedChannelInfo>()
                .IncludeBase<KalturaUnifiedChannel, ApiLogic.Catalog.UnifiedChannel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            cfg.CreateMap<ApiLogic.Catalog.UnifiedChannelInfo, KalturaUnifiedChannelInfo>()
                .IncludeBase<ApiLogic.Catalog.UnifiedChannel, KalturaUnifiedChannel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name));

            cfg.CreateMap<ApiLogic.Catalog.CategoryTree, KalturaCategoryTree>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.Children))
                .ForMember(dest => dest.UnifiedChannels, opt => opt.MapFrom(src => src.UnifiedChannels))
                .ForMember(dest => dest.DynamicData, opt => opt.MapFrom(src => src.DynamicData != null ? src.DynamicData.ToDictionary(k => k.Key, v => v.Value) : null))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images));

            cfg.CreateMap<KalturaCategoryTree, ApiLogic.Catalog.CategoryTree>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.Children))
                .ForMember(dest => dest.UnifiedChannels, opt => opt.MapFrom(src => src.UnifiedChannels))
                .ForMember(dest => dest.DynamicData, opt => opt.MapFrom(src => src.DynamicData != null ? src.DynamicData.ToDictionary(k => k.Key, v => v.Value) : null))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images));

            cfg.CreateMap<KalturaCategoryItemFilter, ApiLogic.Catalog.CategoryItemFilter>()
                .ForMember(dest => dest.OrderBy, opt => opt.MapFrom(src =>  CatalogConvertor.ConvertOrderToOrderObj(src.OrderBy)));

            cfg.CreateMap<KalturaCategoryItemByIdInFilter, ApiLogic.Catalog.CategoryItemByIdInFilter>()
               .IncludeBase<KalturaCategoryItemFilter, ApiLogic.Catalog.CategoryItemFilter>()
               .ForMember(dest => dest.IdIn, opt => opt.ResolveUsing(src => !string.IsNullOrEmpty(src.IdIn) ? src.GetItemsIn<List<long>, long>(src.IdIn, "KalturaCategoryItemByIdInFilter.IdIn", true) : null));

            cfg.CreateMap<KalturaCategoryItemSearchFilter, ApiLogic.Catalog.CategoryItemSearchFilter>()
              .IncludeBase<KalturaCategoryItemFilter, ApiLogic.Catalog.CategoryItemFilter>()
              .ForMember(dest => dest.Ksql, opt => opt.MapFrom(src => src.Ksql))
              .ForMember(dest => dest.RootOnly, opt => opt.MapFrom(src => src.RootOnly));

            cfg.CreateMap<KalturaCategoryItemAncestorsFilter, ApiLogic.Catalog.CategoryItemAncestorsFilter>()
              .IncludeBase<KalturaCategoryItemFilter, ApiLogic.Catalog.CategoryItemFilter>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            #endregion CategoryItem
        }

        private static int? ConvertToNullableInt(bool? value)
        {
            if (!value.HasValue)
            {
                return null;
            }

            return value.Value ? 1 : 0;
        }

        private static bool? ConvertToNullableBool(int? value)
        {
            if (!value.HasValue)
            {
                return null;
            }
            else if (value.Equals(1))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        private static object GetProgramDescription(EPGChannelProgrammeObject src)
        {
            return KalturaMultilingualString.GetCurrent(src.ProgrammeDescription, src.DESCRIPTION);
        }

        private static object GetProgramName(EPGChannelProgrammeObject src)
        {
            return KalturaMultilingualString.GetCurrent(src.ProgrammeName, src.NAME);
        }

        private static AssetInheritancePolicy? ConvertInheritancePolicy(KalturaAssetInheritancePolicy? inheritancePolicy)
        {
            if (!inheritancePolicy.HasValue)
            {
                return null;
            }

            switch (inheritancePolicy)
            {
                case KalturaAssetInheritancePolicy.Disable:
                    return AssetInheritancePolicy.Disable;

                case KalturaAssetInheritancePolicy.Enable:
                    return AssetInheritancePolicy.Enable;

                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown streamer type");
            }
        }

        private static KalturaAssetInheritancePolicy? ConvertInheritancePolicy(AssetInheritancePolicy? inheritancePolicy)
        {
            if (!inheritancePolicy.HasValue)
            {
                return null;
            }

            switch (inheritancePolicy)
            {
                case AssetInheritancePolicy.Disable:
                    return KalturaAssetInheritancePolicy.Disable;

                case AssetInheritancePolicy.Enable:
                    return KalturaAssetInheritancePolicy.Enable;

                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown streamer type");
            }
        }

        private static List<GroupsCacheManager.ManualMedia> ConvertToManualMedias(string mediaIdString)
        {
            List<GroupsCacheManager.ManualMedia> manualMedias = new List<GroupsCacheManager.ManualMedia>();
            GroupsCacheManager.ManualMedia manualMedia = null;

            if (!string.IsNullOrEmpty(mediaIdString))
            {
                var mediaIds = mediaIdString.Split(',');
                for (int orderNum = 1; orderNum <= mediaIds.Length; orderNum++)
                {
                    manualMedia = new GroupsCacheManager.ManualMedia(mediaIds[orderNum - 1], orderNum);
                    manualMedias.Add(manualMedia);
                }
            }

            return manualMedias;
        }

        #region New Catalog Management

        private static KalturaMediaFileStreamerType? ConvertStreamerType(StreamerType? type)
        {
            if (!type.HasValue)
            {
                return null;
            }

            switch (type)
            {
                case StreamerType.none:
                    return KalturaMediaFileStreamerType.NONE;

                case StreamerType.applehttp:
                    return KalturaMediaFileStreamerType.APPLE_HTTP;

                case StreamerType.mpegdash:
                    return KalturaMediaFileStreamerType.MPEG_DASH;

                case StreamerType.smothstreaming:
                    return KalturaMediaFileStreamerType.SMOOTH_STREAMING;

                case StreamerType.url:
                    return KalturaMediaFileStreamerType.URL;

                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown streamer type");
            }
        }

        private static StreamerType? ConvertStreamerType(KalturaMediaFileStreamerType? type)
        {
            if (!type.HasValue)
            {
                return null;
            }

            switch (type)
            {
                case KalturaMediaFileStreamerType.NONE:
                    return StreamerType.none;

                case KalturaMediaFileStreamerType.APPLE_HTTP:
                    return StreamerType.applehttp;

                case KalturaMediaFileStreamerType.MPEG_DASH:
                    return StreamerType.mpegdash;

                case KalturaMediaFileStreamerType.SMOOTH_STREAMING:
                    return StreamerType.smothstreaming;

                case KalturaMediaFileStreamerType.URL:
                    return StreamerType.url;

                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown streamer type");
            }
        }

        private static KalturaMediaFileTypeQuality? ConvertToKalturaAssetFileTypeQuality(MediaFileTypeQuality qualityType)
        {
            KalturaMediaFileTypeQuality? response = null;
            switch (qualityType)
            {
                case MediaFileTypeQuality.None:
                    break;
                case MediaFileTypeQuality.Adaptive:
                    response = KalturaMediaFileTypeQuality.ADAPTIVE;
                    break;
                case MediaFileTypeQuality.SD:
                    response = KalturaMediaFileTypeQuality.SD;
                    break;
                case MediaFileTypeQuality.HD_720:
                    response = KalturaMediaFileTypeQuality.HD_720;
                    break;
                case MediaFileTypeQuality.HD_1080:
                    response = KalturaMediaFileTypeQuality.HD_1080;
                    break;
                case MediaFileTypeQuality.UHD_4K:
                    response = KalturaMediaFileTypeQuality.UHD_4K;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown asset file type quality");
                    break;
            }

            return response;
        }

        private static MediaFileTypeQuality ConvertToAssetFileTypeQuality(KalturaMediaFileTypeQuality? qualityType)
        {
            MediaFileTypeQuality response;
            if (qualityType.HasValue)
            {
                switch (qualityType.Value)
                {
                    case KalturaMediaFileTypeQuality.ADAPTIVE:
                        response = MediaFileTypeQuality.Adaptive;
                        break;
                    case KalturaMediaFileTypeQuality.SD:
                        response = MediaFileTypeQuality.SD;
                        break;
                    case KalturaMediaFileTypeQuality.HD_720:
                        response = MediaFileTypeQuality.HD_720;
                        break;
                    case KalturaMediaFileTypeQuality.HD_1080:
                        response = MediaFileTypeQuality.HD_1080;
                        break;
                    case KalturaMediaFileTypeQuality.UHD_4K:
                        response = MediaFileTypeQuality.UHD_4K;
                        break;
                    default:
                        throw new ClientException((int)StatusCode.Error, "Unknown asset file type quality");
                        break;
                }
            }
            else
            {
                response = MediaFileTypeQuality.None;
            }

            return response;
        }

        private static KalturaMetaDataType? ConvertToKalturaMetaDataType(ApiObjects.MetaType metaType)
        {
            KalturaMetaDataType response;
            switch (metaType)
            {
                case ApiObjects.MetaType.String:
                case ApiObjects.MetaType.Tag:
                    response = KalturaMetaDataType.STRING;
                    break;
                case ApiObjects.MetaType.Number:
                    response = KalturaMetaDataType.NUMBER;
                    break;
                case ApiObjects.MetaType.Bool:
                    response = KalturaMetaDataType.BOOLEAN;
                    break;
                case ApiObjects.MetaType.DateTime:
                    response = KalturaMetaDataType.DATE;
                    break;
                case ApiObjects.MetaType.MultilingualString:
                    response = KalturaMetaDataType.MULTILINGUAL_STRING;
                    break;
                case ApiObjects.MetaType.ReleatedEntity:
                    response = KalturaMetaDataType.RELEATED_ENTITY;
                    break;
                case ApiObjects.MetaType.All:
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown meta type");
            }

            return response;
        }

        internal static ApiObjects.MetaType ConvertToMetaType(KalturaMetaDataType? metaDataType, bool? multipleValue)
        {
            ApiObjects.MetaType response;
            if (metaDataType.HasValue)
            {
                switch (metaDataType.Value)
                {
                    case KalturaMetaDataType.STRING:
                        if (multipleValue.HasValue && multipleValue.Value)
                        {
                            response = ApiObjects.MetaType.Tag;
                        }
                        else
                        {
                            response = ApiObjects.MetaType.String;
                        }
                        break;
                    case KalturaMetaDataType.MULTILINGUAL_STRING:
                        response = ApiObjects.MetaType.MultilingualString;
                        break;
                    case KalturaMetaDataType.NUMBER:
                        response = ApiObjects.MetaType.Number;
                        break;
                    case KalturaMetaDataType.BOOLEAN:
                        response = ApiObjects.MetaType.Bool;
                        break;
                    case KalturaMetaDataType.DATE:
                        response = ApiObjects.MetaType.DateTime;
                        break;
                    case KalturaMetaDataType.RELEATED_ENTITY:
                        response = ApiObjects.MetaType.ReleatedEntity;
                        break;
                    default:
                        throw new ClientException((int)StatusCode.Error, "Unknown meta data type");
                        break;
                }
            }
            else
            {
                response = ApiObjects.MetaType.All;
            }

            return response;
        }

        private static List<Metas> GetMetaList(SerializableDictionary<string, KalturaValue> metasDictionary)
        {
            List<Metas> metas = new List<Metas>();

            if (metasDictionary == null || metasDictionary.Count == 0)
            {
                return metas;
            }

            HashSet<string> metaNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, KalturaValue> meta in metasDictionary)
            {
                if (metaNames.Contains(meta.Key))
                {
                    throw new ClientException((int)StatusCode.Error, string.Format("The request contains meta with the name {0} more than once", meta.Key));
                }

                Metas metaToAdd = new Metas() { m_oTagMeta = new TagMeta() { m_sName = meta.Key } };
                Type metaType = meta.Value.GetType();
                if (metaType == typeof(KalturaBooleanValue))
                {
                    KalturaBooleanValue metaValue = meta.Value as KalturaBooleanValue;
                    metaToAdd.m_oTagMeta.m_sType = ApiObjects.MetaType.Bool.ToString();
                    metaToAdd.m_sValue = metaValue.value ? "1" : "0";
                }
                else if (metaType == typeof(KalturaStringValue))
                {
                    KalturaStringValue metaValue = meta.Value as KalturaStringValue;
                    metaToAdd.m_oTagMeta.m_sType = ApiObjects.MetaType.String.ToString();
                    metaToAdd.m_sValue = metaValue.value;
                }
                else if (metaType == typeof(KalturaMultilingualStringValue))
                {
                    KalturaMultilingualStringValue metaValue = meta.Value as KalturaMultilingualStringValue;
                    metaToAdd.m_oTagMeta.m_sType = ApiObjects.MetaType.MultilingualString.ToString();
                    metaToAdd.m_sValue = metaValue.value != null ? metaValue.value.GetDefaultLanugageValue() : null;
                    metaToAdd.Value = metaValue.value != null ? metaValue.value.GetNoneDefaultLanugageContainer().ToArray() : null;
                }
                else if (metaType == typeof(KalturaDoubleValue))
                {
                    KalturaDoubleValue metaValue = meta.Value as KalturaDoubleValue;
                    metaToAdd.m_oTagMeta.m_sType = ApiObjects.MetaType.Number.ToString();
                    metaToAdd.m_sValue = metaValue.value.ToString();
                }
                else if (metaType == typeof(KalturaLongValue))
                {
                    KalturaLongValue metaValue = meta.Value as KalturaLongValue;
                    metaToAdd.m_oTagMeta.m_sType = ApiObjects.MetaType.DateTime.ToString();
                    metaToAdd.m_sValue = DateUtils.UtcUnixTimestampSecondsToDateTime(metaValue.value)
                        .ToString(DateUtils.MAIN_FORMAT, CultureInfo.InvariantCulture);
                }
                else if (metaType == typeof(KalturaIntegerValue))
                {
                    throw new ClientException((int)StatusCode.Error, "Only KalturaDoubleValue type allowed for numbers type");
                }

                metaNames.Add(meta.Key);
                metas.Add(metaToAdd);
            }

            return metas;
        }

        private static List<Tags> GetTagsList(SerializableDictionary<string, KalturaMultilingualStringValueArray> tagsDictionary)
        {
            List<Tags> tags = new List<Tags>();

            if (tagsDictionary == null || tagsDictionary.Count == 0)
            {
                return tags;
            }

            HashSet<string> tagNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, KalturaMultilingualStringValueArray> tag in tagsDictionary)
            {
                if (tagNames.Contains(tag.Key))
                {
                    throw new ClientException((int)StatusCode.Error, string.Format("The request contains tag with the name {0} more than once", tag.Key));
                }

                Tags tagToAdd = new Tags() { m_oTagMeta = new TagMeta(tag.Key, ApiObjects.MetaType.Tag.ToString()) };
                HashSet<string> tagUniqueValues = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                tagToAdd.m_lValues = new List<string>();
                if (tag.Value.Objects != null)
                {
                    foreach (KalturaMultilingualStringValue tagValue in tag.Value.Objects)
                    {
                        string defaultLangValue = tagValue.value.GetDefaultLanugageValue();
                        if (tagUniqueValues.Contains(defaultLangValue))
                        {
                            throw new ClientException((int)StatusCode.Error, string.Format("The request contains tag with the name {0} and value {1} more than once", tag.Key, defaultLangValue));
                        }

                        tagUniqueValues.Add(defaultLangValue);
                        tagToAdd.m_lValues.Add(defaultLangValue);
                    }
                }

                tagNames.Add(tag.Key);
                tags.Add(tagToAdd);
            }

            return tags;
        }

        private static DateTime? ConvertToNullableDatetime(long? date)
        {
            DateTime? response = null;
            if (date.HasValue)
            {
                response = DateUtils.UtcUnixTimestampSecondsToDateTime(date.Value);
            }

            return response;
        }

        private static KalturaImageObjectType ConvertImageObjectType(eAssetImageType imageObjectType)
        {
            switch (imageObjectType)
            {
                case eAssetImageType.Media:
                    return KalturaImageObjectType.MEDIA_ASSET;
                case eAssetImageType.Program:
                    return KalturaImageObjectType.PROGRAM_ASSET;
                case eAssetImageType.Channel:
                    return KalturaImageObjectType.CHANNEL;
                case eAssetImageType.Category:
                    return KalturaImageObjectType.CATEGORY;
                case eAssetImageType.ImageType:
                    return KalturaImageObjectType.IMAGE_TYPE;
                case eAssetImageType.ProgramGroup:
                    return KalturaImageObjectType.PROGRAM_GROUP;
                case eAssetImageType.LogoPic:
                    return KalturaImageObjectType.PARTNER;
                case eAssetImageType.DefaultPic:
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown image object type");
            }
        }

        public static eAssetImageType ConvertImageObjectType(KalturaImageObjectType imageObjectType)
        {
            switch (imageObjectType)
            {
                case KalturaImageObjectType.MEDIA_ASSET:
                    return eAssetImageType.Media;
                case KalturaImageObjectType.PROGRAM_ASSET:
                    return eAssetImageType.Program;
                case KalturaImageObjectType.CHANNEL:
                    return eAssetImageType.Channel;
                case KalturaImageObjectType.CATEGORY:
                    return eAssetImageType.Category;
                case KalturaImageObjectType.IMAGE_TYPE:
                    return eAssetImageType.ImageType;
                case KalturaImageObjectType.PROGRAM_GROUP:
                    return eAssetImageType.ProgramGroup;
                case KalturaImageObjectType.PARTNER:
                    return eAssetImageType.LogoPic;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown image object type");
            }
        }

        private static KalturaImageStatus ConvertImageStatus(eTableStatus status)
        {
            switch (status)
            {
                case eTableStatus.Pending:
                    return KalturaImageStatus.PENDING;
                    break;
                case eTableStatus.OK:
                    return KalturaImageStatus.READY;
                    break;
                case eTableStatus.Failed:
                    return KalturaImageStatus.FAILED;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown image status");
                    break;
            }
        }

        private static eTableStatus ConvertImageStatus(KalturaImageStatus status)
        {
            switch (status)
            {
                case KalturaImageStatus.PENDING:
                    return eTableStatus.Pending;
                    break;
                case KalturaImageStatus.READY:
                    return eTableStatus.OK;
                    break;
                case KalturaImageStatus.FAILED:
                    return eTableStatus.Failed;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown image status");
                    break;
            }
        }

        //KalturaAssetType to eAssetType
        public static eAssetTypes ConvertToAssetTypes(KalturaAssetReferenceType assetReferenceType)
        {
            eAssetTypes assetType = eAssetTypes.UNKNOWN;
            switch (assetReferenceType)
            {
                case KalturaAssetReferenceType.media:
                    assetType = eAssetTypes.MEDIA;
                    break;
                case KalturaAssetReferenceType.epg_internal:
                    assetType = eAssetTypes.EPG;
                    break;
                case KalturaAssetReferenceType.epg_external:
                    assetType = eAssetTypes.EPG;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Invalid assetType");
                    break;
            }

            return assetType;
        }

        public static List<KalturaMediaImage> ConvertImageListToKalturaMediaImageList(List<Image> images, Dictionary<long, string> imageTypeIdToRatioNameMap)
        {
            List<KalturaMediaImage> result = new List<KalturaMediaImage>();
            if (images != null && images.Count > 0)
            {
                foreach (Image image in images)
                {
                    string ratioName = !string.IsNullOrEmpty(image.RatioName) ? image.RatioName :
                        imageTypeIdToRatioNameMap != null && imageTypeIdToRatioNameMap.ContainsKey(image.ImageTypeId) ?
                            imageTypeIdToRatioNameMap[image.ImageTypeId] : string.Empty;

                    image.RatioName = ratioName;
                    KalturaMediaImage convertedImage = Mapper.Map<KalturaMediaImage>(image);

                    if (convertedImage != null)
                    {
                        result.Add(convertedImage);
                    }
                }
            }

            return result;
        }

        private static string ConvertAssetGroupByToGroupBy(KalturaAssetGroupBy groupBy)
        {
            if (groupBy == null)
            {
                return string.Empty;
            }
            return groupBy.GetValue();
        }

        private static KalturaAssetGroupBy ConvertGroupByToAssetGroupBy(string groupBy)
        {
            KalturaAssetGroupBy kalturaAssetGroupBy;

            if (Enum.IsDefined(typeof(KalturaGroupByField), groupBy))
            {
                kalturaAssetGroupBy = new KalturaAssetFieldGroupBy();
                KalturaGroupByField groupByField = (KalturaGroupByField)Enum.Parse(typeof(KalturaGroupByField), groupBy);

                ((KalturaAssetFieldGroupBy)kalturaAssetGroupBy).Value = groupByField;

            }
            else
            {
                kalturaAssetGroupBy = new KalturaAssetMetaOrTagGroupBy();
                ((KalturaAssetMetaOrTagGroupBy)kalturaAssetGroupBy).Value = groupBy;
            }
            return kalturaAssetGroupBy;
        }

        public static KalturaAssetOrderBy ConvertOrderObjToAssetOrder(OrderObj orderObj)
        {
            if (orderObj == null)
            {
                return KalturaAssetOrderBy.START_DATE_DESC;
            }

            return ConvertOrderObjToAssetOrder(orderObj.m_eOrderBy, orderObj.m_eOrderDir);
        }

        public static OrderObj ConvertAssetOrderToOrderObj(KalturaAssetOrderBy order)
        {
            OrderObj result = new OrderObj();

            switch (order)
            {
                case KalturaAssetOrderBy.NAME_ASC:
                    result.m_eOrderBy = OrderBy.NAME;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                    break;
                case KalturaAssetOrderBy.NAME_DESC:
                    result.m_eOrderBy = OrderBy.NAME;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.VIEWS_DESC:
                    result.m_eOrderBy = OrderBy.VIEWS;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.RATINGS_DESC:
                    result.m_eOrderBy = OrderBy.RATING;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.VOTES_DESC:
                    result.m_eOrderBy = OrderBy.VOTES_COUNT;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.START_DATE_DESC:
                    result.m_eOrderBy = OrderBy.START_DATE;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.RELEVANCY_DESC:
                    result.m_eOrderBy = OrderBy.RELATED;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.START_DATE_ASC:
                    result.m_eOrderBy = OrderBy.START_DATE;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                    break;
                case KalturaAssetOrderBy.CREATE_DATE_ASC:
                    result.m_eOrderBy = OrderBy.CREATE_DATE;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                    break;
                case KalturaAssetOrderBy.CREATE_DATE_DESC:
                    result.m_eOrderBy = OrderBy.CREATE_DATE;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.LIKES_DESC:
                    result.m_eOrderBy = OrderBy.LIKE_COUNTER;
                    result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
            }

            return result;
        }

        public static OrderObj ConvertAssetOrderToOrderObj(KalturaChannelOrder order)
        {
            OrderObj result = null;
            if (order != null)
            {
                result = new OrderObj();
                if (order.DynamicOrderBy != null)
                {
                    result.m_eOrderBy = OrderBy.META;
                    result.m_eOrderDir = order.DynamicOrderBy.OrderBy.HasValue ? order.DynamicOrderBy.OrderBy.Value == KalturaMetaTagOrderBy.META_ASC ? ApiObjects.SearchObjects.OrderDir.ASC : ApiObjects.SearchObjects.OrderDir.DESC : ApiObjects.SearchObjects.OrderDir.ASC;
                    result.m_sOrderValue = order.DynamicOrderBy.Name;
                }
                else
                {
                    result.m_sOrderValue = null;
                    switch (order.orderBy)
                    {
                        case KalturaChannelOrderBy.NAME_ASC:
                            result.m_eOrderBy = OrderBy.NAME;
                            result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                            break;
                        case KalturaChannelOrderBy.NAME_DESC:
                            result.m_eOrderBy = OrderBy.NAME;
                            result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                            break;
                        case KalturaChannelOrderBy.VIEWS_DESC:
                            result.m_eOrderBy = OrderBy.VIEWS;
                            result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                            break;
                        case KalturaChannelOrderBy.RATINGS_DESC:
                            result.m_eOrderBy = OrderBy.RATING;
                            result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                            break;
                        case KalturaChannelOrderBy.VOTES_DESC:
                            result.m_eOrderBy = OrderBy.VOTES_COUNT;
                            result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                            break;
                        case KalturaChannelOrderBy.START_DATE_DESC:
                            result.m_eOrderBy = OrderBy.START_DATE;
                            result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                            break;
                        case KalturaChannelOrderBy.RELEVANCY_DESC:
                            result.m_eOrderBy = OrderBy.RELATED;
                            result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                            break;
                        case KalturaChannelOrderBy.START_DATE_ASC:
                            result.m_eOrderBy = OrderBy.START_DATE;
                            result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                            break;
                        case KalturaChannelOrderBy.CREATE_DATE_ASC:
                            result.m_eOrderBy = OrderBy.CREATE_DATE;
                            result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                            break;
                        case KalturaChannelOrderBy.CREATE_DATE_DESC:
                            result.m_eOrderBy = OrderBy.CREATE_DATE;
                            result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                            break;
                        case KalturaChannelOrderBy.LIKES_DESC:
                            result.m_eOrderBy = OrderBy.LIKE_COUNTER;
                            result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                            break;
                        case KalturaChannelOrderBy.ORDER_NUM:
                            result.m_eOrderBy = OrderBy.ID;
                            result.m_eOrderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                            break;
                    }

                    if (order.SlidingWindowPeriod.HasValue)
                    {
                        result.m_bIsSlidingWindowField = result.isSlidingWindowFromRestApi = true;
                        result.lu_min_period_id = order.SlidingWindowPeriod.Value;
                    }
                }
            }

            return result;
        }

        public static KalturaChannelOrder ConvertToKalturaChannelOrder(OrderObj orderObj)
        {
            KalturaChannelOrder result = new KalturaChannelOrder();

            switch (orderObj.m_eOrderBy)
            {
                case OrderBy.VIEWS:
                    {
                        result.orderBy = KalturaChannelOrderBy.VIEWS_DESC;
                        break;
                    }
                case OrderBy.RATING:
                    {
                        result.orderBy = KalturaChannelOrderBy.RATINGS_DESC;
                        break;
                    }
                case OrderBy.VOTES_COUNT:
                    {
                        result.orderBy = KalturaChannelOrderBy.VOTES_DESC;
                        break;
                    }
                case OrderBy.START_DATE:
                    {
                        if (orderObj.m_eOrderDir == ApiObjects.SearchObjects.OrderDir.DESC)
                        {
                            result.orderBy = KalturaChannelOrderBy.START_DATE_DESC;
                        }
                        else
                        {
                            result.orderBy = KalturaChannelOrderBy.START_DATE_ASC;
                        }
                        break;
                    }
                case OrderBy.CREATE_DATE:
                    {
                        if (orderObj.m_eOrderDir == ApiObjects.SearchObjects.OrderDir.DESC)
                        {
                            result.orderBy = KalturaChannelOrderBy.CREATE_DATE_DESC;
                        }
                        else
                        {
                            result.orderBy = KalturaChannelOrderBy.CREATE_DATE_ASC;
                        }
                        break;
                    }
                case OrderBy.NAME:
                    {
                        if (orderObj.m_eOrderDir == ApiObjects.SearchObjects.OrderDir.ASC)
                        {
                            result.orderBy = KalturaChannelOrderBy.NAME_ASC;
                        }
                        else
                        {
                            result.orderBy = KalturaChannelOrderBy.NAME_DESC;
                        }
                        break;
                    }
                case OrderBy.RELATED:
                    {
                        result.orderBy = KalturaChannelOrderBy.RELEVANCY_DESC;
                        break;
                    }
                case OrderBy.META:
                    {
                        KalturaMetaTagOrderBy metaOrderBy = orderObj.m_eOrderDir == ApiObjects.SearchObjects.OrderDir.DESC ? KalturaMetaTagOrderBy.META_DESC : KalturaMetaTagOrderBy.META_ASC;
                        result.DynamicOrderBy = new KalturaDynamicOrderBy() { OrderBy = metaOrderBy, Name = orderObj.m_sOrderValue };
                        break;
                    }
                case OrderBy.RECOMMENDATION:
                case OrderBy.RANDOM:
                case OrderBy.LIKE_COUNTER:
                case OrderBy.NONE:
                case OrderBy.ID:
                default:
                    break;
            }

            if (orderObj.isSlidingWindowFromRestApi)
            {
                result.SlidingWindowPeriod = orderObj.lu_min_period_id;
            }

            return result;
        }

        public static OrderBy ConvertToOrderBy(KalturaChannelOrder kalturaChannelOrder)
        {
            OrderBy orderBy = OrderBy.NONE;

            switch (kalturaChannelOrder.orderBy.Value)
            {
                case KalturaChannelOrderBy.RELEVANCY_DESC:
                    orderBy = OrderBy.RELATED;
                    break;
                case KalturaChannelOrderBy.NAME_ASC:
                case KalturaChannelOrderBy.NAME_DESC:
                    orderBy = OrderBy.NAME;
                    break;
                case KalturaChannelOrderBy.VIEWS_DESC:
                    orderBy = OrderBy.VIEWS;
                    break;
                case KalturaChannelOrderBy.RATINGS_DESC:
                    orderBy = OrderBy.RATING;
                    break;
                case KalturaChannelOrderBy.VOTES_DESC:
                    orderBy = OrderBy.VOTES_COUNT;
                    break;
                case KalturaChannelOrderBy.START_DATE_DESC:
                case KalturaChannelOrderBy.START_DATE_ASC:
                    orderBy = OrderBy.START_DATE;
                    break;
                case KalturaChannelOrderBy.LIKES_DESC:
                    break;
                case KalturaChannelOrderBy.CREATE_DATE_ASC:
                case KalturaChannelOrderBy.CREATE_DATE_DESC:
                    orderBy = OrderBy.CREATE_DATE;
                    break;
                case KalturaChannelOrderBy.ORDER_NUM:
                default:
                    break;
            }

            return orderBy;
        }

        public static ApiObjects.SearchObjects.OrderDir ConvertToOrderDir(KalturaChannelOrder kalturaChannelOrder)
        {
            ApiObjects.SearchObjects.OrderDir orderDir = ApiObjects.SearchObjects.OrderDir.NONE;

            switch (kalturaChannelOrder.orderBy.Value)
            {
                case KalturaChannelOrderBy.NAME_DESC:
                case KalturaChannelOrderBy.RELEVANCY_DESC:
                case KalturaChannelOrderBy.VIEWS_DESC:
                case KalturaChannelOrderBy.RATINGS_DESC:
                case KalturaChannelOrderBy.VOTES_DESC:
                case KalturaChannelOrderBy.LIKES_DESC:
                case KalturaChannelOrderBy.START_DATE_DESC:
                case KalturaChannelOrderBy.CREATE_DATE_DESC:
                    orderDir = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaChannelOrderBy.NAME_ASC:
                case KalturaChannelOrderBy.START_DATE_ASC:
                case KalturaChannelOrderBy.CREATE_DATE_ASC:
                    orderDir = ApiObjects.SearchObjects.OrderDir.ASC;
                    break;
                case KalturaChannelOrderBy.ORDER_NUM:
                default:
                    break;
            }

            return orderDir;
        }

        private static KalturaTimeShiftedTvState? ConvertToKalturaTimeShiftedTvState(TstvState? tstvState)
        {
            KalturaTimeShiftedTvState? response = null;
            if (tstvState.HasValue)
            {
                switch (tstvState.Value)
                {
                    case TstvState.Inherited:
                        response = KalturaTimeShiftedTvState.INHERITED;
                        break;
                    case TstvState.Enabled:
                        response = KalturaTimeShiftedTvState.ENABLED;
                        break;
                    case TstvState.Disabled:
                        response = KalturaTimeShiftedTvState.DISABLED;
                        break;
                    default:
                        throw new ClientException((int)StatusCode.Error, "Unknown TstvState");
                }
            }

            return response;
        }

        private static TstvState? ConvertToTstvState(KalturaTimeShiftedTvState? timeShiftedTvState)
        {
            TstvState? response = null;
            if (timeShiftedTvState.HasValue)
            {
                switch (timeShiftedTvState.Value)
                {
                    case KalturaTimeShiftedTvState.INHERITED:
                        response = TstvState.Inherited;
                        break;
                    case KalturaTimeShiftedTvState.ENABLED:
                        response = TstvState.Enabled;
                        break;
                    case KalturaTimeShiftedTvState.DISABLED:
                        response = TstvState.Disabled;
                        break;
                    default:
                        throw new ClientException((int)StatusCode.Error, "Unknown KalturaTimeShiftedTvState");
                        break;
                }
            }

            return response;
        }

        private static KalturaLinearChannelType? ConvertToKalturaLinearChannelType(LinearChannelType? channelType)
        {
            KalturaLinearChannelType? response = null;
            if (channelType.HasValue)
            {
                switch (channelType.Value)
                {
                    case LinearChannelType.Dtt:
                        response = KalturaLinearChannelType.DTT;
                        break;
                    case LinearChannelType.Dtt_and_ott:
                        response = KalturaLinearChannelType.DTT_AND_OTT;
                        break;
                    case LinearChannelType.Ott:
                        response = KalturaLinearChannelType.OTT;
                        break;
                    case LinearChannelType.Vrm_export:
                        response = KalturaLinearChannelType.VRM_EXPORT;
                        break;
                    case LinearChannelType.Unknown:
                        response = KalturaLinearChannelType.UNKNOWN;
                        break;
                    default:
                        throw new ClientException((int)StatusCode.Error, "Unknown LinearChannelType");
                }
            }

            return response;
        }

        private static LinearChannelType? ConvertToLinearChannelType(KalturaLinearChannelType? channelType)
        {
            LinearChannelType? response = null;
            if (channelType.HasValue)
            {
                switch (channelType.Value)
                {
                    case KalturaLinearChannelType.DTT:
                        response = LinearChannelType.Dtt;
                        break;
                    case KalturaLinearChannelType.DTT_AND_OTT:
                        response = LinearChannelType.Dtt_and_ott;
                        break;
                    case KalturaLinearChannelType.OTT:
                        response = LinearChannelType.Ott;
                        break;
                    case KalturaLinearChannelType.VRM_EXPORT:
                        response = LinearChannelType.Vrm_export;
                        break;
                    case KalturaLinearChannelType.UNKNOWN:
                        response = LinearChannelType.Unknown;
                        break;
                    default:
                        throw new ClientException((int)StatusCode.Error, "Unknown KalturaLinearChannelType");
                        break;
                }
            }

            return response;
        }

        #endregion

        private static KalturaAssetGroupBy ConvertToGroupBy(SearchAggregationGroupBy searchAggregationGroupBy)
        {
            KalturaAssetGroupBy kalturaAssetGroupBy = null;

            if (searchAggregationGroupBy != null && searchAggregationGroupBy.groupBy != null && searchAggregationGroupBy.groupBy.Count() > 0)
            {

                if (Enum.IsDefined(typeof(KalturaGroupByField), searchAggregationGroupBy.groupBy.FirstOrDefault()))
                {
                    kalturaAssetGroupBy = new KalturaAssetFieldGroupBy();

                    KalturaGroupByField groupByField = (KalturaGroupByField)Enum.Parse(typeof(KalturaGroupByField), searchAggregationGroupBy.groupBy.FirstOrDefault());

                    ((KalturaAssetFieldGroupBy)kalturaAssetGroupBy).Value = groupByField;
                }
                else
                {
                    kalturaAssetGroupBy = new KalturaAssetMetaOrTagGroupBy();
                    ((KalturaAssetMetaOrTagGroupBy)kalturaAssetGroupBy).Value = searchAggregationGroupBy.groupBy.FirstOrDefault();
                }
            }

            return kalturaAssetGroupBy;
        }

        private static SearchAggregationGroupBy ConvertToGroupBy(KalturaAssetGroupBy kalturaAssetGroupBy)
        {
            SearchAggregationGroupBy searchAggregationGroupBy = new SearchAggregationGroupBy();
            if (kalturaAssetGroupBy != null)
            {
                searchAggregationGroupBy.groupBy = new List<string>();
                searchAggregationGroupBy.groupBy.Add(kalturaAssetGroupBy.GetValue());
                searchAggregationGroupBy.distinctGroup = kalturaAssetGroupBy.GetValue();
                searchAggregationGroupBy.topHitsCount = 1;
            }
            return searchAggregationGroupBy;
        }

        //eAssetTypes to KalturaAssetType
        public static KalturaAssetType ConvertAssetType(eAssetTypes assetType)
        {
            KalturaAssetType result;
            switch (assetType)
            {
                case eAssetTypes.EPG:
                    result = KalturaAssetType.epg;
                    break;
                case eAssetTypes.NPVR:
                    result = KalturaAssetType.recording;
                    break;
                case eAssetTypes.MEDIA:
                    result = KalturaAssetType.media;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown Asset Type");
            }

            return result;
        }

        //eAssetType to KalturaAssetType
        public static KalturaAssetType ConvertToKalturaAssetType(eAssetType assetType)
        {
            KalturaAssetType result;
            switch (assetType)
            {
                case eAssetType.PROGRAM:
                    result = KalturaAssetType.epg;
                    break;
                case eAssetType.MEDIA:
                    result = KalturaAssetType.media;
                    break;
                case eAssetType.UNKNOWN:
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown Asset Type");
            }

            return result;
        }

        //KalturaAssetType to eAssetType
        public static eAssetType ConvertToAssetType(KalturaAssetType assetType)
        {
            eAssetType result;
            switch (assetType)
            {
                case KalturaAssetType.epg:
                    result = eAssetType.PROGRAM;
                    break;
                case KalturaAssetType.media:
                    result = eAssetType.MEDIA;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown Asset Type");
            }

            return result;
        }

        // eUserType to KalturaPositionOwner 
        public static KalturaPositionOwner ConvertPositionOwner(eUserType userType)
        {
            KalturaPositionOwner result;
            switch (userType)
            {
                case eUserType.HOUSEHOLD:
                    result = KalturaPositionOwner.household;
                    break;
                case eUserType.PERSONAL:
                    result = KalturaPositionOwner.user;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown position owner");
            }

            return result;
        }

        // KalturaWatchStatus to eWatchStatus
        public static eWatchStatus ConvertKalturaWatchStatus(KalturaWatchStatus watchStatus)
        {
            eWatchStatus result;
            switch (watchStatus)
            {
                case KalturaWatchStatus.progress:
                    result = eWatchStatus.Progress;
                    break;
                case KalturaWatchStatus.done:
                    result = eWatchStatus.Done;
                    break;
                case KalturaWatchStatus.all:
                    result = eWatchStatus.All;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown watch status");
            }
            return result;
        }

        private static int GetPictureWidth(string size)
        {
            string[] sizeArr = size.ToLower().Split('x');

            if (sizeArr != null && sizeArr.Length == 2)
            {
                int width;

                if (int.TryParse(sizeArr[0].Trim(), out width))
                {
                    return width;
                }
            }

            return 0;
        }

        private static int GetPictureHeight(string size)
        {
            string[] sizeArr = size.ToLower().Split('x');

            if (sizeArr != null && sizeArr.Length == 2)
            {
                int height;

                if (int.TryParse(sizeArr[1].Trim(), out height))
                {
                    return height;
                }
            }

            return 0;
        }

        private static Dictionary<string, KalturaStringValue> BuildExtraParamsDictionary(EPGChannelProgrammeObject epg)
        {
            Dictionary<string, KalturaStringValue> extraParams = new Dictionary<string, KalturaStringValue>();

            extraParams.Add("epg_channel_id", new KalturaStringValue() { value = epg.EPG_CHANNEL_ID });
            extraParams.Add("epg_id", new KalturaStringValue() { value = epg.EPG_IDENTIFIER });
            extraParams.Add("related_media_id", new KalturaStringValue() { value = epg.media_id });

            extraParams.Add("enable_cdvr", new KalturaStringValue() { value = epg.ENABLE_CDVR == 1 ? "True" : "False" });
            extraParams.Add("enable_catch_up", new KalturaStringValue() { value = epg.ENABLE_CATCH_UP == 1 ? "True" : "False" });
            extraParams.Add("enable_start_over", new KalturaStringValue() { value = epg.ENABLE_START_OVER == 1 ? "True" : "False" });
            extraParams.Add("enable_trick_play", new KalturaStringValue() { value = epg.ENABLE_TRICK_PLAY == 1 ? "True" : "False" });
            extraParams.Add("crid", new KalturaStringValue() { value = epg.CRID });

            return extraParams;
        }

        private static Dictionary<string, KalturaMultilingualStringValueArray> BuildTagsDictionary(List<EPGDictionary> list)
        {
            if (list == null)
            {
                return null;
            }

            SerializableDictionary<string, KalturaMultilingualStringValueArray> tags = new SerializableDictionary<string, KalturaMultilingualStringValueArray>();
            KalturaMultilingualStringValueArray tagsList;

            foreach (var tag in list)
            {
                KalturaMultilingualStringValue valueToAdd = null;

                if (tag.Values != null)
                {
                    valueToAdd = new KalturaMultilingualStringValue()
                    {
                        value = new KalturaMultilingualString(tag.Values)
                    };
                }
                else
                {
                    LanguageContainer[] containers = new LanguageContainer[1]
                        {
                            new LanguageContainer()
                            {
                                m_sLanguageCode3 = WebAPI.Utils.Utils.GetDefaultLanguage(),
                                m_sValue = tag.Value
                            }
                        };

                    valueToAdd = new KalturaMultilingualStringValue()
                    {
                        value = new KalturaMultilingualString(containers)
                    };
                }

                if (tags.ContainsKey(tag.Key))
                {
                    tags[tag.Key].Objects.Add(valueToAdd);
                }
                else
                {
                    tagsList = new KalturaMultilingualStringValueArray();
                    tagsList.Objects.Add(valueToAdd);
                    tags.Add(tag.Key, tagsList);
                }
            }

            return tags;
        }

        private static Dictionary<string, KalturaMultilingualStringValue> BuildMetasDictionary(List<EPGDictionary> list)
        {
            if (list == null)
            {
                return null;
            }

            Dictionary<string, KalturaMultilingualStringValue> metas = new Dictionary<string, KalturaMultilingualStringValue>();

            foreach (var meta in list)
            {
                if (meta.Values != null)
                {
                    metas.Add(meta.Key, new KalturaMultilingualStringValue()
                    {
                        value = new KalturaMultilingualString(meta.Values)
                    });
                }
                else
                {
                    LanguageContainer[] containers = new LanguageContainer[1]
                    {
                        new LanguageContainer()
                        {
                            m_sLanguageCode3 = WebAPI.Utils.Utils.GetDefaultLanguage(),
                            m_sValue = meta.Value
                        }
                    };

                    metas.Add(meta.Key, new KalturaMultilingualStringValue()
                    {
                        value = new KalturaMultilingualString(containers)
                    });
                }
            }

            return metas;
        }

        private static Dictionary<string, KalturaStringValue> BuildExtraParamsDictionary(MediaObj media)
        {
            Dictionary<string, KalturaStringValue> extraParams = new Dictionary<string, KalturaStringValue>();

            extraParams.Add("sys_start_date", new KalturaStringValue() { value = DateUtils.DateTimeToUtcUnixTimestampSeconds(media.m_dStartDate).ToString() });
            extraParams.Add("sys_final_date", new KalturaStringValue() { value = DateUtils.DateTimeToUtcUnixTimestampSeconds(media.m_dFinalDate).ToString() });
            extraParams.Add("external_ids", new KalturaStringValue() { value = media.m_ExternalIDs });

            extraParams.Add("enable_cdvr", new KalturaStringValue() { value = media.EnableCDVR.ToString() });
            extraParams.Add("enable_catch_up", new KalturaStringValue() { value = media.EnableCatchUp.ToString() });
            extraParams.Add("enable_start_over", new KalturaStringValue() { value = media.EnableStartOver.ToString() });
            extraParams.Add("enable_trick_play", new KalturaStringValue() { value = media.EnableTrickPlay.ToString() });

            extraParams.Add("catch_up_buffer", new KalturaStringValue() { value = media.CatchUpBuffer.ToString() });
            extraParams.Add("trick_play_buffer", new KalturaStringValue() { value = media.TrickPlayBuffer.ToString() });

            extraParams.Add("enable_recording_playback_non_entitled_channel", new KalturaStringValue() { value = media.EnableRecordingPlaybackNonEntitledChannel.ToString() });


            return extraParams;
        }

        private static SerializableDictionary<string, KalturaMultilingualStringValueArray> BuildTagsDictionary(List<Tags> list)
        {
            if (list == null)
            {
                return null;
            }

            SerializableDictionary<string, KalturaMultilingualStringValueArray> tags = new SerializableDictionary<string, KalturaMultilingualStringValueArray>();

            foreach (var tag in list)
            {
                tags.Add(tag.m_oTagMeta.m_sName, new KalturaMultilingualStringValueArray()
                {
                    Objects = tag.Values.Select(v => new KalturaMultilingualStringValue() { value = new KalturaMultilingualString(v) }).ToList()
                });
            }

            return tags;
        }

        private static Dictionary<string, KalturaValue> BuildMetasDictionary(List<Metas> list)
        {
            if (list == null)
            {
                return null;
            }

            Dictionary<string, KalturaValue> metas = new Dictionary<string, KalturaValue>();

            KalturaValue value = null;
            foreach (var meta in list)
            {
                string currentMetaType = meta.m_oTagMeta.m_sType;
                string currentMetaTypeLowered = currentMetaType.ToLower();

                if (currentMetaTypeLowered == typeof(bool).ToString().ToLower() || currentMetaType == ApiObjects.MetaType.Bool.ToString())
                {
                    value = new KalturaBooleanValue() { value = meta.m_sValue == "1" ? true : false };
                }
                else if (currentMetaType == ApiObjects.MetaType.String.ToString())
                {
                    value = new KalturaStringValue() { value = meta.m_sValue };
                }
                else if (currentMetaTypeLowered == typeof(string).ToString().ToLower() || currentMetaType == ApiObjects.MetaType.MultilingualString.ToString())
                {
                    if (string.IsNullOrEmpty(meta.m_sValue))
                    {
                        value = new KalturaMultilingualStringValue() { value = new KalturaMultilingualString(meta.Value) };
                    }
                    else
                    {
                        value = new KalturaMultilingualStringValue() { value = new KalturaMultilingualString(meta.Value.ToList(), meta.m_sValue) };
                    }
                }
                else if (currentMetaTypeLowered == typeof(double).ToString().ToLower() || currentMetaType == ApiObjects.MetaType.Number.ToString())
                {
                    value = new KalturaDoubleValue() { value = double.Parse(meta.m_sValue, NumberStyles.Float, CultureInfo.InvariantCulture) };
                }
                else if (currentMetaTypeLowered == typeof(DateTime).ToString().ToLower() || currentMetaType == ApiObjects.MetaType.DateTime.ToString())
                {
                    if (!string.IsNullOrEmpty(meta.m_sValue))
                    {
                        value = new KalturaLongValue() { value = DateUtils.StringToUtcUnixTimestampSeconds(meta.m_sValue) };
                    }
                }
                else
                {
                    throw new ClientException((int)StatusCode.Error, "Unknown meta type");
                }

                metas.Add(meta.m_oTagMeta.m_sName, value);
            }

            return metas;
        }

        public static KalturaBookmark ConvertBookmark(AssetBookmarks assetBookmark, Bookmark catalogBookmark)
        {
            return new KalturaBookmark()
            {
                Id = assetBookmark.AssetID,
                IsFinishedWatching = catalogBookmark.IsFinishedWatching,
                Position = catalogBookmark.Location,
                PositionOwner = ConvertPositionOwner(catalogBookmark.UserType),
                Type = ConvertAssetType(assetBookmark.AssetType),
                UserId = catalogBookmark.User.m_sSiteGUID,
                User = Mapper.Map<WebAPI.Models.Users.KalturaBaseOTTUser>(catalogBookmark.User)
            };
        }

        public static List<KalturaBookmark> ConvertBookmarks(List<AssetBookmarks> assetBookmarks, KalturaBookmarkOrderBy orderBy)
        {
            List<KalturaBookmark> bookmarks = null;
            if (assetBookmarks != null)
            {
                bookmarks = new List<KalturaBookmark>();

                foreach (var assetBookmark in assetBookmarks)
                {
                    if (assetBookmark.Bookmarks != null)
                    {
                        foreach (var catalogBookmark in assetBookmark.Bookmarks)
                        {
                            bookmarks.Add(ConvertBookmark(assetBookmark, catalogBookmark));
                        }
                    }
                }

                switch (orderBy)
                {
                    case KalturaBookmarkOrderBy.POSITION_ASC:
                        bookmarks = bookmarks.OrderBy(b => b.Position).ToList();
                        break;
                    case KalturaBookmarkOrderBy.POSITION_DESC:
                        bookmarks = bookmarks.OrderByDescending(b => b.Position).ToList();
                        break;
                    default:
                        break;
                }

            }
            return bookmarks;
        }

        //KalturaAssetType to StatsType
        public static StatsType ConvertAssetType(KalturaAssetType assetType)
        {
            StatsType result;
            switch (assetType)
            {
                case KalturaAssetType.epg:
                    result = StatsType.EPG;
                    break;
                case KalturaAssetType.media:
                    result = StatsType.MEDIA;
                    break;
                case KalturaAssetType.recording:
                    throw new ClientException((int)StatusCode.Error, "recording is not supported");
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown Asset Type");
            }

            return result;
        }

        public static KalturaAssetOrderBy ConvertOrderObjToAssetOrder(OrderBy orderBy, ApiObjects.SearchObjects.OrderDir orderDir)
        {
            KalturaAssetOrderBy result = KalturaAssetOrderBy.START_DATE_DESC;

            switch (orderBy)
            {
                case OrderBy.VIEWS:
                    {
                        result = KalturaAssetOrderBy.VIEWS_DESC;
                        break;
                    }
                case OrderBy.RATING:
                    {
                        result = KalturaAssetOrderBy.RATINGS_DESC;
                        break;
                    }
                case OrderBy.VOTES_COUNT:
                    {
                        result = KalturaAssetOrderBy.VOTES_DESC;
                        break;
                    }
                case OrderBy.START_DATE:
                    {
                        if (orderDir == ApiObjects.SearchObjects.OrderDir.DESC)
                        {
                            result = KalturaAssetOrderBy.START_DATE_DESC;
                        }
                        else
                        {
                            result = KalturaAssetOrderBy.START_DATE_ASC;
                        }
                        break;
                    }
                case OrderBy.CREATE_DATE:
                    {
                        if (orderDir == ApiObjects.SearchObjects.OrderDir.DESC)
                        {
                            result = KalturaAssetOrderBy.CREATE_DATE_DESC;
                        }
                        else
                        {
                            result = KalturaAssetOrderBy.CREATE_DATE_ASC;
                        }
                        break;
                    }
                case OrderBy.NAME:
                    {
                        if (orderDir == ApiObjects.SearchObjects.OrderDir.ASC)
                        {
                            result = KalturaAssetOrderBy.NAME_ASC;
                        }
                        else
                        {
                            result = KalturaAssetOrderBy.NAME_DESC;
                        }
                        break;
                    }
                case OrderBy.RELATED:
                    {
                        result = KalturaAssetOrderBy.RELEVANCY_DESC;
                        break;
                    }
                case OrderBy.META:
                case OrderBy.RECOMMENDATION:
                case OrderBy.RANDOM:
                case OrderBy.LIKE_COUNTER:
                case OrderBy.NONE:
                case OrderBy.ID:
                default:
                    break;
            }

            return result;
        }

        public static KalturaScheduledRecordingAssetType ConvertScheduledRecordingAssetType(ScheduledRecordingAssetType scheduledRecordingAssetType)
        {
            KalturaScheduledRecordingAssetType result;
            switch (scheduledRecordingAssetType)
            {
                case ScheduledRecordingAssetType.SINGLE:
                    result = KalturaScheduledRecordingAssetType.single;
                    break;
                case ScheduledRecordingAssetType.SERIES:
                    result = KalturaScheduledRecordingAssetType.series;
                    break;
                case ScheduledRecordingAssetType.ALL:
                    result = KalturaScheduledRecordingAssetType.all;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown ScheduledRecordingAssetType");
            }

            return result;
        }

        public static ScheduledRecordingAssetType ConvertKalturaScheduledRecordingAssetType(KalturaScheduledRecordingAssetType scheduledRecordingAssetType)
        {
            ScheduledRecordingAssetType result;
            switch (scheduledRecordingAssetType)
            {
                case KalturaScheduledRecordingAssetType.single:
                    result = ScheduledRecordingAssetType.SINGLE;
                    break;
                case KalturaScheduledRecordingAssetType.series:
                    result = ScheduledRecordingAssetType.SERIES;
                    break;
                case KalturaScheduledRecordingAssetType.all:
                    result = ScheduledRecordingAssetType.ALL;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown KalturaScheduledRecordingAssetType");
            }

            return result;
        }

        public static List<KalturaLastPosition> ConvertBookmarks(List<AssetBookmarks> assetBookmarks)
        {
            List<KalturaLastPosition> lastPositions = null;
            if (assetBookmarks != null)
            {
                lastPositions = new List<KalturaLastPosition>();

                foreach (var assetBookmark in assetBookmarks)
                {
                    if (assetBookmark.Bookmarks != null)
                    {
                        foreach (var catalogBookmark in assetBookmark.Bookmarks)
                        {
                            lastPositions.Add(new KalturaLastPosition()
                            {
                                Position = catalogBookmark.Location,
                                PositionOwner = ConvertPositionOwner(catalogBookmark.UserType),
                                UserId = catalogBookmark.User.m_sSiteGUID,
                            });
                        }
                    }
                }
            }
            return lastPositions;
        }

        public static KalturaStringValueArray BuildPPVModulesList(List<string> list)
        {
            if (list == null)
            {
                return null;
            }

            KalturaStringValueArray ppvModules = new KalturaStringValueArray();

            foreach (var ppvModule in list)
            {
                ppvModules.Objects.Add(new KalturaStringValue() { value = ppvModule });
            }

            return ppvModules;
        }

        public static string GetPPVModule(KalturaStringValueArray ppvModules)
        {
            string ppvModule = null;
            if (ppvModules != null && ppvModules.Objects != null && ppvModules.Objects.Count > 0)
            {
                var stringValue = ppvModules.Objects.FirstOrDefault();
                if (stringValue != null)
                {
                    ppvModule = stringValue.value;
                }
            }

            return ppvModule;
        }

        private static List<RelatedEntities> GetRelatedEntitiesList(SerializableDictionary<string, KalturaRelatedEntityArray> relatedEntitiesDictionary)
        {
            List<RelatedEntities> relatedEntitiesList = new List<RelatedEntities>();
            RelatedEntities relatedEntitiesToAdd = null;
            RelatedEntity relatedEntity = null;

            if (relatedEntitiesDictionary == null || relatedEntitiesDictionary.Count == 0)
            {
                return relatedEntitiesList;
            }

            HashSet<string> relatedEntityNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (KeyValuePair<string, KalturaRelatedEntityArray> relatedEntities in relatedEntitiesDictionary)
            {
                if (relatedEntityNames.Contains(relatedEntities.Key))
                {
                    throw new ClientException((int)StatusCode.Error, string.Format("The request contains relatedEntity with the name {0} more than once", relatedEntities.Key));
                }

                relatedEntitiesToAdd = new RelatedEntities() { TagMeta = new TagMeta(relatedEntities.Key, ApiObjects.MetaType.ReleatedEntity.ToString()) };
                relatedEntitiesToAdd.Items = new List<RelatedEntity>();

                if (relatedEntities.Value.Objects != null)
                {
                    foreach (KalturaRelatedEntity kalturaRelatedEntity in relatedEntities.Value.Objects)
                    {
                        relatedEntity = new RelatedEntity()
                        {
                            Id = kalturaRelatedEntity.Id,
                            Type = ConvertRelatedEntityType(kalturaRelatedEntity.Type)
                        };

                        if (relatedEntitiesToAdd.Items.Contains(relatedEntity))
                        {
                            throw new ClientException((int)StatusCode.Error, string.Format("The request contains relatedEntity with the id {0} and type {1} more than once", relatedEntity.Id, relatedEntity.Type));
                        }

                        relatedEntitiesToAdd.Items.Add(relatedEntity);
                    }
                }

                relatedEntityNames.Add(relatedEntities.Key);
                relatedEntitiesList.Add(relatedEntitiesToAdd);
            }

            return relatedEntitiesList;
        }

        private static RelatedEntityType ConvertRelatedEntityType(KalturaRelatedEntityType type)
        {
            RelatedEntityType result;
            switch (type)
            {
                case KalturaRelatedEntityType.CHANNEL:
                    return RelatedEntityType.Channel;
                case KalturaRelatedEntityType.EXTERNAL_CHANNEL:
                    return RelatedEntityType.ExternalChannel;
                case KalturaRelatedEntityType.MEDIA:
                    return RelatedEntityType.Media;
                case KalturaRelatedEntityType.PROGRAM:
                    return RelatedEntityType.Program;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown KalturaRelatedEntryType");
            }
        }

        private static KalturaRelatedEntityType ConvertRelatedEntityType(RelatedEntityType type)
        {
            KalturaRelatedEntityType result;
            switch (type)
            {
                case RelatedEntityType.Channel:
                    return KalturaRelatedEntityType.CHANNEL;
                case RelatedEntityType.ExternalChannel:
                    return KalturaRelatedEntityType.EXTERNAL_CHANNEL;
                case RelatedEntityType.Media:
                    return KalturaRelatedEntityType.MEDIA;
                case RelatedEntityType.Program:
                    return KalturaRelatedEntityType.PROGRAM;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown RelatedEntryType");
            }
        }

        private static Dictionary<string, KalturaRelatedEntityArray> BuildRelatedEntitiesDictionary(List<RelatedEntities> relatedEntitiesList)
        {
            if (relatedEntitiesList == null)
            {
                return null;
            }

            SerializableDictionary<string, KalturaRelatedEntityArray> result = new SerializableDictionary<string, KalturaRelatedEntityArray>();

            foreach (var item in relatedEntitiesList)
            {
                result.Add(item.TagMeta.m_sName, new KalturaRelatedEntityArray()
                {
                    Objects = item.Items.Select(v => new KalturaRelatedEntity() { Id = v.Id, Type = ConvertRelatedEntityType(v.Type) }).ToList()
                });
            }

            return result;
        }
    }
}