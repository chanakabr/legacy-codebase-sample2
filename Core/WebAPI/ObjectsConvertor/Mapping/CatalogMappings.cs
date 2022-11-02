using ApiObjects;
using ApiObjects.BulkUpload;
using ApiObjects.Catalog;
using ApiObjects.Epg;
using ApiObjects.SearchObjects;
using ApiObjects.SearchPriorityGroups;
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
using WebAPI.Models.Catalog.Ordering;
using WebAPI.Models.Catalog.SearchPriorityGroup;
using WebAPI.Models.General;
using WebAPI.Models.Upload;
using WebAPI.ModelsFactory;
using WebAPI.ObjectsConvertor.Extensions;
using WebAPI.ObjectsConvertor.Mapping.Utils;
using OrderDir = ApiObjects.SearchObjects.OrderDir;

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
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id))
                 .ForMember(dest => dest.ImageTypeId, opt => opt.MapFrom(src => src.imageTypeId))
                 ;

            // EPGPicture to KalturaMediaImage
            cfg.CreateMap<EpgPicture, KalturaMediaImage>()
                 .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
                 .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.PicHeight))
                 .ForMember(dest => dest.Width, opt => opt.MapFrom(src => src.PicWidth))
                 .ForMember(dest => dest.Ratio, opt => opt.MapFrom(src => src.Ratio))
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                 .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.Version))
                 .ForMember(dest => dest.ImageTypeId, opt => opt.MapFrom(src => src.ImageTypeId));

            cfg.CreateMap<Image, KalturaMediaImage>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ContentId))
                .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.IsDefault))
                .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
                .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.Version))
                .ForMember(dest => dest.Ratio, opt => opt.MapFrom(src => src.RatioName))
                .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.Height.ToNullable()))
                .ForMember(dest => dest.Width, opt => opt.MapFrom(src => src.Width.ToNullable()))
                .ForMember(dest => dest.ImageTypeId, opt => opt.MapFrom(src => src.ImageTypeId));

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
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => MultilingualStringMapper.GetCurrent(src.Name, src.m_sName)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => MultilingualStringMapper.GetCurrent(src.Description, src.m_sDescription)))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dStartDate)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.m_dEndDate)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.m_oMediaType.m_nTypeID))
                .ForMember(dest => dest.Metas, opt => opt.MapFrom(src => BuildMetasDictionary(src.m_lMetas)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => BuildTagsDictionary(src.m_lTags)))
                .ForMember(dest => dest.ExtraParams, opt => opt.MapFrom(src => BuildExtraParamsDictionary(src)));

            //EPG to AssetInfo
            cfg.CreateMap<EPGChannelProgrammeObject, KalturaAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.EPG_ID))
                // MultilingualStringMapper.GetCurrent(src.ProgrammeName, src.NAME)
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => GetProgramName(src)))
                // MultilingualStringMapper.GetCurrent(src.ProgrammeDescription, src.DESCRIPTION))
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
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => MultilingualStringMapper.GetCurrent(src.m_oProgram.ProgrammeName, src.m_oProgram.NAME)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => MultilingualStringMapper.GetCurrent(src.m_oProgram.ProgrammeDescription, src.m_oProgram.DESCRIPTION)))
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
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => MultilingualStringMapper.GetCurrent(src.Program.m_oProgram.ProgrammeName, src.Program.m_oProgram.NAME)))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => MultilingualStringMapper.GetCurrent(src.Program.m_oProgram.ProgrammeDescription, src.Program.m_oProgram.DESCRIPTION)))
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
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.Name).ToString()))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.Description).ToString()))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.m_oMediaType.m_nTypeID));

            //Media to SlimAssetInfo
            cfg.CreateMap<ProgramObj, KalturaBaseAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.m_oProgram.ProgrammeName).ToString()))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.m_oProgram.ProgrammeDescription).ToString()))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (int)AssetType.epg));

            #endregion

            #region -> KalturaChannel

            //channelObj to Channel
            cfg.CreateMap<channelObj, WebAPI.Models.Catalog.KalturaChannel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nChannelID))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.m_sTitle)))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.m_lPic))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.m_sDescription)));

            //Channel (Catalog) to Channel
            cfg.CreateMap<GroupsCacheManager.Channel, WebAPI.Models.Catalog.KalturaChannel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nChannelID))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.NamesInOtherLanguages, src.m_sName)))
                .ForMember(dest => dest.AssetTypes, opt => opt.MapFrom(src => src.m_nMediaType))
                .ForMember(dest => dest.MediaTypes, opt => opt.MapFrom(src => src.m_nMediaType))
                .ForMember(dest => dest.FilterExpression, opt => opt.MapFrom(src => src.filterQuery))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.DescriptionInOtherLanguages, src.m_sDescription)))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => ConvertToNullableBool(src.m_nIsActive)))
                .ForMember(dest => dest.Order, opt => opt.ResolveUsing(src => ConvertOrderObjToAssetOrder(src.m_OrderObject.m_eOrderBy, src.m_OrderObject.m_eOrderDir)))
                .ForMember(dest => dest.GroupBy, opt => opt.ResolveUsing(src => ConvertToGroupBy(src.searchGroupBy)))
                .ForMember(dest => dest.SupportSegmentBasedOrdering, opt => opt.MapFrom(src => src.SupportSegmentBasedOrdering))
                .ForMember(dest => dest.AssetUserRuleId, opt => opt.MapFrom(src => src.AssetUserRuleId))
                .ForMember(dest => dest.VirtualAssetId, opt => opt.MapFrom(src => src.VirtualAssetId));

            //KSQLChannel to KalturaChannel
            cfg.CreateMap<KSQLChannel, KalturaChannel>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
               .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.Name)))
               .ForMember(dest => dest.OldName, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.AssetTypes, opt => opt.MapFrom(src => src.AssetTypes))
               .ForMember(dest => dest.MediaTypes, opt => opt.MapFrom(src => src.AssetTypes))
               .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.Description)))
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
               .ForMember(dest => dest.AssetTypes, opt => opt.MapFrom(src => src.GetAssetTypes()))
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
            cfg.CreateMap<GroupsCacheManager.Channel, KalturaDynamicChannel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nChannelID))
                .ForMember(dest => dest.SystemName, opt => opt.MapFrom(src => src.SystemName))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.NamesInOtherLanguages, src.m_sName)))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.DescriptionInOtherLanguages, src.m_sDescription)))
                .ForMember(dest => dest.AssetTypes, opt => opt.MapFrom(src => src.m_nMediaType))
                .ForMember(dest => dest.Ksql, opt => opt.MapFrom(src => src.filterQuery))
                .ForMember(dest => dest.IsActive, opt => opt.ResolveUsing(src => ConvertToNullableBool(src.m_nIsActive)))
                .ForMember(dest => dest.OrderingParameters, opt => opt.MapFrom(src => src.OrderingParameters))
                .ForMember(dest => dest.OrderBy, opt => opt.ResolveUsing(src => GetKalturaChannelOrder(src.OrderingParameters)))
                .ForMember(dest => dest.GroupBy, opt => opt.ResolveUsing(src => ConvertToGroupBy(src.searchGroupBy)))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.CreateDate)))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.UpdateDate)))
                .ForMember(dest => dest.SupportSegmentBasedOrdering, opt => opt.MapFrom(src => src.SupportSegmentBasedOrdering))
                .ForMember(dest => dest.AssetUserRuleId, opt => opt.MapFrom(src => src.AssetUserRuleId))
                .ForMember(dest => dest.MetaData, opt => opt.MapFrom(src => ConditionalAccessMappings.ConvertMetaData(src.MetaData)))
                .AfterMap((src, dest) => dest.MetaData = dest.MetaData != null && dest.MetaData.Any() ? dest.MetaData : null);


            //KalturaDynamicChannel to Channel (Catalog)
            cfg.CreateMap<KalturaDynamicChannel, GroupsCacheManager.Channel>()
               .ForMember(dest => dest.m_nChannelID, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.SystemName, opt => opt.MapFrom(src => src.SystemName))
               .ForMember(dest => dest.m_sName, opt => opt.MapFrom(src => src.Name.GetDefaultLanugageValue()))
               .ForMember(dest => dest.NamesInOtherLanguages, opt => opt.MapFrom(src => src.Name.GetNoneDefaultLanugageContainer()))
               .ForMember(dest => dest.m_nMediaType, opt => opt.MapFrom(src => src.getAssetTypes()))
               .ForMember(dest => dest.m_sDescription, opt => opt.MapFrom(src => src.Description.GetDefaultLanugageValue()))
               .ForMember(dest => dest.DescriptionInOtherLanguages, opt => opt.MapFrom(src => src.Description.GetNoneDefaultLanugageContainer()))
               .ForMember(dest => dest.filterQuery, opt => opt.MapFrom(src => src.Ksql))
               .ForMember(dest => dest.m_nIsActive, opt => opt.MapFrom(src => ConvertToNullableInt(src.IsActive)))
               .ForMember(dest => dest.OrderingParameters, opt => opt.MapFrom(src => src.OrderingParameters))
               .ForMember(dest => dest.m_OrderObject, opt => opt.ResolveUsing(src => GetOrderObj(src.OrderingParameters)))
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
            cfg.CreateMap<GroupsCacheManager.Channel, KalturaManualChannel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nChannelID))
                .ForMember(dest => dest.SystemName, opt => opt.MapFrom(src => src.SystemName))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.NamesInOtherLanguages, src.m_sName)))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.DescriptionInOtherLanguages, src.m_sDescription)))
                .ForMember(dest => dest.IsActive, opt => opt.ResolveUsing(src => ConvertToNullableBool(src.m_nIsActive)))
                .ForMember(dest => dest.OrderingParameters, opt => opt.MapFrom(src => src.OrderingParameters))
                .ForMember(dest => dest.OrderBy, opt => opt.ResolveUsing(src => GetKalturaChannelOrder(src.OrderingParameters)))
                .ForMember(dest => dest.MediaIds, opt => opt.MapFrom(src => src.m_lManualMedias != null ? string.Join(",", src.m_lManualMedias.OrderBy(x => x.m_nOrderNum).Select(x => x.m_sMediaId)) : string.Empty))
                .ForMember(dest => dest.Assets, opt => opt.ResolveUsing(src => ConvertToManualAssets(src.ManualAssets)))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.CreateDate)))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.UpdateDate)))
                .ForMember(dest => dest.SupportSegmentBasedOrdering, opt => opt.MapFrom(src => src.SupportSegmentBasedOrdering))
                .ForMember(dest => dest.AssetUserRuleId, opt => opt.MapFrom(src => src.AssetUserRuleId))
                .ForMember(dest => dest.MetaData, opt => opt.MapFrom(src => ConditionalAccessMappings.ConvertMetaData(src.MetaData)))
                .AfterMap((src, dest) => dest.MetaData = dest.MetaData != null && dest.MetaData.Any() ? dest.MetaData : null);

            //KalturaManualChannel to Channel (Catalog)
            cfg.CreateMap<KalturaManualChannel, GroupsCacheManager.Channel>()
               .ForMember(dest => dest.m_nChannelID, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.SystemName, opt => opt.MapFrom(src => src.SystemName))
               .ForMember(dest => dest.m_sName, opt => opt.MapFrom(src => src.Name.GetDefaultLanugageValue()))
               .ForMember(dest => dest.NamesInOtherLanguages, opt => opt.MapFrom(src => src.Name.GetNoneDefaultLanugageContainer()))
               .ForMember(dest => dest.m_nMediaType, opt => opt.MapFrom(src => src.GetAssetTypes()))
               .ForMember(dest => dest.m_sDescription, opt => opt.MapFrom(src => src.Description.GetDefaultLanugageValue()))
               .ForMember(dest => dest.DescriptionInOtherLanguages, opt => opt.MapFrom(src => src.Description.GetNoneDefaultLanugageContainer()))
               .ForMember(dest => dest.m_lManualMedias, opt => opt.ResolveUsing(src => ConvertToManualMedias(src.MediaIds)))
               .ForMember(dest => dest.ManualAssets, opt => opt.ResolveUsing(src => ConvertToManualAssets(src.Assets)))
               .ForMember(dest => dest.m_nIsActive, opt => opt.MapFrom(src => ConvertToNullableInt(src.IsActive)))
               .ForMember(dest => dest.OrderingParameters, opt => opt.MapFrom(src => src.OrderingParameters))
               .ForMember(dest => dest.m_OrderObject, opt => opt.ResolveUsing(src => GetOrderObj(src.OrderingParameters)))
               .ForMember(dest => dest.searchGroupBy, opt => opt.ResolveUsing(src => ConvertToGroupBy(src.GroupBy)))
               .ForMember(dest => dest.m_nChannelTypeID, opt => opt.MapFrom(src => (int)ChannelType.Manual))
               .ForMember(dest => dest.m_eOrderBy, opt => opt.Ignore())
               .ForMember(dest => dest.m_eOrderDir, opt => opt.Ignore())
               .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
               .ForMember(dest => dest.UpdateDate, opt => opt.Ignore())
               .ForMember(dest => dest.SupportSegmentBasedOrdering, opt => opt.MapFrom(src => src.SupportSegmentBasedOrdering))
               .ForMember(dest => dest.AssetUserRuleId, opt => opt.MapFrom(src => src.AssetUserRuleId))
               .ForMember(dest => dest.MetaData, opt => opt.MapFrom(src => ConditionalAccessMappings.ConvertMetaData(src.MetaData)))
               .AfterMap((src, dest) => dest.MetaData = src.MetaData != null ? dest.MetaData : null)
               .AfterMap((src, dest) => dest.ManualAssets = src.Assets != null ? dest.ManualAssets : null);

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
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.AssetType))
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
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.AssetType));

            // Country
            cfg.CreateMap<Core.Users.Country, WebAPI.Models.Users.KalturaCountry>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nObjecrtID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sCountryName))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.m_sCountryCode));

            //EPGChannelProgrammeObject to KalturaProgramAsset
            cfg.CreateMap<EPGChannelProgrammeObject, KalturaProgramAsset>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.EPG_ID)) //???
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.ProgrammeName)))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.ProgrammeDescription)))
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
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.Name.ToList(), src.m_sName)))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.Description.ToList(), src.m_sDescription)))
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
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.Name.ToList(), src.m_sName)))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.Description.ToList(), src.m_sDescription)))
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
                .ForMember(dest => dest.PaddingBeforeProgramStarts, opt => opt.MapFrom(src => src.PaddingBeforeProgramStarts))
                .ForMember(dest => dest.PaddingBeforeProgramStarts, opt => opt.MapFrom(src => src.PaddingAfterProgramEnds))
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
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.m_oProgram.ProgrammeName)))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.m_oProgram.ProgrammeDescription)))
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
                .ForMember(dest => dest.TrickPlayEnabled, opt => opt.MapFrom(src => src.m_oProgram.ENABLE_TRICK_PLAY == 1))

                // backward compatibility
                .ForMember(dest => dest.EnableCatchUp, opt => opt.MapFrom(src => src.m_oProgram.ENABLE_CATCH_UP == 1))
                .ForMember(dest => dest.EnableCdvr, opt => opt.MapFrom(src => src.m_oProgram.ENABLE_CDVR == 1))
                .ForMember(dest => dest.EnableStartOver, opt => opt.MapFrom(src => src.m_oProgram.ENABLE_START_OVER == 1))
                .ForMember(dest => dest.EnableTrickPlay, opt => opt.MapFrom(src => src.m_oProgram.ENABLE_TRICK_PLAY == 1))
                ;
            //RecordingObj to KalturaRecordingAsset
            cfg.CreateMap<RecordingObj, KalturaRecordingAsset>()
                .IncludeBase<BaseObject, KalturaAsset>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Program.AssetId))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.Program.m_oProgram.ProgrammeName)))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.Program.m_oProgram.ProgrammeDescription)))
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
                .ForMember(dest => dest.RecordingType, opt => opt.MapFrom(src => src.RecordingType))
                ;

            #endregion

            #region New Asset (OPC)

            //KalturaAsset to Asset
            cfg.CreateMap<KalturaAsset, Asset>()
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
                .ForMember(dest => dest.PaddingBeforeProgramStarts, opt => opt.MapFrom(src => src.PaddingBeforeProgramStarts))
                .ForMember(dest => dest.PaddingAfterProgramEnds, opt => opt.MapFrom(src => src.PaddingAfterProgramEnds))
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
                .ForMember(dest => dest.TrickPlayEnabled, opt => opt.MapFrom(src => src.TrickPlayEnabled))
                .ForMember(dest => dest.ExternalOfferIds, opt =>
                {
                    opt.Condition(src => src.ExternalOfferIds != null);
                    opt.ResolveUsing(src => src.ExternalOfferIds
                        .GetItemsIn<string>(out var failed, true)
                        .ThrowIfFailed(failed, () => new ClientException((int) StatusCode.InvalidArgumentValue, "Invalid value in ExternalOfferIds")));
                }); // Workaround for automatic null to empty list conversion.

            cfg.CreateMap<RecordingAsset, KalturaRecordingAsset>()
                .IncludeBase<EpgAsset, KalturaProgramAsset>()
                .ForMember(dest => dest.RecordingType, opt => opt.MapFrom(src => src.RecordingType))
                .ForMember(dest => dest.RecordingId, opt => opt.MapFrom(src => src.RecordingId))
                .ForMember(dest => dest.ViewableUntilDate, opt => opt.MapFrom(src => src.ViewableUntilDate))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.EpgIdentifier))
                ;

            // Asset to KalturaAsset
            cfg.CreateMap<Asset, KalturaAsset>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.NamesWithLanguages, src.Name)))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.DescriptionsWithLanguages, src.Description)))
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
                .ForMember(dest => dest.PaddingBeforeProgramStarts, opt => opt.MapFrom(src => src.PaddingBeforeProgramStarts))
                .ForMember(dest => dest.PaddingAfterProgramEnds, opt => opt.MapFrom(src => src.PaddingAfterProgramEnds))
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

            cfg.CreateMap<LiveToVodAsset, KalturaMediaAsset>()
                .IncludeBase<MediaAsset, KalturaMediaAsset>()
                .ForMember(dest => dest.LiveToVod, opt => opt.MapFrom(s => s));

            cfg.CreateMap<LiveToVodAsset, KalturaLiveToVodInfoAsset>()
                .ForMember(dest => dest.LinearAssetId, opt => opt.MapFrom(src => src.LinearAssetId))
                .ForMember(dest => dest.EpgId, opt => opt.MapFrom(src => src.EpgIdentifier))
                .ForMember(dest => dest.EpgChannelId, opt => opt.MapFrom(src => src.EpgChannelId))
                .ForMember(dest => dest.Crid, opt => opt.MapFrom(src => src.Crid))
                .ForMember(dest => dest.OriginalStartDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.OriginalStartDate)))
                .ForMember(dest => dest.OriginalEndDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.OriginalEndDate)))
                .ForMember(dest => dest.PaddingBeforeProgramStarts, opt => opt.MapFrom(src => src.PaddingBeforeProgramStarts))
                .ForMember(dest => dest.PaddingAfterProgramEnds, opt => opt.MapFrom(src => src.PaddingAfterProgramEnds));

            //LineupChannelAsset to KalturaLineupChannelAsset
            cfg.CreateMap<LineupChannelAsset, KalturaLineupChannelAsset>()
                .IncludeBase<LiveAsset, KalturaLiveAsset>()
                .ForMember(dest => dest.LinearChannelNumber, opt => opt.MapFrom(src => src.LinearChannelNumber));

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
                .ForMember(dest => dest.EnableTrickPlay, opt => opt.MapFrom(src => src.TrickPlayEnabled))
                .ForMember(dest => dest.ExternalOfferIds, opt => opt.ResolveUsing(src => src.ExternalOfferIds.ConvertToCommaSeparatedString(string.Empty)));
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
                .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => src.AssetType))
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
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.NamesInOtherLanguages, src.Name)))
                .ForMember(dest => dest.SystemName, opt => opt.MapFrom(src => src.SystemName))
                .ForMember(dest => dest.IsProtected, opt => opt.MapFrom(src => src.IsPredefined))
                .ForMember(dest => dest.MetaIds, opt => opt.MapFrom(src => src.MetaIds != null ? string.Join(",", src.MetaIds) : string.Empty))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate))
                .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.GetCommaSeparatedFeatures()))
                .ForMember(dest => dest.PluralName, opt => opt.MapFrom(src => src.PluralName))
                .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentId))
                .ForMember(dest => dest.ConnectingMetaId, opt => opt.MapFrom(src => src.ConnectingMetaId))
                .ForMember(dest => dest.ConnectedParentMetaId, opt => opt.MapFrom(src => src.ConnectedParentMetaId))
                .ForMember(dest => dest.DynamicData, opt => opt.ResolveUsing(src => WebAPI.Utils.Utils.ConvertToSerializableDictionary(src.DynamicData)))
                .AfterMap((src, dest) => dest.DynamicData = src.DynamicData != null ? dest.DynamicData : null);

            // KalturaAssetStruct to AssetStruct
            cfg.CreateMap<KalturaAssetStruct, AssetStruct>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.GetDefaultLanugageValue()))
                .ForMember(dest => dest.NamesInOtherLanguages, opt => opt.MapFrom(src => src.Name.GetNoneDefaultLanugageContainer()))
                .ForMember(dest => dest.SystemName, opt => opt.MapFrom(src => src.SystemName))
                .ForMember(dest => dest.IsPredefined, opt => opt.MapFrom(src => src.IsProtected))
                .ForMember(dest => dest.MetaIds, opt => opt.ResolveUsing(src => WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(src.MetaIds, "KalturaAssetStruct.metaIds")))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate))
                .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.GetFeaturesAsHashSet()))
                .ForMember(dest => dest.PluralName, opt => opt.MapFrom(src => src.PluralName))
                .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentId))
                .ForMember(dest => dest.ConnectingMetaId, opt => opt.MapFrom(src => src.ConnectingMetaId))
                .ForMember(dest => dest.ConnectedParentMetaId, opt => opt.MapFrom(src => src.ConnectedParentMetaId))
                .ForMember(dest => dest.DynamicData, opt => opt.ResolveUsing(src => src.DynamicData?.ToDictionary(x => x.Key, x => x.Value?.value)))
                .AfterMap((src, dest) => dest.DynamicData = src.DynamicData != null ? dest.DynamicData : null);

            // MediaFileType to KalturaMediaFileType
            cfg.CreateMap<MediaFileType, KalturaMediaFileType>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.Status, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate))
                .ForMember(dest => dest.IsTrailer, opt => opt.MapFrom(src => src.IsTrailer))
                .ForMember(dest => dest.StreamerType, opt => opt.MapFrom(src => src.StreamerType))
                .ForMember(dest => dest.DrmProfileId, opt => opt.MapFrom(src => src.DrmId))
                .ForMember(dest => dest.Quality, opt => opt.MapFrom(src => src.Quality))
                .ForMember(dest => dest.VideoCodecs, opt => opt.MapFrom(src => src.CreateMappedHashSetForKalturaMediaFileType(src.VideoCodecs)))
                .ForMember(dest => dest.AudioCodecs, opt => opt.MapFrom(src => src.CreateMappedHashSetForKalturaMediaFileType(src.AudioCodecs)));

            cfg.CreateMap<MediaFileTypeQuality, KalturaMediaFileTypeQuality?>()
                .ConvertUsing(type =>
                {
                    switch (type)
                    {
                        case MediaFileTypeQuality.None: return null;
                        case MediaFileTypeQuality.Adaptive: return KalturaMediaFileTypeQuality.ADAPTIVE;
                        case MediaFileTypeQuality.SD: return KalturaMediaFileTypeQuality.SD;
                        case MediaFileTypeQuality.HD_720: return KalturaMediaFileTypeQuality.HD_720;
                        case MediaFileTypeQuality.HD_1080: return KalturaMediaFileTypeQuality.HD_1080;
                        case MediaFileTypeQuality.UHD_4K: return KalturaMediaFileTypeQuality.UHD_4K;
                        default: throw new ClientException((int)StatusCode.Error, "Unknown asset file type quality");
                    }
                });

            cfg.CreateMap<StreamerType?, KalturaMediaFileStreamerType?>()
                .ConvertUsing(type =>
                {
                    if (!type.HasValue) { return null; }
                    switch (type)
                    {
                        case StreamerType.none: return KalturaMediaFileStreamerType.NONE;
                        case StreamerType.applehttp: return KalturaMediaFileStreamerType.APPLE_HTTP;
                        case StreamerType.mpegdash: return KalturaMediaFileStreamerType.MPEG_DASH;
                        case StreamerType.smothstreaming: return KalturaMediaFileStreamerType.SMOOTH_STREAMING;
                        case StreamerType.url: return KalturaMediaFileStreamerType.URL;
                        case StreamerType.multicast: return KalturaMediaFileStreamerType.MULTICAST;
                        default: throw new ClientException((int)StatusCode.Error, "Unknown streamer type");
                    }
                });

            // KalturaMediaFileType to MediaFileType
            cfg.CreateMap<KalturaMediaFileType, MediaFileType>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.Description))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.Status.HasValue ? src.Status.Value : true))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
                .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate))
                .ForMember(dest => dest.IsTrailer, opt => opt.MapFrom(src => src.IsTrailer))
                .ForMember(dest => dest.StreamerType, opt => opt.MapFrom(src => src.StreamerType))
                .ForMember(dest => dest.DrmId, opt => opt.MapFrom(src => src.DrmProfileId))
                .ForMember(dest => dest.Quality, opt => opt.MapFrom(src => src.Quality))
                .ForMember(dest => dest.VideoCodecs, opt => opt.MapFrom(src => src.CreateMappedHashSetForMediaFileType(src.VideoCodecs)))
                .ForMember(dest => dest.AudioCodecs, opt => opt.MapFrom(src => src.CreateMappedHashSetForMediaFileType(src.AudioCodecs)));

            cfg.CreateMap<KalturaMediaFileTypeQuality?, MediaFileTypeQuality>()
                .ConvertUsing(type =>
                {
                    if (!type.HasValue) { return MediaFileTypeQuality.None; }
                    return ToMediaFileTypeQuality(type.Value);
                });
            cfg.CreateMap<KalturaMediaFileTypeQuality, MediaFileTypeQuality>().ConvertUsing(ToMediaFileTypeQuality);

            cfg.CreateMap<KalturaMediaFileStreamerType?, StreamerType?>()
                .ConvertUsing(type =>
                {
                    if (!type.HasValue) { return null; }
                    switch (type)
                    {
                        case KalturaMediaFileStreamerType.NONE: return StreamerType.none;
                        case KalturaMediaFileStreamerType.APPLE_HTTP: return StreamerType.applehttp;
                        case KalturaMediaFileStreamerType.MPEG_DASH: return StreamerType.mpegdash;
                        case KalturaMediaFileStreamerType.SMOOTH_STREAMING: return StreamerType.smothstreaming;
                        case KalturaMediaFileStreamerType.URL: return StreamerType.url;
                        case KalturaMediaFileStreamerType.MULTICAST: return StreamerType.multicast;
                        default: throw new ClientException((int)StatusCode.Error, "Unknown streamer type");
                    }
                });

            // Topic to KalturaMeta
            cfg.CreateMap<Topic, Models.API.KalturaMeta>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.NamesInOtherLanguages, src.Name)))
              .ForMember(dest => dest.SystemName, opt => opt.MapFrom(src => src.SystemName))
              .ForMember(dest => dest.DataType, opt => opt.ResolveUsing(src => ConvertToKalturaMetaDataType(src.Type)))
              .ForMember(dest => dest.MultipleValue, opt => opt.MapFrom(src => src.Type == ApiObjects.MetaType.Tag))
              .ForMember(dest => dest.IsProtected, opt => opt.MapFrom(src => src.IsPredefined))
              .ForMember(dest => dest.HelpText, opt => opt.MapFrom(src => src.HelpText))
              .ForMember(dest => dest.Features, opt => opt.MapFrom(src => src.GetCommaSeparatedFeatures(null)))
              .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentId.HasValue ? src.ParentId.Value.ToString() : null))
              .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
              .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate))
              .ForMember(dest => dest.Type, opt => opt.Ignore())
              .ForMember(dest => dest.PartnerId, opt => opt.Ignore())
              .ForMember(dest => dest.FieldName, opt => opt.Ignore())
              .ForMember(dest => dest.AssetType, opt => opt.Ignore())
              .ForMember(dest => dest.DynamicData, opt => opt.MapFrom(src => src.DynamicData != null ?
                src.DynamicData.ToDictionary(k => k.Key, v => v.Value) : null))
              ;

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
              .AfterMap((src, dest) => dest.Features = src.Features != null ? dest.Features : null)
              .ForMember(dest => dest.ParentId, opt => opt.ResolveUsing(src => StringUtils.TryConvertTo<long>(src.ParentId)))
              .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
              .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate))
              .ForMember(dest => dest.DynamicData, opt => opt.MapFrom(src => WebAPI.Utils.Utils.ConvertSerializeableDictionary(src.DynamicData, true, true)))
              .AfterMap((src, dest) => dest.DynamicData = src.DynamicData != null ? dest.DynamicData : null)
              ;

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
                .ForMember(dest => dest.IsLocationTag, opt => opt.MapFrom(src => src.IsLocationTag))
                .ForMember(dest => dest.SuppressedOrder, opt => opt.MapFrom(src => src.SuppressedOrder))
                .ForMember(dest => dest.AliasName, opt => opt.MapFrom(src => src.Alias))
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
               .ForMember(dest => dest.IsLocationTag, opt => opt.MapFrom(src => src.IsLocationTag))
               .ForMember(dest => dest.SuppressedOrder, opt => opt.MapFrom(src => src.SuppressedOrder))
               .ForMember(dest => dest.Alias, opt => opt.MapFrom(src => src.AliasName))
               ;

            #endregion

            #region Tag

            cfg.CreateMap<TagValue, KalturaTag>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.tagId))
              .ForMember(dest => dest.TagTypeId, opt => opt.MapFrom(src => src.topicId))
              .ForMember(dest => dest.Tag, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.TagsInOtherLanguages, src.value)))
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
                 .ForMember(dest => dest.Opl, opt => opt.MapFrom(src => src.Opl))
                 .ForMember(dest => dest.Labels, opt => opt.MapFrom(src => src.Labels))
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
                .ForMember(dest => dest.PpvModule, opt => opt.MapFrom(src => GetPPVModule(src.PPVModules)))
                .ForMember(dest => dest.Opl, opt => opt.MapFrom(src => src.Opl))
                .ForMember(dest => dest.Labels, opt => opt.MapFrom(src => src.Labels))
                ;
            cfg.CreateMap<KalturaMediaFile, KalturaDiscoveryMediaFile>();

            #endregion

            #region BulkUpload

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

            cfg.CreateMap<BulkUploadStatistics, KalturaBulkUploadStatistics>()
                .ForMember(dest => dest.Queued, opt => opt.MapFrom(src => src.Queued))
                .ForMember(dest => dest.Failed, opt => opt.MapFrom(src => src.Failed))
                .ForMember(dest => dest.Success, opt => opt.MapFrom(src => src.Success))
                .ForMember(dest => dest.Pending, opt => opt.MapFrom(src => src.Pending))
                .ForMember(dest => dest.Parsing, opt => opt.MapFrom(src => src.Parsing))
                .ForMember(dest => dest.Processed, opt => opt.MapFrom(src => src.Processed))
                .ForMember(dest => dest.Processing, opt => opt.MapFrom(src => src.Processing))
                .ForMember(dest => dest.Fatal, opt => opt.MapFrom(src => src.Fatal))
                .ForMember(dest => dest.Partial, opt => opt.MapFrom(src => src.Partial))
                .ForMember(dest => dest.Uploaded, opt => opt.MapFrom(src => src.Uploaded))
                .ReverseMap();


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

            cfg.CreateMap<BulkUploadDynamicListResult, KalturaBulkUploadDynamicListResult>()
               .IncludeBase<BulkUploadResult, KalturaBulkUploadResult>();

            cfg.CreateMap<BulkUploadUdidDynamicListResult, KalturaBulkUploadUdidDynamicListResult>()
              .IncludeBase<BulkUploadDynamicListResult, KalturaBulkUploadDynamicListResult>()
              .ForMember(dest => dest.Udid, opt => opt.MapFrom(src => src.Udid));

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

            cfg.CreateMap<KalturaBulkUploadDynamicListData, BulkUploadDynamicListData>()
               .IncludeBase<KalturaBulkUploadObjectData, BulkUploadObjectData>()
               .ForMember(dest => dest.DynamicListId, opt => opt.MapFrom(src => src.DynamicListId));

            cfg.CreateMap<KalturaBulkUploadUdidDynamicListData, BulkUploadUdidDynamicListData>()
               .IncludeBase<KalturaBulkUploadDynamicListData, BulkUploadDynamicListData>();

            #endregion

            #region CategoryItem

            cfg.CreateMap<KalturaCategoryItem, ApiLogic.Catalog.CategoryItem>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.GetDefaultLanugageValue()))
              .ForMember(dest => dest.NamesInOtherLanguages, opt => opt.MapFrom(src => src.Name.GetNoneDefaultLanugageContainer()))
              .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentId))
              .ForMember(dest => dest.ChildrenIds, opt => opt.ResolveUsing(src => WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(src.ChildrenIds, "kalturaCategoryItem.childrenIds")))
              .AfterMap((src, dest) => dest.ChildrenIds = src.ChildrenIds != null ? dest.ChildrenIds : null)
              .ForMember(dest => dest.UnifiedChannels, opt => opt.MapFrom(src => src.UnifiedChannels))
              .AfterMap((src, dest) => dest.UnifiedChannels = src.UnifiedChannels != null ? dest.UnifiedChannels : null)
              .ForMember(dest => dest.DynamicData, opt => opt.MapFrom(src => WebAPI.Utils.Utils.ConvertSerializeableDictionary(src.DynamicData, true, false)))
              .AfterMap((src, dest) => dest.DynamicData = src.DynamicData != null ? dest.DynamicData : null)
              .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
              .ForMember(dest => dest.TimeSlot, opt => opt.ResolveUsing(src => ConvertToTimeSlot(src.StartDateInSeconds, src.EndDateInSeconds, src.NullableProperties)))
              .ForMember(dest => dest.VersionId, opt => opt.MapFrom(src => src.VersionId))
              .ForMember(dest => dest.VirtualAssetId, opt => opt.MapFrom(src => src.VirtualAssetId))
              .ForMember(dest => dest.ReferenceId, opt => opt.MapFrom(src => src.ReferenceId));

            cfg.CreateMap<ApiLogic.Catalog.CategoryItem, KalturaCategoryItem>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.NamesInOtherLanguages, src.Name)))
               .ForMember(dest => dest.ParentId, opt => opt.MapFrom(src => src.ParentId))
               .ForMember(dest => dest.ChildrenIds, opt => opt.MapFrom(src => src.ChildrenIds != null ? string.Join(",", src.ChildrenIds) : null))
               .ForMember(dest => dest.UnifiedChannels, opt => opt.MapFrom(src => src.UnifiedChannels))
               .ForMember(dest => dest.DynamicData, opt => opt.MapFrom(src => src.DynamicData != null ? src.DynamicData.ToDictionary(k => k.Key, v => v.Value) : null))
               .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => DateUtils.DateTimeToUtcUnixTimestampSeconds(src.UpdateDate)))
               .ForMember(dest => dest.DynamicData, opt => opt.MapFrom(src => src.DynamicData != null ? src.DynamicData.ToDictionary(k => k.Key, v => v.Value) : null))
               .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
               .ForMember(dest => dest.StartDateInSeconds, opt => opt.MapFrom(src => src.TimeSlot.StartDateInSeconds))
               .ForMember(dest => dest.EndDateInSeconds, opt => opt.MapFrom(src => src.TimeSlot.EndDateInSeconds))
               .ForMember(dest => dest.VersionId, opt => opt.MapFrom(src => src.VersionId))
               .ForMember(dest => dest.VirtualAssetId, opt => opt.MapFrom(src => src.VirtualAssetId))
               .ForMember(dest => dest.ReferenceId, opt => opt.MapFrom(src => src.ReferenceId));

            cfg.CreateMap<UnifiedChannelType, KalturaChannelType>()
                .ConvertUsing(type =>
                {
                    switch (type)
                    {
                        case UnifiedChannelType.Internal:
                            return KalturaChannelType.Internal;
                        case UnifiedChannelType.External:
                            return KalturaChannelType.External;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown UnifiedChannelType value : {type.ToString()}");
                    }
                });

            cfg.CreateMap<KalturaChannelType, UnifiedChannelType>()
                .ConvertUsing(type =>
                {
                    switch (type)
                    {
                        case KalturaChannelType.Internal:
                            return UnifiedChannelType.Internal;
                        case KalturaChannelType.External:
                            return UnifiedChannelType.External;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown KalturaChannelType value : {type.ToString()}");
                    }
                });

            cfg.CreateMap<KalturaUnifiedChannel, UnifiedChannel>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type));

            cfg.CreateMap<UnifiedChannel, KalturaUnifiedChannel>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
              .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type));

            cfg.CreateMap<KalturaUnifiedChannelInfo, UnifiedChannelInfo>()
                .IncludeBase<KalturaUnifiedChannel, UnifiedChannel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.TimeSlot, opt => opt.ResolveUsing(src => ConvertToTimeSlot(src.StartDateInSeconds, src.EndDateInSeconds, src.NullableProperties)));

            cfg.CreateMap<UnifiedChannelInfo, KalturaUnifiedChannelInfo>()
                .IncludeBase<UnifiedChannel, KalturaUnifiedChannel>()
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
                .ForMember(dest => dest.StartDateInSeconds, opt => opt.MapFrom(src => src.TimeSlot.StartDateInSeconds))
                .ForMember(dest => dest.EndDateInSeconds, opt => opt.MapFrom(src => src.TimeSlot.EndDateInSeconds));

            cfg.CreateMap<ApiLogic.Catalog.CategoryTree, KalturaCategoryTree>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.NamesInOtherLanguages, src.Name)))
                .ForMember(dest => dest.Children, opt => opt.MapFrom(src => src.Children))
                .ForMember(dest => dest.UnifiedChannels, opt => opt.MapFrom(src => src.UnifiedChannels))
                .ForMember(dest => dest.DynamicData, opt => opt.MapFrom(src => src.DynamicData != null ? src.DynamicData.ToDictionary(k => k.Key, v => v.Value) : null))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.Images))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.IsActive))
                .ForMember(dest => dest.StartDateInSeconds, opt => opt.MapFrom(src => src.TimeSlot.StartDateInSeconds))
                .ForMember(dest => dest.EndDateInSeconds, opt => opt.MapFrom(src => src.TimeSlot.EndDateInSeconds))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.VersionId, opt => opt.MapFrom(src => src.VersionId))
                .ForMember(dest => dest.VirtualAssetId, opt => opt.MapFrom(src => src.VirtualAssetId))
                .ForMember(dest => dest.ReferenceId, opt => opt.MapFrom(src => src.ReferenceId));

            cfg.CreateMap<KalturaCategoryItemFilter, ApiLogic.Catalog.CategoryItemFilter>()
                .ForMember(dest => dest.OrderBy, opt => opt.MapFrom(src => CatalogConvertor.ConvertOrderToOrderObj(src.OrderBy)));

            cfg.CreateMap<KalturaCategoryItemByIdInFilter, ApiLogic.Catalog.CategoryItemByIdInFilter>()
               .IncludeBase<KalturaCategoryItemFilter, ApiLogic.Catalog.CategoryItemFilter>()
               .ForMember(dest => dest.IdIn, opt => opt.ResolveUsing(src => !string.IsNullOrEmpty(src.IdIn) ? WebAPI.Utils.Utils.ParseCommaSeparatedValues<List<long>, long>(src.IdIn, "KalturaCategoryItemByIdInFilter.IdIn", true) : null));

            cfg.CreateMap<KalturaCategoryItemSearchFilter, ApiLogic.Catalog.CategoryItemSearchFilter>()
              .IncludeBase<KalturaCategoryItemFilter, ApiLogic.Catalog.CategoryItemFilter>()
              .ForMember(dest => dest.Ksql, opt => opt.MapFrom(src => src.Ksql))
              .ForMember(dest => dest.RootOnly, opt => opt.MapFrom(src => src.RootOnly))
              .ForMember(dest => dest.IsOrderByUpdateDate, opt => opt.MapFrom(src => SetOrderByUpdate(src.OrderBy)));


            cfg.CreateMap<KalturaCategoryItemAncestorsFilter, ApiLogic.Catalog.CategoryItemAncestorsFilter>()
              .IncludeBase<KalturaCategoryItemFilter, ApiLogic.Catalog.CategoryItemFilter>()
              .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id));

            #endregion CategoryItem

            cfg.CreateMap<UserWatchHistory, KalturaAssetHistory>()
                .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => long.Parse(src.AssetId)))
                .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.Duration))
                .ForMember(dest => dest.IsFinishedWatching, opt => opt.MapFrom(src => src.IsFinishedWatching))
                .ForMember(dest => dest.LastWatched, opt => opt.MapFrom(src => src.LastWatch))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Location))
                .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => src.AssetType));

            cfg.CreateMap<eAssetTypes, KalturaAssetType>()
                .ConvertUsing(type =>
                {
                    switch (type)
                    {
                        case eAssetTypes.EPG:
                            return KalturaAssetType.epg;
                        case eAssetTypes.NPVR:
                            return KalturaAssetType.recording;
                        case eAssetTypes.MEDIA:
                            return KalturaAssetType.media;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown Asset Type: {type.ToString()}");
                    }
                });

            cfg.CreateMap<KalturaAssetType, eAssetTypes>()
                .ConvertUsing(type =>
                {
                    switch (type)
                    {
                        case KalturaAssetType.media:
                            return eAssetTypes.MEDIA;
                        case KalturaAssetType.recording:
                            return eAssetTypes.NPVR;
                        case KalturaAssetType.epg:
                            return eAssetTypes.EPG;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown Asset Type: {type.ToString()}");

                    }
                });

            cfg.CreateMap<KalturaAssetType, ePlayType>()
            .ConvertUsing(type =>
            {
                switch (type)
                {
                    case KalturaAssetType.media:
                        return ePlayType.MEDIA;
                    case KalturaAssetType.recording:
                        return ePlayType.NPVR;
                    case KalturaAssetType.epg:
                        return ePlayType.EPG;
                    default:
                        throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown Play Type: {type.ToString()}");
                }
            });

            cfg.CreateMap<KalturaAssetType?, eAssetTypes>()
                .ConvertUsing(assetType =>
                {
                    eAssetTypes response = eAssetTypes.UNKNOWN;
                    if (assetType.HasValue)
                    {
                        switch (assetType)
                        {
                            case KalturaAssetType.epg:
                                response = eAssetTypes.EPG;
                                break;
                            case KalturaAssetType.recording:
                                response = eAssetTypes.NPVR;
                                break;
                            case KalturaAssetType.media:
                                response = eAssetTypes.MEDIA;
                                break;
                            default:
                                throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown Asset Type: {assetType.ToString()}");
                        }
                    }

                    return response;
                });

            cfg.CreateMap<eAssetType, KalturaAssetType>()
                .ConvertUsing(type =>
                {
                    switch (type)
                    {
                        case eAssetType.PROGRAM:
                            return KalturaAssetType.epg;
                        case eAssetType.MEDIA:
                            return KalturaAssetType.media;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown Asset Type: {type.ToString()}");
                    }
                });

            cfg.CreateMap<KalturaAssetType, eAssetType>()
                .ConvertUsing(type =>
                {
                    switch (type)
                    {
                        case KalturaAssetType.media:
                            return eAssetType.MEDIA;
                        case KalturaAssetType.epg:
                            return eAssetType.PROGRAM;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown Asset Type: {type.ToString()}");
                    }
                });

            cfg.CreateMap<KalturaAssetType, StatsType>()
                .ConvertUsing(type =>
                {
                    switch (type)
                    {
                        case KalturaAssetType.media:
                            return StatsType.MEDIA;
                        case KalturaAssetType.epg:
                            return StatsType.EPG;
                        case KalturaAssetType.recording:
                            throw new ClientException((int)StatusCode.Error, "recording is not supported");
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown Asset Type: {type.ToString()}");
                    }
                });

            cfg.CreateMap<ApiObjects.MediaMarks.DevicePlayData, KalturaStreamingDevice>()
               .ForMember(dest => dest.UserId, opt => opt.MapFrom(src => src.UserId))
               .ForMember(dest => dest.Udid, opt => opt.MapFrom(src => src.UDID))
               .ForMember(dest => dest.Asset, opt => opt.ResolveUsing(src => ResolveKalturaSlimAsset(src)))
               ;

            #region CategoryVersion

            cfg.CreateMap<KalturaCategoryVersion, CategoryVersion>()
              .ForMember(dest => dest.BaseVersionId, opt => opt.MapFrom(src => src.BaseVersionId))
              .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
              .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Comment));

            cfg.CreateMap<CategoryVersion, KalturaCategoryVersion>()
               .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name))
               .ForMember(dest => dest.BaseVersionId, opt => opt.MapFrom(src => src.BaseVersionId))
               .ForMember(dest => dest.TreeId, opt => opt.MapFrom(src => src.TreeId))
               .ForMember(dest => dest.CategoryRootId, opt => opt.MapFrom(src => src.CategoryItemRootId))
               .ForMember(dest => dest.Comment, opt => opt.MapFrom(src => src.Comment))
               .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.State))
               .ForMember(dest => dest.UpdaterId, opt => opt.MapFrom(src => src.UpdaterId))
               .ForMember(dest => dest.DefaultDate, opt => opt.MapFrom(src => src.DefaultDate))
               .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => src.CreateDate))
               .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => src.UpdateDate));

            cfg.CreateMap<CategoryVersionState, KalturaCategoryVersionState>()
                .ConvertUsing(type =>
                {
                    switch (type)
                    {
                        case CategoryVersionState.Default:
                            return KalturaCategoryVersionState.DEFAULT;
                        case CategoryVersionState.Draft:
                            return KalturaCategoryVersionState.DRAFT;
                        case CategoryVersionState.Released:
                            return KalturaCategoryVersionState.RELEASED;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown CategoryVersionState value : {type.ToString()}");
                    }
                });


            cfg.CreateMap<KalturaCategoryVersionFilter, CategoryVersionFilter>()
                .ForMember(dest => dest.OrderBy, opt => opt.MapFrom(src => CatalogConvertor.ConvertOrderToOrderBy(src.OrderBy)));

            cfg.CreateMap<KalturaCategoryVersionFilterByTree, CategoryVersionFilterByTree>()
                .IncludeBase<KalturaCategoryVersionFilter, CategoryVersionFilter>()
                .ForMember(dest => dest.TreeId, opt => opt.MapFrom(src => src.TreeIdEqual))
                .ForMember(dest => dest.State, opt => opt.MapFrom(src => src.StateEqual));

            #endregion CategoryVersion

            #region Label

            cfg.CreateMap<LabelValue, KalturaLabel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value))
                .ForMember(dest => dest.EntityAttribute, opt => opt.MapFrom(src => src.EntityAttribute));

            cfg.CreateMap<KalturaLabel, LabelValue>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value))
                .ForMember(dest => dest.EntityAttribute, opt => opt.MapFrom(src => src.EntityAttribute));

            cfg.CreateMap<EntityAttribute, KalturaEntityAttribute>()
                .ConvertUsing(type =>
                {
                    switch (type)
                    {
                        case EntityAttribute.MediaFileLabels:
                            return KalturaEntityAttribute.MEDIA_FILE_LABELS;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown {nameof(EntityAttribute)}: {type.ToString()}");
                    }
                });

            cfg.CreateMap<KalturaEntityAttribute, EntityAttribute>()
                .ConvertUsing(type =>
                {
                    switch (type)
                    {
                        case KalturaEntityAttribute.MEDIA_FILE_LABELS:
                            return EntityAttribute.MediaFileLabels;
                    }

                    return (EntityAttribute)0;
                });

            #endregion

            #region SearchPriorityGroup

            cfg.CreateMap<KalturaSearchPriorityGroup, SearchPriorityGroup>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.Name.GetLanugageContainer().ToArray()))
                .ForMember(dest => dest.Criteria, opt => opt.MapFrom(src => src.Criteria));

            cfg.CreateMap<KalturaSearchPriorityCriteria, SearchPriorityCriteria>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));

            cfg.CreateMap<KalturaSearchPriorityGroupOrderedIdsSet, SearchPriorityGroupOrderedIdsSet>()
                .ConstructUsing((src, dest) => new SearchPriorityGroupOrderedIdsSet())
                .ForMember(dest => dest.PriorityGroupIds, opt => opt.MapFrom(src => src.GetPriorityGroupIds()));

            cfg.CreateMap<KalturaSearchPriorityCriteriaType, SearchPriorityCriteriaType>()
                .ConvertUsing(type =>
                {
                    switch (type)
                    {
                        case KalturaSearchPriorityCriteriaType.KSql:
                            return SearchPriorityCriteriaType.KSql;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown {nameof(KalturaSearchPriorityCriteriaType)}: {type.ToString()}");
                    }
                });

            cfg.CreateMap<SearchPriorityGroup, KalturaSearchPriorityGroup>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => MultilingualStringFactory.Create(src.Name)))
                .ForMember(dest => dest.Criteria, opt => opt.MapFrom(src => src.Criteria));

            cfg.CreateMap<SearchPriorityCriteria, KalturaSearchPriorityCriteria>()
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.Type))
                .ForMember(dest => dest.Value, opt => opt.MapFrom(src => src.Value));

            cfg.CreateMap<SearchPriorityGroupOrderedIdsSet, KalturaSearchPriorityGroupOrderedIdsSet>()
                .ForMember(dest => dest.PriorityGroupIds, opt => opt.MapFrom(src => string.Join(",", src.PriorityGroupIds)));

            cfg.CreateMap<SearchPriorityCriteriaType, KalturaSearchPriorityCriteriaType>()
                .ConvertUsing(type =>
                {
                    switch (type)
                    {
                        case SearchPriorityCriteriaType.KSql:
                            return KalturaSearchPriorityCriteriaType.KSql;
                        default:
                            throw new ClientException((int)StatusCode.UnknownEnumValue, $"Unknown {nameof(SearchPriorityCriteriaType)}: {type.ToString()}");
                    }
                });

            #endregion

            #region Ordering

            cfg.CreateMap<KalturaBaseChannelOrder, AssetOrder>()
                .Include<KalturaChannelFieldOrder, AssetOrder>()
                .Include<KalturaChannelDynamicOrder, AssetOrderByMeta>()
                .Include<KalturaChannelSlidingWindowOrder, AssetSlidingWindowOrder>();

            cfg.CreateMap<KalturaChannelFieldOrder, AssetOrder>()
                .ForMember(dest => dest.Field, opt => opt.ResolveUsing(src => GetOrderBy(src.OrderBy)))
                .ForMember(dest => dest.Direction, opt => opt.ResolveUsing(src => GetOrderDir(src.OrderBy)));

            cfg.CreateMap<KalturaChannelDynamicOrder, AssetOrderByMeta>()
                .ForMember(dest => dest.Field, opt => opt.ResolveUsing(src => GetOrderBy(src.OrderBy)))
                .ForMember(dest => dest.Direction, opt => opt.ResolveUsing(src => GetOrderDir(src.OrderBy)))
                .ForMember(dest => dest.MetaName, opt => opt.MapFrom(src => src.Name));

            cfg.CreateMap<KalturaChannelSlidingWindowOrder, AssetSlidingWindowOrder>()
                .ForMember(dest => dest.Field, opt => opt.ResolveUsing(src => GetOrderBy(src.OrderBy)))
                .ForMember(dest => dest.Direction, opt => opt.ResolveUsing(src => GetOrderDir(src.OrderBy)))
                .ForMember(dest => dest.SlidingWindowPeriod, opt => opt.MapFrom(src => src.SlidingWindowPeriod));

            cfg.CreateMap<AssetOrder, KalturaBaseChannelOrder>()
                .ConstructUsing((assetOrder, context) =>
                {
                    switch (assetOrder)
                    {
                        case AssetOrderByMeta _:
                            return new KalturaChannelDynamicOrder();
                        case AssetSlidingWindowOrder _:
                            return new KalturaChannelSlidingWindowOrder();
                        case AssetOrder _:
                            return new KalturaChannelFieldOrder { OrderBy = GetKalturaChannelFieldOrderBy(assetOrder.Field, assetOrder.Direction) };
                        default:
                            throw new ClientException((int)StatusCode.Error, $"{nameof(KalturaBaseChannelOrder)} can not be defined.");
                    }
                })
                .Include<AssetOrderByMeta, KalturaChannelDynamicOrder>()
                .Include<AssetSlidingWindowOrder, KalturaChannelSlidingWindowOrder>();

            cfg.CreateMap<AssetOrderByMeta, KalturaChannelDynamicOrder>()
                .ForMember(dest => dest.OrderBy, opt => opt.ResolveUsing(src => GetKalturaMetaTagOrderBy(src.Field, src.Direction)))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.MetaName));

            cfg.CreateMap<AssetSlidingWindowOrder, KalturaChannelSlidingWindowOrder>()
                .ForMember(dest => dest.OrderBy, opt => opt.ResolveUsing(src => GetKalturaChannelSlidingWindowOrderBy(src.Field, src.Direction)))
                .ForMember(dest => dest.SlidingWindowPeriod, opt => opt.MapFrom(src => src.SlidingWindowPeriod));

            #endregion
        }

        private static MediaFileTypeQuality ToMediaFileTypeQuality(KalturaMediaFileTypeQuality t)
        {
            switch (t)
            {
                case KalturaMediaFileTypeQuality.ADAPTIVE: return MediaFileTypeQuality.Adaptive;
                case KalturaMediaFileTypeQuality.SD: return MediaFileTypeQuality.SD;
                case KalturaMediaFileTypeQuality.HD_720: return MediaFileTypeQuality.HD_720;
                case KalturaMediaFileTypeQuality.HD_1080: return MediaFileTypeQuality.HD_1080;
                case KalturaMediaFileTypeQuality.UHD_4K: return MediaFileTypeQuality.UHD_4K;
                default: throw new ClientException((int)StatusCode.Error, "Unknown asset file type quality");
            }
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
            return MultilingualStringMapper.GetCurrent(src.ProgrammeDescription, src.DESCRIPTION);
        }

        private static object GetProgramName(EPGChannelProgrammeObject src)
        {
            return MultilingualStringMapper.GetCurrent(src.ProgrammeName, src.NAME);
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

        public static List<KalturaMediaImage> ConvertImageListToKalturaMediaImageList(List<Image> images, int groupId)
        {
            List<KalturaMediaImage> result = new List<KalturaMediaImage>();
            if (images != null && images.Count > 0)
            {
                Dictionary<long, string> imageTypeIdToRatioNameMap = Core.Catalog.CatalogManagement.ImageManager.GetImageTypeIdToRatioNameMap(groupId);
                Dictionary<long, string> imageTypeIdToNameMap = Core.Catalog.CatalogManagement.ImageManager.GetImageTypeIdToNameMap(groupId);
                foreach (Image image in images)
                {
                    string ratioName = !string.IsNullOrEmpty(image.RatioName) ? image.RatioName :
                        imageTypeIdToRatioNameMap != null && imageTypeIdToRatioNameMap.ContainsKey(image.ImageTypeId) ?
                            imageTypeIdToRatioNameMap[image.ImageTypeId] : string.Empty;

                    image.RatioName = ratioName;
                    KalturaMediaImage convertedImage = Mapper.Map<KalturaMediaImage>(image);

                    if (convertedImage != null)
                    {
                        convertedImage.ImageTypeName = imageTypeIdToNameMap != null && imageTypeIdToNameMap.ContainsKey(image.ImageTypeId) ?
                            imageTypeIdToNameMap[image.ImageTypeId] : string.Empty;
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

        private static OrderBy GetOrderBy(KalturaChannelFieldOrderByType orderBy)
        {
            switch (orderBy)
            {
                case KalturaChannelFieldOrderByType.NAME_ASC:
                case KalturaChannelFieldOrderByType.NAME_DESC:
                    return OrderBy.NAME;
                case KalturaChannelFieldOrderByType.START_DATE_ASC:
                case KalturaChannelFieldOrderByType.START_DATE_DESC:
                    return OrderBy.START_DATE;
                case KalturaChannelFieldOrderByType.CREATE_DATE_ASC:
                case KalturaChannelFieldOrderByType.CREATE_DATE_DESC:
                    return OrderBy.CREATE_DATE;
                case KalturaChannelFieldOrderByType.RELEVANCY_DESC:
                    return OrderBy.RELATED;
                case KalturaChannelFieldOrderByType.ORDER_NUM:
                    return OrderBy.ID;
                default:
                    throw new ClientException((int)StatusCode.Error, $"Unknown {nameof(KalturaChannelFieldOrderByType)}.");
            }
        }

        private static OrderBy GetOrderBy(KalturaMetaTagOrderBy orderBy)
        {
            switch (orderBy)
            {
                case KalturaMetaTagOrderBy.META_ASC:
                case KalturaMetaTagOrderBy.META_DESC:
                    return OrderBy.META;
                default:
                    throw new ClientException((int)StatusCode.Error, $"Unknown {nameof(KalturaChannelDynamicOrder)}.");
            }
        }

        private static OrderBy GetOrderBy(KalturaChannelSlidingWindowOrderByType orderBy)
        {
            switch (orderBy)
            {
                case KalturaChannelSlidingWindowOrderByType.LIKES_DESC:
                    return OrderBy.LIKE_COUNTER;
                case KalturaChannelSlidingWindowOrderByType.RATINGS_DESC:
                    return OrderBy.RATING;
                case KalturaChannelSlidingWindowOrderByType.VOTES_DESC:
                    return OrderBy.VOTES_COUNT;
                case KalturaChannelSlidingWindowOrderByType.VIEWS_DESC:
                    return OrderBy.VIEWS;
                default:
                    throw new ClientException((int)StatusCode.Error, $"Unknown {nameof(KalturaChannelSlidingWindowOrderByType)}.");
            }
        }

        private static OrderDir GetOrderDir(KalturaChannelFieldOrderByType orderBy)
        {
            switch (orderBy)
            {
                case KalturaChannelFieldOrderByType.NAME_ASC:
                case KalturaChannelFieldOrderByType.CREATE_DATE_ASC:
                case KalturaChannelFieldOrderByType.START_DATE_ASC:
                case KalturaChannelFieldOrderByType.ORDER_NUM:
                    return OrderDir.ASC;
                case KalturaChannelFieldOrderByType.NAME_DESC:
                case KalturaChannelFieldOrderByType.CREATE_DATE_DESC:
                case KalturaChannelFieldOrderByType.START_DATE_DESC:
                case KalturaChannelFieldOrderByType.RELEVANCY_DESC:
                    return OrderDir.DESC;
                default:
                    throw new ClientException((int)StatusCode.Error, $"Unknown {nameof(KalturaChannelFieldOrderByType)}.");
            }
        }

        private static OrderDir GetOrderDir(KalturaMetaTagOrderBy orderBy)
        {
            switch (orderBy)
            {
                case KalturaMetaTagOrderBy.META_ASC:
                    return OrderDir.ASC;
                case KalturaMetaTagOrderBy.META_DESC:
                    return OrderDir.DESC;
                default:
                    throw new ClientException((int)StatusCode.Error, $"Unknown {nameof(KalturaMetaTagOrderBy)}.");
            }
        }

        private static OrderDir GetOrderDir(KalturaChannelSlidingWindowOrderByType orderBy)
        {
            switch (orderBy)
            {
                case KalturaChannelSlidingWindowOrderByType.LIKES_DESC:
                case KalturaChannelSlidingWindowOrderByType.RATINGS_DESC:
                case KalturaChannelSlidingWindowOrderByType.VOTES_DESC:
                case KalturaChannelSlidingWindowOrderByType.VIEWS_DESC:
                    return OrderDir.DESC;
                default:
                    throw new ClientException((int)StatusCode.Error, $"Unknown {nameof(KalturaChannelSlidingWindowOrderByType)}.");
            }
        }

        private static KalturaChannelFieldOrderByType GetKalturaChannelFieldOrderBy(OrderBy orderBy, OrderDir orderDir)
        {
            if (orderBy == OrderBy.NAME && orderDir == OrderDir.ASC)
            {
                return KalturaChannelFieldOrderByType.NAME_ASC;
            }

            if (orderBy == OrderBy.NAME && orderDir == OrderDir.DESC)
            {
                return KalturaChannelFieldOrderByType.NAME_DESC;
            }

            if (orderBy == OrderBy.START_DATE && orderDir == OrderDir.ASC)
            {
                return KalturaChannelFieldOrderByType.START_DATE_ASC;
            }

            if (orderBy == OrderBy.START_DATE && orderDir == OrderDir.DESC)
            {
                return KalturaChannelFieldOrderByType.START_DATE_DESC;
            }

            if (orderBy == OrderBy.CREATE_DATE && orderDir == OrderDir.ASC)
            {
                return KalturaChannelFieldOrderByType.CREATE_DATE_ASC;
            }

            if (orderBy == OrderBy.CREATE_DATE && orderDir == OrderDir.DESC)
            {
                return KalturaChannelFieldOrderByType.CREATE_DATE_DESC;
            }

            if (orderBy == OrderBy.RELATED)
            {
                return KalturaChannelFieldOrderByType.RELEVANCY_DESC;
            }

            if (orderBy == OrderBy.ID)
            {
                return KalturaChannelFieldOrderByType.ORDER_NUM;
            }

            throw new ClientException((int)StatusCode.Error, $"{nameof(KalturaChannelFieldOrderByType)} can not be defined: {nameof(orderBy)}={orderBy}, {nameof(orderDir)}={orderDir}.");
        }

        private static KalturaMetaTagOrderBy GetKalturaMetaTagOrderBy(OrderBy orderBy, OrderDir orderDir)
        {
            if (orderBy == OrderBy.META && orderDir == OrderDir.ASC)
            {
                return KalturaMetaTagOrderBy.META_ASC;
            }

            if (orderBy == OrderBy.META && orderDir == OrderDir.DESC)
            {
                return KalturaMetaTagOrderBy.META_DESC;
            }

            throw new ClientException((int)StatusCode.Error, $"{nameof(KalturaMetaTagOrderBy)} can not be defined: {nameof(orderBy)}={orderBy}, {nameof(orderDir)}={orderDir}.");
        }

        private static KalturaChannelSlidingWindowOrderByType GetKalturaChannelSlidingWindowOrderBy(OrderBy orderBy, OrderDir orderDir)
        {
            if (orderBy == OrderBy.LIKE_COUNTER)
            {
                return KalturaChannelSlidingWindowOrderByType.LIKES_DESC;
            }

            if (orderBy == OrderBy.RATING)
            {
                return KalturaChannelSlidingWindowOrderByType.RATINGS_DESC;
            }

            if (orderBy == OrderBy.VOTES_COUNT)
            {
                return KalturaChannelSlidingWindowOrderByType.VOTES_DESC;
            }

            if (orderBy == OrderBy.VIEWS)
            {
                return KalturaChannelSlidingWindowOrderByType.VIEWS_DESC;
            }

            throw new ClientException((int)StatusCode.Error, $"{nameof(KalturaChannelSlidingWindowOrderByType)} can not be defined: {nameof(orderBy)}={orderBy}, {nameof(orderDir)}={orderDir}.");
        }

        private static KalturaChannelOrderBy GetKalturaChannelOrderBy(OrderBy orderBy, OrderDir orderDir)
        {
            if (orderBy == OrderBy.NAME && orderDir == OrderDir.ASC)
            {
                return KalturaChannelOrderBy.NAME_ASC;
            }

            if (orderBy == OrderBy.NAME && orderDir == OrderDir.DESC)
            {
                return KalturaChannelOrderBy.NAME_DESC;
            }

            if (orderBy == OrderBy.START_DATE && orderDir == OrderDir.ASC)
            {
                return KalturaChannelOrderBy.START_DATE_ASC;
            }

            if (orderBy == OrderBy.START_DATE && orderDir == OrderDir.DESC)
            {
                return KalturaChannelOrderBy.START_DATE_DESC;
            }

            if (orderBy == OrderBy.CREATE_DATE && orderDir == OrderDir.ASC)
            {
                return KalturaChannelOrderBy.CREATE_DATE_ASC;
            }

            if (orderBy == OrderBy.CREATE_DATE && orderDir == OrderDir.DESC)
            {
                return KalturaChannelOrderBy.CREATE_DATE_DESC;
            }

            if (orderBy == OrderBy.ID)
            {
                return KalturaChannelOrderBy.ORDER_NUM;
            }

            if (orderBy == OrderBy.RELATED)
            {
                return KalturaChannelOrderBy.RELEVANCY_DESC;
            }

            if (orderBy == OrderBy.VIEWS)
            {
                return KalturaChannelOrderBy.VIEWS_DESC;
            }

            if (orderBy == OrderBy.RATING)
            {
                return KalturaChannelOrderBy.RATINGS_DESC;
            }

            if (orderBy == OrderBy.LIKE_COUNTER)
            {
                return KalturaChannelOrderBy.LIKES_DESC;
            }

            if (orderBy == OrderBy.VOTES_COUNT)
            {
                return KalturaChannelOrderBy.VOTES_DESC;
            }

            throw new ClientException((int)StatusCode.Error, $"{nameof(KalturaChannelOrderBy)} can not be defined: {nameof(orderBy)}={orderBy}, {nameof(orderDir)}={orderDir}.");
        }

        private static KalturaChannelOrder GetKalturaChannelOrder(IEnumerable<AssetOrder> orderingParameters)
        {
            var channelOrder = new KalturaChannelOrder();

            var assetOrder = orderingParameters.First();
            if (assetOrder is AssetOrderByMeta orderByMeta)
            {
                var orderBy = GetKalturaMetaTagOrderBy(orderByMeta.Field, orderByMeta.Direction);
                channelOrder.DynamicOrderBy = new KalturaDynamicOrderBy
                {
                    OrderBy = orderBy,
                    Name = orderByMeta.MetaName
                };
            }
            else if (assetOrder is AssetSlidingWindowOrder slidingWindowOrder)
            {
                var orderBy = GetKalturaChannelOrderBy(slidingWindowOrder.Field, slidingWindowOrder.Direction);
                channelOrder.orderBy = orderBy;
                channelOrder.SlidingWindowPeriod = slidingWindowOrder.SlidingWindowPeriod;
            }
            else if (assetOrder != null)
            {
                var orderBy = GetKalturaChannelOrderBy(assetOrder.Field, assetOrder.Direction);
                channelOrder.orderBy = orderBy;
            }
            else
            {
                throw new ClientException((int)StatusCode.Error, $"{nameof(KalturaChannelOrder)} can not be defined.");
            }

            return channelOrder;
        }

        private static OrderObj GetOrderObj(IEnumerable<KalturaBaseChannelOrder> orderingParameters)
        {
            var orderObj = new OrderObj();
            var channelOrder = orderingParameters.FirstOrDefault();
            switch (channelOrder)
            {
                case KalturaChannelDynamicOrder dynamicOrder:
                    orderObj.m_eOrderBy = GetOrderBy(dynamicOrder.OrderBy);
                    orderObj.m_eOrderDir = GetOrderDir(dynamicOrder.OrderBy);
                    orderObj.m_sOrderValue = dynamicOrder.Name;
                    break;
                case KalturaChannelSlidingWindowOrder slidingWindowOrder:
                    orderObj.m_eOrderBy = GetOrderBy(slidingWindowOrder.OrderBy);
                    orderObj.m_eOrderDir = GetOrderDir(slidingWindowOrder.OrderBy);
                    orderObj.m_bIsSlidingWindowField = true;
                    orderObj.lu_min_period_id = slidingWindowOrder.SlidingWindowPeriod;
                    orderObj.isSlidingWindowFromRestApi = true;
                    break;
                case KalturaChannelFieldOrder fieldOrder:
                    orderObj.m_eOrderBy = GetOrderBy(fieldOrder.OrderBy);
                    orderObj.m_eOrderDir = GetOrderDir(fieldOrder.OrderBy);
                    break;
                case null:
                    orderObj = null;
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, $"{nameof(OrderObj)} can not be defined.");
            }

            return orderObj;
        }

        // ReSharper disable once UnusedMember.Global
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

        private static KalturaSlimAsset ResolveKalturaSlimAsset(ApiObjects.MediaMarks.DevicePlayData devicePlayData)
        {
            if (devicePlayData == null || devicePlayData.AssetId == 0)
                return null;

            var sa = new KalturaSlimAsset { Id = devicePlayData.AssetId.ToString() };


            if (Enum.TryParse<KalturaAssetType>(devicePlayData.playType, true, out KalturaAssetType _type))
            {
                sa.Type = _type;
            }
            else
            {
                switch (devicePlayData.playType?.ToLower())
                {
                    case "npvr":
                    {
                        sa.Type = KalturaAssetType.recording;
                        break;
                    }
                    case "program":
                    {
                        sa.Type = KalturaAssetType.epg;
                        break;
                    }
                    default:
                        break;
                }
            }
            return sa;
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
                        value = MultilingualStringFactory.Create(tag.Values)
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
                        value = MultilingualStringFactory.Create(containers)
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
                        value = MultilingualStringFactory.Create(meta.Values)
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
                        value = MultilingualStringFactory.Create(containers)
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

            extraParams.Add("padding_before_program_starts", new KalturaStringValue { value = media.PaddingBeforeProgramStarts.ToString() });
            extraParams.Add("padding_after_program_ends", new KalturaStringValue { value = media.PaddingAfterProgramEnds.ToString() });

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
                    Objects = tag.Values.Select(v => new KalturaMultilingualStringValue() { value = MultilingualStringFactory.Create(v) }).ToList()
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
                        value = new KalturaMultilingualStringValue() { value = MultilingualStringFactory.Create(meta.Value) };
                    }
                    else
                    {
                        value = new KalturaMultilingualStringValue() { value = MultilingualStringFactory.Create(meta.Value.ToList(), meta.m_sValue) };
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
                Type = Mapper.Map<KalturaAssetType>(assetBookmark.AssetType),
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

        private static bool SetOrderByUpdate(KalturaCategoryItemOrderBy orderBy)
        {
            return (orderBy == KalturaCategoryItemOrderBy.UPDATE_DATE_ASC || orderBy == KalturaCategoryItemOrderBy.UPDATE_DATE_DESC);
        }

        private static TimeSlot ConvertToTimeSlot(long? startDateInSeconds, long? endDateInSeconds, HashSet<string> nullableProperties)
        {
            if (nullableProperties?.Count > 0)
            {
                if (nullableProperties.Contains("startdateinseconds"))
                {
                    startDateInSeconds = 0;
                }

                if (nullableProperties.Contains("enddateinseconds"))
                {
                    endDateInSeconds = 0;
                }
            }
            if (startDateInSeconds.HasValue || endDateInSeconds.HasValue)
            {
                return new TimeSlot() { StartDateInSeconds = startDateInSeconds, EndDateInSeconds = endDateInSeconds };
            }

            return null;
        }

        private static List<ManualAsset> ConvertToManualAssets(List<KalturaManualCollectionAsset> assets)
        {
            List<ManualAsset> manualMedias = null;

            if (assets?.Count > 0)
            {
                manualMedias = new List<ManualAsset>();
                long assetId;
                for (int orderNum = 1; orderNum <= assets.Count; orderNum++)
                {
                    long.TryParse(assets[orderNum - 1].Id, out assetId);
                    manualMedias.Add(new ManualAsset() { AssetId = assetId, AssetType = ConvertToAssetTypes(assets[orderNum - 1].Type), OrderNum = orderNum });
                }
            }

            return manualMedias;
        }

        public static eAssetTypes ConvertToAssetTypes(KalturaManualCollectionAssetType type)
        {
            switch (type)
            {
                case KalturaManualCollectionAssetType.media:
                    return eAssetTypes.MEDIA;
                case KalturaManualCollectionAssetType.epg:
                    return eAssetTypes.EPG;
                default:
                    throw new ClientException((int)StatusCode.Error, "Invalid assetType");
            }
        }

        public static KalturaManualCollectionAssetType ConvertToAssetTypes(eAssetTypes type)
        {
            switch (type)
            {
                case eAssetTypes.EPG:
                    return KalturaManualCollectionAssetType.epg;
                case eAssetTypes.MEDIA:
                    return KalturaManualCollectionAssetType.media;
                default:
                    throw new ClientException((int)StatusCode.Error, "Invalid assetType");
            }
        }

        private static List<KalturaManualCollectionAsset> ConvertToManualAssets(List<ManualAsset> manualAssets)
        {
            if (manualAssets?.Count > 0)
            {
                return manualAssets.OrderBy(x => x.OrderNum).ToList().Select(x => new KalturaManualCollectionAsset() { Id = x.AssetId.ToString(), Type = ConvertToAssetTypes(x.AssetType) }).ToList();
            }

            return null;
        }

        public static ChannelType? ConvertChannelType(KalturaChannelStruct? type)
        {
            if (type.HasValue)
            {
                switch (type)
                {
                    case KalturaChannelStruct.Dynamic:
                        return ChannelType.KSQL;
                    case KalturaChannelStruct.Manual:
                        return ChannelType.Manual;
                    default:
                        return null;
                }
            }

            return null;
        }

        public static void ConvertChannelsOrderBy (KalturaChannelsOrderBy kOrderBy, out ChannelOrderBy orderBy, out ApiObjects.SearchObjects.OrderDir orderDirection)
        {
            orderBy = ChannelOrderBy.Id;
            orderDirection = ApiObjects.SearchObjects.OrderDir.NONE;

            switch (kOrderBy)
            {
                case KalturaChannelsOrderBy.NONE:
                    orderBy = ChannelOrderBy.Id;
                    orderDirection = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaChannelsOrderBy.NAME_ASC:
                    orderBy = ChannelOrderBy.Name;
                    orderDirection = ApiObjects.SearchObjects.OrderDir.ASC;
                    break;
                case KalturaChannelsOrderBy.NAME_DESC:
                    orderBy = ChannelOrderBy.Name;
                    orderDirection = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaChannelsOrderBy.CREATE_DATE_ASC:
                    orderBy = ChannelOrderBy.CreateDate;
                    orderDirection = ApiObjects.SearchObjects.OrderDir.ASC;
                    break;
                case KalturaChannelsOrderBy.CREATE_DATE_DESC:
                    orderBy = ChannelOrderBy.CreateDate;
                    orderDirection = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                case KalturaChannelsOrderBy.UPDATE_DATE_ASC:
                    orderBy = ChannelOrderBy.UpdateDate;
                    orderDirection = ApiObjects.SearchObjects.OrderDir.ASC;
                    break;
                case KalturaChannelsOrderBy.UPDATE_DATE_DESC:
                    orderBy = ChannelOrderBy.UpdateDate;
                    orderDirection = ApiObjects.SearchObjects.OrderDir.DESC;
                    break;
                default:
                    break;
            }
        }
    }
}