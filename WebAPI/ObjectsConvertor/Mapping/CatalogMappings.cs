using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using WebAPI.Catalog;
using WebAPI.Models;
using WebAPI.Utils;
using WebAPI.Models.Catalog;
using WebAPI.Exceptions;
using WebAPI.Managers.Models;
using WebAPI.Models.General;
using WebAPI.ObjectsConvertor.Mapping.Utils;

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
                 .ForMember(dest => dest.Ratio, opt => opt.MapFrom(src => src.Ratio));

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
            Mapper.CreateMap<WebAPI.Catalog.Channel, WebAPI.Models.Catalog.KalturaChannel>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nChannelID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sName))
                .ForMember(dest => dest.AssetTypes, opt => opt.MapFrom(src => src.m_nMediaType))
                .ForMember(dest => dest.FilterExpression, opt => opt.MapFrom(src => src.filterQuery))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.m_sDescription))
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
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_sSiteGUIDField))
                .ForMember(dest => dest.Username, opt => opt.MapFrom(src => src.m_oBasicDataField.m_sUserNameField))
                .ForMember(dest => dest.FirstName, opt => opt.MapFrom(src => src.m_oBasicDataField.m_sFirstNameField))
                .ForMember(dest => dest.LastName, opt => opt.MapFrom(src => src.m_oBasicDataField.m_sLastNameField));
        
            //UnifiedSearchResult to KalturaSlimAsset
            Mapper.CreateMap<UnifiedSearchResult, WebAPI.Models.Catalog.KalturaSlimAsset>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => ConvertAssetType(src.AssetType)));

            // Country
            Mapper.CreateMap<Catalog.Country, WebAPI.Models.Users.KalturaCountry>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.m_nObjecrtIDField))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sCountryNameField))
                .ForMember(dest => dest.Code, opt => opt.MapFrom(src => src.m_sCountryCodeField));

            //EPG to KalturaAsset
            Mapper.CreateMap<EPGChannelProgrammeObject, KalturaAsset>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.EPG_ID))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.NAME))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.DESCRIPTION))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => 0))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.START_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(DateTime.ParseExact(src.END_DATE, "dd/MM/yyyy HH:mm:ss", null))))
                .ForMember(dest => dest.Metas, opt => opt.MapFrom(src => BuildMetasDictionary(src.EPG_Meta)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => BuildTagsDictionary(src.EPG_TAGS)))
                .ForMember(dest => dest.ExtraParams, opt => opt.MapFrom(src => BuildExtraParamsDictionary(src)))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.EPG_PICTURES));

            //Media to KalturaAsset
            Mapper.CreateMap<MediaObj, KalturaAsset>()
                .ForMember(dest => dest.Id, opt => opt.MapFrom(src => src.AssetId))
                .ForMember(dest => dest.Name, opt => opt.MapFrom(src => src.m_sName))
                .ForMember(dest => dest.Description, opt => opt.MapFrom(src => src.m_sDescription))
                .ForMember(dest => dest.StartDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dStartDate)))
                .ForMember(dest => dest.EndDate, opt => opt.MapFrom(src => SerializationUtils.ConvertToUnixTimestamp(src.m_dEndDate)))
                .ForMember(dest => dest.Type, opt => opt.MapFrom(src => src.m_oMediaType.m_nTypeID))
                .ForMember(dest => dest.Metas, opt => opt.MapFrom(src => BuildMetasDictionary(src.m_lMetas)))
                .ForMember(dest => dest.Tags, opt => opt.MapFrom(src => BuildTagsDictionary(src.m_lTags)))
                .ForMember(dest => dest.ExtraParams, opt => opt.MapFrom(src => BuildExtraParamsDictionary(src)))
                .ForMember(dest => dest.Images, opt => opt.MapFrom(src => src.m_lPicture))
                .ForMember(dest => dest.MediaFiles, opt => opt.MapFrom(src => src.m_lFiles));

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
        }

        //eAssetTypes to KalturaAssetType
        public static KalturaAssetType ConvertAssetType(eAssetTypes assetType)
        {
            KalturaAssetType result;
            switch (assetType)
            {
                case eAssetTypes.EPG:
                    result = KalturaAssetType.EPG;
                    break;
                case eAssetTypes.NPVR:
                    result = KalturaAssetType.RECORDING;
                    break;
                case eAssetTypes.MEDIA:
                    result = KalturaAssetType.MEDIA;
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
                    result = KalturaPositionOwner.HOUSEHOLD;
                    break;
                case eUserType.PERSONAL:
                    result = KalturaPositionOwner.USER;
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

            extraParams.Add("enable_cdvr", new KalturaStringValue() { value = epg.ENABLE_CDVR == 1 ? "True": "False" });
            extraParams.Add("enable_catch_up", new KalturaStringValue() { value = epg.ENABLE_CATCH_UP == 1 ? "True" : "False" });
            extraParams.Add("enable_start_over", new KalturaStringValue() { value = epg.ENABLE_START_OVER == 1 ? "True" : "False" });
            extraParams.Add("enable_trick_play", new KalturaStringValue() { value = epg.ENABLE_TRICK_PLAY == 1 ? "True" : "False" });

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
    }
}