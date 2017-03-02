using ApiObjects;
using ApiObjects.Catalog;
using ApiObjects.Epg;
using ApiObjects.SearchObjects;
using ApiObjects.Statistics;
using AutoMapper;
using Core.Catalog;
using Core.Catalog.Response;
using Core.Users;
using GroupsCacheManager;
using System;
using System.Collections.Generic;
using System.Linq;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.Catalog;
using WebAPI.Models.General;
using WebAPI.ObjectsConvertor.Mapping.Utils;
using WebAPI.Utils;

namespace WebAPI.ObjectsConvertor.Mapping
{
    public class CatalogMappings
    {
        public static void RegisterMappings()
        {
            //MediaPicture to Image
            Mapper.CreateMap<Picture, KalturaMediaImage>()
                 .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.m_sURL))
                 .ForMember(dest => dest.Height, opt => opt.MapFrom(src => GetPictureHeight(src.m_sSize)))
                 .ForMember(dest => dest.Width, opt => opt.MapFrom(src => GetPictureWidth(src.m_sSize)))
                 .ForMember(dest => dest.Ratio, opt => opt.MapFrom(src => src.ratio))
                 .ForMember(dest => dest.IsDefault, opt => opt.MapFrom(src => src.isDefault))
                 .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.version))
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.id));

            //EPGPicture to Image
            Mapper.CreateMap<EpgPicture, KalturaMediaImage>()
                 .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.Url))
                 .ForMember(dest => dest.Height, opt => opt.MapFrom(src => src.PicHeight))
                 .ForMember(dest => dest.Width, opt => opt.MapFrom(src => src.PicWidth))
                 .ForMember(dest => dest.Ratio, opt => opt.MapFrom(src => src.Ratio))
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                 .ForMember(dest => dest.Version, opt => opt.MapFrom(src => src.Version));

            //File 
            Mapper.CreateMap<FileMedia, KalturaMediaFile>()
                 .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.m_nMediaID))
                 .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nFileId))
                 .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.m_sFileFormat))
                 .ForMember(dest => dest.Url, opt => opt.MapFrom(src => src.m_sUrl))
                 .ForMember(dest => dest.Duration, opt => opt.MapFrom(src => src.m_nDuration))
                 .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.m_sCoGUID));

            //BuzzScore
            Mapper.CreateMap<BuzzWeightedAverScore, KalturaBuzzScore>()
                 .ForMember(dest => dest.UpdateDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.UpdateDate)))
                 .ForMember(dest => dest.NormalizedAvgScore, opt => opt.MapFrom(src => src.NormalizedWeightedAverageScore))
                 .ForMember(dest => dest.AvgScore, opt => opt.MapFrom(src => src.WeightedAverageScore));

            //AssetStats 
            Mapper.CreateMap<AssetStatsResult, KalturaAssetStatistics>()
                 .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.m_nAssetID))
                 .ForMember(dest => dest.Likes, opt => opt.MapFrom(src => src.m_nLikes))
                 .ForMember(dest => dest.Views, opt => opt.MapFrom(src => src.m_nViews))
                 .ForMember(dest => dest.RatingCount, opt => opt.MapFrom(src => src.m_nVotes))
                 .ForMember(dest => dest.Rating, opt => opt.MapFrom(src => src.m_dRate))
                 .ForMember(dest => dest.BuzzAvgScore, opt => opt.MapFrom(src => src.m_buzzAverScore));

            //Media to AssetInfo
            Mapper.CreateMap<MediaObj, KalturaAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.m_sDescription))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dStartDate)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dEndDate)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.m_oMediaType.m_nTypeID))
                .ForMember(dest => dest.Metas, opt => opt.MapFrom(src => BuildMetasDictionary(src.m_lMetas)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => BuildTagsDictionary(src.m_lTags)))
                .ForMember(dest => dest.ExtraParams, opt => opt.MapFrom(src => BuildExtraParamsDictionary(src)));

            //EPG to AssetInfo
            Mapper.CreateMap<EPGChannelProgrammeObject, KalturaAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.EPG_ID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.NAME))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.DESCRIPTION))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.START_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.END_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.Metas, opt => opt.MapFrom(src => BuildMetasDictionary(src.EPG_Meta)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => BuildTagsDictionary(src.EPG_TAGS)))
                .ForMember(dest => dest.ExtraParams, opt => opt.MapFrom(src => BuildExtraParamsDictionary(src)));

            //EPG to AssetInfo
            Mapper.CreateMap<ProgramObj, KalturaAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_oProgram.NAME))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.m_oProgram.DESCRIPTION))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.m_oProgram.START_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.m_oProgram.END_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.Metas, opt => opt.MapFrom(src => BuildMetasDictionary(src.m_oProgram.EPG_Meta)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => BuildTagsDictionary(src.m_oProgram.EPG_TAGS)))
                .ForMember(dest => dest.ExtraParams, opt => opt.MapFrom(src => BuildExtraParamsDictionary(src.m_oProgram)));

            //EPG (recording) to AssetInfo
            Mapper.CreateMap<RecordingObj, KalturaRecordingAsset>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.program.AssetId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.program.m_oProgram.NAME))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.program.m_oProgram.DESCRIPTION))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.program.m_oProgram.START_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.program.m_oProgram.END_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.Metas, opt => opt.MapFrom(src => BuildMetasDictionary(src.program.m_oProgram.EPG_Meta)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => BuildTagsDictionary(src.program.m_oProgram.EPG_TAGS)))
                ;

            //EPG (recording) to AssetInfo
            Mapper.CreateMap<RecordingObj, KalturaAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.recordingId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.program.m_oProgram.NAME))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.program.m_oProgram.DESCRIPTION))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.program.m_oProgram.START_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.program.m_oProgram.END_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.Metas, opt => opt.MapFrom(src => BuildMetasDictionary(src.program.m_oProgram.EPG_Meta)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => BuildTagsDictionary(src.program.m_oProgram.EPG_TAGS)))
                ;
            
            //Media to SlimAssetInfo
            Mapper.CreateMap<MediaObj, KalturaBaseAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.m_sDescription))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.m_oMediaType.m_nTypeID));

            //Media to SlimAssetInfo
            Mapper.CreateMap<ProgramObj, KalturaBaseAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_oProgram.NAME))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.m_oProgram.DESCRIPTION))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => (int)AssetType.epg));

            //channelObj to Channel
            Mapper.CreateMap<channelObj, WebAPI.Models.Catalog.KalturaChannel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nChannelID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sTitle))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.m_lPic))
                ;


            //Channel (Catalog) to Channel
            Mapper.CreateMap<Channel, WebAPI.Models.Catalog.KalturaChannel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nChannelID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sName))
                .ForMember(dest => dest.AssetTypes, opt => opt.MapFrom(src => src.m_nMediaType))
                .ForMember(dest => dest.MediaTypes, opt => opt.MapFrom(src => src.m_nMediaType))
                .ForMember(dest => dest.FilterExpression, opt => opt.MapFrom(src => src.filterQuery))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.m_sDescription))
                .ForMember(dest => dest.IsActive, opt => opt.MapFrom(src => src.m_nIsActive))
                .ForMember(dest => dest.Order, opt => opt.MapFrom(src => ConvertOrderObjToAssetOrder(src.m_OrderObject.m_eOrderBy, src.m_OrderObject.m_eOrderDir)))
                ;

            //CategoryResponse to Category
            Mapper.CreateMap<CategoryResponse, WebAPI.Models.Catalog.KalturaOTTCategory>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.ID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sTitle))
                .ForMember(dest => dest.ParentCategoryId, opt => opt.MapFrom(src => src.m_nParentCategoryID))
                .ForMember(dest => dest.ChildCategories, opt => opt.MapFrom(src => src.m_oChildCategories))
                .ForMember(dest => dest.Channels, opt => opt.MapFrom(src => src.m_oChannels))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.m_lPics));

            //AssetBookmarks to KalturaAssetBookmarks
            Mapper.CreateMap<AssetBookmarks, WebAPI.Models.Catalog.KalturaAssetBookmarks>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AssetID))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertAssetType(src.AssetType)))
                .ForMember(dest => dest.Bookmarks, opt => opt.MapFrom(src => src.Bookmarks));

            //Bookmark to KalturaAssetBookmark
            Mapper.CreateMap<Bookmark, WebAPI.Models.Catalog.KalturaAssetBookmark>()
                .ForMember(dest => dest.User, opt => opt.MapFrom(src => src.User))
                .ForMember(dest => dest.Position, opt => opt.MapFrom(src => src.Location))
                .ForMember(dest => dest.PositionOwner, opt => opt.MapFrom(src => ConvertPositionOwner(src.UserType)))
                .ForMember(dest => dest.IsFinishedWatching, opt => opt.MapFrom(src => src.IsFinishedWatching));

            //User to KalturaBaseOTTUser
            Mapper.CreateMap<User, WebAPI.Models.Users.KalturaBaseOTTUser>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_sSiteGUID))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.m_oBasicData.m_sUserName))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.m_oBasicData.m_sFirstName))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.m_oBasicData.m_sLastName));

            //UnifiedSearchResuannounclt to KalturaSlimAsset
            Mapper.CreateMap<UnifiedSearchResult, WebAPI.Models.Catalog.KalturaSlimAsset>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertAssetType(src.AssetType)));

            // Country
            Mapper.CreateMap<Core.Users.Country, WebAPI.Models.Users.KalturaCountry>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nObjecrtID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sCountryName))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.m_sCountryCode));

            //BaseObject to KalturaAsset
            Mapper.CreateMap<BaseObject, KalturaAsset>()
                .Include<ProgramObj, KalturaProgramAsset>()
                .Include<MediaObj, KalturaMediaAsset>()
                //.Include<RecordingObj, KalturaRecordingAsset>()
                ;

            //EPG to KalturaProgramAsset
            Mapper.CreateMap<EPGChannelProgrammeObject, KalturaProgramAsset>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.EPG_ID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.NAME))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.DESCRIPTION))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.START_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.END_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.Metas, opt => opt.MapFrom(src => BuildMetasDictionary(src.EPG_Meta)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => BuildTagsDictionary(src.EPG_TAGS)))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.EPG_PICTURES))
                .ForMember(dest => dest.EpgChannelId, opt => opt.MapFrom(src => src.EPG_CHANNEL_ID))
                .ForMember(dest => dest.EpgId, opt => opt.MapFrom(src => src.EPG_IDENTIFIER))
                .ForMember(dest => dest.RelatedMediaId, opt => opt.MapFrom(src => src.media_id))
                .ForMember(dest => dest.EnableCdvr, opt => opt.MapFrom(src => src.ENABLE_CDVR == 1))
                .ForMember(dest => dest.EnableCatchUp, opt => opt.MapFrom(src => src.ENABLE_CATCH_UP == 1))
                .ForMember(dest => dest.EnableStartOver, opt => opt.MapFrom(src => src.ENABLE_START_OVER == 1))
                .ForMember(dest => dest.EnableTrickPlay, opt => opt.MapFrom(src => src.ENABLE_TRICK_PLAY == 1))
                .ForMember(dest => dest.Crid, opt => opt.MapFrom(src => src.CRID))
                .ForMember(dest => dest.LinearAssetId, opt => opt.MapFrom(src => src.LINEAR_MEDIA_ID > 0 ? (long?) src.LINEAR_MEDIA_ID : null));

            //EPG to KalturaProgramAsset
            Mapper.CreateMap<ProgramObj, KalturaProgramAsset>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_oProgram.NAME))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.m_oProgram.DESCRIPTION))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.m_oProgram.START_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.m_oProgram.END_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.Metas, opt => opt.MapFrom(src => BuildMetasDictionary(src.m_oProgram.EPG_Meta)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => BuildTagsDictionary(src.m_oProgram.EPG_TAGS)))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.m_oProgram.EPG_PICTURES))
                .ForMember(dest => dest.EpgChannelId, opt => opt.MapFrom(src => src.m_oProgram.EPG_CHANNEL_ID))
                .ForMember(dest => dest.EpgId, opt => opt.MapFrom(src => src.m_oProgram.EPG_IDENTIFIER))
                .ForMember(dest => dest.RelatedMediaId, opt => opt.MapFrom(src => src.m_oProgram.media_id))
                .ForMember(dest => dest.EnableCdvr, opt => opt.MapFrom(src => src.m_oProgram.ENABLE_CDVR == 1))
                .ForMember(dest => dest.EnableCatchUp, opt => opt.MapFrom(src => src.m_oProgram.ENABLE_CATCH_UP == 1))
                .ForMember(dest => dest.EnableStartOver, opt => opt.MapFrom(src => src.m_oProgram.ENABLE_START_OVER == 1))
                .ForMember(dest => dest.EnableTrickPlay, opt => opt.MapFrom(src => src.m_oProgram.ENABLE_TRICK_PLAY == 1))
                .ForMember(dest => dest.Crid, opt => opt.MapFrom(src => src.m_oProgram.CRID))
                .ForMember(dest => dest.LinearAssetId, opt => opt.MapFrom(src => src.m_oProgram.LINEAR_MEDIA_ID > 0 ? (long?)src.m_oProgram.LINEAR_MEDIA_ID : null));

            //EPG to KalturaProgramAsset
            Mapper.CreateMap<RecordingObj, KalturaRecordingAsset>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.program.AssetId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.program.m_oProgram.NAME))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.program.m_oProgram.DESCRIPTION))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.program.m_oProgram.START_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.program.m_oProgram.END_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.Metas, opt => opt.MapFrom(src => BuildMetasDictionary(src.program.m_oProgram.EPG_Meta)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => BuildTagsDictionary(src.program.m_oProgram.EPG_TAGS)))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.program.m_oProgram.EPG_PICTURES))
                .ForMember(dest => dest.EpgChannelId, opt => opt.MapFrom(src => src.program.m_oProgram.EPG_CHANNEL_ID))
                .ForMember(dest => dest.EpgId, opt => opt.MapFrom(src => src.program.m_oProgram.EPG_IDENTIFIER))
                .ForMember(dest => dest.RelatedMediaId, opt => opt.MapFrom(src => src.program.m_oProgram.media_id))
                .ForMember(dest => dest.EnableCdvr, opt => opt.MapFrom(src => src.program.m_oProgram.ENABLE_CDVR == 1))
                .ForMember(dest => dest.EnableCatchUp, opt => opt.MapFrom(src => src.program.m_oProgram.ENABLE_CATCH_UP == 1))
                .ForMember(dest => dest.EnableStartOver, opt => opt.MapFrom(src => src.program.m_oProgram.ENABLE_START_OVER == 1))
                .ForMember(dest => dest.EnableTrickPlay, opt => opt.MapFrom(src => src.program.m_oProgram.ENABLE_TRICK_PLAY == 1))
                .ForMember(dest => dest.Crid, opt => opt.MapFrom(src => src.program.m_oProgram.CRID))
                .ForMember(dest => dest.LinearAssetId, opt => opt.MapFrom(src => src.program.m_oProgram.LINEAR_MEDIA_ID > 0 ? (long?)src.program.m_oProgram.LINEAR_MEDIA_ID : null))
                .ForMember(dest => dest.RecordingId, opt => opt.MapFrom(src => src.recordingId))
                ;

            //Media to KalturaMediaAsset
            Mapper.CreateMap<MediaObj, KalturaMediaAsset>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.m_sDescription))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dStartDate)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dEndDate)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.m_oMediaType.m_nTypeID))
                .ForMember(dest => dest.Metas, opt => opt.MapFrom(src => BuildMetasDictionary(src.m_lMetas)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => BuildTagsDictionary(src.m_lTags)))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.m_lPicture))
                .ForMember(dest => dest.MediaFiles, opt => opt.MapFrom(src => src.m_lFiles))
                .ForMember(dest => dest.ExternalIds, opt => opt.MapFrom(src => src.m_ExternalIDs))
                .ForMember(dest => dest.EnableCdvr, opt => opt.MapFrom(src => src.EnableCDVR))
                .ForMember(dest => dest.EnableCatchUp, opt => opt.MapFrom(src => src.EnableCatchUp))
                .ForMember(dest => dest.EnableStartOver, opt => opt.MapFrom(src => src.EnableStartOver))
                .ForMember(dest => dest.EnableTrickPlay, opt => opt.MapFrom(src => src.EnableTrickPlay))
                .ForMember(dest => dest.CatchUpBuffer, opt => opt.MapFrom(src => src.CatchUpBuffer))
                .ForMember(dest => dest.TrickPlayBuffer, opt => opt.MapFrom(src => src.TrickPlayBuffer))
                .ForMember(dest => dest.EnableRecordingPlaybackNonEntitledChannel, opt => opt.MapFrom(src => src.EnableRecordingPlaybackNonEntitledChannel))
                .ForMember(dest => dest.TypeDescription, opt => opt.MapFrom(src => src.m_oMediaType.m_sTypeName))
                .ForMember(dest => dest.DeviceRule, opt => opt.MapFrom(src => src.DeviceRule))
                .ForMember(dest => dest.GeoBlockRule, opt => opt.MapFrom(src => src.GeoblockRule))
                .ForMember(dest => dest.WatchPermissionRule, opt => opt.MapFrom(src => src.WatchPermissionRule))
                .ForMember(dest => dest.ExternalId, opt => opt.MapFrom(src => src.CoGuid))
                .ForMember(dest => dest.EntryIdentifier, opt => opt.MapFrom(src => src.EntryId));

            //EPG to AssetInfo
            Mapper.CreateMap<ProgramObj, KalturaAssetInfo>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_oProgram.NAME))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.m_oProgram.DESCRIPTION))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.m_oProgram.START_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.m_oProgram.END_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.Metas, opt => opt.MapFrom(src => BuildMetasDictionary(src.m_oProgram.EPG_Meta)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => BuildTagsDictionary(src.m_oProgram.EPG_TAGS)))
                .ForMember(dest => dest.ExtraParams, opt => opt.MapFrom(src => BuildExtraParamsDictionary(src.m_oProgram)))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.m_oProgram.EPG_PICTURES));

            //Comments to KalturaAssetComment
            Mapper.CreateMap<Comments, KalturaAssetComment>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.Id))
                .ForMember(dest => dest.AssetId, opt => opt.MapFrom(src => src.m_nAssetID))
                .ForMember(dest => dest.AssetType, opt => opt.MapFrom(src => ConvertToKalturaAssetType(src.AssetType)))
                .ForMember(dest => dest.CreateDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dCreateDate)))
                .ForMember(dest => dest.Header, opt => opt.MapFrom(src => src.m_sHeader))
                .ForMember(dest => dest.SubHeader, opt => opt.MapFrom(src => src.m_sSubHeader))
                .ForMember(dest => dest.Text, opt => opt.MapFrom(src => src.m_sContentText))
                .ForMember(dest => dest.Writer, opt => opt.MapFrom(src => src.m_sWriter));
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

        private static Dictionary<string, KalturaStringValueArray> BuildTagsDictionary(List<EPGDictionary> list)
        {
            if (list == null)
            {
                return null;
            }

            Dictionary<string, KalturaStringValueArray> tags = new Dictionary<string, KalturaStringValueArray>();
            KalturaStringValueArray tagsList;

            foreach (var tag in list)
            {
                if (tags.ContainsKey(tag.Key))
                {
                    tags[tag.Key].Objects.Add(new KalturaStringValue() { value = tag.Value });
                }
                else
                {
                    tagsList = new KalturaStringValueArray();
                    tagsList.Objects.Add(new KalturaStringValue() { value = tag.Value });
                    tags.Add(tag.Key, tagsList);
                }
            }
            return tags;
        }

        private static Dictionary<string, KalturaStringValue> BuildMetasDictionary(List<EPGDictionary> list)
        {
            if (list == null)
            {
                return null;
            }

            Dictionary<string, KalturaStringValue> metas = new Dictionary<string, KalturaStringValue>();

            foreach (var meta in list)
            {
                metas.Add(meta.Key, new KalturaStringValue() { value = meta.Value });
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

        private static SerializableDictionary<string, KalturaStringValueArray> BuildTagsDictionary(List<Tags> list)
        {
            if (list == null)
            {
                return null;
            }

            SerializableDictionary<string, KalturaStringValueArray> tags = new SerializableDictionary<string, KalturaStringValueArray>();

            foreach (var tag in list)
            {
                tags.Add(tag.m_oTagMeta.m_sName, new KalturaStringValueArray()
                {
                    Objects = tag.m_lValues.Select(v => new KalturaStringValue() { value = v }).ToList()
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
                if (meta.m_oTagMeta.m_sType == typeof(bool).ToString())
                {
                    value = new KalturaBooleanValue() { value = meta.m_sValue == "1" ? true : false };
                }
                else if (meta.m_oTagMeta.m_sType == typeof(string).ToString())
                {
                    value = new KalturaStringValue() { value = meta.m_sValue };
                }
                else if (meta.m_oTagMeta.m_sType == typeof(double).ToString())
                {
                    value = new KalturaDoubleValue() { value = double.Parse(meta.m_sValue) };
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
                case OrderBy.CREATE_DATE:
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
    }
}