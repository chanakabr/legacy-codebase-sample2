using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Epg;
using ApiObjects.Rules;
using ApiObjects.SearchObjects;
using ApiObjects.Statistics;
using AutoMapper;
using Catalog.Response;
using Core.Catalog;
using Core.Catalog.CatalogManagement;
using Core.Catalog.Response;
using Core.Users;
using GroupsCacheManager;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.API;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.ObjectsConvertor.Mapping.Utils;
using WebAPI.Utils;
using AutoMapper.Configuration;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class CatalogMappings
    {        

        public static void RegisterMappings(MapperConfigurationExpression cfg)
        {
            //MediaPicture to Image
            cfg.CreateMap<Picture, KalturaMediaImage>()
                 .ForMember(dest => dest.Url, opt => opt.ResolveUsing(src => src.m_sURL))
                 .ForMember(dest => dest.Height, opt => opt.ResolveUsing(src => GetPictureHeight(src.m_sSize)))
                 .ForMember(dest => dest.Width, opt => opt.ResolveUsing(src => GetPictureWidth(src.m_sSize)))
                 .ForMember(dest => dest.Ratio, opt => opt.ResolveUsing(src => src.ratio))
                 .ForMember(dest => dest.IsDefault, opt => opt.ResolveUsing(src => src.isDefault))
                 .ForMember(dest => dest.Version, opt => opt.ResolveUsing(src => src.version))
                 .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.id));

            //EPGPicture to Image
            cfg.CreateMap<EpgPicture, KalturaMediaImage>()
                 .ForMember(dest => dest.Url, opt => opt.ResolveUsing(src => src.Url))
                 .ForMember(dest => dest.Height, opt => opt.ResolveUsing(src => src.PicHeight))
                 .ForMember(dest => dest.Width, opt => opt.ResolveUsing(src => src.PicWidth))
                 .ForMember(dest => dest.Ratio, opt => opt.ResolveUsing(src => src.Ratio))
                 .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
                 .ForMember(dest => dest.Version, opt => opt.ResolveUsing(src => src.Version));

            //File 
            cfg.CreateMap<FileMedia, KalturaMediaFile>()
                 .ForMember(dest => dest.AssetId, opt => opt.ResolveUsing(src => src.m_nMediaID))
                 .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.m_nFileId))
                 .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => src.m_sFileFormat))
                 .ForMember(dest => dest.Url, opt => opt.ResolveUsing(src => src.m_sUrl))
                 .ForMember(dest => dest.Duration, opt => opt.ResolveUsing(src => src.m_nDuration))
                 .ForMember(dest => dest.ExternalId, opt => opt.ResolveUsing(src => src.m_sCoGUID))
                 .ForMember(dest => dest.BillingType, opt => opt.ResolveUsing(src => src.m_sBillingType))
                 .ForMember(dest => dest.Quality, opt => opt.ResolveUsing(src => src.Quality))
                 .ForMember(dest => dest.HandlingType, opt => opt.ResolveUsing(src => src.HandlingType))
                 .ForMember(dest => dest.CdnName, opt => opt.ResolveUsing(src => src.StreamingCompanyName))
                 .ForMember(dest => dest.CdnCode, opt => opt.ResolveUsing(src => src.m_nCdnID))
                 .ForMember(dest => dest.AltCdnCode, opt => opt.ResolveUsing(src => src.m_sAltUrl))
                 .ForMember(dest => dest.PPVModules, opt => opt.ResolveUsing(src => BuildPPVModulesList(src.PPVModules)))
                 .ForMember(dest => dest.ProductCode, opt => opt.ResolveUsing(src => src.ProductCode))
                 .ForMember(dest => dest.FileSize, opt => opt.ResolveUsing(src => src.FileSize));

            //BuzzScore
            cfg.CreateMap<BuzzWeightedAverScore, KalturaBuzzScore>()
                 .ForMember(dest => dest.UpdateDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.UpdateDate)))
                 .ForMember(dest => dest.NormalizedAvgScore, opt => opt.ResolveUsing(src => src.NormalizedWeightedAverageScore))
                 .ForMember(dest => dest.AvgScore, opt => opt.ResolveUsing(src => src.WeightedAverageScore));

            //AssetStats 
            cfg.CreateMap<AssetStatsResult, KalturaAssetStatistics>()
                 .ForMember(dest => dest.AssetId, opt => opt.ResolveUsing(src => src.m_nAssetID))
                 .ForMember(dest => dest.Likes, opt => opt.ResolveUsing(src => src.m_nLikes))
                 .ForMember(dest => dest.Views, opt => opt.ResolveUsing(src => src.m_nViews))
                 .ForMember(dest => dest.RatingCount, opt => opt.ResolveUsing(src => src.m_nVotes))
                 .ForMember(dest => dest.Rating, opt => opt.ResolveUsing(src => src.m_dRate))
                 .ForMember(dest => dest.BuzzAvgScore, opt => opt.ResolveUsing(src => src.m_buzzAverScore));

            //Media to AssetInfo
            cfg.CreateMap<MediaObj, KalturaAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.AssetId))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => KalturaMultilingualString.GetCurrent(src.Name, src.m_sName)))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => KalturaMultilingualString.GetCurrent(src.Description, src.m_sDescription)))
                .ForMember(dest => dest.StartDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dStartDate)))
                .ForMember(dest => dest.EndDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dEndDate)))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => src.m_oMediaType.m_nTypeID))
                .ForMember(dest => dest.Metas, opt => opt.ResolveUsing(src => BuildMetasDictionary(src.m_lMetas)))
                .ForMember(dest => dest.Tags, opt => opt.ResolveUsing(src => BuildTagsDictionary(src.m_lTags)))
                .ForMember(dest => dest.ExtraParams, opt => opt.ResolveUsing(src => BuildExtraParamsDictionary(src)));

            //EPG to AssetInfo
            cfg.CreateMap<EPGChannelProgrammeObject, KalturaAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.EPG_ID))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => KalturaMultilingualString.GetCurrent(src.ProgrammeName, src.NAME)))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => KalturaMultilingualString.GetCurrent(src.ProgrammeDescription, src.DESCRIPTION)))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => 0))
                .ForMember(dest => dest.StartDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.START_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.EndDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.END_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.Metas, opt => opt.ResolveUsing(src => BuildMetasDictionary(src.EPG_Meta)))
                .ForMember(dest => dest.Tags, opt => opt.ResolveUsing(src => BuildTagsDictionary(src.EPG_TAGS)))
                .ForMember(dest => dest.ExtraParams, opt => opt.ResolveUsing(src => BuildExtraParamsDictionary(src)));

            //EPG to AssetInfo
            cfg.CreateMap<ProgramObj, KalturaAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.AssetId))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => KalturaMultilingualString.GetCurrent(src.m_oProgram.ProgrammeName, src.m_oProgram.NAME)))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => KalturaMultilingualString.GetCurrent(src.m_oProgram.ProgrammeDescription, src.m_oProgram.DESCRIPTION)))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => 0))
                .ForMember(dest => dest.StartDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.m_oProgram.START_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.EndDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.m_oProgram.END_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.Metas, opt => opt.ResolveUsing(src => BuildMetasDictionary(src.m_oProgram.EPG_Meta)))
                .ForMember(dest => dest.Tags, opt => opt.ResolveUsing(src => BuildTagsDictionary(src.m_oProgram.EPG_TAGS)))
                .ForMember(dest => dest.ExtraParams, opt => opt.ResolveUsing(src => BuildExtraParamsDictionary(src.m_oProgram)));

            //EPG (recording) to AssetInfo
            cfg.CreateMap<RecordingObj, KalturaRecordingAsset>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Program.AssetId))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.Program.m_oProgram.ProgrammeName)))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.Program.m_oProgram.ProgrammeDescription)))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => 0))
                .ForMember(dest => dest.StartDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.Program.m_oProgram.START_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.EndDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.Program.m_oProgram.END_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.Metas, opt => opt.ResolveUsing(src => BuildMetasDictionary(src.Program.m_oProgram.EPG_Meta)))
                .ForMember(dest => dest.Tags, opt => opt.ResolveUsing(src => BuildTagsDictionary(src.Program.m_oProgram.EPG_TAGS)))
                ;

            //EPG (recording) to AssetInfo
            cfg.CreateMap<RecordingObj, KalturaAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.RecordingId))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => KalturaMultilingualString.GetCurrent(src.Program.m_oProgram.ProgrammeName, src.Program.m_oProgram.NAME)))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => KalturaMultilingualString.GetCurrent(src.Program.m_oProgram.ProgrammeDescription, src.Program.m_oProgram.DESCRIPTION)))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => 0))
                .ForMember(dest => dest.StartDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.Program.m_oProgram.START_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.EndDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.Program.m_oProgram.END_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.Metas, opt => opt.ResolveUsing(src => BuildMetasDictionary(src.Program.m_oProgram.EPG_Meta)))
                .ForMember(dest => dest.Tags, opt => opt.ResolveUsing(src => BuildTagsDictionary(src.Program.m_oProgram.EPG_TAGS)))
                ;

            //Media to SlimAssetInfo
            cfg.CreateMap<MediaObj, KalturaBaseAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.AssetId))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.Name).ToString()))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.Description).ToString()))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => src.m_oMediaType.m_nTypeID));

            //Media to SlimAssetInfo
            cfg.CreateMap<ProgramObj, KalturaBaseAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.AssetId))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.m_oProgram.ProgrammeName).ToString()))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.m_oProgram.ProgrammeDescription).ToString()))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => (int)AssetType.epg));

            //channelObj to Channel
            cfg.CreateMap<channelObj, WebAPI.Models.Catalog.KalturaChannel>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.m_nChannelID))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.m_sTitle)))
                .ForMember(dest => dest.Images, opt => opt.ResolveUsing(src => src.m_lPic))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.m_sDescription)));

            //Channel (Catalog) to Channel
            cfg.CreateMap<Channel, WebAPI.Models.Catalog.KalturaChannel>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.m_nChannelID))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.m_sName)))
                .ForMember(dest => dest.AssetTypes, opt => opt.ResolveUsing(src => src.m_nMediaType))
                .ForMember(dest => dest.MediaTypes, opt => opt.ResolveUsing(src => src.m_nMediaType))
                .ForMember(dest => dest.FilterExpression, opt => opt.ResolveUsing(src => src.filterQuery))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.m_sDescription)))
                .ForMember(dest => dest.IsActive, opt => opt.ResolveUsing(src => src.m_nIsActive))
                .ForMember(dest => dest.Order, opt => opt.ResolveUsing(src => ConvertOrderObjToAssetOrder(src.m_OrderObject.m_eOrderBy, src.m_OrderObject.m_eOrderDir)))
                .ForMember(dest => dest.GroupBy, opt => opt.ResolveUsing(src => ConvertToGroupBy(src.searchGroupBy)));

            //KalturaChannelProfile to KSQLChannel
            cfg.CreateMap<WebAPI.Models.API.KalturaChannelProfile, KSQLChannel>()
               .ForMember(dest => dest.ID, opt => opt.ResolveUsing(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name))
               .ForMember(dest => dest.AssetTypes, opt => opt.ResolveUsing(src => src.AssetTypes))
               .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => src.Description))
               .ForMember(dest => dest.FilterQuery, opt => opt.ResolveUsing(src => src.FilterExpression))
               .ForMember(dest => dest.IsActive, opt => opt.ResolveUsing(src => Convert.ToInt32(src.IsActive)))
               .ForMember(dest => dest.Order, opt => opt.ResolveUsing(src => ApiMappings.ConvertOrderToOrderObj(src.Order)))
               ;

            //KSQLChannel to KalturaChannelProfile
            cfg.CreateMap<KSQLChannel, WebAPI.Models.API.KalturaChannelProfile>()
               .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.ID))
               .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name))
               .ForMember(dest => dest.AssetTypes, opt => opt.ResolveUsing(src => src.AssetTypes))
               .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => src.Description))
               .ForMember(dest => dest.FilterExpression, opt => opt.ResolveUsing(src => src.FilterQuery))
               .ForMember(dest => dest.IsActive, opt => opt.ResolveUsing(src => Convert.ToBoolean(src.IsActive)))
               .ForMember(dest => dest.Order, opt => opt.ResolveUsing(src => ApiMappings.ConvertOrderObjToOrder(src.Order)));

            //KalturaChannel to KSQLChannel
            cfg.CreateMap<KalturaChannel, KSQLChannel>()
               .ForMember(dest => dest.ID, opt => opt.ResolveUsing(src => src.Id))
               .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name != null ? src.Name.GetDefaultLanugageValue() : src.OldName))
               .ForMember(dest => dest.AssetTypes, opt => opt.ResolveUsing(src => src.getAssetTypes()))
               .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => src.Description != null? src.Description.GetDefaultLanugageValue() : src.OldDescription))
               .ForMember(dest => dest.FilterQuery, opt => opt.ResolveUsing(src => src.FilterExpression))
               .ForMember(dest => dest.IsActive, opt => opt.ResolveUsing(src => src.IsActive.HasValue && src.IsActive.Value ? 1 : 0))
               .ForMember(dest => dest.Order, opt => opt.ResolveUsing(src => ConvertAssetOrderToOrderObj(src.Order.Value)))
               .ForMember(dest => dest.GroupBy, opt => opt.ResolveUsing(src => ConvertAssetGroupByToGroupBy(src.GroupBy)));

            //KSQLChannel to KalturaChannel
            cfg.CreateMap<KSQLChannel, KalturaChannel>()
               .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.ID))
               .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.Name)))
               .ForMember(dest => dest.OldName, opt => opt.ResolveUsing(src => src.Name))
               .ForMember(dest => dest.AssetTypes, opt => opt.ResolveUsing(src => src.AssetTypes))
               .ForMember(dest => dest.MediaTypes, opt => opt.ResolveUsing(src => src.AssetTypes))
               .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.Description)))
               .ForMember(dest => dest.OldDescription, opt => opt.ResolveUsing(src => src.Description))
               .ForMember(dest => dest.FilterExpression, opt => opt.ResolveUsing(src => src.FilterQuery))
               .ForMember(dest => dest.IsActive, opt => opt.ResolveUsing(src => Convert.ToBoolean(src.IsActive)))
               .ForMember(dest => dest.Order, opt => opt.ResolveUsing(src => ConvertOrderObjToAssetOrder(src.Order)))
               .ForMember(dest => dest.GroupBy, opt => opt.ResolveUsing(src => ConvertGroupByToAssetGroupBy(src.GroupBy)));

            //Channel (Catalog) to KalturaDynamicChannel
            cfg.CreateMap<Channel, WebAPI.Models.Catalog.KalturaDynamicChannel>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.m_nChannelID))
                .ForMember(dest => dest.SystemName, opt => opt.ResolveUsing(src => src.SystemName))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.NamesInOtherLanguages, src.m_sName)))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.DescriptionInOtherLanguages, src.m_sDescription)))
                .ForMember(dest => dest.AssetTypes, opt => opt.ResolveUsing(src => src.m_nMediaType))
                .ForMember(dest => dest.Ksql, opt => opt.ResolveUsing(src => src.filterQuery))
                .ForMember(dest => dest.IsActive, opt => opt.ResolveUsing(src => Convert.ToBoolean(src.m_nIsActive)))
                .ForMember(dest => dest.OrderBy, opt => opt.ResolveUsing(src => ConvertToKalturaChannelOrder(src.m_OrderObject)))
                .ForMember(dest => dest.GroupBy, opt => opt.ResolveUsing(src => ConvertToGroupBy(src.searchGroupBy)))
                .ForMember(dest => dest.CreateDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.CreateDate)))
                .ForMember(dest => dest.UpdateDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.UpdateDate)));

            //KalturaDynamicChannel to Channel (Catalog)  
            cfg.CreateMap<WebAPI.Models.Catalog.KalturaDynamicChannel, Channel>()
               .ForMember(dest => dest.m_nChannelID, opt => opt.ResolveUsing(src => src.Id))
               .ForMember(dest => dest.SystemName, opt => opt.ResolveUsing(src => src.SystemName))
               .ForMember(dest => dest.m_sName, opt => opt.ResolveUsing(src => src.Name.GetDefaultLanugageValue()))
               .ForMember(dest => dest.NamesInOtherLanguages, opt => opt.ResolveUsing(src => src.Name.GetNoneDefaultLanugageContainer()))
               .ForMember(dest => dest.m_nMediaType, opt => opt.ResolveUsing(src => src.getAssetTypes()))
               .ForMember(dest => dest.m_sDescription, opt => opt.ResolveUsing(src => src.Description.GetDefaultLanugageValue()))
               .ForMember(dest => dest.DescriptionInOtherLanguages, opt => opt.ResolveUsing(src => src.Description.GetNoneDefaultLanugageContainer()))
               .ForMember(dest => dest.filterQuery, opt => opt.ResolveUsing(src => src.Ksql))
               .ForMember(dest => dest.m_nIsActive, opt => opt.ResolveUsing(src => src.IsActive.HasValue && src.IsActive.Value ? 1 : 0))
               .ForMember(dest => dest.m_OrderObject, opt => opt.ResolveUsing(src => ConvertAssetOrderToOrderObj(src.OrderBy)))
               .ForMember(dest => dest.searchGroupBy, opt => opt.ResolveUsing(src => ConvertToGroupBy(src.GroupBy)))
               .ForMember(dest => dest.m_nChannelTypeID, opt => opt.ResolveUsing(src => (int)ChannelType.KSQL))
               .ForMember(dest => dest.m_eOrderBy, opt => opt.Ignore())
               .ForMember(dest => dest.m_eOrderDir, opt => opt.Ignore())
               .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
               .ForMember(dest => dest.UpdateDate, opt => opt.Ignore());

            //Channel (Catalog) to KalturaManualChannel
            cfg.CreateMap<Channel, WebAPI.Models.Catalog.KalturaManualChannel>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.m_nChannelID))
                .ForMember(dest => dest.SystemName, opt => opt.ResolveUsing(src => src.SystemName))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.NamesInOtherLanguages, src.m_sName)))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.DescriptionInOtherLanguages, src.m_sDescription)))
                .ForMember(dest => dest.IsActive, opt => opt.ResolveUsing(src => Convert.ToBoolean(src.m_nIsActive)))
                .ForMember(dest => dest.OrderBy, opt => opt.ResolveUsing(src => ConvertToKalturaChannelOrder(src.m_OrderObject)))
                .ForMember(dest => dest.MediaIds, opt => opt.ResolveUsing(src => src.m_lManualMedias != null ? string.Join(",", src.m_lManualMedias.OrderBy(x => x.m_nOrderNum).Select(x => x.m_sMediaId)) : string.Empty))
                .ForMember(dest => dest.CreateDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.CreateDate)))
                .ForMember(dest => dest.UpdateDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.UpdateDate)));

            //KalturaManualChannel to Channel (Catalog)
            cfg.CreateMap<WebAPI.Models.Catalog.KalturaManualChannel, Channel>()
               .ForMember(dest => dest.m_nChannelID, opt => opt.ResolveUsing(src => src.Id))
               .ForMember(dest => dest.SystemName, opt => opt.ResolveUsing(src => src.SystemName))
               .ForMember(dest => dest.m_sName, opt => opt.ResolveUsing(src => src.Name.GetDefaultLanugageValue()))
               .ForMember(dest => dest.NamesInOtherLanguages, opt => opt.ResolveUsing(src => src.Name.GetNoneDefaultLanugageContainer()))
               .ForMember(dest => dest.m_nMediaType, opt => opt.ResolveUsing(src => src.getAssetTypes()))
               .ForMember(dest => dest.m_sDescription, opt => opt.ResolveUsing(src => src.Description.GetDefaultLanugageValue()))
               .ForMember(dest => dest.DescriptionInOtherLanguages, opt => opt.ResolveUsing(src => src.Description.GetNoneDefaultLanugageContainer()))
               .ForMember(dest => dest.m_lManualMedias, opt => opt.ResolveUsing(src => ConvertToManualMedias(src.MediaIds)))
               .ForMember(dest => dest.m_nIsActive, opt => opt.ResolveUsing(src => src.IsActive.HasValue && src.IsActive.Value ? 1 : 0))
               .ForMember(dest => dest.m_OrderObject, opt => opt.ResolveUsing(src => ConvertAssetOrderToOrderObj(src.OrderBy)))
               .ForMember(dest => dest.searchGroupBy, opt => opt.ResolveUsing(src => ConvertToGroupBy(src.GroupBy)))
               .ForMember(dest => dest.m_nChannelTypeID, opt => opt.ResolveUsing(src => (int)ChannelType.Manual))
               .ForMember(dest => dest.m_eOrderBy, opt => opt.Ignore())
               .ForMember(dest => dest.m_eOrderDir, opt => opt.Ignore())
               .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
               .ForMember(dest => dest.UpdateDate, opt => opt.Ignore());

            //CategoryResponse to Category
            cfg.CreateMap<CategoryResponse, WebAPI.Models.Catalog.KalturaOTTCategory>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.ID))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.m_sTitle))
                .ForMember(dest => dest.ParentCategoryId, opt => opt.ResolveUsing(src => src.m_nParentCategoryID))
                .ForMember(dest => dest.ChildCategories, opt => opt.ResolveUsing(src => src.m_oChildCategories))
                .ForMember(dest => dest.Channels, opt => opt.ResolveUsing(src => src.m_oChannels))
                .ForMember(dest => dest.Images, opt => opt.ResolveUsing(src => src.m_lPics));

            //AssetBookmarks to KalturaAssetBookmarks
            cfg.CreateMap<AssetBookmarks, WebAPI.Models.Catalog.KalturaAssetBookmarks>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.AssetID))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => ConvertAssetType(src.AssetType)))
                .ForMember(dest => dest.Bookmarks, opt => opt.ResolveUsing(src => src.Bookmarks));

            //Bookmark to KalturaAssetBookmark
            cfg.CreateMap<Bookmark, WebAPI.Models.Catalog.KalturaAssetBookmark>()
                .ForMember(dest => dest.User, opt => opt.ResolveUsing(src => src.User))
                .ForMember(dest => dest.Position, opt => opt.ResolveUsing(src => src.Location))
                .ForMember(dest => dest.PositionOwner, opt => opt.ResolveUsing(src => ConvertPositionOwner(src.UserType)))
                .ForMember(dest => dest.IsFinishedWatching, opt => opt.ResolveUsing(src => src.IsFinishedWatching));

            //User to KalturaBaseOTTUser
            cfg.CreateMap<User, WebAPI.Models.Users.KalturaBaseOTTUser>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.m_sSiteGUID))
                .ForMember(dest => dest.Username, opt => opt.ResolveUsing(src => src.m_oBasicData.m_sUserName))
                .ForMember(dest => dest.FirstName, opt => opt.ResolveUsing(src => src.m_oBasicData.m_sFirstName))
                .ForMember(dest => dest.LastName, opt => opt.ResolveUsing(src => src.m_oBasicData.m_sLastName));

            //UnifiedSearchResuannounclt to KalturaSlimAsset
            cfg.CreateMap<UnifiedSearchResult, WebAPI.Models.Catalog.KalturaSlimAsset>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.AssetId))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => ConvertAssetType(src.AssetType)));
            
            // Country
            cfg.CreateMap<Core.Users.Country, WebAPI.Models.Users.KalturaCountry>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.m_nObjecrtID))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.m_sCountryName))
                .ForMember(dest => dest.Code, opt => opt.ResolveUsing(src => src.m_sCountryCode));

            //BaseObject to KalturaAsset
            cfg.CreateMap<BaseObject, KalturaAsset>()
                .Include<ProgramObj, KalturaProgramAsset>()
                .Include<MediaObj, KalturaMediaAsset>()
                .Include<RecordingObj, KalturaRecordingAsset>();

            //EPG to KalturaProgramAsset
            cfg.CreateMap<EPGChannelProgrammeObject, KalturaProgramAsset>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.EPG_ID))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.ProgrammeName)))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.ProgrammeDescription)))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => 0))
                .ForMember(dest => dest.StartDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.START_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.EndDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.END_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.Metas, opt => opt.ResolveUsing(src => BuildMetasDictionary(src.EPG_Meta)))
                .ForMember(dest => dest.Tags, opt => opt.ResolveUsing(src => BuildTagsDictionary(src.EPG_TAGS)))
                .ForMember(dest => dest.Images, opt => opt.ResolveUsing(src => src.EPG_PICTURES))
                .ForMember(dest => dest.EpgChannelId, opt => opt.ResolveUsing(src => src.EPG_CHANNEL_ID))
                .ForMember(dest => dest.EpgId, opt => opt.ResolveUsing(src => src.EPG_IDENTIFIER))
                .ForMember(dest => dest.RelatedMediaId, opt => opt.ResolveUsing(src => src.media_id))
                .ForMember(dest => dest.Crid, opt => opt.ResolveUsing(src => src.CRID))
                .ForMember(dest => dest.LinearAssetId, opt => opt.ResolveUsing(src => src.LINEAR_MEDIA_ID > 0 ? (long?)src.LINEAR_MEDIA_ID : null));

            //EPG to KalturaProgramAsset
            cfg.CreateMap<ProgramObj, KalturaProgramAsset>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.AssetId))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.m_oProgram.ProgrammeName)))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.m_oProgram.ProgrammeDescription)))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => 0))
                .ForMember(dest => dest.StartDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.m_oProgram.START_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.EndDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.m_oProgram.END_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.Metas, opt => opt.ResolveUsing(src => BuildMetasDictionary(src.m_oProgram.EPG_Meta)))
                .ForMember(dest => dest.Tags, opt => opt.ResolveUsing(src => BuildTagsDictionary(src.m_oProgram.EPG_TAGS)))
                .ForMember(dest => dest.Images, opt => opt.ResolveUsing(src => src.m_oProgram.EPG_PICTURES))
                .ForMember(dest => dest.EpgChannelId, opt => opt.ResolveUsing(src => src.m_oProgram.EPG_CHANNEL_ID))
                .ForMember(dest => dest.EpgId, opt => opt.ResolveUsing(src => src.m_oProgram.EPG_IDENTIFIER))
                .ForMember(dest => dest.RelatedMediaId, opt => opt.ResolveUsing(src => src.m_oProgram.media_id))
                .ForMember(dest => dest.Crid, opt => opt.ResolveUsing(src => src.m_oProgram.CRID))
                .ForMember(dest => dest.LinearAssetId, opt => opt.ResolveUsing(src => src.m_oProgram.LINEAR_MEDIA_ID > 0 ? (long?)src.m_oProgram.LINEAR_MEDIA_ID : null));

            //EPG to KalturaProgramAsset
            cfg.CreateMap<RecordingObj, KalturaRecordingAsset>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Program.AssetId))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.Program.m_oProgram.ProgrammeName)))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.Program.m_oProgram.ProgrammeDescription)))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => 0))
                .ForMember(dest => dest.StartDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.Program.m_oProgram.START_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.EndDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.Program.m_oProgram.END_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.Metas, opt => opt.ResolveUsing(src => BuildMetasDictionary(src.Program.m_oProgram.EPG_Meta)))
                .ForMember(dest => dest.Tags, opt => opt.ResolveUsing(src => BuildTagsDictionary(src.Program.m_oProgram.EPG_TAGS)))
                .ForMember(dest => dest.Images, opt => opt.ResolveUsing(src => src.Program.m_oProgram.EPG_PICTURES))
                .ForMember(dest => dest.EpgChannelId, opt => opt.ResolveUsing(src => src.Program.m_oProgram.EPG_CHANNEL_ID))
                .ForMember(dest => dest.EpgId, opt => opt.ResolveUsing(src => src.Program.m_oProgram.EPG_IDENTIFIER))
                .ForMember(dest => dest.RelatedMediaId, opt => opt.ResolveUsing(src => src.Program.m_oProgram.media_id))
                .ForMember(dest => dest.Crid, opt => opt.ResolveUsing(src => src.Program.m_oProgram.CRID))
                .ForMember(dest => dest.LinearAssetId, opt => opt.ResolveUsing(src => src.Program.m_oProgram.LINEAR_MEDIA_ID > 0 ? (long?)src.Program.m_oProgram.LINEAR_MEDIA_ID : null))
                .ForMember(dest => dest.RecordingId, opt => opt.ResolveUsing(src => src.RecordingId))
                .ForMember(dest => dest.RecordingType, opt => opt.ResolveUsing(src => src.RecordingType));            

            //Media to KalturaMediaAsset
            cfg.CreateMap<MediaObj, KalturaMediaAsset>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.AssetId))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.Name.ToList(), src.m_sName)))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.Description.ToList(), src.m_sDescription)))
                .ForMember(dest => dest.StartDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dStartDate)))
                .ForMember(dest => dest.EndDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dEndDate)))
                .ForMember(dest => dest.CreateDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dCreationDate)))
                .ForMember(dest => dest.UpdateDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dUpdateDate)))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => src.m_oMediaType.m_nTypeID))
                .ForMember(dest => dest.Metas, opt => opt.ResolveUsing(src => BuildMetasDictionary(src.m_lMetas)))
                .ForMember(dest => dest.Tags, opt => opt.ResolveUsing(src => BuildTagsDictionary(src.m_lTags)))
                .ForMember(dest => dest.Images, opt => opt.ResolveUsing(src => src.m_lPicture))
                .ForMember(dest => dest.MediaFiles, opt => opt.ResolveUsing(src => src.m_lFiles))
                .ForMember(dest => dest.ExternalIds, opt => opt.ResolveUsing(src => src.m_ExternalIDs))
                .ForMember(dest => dest.CatchUpBuffer, opt => opt.ResolveUsing(src => src.CatchUpBuffer))
                .ForMember(dest => dest.TrickPlayBuffer, opt => opt.ResolveUsing(src => src.TrickPlayBuffer))
                .ForMember(dest => dest.TypeDescription, opt => opt.ResolveUsing(src => src.m_oMediaType.m_sTypeName))
                .ForMember(dest => dest.DeviceRule, opt => opt.ResolveUsing(src => src.DeviceRule))
                .ForMember(dest => dest.GeoBlockRule, opt => opt.ResolveUsing(src => src.GeoblockRule))
                .ForMember(dest => dest.WatchPermissionRule, opt => opt.ResolveUsing(src => src.WatchPermissionRule))
                .ForMember(dest => dest.ExternalId, opt => opt.ResolveUsing(src => src.CoGuid))
                .ForMember(dest => dest.Status, opt => opt.ResolveUsing(src => src.IsActive))
                .ForMember(dest => dest.EntryId, opt => opt.ResolveUsing(src => src.EntryId));

            //Media to KalturaMediaAsset
            cfg.CreateMap<MediaObj, KalturaLiveAsset>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.AssetId))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.Name.ToList(), src.m_sName)))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.Description.ToList(), src.m_sDescription)))
                .ForMember(dest => dest.StartDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dStartDate)))
                .ForMember(dest => dest.EndDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dEndDate)))
                .ForMember(dest => dest.CreateDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dCreationDate)))
                .ForMember(dest => dest.UpdateDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dUpdateDate)))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => src.m_oMediaType.m_nTypeID))
                .ForMember(dest => dest.Metas, opt => opt.ResolveUsing(src => BuildMetasDictionary(src.m_lMetas)))
                .ForMember(dest => dest.Tags, opt => opt.ResolveUsing(src => BuildTagsDictionary(src.m_lTags)))
                .ForMember(dest => dest.Images, opt => opt.ResolveUsing(src => src.m_lPicture))
                .ForMember(dest => dest.MediaFiles, opt => opt.ResolveUsing(src => src.m_lFiles))
                .ForMember(dest => dest.TypeDescription, opt => opt.ResolveUsing(src => src.m_oMediaType.m_sTypeName))
                .ForMember(dest => dest.DeviceRuleId, opt => opt.ResolveUsing(src => src.DeviceRule))
                .ForMember(dest => dest.GeoBlockRuleId, opt => opt.ResolveUsing(src => src.GeoblockRule))
                .ForMember(dest => dest.Status, opt => opt.ResolveUsing(src => src.IsActive))
                .ForMember(dest => dest.WatchPermissionRule, opt => opt.ResolveUsing(src => src.WatchPermissionRule))
                .ForMember(dest => dest.EntryId, opt => opt.ResolveUsing(src => src.EntryId))
                .ForMember(dest => dest.ExternalId, opt => opt.ResolveUsing(src => src.CoGuid))
                .ForMember(dest => dest.SummedCatchUpBuffer, opt => opt.ResolveUsing(src => src.CatchUpBuffer))
                .ForMember(dest => dest.BufferTrickPlay, opt => opt.Ignore())
                .ForMember(dest => dest.BufferCatchUp, opt => opt.Ignore())
                .ForMember(dest => dest.CatchUpEnabled, opt => opt.ResolveUsing(src => src.EnableCatchUp))
                .ForMember(dest => dest.CdvrEnabled, opt => opt.ResolveUsing(src => src.EnableCDVR))
                .ForMember(dest => dest.EnableCatchUpState, opt => opt.Ignore())
                .ForMember(dest => dest.EnableCdvrState, opt => opt.Ignore())
                .ForMember(dest => dest.EnableRecordingPlaybackNonEntitledChannelState, opt => opt.Ignore())
                .ForMember(dest => dest.EnableStartOverState, opt => opt.Ignore())
                .ForMember(dest => dest.EnableTrickPlayState, opt => opt.Ignore())
                .ForMember(dest => dest.ExternalCdvrId, opt => opt.Ignore())
                .ForMember(dest => dest.ExternalEpgIngestId, opt => opt.Ignore())
                .ForMember(dest => dest.RecordingPlaybackNonEntitledChannelEnabled, opt => opt.ResolveUsing(src => src.EnableRecordingPlaybackNonEntitledChannel))
                .ForMember(dest => dest.StartOverEnabled, opt => opt.ResolveUsing(src => src.EnableStartOver))
                .ForMember(dest => dest.SummedTrickPlayBuffer, opt => opt.ResolveUsing(src => src.TrickPlayBuffer))
                .ForMember(dest => dest.TrickPlayEnabled, opt => opt.ResolveUsing(src => src.EnableTrickPlay))
                .ForMember(dest => dest.ChannelType, opt => opt.Ignore())
                .ForMember(dest => dest.ExternalIds, opt => opt.ResolveUsing(src => src.m_ExternalIDs));

            //EPG to AssetInfo
            cfg.CreateMap<ProgramObj, KalturaAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.AssetId))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => KalturaMultilingualString.GetCurrent(src.m_oProgram.ProgrammeName, src.m_oProgram.NAME)))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => KalturaMultilingualString.GetCurrent(src.m_oProgram.ProgrammeDescription, src.m_oProgram.DESCRIPTION)))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => 0))
                .ForMember(dest => dest.StartDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.m_oProgram.START_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.EndDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.m_oProgram.END_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.Metas, opt => opt.ResolveUsing(src => BuildMetasDictionary(src.m_oProgram.EPG_Meta)))
                .ForMember(dest => dest.Tags, opt => opt.ResolveUsing(src => BuildTagsDictionary(src.m_oProgram.EPG_TAGS)))
                .ForMember(dest => dest.ExtraParams, opt => opt.ResolveUsing(src => BuildExtraParamsDictionary(src.m_oProgram)))
                .ForMember(dest => dest.Images, opt => opt.ResolveUsing(src => src.m_oProgram.EPG_PICTURES));

            //Comments to KalturaAssetComment
            cfg.CreateMap<Comments, KalturaAssetComment>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
                .ForMember(dest => dest.AssetId, opt => opt.ResolveUsing(src => src.m_nAssetID))
                .ForMember(dest => dest.AssetType, opt => opt.ResolveUsing(src => ConvertToKalturaAssetType(src.AssetType)))
                .ForMember(dest => dest.CreateDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dCreateDate)))
                .ForMember(dest => dest.Header, opt => opt.ResolveUsing(src => src.m_sHeader))
                .ForMember(dest => dest.SubHeader, opt => opt.ResolveUsing(src => src.m_sSubHeader))
                .ForMember(dest => dest.Text, opt => opt.ResolveUsing(src => src.m_sContentText))
                .ForMember(dest => dest.Writer, opt => opt.ResolveUsing(src => src.m_sWriter));

            // Aggregations - asset counts
            // Aggregation - asset count
            cfg.CreateMap<AggregationsResult, KalturaAssetsCount>()
                .ForMember(dest => dest.Field, opt => opt.ResolveUsing(src => src.field))
                .ForMember(dest => dest.Objects, opt => opt.ResolveUsing(src => src.results));

            cfg.CreateMap<AggregationResult, KalturaAssetCount>()
                .ForMember(dest => dest.Count, opt => opt.ResolveUsing(src => src.count))
                .ForMember(dest => dest.Value, opt => opt.ResolveUsing(src => src.value))
                .ForMember(dest => dest.SubCounts, opt => opt.ResolveUsing(src => src.subs));

            #region New Catalog Management

            // AssetStruct to KalturaAssetStruct
            cfg.CreateMap<AssetStruct, KalturaAssetStruct>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.NamesInOtherLanguages, src.Name)))
                .ForMember(dest => dest.SystemName, opt => opt.ResolveUsing(src => src.SystemName))
                .ForMember(dest => dest.IsProtected, opt => opt.ResolveUsing(src => src.IsPredefined))
                .ForMember(dest => dest.MetaIds, opt => opt.ResolveUsing(src => src.MetaIds != null ? string.Join(",", src.MetaIds) : string.Empty))
                .ForMember(dest => dest.CreateDate, opt => opt.ResolveUsing(src => src.CreateDate))
                .ForMember(dest => dest.UpdateDate, opt => opt.ResolveUsing(src => src.UpdateDate))
                .ForMember(dest => dest.Features, opt => opt.ResolveUsing(src => src.GetCommaSeparatedFeatures()))
                .ForMember(dest => dest.PluralName, opt => opt.ResolveUsing(src => src.PluralName))
                .ForMember(dest => dest.ParentId, opt => opt.ResolveUsing(src => src.ParentId))
                .ForMember(dest => dest.ConnectingMetaId, opt => opt.ResolveUsing(src => src.ConnectingMetaId))
                .ForMember(dest => dest.ConnectedParentMetaId, opt => opt.ResolveUsing(src => src.ConnectedParentMetaId));

            // KalturaAssetStruct to AssetStruct
            cfg.CreateMap<KalturaAssetStruct, AssetStruct>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name.GetDefaultLanugageValue()))
                .ForMember(dest => dest.NamesInOtherLanguages, opt => opt.ResolveUsing(src => src.Name.GetNoneDefaultLanugageContainer()))
                .ForMember(dest => dest.SystemName, opt => opt.ResolveUsing(src => src.SystemName))
                .ForMember(dest => dest.IsPredefined, opt => opt.ResolveUsing(src => src.IsProtected))
                .ForMember(dest => dest.MetaIds, opt => opt.ResolveUsing(src => ConvertAssetStructMetaIdsList(src.MetaIds)))
                .ForMember(dest => dest.CreateDate, opt => opt.ResolveUsing(src => src.CreateDate))
                .ForMember(dest => dest.UpdateDate, opt => opt.ResolveUsing(src => src.UpdateDate))
                .ForMember(dest => dest.Features, opt => opt.ResolveUsing(src => src.GetFeaturesAsHashSet()))
                .ForMember(dest => dest.PluralName, opt => opt.ResolveUsing(src => src.PluralName))
                .ForMember(dest => dest.ParentId, opt => opt.ResolveUsing(src => src.ParentId))
                .ForMember(dest => dest.ConnectingMetaId, opt => opt.ResolveUsing(src => src.ConnectingMetaId))
                .ForMember(dest => dest.ConnectedParentMetaId, opt => opt.ResolveUsing(src => src.ConnectedParentMetaId));

            // MediaFileType to KalturaMediaFileType
            cfg.CreateMap<MediaFileType, KalturaMediaFileType>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => src.Description))
                .ForMember(dest => dest.Status, opt => opt.ResolveUsing(src => src.IsActive))
                .ForMember(dest => dest.CreateDate, opt => opt.ResolveUsing(src => src.CreateDate))
                .ForMember(dest => dest.UpdateDate, opt => opt.ResolveUsing(src => src.UpdateDate))
                .ForMember(dest => dest.IsTrailer, opt => opt.ResolveUsing(src => src.IsTrailer))
                .ForMember(dest => dest.StreamerType, opt => opt.ResolveUsing(src => ConvertStreamerType(src.StreamerType)))
                .ForMember(dest => dest.DrmProfileId, opt => opt.ResolveUsing(src => src.DrmId))
                .ForMember(dest => dest.Quality, opt => opt.ResolveUsing(src => ConvertToKalturaAssetFileTypeQuality(src.Quality)))
                .ForMember(dest => dest.VideoCodecs, opt => opt.ResolveUsing(src => src.CreateMappedHashSetForKalturaMediaFileType(src.VideoCodecs)))
                .ForMember(dest => dest.AudioCodecs, opt => opt.ResolveUsing(src => src.CreateMappedHashSetForKalturaMediaFileType(src.AudioCodecs)));

            // KalturaMediaFileType to MediaFileType
            cfg.CreateMap<KalturaMediaFileType, MediaFileType>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => src.Description))
                .ForMember(dest => dest.IsActive, opt => opt.ResolveUsing(src => src.Status.HasValue ? src.Status.Value : true))
                .ForMember(dest => dest.CreateDate, opt => opt.ResolveUsing(src => src.CreateDate))
                .ForMember(dest => dest.UpdateDate, opt => opt.ResolveUsing(src => src.UpdateDate))
                .ForMember(dest => dest.IsTrailer, opt => opt.ResolveUsing(src => src.IsTrailer))
                .ForMember(dest => dest.StreamerType, opt => opt.ResolveUsing(src => ConvertStreamerType(src.StreamerType)))
                .ForMember(dest => dest.DrmId, opt => opt.ResolveUsing(src => src.DrmProfileId))
                .ForMember(dest => dest.Quality, opt => opt.ResolveUsing(src => ConvertToAssetFileTypeQuality(src.Quality)))
                .ForMember(dest => dest.VideoCodecs, opt => opt.ResolveUsing(src => src.CreateMappedHashSetForMediaFileType(src.VideoCodecs)))
                .ForMember(dest => dest.AudioCodecs, opt => opt.ResolveUsing(src => src.CreateMappedHashSetForMediaFileType(src.AudioCodecs)));

            // Topic to KalturaMeta
            cfg.CreateMap<Topic, Models.API.KalturaMeta>()
              .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
              .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.NamesInOtherLanguages, src.Name)))
              .ForMember(dest => dest.SystemName, opt => opt.ResolveUsing(src => src.SystemName))
              .ForMember(dest => dest.DataType, opt => opt.ResolveUsing(src => ConvertToKalturaMetaDataType(src.Type)))
              .ForMember(dest => dest.MultipleValue, opt => opt.ResolveUsing(src => src.Type == ApiObjects.MetaType.Tag))
              .ForMember(dest => dest.IsProtected, opt => opt.ResolveUsing(src => src.IsPredefined))
              .ForMember(dest => dest.HelpText, opt => opt.ResolveUsing(src => src.HelpText))
              .ForMember(dest => dest.Features, opt => opt.ResolveUsing(src => src.GetCommaSeparatedFeatures()))
              .ForMember(dest => dest.ParentId, opt => opt.ResolveUsing(src => src.ParentId.HasValue ? src.ParentId.Value.ToString() : null))
              .ForMember(dest => dest.CreateDate, opt => opt.ResolveUsing(src => src.CreateDate))
              .ForMember(dest => dest.UpdateDate, opt => opt.ResolveUsing(src => src.UpdateDate))
              .ForMember(dest => dest.Type, opt => opt.Ignore())
              .ForMember(dest => dest.PartnerId, opt => opt.Ignore())
              .ForMember(dest => dest.FieldName, opt => opt.Ignore())
              .ForMember(dest => dest.AssetType, opt => opt.Ignore());

            // KalturaMeta to Topic
            cfg.CreateMap<Models.API.KalturaMeta, Topic>()
              .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => !string.IsNullOrEmpty(src.Id) ? long.Parse(src.Id) : 0))
              .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name.GetDefaultLanugageValue()))
              .ForMember(dest => dest.NamesInOtherLanguages, opt => opt.ResolveUsing(src => src.Name.GetNoneDefaultLanugageContainer()))
              .ForMember(dest => dest.SystemName, opt => opt.ResolveUsing(src => src.SystemName))
              .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => ConvertToMetaType(src.DataType, src.MultipleValue)))
              .ForMember(dest => dest.IsPredefined, opt => opt.ResolveUsing(src => src.IsProtected))
              .ForMember(dest => dest.HelpText, opt => opt.ResolveUsing(src => src.HelpText))
              .ForMember(dest => dest.Features, opt => opt.ResolveUsing(src => src.GetFeaturesAsHashSet()))
              .ForMember(dest => dest.ParentId, opt => opt.ResolveUsing(src => ConvertToNullableLong(src.ParentId)))
              .ForMember(dest => dest.CreateDate, opt => opt.ResolveUsing(src => src.CreateDate))
              .ForMember(dest => dest.UpdateDate, opt => opt.ResolveUsing(src => src.UpdateDate));

            // KalturaAsset to Asset
            cfg.CreateMap<KalturaAsset, Asset>()
                .Include<KalturaMediaAsset, MediaAsset>()
                .Include<KalturaLiveAsset, LiveAsset>();

            //KalturaMediaAsset to MediaAsset
            cfg.CreateMap<KalturaMediaAsset, MediaAsset>()
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name.GetDefaultLanugageValue()))
                .ForMember(dest => dest.NamesWithLanguages, opt => opt.ResolveUsing(src => src.Name.GetNoneDefaultLanugageContainer().ToArray()))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => src.Description != null ? src.Description.GetDefaultLanugageValue() : string.Empty))
                .ForMember(dest => dest.DescriptionsWithLanguages, opt => opt.ResolveUsing(src => src.Description != null ? src.Description.GetNoneDefaultLanugageContainer().ToArray() : null))
                .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdateDate, opt => opt.Ignore())
                .ForMember(dest => dest.StartDate, opt => opt.ResolveUsing(src => ConvertToNullableDatetime(src.StartDate)))
                .ForMember(dest => dest.EndDate, opt => opt.ResolveUsing(src => ConvertToNullableDatetime(src.EndDate)))
                .ForMember(dest => dest.MediaType, opt => opt.ResolveUsing(src => new Core.Catalog.MediaType(string.Empty, src.Type.HasValue ? src.Type.Value : 0)))
                .ForMember(dest => dest.Metas, opt => opt.ResolveUsing(src => src.Metas != null ? GetMetaList(src.Metas) : new List<Metas>()))
                .ForMember(dest => dest.Tags, opt => opt.ResolveUsing(src => src.Tags != null ? GetTagsList(src.Tags) : new List<Tags>()))
                .ForMember(dest => dest.DeviceRuleId, opt => opt.ResolveUsing(src => src.DeviceRuleId))
                .ForMember(dest => dest.GeoBlockRuleId, opt => opt.ResolveUsing(src => src.GeoBlockRuleId))
                .ForMember(dest => dest.IsActive, opt => opt.ResolveUsing(src => src.Status))
                .ForMember(dest => dest.CoGuid, opt => opt.ResolveUsing(src => src.ExternalId))
                .ForMember(dest => dest.EntryId, opt => opt.ResolveUsing(src => src.EntryId));

            //KalturaLinearMediaAsset to LinearMediaAsset
            cfg.CreateMap<KalturaLiveAsset, LiveAsset>()
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name.GetDefaultLanugageValue()))
                .ForMember(dest => dest.NamesWithLanguages, opt => opt.ResolveUsing(src => src.Name.GetNoneDefaultLanugageContainer().ToArray()))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => src.Description != null ? src.Description.GetDefaultLanugageValue() : string.Empty))
                .ForMember(dest => dest.DescriptionsWithLanguages, opt => opt.ResolveUsing(src => src.Description != null ? src.Description.GetNoneDefaultLanugageContainer().ToArray() : null))
                .ForMember(dest => dest.CreateDate, opt => opt.Ignore())
                .ForMember(dest => dest.UpdateDate, opt => opt.Ignore())
                .ForMember(dest => dest.StartDate, opt => opt.ResolveUsing(src => ConvertToNullableDatetime(src.StartDate)))
                .ForMember(dest => dest.EndDate, opt => opt.ResolveUsing(src => ConvertToNullableDatetime(src.EndDate)))
                .ForMember(dest => dest.MediaType, opt => opt.ResolveUsing(src => new Core.Catalog.MediaType(string.Empty, src.Type.HasValue ? src.Type.Value : 0)))
                .ForMember(dest => dest.Metas, opt => opt.ResolveUsing(src => src.Metas != null ? GetMetaList(src.Metas) : new List<Metas>()))
                .ForMember(dest => dest.Tags, opt => opt.ResolveUsing(src => src.Tags != null ? GetTagsList(src.Tags) : new List<Tags>()))
                .ForMember(dest => dest.DeviceRuleId, opt => opt.ResolveUsing(src => src.DeviceRuleId))
                .ForMember(dest => dest.GeoBlockRuleId, opt => opt.ResolveUsing(src => src.GeoBlockRuleId))
                .ForMember(dest => dest.IsActive, opt => opt.ResolveUsing(src => src.Status))
                .ForMember(dest => dest.CoGuid, opt => opt.ResolveUsing(src => src.ExternalId))               
                .ForMember(dest => dest.EntryId, opt => opt.ResolveUsing(src => src.EntryId))
                .ForMember(dest => dest.SummedCatchUpBuffer, opt => opt.ResolveUsing(src => src.SummedCatchUpBuffer))
                .ForMember(dest => dest.SummedTrickPlayBuffer, opt => opt.ResolveUsing(src => src.SummedTrickPlayBuffer))
                .ForMember(dest => dest.BufferCatchUp, opt => opt.ResolveUsing(src => src.BufferCatchUp))
                .ForMember(dest => dest.CatchUpEnabled, opt => opt.ResolveUsing(src => src.CatchUpEnabled))
                .ForMember(dest => dest.CdvrEnabled, opt => opt.ResolveUsing(src => src.CdvrEnabled))
                .ForMember(dest => dest.EnableCatchUpState, opt => opt.ResolveUsing(src => ConvertToTstvState(src.EnableCatchUpState)))
                .ForMember(dest => dest.EnableCdvrState, opt => opt.ResolveUsing(src => ConvertToTstvState(src.EnableCdvrState)))
                .ForMember(dest => dest.EnableRecordingPlaybackNonEntitledChannelState, opt => opt.ResolveUsing(src => ConvertToTstvState(src.EnableRecordingPlaybackNonEntitledChannelState)))
                .ForMember(dest => dest.EnableStartOverState, opt => opt.ResolveUsing(src => ConvertToTstvState(src.EnableStartOverState)))
                .ForMember(dest => dest.EnableTrickPlayState, opt => opt.ResolveUsing(src => ConvertToTstvState(src.EnableTrickPlayState)))               
                .ForMember(dest => dest.ExternalCdvrId, opt => opt.ResolveUsing(src => src.ExternalCdvrId))
                .ForMember(dest => dest.ExternalEpgIngestId, opt => opt.ResolveUsing(src => src.ExternalEpgIngestId))
                .ForMember(dest => dest.RecordingPlaybackNonEntitledChannelEnabled, opt => opt.ResolveUsing(src => src.RecordingPlaybackNonEntitledChannelEnabled))
                .ForMember(dest => dest.StartOverEnabled, opt => opt.ResolveUsing(src => src.StartOverEnabled))
                .ForMember(dest => dest.BufferTrickPlay, opt => opt.ResolveUsing(src => src.BufferTrickPlay))
                .ForMember(dest => dest.TrickPlayEnabled, opt => opt.ResolveUsing(src => src.TrickPlayEnabled))
                .ForMember(dest => dest.ChannelType, opt => opt.ResolveUsing(src => ConvertToLinearChannelType(src.ChannelType)));

            // Asset to KalturaAsset
            cfg.CreateMap<Asset, KalturaAsset>()
                .Include<MediaAsset, KalturaMediaAsset>()
                .Include<LiveAsset, KalturaLiveAsset>();

            //MediaAsset to KalturaMediaAsset
            cfg.CreateMap<MediaAsset, KalturaMediaAsset>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.NamesWithLanguages, src.Name)))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.DescriptionsWithLanguages, src.Description)))
                .ForMember(dest => dest.CreateDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.CreateDate)))
                .ForMember(dest => dest.UpdateDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.UpdateDate)))
                .ForMember(dest => dest.StartDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.StartDate)))
                .ForMember(dest => dest.EndDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.EndDate)))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => src.MediaType.m_nTypeID))
                .ForMember(dest => dest.Metas, opt => opt.ResolveUsing(src => BuildMetasDictionary(src.Metas)))
                .ForMember(dest => dest.Tags, opt => opt.ResolveUsing(src => BuildTagsDictionary(src.Tags)))
                // goes to ConvertImageListToKalturaMediaImageList instead
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.MediaFiles, opt => opt.ResolveUsing(src => src.Files))
                .ForMember(dest => dest.TypeDescription, opt => opt.ResolveUsing(src => src.MediaType.m_sTypeName))
                .ForMember(dest => dest.DeviceRuleId, opt => opt.ResolveUsing(src => src.DeviceRuleId))
                .ForMember(dest => dest.GeoBlockRuleId, opt => opt.ResolveUsing(src => src.GeoBlockRuleId))
                .ForMember(dest => dest.Status, opt => opt.ResolveUsing(src => src.IsActive))
                .ForMember(dest => dest.WatchPermissionRule, opt => opt.Ignore())
                .ForMember(dest => dest.ExternalId, opt => opt.ResolveUsing(src => src.CoGuid))
                .ForMember(dest => dest.EntryId, opt => opt.ResolveUsing(src => src.EntryId));

            //LinearMediaAsset to KalturaLinearMediaAsset
            cfg.CreateMap<LiveAsset, KalturaLiveAsset>()
                .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
                .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.NamesWithLanguages, src.Name)))
                .ForMember(dest => dest.Description, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.DescriptionsWithLanguages, src.Description)))
                .ForMember(dest => dest.CreateDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.CreateDate)))
                .ForMember(dest => dest.UpdateDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.UpdateDate)))
                .ForMember(dest => dest.StartDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.StartDate)))
                .ForMember(dest => dest.EndDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.EndDate)))
                .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => src.MediaType.m_nTypeID))
                .ForMember(dest => dest.Metas, opt => opt.ResolveUsing(src => BuildMetasDictionary(src.Metas)))
                .ForMember(dest => dest.Tags, opt => opt.ResolveUsing(src => BuildTagsDictionary(src.Tags)))
                // goes to ConvertImageListToKalturaMediaImageList instead
                .ForMember(dest => dest.Images, opt => opt.Ignore())
                .ForMember(dest => dest.MediaFiles, opt => opt.ResolveUsing(src => src.Files))
                .ForMember(dest => dest.TypeDescription, opt => opt.ResolveUsing(src => src.MediaType.m_sTypeName))
                .ForMember(dest => dest.DeviceRuleId, opt => opt.ResolveUsing(src => src.DeviceRuleId))
                .ForMember(dest => dest.GeoBlockRuleId, opt => opt.ResolveUsing(src => src.GeoBlockRuleId))
                .ForMember(dest => dest.Status, opt => opt.ResolveUsing(src => src.IsActive))
                .ForMember(dest => dest.WatchPermissionRule, opt => opt.Ignore())
                .ForMember(dest => dest.EntryId, opt => opt.ResolveUsing(src => src.EntryId))
                .ForMember(dest => dest.ExternalId, opt => opt.ResolveUsing(src => src.CoGuid))
                .ForMember(dest => dest.SummedCatchUpBuffer, opt => opt.ResolveUsing(src => src.SummedCatchUpBuffer))
                .ForMember(dest => dest.BufferTrickPlay, opt => opt.ResolveUsing(src => src.BufferTrickPlay))
                .ForMember(dest => dest.BufferCatchUp, opt => opt.ResolveUsing(src => src.BufferCatchUp))
                .ForMember(dest => dest.CatchUpEnabled, opt => opt.ResolveUsing(src => src.CatchUpEnabled))
                .ForMember(dest => dest.CdvrEnabled, opt => opt.ResolveUsing(src => src.CdvrEnabled))
                .ForMember(dest => dest.EnableCatchUpState, opt => opt.ResolveUsing(src => ConvertToKalturaTimeShiftedTvState(src.EnableCatchUpState)))
                .ForMember(dest => dest.EnableCdvrState, opt => opt.ResolveUsing(src => ConvertToKalturaTimeShiftedTvState(src.EnableCdvrState)))
                .ForMember(dest => dest.EnableRecordingPlaybackNonEntitledChannelState, opt => opt.ResolveUsing(src => ConvertToKalturaTimeShiftedTvState(src.EnableRecordingPlaybackNonEntitledChannelState)))
                .ForMember(dest => dest.EnableStartOverState, opt => opt.ResolveUsing(src => ConvertToKalturaTimeShiftedTvState(src.EnableStartOverState)))
                .ForMember(dest => dest.EnableTrickPlayState, opt => opt.ResolveUsing(src => ConvertToKalturaTimeShiftedTvState(src.EnableTrickPlayState)))            
                .ForMember(dest => dest.ExternalCdvrId, opt => opt.ResolveUsing(src => src.ExternalCdvrId))
                .ForMember(dest => dest.ExternalEpgIngestId, opt => opt.ResolveUsing(src => src.ExternalEpgIngestId))
                .ForMember(dest => dest.RecordingPlaybackNonEntitledChannelEnabled, opt => opt.ResolveUsing(src => src.RecordingPlaybackNonEntitledChannelEnabled))
                .ForMember(dest => dest.StartOverEnabled, opt => opt.ResolveUsing(src => src.StartOverEnabled))
                .ForMember(dest => dest.SummedTrickPlayBuffer, opt => opt.ResolveUsing(src => src.SummedTrickPlayBuffer))
                .ForMember(dest => dest.TrickPlayEnabled, opt => opt.ResolveUsing(src => src.TrickPlayEnabled))
                .ForMember(dest => dest.ChannelType, opt => opt.ResolveUsing(src => ConvertToKalturaLinearChannelType(src.ChannelType)));

            // AssetStructMeta to KalturaAssetStructMeta
            cfg.CreateMap<AssetStructMeta, KalturaAssetStructMeta>()
                .ForMember(dest => dest.AssetStructId, opt => opt.ResolveUsing(src => src.AssetStructId))
                .ForMember(dest => dest.MetaId, opt => opt.ResolveUsing(src => src.MetaId))
                .ForMember(dest => dest.IngestReferencePath, opt => opt.ResolveUsing(src => src.IngestReferencePath))
                .ForMember(dest => dest.ProtectFromIngest, opt => opt.ResolveUsing(src => src.ProtectFromIngest))
                .ForMember(dest => dest.DefaultIngestValue, opt => opt.ResolveUsing(src => src.DefaultIngestValue))
                .ForMember(dest => dest.CreateDate, opt => opt.ResolveUsing(src => src.CreateDate))
                .ForMember(dest => dest.UpdateDate, opt => opt.ResolveUsing(src => src.UpdateDate));

            // KalturaAssetStructMeta to AssetStructMeta
            cfg.CreateMap<KalturaAssetStructMeta, AssetStructMeta>()
               .ForMember(dest => dest.AssetStructId, opt => opt.ResolveUsing(src => src.AssetStructId))
               .ForMember(dest => dest.MetaId, opt => opt.ResolveUsing(src => src.MetaId))
               .ForMember(dest => dest.IngestReferencePath, opt => opt.ResolveUsing(src => src.IngestReferencePath))
               .ForMember(dest => dest.ProtectFromIngest, opt => opt.ResolveUsing(src => src.ProtectFromIngest))
               .ForMember(dest => dest.DefaultIngestValue, opt => opt.ResolveUsing(src => src.DefaultIngestValue))
               .ForMember(dest => dest.CreateDate, opt => opt.ResolveUsing(src => src.CreateDate))
               .ForMember(dest => dest.UpdateDate, opt => opt.ResolveUsing(src => src.UpdateDate));

            #endregion

            #region Tag

            cfg.CreateMap<TagValue, KalturaTag>()
              .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.tagId))
              .ForMember(dest => dest.TagTypeId, opt => opt.ResolveUsing(src => src.topicId))
              .ForMember(dest => dest.Tag, opt => opt.ResolveUsing(src => new KalturaMultilingualString(src.TagsInOtherLanguages, src.value)))
              ;

            cfg.CreateMap<KalturaTag, TagValue>()
             .ForMember(dest => dest.topicId, opt => opt.ResolveUsing(src => src.TagTypeId))
             .ForMember(dest => dest.tagId, opt => opt.ResolveUsing(src => src.Id))
             .ForMember(dest => dest.value, opt => opt.ResolveUsing(src => src.Tag.GetDefaultLanugageValue()))
             .ForMember(dest => dest.TagsInOtherLanguages, opt => opt.ResolveUsing(src => src.Tag.GetNoneDefaultLanugageContainer()))
             ;

            #endregion       

            #region ImageType

            cfg.CreateMap<ImageType, KalturaImageType>()
              .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
              .ForMember(dest => dest.DefaultImageId, opt => opt.ResolveUsing(src => src.DefaultImageId))
              .ForMember(dest => dest.HelpText, opt => opt.ResolveUsing(src => src.HelpText))
              .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name))
              .ForMember(dest => dest.RatioId, opt => opt.ResolveUsing(src => src.RatioId))
              .ForMember(dest => dest.SystemName, opt => opt.ResolveUsing(src => src.SystemName))
              ;

            cfg.CreateMap<KalturaImageType, ImageType>()
              .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
              .ForMember(dest => dest.DefaultImageId, opt => opt.ResolveUsing(src => src.DefaultImageId))
              .ForMember(dest => dest.HelpText, opt => opt.ResolveUsing(src => src.HelpText))
              .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name))
              .ForMember(dest => dest.RatioId, opt => opt.ResolveUsing(src => src.RatioId))
              .ForMember(dest => dest.SystemName, opt => opt.ResolveUsing(src => src.SystemName))
             ;

            #endregion       

            #region Ratio

            cfg.CreateMap<Core.Catalog.CatalogManagement.Ratio, KalturaRatio>()
              .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
              .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name))
              .ForMember(dest => dest.Height, opt => opt.ResolveUsing(src => src.Height))
              .ForMember(dest => dest.Width, opt => opt.ResolveUsing(src => src.Width))
              .ForMember(dest => dest.PrecisionPrecentage, opt => opt.ResolveUsing(src => src.PrecisionPrecentage));

            cfg.CreateMap<KalturaRatio, Core.Catalog.CatalogManagement.Ratio>()
              .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
              .ForMember(dest => dest.Name, opt => opt.ResolveUsing(src => src.Name))
              .ForMember(dest => dest.Height, opt => opt.ResolveUsing(src => src.Height))
              .ForMember(dest => dest.Width, opt => opt.ResolveUsing(src => src.Width))
              .ForMember(dest => dest.PrecisionPrecentage, opt => opt.ResolveUsing(src => src.PrecisionPrecentage));

            #endregion

            #region Image

            cfg.CreateMap<Core.Catalog.CatalogManagement.Image, KalturaImage>()
              .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
              .ForMember(dest => dest.ImageObjectId, opt => opt.ResolveUsing(src => src.ImageObjectId))
              .ForMember(dest => dest.ImageTypeId, opt => opt.ResolveUsing(src => src.ImageTypeId))
              .ForMember(dest => dest.ContentId, opt => opt.ResolveUsing(src => src.ContentId))
              .ForMember(dest => dest.Url, opt => opt.ResolveUsing(src => src.Url))
              .ForMember(dest => dest.Version, opt => opt.ResolveUsing(src => src.Version))
              .ForMember(dest => dest.Status, opt => opt.ResolveUsing(src => ConvertImageStatus(src.Status)))
              .ForMember(dest => dest.ImageObjectType, opt => opt.ResolveUsing(src => ConvertImageObjectType(src.ImageObjectType)))
              .ForMember(dest => dest.IsDefault, opt => opt.ResolveUsing(src => src.IsDefault));

            cfg.CreateMap<KalturaImage, Core.Catalog.CatalogManagement.Image>()
              .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
              .ForMember(dest => dest.ImageObjectId, opt => opt.ResolveUsing(src => src.ImageObjectId))
              .ForMember(dest => dest.ImageTypeId, opt => opt.ResolveUsing(src => src.ImageTypeId))
              .ForMember(dest => dest.ContentId, opt => opt.ResolveUsing(src => src.ContentId))
              .ForMember(dest => dest.Url, opt => opt.ResolveUsing(src => src.Url))
              .ForMember(dest => dest.Version, opt => opt.ResolveUsing(src => src.Version))
              .ForMember(dest => dest.Status, opt => opt.ResolveUsing(src => ConvertImageStatus(src.Status)))
              .ForMember(dest => dest.ImageObjectType, opt => opt.ResolveUsing(src => ConvertImageObjectType(src.ImageObjectType.Value)))
              .ForMember(dest => dest.IsDefault, opt => opt.ResolveUsing(src => src.IsDefault));

            #endregion

            #region AssetFile
            //File 
            cfg.CreateMap<AssetFile, KalturaMediaFile>()
                 .ForMember(dest => dest.AdditionalData, opt => opt.ResolveUsing(src => src.AdditionalData))
                 .ForMember(dest => dest.AltExternalId, opt => opt.ResolveUsing(src => src.AltExternalId))
                 .ForMember(dest => dest.AltStreamingCode, opt => opt.ResolveUsing(src => src.AltStreamingCode))
                 .ForMember(dest => dest.AlternativeCdnAdapaterProfileId, opt => opt.ResolveUsing(src => src.AlternativeCdnAdapaterProfileId))
                 .ForMember(dest => dest.AssetId, opt => opt.ResolveUsing(src => src.AssetId))
                 .ForMember(dest => dest.BillingType, opt => opt.ResolveUsing(src => src.BillingType))
                 .ForMember(dest => dest.Duration, opt => opt.ResolveUsing(src => src.Duration))
                 .ForMember(dest => dest.EndDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.EndDate)))
                 .ForMember(dest => dest.ExternalId, opt => opt.ResolveUsing(src => src.ExternalId))
                 .ForMember(dest => dest.ExternalStoreId, opt => opt.ResolveUsing(src => src.ExternalStoreId))
                 .ForMember(dest => dest.FileSize, opt => opt.ResolveUsing(src => src.FileSize))
                 .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
                 .ForMember(dest => dest.IsDefaultLanguage, opt => opt.ResolveUsing(src => src.IsDefaultLanguage))
                 .ForMember(dest => dest.Language, opt => opt.ResolveUsing(src => src.Language))
                 .ForMember(dest => dest.OrderNum, opt => opt.ResolveUsing(src => src.OrderNum))
                 .ForMember(dest => dest.OutputProtecationLevel, opt => opt.ResolveUsing(src => src.OutputProtecationLevel))
                 .ForMember(dest => dest.StartDate, opt => opt.ResolveUsing(src => SerializationUtils.ConvertToUnixTimestamp(src.StartDate)))
                 .ForMember(dest => dest.CdnAdapaterProfileId, opt => opt.ResolveUsing(src => src.CdnAdapaterProfileId))
                 .ForMember(dest => dest.TypeId, opt => opt.ResolveUsing(src => src.TypeId))
                 .ForMember(dest => dest.Type, opt => opt.ResolveUsing(src => src.GetTypeName()))
                 .ForMember(dest => dest.Url, opt => opt.ResolveUsing(src => src.Url))
                 .ForMember(dest => dest.Status, opt => opt.ResolveUsing(src => src.IsActive))
                 ;

            //File 
            cfg.CreateMap<KalturaMediaFile, AssetFile>()
                 .ForMember(dest => dest.AdditionalData, opt => opt.ResolveUsing(src => src.AdditionalData))
                 .ForMember(dest => dest.AltExternalId, opt => opt.ResolveUsing(src => src.AltExternalId))
                 .ForMember(dest => dest.AltStreamingCode, opt => opt.ResolveUsing(src => src.AltStreamingCode))
                 .ForMember(dest => dest.AlternativeCdnAdapaterProfileId, opt => opt.ResolveUsing(src => src.AlternativeCdnAdapaterProfileId))
                 .ForMember(dest => dest.AssetId, opt => opt.ResolveUsing(src => src.AssetId))
                 .ForMember(dest => dest.BillingType, opt => opt.ResolveUsing(src => src.BillingType))
                 .ForMember(dest => dest.Duration, opt => opt.ResolveUsing(src => src.Duration))
                 .ForMember(dest => dest.EndDate, opt => opt.ResolveUsing(src => ConvertToNullableDatetime(src.EndDate)))
                 .ForMember(dest => dest.ExternalId, opt => opt.ResolveUsing(src => src.ExternalId))
                 .ForMember(dest => dest.ExternalStoreId, opt => opt.ResolveUsing(src => src.ExternalStoreId))
                 .ForMember(dest => dest.Id, opt => opt.ResolveUsing(src => src.Id))
                 .ForMember(dest => dest.IsDefaultLanguage, opt => opt.ResolveUsing(src => src.IsDefaultLanguage))
                 .ForMember(dest => dest.Language, opt => opt.ResolveUsing(src => src.Language))
                 .ForMember(dest => dest.OrderNum, opt => opt.ResolveUsing(src => src.OrderNum))
                 .ForMember(dest => dest.OutputProtecationLevel, opt => opt.ResolveUsing(src => src.OutputProtecationLevel))
                 .ForMember(dest => dest.StartDate, opt => opt.ResolveUsing(src => ConvertToNullableDatetime(src.StartDate)))
                 .ForMember(dest => dest.Url, opt => opt.ResolveUsing(src => src.Url))
                 .ForMember(dest => dest.CdnAdapaterProfileId, opt => opt.ResolveUsing(src => src.CdnAdapaterProfileId))
                 .ForMember(dest => dest.TypeId, opt => opt.ResolveUsing(src => src.TypeId))
                 .ForMember(dest => dest.IsActive, opt => opt.ResolveUsing(src => src.Status));

            #endregion
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
                    manualMedia = new GroupsCacheManager.ManualMedia(mediaIds[orderNum-1], orderNum);
                    manualMedias.Add(manualMedia);
                }
            }

            return manualMedias;
        }


        #region New Catalog Management

        private static List<long> ConvertAssetStructMetaIdsList(string metaIds)
        {
            if (metaIds == null)
            {
                return null;
            }

            List<long> list = new List<long>();
            string[] stringValues = metaIds.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string stringValue in stringValues)
            {
                long value;
                if (long.TryParse(stringValue, out value))
                {
                    list.Add(value);
                }
                else
                {
                    throw new BadRequestException(BadRequestException.INVALID_ARGUMENT, "KalturaAssetStruct.metaIds");
                }
            }

            return list;
        }

        private static KalturaMediaFileStreamerType? ConvertStreamerType(StreamerType? type)
        {
            if (!type.HasValue)
            {
                return null;
            }

            switch (type)
            {
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

            foreach (KeyValuePair<string, KalturaValue> meta in metasDictionary)
            {
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
                    metaToAdd.m_sValue = SerializationUtils.ConvertFromUnixTimestamp(metaValue.value).ToString();
                }

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

            foreach (KeyValuePair<string, KalturaMultilingualStringValueArray> tag in tagsDictionary)
            {
                Tags tagToAdd = new Tags() { m_oTagMeta = new TagMeta(tag.Key, ApiObjects.MetaType.Tag.ToString()) };
                tagToAdd.m_lValues = tag.Value.Objects != null ? tag.Value.Objects.Select(x => x.value.GetDefaultLanugageValue()).ToList() : new List<string>();
                tags.Add(tagToAdd);
            }

            return tags;
        }

        private static DateTime? ConvertToNullableDatetime(long? date)
        {
            DateTime? response = null;
            if (date.HasValue)
            {
                response = SerializationUtils.ConvertFromUnixTimestamp(date.Value);
            }

            return response;
        }

        private static long? ConvertToNullableLong(string val)
        {
            long? response = null;
            if (!string.IsNullOrEmpty(val))
            {
                long parseResult;
                if (long.TryParse(val, out parseResult))
                {
                    response = parseResult;
                }
            }

            return response;
        }

        private static KalturaImageObjectType ConvertImageObjectType(eAssetImageType imageObjectType)
        {
            switch (imageObjectType)
            {
                case eAssetImageType.Media:
                    return KalturaImageObjectType.MEDIA_ASSET;
                    break;
                case eAssetImageType.Channel:
                    return KalturaImageObjectType.CHANNEL;
                    break;
                case eAssetImageType.Category:
                    return KalturaImageObjectType.CATEGORY;
                    break;
                case eAssetImageType.ImageType:
                    return KalturaImageObjectType.IMAGE_TYPE;
                    break;
                case eAssetImageType.DefaultPic:
                case eAssetImageType.LogoPic:
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown image object type");
                    break;
            }
        }

        public static eAssetImageType ConvertImageObjectType(KalturaImageObjectType imageObjectType)
        {
            switch (imageObjectType)
            {
                case KalturaImageObjectType.MEDIA_ASSET:
                    return eAssetImageType.Media;
                    break;
                case KalturaImageObjectType.CHANNEL:
                    return eAssetImageType.Channel;
                    break;
                case KalturaImageObjectType.CATEGORY:
                    return eAssetImageType.Category;
                    break;
                case KalturaImageObjectType.IMAGE_TYPE:
                    return eAssetImageType.ImageType;
                    break;
                case KalturaImageObjectType.PARTNER:
                case KalturaImageObjectType.PROGRAM_ASSET:
                    throw new ClientException((int)StatusCode.Error, "Not implemented yet");
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Unknown image object type");
                    break;
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
                    break;
                case KalturaAssetReferenceType.epg_external:
                    break;
                default:
                    throw new ClientException((int)StatusCode.Error, "Invalid assetType");
                    break;
            }

            return assetType;
        }

        public static List<KalturaMediaImage> ConvertImageListToKalturaMediaImageList(int groupId, List<Image> images, Dictionary<long, string> imageTypeIdToRatioNameMap)
        {
            List<KalturaMediaImage> result = new List<KalturaMediaImage>();
            if (images != null && images.Count > 0)
            {
                foreach (Image image in images)
                {
                    string ratioName = imageTypeIdToRatioNameMap != null && imageTypeIdToRatioNameMap.ContainsKey(image.ImageTypeId) ? imageTypeIdToRatioNameMap[image.ImageTypeId] : string.Empty;
                    KalturaMediaImage convertedImage = ConvertImageToKalturaImage(groupId, image, ratioName);
                    if (convertedImage != null)
                    {
                        result.Add(convertedImage);
                    }
                }
            }

            return result;
        }

        private static KalturaMediaImage ConvertImageToKalturaImage(int groupId, Image image, string ratioName)
        {
            KalturaMediaImage result = null;
            if (groupId > 0 && image != null)
            {
                result = new KalturaMediaImage()
                {
                    Id = image.ContentId,
                    IsDefault = image.IsDefault,
                    Url = image.Url,
                    Version = image.Version,
                    Ratio = ratioName
                };
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
                    result.m_eOrderDir = OrderDir.ASC;
                    break;
                case KalturaAssetOrderBy.NAME_DESC:
                    result.m_eOrderBy = OrderBy.NAME;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.VIEWS_DESC:
                    result.m_eOrderBy = OrderBy.VIEWS;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.RATINGS_DESC:
                    result.m_eOrderBy = OrderBy.RATING;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.VOTES_DESC:
                    result.m_eOrderBy = OrderBy.VOTES_COUNT;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.START_DATE_DESC:
                    result.m_eOrderBy = OrderBy.START_DATE;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.RELEVANCY_DESC:
                    result.m_eOrderBy = OrderBy.RELATED;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.START_DATE_ASC:
                    result.m_eOrderBy = OrderBy.START_DATE;
                    result.m_eOrderDir = OrderDir.ASC;
                    break;
                case KalturaAssetOrderBy.CREATE_DATE_ASC:
                    result.m_eOrderBy = OrderBy.CREATE_DATE;
                    result.m_eOrderDir = OrderDir.ASC;
                    break;
                case KalturaAssetOrderBy.CREATE_DATE_DESC:
                    result.m_eOrderBy = OrderBy.CREATE_DATE;
                    result.m_eOrderDir = OrderDir.DESC;
                    break;
                case KalturaAssetOrderBy.LIKES_DESC:
                    result.m_eOrderBy = OrderBy.LIKE_COUNTER;
                    result.m_eOrderDir = OrderDir.DESC;
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
                    result.m_eOrderDir = order.DynamicOrderBy.OrderBy.HasValue ? order.DynamicOrderBy.OrderBy.Value == KalturaMetaTagOrderBy.META_ASC ? OrderDir.ASC : OrderDir.DESC : OrderDir.ASC;
                    result.m_sOrderValue = order.DynamicOrderBy.Name;
                }
                else
                {
                    result.m_sOrderValue = null;
                    switch (order.orderBy)
                    {
                        case KalturaChannelOrderBy.NAME_ASC:
                            result.m_eOrderBy = OrderBy.NAME;
                            result.m_eOrderDir = OrderDir.ASC;
                            break;
                        case KalturaChannelOrderBy.NAME_DESC:
                            result.m_eOrderBy = OrderBy.NAME;
                            result.m_eOrderDir = OrderDir.DESC;
                            break;
                        case KalturaChannelOrderBy.VIEWS_DESC:
                            result.m_eOrderBy = OrderBy.VIEWS;
                            result.m_eOrderDir = OrderDir.DESC;
                            break;
                        case KalturaChannelOrderBy.RATINGS_DESC:
                            result.m_eOrderBy = OrderBy.RATING;
                            result.m_eOrderDir = OrderDir.DESC;
                            break;
                        case KalturaChannelOrderBy.VOTES_DESC:
                            result.m_eOrderBy = OrderBy.VOTES_COUNT;
                            result.m_eOrderDir = OrderDir.DESC;
                            break;
                        case KalturaChannelOrderBy.START_DATE_DESC:
                            result.m_eOrderBy = OrderBy.START_DATE;
                            result.m_eOrderDir = OrderDir.DESC;
                            break;
                        case KalturaChannelOrderBy.RELEVANCY_DESC:
                            result.m_eOrderBy = OrderBy.RELATED;
                            result.m_eOrderDir = OrderDir.DESC;
                            break;
                        case KalturaChannelOrderBy.START_DATE_ASC:
                            result.m_eOrderBy = OrderBy.START_DATE;
                            result.m_eOrderDir = OrderDir.ASC;
                            break;
                        case KalturaChannelOrderBy.CREATE_DATE_ASC:
                            result.m_eOrderBy = OrderBy.CREATE_DATE;
                            result.m_eOrderDir = OrderDir.ASC;
                            break;
                        case KalturaChannelOrderBy.CREATE_DATE_DESC:
                            result.m_eOrderBy = OrderBy.CREATE_DATE;
                            result.m_eOrderDir = OrderDir.DESC;
                            break;
                        case KalturaChannelOrderBy.LIKES_DESC:
                            result.m_eOrderBy = OrderBy.LIKE_COUNTER;
                            result.m_eOrderDir = OrderDir.DESC;
                            break;
                        case KalturaChannelOrderBy.ORDER_NUM:
                            result.m_eOrderBy = OrderBy.ID;
                            result.m_eOrderDir = OrderDir.ASC;
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
                        if (orderObj.m_eOrderDir == OrderDir.DESC)
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
                        if (orderObj.m_eOrderDir == OrderDir.DESC)
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
                        if (orderObj.m_eOrderDir == OrderDir.ASC)
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
                        KalturaMetaTagOrderBy metaOrderBy = orderObj.m_eOrderDir == OrderDir.DESC ? KalturaMetaTagOrderBy.META_DESC : KalturaMetaTagOrderBy.META_ASC;
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

        public static OrderDir ConvertToOrderDir(KalturaChannelOrder kalturaChannelOrder)
        {
            OrderDir orderDir = OrderDir.NONE;

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
                    orderDir = OrderDir.DESC;
                    break;
                case KalturaChannelOrderBy.NAME_ASC:
                case KalturaChannelOrderBy.START_DATE_ASC:
                case KalturaChannelOrderBy.CREATE_DATE_ASC:
                    orderDir = OrderDir.ASC;
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
                                LanguageCode = WebAPI.Utils.Utils.GetDefaultLanguage(),
                                Value = tag.Value
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
                            LanguageCode = WebAPI.Utils.Utils.GetDefaultLanguage(),
                            Value = meta.Value
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

            extraParams.Add("sys_start_date", new KalturaStringValue() { value = SerializationUtils.ConvertToUnixTimestamp(media.m_dStartDate).ToString() });
            extraParams.Add("sys_final_date", new KalturaStringValue() { value = SerializationUtils.ConvertToUnixTimestamp(media.m_dFinalDate).ToString() });
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
                else if (currentMetaTypeLowered == typeof(string).ToString().ToLower() || currentMetaType == ApiObjects.MetaType.String.ToString())
                {
                    value = new KalturaStringValue() { value = meta.m_sValue };
                }
                else if (currentMetaType == ApiObjects.MetaType.MultilingualString.ToString())
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
                        value = new KalturaLongValue() { value = SerializationUtils.ConvertToUnixTimestamp(DateTime.Parse(meta.m_sValue, CultureInfo.InvariantCulture)) };
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

        public static KalturaAssetOrderBy ConvertOrderObjToAssetOrder(OrderBy OrderBy, OrderDir OrderDir)
        {
            KalturaAssetOrderBy result = KalturaAssetOrderBy.START_DATE_DESC;

            switch (OrderBy)
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
                        if (OrderDir == OrderDir.DESC)
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
                        if (OrderDir == OrderDir.DESC)
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
                        if (OrderDir == OrderDir.ASC)
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
    }
}